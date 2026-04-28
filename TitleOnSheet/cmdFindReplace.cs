using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TitleOnSheet
{
    [Transaction(TransactionMode.Manual)]
    public class cmdFindReplace : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                var dlg = new frmFindReplace();
                if (dlg.ShowDialog() != true) return Result.Cancelled;

                string findText = dlg.FindText;
                string replaceText = dlg.ReplaceText;

                var regexOptions = RegexOptions.None;
                if (!dlg.MatchCase) regexOptions |= RegexOptions.IgnoreCase;

                string pattern = dlg.MatchWholeWord
                    ? $@"\b{Regex.Escape(findText)}\b"
                    : Regex.Escape(findText);

                var hits = new List<(Element element, BuiltInParameter param, string newValue)>();

                // Collect views for View Name and/or Title on Sheet
                if (dlg.ReplaceViewName || dlg.ReplaceTitleOnSheet)
                {
                    var viewIds = GetViewIds(doc, uidoc, dlg.Scope);
                    foreach (ElementId id in viewIds)
                    {
                        if (!(doc.GetElement(id) is View v)) continue;

                        if (dlg.ReplaceViewName)
                            AddHit(v, BuiltInParameter.VIEW_NAME, pattern, replaceText, regexOptions, hits);

                        if (dlg.ReplaceTitleOnSheet)
                            AddHit(v, BuiltInParameter.VIEW_DESCRIPTION, pattern, replaceText, regexOptions, hits);
                    }
                }

                // Collect sheets for Sheet Name
                if (dlg.ReplaceSheetName)
                {
                    var sheets = GetSheets(doc, uidoc, dlg.Scope);
                    foreach (ViewSheet sheet in sheets)
                        AddHit(sheet, BuiltInParameter.SHEET_NAME, pattern, replaceText, regexOptions, hits);
                }

                if (hits.Count == 0)
                {
                    TaskDialog.Show("Find and Replace", $"No matches found for \"{findText}\".");
                    return Result.Succeeded;
                }

                // Confirm
                var confirm = new TaskDialog("Find and Replace: Confirm")
                {
                    MainInstruction = $"Update {hits.Count} parameter value(s)?",
                    MainContent = $"Find: \"{findText}\"\nReplace with: \"{replaceText}\"\n\nMatch is case {(dlg.MatchCase ? "sensitive" : "insensitive")}.",
                    CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                    DefaultButton = TaskDialogResult.Yes
                };
                if (confirm.Show() != TaskDialogResult.Yes) return Result.Cancelled;

                // Apply
                int updatedCount = 0;
                using (Transaction t = new Transaction(doc, "Find/Replace Parameters"))
                {
                    t.Start();
                    foreach (var (element, param, newValue) in hits)
                    {
                        Parameter p = element.get_Parameter(param);
                        if (p != null && !p.IsReadOnly)
                        {
                            p.Set(newValue);
                            updatedCount++;
                        }
                    }
                    t.Commit();
                }

                TaskDialog.Show("Find and Replace", $"Updated {updatedCount} parameter value(s).");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private static HashSet<ElementId> GetViewIds(Document doc, UIDocument uidoc, SearchScope scope)
        {
            var viewIds = new HashSet<ElementId>();
            var sheets = GetSheets(doc, uidoc, scope);

            foreach (ViewSheet sheet in sheets)
            {
                foreach (ElementId vpId in sheet.GetAllViewports())
                {
                    if (doc.GetElement(vpId) is Viewport vp)
                        viewIds.Add(vp.ViewId);
                }

                var schedules = new FilteredElementCollector(doc, sheet.Id)
                    .OfClass(typeof(ScheduleSheetInstance))
                    .Cast<ScheduleSheetInstance>();

                foreach (var si in schedules)
                {
                    if (!si.IsTitleblockRevisionSchedule)
                        viewIds.Add(si.ScheduleId);
                }
            }

            return viewIds;
        }

        private static IEnumerable<ViewSheet> GetSheets(Document doc, UIDocument uidoc, SearchScope scope)
        {
            switch (scope)
            {
                case SearchScope.CurrentView:
                    if (uidoc.ActiveView is ViewSheet activeSheet)
                        return new[] { activeSheet };
                    return Enumerable.Empty<ViewSheet>();

                case SearchScope.CurrentSelection:
                    return uidoc.Selection.GetElementIds()
                        .Select(id => doc.GetElement(id))
                        .OfType<ViewSheet>()
                        .Where(s => !s.IsPlaceholder);

                default: // EntireProject
                    return new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewSheet))
                        .Cast<ViewSheet>()
                        .Where(s => !s.IsPlaceholder);
            }
        }

        private static void AddHit(
            Element element,
            BuiltInParameter param,
            string pattern,
            string replaceText,
            RegexOptions regexOptions,
            List<(Element, BuiltInParameter, string)> hits)
        {
            Parameter p = element.get_Parameter(param);
            if (p == null || p.IsReadOnly || p.StorageType != StorageType.String) return;

            string current = p.AsString() ?? string.Empty;
            string updated = Regex.Replace(current, pattern, replaceText, regexOptions);
            if (!string.Equals(updated, current, StringComparison.Ordinal))
                hits.Add((element, param, updated));
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Find & Replace";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "Find and replace text in View Name, Title on Sheet, or Sheet Name");

            return myButtonData.Data;
        }
    }
}
