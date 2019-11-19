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
                path += $"\\Saves\\{ans}.bin";
                bool FileExists = File.Exists(path);
                if (FileExists == true)
                {
                    MessageBox.Show("A file with the same file name already exists in Save folder! Please choose another name!");
                }
                else
                {
                    var t = File.Create(path);
                    t.Close();


                    using (FileStream stream = new FileStream(path, FileMode.Open))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                writer.Write($"{i}={Globals.Board[i]}");
                            }
                            if (Globals.WhitesTurn == true)
                            {
                                writer.Write("true");
                            }
                            else
                            {
                                writer.Write("false");
                            }
                            for (int i = 0; i < Globals.MoveRecord.Count; i++)
                            {
                                writer.Write(Globals.MoveRecord[i]);
                            }

                        }
                    }
                    MessageBox.Show("Save file was successfuly created");
                    this.Close();
                }




            }
        }
    }
}
