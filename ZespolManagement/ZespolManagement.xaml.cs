﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using ABI.Windows.Networking.BackgroundTransfer;
using Flurl;
using Flurl.Http;
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
        public Zespol Zespol { get; }
        // Czy coś zostało zmienione wykrywam na podstawie różnić między końcowym json stringiem zespołu a początkowym, co nie jest optymalne, ale ze względu na np overload is equal osoby najłatwiej tak to zrobić
        // TODO: robienie głębokiego porównania ogzespol i zespol
        private string OgZespolString { get; }
        private ObservableCollection<CzlonekZespolu> ObservableCzlonkowie { get; set; }
        private string SavePath { get; set; }
        private string SaveUrl { get; set; }
        public int ChosenCzlonekIndex { get; set; }

        public ZespolManagement(Zespol Zespol, string SavePath = "", string SaveUrl = "")
        {
            this.Zespol = Zespol;
            this.SavePath = SavePath;
            this.SaveUrl = SaveUrl;
            this.OgZespolString = Zespol.GetJSONString();
            BuildObservableCzlonkowie();
            InitializeComponent();
            CzlonkowieList.ItemsSource = ObservableCzlonkowie;
            ObservableCzlonkowie.CollectionChanged += ObservableCzlonkowieChanged;
            SexChooser.SelectedIndex = (int) Zespol.Kierownik.Plec;
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

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButton(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
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

        private void PromptToChooseRemote()
        {
            RemotePicker dialog = new RemotePicker();

            if (dialog.ShowDialog() == true)
            {
                SaveUrl = dialog.ChosenRemote.Url;
            }
        }

        private async void SaveToRemote()
        {
            if (SaveUrl == "")
            {
                PromptToChooseRemote();
            }

            await SaveUrl.AppendPathSegment("Zespol").PostJsonAsync(Zespol);
        }

        private void SaveToRemoteButton(object sender, RoutedEventArgs e)
        {
            SaveToRemote();
        }

        private void SaveWithNewPath(object sender, RoutedEventArgs e)
        {
            SaveFileWithDialog();
        }

        private void ChangeServer(object sender, RoutedEventArgs e)
        {
            PromptToChooseRemote();
        }

        private void EditCzlonek()
        {
            EdytujCzlonka dialog = new EdytujCzlonka((CzlonekZespolu)ObservableCzlonkowie[ChosenCzlonekIndex].Clone());

            if (dialog.ShowDialog() == true)
            {
                ObservableCzlonkowie[ChosenCzlonekIndex] = dialog.Czlonek;
            }
        }

        private void CzlonkowieListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditCzlonek();
        }

        private void EditCzlonekButton(object sender, RoutedEventArgs e)
        {
            EditCzlonek();
        }

        private void SexChooser_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Zespol.Kierownik.Plec = (Plcie)SexChooser.SelectedIndex;
        }

        private void DeleteCzlonek(object sender, RoutedEventArgs e)
        {
            CzlonekZespolu chosenCzlonek = ObservableCzlonkowie[ChosenCzlonekIndex];
            var dr =
                MessageBox.Show($"Czy jesteś pewien, że chcesz usunąć {chosenCzlonek.Imie} {chosenCzlonek.Nazwisko}?",
                    "Usuwanie członka", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (dr == MessageBoxResult.Yes)
            { 
                ObservableCzlonkowie.RemoveAt(ChosenCzlonekIndex);
            }
            
        }

        private void AddCzlonek(object sender, RoutedEventArgs e)
        {
            CzlonekZespolu czlonek = new CzlonekZespolu();

            EdytujCzlonka dialog = new EdytujCzlonka(czlonek);

            dialog.Title = "Dodaj członka";

            if (dialog.ShowDialog() == true)
            {
                ObservableCzlonkowie.Add(dialog.Czlonek);
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            bool zmiana = OgZespolString != Zespol.GetJSONString();
            if (zmiana)
            {
                PromptSave dialog = new PromptSave();
                dialog.Title = $"Czy chcesz zapisać {Zespol.Nazwa}?";

                if (dialog.ShowDialog() == true)
                {
                    switch (dialog.result)
                    {
                        case PromptSaveResult.File:
                            SaveFile();
                            break;
                        case PromptSaveResult.Server:
                            SaveToRemote();
                            break;
                        case PromptSaveResult.FileAndServer:
                            SaveFile();
                            SaveToRemote();
                            break;
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
