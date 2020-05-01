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
            ColorSelectionDialog.Icon = new BitmapImage(new Uri(Globals.PathToResources + "\\ChessIcon.png"));
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

        private void BlackKing_btn_Click(object sender, RoutedEventArgs e)//Sets the global variable AI equal to White, which in turn sets the human player colour to Black
        {
            Globals.AI = "W";
            this.Close();
        }

        private void WhiteKing_btn_Click(object sender, RoutedEventArgs e)//Sets the global variable AI equal to Black, which in turn sets the human player colour to White
        {
            Globals.AI = "B";
            this.Close();
        }
    }
}
