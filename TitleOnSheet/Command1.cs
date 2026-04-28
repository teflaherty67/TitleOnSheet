using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TitleOnSheet
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                // 1. Find string
                var findDlg = new InputDialog("Find and Replace: Title on Sheet", "Find what:");
                if (findDlg.ShowDialog() != true) return Result.Cancelled;
                string findText = findDlg.ResponseText;
                if (string.IsNullOrEmpty(findText))
                {
                    TaskDialog.Show("Find and Replace", "Find string cannot be empty.");
                    return Result.Cancelled;
                }

                // 2. Replace string (empty allowed for deletion)
                var replaceDlg = new InputDialog("Find and Replace: Title on Sheet", "Replace with:");
                if (replaceDlg.ShowDialog() != true) return Result.Cancelled;
                string replaceText = replaceDlg.ResponseText ?? string.Empty;

                // 3. Collect distinct views placed on any sheet in the project
                var sheets = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSheet))
                    .Cast<ViewSheet>()
                    .Where(s => !s.IsPlaceholder)
                    .ToList();

                var viewIds = new HashSet<ElementId>();
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

                // 4. Build hit list (case-insensitive substring match)
                string pattern = Regex.Escape(findText);
                var hits = new List<KeyValuePair<View, string>>();
                foreach (ElementId id in viewIds)
                {
                    if (!(doc.GetElement(id) is View v)) continue;

                    Parameter p = v.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION);
                    if (p == null || p.IsReadOnly || p.StorageType != StorageType.String) continue;

                    string current = p.AsString() ?? string.Empty;
                    if (current.IndexOf(findText, StringComparison.OrdinalIgnoreCase) < 0) continue;

                    string updated = Regex.Replace(current, pattern, replaceText, RegexOptions.IgnoreCase);
                    if (!string.Equals(updated, current, StringComparison.Ordinal))
                        hits.Add(new KeyValuePair<View, string>(v, updated));
                }

                if (hits.Count == 0)
                {
                    TaskDialog.Show("Find and Replace",
                        $"No views with a Title on Sheet containing \"{findText}\" were found.");
                    return Result.Succeeded;
                }

                // 5. Confirm
                var confirm = new TaskDialog("Find and Replace: Confirm")
                {
                    MainInstruction = $"Update Title on Sheet for {hits.Count} view(s)?",
                    MainContent = $"Find: \"{findText}\"\nReplace with: \"{replaceText}\"\n\nMatch is case insensitive.",
                    CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                    DefaultButton = TaskDialogResult.Yes
                };
                if (confirm.Show() != TaskDialogResult.Yes) return Result.Cancelled;

                // 6. Apply inside a single transaction
                int updatedCount = 0;
                using (Transaction t = new Transaction(doc, "Find/Replace Title on Sheet"))
                {
                    t.Start();
                    foreach (var kv in hits)
                    {
                        Parameter p = kv.Key.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION);
                        if (p != null && !p.IsReadOnly)
                        {
                            p.Set(kv.Value);
                            updatedCount++;
                        }
                    }
                    t.Commit();
                }

                TaskDialog.Show("Find and Replace", $"Updated {updatedCount} view(s).");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
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
                "Find and replace text in the Title on Sheet parameter");

            return myButtonData.Data;
        }
    }
}
