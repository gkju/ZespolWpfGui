using System;
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

namespace ZespolWpfGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Properties["test"] = 12;
        }

        private readonly string propertiesStoreFileName = "properties_store";

        private void App_Startup(object sender, StartupEventArgs e)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                using IsolatedStorageFileStream stream =
                    new IsolatedStorageFileStream(propertiesStoreFileName, FileMode.Open, storage);
                using StreamReader reader = new StreamReader(stream);
                // restoring properties
                string jsonString = reader.ReadToEnd();
                Dictionary<string, object> properties =
                    JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                foreach (var propertyPair in properties)
                {
                    this.Properties[propertyPair.Key] = propertyPair.Value;
                }
            }
            catch (FileNotFoundException ex)
            {
                // properties not found
            }
            catch (JsonException ex)
            {
                   // 
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(propertiesStoreFileName, FileMode.Create, storage))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();
                 
                foreach (string key in this.Properties.Keys)
                {
                    properties.Add(key, Properties[key]);
                }

                string jsonString = JsonSerializer.Serialize(properties);
                writer.Write(jsonString);
            }
        }
    }
}
