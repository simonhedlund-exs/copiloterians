using ClosedXML.Excel;
using Projektverktyg.Models;

namespace Projektverktyg.Services;

public class ExcelGeneratorService
{
    /// <summary>
    /// Generate the Azure DevOps import structure Excel file
    /// </summary>
    public byte[] GenerateDevOpsImport(ProjectData project)
    {
        using var wb = new XLWorkbook();
        var areaPath = project.AreaPath;

        // Main sheet
        var ws = wb.AddWorksheet("AzureDevopsStuktur");
        var headers = new[] { "ID", "Work Item Type", "Order", "Area Path", "Title 1", "Title 2",
            "Title 3", "Title 4", "Title 5", "Description", "State", "Effort", "Original Estimate", "Remaining" };

        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        StyleHeaderRow(ws, 1, headers.Length);

        int row = 2;
        int order = 2829;

        foreach (var (epic, epicIdx) in project.DevOpsStructure.Epics.Select((e, i) => (e, i)))
        {
            // Epic row
            ws.Cell(row, 2).Value = "Epic";
            if (epicIdx == 0) ws.Cell(row, 3).Value = order;
            ws.Cell(row, 4).Value = areaPath;
            if (epicIdx == 0)
                ws.Cell(row, 5).Value = project.MainTitle;
            else if (epic.Title == "Framtida utveckling")
                ws.Cell(row, 5).Value = "Framtida utveckling";
            ws.Cell(row, 6).Value = epic.Title == "Framtida utveckling" ? "" : epic.Title;
            ws.Cell(row, 11).Value = "Proposed";
            StyleWorkItemRow(ws, row, "Epic");
            row++;

            foreach (var feature in epic.Features)
            {
                ws.Cell(row, 2).Value = "Feature";
                ws.Cell(row, 4).Value = areaPath;
                ws.Cell(row, 7).Value = feature.Title;
                ws.Cell(row, 11).Value = "Proposed";
                if (feature.Effort > 0) ws.Cell(row, 12).Value = feature.Effort;
                StyleWorkItemRow(ws, row, "Feature");
                row++;

                foreach (var req in feature.Requirements)
                {
                    ws.Cell(row, 2).Value = "Requirement";
                    ws.Cell(row, 4).Value = areaPath;
                    ws.Cell(row, 8).Value = req.Title;
                    ws.Cell(row, 11).Value = "Proposed";
                    if (req.Estimate > 0)
                    {
                        ws.Cell(row, 13).Value = req.Estimate;
                        ws.Cell(row, 14).Value = req.Estimate;
                    }
                    StyleWorkItemRow(ws, row, "Requirement");
                    row++;

                    foreach (var task in req.Tasks)
                    {
                        ws.Cell(row, 2).Value = "Task";
                        ws.Cell(row, 4).Value = areaPath;
                        ws.Cell(row, 9).Value = task.Title;
                        ws.Cell(row, 11).Value = "Proposed";
                        StyleWorkItemRow(ws, row, "Task");
                        row++;
                    }
                }
            }
        }

        ws.Columns().AdjustToContents();

        // Variables sheet
        var wsVar = wb.AddWorksheet("Variables");
        wsVar.Cell(1, 1).Value = "Customername";
        wsVar.Cell(1, 2).Value = project.CustomerName;
        wsVar.Cell(1, 3).Value = "\\";
        wsVar.Cell(1, 4).Value = $"{project.CustomerName}\\";
        wsVar.Cell(2, 1).Value = "Team name";
        wsVar.Cell(2, 2).Value = project.TeamName;
        wsVar.Cell(3, 1).Value = "Area Path";
        wsVar.Cell(3, 2).Value = areaPath;
        wsVar.Columns().AdjustToContents();

        return ToBytes(wb);
    }

    /// <summary>
    /// Generate the project checklist Excel file
    /// </summary>
    public byte[] GenerateChecklist(ProjectData project)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Checklista");

