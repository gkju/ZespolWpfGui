using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Flurl;
using Flurl.Http;
using ZespolWpfGui.SettingsUtils;
using HandyControl.Themes;
using Microsoft.Win32;
using ZespolLib;
using ZespolWpfGui.Dialogs;
using Path = System.Windows.Shapes.Path;


namespace ZespolWpfGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int WindowCount { get; set; } = 0;
        public ObservableCollection<ZespolFile> ZespolFiles { get; set; }
        public ObservableCollection<ServerRemote> ServerRemotes { get; set; }

        public ObservableCollection<RemoteZespol> RemoteZespols { get; set; } =
            new ObservableCollection<RemoteZespol>();
        public ZespolFile SelectedFile { get; set; }
        public RemoteZespol SelectedRemoteZespol { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            BuildZespolFiles();
            BuildServerRemotes();
            RefreshRemoteZespols();
            RemoteZespolList.ItemsSource = RemoteZespols;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BuildZespolFiles()
        {
            ZespolFiles = new ObservableCollection<ZespolFile>(((App)Application.Current).Settings.RememberedFiles);
            ZespolFilesList.ItemsSource = ZespolFiles;
        }

        private void BuildServerRemotes()
        {
            ServerRemotes =
                new ObservableCollection<ServerRemote>(((App)Application.Current).Settings.RememberedRemotes);
        }

        private void OpenZespolWindow(Zespol Zespol, string SavePath = "", string SaveUrl = "")
        {
            Thread thread = new Thread(() =>
            {
                ZespolManagement.ZespolManagement window =
                    new ZespolManagement.ZespolManagement(Zespol, SavePath, SaveUrl);
                window.Show();

                window.Closed += (sender2, e2) =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Invoke(() => --WindowCount);
                };

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            ++WindowCount;
            thread.Start();
        }

        private void OpenFile(ZespolFile selectedFile)
        {
            Zespol zespol = null;

            string ext = System.IO.Path.GetExtension(selectedFile.FilePath);
            string filepath = System.IO.Path.GetFullPath(selectedFile.FilePath);

            try
            {
                switch (ext)
                {
                    case ".json":
                        zespol = Zespol.OdczytajJSONStatic(filepath);
                        break;
                    case ".xml":
                        zespol = Zespol.OdczytajXMLStatic(filepath);
                        break;
                    case ".yml":
                    case ".yaml":
                        zespol = Zespol.OdczytajYamlStatic(filepath);
                        break;
                    case ".bin":
                        zespol = Zespol.OdczytajBinStatic(filepath);
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Wystąpił błąd (czy plik jest poprawny?)");
            }
            

            if (zespol != null)
            {
                OpenZespolWindow(zespol, filepath);
            }
        }

        private void OpenSelectedFile(object sender = null, RoutedEventArgs e = null)
        {
            if (SelectedFile != null)
            {
                OpenFile(SelectedFile);
            }
        }

        private void OpenFileButton(object sender, RoutedEventArgs e)
        {
            ZespolFile SelectedFile =
                ZespolFiles.Where(file => file.FilePath == ((Button) sender).Tag).FirstOrDefault();
            OpenFile(SelectedFile);
        }

        private void AddRememberedFile(ZespolFile file)
        {
            ((App)Application.Current).Settings.RememberedFiles.Add(file);
            ZespolFiles.Add(file);
        }

        private void AddFile(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter =
                "Pliki json (*.json)|*.json|Pliki xml (*.xml)|*.xml|Pliki yaml (*.yml/*.yaml)|*.yml;*.yaml|Pliki bin, niezalecane (*.bin)|*.bin";
            openFileDialog.Multiselect = true;
            openFileDialog.ValidateNames = true;
            openFileDialog.Title = "Wybierz plik zawierający zespół";
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string[] filenames = openFileDialog.FileNames;

                foreach (var filename in filenames)
                {
                    ZespolFile newFile = new ZespolFile
                    {
                        Name = System.IO.Path.GetFileName(filename),
                        Type = "Plik " + System.IO.Path.GetExtension(filename),
                        FilePath = System.IO.Path.GetFullPath(filename)
                    };

                    AddRememberedFile(newFile);
                }
            }
        }

        private void AddServerRemote(object sender, RoutedEventArgs e)
        {
            AddServerDialog dialog = new AddServerDialog();
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                ((App)Application.Current).Settings.RememberedRemotes.Add(new ServerRemote {Description = dialog.Desc, Name = dialog.Name, Url = dialog.Url});
                RefreshRemoteZespols();
            }
        }

        private async Task RefreshRemoteZespols()
        {
            RemoteZespols.Clear();

            List<ServerRemote> remotes = ((App) Application.Current).Settings.RememberedRemotes;
            foreach (var remote in remotes)
            {
                try
                {
                    List<ZespolCluster> res =
                        await remote.Url.AppendPathSegment("Zespol").GetJsonAsync<List<ZespolCluster>>();
                    foreach (var zespolCluster in res)
                    {
                        RemoteZespols.Add(new RemoteZespol
                        {
                            Id = zespolCluster.Zespol.Id, Name = zespolCluster.Nazwa, zespol = zespolCluster.Zespol,
                            RemoteName = remote.Name,
                            RemoteUrl = remote.Url
                        });
                    }
                }
                catch
                {
                    // TODO: handle unresponsive remotes
                }
                
            }
        }

        private void DeleteSelectedFileFromList(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null)
            {
                ((App)Application.Current).Settings.RememberedFiles = ((App)Application.Current).Settings.RememberedFiles.Where(file => System.IO.Path.GetFullPath(file.FilePath) != System.IO.Path.GetFullPath(SelectedFile.FilePath)).ToList();
                SelectedFile = null;
                BuildZespolFiles();
            }
            else
            {
                MessageBox.Show("Musisz zaznaczyć jakiś plik");
            }
            
        }

        private void OpenRemoteZespol(object sender = null, RoutedEventArgs e = null)
        {
            if (SelectedRemoteZespol != null)
            {
                OpenZespolWindow(SelectedRemoteZespol.zespol, "", SelectedRemoteZespol.RemoteUrl);
            }
        }
        private void RemoteZespolList_OnMouseDoubleClickDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenRemoteZespol();
        }

        private void FileListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedFile();
        }

        private void OpenRemoteButton(object sender, RoutedEventArgs e)
        {
            string Id = ((Button) sender).Tag.ToString();
            var Remote = RemoteZespols.FirstOrDefault(zespol => zespol.Id == Id);

            if (Remote != default(RemoteZespol))
            {
                OpenZespolWindow(Remote.zespol, "", Remote.RemoteUrl);
            }
        }

        private void OpenBlankEditor(object sender, RoutedEventArgs e)
        {
            Zespol zespol = new Zespol();
            zespol.Kierownik = new KierownikZespolu();
            OpenZespolWindow(zespol);
        }

        private void OpenServerManagement(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(() =>
            {
                ServerExplorer window = new ServerExplorer();
                window.Show();

                window.Closed += (sender2, e2) =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Invoke(() => --WindowCount);
                };

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            ++WindowCount;
            thread.Start();
        }

        private void RefreshRemotesButton(object sender, RoutedEventArgs e)
        {
            RefreshRemoteZespols();
        }

        private void HandleClosing(object sender, CancelEventArgs e)
        {
            if (WindowCount > 0)
            {
                e.Cancel = true;
                MessageBox.Show("Zamknij wszystkie okna przed zamknięciem głównego", "Ważne", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }
    }
}
