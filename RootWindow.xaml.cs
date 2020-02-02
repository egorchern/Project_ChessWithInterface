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
            newGame_btn.Click += newGameBtn_Click;
            loadGame_btn.Click += LoadGameBtn_Click;
            gameArchive_btn.Click += GameArchiveBtn_Click;
            Globals.PathToResources = GetPathToResources();

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

        private void LoadGameBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadGameDialog loadDialog = new LoadGameDialog();
            loadDialog.ShowDialog();
            string SaveChosen = "";
            
            SaveChosen = loadDialog.PathChosen;
            
            
            LoadGame(SaveChosen);
            MainWindow instance = new MainWindow();
            instance.Show();
            this.Close();
            
        }

        private void newGameBtn_Click(object sender, RoutedEventArgs e)
        {
            StartGameParameters startGame = new StartGameParameters();
            startGame.Show();
            this.Close();
        }
        public static void LoadGame(string path)
        {
            
            List<string> SaveLines = File.ReadAllLines(path).ToList();
            InitializeBoardLoad(SaveLines);

        }
        public static void InitializeBoardLoad(List<string> list)
        {
            for (int i = 0; i < 8 * 8; i++)
            {
                Globals.Board.Add("00");
            }
            for (int i = 0; i < Globals.Board.Count; i++)
            {
                string temp = list[i];
                temp = Regex.Replace(temp, @"\d*=", "");
                Globals.Board[i] = temp;
                
            }
            if (list[64] == "true")
            {
                Globals.WhitesTurn = true;
            }
            else
            {
                Globals.WhitesTurn = false;
            }
            if (list.Count > 64)
            {
                int counter = 65;
                while(1 == 1) 
                {
                    
                    Globals.MoveRecord.Add(list[counter]);
                    counter++;
                    if (list[counter] == "null" || list[counter] == "B" || list[counter] == "W")
                    {
                        break;
                    }
                }
                string AI = null;
                if(list[counter] == "B")
                {
                    AI = "B";
                }
                else if(list[counter] == "W")
                {
                    AI = "W";
                }

                Globals.AI = AI;
                Globals.PrimePlayerTimerTimeSeconds = Convert.ToInt32(list[counter + 1]);
                Globals.OtherPlayerTimerTimeSeconds = Convert.ToInt32(list[counter + 2]);
                
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
            double d = 0.0;


        }

        private void GameArchiveBtn_Click(object sender, RoutedEventArgs e)
        {
            GameArchive archive = new GameArchive();
            archive.Show();
            this.Close();
        }
    }
}
