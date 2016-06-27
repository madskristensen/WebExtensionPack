using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebExtensionPack
{
    public class DataStore
    {
        const string _fileName = "%userprofile%\\.webextensionpack";

        public DataStore()
        {
            Initialize();
        }

        public List<string> PreviouslyInstalledExtensions { get; private set; } = new List<string>();

        public bool HasBeenInstalled(string productId)
        {
            return PreviouslyInstalledExtensions.Contains(productId);
        }

        public void Save()
        {
            var path = Environment.ExpandEnvironmentVariables(_fileName);
            File.WriteAllLines(path, PreviouslyInstalledExtensions);
        }

        private void Initialize()
        {
            try
            {
                var path = Environment.ExpandEnvironmentVariables(_fileName);

                if (File.Exists(path))
                    PreviouslyInstalledExtensions = File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
