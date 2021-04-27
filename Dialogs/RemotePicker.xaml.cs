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
using System.Windows.Shapes;
using HandyControl.Controls;
using ZespolWpfGui.SettingsUtils;
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;

namespace ZespolWpfGui.Dialogs
{
    /// <summary>
    /// Interaction logic for RemotePicker.xaml
    /// </summary>
    public partial class RemotePicker : Window
    {
        public int Index { get; set; } = -1;
        public ServerRemote ChosenRemote { get; set; }
        private ObservableCollection<ServerRemote> RemotesAvailable { get; set; }

        public RemotePicker()
        {
            DataContext = this;
            InitializeComponent();
            RebuildRemotesAvailable();
        }

        void RebuildRemotesAvailable()
        {
            RemotesAvailable =
                new ObservableCollection<ServerRemote>(((App) Application.Current).Settings.RememberedRemotes);
            ServerRemoteList.ItemsSource = RemotesAvailable;
        }
        private void Submit(object sender, RoutedEventArgs e)
        {
            if (Index == -1)
            {
                MessageBox.Show("Wybierz jakiś serwer");
            }
            else
            {
                ChosenRemote = RemotesAvailable[Index];
                DialogResult = true;
            }
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AddRemote()
        {
            AddServerDialog dialog = new AddServerDialog();
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                ((App)Application.Current).Settings.RememberedRemotes.Add(new ServerRemote {Description = dialog.Desc, Name = dialog.Name, Url = dialog.Url});
                RebuildRemotesAvailable();
            }
        }

        private void AddRemoteButton(object sender, RoutedEventArgs e)
        {
            AddRemote();
        }
    }
}
