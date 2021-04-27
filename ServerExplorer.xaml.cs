using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ZespolWpfGui.Dialogs;
using ZespolWpfGui.SettingsUtils;

namespace ZespolWpfGui
{
    public partial class ServerExplorer : Window
    {
        public ServerRemote ChosenRemote { get; set; } = null;
        public int ChosenIndex { get; set; } = -1;
        public ObservableCollection<ServerRemote> Remotes { get; set; } =
            new ObservableCollection<ServerRemote>(((App) Application.Current).Settings.RememberedRemotes.ToList());
        public ServerExplorer()
        {
            InitializeComponent();
        }
        
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RebuildRemotesList()
        {
            Remotes = new ObservableCollection<ServerRemote>(((App) Application.Current).Settings.RememberedRemotes.ToList());
            RemotesListView.ItemsSource = Remotes;
        }

        private void OpenEditDialog()
        {
            ServerRemoteEditor window = new ServerRemoteEditor(ChosenRemote.Clone());

            if (window.ShowDialog() == true)
            {
                // TODO: obecnie zakładamy, że inne okno mogło NIE w między czasie zmienić rememberedremotes
                int index = ChosenIndex;

                if (index != -1)
                {
                    ((App) Application.Current).Settings.RememberedRemotes[index] = window.Remote;
                    RebuildRemotesList(); 
                }
            }
        }

        private void DoubleClickHandler(object sender, MouseButtonEventArgs e)
        {
            OpenEditDialog();
        }

        private void EditServerButton(object sender, RoutedEventArgs e)
        {
            OpenEditDialog();
        }

        private void DeleteServerButton(object sender, RoutedEventArgs e)
        {
            if (ChosenRemote != null && ChosenIndex != -1)
            {
                var dr =
                    MessageBox.Show($"Czy jesteś pewien, że chcesz usunąć {ChosenRemote.Name}?",
                        "Usuwanie serwera", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (dr == MessageBoxResult.Yes)
                {
                    int index = ChosenIndex;
                    if (index != -1)
                    {
                        ((App) Application.Current).Settings.RememberedRemotes.RemoveAt(index);
                        RebuildRemotesList();
                    }
                    else
                    {
                        MessageBox.Show("Nie udało się usunąć serwera (czy już został usunięty?)", "Wymagana uwaga", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    
                }
            }
             
        }
    }
}