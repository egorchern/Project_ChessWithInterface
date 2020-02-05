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
using System.IO;
using System.Text.RegularExpressions;

namespace Project_ChessWithInterface
{
    /// <summary>
    /// Interaction logic for LoadGameDialog.xaml
    /// </summary>
    public partial class LoadGameDialog : Window
    {
        public LoadGameDialog()
        {
            InitializeComponent();
            LoadGameWindow.Icon = new BitmapImage(new Uri(Globals.PathToResources + "\\ChessIcon.png")); 


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string saveSelected = ListBox_AvailableSaves.SelectedItem.ToString();
            saveSelected = Regex.Replace(saveSelected, @"\n.+$", "");
            PathChosen += "\\" + saveSelected;
            this.Close();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string PathToSaves = RootWindow.GetPathToSaves();
            DirectoryInfo r = new DirectoryInfo(PathToSaves);
            FileInfo[] s = r.GetFiles("*.txt");
            List<string> NamesOfSaves = new List<string>();
            foreach (FileInfo file in s)
            {
                NamesOfSaves.Add(file.Name + "\nDate Created: " + file.CreationTime);
            }
            ListBox_AvailableSaves.ItemsSource = NamesOfSaves;
            ListBox_AvailableSaves.FontSize = 20;
            ListBox_AvailableSaves.FontWeight = FontWeights.Bold;
            PathChosen = PathToSaves;
            button.Click += Button_Click;
        }
        public  string PathChosen
        {
            get { return pathChosen; }
            set { pathChosen = value; }
        }
        private string pathChosen;
        
    }
    
}
