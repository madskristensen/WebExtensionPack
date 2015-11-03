using System;
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
            var productIds = ExtensionList.ProductIds();
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

        private static void InstallExtension(IVsExtensionRepository repository, IVsExtensionManager manager, string id)
        {
            try
            {
                GalleryEntry entry = repository.CreateQuery<GalleryEntry>(includeTypeInQuery: false, includeSkuInQuery: true, searchSource: "ExtensionManagerUpdate")
                                                                                 .Where(e => e.VsixID == id)
                                                                                 .AsEnumerable()
                                                                                 .FirstOrDefault();

                IInstallableExtension installable = repository.Download(entry);
                manager.Install(installable, false);

                Telemetry.TrackEvent(installable.Header.Name);
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
