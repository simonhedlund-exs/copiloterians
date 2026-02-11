using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Projektverktyg.Models;

namespace Projektverktyg.Services;

public class AzureDevOpsService
{
    private const string ApiVersion = "7.1";

    public async Task<bool> TestConnectionAsync(string org, string project, string pat)
    {
        using var client = CreateClient(pat);
        var url = $"https://dev.azure.com/{org}/{project}/_apis/wit/workitemtypes?api-version={ApiVersion}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<List<PushResult>> PushToDevOpsAsync(
        ProjectData projectData,
        Action<ProgressInfo> onProgress)
    {
        var org = projectData.DevOpsOrg;
        var project = projectData.DevOpsProject;
        var pat = projectData.DevOpsPat;
        var areaPath = projectData.AreaPath;

        // Count total items
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
