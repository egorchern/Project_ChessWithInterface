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
            PawnPromotionWindow.Icon = new BitmapImage(new Uri(Globals.PathToResources + "\\ChessIcon.png"));
            string colorSubString = "";
            if(Globals.WhitesTurn == true)
            {
                colorSubString = "White";
            }
            else if(Globals.WhitesTurn == false)
            {
                colorSubString = "Black";
            }
            Queen_btn.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + colorSubString + "Queen.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Rook_btn.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + colorSubString + "Rook.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Bishop_btn.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + colorSubString + "Bishop.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Knight_btn.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.PathToResources + "\\" + colorSubString + "Knight.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Queen_btn.Click += QueenPromotion;
            Rook_btn.Click += RookPromotion;
            Bishop_btn.Click += BishopPromotion;
            Knight_btn.Click += KnightPromotion;
        }

        private void KnightPromotion(object sender, RoutedEventArgs e)//sets the global variable for promotion to knight
        {
            Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { Globals.PositionOfPawnToBePromotedAndPiece[0], 3 };
            this.Close();
        }

        private void BishopPromotion(object sender, RoutedEventArgs e)//sets the global variable for promotion to bishop
        {
            Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { Globals.PositionOfPawnToBePromotedAndPiece[0], 2 };
            this.Close();
        }

        private void RookPromotion(object sender, RoutedEventArgs e)//sets the global variable for promotion to rook
        {
            Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { Globals.PositionOfPawnToBePromotedAndPiece[0], 1 };
            this.Close();
        }

        private void QueenPromotion(object sender, RoutedEventArgs e)//sets the global variable for promotion to queen 
        {
            Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { Globals.PositionOfPawnToBePromotedAndPiece[0], 0 };
            this.Close();
        }
    }
}
