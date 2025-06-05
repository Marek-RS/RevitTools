using System;
using System.Windows;
using System.Windows.Controls;

namespace TTTRevitTools.ProgressDisplay
{
    /// <summary>
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : Window
    {
        public bool UserCancelled { get; set; } = false;
        public ProcessWindow()
        {
            SetOwner();
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch handleSearch = WindowHandleSearch.MainWindowHandle;
            handleSearch.SetAsOwner(this);
        }

        public void UpdateControls(string messageLine)
        {
            TextBoxInfo.Text += messageLine + Environment.NewLine;
            ProgressBar.Value++;
        }

        private void Close_Btn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBoxInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxInfo.ScrollToEnd();
        }

        private void Abort_Btn_Click(object sender, RoutedEventArgs e)
        {
            UserCancelled = true;
            TextBoxInfo.Text += "Operation cancelled by user! Close window";
            BtnClose.IsEnabled = true;
            BtnCancel.IsEnabled = false;
        }
    }
}