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

namespace Project_ChessWithInterface
{
    /// <summary>
    /// Interaction logic for NameForDatabaseDialog.xaml
    /// </summary>
    public partial class NameForDatabaseDialog : Window
    {
        public NameForDatabaseDialog(string Winner)
        {
            InitializeComponent();
            DataBaseDialog.Icon = new BitmapImage(new Uri(Globals.PathToResources + "\\ChessIcon.png"));
            string inquire = Winner;
            
            if(inquire == "Human")
            {
                Nickname_lbl.Content = "Yours nickname:";
            }
            else if(inquire == "Black")
            {
                Nickname_lbl.Content = "Black player's nickname:";
            }
            else if(inquire == "White")
            {
                Nickname_lbl.Content = "White player's nickname:";
            }

        }

        private void submit_btn_Click(object sender, RoutedEventArgs e)
        {
            NicknameChosen = Nickname_txt.Text;
            this.Close();
        }
        public string NicknameChosen
        {
            set { name = value; }
            get { return name; }
        }
        private string name;
    }
}
