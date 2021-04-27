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
using ZespolLib;

namespace ZespolWpfGui.Dialogs
{
    /// <summary>
    /// Interaction logic for EdytujCzlonka.xaml
    /// </summary>
    public partial class EdytujCzlonka : Window
    {
        public CzlonekZespolu Czlonek { get; set; }

        public EdytujCzlonka(CzlonekZespolu Czlonek)
        {
            this.Czlonek = Czlonek;
            InitializeComponent();
            SexChooser.SelectedIndex = (int)Czlonek.Plec;
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            if (Czlonek != null && Czlonek.Imie != "" && Czlonek.Nazwisko != "" && Czlonek.Funkcja != "" && Czlonek.PESEL != "" && Czlonek.DataUrodzenia != default(DateTime))
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Wypełnij wszystkie pola");
            }
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SexChooser_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Czlonek.Plec = (Plcie) SexChooser.SelectedIndex;
        }
    }
}
