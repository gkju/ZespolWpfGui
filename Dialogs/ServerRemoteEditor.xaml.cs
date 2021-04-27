using System.Windows;
using ZespolWpfGui.SettingsUtils;

namespace ZespolWpfGui.Dialogs
{
    public partial class ServerRemoteEditor : Window
    {
        public ServerRemote Remote { get; set; }
        
        public ServerRemoteEditor(ServerRemote Remote)
        {
            this.Remote = Remote;
            InitializeComponent();
        }
        
        private void CloseButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            if (Remote.Name != "" && Remote.Description != "" && Remote.Url != "")
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Wypełnij wszystkie pola", "Wymagana uwaga", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}