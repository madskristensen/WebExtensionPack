using System;
using System.Windows;
using System.Windows.Threading;
using WebExtensionPack.Controls;

namespace WebExtensionPack
{
    public partial class InstallerProgress : Window
    {
        public InstallerProgress(int total, string message)
        {
            Loaded += delegate
            {
                Title = Vsix.Name;
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

        public void AddExtension(string guid, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Height += 40;
                this.Extensions.Children.Add(new ExtensionItem(guid, name) { Margin = new Thickness(0, 5, 0, 5) });
            });
        }

        public void StartDownloading(string key)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (var child in Extensions.Children)
                {
                    var extension = (ExtensionItem)child;
                    if (extension.ExtensionGuid == key)
                    {
                        extension.StartDownloading();
                        break;
                    }
                }
            });
        }

        public void InstallComplete(string key)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (var child in Extensions.Children)
                {
                    var extension = (ExtensionItem)child;
                    if (extension.ExtensionGuid == key)
                    {
                        extension.SetAsComplete();
                        break;
                    }
                }
            });
        }
    }
}