        ws.Cell(1, 1).Value = "Checklista";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;

        var headers = new[] { "Titel", "Beskrivning", "Fas", "Status", "Kommentar" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(2, i + 1).Value = headers[i];
        StyleHeaderRow(ws, 2, headers.Length);

        var checklist = DefaultData.GetDefaultChecklist();
        for (int i = 0; i < checklist.Count; i++)
        {
            var item = checklist[i];
            ws.Cell(i + 3, 1).Value = item.Title;
            ws.Cell(i + 3, 1).Style.Font.Bold = true;
            ws.Cell(i + 3, 2).Value = item.Description;
            ws.Cell(i + 3, 2).Style.Alignment.WrapText = true;
            ws.Cell(i + 3, 3).Value = item.Phase;
            ws.Cell(i + 3, 4).Value = item.Status;
        }

        ws.Column(1).Width = 35;
        ws.Column(2).Width = 80;
        ws.Column(3).Width = 14;
        ws.Column(4).Width = 12;
        ws.Column(5).Width = 30;

        return ToBytes(wb);
    }

    /// <summary>
    /// Generate the full project tool Excel file
    /// </summary>
    public byte[] GenerateProjektverktyg(ProjectData project)
    {
        using var wb = new XLWorkbook();

        // 1. Aktivitetslista
        var wsAkt = wb.AddWorksheet("Aktivitetslista");
        wsAkt.Cell(1, 1).Value = "Aktivitetslista";
        wsAkt.Cell(1, 1).Style.Font.Bold = true;
        wsAkt.Cell(1, 1).Style.Font.FontSize = 16;
        var aktHeaders = new[] { "Nr", "Ärende", "Status", "Prioritet", "Datum", "Ansvarig kund",
            "Ansvarig Exsitec", "Estimerad tid (h)", "Upparbetat (h)", "OK datum kund", "Deadline", "Kommentar", "Kommentar kund" };
        for (int i = 0; i < aktHeaders.Length; i++)
            wsAkt.Cell(2, i + 1).Value = aktHeaders[i];
        StyleHeaderRow(wsAkt, 2, aktHeaders.Length);
        wsAkt.Columns().AdjustToContents();

        // 2. Intressenter
        var wsInt = wb.AddWorksheet("Intressenter");
        wsInt.Cell(1, 1).Value = "Kontaktinformation";
        wsInt.Cell(1, 1).Style.Font.Bold = true;
        wsInt.Cell(1, 1).Style.Font.FontSize = 16;

        int intRow = 3;
        wsInt.Cell(intRow, 1).Value = "EXSITEC";
        wsInt.Cell(intRow, 1).Style.Font.Bold = true;
        wsInt.Cell(intRow, 1).Style.Font.FontSize = 14;
        intRow++;

        var intHeaders = new[] { "Person", "Roll i projektet", "PG/SG", "Projekttid (h/v)", "E-post", "Telefon", "Kommentar" };
        for (int i = 0; i < intHeaders.Length; i++)
            wsInt.Cell(intRow, i + 1).Value = intHeaders[i];
        StyleHeaderRow(wsInt, intRow, intHeaders.Length);
        intRow++;

        foreach (var m in project.ExsitecMembers)
        {
            wsInt.Cell(intRow, 1).Value = m.Name;
            wsInt.Cell(intRow, 2).Value = m.Role;
            wsInt.Cell(intRow, 3).Value = m.Group;
            wsInt.Cell(intRow, 5).Value = m.Email;
            wsInt.Cell(intRow, 6).Value = m.Phone;
            intRow++;
        }

        intRow++;
        wsInt.Cell(intRow, 1).Value = "KUND";
        wsInt.Cell(intRow, 1).Style.Font.Bold = true;
        wsInt.Cell(intRow, 1).Style.Font.FontSize = 14;
        intRow++;
        for (int i = 0; i < intHeaders.Length; i++)
            wsInt.Cell(intRow, i + 1).Value = intHeaders[i];
        StyleHeaderRow(wsInt, intRow, intHeaders.Length);
        intRow++;

        foreach (var m in project.CustomerMembers)
        {
            wsInt.Cell(intRow, 1).Value = m.Name;
            wsInt.Cell(intRow, 2).Value = m.Role;
            wsInt.Cell(intRow, 3).Value = m.Group;
            wsInt.Cell(intRow, 5).Value = m.Email;
            wsInt.Cell(intRow, 6).Value = m.Phone;
            intRow++;
        }

        if (project.OtherContacts.Count > 0)
        {
            intRow++;
            wsInt.Cell(intRow, 1).Value = "ÖVRIGA KONTAKTER";
            wsInt.Cell(intRow, 1).Style.Font.Bold = true;
            intRow++;
            foreach (var m in project.OtherContacts)
            {
                wsInt.Cell(intRow, 1).Value = m.Name;
                wsInt.Cell(intRow, 2).Value = m.Role;
                wsInt.Cell(intRow, 5).Value = m.Email;
                wsInt.Cell(intRow, 6).Value = m.Phone;
                wsInt.Cell(intRow, 7).Value = m.Comment;
                intRow++;
            }
        }
        wsInt.Columns().AdjustToContents();

        // 3. Kalenderplan
        var wsCal = wb.AddWorksheet("Kalenderplan");
        wsCal.Cell(1, 1).Value = "Uppdatera denna flik med aktuella aktiviteter och datum för att få en tidslinje.";
        var calHeaders = new[] { "Delprojekt", "Aktivitet", "Startdatum", "Slutdatum" };
        for (int i = 0; i < calHeaders.Length; i++)
            wsCal.Cell(2, i + 1).Value = calHeaders[i];
        StyleHeaderRow(wsCal, 2, calHeaders.Length);

        int calRow = 3;
        if (project.ProjectStart.HasValue)
        {
            wsCal.Cell(calRow, 1).Value = "Övergripande";
            wsCal.Cell(calRow, 2).Value = "Projektstart";
            wsCal.Cell(calRow, 3).Value = project.ProjectStart.Value.ToDateTime(TimeOnly.MinValue);
            wsCal.Cell(calRow, 3).Style.DateFormat.Format = "yyyy-MM-dd";
            wsCal.Cell(calRow, 4).Value = project.ProjectStart.Value.ToDateTime(TimeOnly.MinValue);
            wsCal.Cell(calRow, 4).Style.DateFormat.Format = "yyyy-MM-dd";
            calRow++;
        }
        if (project.GoLive.HasValue)
        {
            wsCal.Cell(calRow, 1).Value = "Övergripande";
            wsCal.Cell(calRow, 2).Value = "GO-LIVE";
            wsCal.Cell(calRow, 3).Value = project.GoLive.Value.ToDateTime(TimeOnly.MinValue);
            wsCal.Cell(calRow, 3).Style.DateFormat.Format = "yyyy-MM-dd";
            wsCal.Cell(calRow, 4).Value = project.GoLive.Value.ToDateTime(TimeOnly.MinValue);
            wsCal.Cell(calRow, 4).Style.DateFormat.Format = "yyyy-MM-dd";
            calRow++;
        }
        foreach (var ms in project.Milestones)
        {
            wsCal.Cell(calRow, 1).Value = string.IsNullOrWhiteSpace(ms.Subproject) ? "Övergripande" : ms.Subproject;
            wsCal.Cell(calRow, 2).Value = ms.Activity;
            if (ms.StartDate.HasValue)
            {
                wsCal.Cell(calRow, 3).Value = ms.StartDate.Value.ToDateTime(TimeOnly.MinValue);
                wsCal.Cell(calRow, 3).Style.DateFormat.Format = "yyyy-MM-dd";
            }
            if (ms.EndDate.HasValue)
            {
                wsCal.Cell(calRow, 4).Value = ms.EndDate.Value.ToDateTime(TimeOnly.MinValue);
                wsCal.Cell(calRow, 4).Style.DateFormat.Format = "yyyy-MM-dd";
            }
            calRow++;
        }
        wsCal.Columns().AdjustToContents();

        // 4. Tidsuppföljning
        var wsTime = wb.AddWorksheet("Tidsuppföljning");
        wsTime.Cell(1, 1).Value = "Tiduppföljning";
        wsTime.Cell(1, 1).Style.Font.Bold = true;
        wsTime.Cell(1, 1).Style.Font.FontSize = 16;
        var timeHeaders = new[] { "Datum veckovis", "Planerad takt [h/v]", "Summerad takt [h]", "Nedlagt [h]",
            "Prognos [h]", "Återstående [h]", "Budget [h]", "Utfall", "Kommentar" };
        for (int i = 0; i < timeHeaders.Length; i++)
            wsTime.Cell(3, i + 1).Value = timeHeaders[i];
        StyleHeaderRow(wsTime, 3, timeHeaders.Length);

        if (project.ProjectStart.HasValue && project.BudgetHours > 0 && project.WeeklyPace > 0)
        {
            var start = project.ProjectStart.Value;
            int cumulative = 0;
            int totalWeeks = (int)Math.Ceiling((double)project.BudgetHours / project.WeeklyPace);

            for (int w = 0; w < totalWeeks; w++)
            {
                var weekDate = start.AddDays(w * 7);
                cumulative += project.WeeklyPace;
                if (cumulative > project.BudgetHours) cumulative = project.BudgetHours;

                int timeRow = 4 + w;
                wsTime.Cell(timeRow, 1).Value = weekDate.ToDateTime(TimeOnly.MinValue);
                wsTime.Cell(timeRow, 1).Style.DateFormat.Format = "yyyy-MM-dd";
                wsTime.Cell(timeRow, 2).Value = project.WeeklyPace;
                wsTime.Cell(timeRow, 3).Value = cumulative;
                wsTime.Cell(timeRow, 4).Value = 0;
                wsTime.Cell(timeRow, 5).Value = project.BudgetHours;
                wsTime.Cell(timeRow, 6).Value = project.BudgetHours;
                wsTime.Cell(timeRow, 7).Value = project.BudgetHours;
                wsTime.Cell(timeRow, 8).Value = 1;
            }
        }
        wsTime.Columns().AdjustToContents();

        // 5. Budget och ÄTA-hantering
        var wsBudget = wb.AddWorksheet("Budget och ÄTA-hantering");
        wsBudget.Cell(1, 1).Value = "Ändringslista";
        wsBudget.Cell(1, 1).Style.Font.Bold = true;
        var budgetHeaders = new[] { "", "Beställare", "Datum för ändring", "Förändrad tid +/- h",
            "Påverkar Go-live", "Godkänd", "Kommentar" };
        for (int i = 0; i < budgetHeaders.Length; i++)
            wsBudget.Cell(2, i + 1).Value = budgetHeaders[i];
        StyleHeaderRow(wsBudget, 2, budgetHeaders.Length);
        wsBudget.Columns().AdjustToContents();

        // 6. Riskanalys
        var wsRisk = wb.AddWorksheet("Riskanalys");
        var riskHeaders = new[] { "Risk-ID", "Rubrik / Kortnamn", "Beskrivning", "Orsak", "Konsekvens", "Kategori",
            "Sannolikhet (1–5)", "Påverkan (1–5)", "Riskvärde", "Prioritet", "Riskägare", "Åtgärdsstrategi",
            "Förebyggande åtgärder", "Fallback/Contingency", "Trigger/Indikator" };
        for (int i = 0; i < riskHeaders.Length; i++)
            wsRisk.Cell(2, i + 1).Value = riskHeaders[i];
        StyleHeaderRow(wsRisk, 2, riskHeaders.Length);

        int riskRow = 3;
        int riskId = 1;
        foreach (var risk in project.Risks.Where(r => r.Selected))
        {
            wsRisk.Cell(riskRow, 1).Value = riskId++;
            wsRisk.Cell(riskRow, 2).Value = risk.Title;
            wsRisk.Cell(riskRow, 3).Value = risk.Description;
            wsRisk.Cell(riskRow, 3).Style.Alignment.WrapText = true;
            wsRisk.Cell(riskRow, 4).Value = risk.Cause;
            wsRisk.Cell(riskRow, 4).Style.Alignment.WrapText = true;
            wsRisk.Cell(riskRow, 5).Value = risk.Consequence;
            wsRisk.Cell(riskRow, 5).Style.Alignment.WrapText = true;
            wsRisk.Cell(riskRow, 6).Value = risk.Category;
            wsRisk.Cell(riskRow, 7).Value = risk.Probability;
            wsRisk.Cell(riskRow, 8).Value = risk.Impact;
            wsRisk.Cell(riskRow, 9).Value = risk.RiskValue;

            // Color code risk value
            var riskColor = risk.RiskValue >= 10 ? XLColor.FromHtml("#fde7e9") :
                           risk.RiskValue >= 6 ? XLColor.FromHtml("#fff4ce") :
                           XLColor.FromHtml("#dff6dd");
            wsRisk.Cell(riskRow, 9).Style.Fill.BackgroundColor = riskColor;

            wsRisk.Cell(riskRow, 10).Value = risk.Priority;
            wsRisk.Cell(riskRow, 11).Value = risk.Owner;
            wsRisk.Cell(riskRow, 12).Value = risk.Strategy;
            wsRisk.Cell(riskRow, 13).Value = risk.Preventive;
            wsRisk.Cell(riskRow, 13).Style.Alignment.WrapText = true;
            wsRisk.Cell(riskRow, 14).Value = risk.Fallback;
            wsRisk.Cell(riskRow, 14).Style.Alignment.WrapText = true;
            wsRisk.Cell(riskRow, 15).Value = risk.Trigger;
            wsRisk.Cell(riskRow, 15).Style.Alignment.WrapText = true;
            riskRow++;
        }
        wsRisk.Columns().AdjustToContents();

        // 7. Länkar
        var wsLinks = wb.AddWorksheet("Länkar");
        wsLinks.Cell(1, 1).Value = "Länkar";
        wsLinks.Cell(1, 1).Style.Font.Bold = true;
        wsLinks.Cell(2, 1).Value = "Azure Devops";
        wsLinks.Cell(2, 2).Value = $"https://dev.azure.com/exsitec2/{Uri.EscapeDataString(project.CustomerName)}";
        wsLinks.Cell(3, 1).Value = "Projektmapp Google drive";
        wsLinks.Columns().AdjustToContents();

        return ToBytes(wb);
    }

    private static void StyleHeaderRow(IXLWorksheet ws, int row, int cols)
    {
        for (int i = 1; i <= cols; i++)
        {
            ws.Cell(row, i).Style.Font.Bold = true;
            ws.Cell(row, i).Style.Fill.BackgroundColor = XLColor.FromHtml("#0078d4");
            ws.Cell(row, i).Style.Font.FontColor = XLColor.White;
        }
    }

    private static void StyleWorkItemRow(IXLWorksheet ws, int row, string type)
    {
        var color = type switch
        {
            "Epic" => XLColor.FromHtml("#e8d4f8"),
            "Feature" => XLColor.FromHtml("#d4e8fc"),
            "Requirement" => XLColor.FromHtml("#d4f8e8"),
            "Task" => XLColor.FromHtml("#fff4ce"),
            _ => XLColor.NoColor
        };
        ws.Cell(row, 2).Style.Fill.BackgroundColor = color;
    }

    private static byte[] ToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
