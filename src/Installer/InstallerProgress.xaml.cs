using System.Windows;

namespace WebExtensionPack
{
    public partial class InstallerProgress : Window
    {
        public InstallerProgress(int total, string message)
        {
            Loaded += delegate
            {
                Title = VSPackage.Title;
                bar.Maximum = total;
                lblText.Content = message;
            };

            InitializeComponent();
        }

        public void SetMessage(string message)
        {
            lblText.Content = message;
            //bar.Value += 1;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
