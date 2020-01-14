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
    /// Interaction logic for StartGameParameters.xaml
    /// </summary>
    public partial class StartGameParameters : Window
    {
        public StartGameParameters()
        {
            InitializeComponent();
           
            ParametersWindow.Icon = new BitmapImage(new Uri(Globals.pathToResources + "\\ChessIcon.png"));
            listBox.FontSize = 18;
            listBox.FontWeight = FontWeights.Bold;
            listBox.Items.Add("AI");
            listBox.Items.Add("Local play(both sides playable)");
        }
    }
}
