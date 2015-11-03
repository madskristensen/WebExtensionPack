using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace WebExtensionPack
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [Guid(ProductId)]
    public sealed class VSPackage : Package
    {
        public const string ProductId = "92e3e73b-510f-45bb-8aee-c637e83778b3";
        public const string Version = "1.0";
        public const string Title = "Web Extension Pack";

        protected async override void Initialize()
        {
            await Dispatcher.CurrentDispatcher.BeginInvoke(new Action(async () =>
            {
                if (!HasAlreadyRun())
                {
                    await Install();
                }

            }), DispatcherPriority.SystemIdle, null);

            base.Initialize();
        }

        private bool HasAlreadyRun()
        {
            using (var key = UserRegistryRoot.CreateSubKey(Title))
            {
                if (key.GetValue("Version", string.Empty).ToString() == Version)
                    return true;

                key.SetValue("Version", Version);
            }

            return false;
        }

        private async System.Threading.Tasks.Task Install()
        {
            var repository = (IVsExtensionRepository)GetService(typeof(SVsExtensionRepository));
            var manager = (IVsExtensionManager)GetService(typeof(SVsExtensionManager));

            var installed = manager.GetInstalledExtensions();
            var productIds = GetProductIds();
            var missing = productIds.Where(id => !installed.Any(ins => ins.Header.Identifier == id));

            var progress = new InstallerProgress(missing.Count(), $"Downloading and installing {missing.Count()} extension(s)...");
            progress.Show();

            await System.Threading.Tasks.Task.Run(() =>
            {
                foreach (var id in missing)
                {
                    InstallExtension(repository, manager, id);
                }

            });

            if (progress.IsVisible)
            {
                progress.Close();
                progress = null;
                PromptForRestart();
            }
        }

        private IEnumerable<string> GetProductIds()
        {
            return new[] {
                "5fb7364d-2e8c-44a4-95eb-2a382e30fec9", // Web Essentials
                "148ffa77-d70a-407f-892b-9ee542346862", // Web Compiler
                "36bf2130-106e-40f2-89ff-a2bdac6be879", // Web Analyzer
                "bf95754f-93d3-42ff-bfe3-e05d23188b08", // Image optimizer
                "950d05f7-bb25-43ce-b682-44b377b5307d", // Glyphfriend
                "6ed6c371-5815-407f-9148-f64b3a025dd9", // Bootstrap Snippet Pack
                "f4ab1e64-5d35-4f06-bad9-bf414f4b3bbb", // Open Command Line
                "fdd64809-376e-4542-92ce-808a8df06bcc", // Package Installer
            };
        }

        private static void InstallExtension(IVsExtensionRepository repository, IVsExtensionManager manager, string id)
        {
            GalleryEntry entry = repository.CreateQuery<GalleryEntry>(includeTypeInQuery: false, includeSkuInQuery: true, searchSource: "ExtensionManagerUpdate")
                                                                             .Where(e => e.VsixID == id)
                                                                             .AsEnumerable()
                                                                             .FirstOrDefault();

            IInstallableExtension installable = repository.Download(entry);
            manager.Install(installable, false);
        }

        private void PromptForRestart()
        {
            string prompt = "You must restart Visual Studio for the extensions to be loaded.\r\rRestart now?";
            var result = MessageBox.Show(prompt, Title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
                IVsShell4 shell = (IVsShell4)GetService(typeof(SVsShell));
                shell.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);

            }
        }
    }
}
