using System.Windows;
using System.Windows.Input;

namespace TitleOnSheet
{
    public enum SearchScope { CurrentSelection, CurrentView, EntireProject }

    public partial class frmFindReplace : Window
    {
        public string FindText { get; private set; }
        public string ReplaceText { get; private set; }
        public bool ReplaceViewName { get; private set; }
        public bool ReplaceTitleOnSheet { get; private set; }
        public bool ReplaceSheetName { get; private set; }
        public SearchScope Scope { get; private set; }
        public bool MatchCase { get; private set; }
        public bool MatchWholeWord { get; private set; }

        public frmFindReplace()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            cbxViewName.IsChecked = true;
            cbxTitleOnSheet.IsChecked = true;
            cbxSheetName.IsChecked = true;
        }

        private void CheckNone_Click(object sender, RoutedEventArgs e)
        {
            cbxViewName.IsChecked = false;
            cbxTitleOnSheet.IsChecked = false;
            cbxSheetName.IsChecked = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (cbxViewName.IsChecked != true && cbxTitleOnSheet.IsChecked != true && cbxSheetName.IsChecked != true)
            {
                System.Windows.MessageBox.Show(
                    "Please select at least one parameter to search.",
                    "Find and Replace",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(tbxFind.Text))
            {
                System.Windows.MessageBox.Show(
                    "Find string cannot be empty.",
                    "Find and Replace",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            FindText = tbxFind.Text;
            ReplaceText = tbxReplace.Text ?? string.Empty;
            ReplaceViewName = cbxViewName.IsChecked == true;
            ReplaceTitleOnSheet = cbxTitleOnSheet.IsChecked == true;
            ReplaceSheetName = cbxSheetName.IsChecked == true;
            MatchCase = cbxMatchCase.IsChecked == true;
            MatchWholeWord = cbxMatchWholeWord.IsChecked == true;

            if (rbCurrentSelection.IsChecked == true)
                Scope = SearchScope.CurrentSelection;
            else if (rbCurrentView.IsChecked == true)
                Scope = SearchScope.CurrentView;
            else
                Scope = SearchScope.EntireProject;

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            // TODO: add help URL
        }
    }
}
