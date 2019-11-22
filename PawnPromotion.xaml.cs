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
                Source = new BitmapImage(new Uri(Globals.pathToResources + "\\" + colorSubString + "Queen.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            btn1.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.pathToResources + "\\" + colorSubString + "Rook.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            btn2.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.pathToResources + "\\" + colorSubString + "Bishop.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            btn3.Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.pathToResources + "\\" + colorSubString + "Knight.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
        }
    }
}
