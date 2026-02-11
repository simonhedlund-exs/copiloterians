using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Projektverktyg.Models;

namespace Projektverktyg.Services;

public class AzureDevOpsService
{
    private const string ApiVersion = "7.1";

    // ── Connection ──────────────────────────────────────────────

    public async Task<bool> TestConnectionAsync(string org, string project, string pat)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/{project}/_apis/wit/workitemtypes?api-version={ApiVersion}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> TestOrgConnectionAsync(string org, string pat)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/_apis/projects?api-version={ApiVersion}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return true;
    }

    // ── Create Project ──────────────────────────────────────────

    public async Task<string> CreateProjectAsync(string org, string pat, string projectName, string description, string processTemplate = "CMMI")
    {
        using var client = CreateClient(pat);

        // Get process template id
        var processId = await GetProcessTemplateIdAsync(client, org, processTemplate);

        var body = new
        {
            name = projectName,
            description,
            capabilities = new
            {
                versioncontrol = new { sourceControlType = "Git" },
                processTemplate = new { templateTypeId = processId }
            }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"https://dev.azure.com/{org}/_apis/projects?api-version={ApiVersion}";
        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        // This returns an operation – we need to poll for completion
        var operationId = doc.RootElement.GetProperty("id").GetString()!;
        await WaitForOperationAsync(client, org, operationId);

        // Get the project ID
        return await GetProjectIdAsync(client, org, projectName);
    }

    private async Task<string> GetProcessTemplateIdAsync(HttpClient client, string org, string processName)
    {
        var url = $"https://dev.azure.com/{org}/_apis/process/processes?api-version={ApiVersion}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        foreach (var process in doc.RootElement.GetProperty("value").EnumerateArray())
        {
            var name = process.GetProperty("name").GetString();
            if (string.Equals(name, processName, StringComparison.OrdinalIgnoreCase))
                return process.GetProperty("id").GetString()!;
        }

        // Fallback to first process
        return doc.RootElement.GetProperty("value")[0].GetProperty("id").GetString()!;
    }

    private async Task WaitForOperationAsync(HttpClient client, string org, string operationId, int maxWaitMs = 60000)
    {
        var url = $"https://dev.azure.com/{org}/_apis/operations/{operationId}?api-version={ApiVersion}";
        var elapsed = 0;
        while (elapsed < maxWaitMs)
        {
            await Task.Delay(2000);
            elapsed += 2000;
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var status = doc.RootElement.GetProperty("status").GetString();
            if (status == "succeeded") return;
            if (status == "failed" || status == "cancelled")
                throw new Exception($"Operation {status}: {json}");
        }
        throw new TimeoutException("Timed out waiting for project creation");
    }

    private async Task<string> GetProjectIdAsync(HttpClient client, string org, string projectName)
    {
        var url = $"https://dev.azure.com/{org}/_apis/projects/{projectName}?api-version={ApiVersion}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetString()!;
    }

    // ── Git Repo & Push Files ───────────────────────────────────

    public async Task<string> GetDefaultRepoIdAsync(string org, string project, string pat)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories?api-version={ApiVersion}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("value")[0].GetProperty("id").GetString()!;
    }

    public async Task<string> CreateRepoAsync(string org, string project, string pat, string repoName)
    {
        using var client = CreateClient(pat);

        // Get project ID first
        var projectId = await GetProjectIdAsync(client, org, project);

        var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories?api-version={ApiVersion}";
        var body = new
        {
            name = repoName,
            project = new { id = projectId }
        };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("id").GetString()!;
    }

    public async Task<string> GetRepoIdByNameAsync(string org, string project, string pat, string repoName)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories?api-version={ApiVersion}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        foreach (var repo in doc.RootElement.GetProperty("value").EnumerateArray())
        {
            if (string.Equals(repo.GetProperty("name").GetString(), repoName, StringComparison.OrdinalIgnoreCase))
                return repo.GetProperty("id").GetString()!;
        }
        throw new Exception($"Repo '{repoName}' hittades inte i projektet '{project}'");
    }

    public async Task PushFilesToRepoAsync(string org, string project, string pat, string repoId, Dictionary<string, byte[]> files)
    {
        using var client = CreateClient(pat);

        // Check if repo has any commits (empty repo needs special handling)
        string? oldObjectId = null;
        string refName = "refs/heads/main";

        var refsUrl = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repoId}/refs?api-version={ApiVersion}";
        var refsResponse = await client.GetAsync(refsUrl);
        if (refsResponse.IsSuccessStatusCode)
        {
            var refsJson = await refsResponse.Content.ReadAsStringAsync();
            using var refsDoc = JsonDocument.Parse(refsJson);
            var refs = refsDoc.RootElement.GetProperty("value");
            if (refs.GetArrayLength() > 0)
            {
                oldObjectId = refs[0].GetProperty("objectId").GetString();
                refName = refs[0].GetProperty("name").GetString()!;
            }
        }

        // Build the push
        var changes = new List<object>();
        foreach (var (path, content) in files)
        {
            changes.Add(new
            {
                changeType = "add",
                item = new { path = $"/{path}" },
                newContent = new
                {
                    content = Convert.ToBase64String(content),
                    contentType = "base64encoded"
                }
            });
        }

        var pushBody = new
        {
            refUpdates = new[]
            {
                new
                {
                    name = refName,
                    oldObjectId = oldObjectId ?? "0000000000000000000000000000000000000000"
                }
            },
            commits = new[]
            {
                new
                {
                    comment = "Projektfiler tillagda via Projektverktyg",
                    changes
                }
            }
        };

        var json = JsonSerializer.Serialize(pushBody);
        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repoId}/pushes?api-version={ApiVersion}";
        var response = await client.PostAsync(url, requestContent);
        response.EnsureSuccessStatusCode();
    }

    // ── Area Paths ──────────────────────────────────────────────

    public async Task CreateAreaPathAsync(string org, string project, string pat, string areaName)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/{project}/_apis/wit/classificationnodes/areas?api-version={ApiVersion}";
        var body = new { name = areaName };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task CreateChildAreaPathAsync(string org, string project, string pat, string parentPath, string childName)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/{project}/_apis/wit/classificationnodes/areas/{parentPath}?api-version={ApiVersion}";
        var body = new { name = childName };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    // ── Iteration Paths ─────────────────────────────────────────

    public async Task CreateIterationAsync(string org, string project, string pat, string iterationName, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/{project}/_apis/wit/classificationnodes/iterations?api-version={ApiVersion}";

        var body = new Dictionary<string, object> { ["name"] = iterationName };
        if (startDate.HasValue && endDate.HasValue)
        {
            body["attributes"] = new
            {
                startDate = startDate.Value.ToString("yyyy-MM-dd") + "T00:00:00Z",
                finishDate = endDate.Value.ToString("yyyy-MM-dd") + "T00:00:00Z"
            };
        }

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    // ── Teams ───────────────────────────────────────────────────

    public async Task<string> CreateTeamAsync(string org, string project, string pat, string teamName, string description = "")
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/_apis/projects/{project}/teams?api-version={ApiVersion}";
        var body = new { name = teamName, description };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("id").GetString()!;
    }

    public async Task SetTeamAreaPathAsync(string org, string project, string pat, string teamId, string areaPath)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/{project}/{teamId}/_apis/work/teamsettings/teamfieldvalues?api-version={ApiVersion}";
        var body = new
        {
            defaultValue = areaPath,
            values = new[] { new { value = areaPath, includeChildren = true } }
        };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = content
        };
        var response = await client.SendAsync(request);
        // May fail if area path doesn't exist yet, that's okay
    }

    public async Task SetTeamIterationsAsync(string org, string project, string pat, string teamId, string backlogIteration)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/{project}/{teamId}/_apis/work/teamsettings?api-version={ApiVersion}";
        var body = new { backlogIteration = new { id = backlogIteration } };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = content
        };
        var response = await client.SendAsync(request);
        // Best-effort
    }

    // ── Full Setup Orchestration ────────────────────────────────

    public async Task<SetupResult> FullProjectSetupAsync(
        ProjectData data,
        bool createProject,
        bool createRepo,
        bool uploadFiles,
        bool createAreas,
        bool createIterations,
        bool createTeam,
        bool createWorkItems,
        string? repoName,
        Dictionary<string, byte[]>? filesToUpload,
        Action<ProgressInfo> onProgress)
    {
        var org = data.DevOpsOrg;
        var pat = data.DevOpsPat;
        var projectName = data.DevOpsProject;
        var result = new SetupResult();

        int totalSteps = 0;
        if (createProject) totalSteps++;
        if (createRepo) totalSteps++;
        if (uploadFiles) totalSteps++;
        if (createAreas) totalSteps += 2; // customer + team area
        if (createIterations) totalSteps += data.SprintCount;
        if (createTeam) totalSteps++;
        if (createWorkItems) totalSteps++;
        int current = 0;

        try
        {
            // 1. Create project
            if (createProject)
            {
                current++;
                onProgress(new(totalSteps, current, $"Skapar projekt '{projectName}'..."));
                var projectId = await CreateProjectAsync(org, pat, projectName, data.ProjectDescription, data.ProcessTemplate);
                result.ProjectCreated = true;
                result.ProjectId = projectId;
                result.Log.Add($"Projekt '{projectName}' skapat (ID: {projectId})");
            }

            // 2. Area Paths
            if (createAreas)
            {
                current++;
                onProgress(new(totalSteps, current, $"Skapar Area Path: {data.CustomerName}..."));
                try
                {
                    await CreateAreaPathAsync(org, projectName, pat, data.CustomerName);
                    result.Log.Add($"Area Path '{data.CustomerName}' skapad");

                    current++;
                    onProgress(new(totalSteps, current, $"Skapar Area Path: {data.TeamName}..."));
                    await CreateChildAreaPathAsync(org, projectName, pat, data.CustomerName, data.TeamName);
                    result.Log.Add($"Area Path '{data.CustomerName}\\{data.TeamName}' skapad");
                    result.AreaPathsCreated = true;
                }
                catch (Exception ex)
                {
                    result.Log.Add($"Area Path-fel: {ex.Message}");
                }
            }

            // 3. Iterations / Sprints
            if (createIterations && data.SprintCount > 0)
            {
                var sprintStart = data.ProjectStart ?? DateOnly.FromDateTime(DateTime.Today);
                var sprintLength = data.SprintLengthWeeks;

                for (int i = 1; i <= data.SprintCount; i++)
                {
                    current++;
                    var sprintName = $"Sprint {i}";
                    var start = sprintStart.AddDays((i - 1) * sprintLength * 7);
                    var end = start.AddDays(sprintLength * 7 - 1);

                    onProgress(new(totalSteps, current, $"Skapar {sprintName}..."));
                    try
                    {
                        await CreateIterationAsync(org, projectName, pat, sprintName, start, end);
                        result.Log.Add($"{sprintName}: {start:yyyy-MM-dd} – {end:yyyy-MM-dd}");
                    }
                    catch (Exception ex)
                    {
                        result.Log.Add($"{sprintName} fel: {ex.Message}");
                    }
                }
                result.IterationsCreated = true;
            }

            // 4. Create Team
            if (createTeam)
            {
                current++;
                onProgress(new(totalSteps, current, $"Skapar team '{data.TeamName}'..."));
                try
                {
                    var teamId = await CreateTeamAsync(org, projectName, pat, data.TeamName, $"Team för {data.CustomerName}");
                    result.TeamCreated = true;
                    result.Log.Add($"Team '{data.TeamName}' skapat");

                    // Try to set area path for the team
                    if (createAreas)
                    {
                        await SetTeamAreaPathAsync(org, projectName, pat, teamId, $"{projectName}\\{data.CustomerName}\\{data.TeamName}");
                        result.Log.Add($"Team kopplat till Area Path");
                    }
                }
                catch (Exception ex)
                {
                    result.Log.Add($"Team-fel: {ex.Message}");
                }
            }

            // 5. Create or get repo & push files
            string? repoId = null;
            if (createRepo && !string.IsNullOrWhiteSpace(repoName))
            {
                current++;
                onProgress(new(totalSteps, current, $"Skapar repo '{repoName}'..."));
                try
                {
                    repoId = await CreateRepoAsync(org, projectName, pat, repoName);
                    result.RepoCreated = true;
                    result.RepoName = repoName;
                    result.Log.Add($"Repo '{repoName}' skapat (ID: {repoId})");
                }
                catch (Exception ex)
                {
                    result.Log.Add($"Repo-skapande misslyckades: {ex.Message}");
                }
            }
            else if (createProject)
            {
                // Project was just created – get the default repo
                try
                {
                    repoId = await GetDefaultRepoIdAsync(org, projectName, pat);
                    result.RepoCreated = true;
                    result.Log.Add($"Standard-repo identifierat (ID: {repoId})");
                }
                catch (Exception ex)
                {
                    result.Log.Add($"Kunde inte hämta standard-repo: {ex.Message}");
                }
            }
            else if (uploadFiles)
            {
                // Existing project, no new repo requested – find target repo
                try
                {
                    if (!string.IsNullOrWhiteSpace(repoName))
                        repoId = await GetRepoIdByNameAsync(org, projectName, pat, repoName);
                    else
                        repoId = await GetDefaultRepoIdAsync(org, projectName, pat);
                }
                catch (Exception ex)
                {
                    result.Log.Add($"Kunde inte hitta repo: {ex.Message}");
                }
            }

            if (uploadFiles && filesToUpload != null && filesToUpload.Count > 0 && repoId != null)
            {
                current++;
                onProgress(new(totalSteps, current, $"Laddar upp {filesToUpload.Count} filer till repo..."));
                try
                {
                    await PushFilesToRepoAsync(org, projectName, pat, repoId, filesToUpload);
                    result.FilesUploaded = true;
                    result.Log.Add($"{filesToUpload.Count} filer uppladdade till repo");
                }
                catch (Exception ex)
                {
                    result.Log.Add($"Filuppladdning misslyckades: {ex.Message}");
                }
            }

            // 6. Create work items
            if (createWorkItems)
            {
                current++;
                onProgress(new(totalSteps, current, "Skapar work items..."));
                var workItemResults = await PushToDevOpsAsync(data, p =>
                    onProgress(new(totalSteps, current, p.Message)));
                result.WorkItemsCreated = true;
                result.WorkItemCount = workItemResults.Count;
                result.Log.Add($"{workItemResults.Count} work items skapade");
            }

            onProgress(new(totalSteps, totalSteps, "Projektuppsättning klar!"));
        }
        catch (Exception ex)
        {
            result.Log.Add($"KRITISKT FEL: {ex.Message}");
            onProgress(new(totalSteps, current, $"Fel: {ex.Message}"));
        }

        return result;
    }

    // ── Work Items (existing) ───────────────────────────────────

    public async Task<List<PushResult>> PushToDevOpsAsync(
        ProjectData projectData,
        Action<ProgressInfo> onProgress)
    {
        var org = projectData.DevOpsOrg;
        var project = projectData.DevOpsProject;
        var pat = projectData.DevOpsPat;
        var areaPath = $"{project}\\{projectData.CustomerName}\\{projectData.TeamName}";

        int totalItems = 0;
        foreach (var epic in projectData.DevOpsStructure.Epics)
        {
            totalItems++;
            foreach (var feature in epic.Features)
            {
                totalItems++;
                totalItems += feature.Requirements.Count;
                totalItems += feature.Requirements.Sum(r => r.Tasks.Count);
            }
        }

        var results = new List<PushResult>();
        int current = 0;
        onProgress(new(totalItems, 0, "Startar..."));

        using var client = CreateClient(pat);

        foreach (var (epic, epicIdx) in projectData.DevOpsStructure.Epics.Select((e, i) => (e, i)))
        {
            var epicTitle = epicIdx == 0 ? projectData.MainTitle : epic.Title;
            current++;
            onProgress(new(totalItems, current, $"Skapar Epic: {epicTitle}"));

            var epicId = await CreateWorkItemAsync(client, org, project, "Epic", new Dictionary<string, object>
            {
                ["System.Title"] = epicTitle,
                ["System.AreaPath"] = areaPath,
                ["System.State"] = "Proposed"
            });
            results.Add(new("Epic", epicTitle, epicId));

            foreach (var feature in epic.Features)
            {
                current++;
                onProgress(new(totalItems, current, $"Skapar Feature: {feature.Title}"));

                var featureFields = new Dictionary<string, object>
                {
                    ["System.Title"] = feature.Title,
                    ["System.AreaPath"] = areaPath,
                    ["System.State"] = "Proposed"
                };
                if (feature.Effort > 0)
                    featureFields["Microsoft.VSTS.Scheduling.Effort"] = feature.Effort;

                var featureId = await CreateWorkItemAsync(client, org, project, "Feature", featureFields, epicId);
                results.Add(new("Feature", feature.Title, featureId));

                foreach (var req in feature.Requirements)
                {
                    current++;
                    onProgress(new(totalItems, current, $"Skapar Requirement: {req.Title}"));

                    var reqFields = new Dictionary<string, object>
                    {
                        ["System.Title"] = req.Title,
                        ["System.AreaPath"] = areaPath,
                        ["System.State"] = "Proposed"
                    };
                    if (req.Estimate > 0)
                    {
                        reqFields["Microsoft.VSTS.Scheduling.OriginalEstimate"] = req.Estimate;
                        reqFields["Microsoft.VSTS.Scheduling.RemainingWork"] = req.Estimate;
                    }

                    var reqId = await CreateWorkItemAsync(client, org, project, "Requirement", reqFields, featureId);
                    results.Add(new("Requirement", req.Title, reqId));

                    foreach (var task in req.Tasks)
                    {
                        current++;
                        onProgress(new(totalItems, current, $"Skapar Task: {task.Title}"));

                        var taskId = await CreateWorkItemAsync(client, org, project, "Task", new Dictionary<string, object>
                        {
                            ["System.Title"] = task.Title,
                            ["System.AreaPath"] = areaPath,
                            ["System.State"] = "Proposed"
                        }, reqId);
                        results.Add(new("Task", task.Title, taskId));
                    }
                }
            }
        }

        onProgress(new(totalItems, totalItems, $"Klart! {totalItems} work items skapade."));
        return results;
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static async Task<int> CreateWorkItemAsync(
        HttpClient client, string org, string project, string type,
        Dictionary<string, object> fields, int? parentId = null)
    {
        var url = $"https://dev.azure.com/{org}/{project}/_apis/wit/workitems/${type}?api-version={ApiVersion}";

        var patchDoc = fields.Select(kv => new
        {
            op = "add",
            path = $"/fields/{kv.Key}",
            value = kv.Value
        }).Cast<object>().ToList();

        if (parentId.HasValue)
        {
            patchDoc.Add(new
            {
                op = "add",
                path = "/relations/-",
                value = new
                {
                    rel = "System.LinkTypes.Hierarchy-Reverse",
                    url = $"https://dev.azure.com/{org}/{project}/_apis/wit/workItems/{parentId}",
                    attributes = new { comment = "Skapad via Projektverktyg" }
                }
            });
        }

        var json = JsonSerializer.Serialize(patchDoc);
        var content = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private static HttpClient CreateClient(string pat)
    {
        var client = new HttpClient();
        var authBytes = Encoding.ASCII.GetBytes($":{pat}");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
        return client;
    }
}

public record ProgressInfo(int Total, int Current, string Message);
public record PushResult(string Type, string Title, int Id);

public class SetupResult
{
    public bool ProjectCreated { get; set; }
    public string? ProjectId { get; set; }
    public bool RepoCreated { get; set; }
    public string? RepoName { get; set; }
    public bool FilesUploaded { get; set; }
    public bool AreaPathsCreated { get; set; }
    public bool IterationsCreated { get; set; }
    public bool TeamCreated { get; set; }
    public bool WorkItemsCreated { get; set; }
    public int WorkItemCount { get; set; }
    public List<string> Log { get; set; } = new();
}
