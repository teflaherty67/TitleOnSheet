using System.Windows;
using System.Windows.Input;

namespace TitleOnSheet
{
    public partial class frmFindReplace : Window
    {
        public string FindText { get; private set; }
        public string ReplaceText { get; private set; }
        public bool ReplaceViewName { get; private set; }
        public bool ReplaceTitleOnSheet { get; private set; }
        public bool ReplaceSheetName { get; private set; }

        public frmFindReplace()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
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
