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
using System.IO;
using System.Threading;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Timers;
using System.Windows.Threading;
using System.Media;

namespace Project_ChessWithInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int BoardSize = 8;
        const string Empty = "00";
        const string Pawn = "p";
        const string Rook = "r";
        const string Knight = "n";
        const string Bishop = "b";
        const string Queen = "q";
        const string King = "k";
        const string Black = "B";
        const string White = "W";
        const int WhitePieces = 16;
        const int BlackPieces = 16;
        public MainWindow(int GameMode)
        {
            
            InitializeComponent();
            GameModel = GameMode;
            
            
            //GetConnectionStringForDatabase();

        }
        public static int GameModel = 0;
        
        

        public  void InitializeUI()// Initializes visual UI elements
        {
            GetAllButtonElements();
            SortButtons();
            Globals.FromBoardToPiecePathes = PopulateADictionary();
           
            GameWindow.Icon = new BitmapImage(new Uri(Globals.PathToResources + "\\ChessIcon.png"));
            Board.Source = new BitmapImage(new Uri(Globals.PathToResources + "\\Board.png"));
            SaveGameImage.Source = new BitmapImage(new Uri(Globals.PathToResources + "\\SaveGameIcon.png"));
            SaveGameImage.MouseDown += SaveGameImage_MouseDown;
            SuggestMove_img.MouseDown += SuggestMove_img_MouseDown;
            SuggestMove_img.Source = new BitmapImage(new Uri(Globals.PathToResources + "\\LightBulbIcon.png"));

            InitializeTimers();

            foreach (Button btn in Globals.AllButtons)
            {
                btn.Click += UniversalSquareClickEventHandle;
            }
        } 

        private void SuggestMove_img_MouseDown(object sender, MouseButtonEventArgs e)//Uses FindBestMoveAI function to suggest a move to the user in the current situation
        {
            string valueOfAIBefore = Globals.AI;
            if(Globals.WhitesTurn == true)
            {
                Globals.AI = "W";
            }
            else
            {
                Globals.AI = "B";
            }
            List<int> SuggestedMove = FindBestMoveAI(Globals.AI, Globals.Board);
            string initialPosition = ConvertAbsoluteToBoardNotation(SuggestedMove[0]);
            string finalPosition = ConvertAbsoluteToBoardNotation(SuggestedMove[1]);
            MessageBox.Show($"Suggested move:\n{initialPosition} => {finalPosition}");
            Globals.AI = valueOfAIBefore;

        }

        public static void InsertPlayedGameIntoDatabase(string Winner)//Uses SQL command INSERT in order to create a new record in the PlayedGames database with appropriate parameters
        {
            
            if (Winner != "Draw" && Winner != "AI")
            {

                string NickName = "";
                NameForDatabaseDialog nameDialog = new NameForDatabaseDialog(Winner);
                nameDialog.ShowDialog();
                NickName = nameDialog.NicknameChosen;
                Winner += $"({NickName})";
            }
            
            string connectionString = GetConnectionStringForDatabase();
            SqlConnection gameArchive = new SqlConnection();
            SqlCommand command = new SqlCommand();
            gameArchive.ConnectionString = connectionString;
            gameArchive.Open();
            command.Connection = gameArchive;

            command.CommandText = "SELECT TOP 1 Id FROM PlayedGames ORDER BY Id DESC";
            int lastId = 0;
            


            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {


                lastId = Convert.ToInt32(reader.GetValue(0));
            }
            
           
            
            int currentId = lastId + 1;
            reader.Close();
            string datePlayed = DateTime.Today.ToString("yyyy-MM-dd");
            command.CommandText = $"INSERT INTO PlayedGames VALUES({currentId},'{Winner}','{datePlayed}','{"\\ID" + currentId + ".txt"}',{Globals.MoveCounter - 1})";
            string referenceFolder = GameArchive.GetPathToReferenceFolder();
            string fullFilePath = referenceFolder + "\\ID" + currentId + ".txt";
            File.Create(fullFilePath).Close();
            File.WriteAllLines(fullFilePath, Globals.MoveRecord);
            command.ExecuteNonQuery();
            MessageBox.Show("Database entry successfuly inserted!");
            
        }
        public void InitializeTimers()//Initializes timers based on Global variables PrimePlayerTimerSeconds and OtherPlayerTimerSeconds which are set in TimerDialog window
        {
            PrimePlayerTimerLabelUpdate();
            OtherPlayerTimerLabelUpdate();
            if (Globals.AI != null)
            {
                TimerLabel.Content = "Human";
                TimerLabel2.Visibility = Visibility.Hidden;
            }
            if (Globals.AI == null)
            {
                if (Globals.OtherPlayerTimerTimeSeconds != -1)
                {


                    OtherPlayer_label.Visibility = Visibility.Visible;
                    Globals.OtherPlayerTimer.Interval = TimeSpan.FromSeconds(1);
                    Globals.OtherPlayerTimer.Tick += OtherPlayerTimer_Tick;
                    Globals.OtherPlayerTimer.Start();
                    if(Globals.WhitesTurn == true)
                    {
                        Globals.OtherPlayerTimer.IsEnabled = false;
                    }
                    else
                    {
                        Globals.OtherPlayerTimer.IsEnabled = true;
                    }
                    
                }
                else
                {
                    TimerLabel2.Visibility = Visibility.Hidden;
                }
            }
            if (Globals.PrimePlayerTimerTimeSeconds != -1)
            {

                PrimePlayerTimer_label.Visibility = Visibility.Visible;
                Globals.PrimePlayerTimer.Interval = TimeSpan.FromSeconds(1);
                Globals.PrimePlayerTimer.Tick += PrimePlayerTimer_Tick;
                Globals.PrimePlayerTimer.Start();
                if(Globals.AI == null && Globals.WhitesTurn == false)
                {
                    Globals.PrimePlayerTimer.IsEnabled = false;
                }
                else if(Globals.AI == null && Globals.WhitesTurn == true)
                {
                    Globals.PrimePlayerTimer.IsEnabled = true;
                }

            }
            else
            {
                TimerLabel.Visibility = Visibility.Hidden;
            }
            
            
        }

        private void OtherPlayerTimer_Tick(object sender, EventArgs e)//Decrements the other player’s remaining time
        {
            if (Globals.OtherPlayerTimerTimeSeconds == 0)
            {
                Globals.OtherPlayerTimer.Stop();
                MessageBox.Show("You have ran out of time!");
                SomeTimerTimedOut();
            }
            else
            {


                Globals.OtherPlayerTimerTimeSeconds--;
                OtherPlayerTimerLabelUpdate();
            }
        }

        public  void PrimePlayerTimerLabelUpdate()
        {
            var minutes = Globals.PrimePlayerTimerTimeSeconds / 60;
            string seconds = Convert.ToString(Globals.PrimePlayerTimerTimeSeconds - minutes * 60);
            
            if(Convert.ToInt32(seconds) < 10)
            {
               seconds = seconds.Insert(0, "0");
            }
            PrimePlayerTimer_label.Content = $"{minutes}:{seconds}";
        }
        public void OtherPlayerTimerLabelUpdate()
        {
            var minutes = Globals.OtherPlayerTimerTimeSeconds / 60;
            string seconds = Convert.ToString(Globals.OtherPlayerTimerTimeSeconds - minutes * 60);

            if (Convert.ToInt32(seconds) < 10)
            {
                seconds = seconds.Insert(0, "0");
            }
            OtherPlayer_label.Content = $"{minutes}:{seconds}";
        }
        private void PrimePlayerTimer_Tick(object sender, EventArgs e)//Decrements the prime player’s remaining time
        {
            if (Globals.PrimePlayerTimerTimeSeconds == 0)
            {
                Globals.PrimePlayerTimer.Stop();
                MessageBox.Show("You have ran out of time!");
                SomeTimerTimedOut();
            }
            else
            {


                Globals.PrimePlayerTimerTimeSeconds--;
                PrimePlayerTimerLabelUpdate();
            }
        }

        public static string GetConnectionStringForDatabase()//Returns a connection string for a database of played games
        {
            string pathToResources = Globals.PathToResources;
            string pathToMainFolder = Regex.Replace(pathToResources, @"\\[^\\]+$", "");
            string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;" + $"AttachDbFilename={pathToMainFolder+"\\PlayedGamesDB.mdf"};" + @"Integrated Security=True";
           
            return ConnectionString;
        }
        private void SaveGameImage_MouseDown(object sender, MouseButtonEventArgs e)//Displays the SaveGame dialog window which then does all processing to save the game.
        {
            SaveGameDialog save = new SaveGameDialog(GameModel);
            save.ShowDialog();
        }
        
        public static bool CanProceedWithFirstClick(int index) // A procedure that determines if first click was valid, for example if it's black's turn if white piece is clicked, then error message will be displayed
        {
            string CurrentPieceOnSquare = Globals.Board[index];
            if(CurrentPieceOnSquare == Empty)
            {
                MessageBox.Show("ERROR: Empty square selected");
                return false;
            }
            else
            {
                if(Globals.WhitesTurn == true && CurrentPieceOnSquare[0] == 'B')
                {
                    MessageBox.Show("ERROR: Opposite color piece selected");
                    return false;
                }
                else if(Globals.WhitesTurn == false && CurrentPieceOnSquare[0] == 'W')
                {
                    MessageBox.Show("ERROR: Opposite color piece selected");
                    return false;
                }
            }
            return true;
        }
       
        public void DisplayBoardOnInterface()
        {
            foreach(Button btn in Globals.AllButtons)
            {
                btn.Content = null;
                
            }
            for(int i = 0; i < Globals.Board.Count; i++)
            {
                if (Globals.Board[i] != Empty)
                {


                    var temp = "";
                    Globals.FromBoardToPiecePathes.TryGetValue(Globals.Board[i], out temp);
                    string pathToPiece = temp;
                    Globals.AllButtons[i].Content = new Image
                    {
                        Source = new BitmapImage(new Uri(Globals.PathToResources + pathToPiece)),
                        VerticalAlignment = VerticalAlignment.Center

                    };
                }
            }
            if(Globals.WhitesTurn == true)
            {
                TurnIndicator.Text = "White's Turn";
            }
            else
            {
                TurnIndicator.Text = "Black's Turn";
            }
            MoveRecordWhite.Document.Blocks.Clear();
            MoveRecordWhite.Document.Blocks.Add(new Paragraph(new Run(String.Join("\n", Globals.MoveRecord))));
        }
        public void SortButtons()// Uses bubble sort to sort elements of Globals.AllButtons list in order to allow easier indexing
        {
            List<Button> NamesOfButtons = new List<Button>();
            foreach(Button btn in Globals.AllButtons)
            {
                NamesOfButtons.Add(btn);
            }
            
            for(int i = 0; i < NamesOfButtons.Count; i++)
            {
                for(int r = 0; r < NamesOfButtons.Count - 1; r++)
                {
                    int Number = Convert.ToInt32(Regex.Replace(NamesOfButtons[r].Name, @"^btn", ""));
                    int Number1 = Convert.ToInt32(Regex.Replace(NamesOfButtons[r + 1].Name, @"^btn", ""));
                    if(Number > Number1)
                    {
                        var temp = NamesOfButtons[r];
                        NamesOfButtons[r] = NamesOfButtons[r + 1];
                        NamesOfButtons[r + 1] = temp;
                    }
                }
                
                
            }
            for(int i = 0; i < NamesOfButtons.Count; i++)
            {
                Globals.AllButtons[i] = NamesOfButtons[i];
            }
            
        }
        public static int CheckGameEndConditions(List<string> Board,bool whitesTurn) //Checks if any of the kings is in checkmate or if there is a draw; 0-Nothing; 1-White Checkmated; 2-Black Checkmated; 3-Stalemate
        {
            List<int> PositionsOfWhiteFigures = new List<int>();
            List<int> PositionsOfBlackFigures = new List<int>();
            List<int> OutList = new List<int>();


            for (int i = 0; i < Board.Count; i++)
            {
                char temp = Board[i][0];
                if (temp == 'W')
                {





                    PositionsOfWhiteFigures.Add(i);


                }
                else if (temp == 'B')
                {




                    PositionsOfBlackFigures.Add(i);


                }

            }
            if (whitesTurn == true)
            {
                bool AtLeastOneLegalMove = false;
                List<int> LegalMovesForScopedPiece = new List<int>();
                for (int i = 0; i < PositionsOfWhiteFigures.Count; i++)
                {
                    LegalMovesForScopedPiece = IndexesOfPossibleMoves(Board,Board[PositionsOfWhiteFigures[i]], PositionsOfWhiteFigures[i]);
                    if (LegalMovesForScopedPiece.Count > 0)
                    {
                        AtLeastOneLegalMove = true;
                        break;
                    }
                }
                if (AtLeastOneLegalMove == false)
                {
                    List<int> WhatPiecesCheckingKing = KingInCheckAndByWhichFigures(Board,whitesTurn);
                    if (WhatPiecesCheckingKing.Count == 0)
                    {
                        //DRAW
                        return 3;
                    }
                    else
                    {
                        //WHITE CHECKMATED
                        return 1;
                    }
                }
            }
            else
            {
                bool AtLeastOneLegalMove = false;
                List<int> LegalMovesForScopedPiece = new List<int>();
                for (int i = 0; i < PositionsOfBlackFigures.Count; i++)
                {
                    LegalMovesForScopedPiece = IndexesOfPossibleMoves(Board,Board[PositionsOfBlackFigures[i]], PositionsOfBlackFigures[i]);
                    if (LegalMovesForScopedPiece.Count > 0)
                    {
                        AtLeastOneLegalMove = true;
                        break;
                    }
                }
                if (AtLeastOneLegalMove == false)
                {
                    List<int> WhatPiecesCheckingKing = KingInCheckAndByWhichFigures(Board,whitesTurn);
                    if (WhatPiecesCheckingKing.Count == 0)
                    {
                        //DRAW
                        return 3;
                    }
                    else
                    {
                        //BLACK CHECKMATED
                        return 2;
                    }
                }
            }
            return 0;
        }
        public void DisableAllButtons() //Disables all button click events, called when the game has reached terminal state
        {
            foreach (Button btn in Globals.AllButtons)
            {
                btn.Click -= UniversalSquareClickEventHandle;
            }
        }
        private void UniversalSquareClickEventHandle(object sender, RoutedEventArgs e)//Controls the main game loop flow.Registers the first and second click indexes and does the checks to verify that the move is valid. Also calls AI function if the next turn is AI's
        {
            
            string nameOfButton = ((Button)sender).Name;
            string subStringForIndex = Regex.Replace(nameOfButton, @"^btn", "");
            int indexOfClickedSquare = Convert.ToInt32(subStringForIndex);
            if (Globals.WaitingForSecondClick == false)
            {


                bool Verified = CanProceedWithFirstClick(indexOfClickedSquare);
                if (Verified == true)
                {


                    int index = 0;
                    for (int i = 0; i < Globals.AllButtons.Count; i++)
                    {
                        string scopedName = Globals.AllButtons[i].Name;
                        if (scopedName == nameOfButton)
                        {
                            index = i;
                            break;
                        }
                    }
                    Globals.WaitingForSecondClick = true;
                    Globals.FirstClickIndex = indexOfClickedSquare;
                    


                }
            }
            else
            {
                bool verifiedForSecondClick = CanProceedWithSecondClick(Globals.FirstClickIndex, indexOfClickedSquare);
                if(verifiedForSecondClick == false)
                {
                    Globals.WaitingForSecondClick = false;
                    Globals.FirstClickIndex = -1;
                    
                }
                else
                {
                    MovePiece(ConvertAbsoluteToBoardNotation(Globals.FirstClickIndex), ConvertAbsoluteToBoardNotation(indexOfClickedSquare), Globals.MoveCounter);
                    
                    Globals.MoveCounter++;
                    if (Globals.WhitesTurn == true)
                    {
                        Globals.WhitesTurn = false;
                    }
                    else
                    {
                        Globals.WhitesTurn = true;
                    }
                    Globals.WaitingForSecondClick = false;
                    Globals.FirstClickIndex = -1;
                    
                    DisplayBoardOnInterface();
                    
                    Globals.PlacePieceSoundEffect.Open(new Uri(Globals.PathToResources + "\\PlacePieceSound.mp3"));
                    Globals.PlacePieceSoundEffect.Play();
                    if (Globals.WhitesTurn == false)
                    {
                        Globals.OtherPlayerTimer.IsEnabled = true;
                        Globals.PrimePlayerTimer.IsEnabled = false;
                    }
                    else
                    {
                        Globals.OtherPlayerTimer.IsEnabled = false;
                        Globals.PrimePlayerTimer.IsEnabled = true;
                    }
                    int StateOfGame = CheckGameEndConditions(Globals.Board,Globals.WhitesTurn); //0-Nothing; 1-White Checkmated; 2-Black Checkmated; 3-Stalemate
                    if (StateOfGame == 1)
                    {
                        TurnIndicator.Text = "White king chechmated\nBlack has won!";
                        DisableTimers();
                        if (Globals.AI == White)
                        {
                            
                            InsertPlayedGameIntoDatabase($"Human");

                        }
                        else if(Globals.AI == Black)
                        {
                            
                            InsertPlayedGameIntoDatabase($"AI");
                        }
                        else if(Globals.AI == null)
                        {
                            string color = "Black";
                            InsertPlayedGameIntoDatabase(color);
                        }
                        DisableAllButtons();
                    }
                    else if (StateOfGame == 2)
                    {
                        TurnIndicator.Text = "Black king checkmated\nWhite has won!";
                        DisableTimers();
                        if (Globals.AI == White)
                        {
                            
                            InsertPlayedGameIntoDatabase($"AI");
                        }
                        else if (Globals.AI == Black)
                        {
                            
                            InsertPlayedGameIntoDatabase($"Human");
                        }
                        else if (Globals.AI == null)
                        {
                            string color = "White";
                            InsertPlayedGameIntoDatabase(color);
                        }
                        DisableAllButtons();
                    }
                    else if(StateOfGame == 3)
                    {
                        TurnIndicator.Text = "Stalemate\nGame has been drawn!";
                        DisableTimers();
                        InsertPlayedGameIntoDatabase("Draw");
                        DisableAllButtons();
                    }
                    else if(StateOfGame == 0)
                    {
                        
                        if (Globals.WhitesTurn == true && Globals.AI == White)
                        {
                            DelayAction(500, new Action(() => { MakeAIMove(); }));
                            
                        }
                        else if(Globals.WhitesTurn == false && Globals.AI == Black)
                        {
                            DelayAction(500, new Action(() => { MakeAIMove(); }));
                            
                            
                        }
                    }

                    
                }
            }


           
            
        }
        public static void MakeAIMove()//Uses event raising to make an AI move on the board which is given by FindBestMoveAI() function
        {
            Globals.PrimePlayerTimer.IsEnabled = false;
            List<int> AIMove = FindBestMoveAI(Globals.AI, Globals.Board);
            RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
            Globals.AllButtons[AIMove[0]].RaiseEvent(newEventArgs);
            Globals.AllButtons[AIMove[1]].RaiseEvent(newEventArgs);
            Globals.PrimePlayerTimer.IsEnabled = true;
        }
        public static bool CanProceedWithSecondClick(int startIndex,int endIndex) //Returns true if the piece as startIndex is allowed to move to endIndex. Displays an error message otherwise
        {
            List<int> PossibleMovesOfSelectedPiece = IndexesOfPossibleMoves(Globals.Board,Globals.Board[startIndex], startIndex);
            if(PossibleMovesOfSelectedPiece.Contains(endIndex) == false)
            {
                MessageBox.Show("ERROR: Ilegal move");
                return false;
            }
            
            return true;
        }

        public  void GetAllButtonElements()//Used to initialize Global variable AllButtons
        {
            
            Panel mainContainer = (Panel)this.Content;

            
            UIElementCollection element = mainContainer.Children;

            
            List<FrameworkElement> lstElement = element.Cast<FrameworkElement>().ToList();

            
            var ListOfAllControls = lstElement.OfType<Control>();

            foreach (Control control in ListOfAllControls)
            {
                Type type = control.GetType();
                if(type.Name == "Button")
                {
                    Globals.AllButtons.Add(control as Button);
                    
                }
               
            }
        } 
        public void SomeTimerTimedOut()//Called when timer some timer timed out, and displays the results on the screen
        {
            if(Globals.AI != null)
            {
                string playerColor = "";
                
                string computerColor = "";
                switch (Globals.AI)
                {
                    case White:
                        playerColor = "Black";
                        computerColor = "White";
                        break;
                    case Black:
                        playerColor = "White";
                        computerColor = "Black";
                        break;
                }
                TurnIndicator.Text = $"{playerColor} has run out of time\n{computerColor} has won!";
                InsertPlayedGameIntoDatabase($"AI");
            }
            else
            {
                if(Globals.PrimePlayerTimerTimeSeconds == 0)
                {
                    TurnIndicator.Text = $"White has run out of time\nBlack has won!";
                    InsertPlayedGameIntoDatabase("Black");
                }
                else if(Globals.OtherPlayerTimerTimeSeconds == 0)
                {
                    TurnIndicator.Text = $"Black has run out of time\nWhite has won!";
                    InsertPlayedGameIntoDatabase("White");
                }
            }
            
            DisableAllButtons();
        }
        public static void DisableTimers()//Disables the timers, called when the game reaches the terminal stage
        {
            Globals.PrimePlayerTimer.Stop();
            Globals.OtherPlayerTimer.Stop();
        }
        
        public static void DelayAction(int millisecond, Action action) //Delays the execution of the code without freezing the UI
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate

            {
                action.Invoke();
                timer.Stop();
            };

            timer.Interval = TimeSpan.FromMilliseconds(millisecond);
            timer.Start();
        }


        public static void InitializeBoard()//If default game mode selected, Set up Globals.Board according to standard initial position in chess.
        {
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                Globals.Board.Add(Empty);
            }
            for (int i = 0; i < WhitePieces; i++)
            {
                if (i == 0 | i == 7)
                {
                    Globals.Board[i] = $"{White}{Rook}";

                }
                else if (i == 1 | i == 6)
                {
                    Globals.Board[i] = $"{White}{Knight}";
                }
                else if (i == 2 | i == 5)
                {
                    Globals.Board[i] = $"{White}{Bishop}";
                }
                else if (i == 3)
                {
                    Globals.Board[i] = $"{White}{Queen}";

                }
                else if (i == 4)
                {
                    Globals.Board[i] = $"{White}{King}";
                }
                else
                {
                    Globals.Board[i] = $"{White}{Pawn}";
                }


            }
            for (int i = Globals.Board.Count - BoardSize * 2; i < Globals.Board.Count; i++)
            {
                if (i == 56 | i == 63)
                {
                    Globals.Board[i] = $"{Black}{Rook}";

                }
                else if (i == 57 | i == 62)
                {
                    Globals.Board[i] = $"{Black}{Knight}";
                }
                else if (i == 58 | i == 61)
                {
                    Globals.Board[i] = $"{Black}{Bishop}";
                }
                else if (i == 59)
                {
                    Globals.Board[i] = $"{Black}{Queen}";

                }
                else if (i == 60)
                {
                    Globals.Board[i] = $"{Black}{King}";
                }
                else
                {
                    Globals.Board[i] = $"{Black}{Pawn}";
                }
            }
        } 
        public static List<int> IndexesOfPossibleMoves(List<string> Board,string piece, int position)//Parent function that calls IndexesOfPossibleMovesNoReccursion and then checks if this move will result in king being in check.
        {

            List<int> OutList = new List<int>();
            
            OutList = IndexesOfPossibleMovesNoReccursion(piece, position, Board);
            List<string> ScopedBoard = new List<string>();

            List<int> PiecesCheckingKing = new List<int>();
            List<int> PossibleMoves = new List<int>();
            foreach (int item in OutList)
            {
                PossibleMoves.Add(item);
            }
            
            for (int i = 0; i < PossibleMoves.Count; i++)
            {
                foreach (string item in Board)
                {
                    ScopedBoard.Add(item);
                }
                ScopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(position), ConvertAbsoluteToBoardNotation(PossibleMoves[i]), ScopedBoard);
                bool whitesTurn = true;
                if(piece[0] == 'B')
                {
                    whitesTurn = false;
                }
                PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard,whitesTurn);
                if (PiecesCheckingKing.Count == 0)
                {

                }
                else
                {
                    OutList.Remove(PossibleMoves[i]);
                }
                ScopedBoard.Clear();
            }


            return OutList;
        } 
        public static List<int> IndexesOfPossibleMovesNoReccursion(string piece, int position, List<string> ScopedBoard)//A method that calls GetIndexesOfPossibleMoves(piece)
        {
            int column = position % BoardSize;
            int row = position / BoardSize;
            List<int> OutList = new List<int>();
            char currentPiece = piece[1];
            bool whitesTurnn = false;
            if (piece[0] == 'W')
            {
                whitesTurnn = true;
            }
            List<string> Board = new List<string>();
            foreach (string item in ScopedBoard)
            {
                Board.Add(item);
            }
            if (currentPiece == Convert.ToChar(Pawn))
            {

                OutList = GetIndexesOfPossibleMovesPawn(column, row, whitesTurnn, Board);
            }
            else if (currentPiece == Convert.ToChar(Knight))
            {
                OutList = GetIndexesOfPossibleMovesKnight(column, row, whitesTurnn, Board);
            }
            else if (currentPiece == Convert.ToChar(Rook))
            {
                OutList = GetIndexesOfPossibleMovesRook(column, row, whitesTurnn, Board);
            }
            else if (currentPiece == Convert.ToChar(Bishop))
            {
                OutList = GetIndexesOfPossibleMovesBishop(column, row, whitesTurnn, Board);
            }
            else if (currentPiece == Convert.ToChar(Queen))
            {
                OutList = GetIndexesOfPossibleMovesQueen(column, row, whitesTurnn, Board);
            }
            else if (currentPiece == Convert.ToChar(King))
            {
                OutList = GetIndexesOfPossibleMovesKing(column, row, whitesTurnn, Board);
            }
            return OutList;
        } 
        public static List<int> GetIndexesOfPossibleMovesKnight(int column, int row, bool whitesTurnn, List<string> Board)//Returns indexes of possible moves of a knight on a certain position in a certain board
        {
            List<int> ScopedPotentialPositions = new List<int>();
            ScopedPotentialPositions.Add(GetAbolutePosition(column - 1, row + 2));
            ScopedPotentialPositions.Add(GetAbolutePosition(column + 1, row + 2));
            ScopedPotentialPositions.Add(GetAbolutePosition(column - 2, row + 1));
            ScopedPotentialPositions.Add(GetAbolutePosition(column - 2, row - 1));
            ScopedPotentialPositions.Add(GetAbolutePosition(column + 2, row + 1));
            ScopedPotentialPositions.Add(GetAbolutePosition(column + 2, row - 1));
            ScopedPotentialPositions.Add(GetAbolutePosition(column - 1, row - 2));
            ScopedPotentialPositions.Add(GetAbolutePosition(column + 1, row - 2));
            bool close = false;
            while (close == false)
            {

                bool removedElem = false;
                for (int i = 0; i < ScopedPotentialPositions.Count; i++)
                {
                    if (ScopedPotentialPositions[i] == -1)
                    {
                        ScopedPotentialPositions.RemoveAt(i);
                        removedElem = true;
                        break;
                    }
                    else
                    {
                        if (whitesTurnn == true)
                        {
                            if (Board[ScopedPotentialPositions[i]] == Empty)
                            {

                            }
                            else
                            {
                                string piece = Board[ScopedPotentialPositions[i]];
                                if (piece[0] == 'W')
                                {
                                    ScopedPotentialPositions.RemoveAt(i);
                                    removedElem = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (Board[ScopedPotentialPositions[i]] == Empty)
                            {

                            }
                            else
                            {
                                string piece = Board[ScopedPotentialPositions[i]];
                                if (piece[0] == 'B')
                                {
                                    ScopedPotentialPositions.RemoveAt(i);
                                    removedElem = true;
                                    break;
                                }
                            }
                        }
                    }

                }
                if (removedElem == false)
                {
                    close = true;
                }

            }
            return ScopedPotentialPositions;
        }
        public static List<int> GetIndexesOfPossibleMovesPawn(int column, int row, bool whitesTurnn, List<string> Board)//Returns indexes of possible moves of a pawn on a certain position in a certain board
        {
            List<int> OutList = new List<int>();
            string piece = Globals.Board[GetAbolutePosition(column, row)];
            if (whitesTurnn == true)
            {
                int absolutePosNext = 0;
                absolutePosNext = GetAbolutePosition(column, row + 1);
                if (Board[absolutePosNext] == Empty)
                {
                    OutList.Add(absolutePosNext);
                    absolutePosNext = GetAbolutePosition(column, row + 2);
                    if (piece == White + Pawn && row == 1 && Board[absolutePosNext] == Empty)
                    {
                        OutList.Add(absolutePosNext);
                    }
                }


                if (column != 0)
                {
                    absolutePosNext = GetAbolutePosition(column - 1, row + 1);
                    if (Regex.IsMatch(Board[absolutePosNext], @"^B") == true)
                    {
                        OutList.Add(absolutePosNext);
                    }
                }
                if (column != 7)
                {
                    absolutePosNext = GetAbolutePosition(column + 1, row + 1);
                    if (Regex.IsMatch(Board[absolutePosNext], @"^B") == true)
                    {
                        OutList.Add(absolutePosNext);
                    }
                }
            }
            else
            {
                int absolutePosNext = 0;
                absolutePosNext = GetAbolutePosition(column, row - 1);
                if (Board[absolutePosNext] == Empty)
                {
                    OutList.Add(absolutePosNext);
                    absolutePosNext = GetAbolutePosition(column, row - 2);
                    if (piece == Black + Pawn && row == 6 && Board[absolutePosNext] == Empty)
                    {
                        OutList.Add(absolutePosNext);
                    }
                }


                if (column != 0)
                {
                    absolutePosNext = GetAbolutePosition(column - 1, row - 1);
                    if (Regex.IsMatch(Board[absolutePosNext], @"^W") == true)
                    {
                        OutList.Add(absolutePosNext);
                    }
                }
                if (column != 7)
                {
                    absolutePosNext = GetAbolutePosition(column + 1, row - 1);
                    if (Regex.IsMatch(Board[absolutePosNext], @"^W") == true)
                    {
                        OutList.Add(absolutePosNext);
                    }
                }
            }
            
            string lastMove = "";
            if (Globals.MoveRecord.Count != 0)
            {

                lastMove = Globals.MoveRecord.Last();
                if (whitesTurnn == true)
                {
                    Match matchForDestination = Regex.Match(lastMove, @"^\d+: Bp.7 => (?<capture>.5)$");
                    if (matchForDestination.Success == true)
                    {
                        lastMove = matchForDestination.Groups["capture"].Value;
                        int AbsolutePostionOfEnPessant = ChessNotationToAbsolute(lastMove);
                        int AbsolutePostionOfPiece = GetAbolutePosition(column, row);
                        if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece + 1)
                        {
                            OutList.Add(GetAbolutePosition(column + 1, row + 1));
                            
                        }
                        else if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece - 1)
                        {
                            OutList.Add(GetAbolutePosition(column - 1, row + 1));
                            
                        }

                    }

                }
                else
                {
                    Match matchForDestination = Regex.Match(lastMove, @"^\d+: Wp.2 => (?<capture>.4)$");
                    if (matchForDestination.Success == true)
                    {
                        lastMove = matchForDestination.Groups["capture"].Value;
                        int AbsolutePostionOfEnPessant = ChessNotationToAbsolute(lastMove);
                        int AbsolutePostionOfPiece = GetAbolutePosition(column, row);
                        if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece + 1)
                        {
                            OutList.Add(GetAbolutePosition(column + 1, row - 1));
                            
                        }
                        else if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece - 1)
                        {
                            OutList.Add(GetAbolutePosition(column - 1, row - 1));
                            
                        }

                    }
                }
            }
            
            for(int i = 0; i < OutList.Count; i++)
            {
                if (OutList[i] == -1)
                {
                    OutList.RemoveAt(i);
                }
            }
            return OutList;
        }
        public static List<int> GetIndexesOfPossibleMovesRook(int column, int row, bool whitesTurnn, List<string> Board)//Returns indexes of possible moves of a rook on a certain position in a certain board
        {
            List<int> OutList = new List<int>();


            for (int i = row + 1; i <= BoardSize - 1; i++)
            {
                int absolutePos = GetAbolutePosition(column, i);
                if (Board[absolutePos] == Empty)
                {
                    OutList.Add(absolutePos);
                }
                else
                {
                    if (whitesTurnn == true)
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^B") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^W") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            for (int i = row - 1; i >= 0; i--)
            {
                int absolutePos = GetAbolutePosition(column, i);
                if (Board[absolutePos] == Empty)
                {
                    OutList.Add(absolutePos);
                }
                else
                {
                    if (whitesTurnn == true)
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^B") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^W") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            for (int i = column + 1; i <= BoardSize - 1; i++)
            {
                int absolutePos = GetAbolutePosition(i, row);
                if (Board[absolutePos] == Empty)
                {
                    OutList.Add(absolutePos);
                }
                else
                {
                    if (whitesTurnn == true)
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^B") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^W") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            for (int i = column - 1; i >= 0; i--)
            {
                int absolutePos = GetAbolutePosition(i, row);
                if (Board[absolutePos] == Empty)
                {
                    OutList.Add(absolutePos);
                }
                else
                {
                    if (whitesTurnn == true)
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^B") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^W") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return OutList;
        }
        public static List<int> GetIndexesOfPossibleMovesBishop(int column, int row, bool whitesTurnn, List<string> Board)//Returns indexes of possible moves of a bishop on a certain position in a certain board
        {
            List<int> OutList = new List<int>();
            int scopedRow = row;
            for (int i = column + 1; i < BoardSize; i++)
            {
                scopedRow++;

                int absolutePos = GetAbolutePosition(i, scopedRow);
                if (absolutePos == -1)
                {
                    break;
                }
                if (Board[absolutePos] == Empty)
                {
                    OutList.Add(absolutePos);
                }
                else
                {
                    if (whitesTurnn == true)
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^B") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^W") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            scopedRow = row;
            for (int i = column + 1; i < BoardSize; i++)
            {
                scopedRow--;
                int absolutePos = GetAbolutePosition(i, scopedRow);
                if (absolutePos == -1)
                {
                    break;
                }
                if (Board[absolutePos] == Empty)
                {
                    OutList.Add(absolutePos);
                }
                else
                {
                    if (whitesTurnn == true)
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^B") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^W") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            scopedRow = row;
            for (int i = column - 1; i >= 0; i--)
            {
                scopedRow++;
                int absolutePos = GetAbolutePosition(i, scopedRow);
                if (absolutePos == -1)
                {
                    break;
                }
                if (Board[absolutePos] == Empty)
                {
                    OutList.Add(absolutePos);
                }
                else
                {
                    if (whitesTurnn == true)
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^B") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^W") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            scopedRow = row;
            for (int i = column - 1; i >= 0; i--)
            {
                scopedRow--;
                int absolutePos = GetAbolutePosition(i, scopedRow);
                if (absolutePos == -1)
                {
                    break;
                }
                if (Board[absolutePos] == Empty)
                {
                    OutList.Add(absolutePos);
                }
                else
                {
                    if (whitesTurnn == true)
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^B") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(Board[absolutePos], @"^W") == true)
                        {
                            OutList.Add(absolutePos);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return OutList;
        }
        public static string ConvertAbsoluteToBoardNotation(int index) //Converts absolute List position to a human readable format
        {
            string ForOut = "";
            int column = index % BoardSize;
            int row = index / BoardSize;
            Dictionary<int, string> ColumnTable = new Dictionary<int, string>
            {
                { 0,"A" },
                {1, "B" },
                {2,"C" },
                {3,"D" },
                {4,"E" },
                {5,"F" },
                {6,"G" },
                {7,"H" }

            };
            string temp = "";
            ColumnTable.TryGetValue(column, out temp);
            ForOut += $"{temp}{row + 1}";
            return ForOut;

        }
        public static List<int> GetIndexesOfPossibleMovesQueen(int column, int row, bool whitesTurnn, List<string> Board)//Returns indexes of possible moves of a queen on a certain position in a certain board
        {
            List<int> ForOut = new List<int>();
            List<int> RookMoves = GetIndexesOfPossibleMovesRook(column, row, whitesTurnn, Board);
            List<int> BishopMoves = GetIndexesOfPossibleMovesBishop(column, row, whitesTurnn, Board);
            foreach (int item in RookMoves)
            {
                ForOut.Add(item);
            }
            foreach (int item in BishopMoves)
            {
                ForOut.Add(item);
            }
            return ForOut;
        }
        public static int GetAbolutePosition(int column, int row)//Transforms column,row notation of position to an absolute position on the Board
        {
            if (column < 0 || column > 7 || row < 0 || row > 7)
            {
                return -1;
            }
            int forOut = 0;
            forOut += column;
            while (row != 0)
            {
                forOut += 8;
                row--;
            }
            return forOut;
        } 
        public static int ChessNotationToAbsolute(string n)// Transforms human readable position to absolute position
        {
            Dictionary<string, int> ColumnTable = new Dictionary<string, int>
            {
                { "A",0 },
                {"B",1 },
                {"C",2 },
                {"D",3 },
                {"E",4 },
                {"F",5 },
                {"G",6 },
                {"H",7 }

            };
            string temp = Convert.ToString(n[0]);
            int temp2 = 0;
            ColumnTable.TryGetValue(temp, out temp2);
            int column = temp2;
            temp = Convert.ToString(n[1]);
            int row = Convert.ToInt32(temp) - 1;
            return GetAbolutePosition(column, row);


        }
        public static void MovePiece(string initialPos, string destination, int counter)
        {
            string piece = "";
            int AbsoluteInitialPosition = ChessNotationToAbsolute(initialPos);
            int AbsoluteFinalPosition = ChessNotationToAbsolute(destination);
            piece = Globals.Board[AbsoluteInitialPosition];
            List<int> IndexesOfPossibleMovess = IndexesOfPossibleMoves(Globals.Board,piece, AbsoluteInitialPosition);
            string lastMove = "";
            int column = AbsoluteInitialPosition % BoardSize;
            int row = AbsoluteInitialPosition / BoardSize;
            int EnPessantDestination = -1;
            int CapturedInEnPessant = -1;


            if (Globals.MoveRecord.Count != 0)
            {
                if (piece[1] == 'p')
                {


                    lastMove = Globals.MoveRecord.Last();
                    if (Globals.WhitesTurn == true)
                    {
                        Match matchForDestination = Regex.Match(lastMove, @"^\d+: Bp.7 => (?<capture>.5)$");
                        
                        if (matchForDestination.Success == true)
                        {
                            lastMove = matchForDestination.Groups["capture"].Value;
                            int AbsolutePostionOfEnPessant = ChessNotationToAbsolute(lastMove);
                            int AbsolutePostionOfPiece = GetAbolutePosition(column, row);
                            if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece + 1)
                            {

                                EnPessantDestination = GetAbolutePosition(column + 1, row + 1);
                                CapturedInEnPessant = AbsolutePostionOfEnPessant;
                                
                            }
                            else if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece - 1)
                            {

                                CapturedInEnPessant = AbsolutePostionOfEnPessant;
                                EnPessantDestination = GetAbolutePosition(column - 1, row + 1);
                               
                            }

                        }

                    }
                    else
                    {
                        Match matchForDestination = Regex.Match(lastMove, @"^\d+: Wp.2 => (?<capture>.4)$");
                        if (matchForDestination.Success == true)
                        {
                            lastMove = matchForDestination.Groups["capture"].Value;
                            int AbsolutePostionOfEnPessant = ChessNotationToAbsolute(lastMove);
                            int AbsolutePostionOfPiece = GetAbolutePosition(column, row);
                            if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece + 1)
                            {
                               
                                EnPessantDestination = GetAbolutePosition(column + 1, row - 1);
                                CapturedInEnPessant = AbsolutePostionOfEnPessant;
                                
                            }
                            else if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece - 1)
                            {
                                
                                EnPessantDestination = GetAbolutePosition(column - 1, row - 1);
                                CapturedInEnPessant = AbsolutePostionOfEnPessant;
                                
                            }

                        }
                    }
                }
            }
            if (IndexesOfPossibleMovess.Contains(AbsoluteFinalPosition) == true)
            {
                
                bool kingCastling = false;

                if (initialPos == "E1")
                {
                    if (Globals.WhiteKingMoved == false)
                    {


                        if (destination == "G1")
                        {
                            Globals.Board[ChessNotationToAbsolute("E1")] = Empty;
                            Globals.Board[ChessNotationToAbsolute("G1")] = White + King;
                            Globals.Board[ChessNotationToAbsolute("H1")] = Empty;
                            Globals.Board[ChessNotationToAbsolute("F1")] = White + Rook;
                            Globals.WhiteKingMoved = true;
                            Globals.MoveRecord.Add($"{counter}: {piece}{initialPos} => O-O");
                            kingCastling = true;

                        }
                        if (destination == "C1")
                        {
                            Globals.Board[ChessNotationToAbsolute("E1")] = Empty;
                            Globals.Board[ChessNotationToAbsolute("C1")] = White + King;
                            Globals.Board[ChessNotationToAbsolute("A1")] = Empty;
                            Globals.Board[ChessNotationToAbsolute("D1")] = White + Rook;
                            Globals.WhiteKingMoved = true;
                            Globals.MoveRecord.Add($"{counter}: {piece}{initialPos} => O-O-O");
                            kingCastling = true;
                        }
                    }
                }


                if (initialPos == "E8")
                {
                    if (Globals.BlackKingMoved == false)
                    {
                        if (destination == "G8")
                        {



                            Globals.Board[ChessNotationToAbsolute("E8")] = Empty;
                            Globals.Board[ChessNotationToAbsolute("G8")] = Black + King;
                            Globals.Board[ChessNotationToAbsolute("H8")] = Empty;
                            Globals.Board[ChessNotationToAbsolute("F8")] = Black + Rook;
                            Globals.BlackKingMoved = true;
                            Globals.MoveRecord.Add($"{counter}: {piece}{initialPos} => O-O");
                            kingCastling = true;

                        }
                        if (destination == "C8")
                        {
                            Globals.Board[ChessNotationToAbsolute("E8")] = Empty;
                            Globals.Board[ChessNotationToAbsolute("C8")] = Black + King;
                            Globals.Board[ChessNotationToAbsolute("A8")] = Empty;
                            Globals.Board[ChessNotationToAbsolute("D8")] = Black + Rook;
                            Globals.MoveRecord.Add($"{counter}: {piece}{initialPos} => O-O-O");
                            Globals.BlackKingMoved = true;
                            kingCastling = true;
                        }
                    }
                }

                if (kingCastling == false)
                {
                    if (ConvertAbsoluteToBoardNotation(EnPessantDestination) == destination && piece[1] == 'p')
                    {
                        Globals.Board[AbsoluteInitialPosition] = Empty;
                        Globals.Board[AbsoluteFinalPosition] = piece;
                        Globals.Board[CapturedInEnPessant] = Empty;
                        Globals.MoveRecord.Add($"{counter}: {piece}{initialPos} => {destination}(En Pessant {ConvertAbsoluteToBoardNotation(CapturedInEnPessant)})");
                        
                    }
                    else
                    {



                        if (Globals.Board[AbsoluteFinalPosition] == Empty)
                        {
                            Globals.Board[AbsoluteFinalPosition] = piece;
                            Globals.Board[AbsoluteInitialPosition] = Empty;
                            Globals.MoveRecord.Add($"{counter}: {piece}{initialPos} => {destination}");
                            if (piece == White + King)
                            {
                                Globals.WhiteKingMoved = true;

                            }
                            else if (piece == Black + King)
                            {
                                Globals.BlackKingMoved = true;
                            }
                            if (piece[1] == 'p')
                            {
                                if (AbsoluteFinalPosition >= 56 || AbsoluteFinalPosition <= 7)

                                {
                                    string color = Convert.ToString(piece[0]);
                                    Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { AbsoluteFinalPosition, -1 };
                                    if(color == Globals.AI)
                                    {
                                        Globals.PositionOfPawnToBePromotedAndPiece[1] = 0;
                                    }
                                    else
                                    {
                                        PawnPromotion promotion = new PawnPromotion();
                                        promotion.ShowDialog();
                                    }
                                    
                                    string PromotePieceTo = "";
                                    switch (Globals.PositionOfPawnToBePromotedAndPiece[1])
                                    {
                                        case 0:
                                            PromotePieceTo = Queen;
                                            break;
                                        case 1:
                                            PromotePieceTo = Rook;
                                            break;
                                        case 2:
                                            PromotePieceTo = Bishop;
                                            break;
                                        case 3:
                                            PromotePieceTo = Knight;
                                            break;

                                    }
                                    PromoteAPawn(AbsoluteFinalPosition, PromotePieceTo);
                                    string PromotedTo = PromotePieceTo;
                                    Globals.MoveRecord[Globals.MoveRecord.Count - 1] += $"(Promoted to {PromotedTo.ToUpper()})";

                                }
                            }
                        }
                        else
                        {
                            string temp = Globals.Board[AbsoluteFinalPosition];
                            Globals.Board[AbsoluteFinalPosition] = piece;
                            Globals.Board[AbsoluteInitialPosition] = Empty;
                            Globals.MoveRecord.Add($"{counter}: {piece}{initialPos} => {destination}({piece} x {temp})");
                            if (piece == White + King)
                            {
                                Globals.WhiteKingMoved = true;

                            }
                            else if (piece == Black + King)
                            {
                                Globals.BlackKingMoved = true;
                            }
                            if (piece[1] == 'p')
                            {
                                if (AbsoluteFinalPosition >= 56 || AbsoluteFinalPosition <= 7)

                                {
                                    string color = Convert.ToString(piece[0]);
                                    Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { AbsoluteFinalPosition, -1 };
                                    if (color == Globals.AI)
                                    {
                                        Globals.PositionOfPawnToBePromotedAndPiece[1] = 0;
                                    }
                                    else
                                    {
                                        PawnPromotion promotion = new PawnPromotion();
                                        promotion.ShowDialog();
                                    }
                                    string PromotePieceTo = "";
                                    switch (Globals.PositionOfPawnToBePromotedAndPiece[1])
                                    {
                                        case 0:
                                            PromotePieceTo = Queen;
                                            break;
                                        case 1:
                                            PromotePieceTo = Rook;
                                            break;
                                        case 2:
                                            PromotePieceTo = Bishop;
                                            break;
                                        case 3:
                                            PromotePieceTo = Knight;
                                            break;

                                    }
                                    PromoteAPawn(AbsoluteFinalPosition, PromotePieceTo);
                                    string PromotedTo = PromotePieceTo;
                                    Globals.MoveRecord[Globals.MoveRecord.Count - 1] += $"(Promoted to {PromotedTo.ToUpper()})";

                                }
                            }
                        }
                    }
                }
            }



        }
        public static void PromoteAPawn(int position,string ToWhat)
        {
            if(Globals.WhitesTurn == true)
            {
                Globals.Board[position] = White + ToWhat;
            }
            else
            {
                Globals.Board[position] = Black + ToWhat;
            }
        }
        public static List<string> MovePieceLocal(string initialPos, string destination, List<string> arr) //Return a list that represents a board on which a certain move has been made 
        {
            List<string> ScopedBoard = new List<string>();
            foreach (string item in arr)
            {
                ScopedBoard.Add(item);
            }
            string piece = "";
            int AbsoluteInitialPosition = ChessNotationToAbsolute(initialPos);
            int AbsoluteFinalPosition = ChessNotationToAbsolute(destination);
            piece = ScopedBoard[AbsoluteInitialPosition];
            




            if (ScopedBoard[AbsoluteFinalPosition] == Empty)
            {
                



                    if (initialPos == "E1")
                    {
                        if (piece[1] == Convert.ToChar(King))
                        {



                            if (destination == "G1")
                            {
                                ScopedBoard[ChessNotationToAbsolute("E1")] = Empty;
                                ScopedBoard[ChessNotationToAbsolute("G1")] = White + King;
                                ScopedBoard[ChessNotationToAbsolute("H1")] = Empty;
                                ScopedBoard[ChessNotationToAbsolute("F1")] = White + Rook;




                            }
                            if (destination == "C1")
                            {
                                ScopedBoard[ChessNotationToAbsolute("E1")] = Empty;
                                ScopedBoard[ChessNotationToAbsolute("C1")] = White + King;
                                ScopedBoard[ChessNotationToAbsolute("A1")] = Empty;
                                ScopedBoard[ChessNotationToAbsolute("D1")] = White + Rook;



                            }
                        }

                    }


                    if (initialPos == "E8")
                    {
                        if (piece[1] == Convert.ToChar(King))
                        {
                            if (destination == "G8")
                            {



                                ScopedBoard[ChessNotationToAbsolute("E8")] = Empty;
                                ScopedBoard[ChessNotationToAbsolute("G8")] = Black + King;
                                ScopedBoard[ChessNotationToAbsolute("H8")] = Empty;
                                ScopedBoard[ChessNotationToAbsolute("F8")] = Black + Rook;




                            }
                            if (destination == "C8")
                            {
                                ScopedBoard[ChessNotationToAbsolute("E8")] = Empty;
                                ScopedBoard[ChessNotationToAbsolute("C8")] = Black + King;
                                ScopedBoard[ChessNotationToAbsolute("A8")] = Empty;
                                ScopedBoard[ChessNotationToAbsolute("D8")] = Black + Rook;



                            }
                        }
                    }
                    if (piece == White + Pawn)
                    {
                        if (AbsoluteInitialPosition <= 55 && AbsoluteInitialPosition >= 48 && AbsoluteFinalPosition > 55)
                        {
                            ScopedBoard[AbsoluteInitialPosition] = Empty;
                            ScopedBoard[AbsoluteFinalPosition] = White + Queen;
                        }
                        else
                        {
                            ScopedBoard[AbsoluteFinalPosition] = piece;
                            ScopedBoard[AbsoluteInitialPosition] = Empty;
                        }
                    }
                    else if (piece == Black + Pawn)
                    {
                        if (AbsoluteInitialPosition >= 8 && AbsoluteInitialPosition <= 15 && AbsoluteFinalPosition < 8)
                        {
                            ScopedBoard[AbsoluteInitialPosition] = Empty;
                            ScopedBoard[AbsoluteFinalPosition] = Black + Queen;
                        }
                        else
                        {
                            ScopedBoard[AbsoluteFinalPosition] = piece;
                            ScopedBoard[AbsoluteInitialPosition] = Empty;
                        }
                    }
                    else
                    {


                        ScopedBoard[AbsoluteFinalPosition] = piece;
                        ScopedBoard[AbsoluteInitialPosition] = Empty;
                    }
                
            }
            else
            {
                if (piece == White + Pawn)
                {
                    if (AbsoluteInitialPosition <= 55 && AbsoluteInitialPosition >= 48 && AbsoluteFinalPosition > 55)
                    {
                        ScopedBoard[AbsoluteInitialPosition] = Empty;
                        ScopedBoard[AbsoluteFinalPosition] = White + Queen;
                    }
                    else
                    {
                        ScopedBoard[AbsoluteFinalPosition] = piece;
                        ScopedBoard[AbsoluteInitialPosition] = Empty;
                    }
                }
                else if (piece == Black + Pawn)
                {
                    if (AbsoluteInitialPosition >= 8 && AbsoluteInitialPosition <= 15 && AbsoluteFinalPosition < 8)
                    {
                        ScopedBoard[AbsoluteInitialPosition] = Empty;
                        ScopedBoard[AbsoluteFinalPosition] = Black + Queen;
                    }
                    else
                    {
                        ScopedBoard[AbsoluteFinalPosition] = piece;
                        ScopedBoard[AbsoluteInitialPosition] = Empty;
                    }
                }
                else
                {


                    ScopedBoard[AbsoluteFinalPosition] = piece;
                    ScopedBoard[AbsoluteInitialPosition] = Empty;
                }
            }
           

            return ScopedBoard;



        }

        public static List<int> GetIndexesOfPossibleMovesKing(int column, int row, bool whitesTurnn, List<string> board)//Returns indexes of possible moves of a king on a certain position in a certain board
        {
            bool close = false;
            List<int> ForOut = new List<int>();
            ForOut.Add(GetAbolutePosition(column, row + 1));
            ForOut.Add(GetAbolutePosition(column, row - 1));
            ForOut.Add(GetAbolutePosition(column - 1, row));
            ForOut.Add(GetAbolutePosition(column + 1, row));
            ForOut.Add(GetAbolutePosition(column + 1, row + 1));
            ForOut.Add(GetAbolutePosition(column + 1, row - 1));
            ForOut.Add(GetAbolutePosition(column - 1, row + 1));
            ForOut.Add(GetAbolutePosition(column - 1, row - 1));
            while (close == false)
            {
                bool removed = false;
                for (int i = 0; i < ForOut.Count; i++)
                {
                    if (ForOut[i] == -1)
                    {
                        ForOut.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
                if (removed == false)
                {


                    close = true;
                }
            }
            close = false;
            while (close == false)
            {
                bool removed = false;
                for (int i = 0; i < ForOut.Count; i++)
                {
                    if (board[ForOut[i]] == Empty)
                    {

                    }
                    else
                    {
                        if (whitesTurnn == true)
                        {
                            if (board[ForOut[i]][0] == Convert.ToChar(White))
                            {
                                ForOut.RemoveAt(i);
                                removed = true;
                                break;
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            if (board[ForOut[i]][0] == Convert.ToChar(Black))
                            {
                                ForOut.RemoveAt(i);
                                removed = true;
                                break;
                            }
                            else
                            {

                            }
                        }
                    }

                }
                if (removed == false)
                {


                    close = true;
                }
            }
            if (GameModel == 0)
            {


                List<bool> RooksMoved = DetermineIfRooksMovedInOrder();
                if (whitesTurnn == true)
                {
                    if (Globals.WhiteKingMoved == false)
                    {
                        if (board[5] == Empty && board[6] == Empty && RooksMoved[1] == false && board[7] == White + Rook)
                        {
                            List<int> PiecesCheckingKing = new List<int>();
                            List<string> ScopedBoard = new List<string>();
                            foreach (string item in board)
                            {
                                ScopedBoard.Add(item);
                            }
                            PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                            if (PiecesCheckingKing.Count == 0)
                            {


                                ScopedBoard = MovePieceLocal("E1", "F1", ScopedBoard);
                                PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                if (PiecesCheckingKing.Count == 0)
                                {
                                    ScopedBoard = MovePieceLocal("F1", "G1", ScopedBoard);
                                    PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                    if (PiecesCheckingKing.Count == 0)
                                    {

                                        ForOut.Add(6);
                                    }
                                }
                            }

                        }

                        if (board[1] == Empty && board[2] == Empty && board[3] == Empty && RooksMoved[0] == false && board[0] == White + Rook)
                        {
                            List<int> PiecesCheckingKing = new List<int>();
                            List<string> ScopedBoard = new List<string>();
                            foreach (string item in board)
                            {
                                ScopedBoard.Add(item);
                            }
                            PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                            if (PiecesCheckingKing.Count == 0)
                            {
                                ScopedBoard = MovePieceLocal("E1", "D1", ScopedBoard);
                                PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                if (PiecesCheckingKing.Count == 0)
                                {
                                    ScopedBoard = MovePieceLocal("D1", "C1", ScopedBoard);
                                    PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                    if (PiecesCheckingKing.Count == 0)
                                    {
                                        ScopedBoard = MovePieceLocal("C1", "B1", ScopedBoard);
                                        PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                        if (PiecesCheckingKing.Count == 0)
                                        {
                                            ForOut.Add(2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Globals.BlackKingMoved == false)
                    {
                        if (board[61] == Empty && board[62] == Empty && RooksMoved[3] == false && board[63] == Black + Rook)
                        {
                            List<int> PiecesCheckingKing = new List<int>();
                            List<string> ScopedBoard = new List<string>();
                            foreach (string item in board)
                            {
                                ScopedBoard.Add(item);
                            }
                            PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                            if (PiecesCheckingKing.Count == 0)
                            {
                                ScopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(GetAbolutePosition(column, row)), "F8", ScopedBoard);
                                PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                if (PiecesCheckingKing.Count == 0)
                                {
                                    ScopedBoard = MovePieceLocal("F8", "G8", ScopedBoard);
                                    PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                    if (PiecesCheckingKing.Count == 0)
                                    {
                                        ForOut.Add(62);
                                    }
                                }
                            }

                        }
                        if (board[57] == Empty && board[58] == Empty && board[59] == Empty && RooksMoved[2] == false && board[56] == Black + Rook)
                        {
                            List<int> PiecesCheckingKing = new List<int>();
                            List<string> ScopedBoard = new List<string>();
                            foreach (string item in board)
                            {
                                ScopedBoard.Add(item);
                            }
                            PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                            if (PiecesCheckingKing.Count == 0)
                            {
                                ScopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(GetAbolutePosition(column, row)), "D8", ScopedBoard);
                                PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                if (PiecesCheckingKing.Count == 0)
                                {
                                    ScopedBoard = MovePieceLocal("D8", "C8", ScopedBoard);
                                    PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                    if (PiecesCheckingKing.Count == 0)
                                    {
                                        ScopedBoard = MovePieceLocal("C8", "B8", ScopedBoard);
                                        PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard, whitesTurnn);
                                        if (PiecesCheckingKing.Count == 0)
                                        {
                                            ForOut.Add(58);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            int whiteKing = board.FindIndex(x => x == White + King);
            int blackKing = board.FindIndex(x => x == Black + King);
            int columnOppKing;
            int rowOppKing;
            if (whitesTurnn == true)
            {
                columnOppKing = blackKing % BoardSize;
                rowOppKing = blackKing / BoardSize;
            }
            else
            {
                columnOppKing = whiteKing % BoardSize;
                rowOppKing = whiteKing / BoardSize;
            }
            

            List<int> ScopeOfOppositeKing = new List<int>();
            ScopeOfOppositeKing.Add(GetAbolutePosition(columnOppKing, rowOppKing + 1));
            ScopeOfOppositeKing.Add(GetAbolutePosition(columnOppKing, rowOppKing - 1));
            ScopeOfOppositeKing.Add(GetAbolutePosition(columnOppKing - 1, rowOppKing));
            ScopeOfOppositeKing.Add(GetAbolutePosition(columnOppKing + 1, rowOppKing));
            ScopeOfOppositeKing.Add(GetAbolutePosition(columnOppKing + 1, rowOppKing + 1));
            ScopeOfOppositeKing.Add(GetAbolutePosition(columnOppKing + 1, rowOppKing - 1));
            ScopeOfOppositeKing.Add(GetAbolutePosition(columnOppKing - 1, rowOppKing + 1));
            ScopeOfOppositeKing.Add(GetAbolutePosition(columnOppKing - 1, rowOppKing - 1));
            
            close = false;
            while(close == false)
            {
                bool elementRemoved = false;
                for(int i = 0; i < ForOut.Count; i++)
                {
                    for(int q = 0; q < ScopeOfOppositeKing.Count; q++)
                    {
                        if(ForOut[i] == ScopeOfOppositeKing[q])
                        {
                            ForOut.RemoveAt(i);
                            elementRemoved = true;
                            break;
                        }
                    }
                    
                }
                if (elementRemoved == false)
                {
                    close = true;
                }
            }
            

            return ForOut;
        }

        public static List<int> KingInCheckAndByWhichFigures(List<string> Board,bool whitesTurn)//Returns a list of indexes of pieces that are checking the king specified by boolean whitesTurnn
        {
            List<int> PositionsOfWhiteFigures = new List<int>();
            List<int> PositionsOfBlackFigures = new List<int>();
            List<int> OutList = new List<int>();
            int positionOfWhiteKing = 0;
            int positionOfBlackKing = 0;

            for (int i = 0; i < Board.Count; i++)
            {
                char temp = Board[i][0];
                if (temp == 'W')
                {
                    if (Board[i] == White + King)
                    {
                        positionOfWhiteKing = i;

                    }
                    else
                    {


                        PositionsOfWhiteFigures.Add(i);
                    }

                }
                else if (temp == 'B')
                {
                    if (Board[i] == Black + King)
                    {
                        positionOfBlackKing = i;

                    }
                    else
                    {


                        PositionsOfBlackFigures.Add(i);
                    }

                }
            }
            
            if (whitesTurn == true)
            {
                for (int i = 0; i < PositionsOfBlackFigures.Count; i++)
                {
                    List<int> IndexesOfLegalMoves = new List<int>();
                    string piece = Board[PositionsOfBlackFigures[i]];
                    IndexesOfLegalMoves = IndexesOfPossibleMovesNoReccursion(piece, PositionsOfBlackFigures[i], Board);
                    if (IndexesOfLegalMoves.Contains(positionOfWhiteKing))
                    {
                        OutList.Add(PositionsOfBlackFigures[i]);
                    }
                }

            }
            else
            {
                for (int i = 0; i < PositionsOfWhiteFigures.Count; i++)
                {
                    List<int> IndexesOfLegalMoves = new List<int>();
                    string piece = Board[PositionsOfWhiteFigures[i]];
                    IndexesOfLegalMoves = IndexesOfPossibleMovesNoReccursion(piece, PositionsOfWhiteFigures[i], Board);
                    if (IndexesOfLegalMoves.Contains(positionOfBlackKing))
                    {
                        OutList.Add(PositionsOfWhiteFigures[i]);
                    }
                }
            }
            return OutList;
        }
        
        
       
        public static List<bool> DetermineIfRooksMovedInOrder()//Returns a list that contains information if any of the rooks has moved from thei initial position
        {
            var tempList = new List<string>();
            foreach (string item in Globals.MoveRecord)
            {
                string forReplacement = item;
                forReplacement = Regex.Replace(forReplacement, @"\d*: ", "");
                tempList.Add(forReplacement);
            }
            List<bool> ForOut = new List<bool>();
            if (tempList.Any(x => x.StartsWith("WrA1")) == true)
            {
                ForOut.Add(true);
            }
            else
            {
                ForOut.Add(false);
            }
            if (tempList.Any(x => x.StartsWith("WrH1")) == true)
            {
                ForOut.Add(true);
            }
            else
            {
                ForOut.Add(false);
            }
            if (tempList.Any(x => x.StartsWith("BrA8")) == true)
            {
                ForOut.Add(true);
            }
            else
            {
                ForOut.Add(false);
            }
            if (tempList.Any(x => x.StartsWith("BrH8")) == true)
            {
                ForOut.Add(true);
            }
            else
            {
                ForOut.Add(false);
            }
            return ForOut;
        }
        
        public static Dictionary<string, string> PopulateADictionary()//A method that maps a chess piece to it’s image in Resources folder. Used to set up global variable FromBoardToPiecePaths
        {
            Dictionary<string, string> dict = new Dictionary<string, string>
            {
                {"Wr","\\WhiteRook.png" },
                {"Wn", "\\WhiteKnight.png"},
                {"Wb","\\WhiteBishop.png" },
                {"Wq","\\WhiteQueen.png" },
                {"Wk","\\WhiteKing.png" },
                {"Wp","\\WhitePawn.png" },
                {"Br","\\BlackRook.png" },
                {"Bn", "\\BlackKnight.png"},
                {"Bb","\\BlackBishop.png" },
                {"Bq","\\BlackQueen.png" },
                {"Bk","\\BlackKing.png" },
                {"Bp","\\BlackPawn.png" },
            };
            return dict;
        }

        private void GameWindow_Loaded(object sender, RoutedEventArgs e)//Entry point of the window, used to call other important code
        {
            InitializeUI();
            



            DisplayBoardOnInterface();
            Globals.MoveCounter = Globals.MoveRecord.Count + 1;
            if(Globals.AI == White && Globals.WhitesTurn == true)
            {
                DelayAction(500, new Action(() => { MakeAIMove(); }));
            }
        } 
        public static List<int> FindBestMoveAI(string color, List<string> Board)//Driver code for AI logic, returns the best move calculated by the algorithm. Also splits the possible moves in two parts in order to utilize parallel processing 
        {
            List<List<int>> PossibleMoves = GetAllLegalMovesForSelectedColor(color, Board); //Get all possible moves for a particular color in a particular board
            List<int> BestMove = new List<int>();
            List<object> ForThread = new List<object>() ;
            ForThread.Add(Board);
            var PossibleMovesMainThread = new List<List<int>>();
            var PossibleMovesThread1 = new List<List<int>>();
            int midPoint = PossibleMoves.Count / 2;
            for(int i = 0; i < midPoint; i++)
            {
                PossibleMovesMainThread.Add(PossibleMoves[i]);
            }
            for (int i = midPoint; i < PossibleMoves.Count; i++)
            {
                PossibleMovesThread1.Add(PossibleMoves[i]);
            }
            ForThread.Add(PossibleMovesThread1);
            PossibleMoves.Clear();
            //Split the possible moves into two parts, second part will be processed on another thread

            double bestScore = -10000000;
            
            ParameterizedThreadStart thread1Start = new ParameterizedThreadStart(ThreadTwo);
            Thread thread1 = new Thread(thread1Start);
            thread1.Start(ForThread);
            //Start second core
            for (int i = 0; i < PossibleMovesMainThread.Count; i++)
            {
                var list = PossibleMovesMainThread[i];
                for (int q = 1; q < list.Count; q++)
                {
                    var scopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(list[0]), ConvertAbsoluteToBoardNotation(list[q]), Board);
                    double score = minimax(scopedBoard, 0,false);
                    

                    if (score > bestScore)
                    {
                        BestMove.Clear();
                        bestScore = score;
                        BestMove.Add(list[0]);
                        BestMove.Add(list[q]);

                    }
                    //Apply minimax algorithm and determine move with the best score
                }
            }


            while (thread1.IsAlive == true)
            {
                Thread.Sleep(2000);
                //If second thread hasn't finished, wait 2 seconds


            }
            List<object> thread1Info = Globals.ExitThreadInfo;
            double scoreForSecondThread = (double)thread1Info[1];
            if (bestScore <= scoreForSecondThread)
            {
                BestMove = (List<int>)thread1Info[0];
            }
            Globals.ExitThreadInfo.Clear();
            return BestMove;
            //Choose the move with the best score choosing from mainThread move and secondThread move




        }
        public static double minimax(List<string> Board, int depth,bool maximizing)//The algorithm responsible for valuation of moves. Uses recursion to give numerical score to all moves 
        {
            
            string color = "";
            if (Globals.AI == White && maximizing == true)
            {
                color = White;
            }
            else if (Globals.AI == White && maximizing == false)
            {
                color = Black;
            }
            else if (Globals.AI == Black && maximizing == true)
            {
                color = Black;
            }
            else if (Globals.AI == Black && maximizing == false)
            {
                color = White;
            }
            bool whiteTurn = true;
            if(color == Black)
            {
                whiteTurn = false;
            }
            int stateOfGame = CheckGameEndConditions(Board,whiteTurn);
            
            
            if (stateOfGame != 0) // Determine if Board is in Terminal state
            {
                
                if (stateOfGame == 1)
                {
                    if (Globals.AI == White && maximizing == true)
                    {
                        return -100000;
                    }
                    else if (Globals.AI == White && maximizing == false)
                    {
                        return -100000;
                    }
                    else if (Globals.AI == Black && maximizing == true)
                    {
                        return 100000;
                    }
                    else if (Globals.AI == Black && maximizing == false)
                    {
                        return 100000;
                    }
                }
                else if (stateOfGame == 2)
                {
                    if (Globals.AI == White && maximizing == true)
                    {
                        return 100000;
                    }
                    else if (Globals.AI == White && maximizing == false)
                    {
                        return 100000;
                    }
                    else if (Globals.AI == Black && maximizing == true)
                    {
                        return -100000;
                    }
                    else if (Globals.AI == Black && maximizing == false)
                    {
                        return -100000;
                    }
                }
                else if (stateOfGame == 3)
                {
                    return 0;
                }
            }
            if (depth == 2) //Depth limit reached - Return static valuation
            {
                return ValuatePosition(Board);
            }
            if (maximizing == true)
            {
                double bestScore = -10000000;
                
                
                List<List<int>> PossibleMoves = GetAllLegalMovesForSelectedColor(color, Board);


                for (int i = 0; i < PossibleMoves.Count; i++)
                {
                    var list = PossibleMoves[i];
                    for (int q = 1; q < list.Count; q++)
                    {
                        var scopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(list[0]), ConvertAbsoluteToBoardNotation(list[q]), Board);
                        double score = minimax(scopedBoard, depth + 1, false);
                        //Recursivly apply minimax to score the node
                        if (score > bestScore)
                        {

                            bestScore = score;

                        }
                        
                    }

                }
                return bestScore;



            }
            else
            {
                double bestScore = 10000000;
                
                
                List<List<int>> PossibleMoves = GetAllLegalMovesForSelectedColor(color, Board);


                for (int i = 0; i < PossibleMoves.Count; i++)
                {
                    var list = PossibleMoves[i];
                    for (int q = 1; q < list.Count; q++)
                    {
                        var scopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(list[0]), ConvertAbsoluteToBoardNotation(list[q]), Board);
                        double score = minimax(scopedBoard, depth + 1, true);
                        //Recursivly apply minimax to score the node
                        if (score < bestScore)
                        {

                            bestScore = score;

                        }
                        
                    }

                }
                return bestScore;
            }
            
        }
         public static void ThreadTwo(object c)//Calls minimax on the second half of the moves, which parallelises the processing
        {
            
            List<object> Unpack = c as List<object>;
            List<string> Board = Unpack[0] as List<string>;
            List<int> BestMove = new List<int>();
            List<List<int>> PossibleMoves = Unpack[1] as List<List<int>>;
            
            double bestScore = -10000000;
            for (int i = 0; i < PossibleMoves.Count; i++)
            {
                var list = PossibleMoves[i];
                for (int q = 1; q < list.Count; q++)
                {
                    var scopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(list[0]), ConvertAbsoluteToBoardNotation(list[q]), Board);
                    double score = minimax(scopedBoard, 0/*, -10000000, 10000000*/, false);
                    
                    if (score > bestScore)
                    {
                        BestMove.Clear();
                        bestScore = score;
                        BestMove.Add(list[0]);
                        BestMove.Add(list[q]);

                    }
                }
            }
            Globals.ExitThreadInfo.Add(BestMove);
            Globals.ExitThreadInfo.Add(bestScore);


        }
        
        public static List<List<int>> GetAllLegalMovesForSelectedColor(string color, List<string> Board)//Returns all possible moves for selected color on a certain board
        {
            List<List<int>> listOfAllMoves = new List<List<int>>();
            List<int> PositionsOfWhiteFigures = new List<int>();
            List<int> PositionsOfBlackFigures = new List<int>();
            List<string> LocalCopyOfBoard = new List<string>();
            for (int i = 0; i < Board.Count; i++)
            {
                char temp = Board[i][0];
                if (temp == 'W')
                {




                    PositionsOfWhiteFigures.Add(i);


                }
                else if (temp == 'B')
                {




                    PositionsOfBlackFigures.Add(i);


                }
            }
            if (color == "W")
            {
                for (int i = 0; i < PositionsOfWhiteFigures.Count; i++)
                {
                    List<int> LegalMovesForScopedFigure = IndexesOfPossibleMoves(Board,Board[PositionsOfWhiteFigures[i]], PositionsOfWhiteFigures[i]);
                    var temp = new List<int>();
                    temp.Add(PositionsOfWhiteFigures[i]);
                    foreach (var item in LegalMovesForScopedFigure)
                    {
                        temp.Add(item);
                    }
                    listOfAllMoves.Add(temp);
                }
            }
            else
            {
                for (int i = 0; i < PositionsOfBlackFigures.Count; i++)
                {
                    List<int> LegalMovesForScopedFigure = IndexesOfPossibleMoves(Board,Board[PositionsOfBlackFigures[i]], PositionsOfBlackFigures[i]);
                    var temp = new List<int>();
                    temp.Add(PositionsOfBlackFigures[i]);
                    foreach (var item in LegalMovesForScopedFigure)
                    {
                        temp.Add(item);
                    }
                    listOfAllMoves.Add(temp);
                }
            }
            bool close = false;
            while (close == false)
            {

                bool deleted = false;
                for (int i = 0; i < listOfAllMoves.Count; i++)
                {
                    if (listOfAllMoves[i].Count == 1)
                    {
                        listOfAllMoves.RemoveAt(i);
                        deleted = true;
                        break;
                    }
                }
                if (deleted == false)
                {
                    close = true;
                }
            }
            return listOfAllMoves;
        }
        public static double ValuatePosition(List<string> Board) // A very simple static evaluation of board
        {
            List<string> LocalCopyOfBoard = new List<string>();
            foreach(var item in Board)
            {
                LocalCopyOfBoard.Add(item);
            }
            Dictionary<string, double> ValuationsForPieces = new Dictionary<string, double>
            {
                {"p",1 },
                {"n",3 },
                {"b",3.2 },
                {"r",8 },
                {"q",16 }

            };
            double ValuationWhite = 0;
            double ValuationBlack = 0;
            for(int i = 0;i < LocalCopyOfBoard.Count; i++)
            {
                if (LocalCopyOfBoard[i].StartsWith("W"))
                {
                    LocalCopyOfBoard[i] = Regex.Replace(LocalCopyOfBoard[i], @"^W", "");
                    double temp = 0;
                    ValuationsForPieces.TryGetValue(LocalCopyOfBoard[i], out temp);
                    /*
                    if(temp == 1)
                    {
                        int column = i % BoardSize;
                        int row = i / BoardSize;
                        if(row == 3)
                        {
                            temp = temp + 0.1;
                        }
                    }
                    */
                    ValuationWhite += temp;
                }
                else
                {
                    LocalCopyOfBoard[i] = Regex.Replace(LocalCopyOfBoard[i], @"^B", "");
                    double temp = 0;
                    ValuationsForPieces.TryGetValue(LocalCopyOfBoard[i], out temp);
                    /*
                    if (temp == 1)
                    {
                        int column = i % BoardSize;
                        int row = i / BoardSize;
                        if (row == 4)
                        {
                            temp = temp + 0.1;
                        }
                    }
                    */
                    ValuationBlack += temp;
                }
               
               



            }
            if (Globals.AI == White)
            {


                return ValuationWhite - ValuationBlack;
            }
            else if(Globals.AI == Black)
            {
                return ValuationBlack - ValuationWhite;
            }
            return 0;
        }
        public static void InitializeBoardFischerChess()
        {
            Random rnd = new Random();
            for(int i = 0; i < 64; i++)
            {
                Globals.Board.Add(Empty);
            }
            List<int> OccupiedSquares = new List<int>();
            bool close = false;
            int tempRndValue = 0;
            while(close == false)
            {
                tempRndValue = rnd.Next(0, 8);
                if(tempRndValue % 2 == 0)
                {
                    OccupiedSquares.Add(tempRndValue);
                    close = true;
                }

            }
            close = false;
            int tempRndValue2 = 0;
            while(close == false)
            {
                tempRndValue2 = rnd.Next(0, 8);
                if(tempRndValue2 % 2 == 1 && OccupiedSquares.Contains(tempRndValue2) == false)
                {
                    OccupiedSquares.Add(tempRndValue2);
                    close = true;
                }
            }
            Globals.Board[tempRndValue] = White + Bishop;
            Globals.Board[tempRndValue2] = White + Bishop;
            Globals.Board[tempRndValue + 56] = Black + Bishop;
            Globals.Board[tempRndValue2 + 56] = Black + Bishop;
            tempRndValue = 0;
            close = false;
            while (close == false)
            {
                tempRndValue = rnd.Next(0, 8);
                if (OccupiedSquares.Contains(tempRndValue) == false)
                {
                    OccupiedSquares.Add(tempRndValue);
                    close = true;
                }

            }
            close = false;
            tempRndValue2 = 0;
            while (close == false)
            {
                tempRndValue2 = rnd.Next(0, 8);
                if (OccupiedSquares.Contains(tempRndValue2) == false)
                {
                    OccupiedSquares.Add(tempRndValue2);
                    close = true;
                }

            }
            Globals.Board[tempRndValue] = White + Rook;
            Globals.Board[tempRndValue2] = White + Rook;
            Globals.Board[tempRndValue + 56] = Black + Rook;
            Globals.Board[tempRndValue2 + 56] = Black + Rook;
            tempRndValue = 0;
            close = false;
            while (close == false)
            {
                tempRndValue = rnd.Next(0, 8);
                if (OccupiedSquares.Contains(tempRndValue) == false)
                {
                    OccupiedSquares.Add(tempRndValue);
                    close = true;
                }

            }
            close = false;
            tempRndValue2 = 0;
            while (close == false)
            {
                tempRndValue2 = rnd.Next(0, 8);
                if (OccupiedSquares.Contains(tempRndValue2) == false)
                {
                    OccupiedSquares.Add(tempRndValue2);
                    close = true;
                }

            }
            Globals.Board[tempRndValue] = White + Knight;
            Globals.Board[tempRndValue2] = White + Knight;
            Globals.Board[tempRndValue + 56] = Black + Knight;
            Globals.Board[tempRndValue2 + 56] = Black + Knight;
            tempRndValue = 0;
            close = false;
            while (close == false)
            {
                tempRndValue = rnd.Next(0, 8);
                if (OccupiedSquares.Contains(tempRndValue) == false)
                {
                    OccupiedSquares.Add(tempRndValue);
                    close = true;
                }

            }
            Globals.Board[tempRndValue] = White + Queen;
            Globals.Board[tempRndValue + 56] = Black + Queen;
            int counter = 0;
            while(OccupiedSquares.Contains(counter) == true)
            {
                counter++;
            }
            Globals.Board[counter] = White + King;
            Globals.Board[counter + 56] = Black + King;
            for(int i = 8; i < 16; i++)
            {
                Globals.Board[i] = White + Pawn;
            }
            for (int i = 48; i <56; i++)
            {
                Globals.Board[i] = Black + Pawn;
            }

        }
        

        
    }

    public static class Globals
    {
        private static int moveCounter;
        private static string pathToSave = "";
        private static string pathToResources = "";
        private static List<Button> allButtons = new List<Button>();
        
        private static List<string> board = new List<string>();

        private static bool whitesTurn = true;
        private static bool whiteKingMoved = false;
        private static bool blackKingMoved = false;
        private static List<string> moveRecord = new List<string>();
        public static List<string> Board { get => board; set => board = value; }

        public static bool WhitesTurn { get => whitesTurn; set => whitesTurn = value; }
        public static bool WhiteKingMoved { get => whiteKingMoved; set => whiteKingMoved = value; }
        public static bool BlackKingMoved { get => blackKingMoved; set => blackKingMoved = value; }
        public static List<string> MoveRecord { get => moveRecord; set => moveRecord = value; }
        public static int MoveCounter { get => moveCounter; set => moveCounter = value; }
        public static string PathToSave { get => pathToSave; set => pathToSave = value; }
        public static string PathToResources { get => pathToResources; set => pathToResources = value; }
        public static List<Button> AllButtons { get => allButtons; set => allButtons = value; }
       
       
        public static Dictionary<string, string> FromBoardToPiecePathes { get => fromBoardToPiecePathes; set => fromBoardToPiecePathes = value; }
        public static bool WaitingForSecondClick { get => waitingForSecondClick; set => waitingForSecondClick = value; }
        public static int FirstClickIndex { get => firstClickIndex; set => firstClickIndex = value; }
        public static List<int> PositionOfPawnToBePromotedAndPiece { get => positionOfPawnToBePromotedAndPiece; set => positionOfPawnToBePromotedAndPiece = value; }
        public static List<object> ExitThreadInfo { get => exitThreadInfo; set => exitThreadInfo = value; }
        public static string AI { get => aI; set => aI = value; }
        public static int PrimePlayerTimerTimeSeconds { get => primePlayerTimerTimeSeconds; set => primePlayerTimerTimeSeconds = value; }
        public static int OtherPlayerTimerTimeSeconds { get => otherPlayerTimerTimeSeconds; set => otherPlayerTimerTimeSeconds = value; }
        public static DispatcherTimer PrimePlayerTimer { get => primalPlayerTimer; set => primalPlayerTimer = value; }
        public static DispatcherTimer OtherPlayerTimer { get => otherPlayerTimer; set => otherPlayerTimer = value; }
        public static MediaPlayer PlacePieceSoundEffect { get => placePieceSoundEffect; set => placePieceSoundEffect = value; }

       
        private static Dictionary<string, string> fromBoardToPiecePathes;
        private static bool waitingForSecondClick = false;
        private static int firstClickIndex = -1;
        private static List<int> positionOfPawnToBePromotedAndPiece = null;
        private static List<object> exitThreadInfo = new List<object>();
        private static string aI;
        private static int primePlayerTimerTimeSeconds = -2;
        private static int otherPlayerTimerTimeSeconds = -2;
        private static DispatcherTimer primalPlayerTimer = new DispatcherTimer();
        private static DispatcherTimer otherPlayerTimer = new DispatcherTimer();
        private static MediaPlayer placePieceSoundEffect = new MediaPlayer();
        
    }
    

}

