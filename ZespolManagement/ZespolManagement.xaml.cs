using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
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
using System.Windows.Shapes;
using Microsoft.Win32;
using ZespolLib;
using ZespolWpfGui.Dialogs;

namespace ZespolWpfGui.ZespolManagement
{
    /// <summary>
    /// Interaction logic for ZespolManagement.xaml
    /// </summary>
    public partial class ZespolManagement : Window
    {
        private Zespol Zespol { get; set; }
        private ObservableCollection<CzlonekZespolu> ObservableCzlonkowie { get; set; }
        private string SavePath { get; set; }
        private string SaveUrl { get; set; }
        private int ChosenCzlonekIndex { get; set; }

        public ZespolManagement()
        {
            InitializeComponent();
            DataContext = this;
            Zespol = new Zespol();
            BuildObservableCzlonkowie();
            ObservableCzlonkowie.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ObservableCzlonkowieChanged);
        }

        public ZespolManagement(Zespol Zespol, string SavePath = "", string SaveUrl = "") : this()
        {
            this.Zespol = Zespol;
            this.SavePath = SavePath;
            this.SaveUrl = SaveUrl;
            BuildObservableCzlonkowie();
        }

        private void BuildObservableCzlonkowie()
        {
            ObservableCzlonkowie = new ObservableCollection<CzlonekZespolu>(Zespol.Czlonkowie);
        }

        private void ObservableCzlonkowieChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: smart update handling
            Zespol.Czlonkowie = ObservableCzlonkowie.ToList();
        }

        private void SaveFileWithDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = "Pliki json (*.json)|*.json|Pliki xml (*.xml)|*.xml|Pliki yaml (*.yml/*.yaml)|*.yml;*.yaml|Pliki bin, niezalecane (*.bin)|*.bin";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = System.IO.Path.GetFullPath(saveFileDialog.FileName);
                SavePath = filePath;
                SaveFile(filePath);
            }
        }

        private void SaveFileWithRememberedPath()
        {
            SaveFile(SavePath);
        }

        private void SaveFile(string filepath)
        {
            string ext = System.IO.Path.GetExtension(filepath);

            switch (ext)
            {
                case ".json":
                    Zespol.ZapiszJSON(filepath);
                    break;
                case ".xml":
                    Zespol.ZapiszXML(filepath);
                    break;
                case ".yml":
                case ".yaml":
                    Zespol.ZapiszYaml(filepath);
                    break;
                case ".bin":
                    Zespol.ZapiszBin(filepath);
                    break;
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButton(object sender, RoutedEventArgs e)
        {
            if (SavePath != "")
            {
                SaveFileWithRememberedPath();
            }
            else
            {
                SaveFileWithDialog();
            }
        }

        private void PromptToChooseRemote()
        {
            RemotePicker dialog = new RemotePicker();

            if (dialog.ShowDialog() == true)
            {
                SaveUrl = dialog.ChosenRemote.Url;
            }
        }

        private void SaveToRemoteButton(object sender, RoutedEventArgs e)
        {
            if (SaveUrl == "")
            {
                PromptToChooseRemote();
            }
        }

        private void SaveWithNewPath(object sender, RoutedEventArgs e)
        {
            SaveFileWithDialog();
        }

        private void ChangeServer(object sender, RoutedEventArgs e)
        {
            PromptToChooseRemote();
        }

        private void CzlonkowieListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
