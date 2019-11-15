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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Project_ChessWithInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string pathToResources = Environment.CurrentDirectory;
            pathToResources = Regex.Replace(pathToResources, @"\\", "¬");
            var temp = pathToResources.Split('¬').ToList();
            temp.RemoveAt(temp.Count - 1);
            temp.RemoveAt(temp.Count - 1);
            for(int i = 0; i < temp.Count; i++)
            {
                temp[i] += @"\";
            }
            pathToResources = String.Join("", temp) + "Resources";
            double d = 0.0;
            image1.Source = new BitmapImage(new Uri(pathToResources + "\\WhiteKing.png"));
            
        }
    }
}
