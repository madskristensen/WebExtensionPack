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
            Logger.Initialize(this, Title);
            Telemetry.Initialize(this, Version, "fbfac2d0-cd41-4458-9106-488be47240c2");

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
#if DEBUG
            return false;
#else
            using (var key = UserRegistryRoot.CreateSubKey(Title))
            {
                if (key.GetValue("Version", string.Empty).ToString() == Version)
                    return true;

                key.SetValue("Version", Version);
            }

            return false;
#endif
        }

        private async System.Threading.Tasks.Task Install()
        {
            var repository = (IVsExtensionRepository)GetService(typeof(SVsExtensionRepository));
            var manager = (IVsExtensionManager)GetService(typeof(SVsExtensionManager));

            var installed = manager.GetInstalledExtensions();
            var products = ExtensionList.Products();
            var missing = products.Where(product => !installed.Any(ins => ins.Header.Identifier == product.Key));

            if (!missing.Any())
                return;

            var progress = new InstallerProgress(missing.Count() + 1, $"Downloading extensions...");
            progress.Show();

            await System.Threading.Tasks.Task.Run(() =>
            {
                foreach (var product in missing)
                {
                    if (!progress.IsVisible)
                        break; // User cancelled the dialog

                    progress.SetMessage($"Installing {product.Value}...");
                    InstallExtension(repository, manager, product);
                }
            });

            if (progress.IsVisible)
            {
                progress.Close();
                progress = null;
                PromptForRestart();
            }
        }

        private void InstallExtension(IVsExtensionRepository repository, IVsExtensionManager manager, KeyValuePair<string, string> product)
        {
            try
            {
                GalleryEntry entry = repository.CreateQuery<GalleryEntry>(includeTypeInQuery: false, includeSkuInQuery: true, searchSource: "ExtensionManagerUpdate")
                                                                                 .Where(e => e.VsixID == product.Key)
                                                                                 .AsEnumerable()
                                                                                 .FirstOrDefault();

                if (entry != null)
                {
                    IInstallableExtension installable = repository.Download(entry);
                    manager.Install(installable, false);

                    Telemetry.TrackEvent(installable.Header.Name);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
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
