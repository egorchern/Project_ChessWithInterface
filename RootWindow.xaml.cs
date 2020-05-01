﻿using System;
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
            newGame_btn.Click += NewGameBtn_Click;
            loadGame_btn.Click += LoadGameBtn_Click;
            gameArchive_btn.Click += GameArchiveBtn_Click;
            Globals.PathToResources = GetPathToResources();
            RootMenu.Icon = new BitmapImage(new Uri(Globals.PathToResources + "\\ChessIcon.png"));
            

        }
        public static string GetPathToResources()//Returns the full path to the Resources folder which is in the root project folder
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

        private void LoadGameBtn_Click(object sender, RoutedEventArgs e)//An event handler if the load game button is clicked, displays the loadGame dialog window in which the user selects the save file to load. Also closes the RootWindow
        {
            LoadGameDialog loadDialog = new LoadGameDialog();
            loadDialog.ShowDialog();
            string SaveChosen = "";
            
            SaveChosen = loadDialog.PathChosen;
            
            
            int GameMode = LoadGame(SaveChosen);
            MainWindow instance = new MainWindow(GameMode);
            instance.Show();
            this.Close();
            
        }

        private void NewGameBtn_Click(object sender, RoutedEventArgs e)//An event handler which is called if the new game button is clicked, displays the second menu that has new game parameters which then need to be filled by user. Also closes the RootWindow
        {
            StartGameParameters startGame = new StartGameParameters();
            startGame.Show();
            this.Close();
        }
        public static int LoadGame(string path)//Driver function that calls InitializeBoardLoad
        {
            
            List<string> SaveLines = File.ReadAllLines(path).ToList();
            int GameMode = InitializeBoardLoad(SaveLines);
            return GameMode;

        }
        public static int InitializeBoardLoad(List<string> list)//If the ‘load game’ option is chosen in the main menu, this subroutine initializes important parameters for the main game loop window such as positions of all pieces and time remaining on timers
        {
            int counter = 0;
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
                counter = 65;
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
            return Convert.ToInt32(list[counter + 3]);


        }

        private void GameArchiveBtn_Click(object sender, RoutedEventArgs e)//An event handler if the game archive button is clicked, displays the GameArchive window and closes the RootWindow
        {
            GameArchive archive = new GameArchive();
            archive.Show();
            this.Close();
        }
    }
}
