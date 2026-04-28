using System.Windows;

namespace TitleOnSheet
{
    internal class InputDialog : Window
    {
        private readonly System.Windows.Controls.TextBox _textBox;

        public string ResponseText => _textBox.Text;

        public InputDialog(string title, string prompt)
        {
            Title = title;
            Width = 400;
            Height = 155;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var stack = new System.Windows.Controls.StackPanel { Margin = new Thickness(12) };

            stack.Children.Add(new System.Windows.Controls.Label { Content = prompt, Padding = new Thickness(0) });

            _textBox = new System.Windows.Controls.TextBox { Margin = new Thickness(0, 4, 0, 10) };
            stack.Children.Add(_textBox);

            var btnRow = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var ok = new System.Windows.Controls.Button { Content = "OK", Width = 75, IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            ok.Click += (s, e) => { DialogResult = true; };

            var cancel = new System.Windows.Controls.Button { Content = "Cancel", Width = 75, IsCancel = true };

            btnRow.Children.Add(ok);
            btnRow.Children.Add(cancel);
            stack.Children.Add(btnRow);

            Content = stack;
        }
    }
}
