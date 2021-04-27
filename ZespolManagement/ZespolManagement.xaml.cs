using System;
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
        private string OgZespolString { get; set; }
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
            bool res = SaveFile();
            HandleButtonRes(res);
        }

        // zwraca true jeśli zapisał plik
        private bool SaveFile()
        {
            if (SavePath != "")
            {
                return SaveFileWithRememberedPath();
            }
            else
            {
                return SaveFileWithDialog();
            }
        }

        private bool SaveFileWithDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = "Pliki json (*.json)|*.json|Pliki xml (*.xml)|*.xml|Pliki yaml (*.yml/*.yaml)|*.yml;*.yaml|Pliki bin, niezalecane (*.bin)|*.bin";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = System.IO.Path.GetFullPath(saveFileDialog.FileName);
                SavePath = filePath;
                return SaveFile(filePath);
            }

            return false;
        }

        private bool SaveFileWithRememberedPath()
        {
            return SaveFile(SavePath);
        }

        private bool SaveFile(string filepath)
        {
            string ext = System.IO.Path.GetExtension(filepath);

            try
            {
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

                OgZespolString = Zespol.GetJSONString();
                return true;
            }
            catch
            {
                return false;
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

        // zwraca false jeśli nic nie zapisał
        private async Task<bool> SaveToRemote()
        {
            if (SaveUrl == "")
            {
                PromptToChooseRemote();

                while (SaveUrl == "")
                {
                    var dr = MessageBox.Show(
                        "Nie wybrałeś żadnego serwera. Czy chcesz spróbować ponownie?",
                        "Wymagana uwaga", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (dr == MessageBoxResult.Yes)
                    {
                        PromptToChooseRemote();
                    }
                    else if(dr == MessageBoxResult.No)
                    {
                        return false;
                    }
                }
            }

            try
            {
                var resp = await SaveUrl.AppendPathSegment("Zespol").PostJsonAsync(Zespol);
                if (resp.StatusCode == 200)
                {
                    OgZespolString = Zespol.GetJSONString();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
            
        }

        private async void SaveToRemoteButton(object sender, RoutedEventArgs e)
        {
            bool res = await SaveToRemote();
            HandleButtonRes(res);
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
            if (ChosenCzlonekIndex != -1)
            {
                EdytujCzlonka dialog = new EdytujCzlonka((CzlonekZespolu)ObservableCzlonkowie[ChosenCzlonekIndex].Clone());

                if (dialog.ShowDialog() == true)
                {
                    ObservableCzlonkowie[ChosenCzlonekIndex] = dialog.Czlonek;
                }
            }
            else
            {
                MessageBox.Show("Wybierz jakiegoś członka", "Wymagana uwaga", MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
            if (ChosenCzlonekIndex != -1)
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
            else
            {
                MessageBox.Show("Wybierz jakiegoś członka", "Wymagana uwaga", MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

        private async void OnClosing(object sender, CancelEventArgs e)
        {
            bool res;
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
                            res = SaveFile();
                            HandleRes(res, e);
                            break;
                        case PromptSaveResult.Server:
                            res = await SaveToRemote();
                            HandleRes(res, e);
                            break;
                        case PromptSaveResult.FileAndServer:
                            res = SaveFile();
                            bool res2 = await SaveToRemote();
                            HandleRes(res, e, res2);
                            break;
                        case PromptSaveResult.No:
                            break;
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void HandleButtonRes(bool res)
        {
            if (!res)
            {
                MessageBox.Show(
                    "Nie udało zapisać się zespołu", 
                    "Wymagana uwaga", 
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(
                    "Pomyślnie zapisano zespół", 
                    "Sukces", 
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void HandleRes(bool res, CancelEventArgs e, bool? res2 = null)
        {
            if (!res)
            {
                MessageBox.Show(string.Format("Nie udało zapisać się pliku{0}", res2 == true ?  ", ale udało się na serwer" : ""), "Wymagana uwaga", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                e.Cancel = true;
            } else if (res2 == false)
            {
                MessageBox.Show(string.Format("Nie udało zapisać się na serwer{0}", res == true ?  ", ale udało się plik" : ""), "Wymagana uwaga", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                e.Cancel = true;
            }
        }

        private void SaveAll(object sender, RoutedEventArgs e)
        {
            SaveFile();
            SaveToRemote();
        }
    }
}
