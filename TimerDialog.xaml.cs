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
using System.Text.RegularExpressions;

namespace Project_ChessWithInterface
{
    /// <summary>
    /// Interaction logic for TimerDialog.xaml
    /// </summary>
    public partial class TimerDialog : Window
    {
        public TimerDialog()
        {
            InitializeComponent();
            TimersWindow.Icon = new BitmapImage(new Uri(Globals.PathToResources + "\\ChessIcon.png"));
            if (Globals.AI == null)
            {

            }
            else
            {
                WhiteLabel.Margin = new Thickness(257, 12, 0, 0);
                BlackLabel.Visibility = Visibility.Hidden;
                BlackTime_txt.Visibility = Visibility.Hidden;
                WhiteTime_txt.Margin = new Thickness(319, 107, 0, 0);
                WhiteLabel.Content = "Time limit\n(null for unlimited)";
            }
        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {

            if (BlackTime_txt.Visibility == Visibility.Hidden)
            {
                Globals.OtherPlayerTimerTimeSeconds = -1;
                if (WhiteTime_txt.Text != "null")
                {


                    Match matchForTime = Regex.Match(WhiteTime_txt.Text, @"^(?<minutes>0|[1-9][0-9]?):(?<seconds>(00)|(0[1-9])|[1-9][0-9]?)$");
                    if (matchForTime.Success == true)
                    {
                        int seconds;
                        int minutes;
                        minutes = Convert.ToInt32(matchForTime.Groups["minutes"].Value);
                        seconds = minutes * 60 + Convert.ToInt32(matchForTime.Groups["seconds"].Value);
                        Globals.PrimePlayerTimerTimeSeconds = seconds;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Invalid time!");
                    }
                }
                else
                {
                    Globals.PrimePlayerTimerTimeSeconds = -1;
                    this.Close();
                }



            }
            else
            {
                bool PrimeNulled = false;
                if (WhiteTime_txt.Text == "null")
                {
                    Globals.PrimePlayerTimerTimeSeconds = -1;

                }
                if (BlackTime_txt.Text == "null")
                {
                    Globals.OtherPlayerTimerTimeSeconds = -1;
                }
                if (Globals.PrimePlayerTimerTimeSeconds != -1)
                {



                    Match matchForTime = Regex.Match(WhiteTime_txt.Text, @"^(?<minutes>0|[1-9][0-9]?):(?<seconds>(00)|(0[1-9])|[1-9][0-9]?)$");
                   
                    if (matchForTime.Success == true )
                    {
                        int seconds;
                        int minutes;
                        minutes = Convert.ToInt32(matchForTime.Groups["minutes"].Value);
                        seconds = minutes * 60 + Convert.ToInt32(matchForTime.Groups["seconds"].Value);
                        Globals.PrimePlayerTimerTimeSeconds = seconds;
                        

                    }
                    else
                    {
                        MessageBox.Show("Invalid time!");
                    }
                }
                
                if(Globals.OtherPlayerTimerTimeSeconds != -1)
                {
                    Match matchForTimeBlack = Regex.Match(BlackTime_txt.Text, @"^(?<minutes>0|[1-9][0-9]?):(?<seconds>(00)|(0[1-9])|[1-9][0-9]?)$");
                    if (matchForTimeBlack.Success == true)
                    {
                        int minutes;
                        int seconds;

                        minutes = Convert.ToInt32(matchForTimeBlack.Groups["minutes"].Value);
                        seconds = minutes * 60 + Convert.ToInt32(matchForTimeBlack.Groups["seconds"].Value);
                        Globals.OtherPlayerTimerTimeSeconds = seconds;
                        if(Globals.PrimePlayerTimerTimeSeconds != -2)
                        {
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid time!");
                    }
                }
                else
                {
                    if (Globals.PrimePlayerTimerTimeSeconds != -2)
                    {
                        this.Close();
                    }
                }
                
            }
            
            

        }
    }
}
