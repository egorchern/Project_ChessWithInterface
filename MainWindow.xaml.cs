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
using System.Web.UI.HtmlControls;



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

            

            
            GetAllButtonElements();
            
            Globals.pathToResources = GetPathToResources();

            foreach(Button btn in Globals.AllButtons)
            {
                btn.Click += UniversalSquareClickEventHandle;
            }
            foreach(string item in Globals.PathsToPieces)
            {
                comboBox.Items.Add(item);
            }
                
                
        }
        public static bool CanProceedWithTurn(int index)
        {
            return true;
        }
        private void UniversalSquareClickEventHandle(object sender, RoutedEventArgs e)
        {
            
            string name = ((Button)sender).Name;
            string subStringForIndex = Regex.Replace(name, @"^btn", "");
            int indexOfClickedSquare = Convert.ToInt32(subStringForIndex);
            bool Verified = CanProceedWithTurn(indexOfClickedSquare);
            string pathToPiece = comboBox.SelectedItem.ToString();
            int index = 0;
            for(int i = 0; i < Globals.AllButtons.Count; i++)
            {
                string scopedName = Globals.AllButtons[i].Name;
                if(scopedName == name)
                {
                    index = i;
                    break;
                }
            }

            Globals.AllButtons[index].Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.pathToResources + pathToPiece)),
                VerticalAlignment = VerticalAlignment.Center

            };
            
        }
        public void GetAllButtonElements()
        {
            /// casting the content into panel
            Panel mainContainer = (Panel)this.Content;

            /// GetAll UIElement
            UIElementCollection element = mainContainer.Children;

            /// casting the UIElementCollection into List
            List<FrameworkElement> lstElement = element.Cast<FrameworkElement>().ToList();

            /// Geting all Control from list
            var lstControl = lstElement.OfType<Control>();

            foreach (Control control in lstControl)
            {
                Type s = control.GetType();
                if(s.Name == "Button")
                {
                    Globals.AllButtons.Add(control as Button);
                    
                }
               
            }
        }


        public static string GetPathToResources()
        {
            string pathToResources = Environment.CurrentDirectory;
            pathToResources = Regex.Replace(pathToResources, @"\\", "¬");
            var temp = pathToResources.Split('¬').ToList();
            temp.RemoveAt(temp.Count - 1);
            temp.RemoveAt(temp.Count - 1);
            for (int i = 0; i < temp.Count; i++)
            {
                temp[i] += @"\";
            }
            pathToResources = String.Join("", temp) + "Resources";
            return pathToResources;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public static class Globals
    {
        public static int CurrentySelected = 0;
        public static List<string> PathsToPieces = new List<string> { "\\WhiteRook.png", "\\WhiteKnight.png", "\\WhiteBishop.png", "\\WhiteQueen.png", "\\WhiteKing.png", "\\WhitePawn.png", "\\BlackRook.png", "\\BlackKnight.png", "\\BlackBishop.png", "\\BlackQueen.png", "\\BlackKing.png", "\\BlackPawn.png" };
        public static string pathToResources = "";
        public static List<Button> AllButtons = new List<Button>();
        public static int CurrentSquareClicked = -1;



    }
    

}

