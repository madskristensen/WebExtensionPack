using System;
using System.Windows;
using System.Windows.Threading;

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
                bar.Value = 0;
                lblText.Content = message;
            };

            InitializeComponent();
        }

        public void SetMessage(string message)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                lblText.Content = message;
                bar.Value += 1;

            }), DispatcherPriority.Normal, null);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
