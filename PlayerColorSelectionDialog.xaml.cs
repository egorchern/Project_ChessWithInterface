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

namespace Project_ChessWithInterface
{
    /// <summary>
    /// Interaction logic for PlayerColorSelectionDialog.xaml
    /// </summary>
    public partial class PlayerColorSelectionDialog : Window
    {
        public PlayerColorSelectionDialog()
        {
            InitializeComponent();
            WhiteKing_btn.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + "WhiteKing.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            BlackKing_btn.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + "BlackKing.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            WhiteKing_btn.Click += WhiteKing_btn_Click;
            BlackKing_btn.Click += BlackKing_btn_Click;
        }

        private void BlackKing_btn_Click(object sender, RoutedEventArgs e)
        {
            Globals.AI = "W";
            this.Close();
        }

        private void WhiteKing_btn_Click(object sender, RoutedEventArgs e)
        {
            Globals.AI = "B";
            this.Close();
        }
    }
}
