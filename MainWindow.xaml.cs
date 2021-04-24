using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using ZespolWpfGui.Settings;
using HandyControl.Themes;


namespace ZespolWpfGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ZespolFile> ZespolFiles { get; set; } = new ObservableCollection<ZespolFile>();
        public ObservableCollection<ServerRemote> ServerRemotes { get; set; } = new ObservableCollection<ServerRemote>();
        public ZespolFile SelectedFile { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                ServerRemotes = new ObservableCollection<ServerRemote>((List<ServerRemote>) Application.Current.Properties["ServerRemotes"]);
            }
            catch
            {

            }
            try
            {
                ZespolFiles = new ObservableCollection<ZespolFile>((List<ZespolFile>) Application.Current.Properties["ZespolFiles"]);
            }
            catch
            {

            }

            ZespolFiles = new ObservableCollection<ZespolFile>()
                {new ZespolFile {Description = "waweaweaw", Name = "fajny plik"}};

            ZespolFilesList.ItemsSource = ZespolFiles;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenFile(ZespolFile selectedFile)
        {
            ZespolFiles.Add(new ZespolFile {Description = selectedFile.Name, Name = selectedFile.Description});
        }

        private void OpenFileDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFile(SelectedFile);
        }

        private void OpenFileButton(object sender, RoutedEventArgs e)
        {
            ZespolFile SelectedFile =
                ZespolFiles.Where(file => file.FilePath == ((Button) sender).Tag).FirstOrDefault();
            OpenFile(SelectedFile);
        }
    }
}
