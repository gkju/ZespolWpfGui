using System;
using System.Collections.Generic;
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

namespace ZespolWpfGui.Dialogs
{
    /// <summary>
    /// Interaction logic for PromptSave.xaml
    /// </summary>
    public partial class PromptSave : Window
    {
        public PromptSaveResult result { get; set; }

        public PromptSave()
        {
            InitializeComponent();
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void YesFile(object sender, RoutedEventArgs e)
        {
            result = PromptSaveResult.File;
            DialogResult = true;
        }

        private void YesServer(object sender, RoutedEventArgs e)
        {
            result = PromptSaveResult.Server;
            DialogResult = true;
        }

        private void YesFileAndServer(object sender, RoutedEventArgs e)
        {
            result = PromptSaveResult.FileAndServer;
            DialogResult = true;
        }

        private void No(object sender, RoutedEventArgs e)
        {
            result = PromptSaveResult.No;
            DialogResult = true;
        }
    }

    public enum PromptSaveResult
    {
        No, File, Server, FileAndServer
    }
}
