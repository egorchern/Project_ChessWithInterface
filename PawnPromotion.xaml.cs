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
    /// Interaction logic for PawnPromotion.xaml
    /// </summary>
    public partial class PawnPromotion : Window
    {
        public PawnPromotion()
        {
            InitializeComponent();
            string colorSubString = "";
            if(Globals.WhitesTurn == true)
            {
                colorSubString = "White";
            }
            else if(Globals.WhitesTurn == false)
            {
                colorSubString = "Black";
            }
            btn0.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + colorSubString + "Queen.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            btn1.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + colorSubString + "Rook.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            btn2.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + colorSubString + "Bishop.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            btn3.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + colorSubString + "Knight.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            btn0.Click += Btn0_Click;
            btn1.Click += Btn1_Click;
            btn2.Click += Btn2_Click;
            btn3.Click += Btn3_Click;
        }

        private void Btn3_Click(object sender, RoutedEventArgs e)
        {
            Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { Globals.PositionOfPawnToBePromotedAndPiece[0], 3 };
            this.Close();
        }

        private void Btn2_Click(object sender, RoutedEventArgs e)
        {
            Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { Globals.PositionOfPawnToBePromotedAndPiece[0], 2 };
            this.Close();
        }

        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { Globals.PositionOfPawnToBePromotedAndPiece[0], 1 };
            this.Close();
        }

        private void Btn0_Click(object sender, RoutedEventArgs e)
        {
            Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { Globals.PositionOfPawnToBePromotedAndPiece[0], 0 };
            this.Close();
        }
    }
}
