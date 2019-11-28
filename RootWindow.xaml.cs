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
    /// Interaction logic for RootWindow.xaml
    /// </summary>
    public partial class RootWindow : Window
    {
        public RootWindow()
        {
            InitializeComponent();
            button.Click += Button_Click;
            button_Copy.Click += Button_Copy_Click;
        }
        public static string GetPathToSaves()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var temp = path.Split('\\').ToList();
            temp.RemoveAt(temp.Count - 1);
            temp.RemoveAt(temp.Count - 1);
            temp.RemoveAt(temp.Count - 1);
            path = String.Join("\\", temp);
            path += $"\\Saves\\";
            
            Globals.PathToSave = path;
            return path;
        }

        private void Button_Copy_Click(object sender, RoutedEventArgs e)
        {
            LoadGameDialog loadDialog = new LoadGameDialog();
            loadDialog.ShowDialog();
            string SaveChosen = "";
            SaveChosen = res.PathChosen;
            LoadGame(SaveChosen);
            MainWindow instance = new MainWindow();
            instance.Show();
            this.Close();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow instance = new MainWindow();
            MainWindow.InitializeBoard();
            Globals.WhitesTurn = true;
            instance.Show();
            
            this.Close();
        }
        public static void LoadGame(string path)
        {
            byte[] fileBytes = File.ReadAllBytes(path);

            string ter = "";
            foreach (byte b in fileBytes)
            {
                ter += (char)b;
            }

            List<string> arr = new List<string>();
            arr = ter.Split('\u0005', '\u0004').ToList();
            string temp = arr.Last();
            var p = temp.Split('\r', '\u0016', '\u0017', '\u000E').ToList();
            arr.RemoveAt(0);
            arr.RemoveAt(arr.Count - 1);
            foreach (string element in p)
            {
                arr.Add(element);
            }
            InitializeBoardLoad(arr);


        }
        public static void InitializeBoardLoad(List<string> l)
        {
            for (int i = 0; i < 8 * 8; i++)
            {
                Globals.Board.Add("00");
            }
            for (int i = 0; i < Globals.Board.Count; i++)
            {
                string temp = l[i];
                temp = Regex.Replace(temp, @"\d*=", "");
                Globals.Board[i] = temp;
                double d = 0.0;
            }
            if (l[64] == "true")
            {
                Globals.WhitesTurn = true;
            }
            else
            {
                Globals.WhitesTurn = false;
            }
            if (l.Count > 64)
            {
                for (int i = 65; i < l.Count; i++)
                {
                    Globals.MoveRecord.Add(l[i]);
                }
            }
            bool whiteKingMoved = false;
            bool blackKingMoved = false;
            var tempList = new List<string>();
            foreach (string item in Globals.MoveRecord)
            {
                string forReplacement = item;
                forReplacement = Regex.Replace(forReplacement, @"\d*: ", "");
                tempList.Add(forReplacement);
            }
            if (tempList.Any(x => x.StartsWith("Wk")) == true)
            {
                whiteKingMoved = true;
            }
            if (tempList.Any(x => x.StartsWith("Bk")) == true)
            {
                blackKingMoved = true;
            }




            if (whiteKingMoved == true)
            {
                Globals.WhiteKingMoved = true;
            }
            else if (blackKingMoved == true)
            {
                Globals.BlackKingMoved = true;
            }


        }
    }
}
