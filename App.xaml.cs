using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.ApplicationModel.Appointments;
using ZespolWpfGui.SettingsUtils;

namespace ZespolWpfGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly string settingsStoreFileName = "settings_store";
        private readonly string propertiesStoreFileName = "properties_store";
        public Settings Settings { get; set; }

        public App()
        {
            Resources["test"] = 12;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            ReadDictionary(propertiesStoreFileName, Properties);
            try
            {
                Settings = ReadObject<Settings>(settingsStoreFileName);
            }
            catch
            {
                Settings = new Settings();
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            SaveDictionary(propertiesStoreFileName, Properties);
            SaveObject(settingsStoreFileName, Settings);
        }

        private void ReadDictionary(string filename, IDictionary dict)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                string jsonString = ReadFile(filename, storage);
                Dictionary<string, object> dic =
                    JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                foreach (var pair in dic)
                {
                    dict[pair.Key] = pair.Value;
                }
            }
            catch
            {
                // todo
            }
        }

        private T ReadObject<T>(string filename)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            string jsonString = ReadFile(filename, storage);
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        private string ReadFile(string filename, IsolatedStorageFile storage)
        {
            using IsolatedStorageFileStream stream =
                new IsolatedStorageFileStream(filename, FileMode.Open, storage);
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private void SaveDictionary(string filename, IDictionary dict)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(filename, FileMode.Create, storage))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();

                foreach (string key in dict.Keys)
                {
                    dic.Add(key, dict[key]);
                }

                string jsonString = JsonSerializer.Serialize(dic);
                writer.Write(jsonString);
            }
        }

        private void SaveObject<T>(string filename, T obj)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(filename, FileMode.Create, storage))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string jsonString = JsonSerializer.Serialize(obj);
                writer.Write(jsonString);
            }
        }

    }
}
