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

namespace Project_ChessWithInterface
{
    /// <summary>
    /// Interaction logic for SaveGameDialog.xaml
    /// </summary>
    public partial class SaveGameDialog : Window
    {
        public SaveGameDialog()
        {
            InitializeComponent();
        }
        
        private void btn_Submit_Click(object sender, RoutedEventArgs e)
        {
            SaveGame();
        }
        public void SaveGame()
        {
            string ans = txt_FileNameForSave.Text;


            if (ans.Length > 0)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                var temp = path.Split('\\').ToList();
                temp.RemoveAt(temp.Count - 1);
                temp.RemoveAt(temp.Count - 1);
                temp.RemoveAt(temp.Count - 1);
                path = String.Join("\\", temp);
                path += $"\\Saves\\{ans}.txt";
                bool FileExists = File.Exists(path);
                Globals.PathToSave = path;
                if (FileExists == true)
                {
                    MessageBox.Show("A file with the same file name already exists in Save folder! Please choose another name!");
                }
                else
                {
                    var t = File.Create(path);
                    t.Close();



                    List<string> ToWrite = new List<string>();
                    for (int i = 0; i < 64; i++)
                    {
                        ToWrite.Add($"{i}={Globals.Board[i]}");
                    }
                    if (Globals.WhitesTurn == true)
                    {
                        ToWrite.Add("true");
                    }
                    else
                    {
                        ToWrite.Add("false");
                    }
                    for (int i = 0; i < Globals.MoveRecord.Count; i++)
                    {
                        ToWrite.Add(Globals.MoveRecord[i]);
                    }
                    string AI = "";
                    if (Globals.AI == null)
                    {
                        AI = "null";
                    }
                    else
                    {
                        AI = Globals.AI;
                    }
                    ToWrite.Add(AI);
                    ToWrite.Add(Convert.ToString(Globals.PrimePlayerTimerTimeSeconds));
                    ToWrite.Add(Convert.ToString(Globals.OtherPlayerTimerTimeSeconds));
                    File.WriteAllLines(path, ToWrite);


                    MessageBox.Show("Save file was successfuly created");
                    this.Close();
                }




            }
        }
    }
}
