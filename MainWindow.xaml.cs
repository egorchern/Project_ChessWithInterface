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
        public MainWindow()
        {
            
            InitializeComponent();
            /*
            //Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=F:\ProjectChessMAIN\Database1.mdf;Integrated Security=True
            SqlConnection gameArchive = new SqlConnection();
            SqlCommand command = new SqlCommand();
            gameArchive.ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;" + @"AttachDbFilename=F:\ProjectChessMAIN\Database1.mdf;" + @"Integrated Security=True";
            gameArchive.Open();
            command.Connection = gameArchive;
            command.CommandText = "INSERT INTO PlayedGames(Id,Outcome) VALUES(192,'White')";
            command.ExecuteNonQuery();
            command.CommandText = "SELECT * FROM PlayedGames WHERE Outcome = 'White'";
            

            SqlDataReader reader = command.ExecuteReader();
            string outPutString = "";
            while (reader.Read())
            {
                outPutString += "\n" + $"{reader.GetValue(0)} : {reader.GetValue(1)}";
            }
            MessageBox.Show(outPutString);
            double d = 0.0;
            */

        }
        public  void InitializeUI()
        {
            GetAllButtonElements();
            SortButtons();
            Globals.FromBoardToPiecePathes = PopulateADictionary();
           
            GameWindow.Icon = new BitmapImage(new Uri(Globals.pathToResources + "\\ChessIcon.png"));
            Board.Source = new BitmapImage(new Uri(Globals.pathToResources + "\\Board.png"));
            SaveGameImage.Source = new BitmapImage(new Uri(Globals.pathToResources + "\\SaveGameIcon.png"));
            SaveGameImage.MouseDown += SaveGameImage_MouseDown;
            image.Source = new BitmapImage(new Uri(Globals.pathToResources + "\\ChessIcon.png"));
            foreach (Button btn in Globals.AllButtons)
            {
                btn.Click += UniversalSquareClickEventHandle;
            }
        }
        private void SaveGameImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SaveGameDialog save = new SaveGameDialog();
            save.ShowDialog();
        }

        public static bool CanProceedWithTurn(int index)
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
        /*public static void HighlightValidSquare(List<int> Moves)
        {
            for (int i = 0; i < Moves.Count; i++)
            {
                if(Moves.Count != 0)
                {
                    int position = Moves[i];
                    Globals.AllButtons[position].Background = Brushes.Blue;
                }
            }
        }
        */
        /*public static void ResetBackgrounds()
        {
            foreach (Button btn in Globals.AllButtons)
            {
                
                btn.Background = default;
            }
        }
        */
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
                        Source = new BitmapImage(new Uri(Globals.pathToResources + pathToPiece)),
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
        public void SortButtons()
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
        public static int CheckGameEndConditions(List<string> Board,bool whitesTurn) //0-Nothing; 1-White Checkmated; 2-Black Checkmated; 3-Stalemate
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
        public void DisableAllButtons()
        {
            foreach (Button btn in Globals.AllButtons)
            {
                btn.Click -= UniversalSquareClickEventHandle;
            }
        }
        private void UniversalSquareClickEventHandle(object sender, RoutedEventArgs e)
        {
            
            string name = ((Button)sender).Name;
            string subStringForIndex = Regex.Replace(name, @"^btn", "");
            int indexOfClickedSquare = Convert.ToInt32(subStringForIndex);
            if (Globals.WaitingForSecondClick == false)
            {


                bool Verified = CanProceedWithTurn(indexOfClickedSquare);
                if (Verified == true)
                {


                    int index = 0;
                    for (int i = 0; i < Globals.AllButtons.Count; i++)
                    {
                        string scopedName = Globals.AllButtons[i].Name;
                        if (scopedName == name)
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
                    int StateOfGame = CheckGameEndConditions(Globals.Board,Globals.WhitesTurn); //0-Nothing; 1-White Checkmated; 2-Black Checkmated; 3-Stalemate
                    if (StateOfGame == 1)
                    {
                        TurnIndicator.Text = "White king chechmated\nBlack has won!";
                        DisableAllButtons();
                    }
                    else if (StateOfGame == 2)
                    {
                        TurnIndicator.Text = "Black king checkmated\nWhite has won!";
                        DisableAllButtons();
                    }
                    else if(StateOfGame == 3)
                    {
                        TurnIndicator.Text = "Stalemate\nGame has been drawn!";
                        DisableAllButtons();
                    }
                }
            }


           
            
        }
        public static bool CanProceedWithSecondClick(int startIndex,int endIndex) 
        {
            List<int> PossibleMovesOfSelectedPiece = IndexesOfPossibleMoves(Globals.Board,Globals.Board[startIndex], startIndex);
            if(PossibleMovesOfSelectedPiece.Contains(endIndex) == false)
            {
                MessageBox.Show("ERROR: Ilegal move");
                return false;
            }
            
            return true;
        }

        public  void GetAllButtonElements()
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


        

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        public static void InitializeBoard()
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
        public static List<int> IndexesOfPossibleMoves(List<string> Board,string piece, int position)
        {

            List<int> OutList = new List<int>();
            List<string> ScopedBoard = new List<string>();
            foreach (string item in Board)
            {
                ScopedBoard.Add(item);
            }
            OutList = IndexesOfPossibleMovesKing(piece, position, ScopedBoard);
            List<string> CopyOf = new List<string>();

            List<int> PiecesCheckingKing = new List<int>();
            List<int> PossibleMoves = new List<int>();
            foreach (int item in OutList)
            {
                PossibleMoves.Add(item);
            }
            double d = 0.0;
            for (int i = 0; i < PossibleMoves.Count; i++)
            {
                foreach (string item in Board)
                {
                    CopyOf.Add(item);
                }
                CopyOf = MovePieceLocal(ConvertAbsoluteToBoardNotation(position), ConvertAbsoluteToBoardNotation(PossibleMoves[i]), CopyOf);
                bool whitesTurn = true;
                if(piece[0] == 'B')
                {
                    whitesTurn = false;
                }
                PiecesCheckingKing = KingInCheckAndByWhichFigures(CopyOf,whitesTurn);
                if (PiecesCheckingKing.Count == 0)
                {

                }
                else
                {
                    OutList.Remove(PossibleMoves[i]);
                }
                CopyOf.Clear();
            }


            return OutList;
        }
        public static List<int> IndexesOfPossibleMovesKing(string piece, int position, List<string> ScopedBoard)
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
        public static List<int> GetIndexesOfPossibleMovesKnight(int column, int row, bool whitesTurnn, List<string> Board)
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
        public static List<int> GetIndexesOfPossibleMovesPawn(int column, int row, bool whitesTurnn, List<string> Board)
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
                            Globals.EnPessantDestination = GetAbolutePosition(column + 1, row + 1);
                            Globals.CapturedInEnPessant = AbsolutePostionOfEnPessant;
                        }
                        else if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece - 1)
                        {
                            OutList.Add(GetAbolutePosition(column - 1, row + 1));
                            Globals.CapturedInEnPessant = AbsolutePostionOfEnPessant;
                            Globals.EnPessantDestination = GetAbolutePosition(column - 1, row + 1);
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
                            Globals.EnPessantDestination = GetAbolutePosition(column + 1, row - 1);
                            Globals.CapturedInEnPessant = AbsolutePostionOfEnPessant;
                        }
                        else if (AbsolutePostionOfEnPessant == AbsolutePostionOfPiece - 1)
                        {
                            OutList.Add(GetAbolutePosition(column - 1, row - 1));
                            Globals.EnPessantDestination = GetAbolutePosition(column - 1, row - 1);
                            Globals.CapturedInEnPessant = AbsolutePostionOfEnPessant;
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
        public static List<int> GetIndexesOfPossibleMovesRook(int column, int row, bool whitesTurnn, List<string> Board)
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
        public static List<int> GetIndexesOfPossibleMovesBishop(int column, int row, bool whitesTurnn, List<string> Board)
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
        public static string ConvertAbsoluteToBoardNotation(int index)
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
        public static List<int> GetIndexesOfPossibleMovesQueen(int column, int row, bool whitesTurnn, List<string> Board)
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
        public static int GetAbolutePosition(int column, int row)
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
        public static int ChessNotationToAbsolute(string n)
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
            int tempo = 0;
            ColumnTable.TryGetValue(temp, out tempo);
            int column = tempo;
            temp = Convert.ToString(n[1]);
            int row = Convert.ToInt32(temp) - 1;
            return GetAbolutePosition(column, row);


        }
        public static void MovePiece(string initialPos, string destination, int counter)
        {
            string piece = "";
            int AbsoluteInitialPos = ChessNotationToAbsolute(initialPos);
            int DestinationAbsolute = ChessNotationToAbsolute(destination);
            piece = Globals.Board[AbsoluteInitialPos];
            List<int> IndexesOfPossibleMovess = IndexesOfPossibleMoves(Globals.Board,piece, AbsoluteInitialPos);

            if (IndexesOfPossibleMovess.Contains(DestinationAbsolute) == true)
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
                    if (ConvertAbsoluteToBoardNotation(Globals.EnPessantDestination) == destination && piece[1] == 'p')
                    {
                        Globals.Board[AbsoluteInitialPos] = Empty;
                        Globals.Board[DestinationAbsolute] = piece;
                        Globals.Board[Globals.CapturedInEnPessant] = Empty;
                        Globals.MoveRecord.Add($"{counter}: {piece}{initialPos} => {destination}(En Pessant {ConvertAbsoluteToBoardNotation(Globals.CapturedInEnPessant)})");
                        Globals.CapturedInEnPessant = -1;
                        Globals.EnPessantDestination = -1;
                    }
                    else
                    {



                        if (Globals.Board[DestinationAbsolute] == Empty)
                        {
                            Globals.Board[DestinationAbsolute] = piece;
                            Globals.Board[AbsoluteInitialPos] = Empty;
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
                                if (DestinationAbsolute >= 56 || DestinationAbsolute <= 7)

                                {
                                    string color = Convert.ToString(piece[0]);
                                    Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { DestinationAbsolute, -1 };
                                    PawnPromotion promotion = new PawnPromotion();
                                    promotion.ShowDialog();
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
                                    PromoteAPawn(DestinationAbsolute, PromotePieceTo);
                                    string PromotedTo = PromotePieceTo;
                                    Globals.MoveRecord[Globals.MoveRecord.Count - 1] += $"(Promoted to {PromotedTo.ToUpper()})";

                                }
                            }
                        }
                        else
                        {
                            string temp = Globals.Board[DestinationAbsolute];
                            Globals.Board[DestinationAbsolute] = piece;
                            Globals.Board[AbsoluteInitialPos] = Empty;
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
                                if (DestinationAbsolute >= 56 || DestinationAbsolute <= 7)

                                {
                                    string color = Convert.ToString(piece[0]);
                                    Globals.PositionOfPawnToBePromotedAndPiece = new List<int> { DestinationAbsolute, -1 };
                                    PawnPromotion promotion = new PawnPromotion();
                                    promotion.ShowDialog();
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
                                    PromoteAPawn(DestinationAbsolute, PromotePieceTo);
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
        public static List<string> MovePieceLocal(string initialPos, string destination, List<string> arr)
        {
            List<string> Copu = new List<string>();
            foreach (string item in arr)
            {
                Copu.Add(item);
            }
            string piece = "";
            int AbsoluteInitialPos = ChessNotationToAbsolute(initialPos);
            int DestinationAbsolute = ChessNotationToAbsolute(destination);
            piece = Copu[AbsoluteInitialPos];
            




            if (Copu[DestinationAbsolute] == Empty)
            {
                if (piece == White + Pawn)
                {
                    if (AbsoluteInitialPos <= 55 && AbsoluteInitialPos >= 48 && DestinationAbsolute > 55)
                    {
                        Copu[AbsoluteInitialPos] = Empty;
                        Copu[DestinationAbsolute] = White + Queen;
                    }
                    else
                    {
                        Copu[DestinationAbsolute] = piece;
                        Copu[AbsoluteInitialPos] = Empty;
                    }
                }
                else if (piece == Black + Pawn)
                {
                    if (AbsoluteInitialPos >= 8 && AbsoluteInitialPos <= 15 && DestinationAbsolute < 8)
                    {
                        Copu[AbsoluteInitialPos] = Empty;
                        Copu[DestinationAbsolute] = Black + Queen;
                    }
                    else
                    {
                        Copu[DestinationAbsolute] = piece;
                        Copu[AbsoluteInitialPos] = Empty;
                    }
                }
                else
                {


                    Copu[DestinationAbsolute] = piece;
                    Copu[AbsoluteInitialPos] = Empty;
                }
            }
            else
            {
                if (piece == White + Pawn)
                {
                    if (AbsoluteInitialPos <= 55 && AbsoluteInitialPos >= 48 && DestinationAbsolute > 55)
                    {
                        Copu[AbsoluteInitialPos] = Empty;
                        Copu[DestinationAbsolute] = White + Queen;
                    }
                    else
                    {
                        Copu[DestinationAbsolute] = piece;
                        Copu[AbsoluteInitialPos] = Empty;
                    }
                }
                else if (piece == Black + Pawn)
                {
                    if (AbsoluteInitialPos >= 8 && AbsoluteInitialPos <= 15 && DestinationAbsolute < 8)
                    {
                        Copu[AbsoluteInitialPos] = Empty;
                        Copu[DestinationAbsolute] = Black + Queen;
                    }
                    else
                    {
                        Copu[DestinationAbsolute] = piece;
                        Copu[AbsoluteInitialPos] = Empty;
                    }
                }
                else
                {


                    Copu[DestinationAbsolute] = piece;
                    Copu[AbsoluteInitialPos] = Empty;
                }
            }

            return Copu;



        }

        public static List<int> GetIndexesOfPossibleMovesKing(int column, int row, bool whitesTurnn, List<string> board)
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
            List<bool> RooksMoved = DetermineIfRooksMovedInOrder();
            if (whitesTurnn == true)
            {
                if (Globals.WhiteKingMoved == false)
                {
                    if (board[5] == Empty && board[6] == Empty && RooksMoved[1] == false)
                    {
                        List<int> PiecesCheckingKing = new List<int>();
                        List<string> ScopedBoard = new List<string>();
                        foreach (string item in board)
                        {
                            ScopedBoard.Add(item);
                        }
                        PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard,whitesTurnn);
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

                    if (board[1] == Empty && board[2] == Empty && board[3] == Empty && RooksMoved[0] == false)
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
                    if (board[61] == Empty && board[62] == Empty && RooksMoved[3] == false)
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
                    if (board[57] == Empty && board[58] == Empty && board[59] == Empty && RooksMoved[2] == false)
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
        
        public static List<int> KingInCheckAndByWhichFigures(List<string> Board,bool whitesTurn)
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
                    IndexesOfLegalMoves = IndexesOfPossibleMovesKing(piece, PositionsOfBlackFigures[i], Board);
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
                    IndexesOfLegalMoves = IndexesOfPossibleMovesKing(piece, PositionsOfWhiteFigures[i], Board);
                    if (IndexesOfLegalMoves.Contains(positionOfBlackKing))
                    {
                        OutList.Add(PositionsOfWhiteFigures[i]);
                    }
                }
            }
            return OutList;
        }
        
        
       
        public static List<bool> DetermineIfRooksMovedInOrder()
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
            if (tempList.Any(x => x.StartsWith("WrA8")) == true)
            {
                ForOut.Add(true);
            }
            else
            {
                ForOut.Add(false);
            }
            if (tempList.Any(x => x.StartsWith("BrH1")) == true)
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
        
        public static Dictionary<string, string> PopulateADictionary()
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

        private void GameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUI();




            DisplayBoardOnInterface();
            Globals.AI = "B";
            
        }
        public static List<int> FindBestMoveAI(string color, List<string> Board)
        {
            List<List<int>> PossibleMoves = GetAllLegalMovesForSelectedColor(color, Board);
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

            double bestScore = -10000000;
            
            ParameterizedThreadStart thread1Start = new ParameterizedThreadStart(ThreadTwo);
            Thread thread1 = new Thread(thread1Start);
            thread1.Start(ForThread);
            for (int i = 0; i < PossibleMovesMainThread.Count; i++)
            {
                var list = PossibleMovesMainThread[i];
                for (int q = 1; q < list.Count; q++)
                {
                    var scopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(list[0]), ConvertAbsoluteToBoardNotation(list[q]), Board);
                    double score = minimax(scopedBoard, 0,/* -10000000, 10000000*/ false);

                    if (score > bestScore)
                    {
                        BestMove.Clear();
                        bestScore = score;
                        BestMove.Add(list[0]);
                        BestMove.Add(list[q]);

                    }
                }
            }


            while (thread1.IsAlive == true)
            {
                Thread.Sleep(2000);



            }
            List<object> thread1Info = Globals.ExitThreadInfo;
            double scoreForSecondThread = (double)thread1Info[1];
            if (bestScore <= scoreForSecondThread)
            {
                BestMove = (List<int>)thread1Info[0];
            }
            Globals.ExitThreadInfo.Clear();
            return BestMove;




        }
        public static double minimax(List<string> Board, int depth,/* double alpha, double beta*/ bool maximizing)
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
            
            
            if (stateOfGame != 0)
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
            if (depth == 2)
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
                        double score = minimax(scopedBoard, depth + 1/*, alpha, beta*/, false);
                        if (score > bestScore)
                        {

                            bestScore = score;

                        }
                        /*
                        if (score > alpha)
                        {
                            alpha = score;
                        }
                        if (beta <= alpha)
                        {
                            break;
                        }
                        */
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
                        double score = minimax(scopedBoard, depth + 1/*, alpha, beta*/, true);
                        if (score < bestScore)
                        {

                            bestScore = score;

                        }
                        /*
                        if (score < beta)
                        {
                            beta = score;
                        }
                        if (beta <= alpha)
                        {
                            break;
                        }
                        */
                    }

                }
                return bestScore;
            }
            
        }
         public static void ThreadTwo(object c)
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
        
        public static List<List<int>> GetAllLegalMovesForSelectedColor(string color, List<string> Board)
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
        public static double ValuatePosition(List<string> Board)
        {
            List<string> LocalCopyOfBoard = new List<string>();
            foreach(var item in Board)
            {
                LocalCopyOfBoard.Add(item);
            }
            Dictionary<string, double> ValuationsForPieces = new Dictionary<string, double>
            {
                {$"p",1 },
                {$"n",3 },
                {$"b",3.2 },
                {$"r",8 },
                {$"q",16 }

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
                    ValuationWhite += temp;
                }
                else
                {
                    LocalCopyOfBoard[i] = Regex.Replace(LocalCopyOfBoard[i], @"^B", "");
                    double temp = 0;
                    ValuationsForPieces.TryGetValue(LocalCopyOfBoard[i], out temp);
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
        

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            List<int> AIMOve = FindBestMoveAI(Globals.AI, Globals.Board);
            RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
            Globals.AllButtons[AIMOve[0]].RaiseEvent(newEventArgs);
            Globals.AllButtons[AIMOve[1]].RaiseEvent(newEventArgs);
            
            /*var i = CheckGameEndConditions(new List<string> { "Wr","Wn","Wb","00","Wk","00","Wn","Wr","Wp","Wp","Wp","Wp","00","Wp","Wp","Wp","00","00","00","00","00","00","00","00","00","00","Wb","00","Wp","00","00","00","00","00","00","00","Bp","00","00","00","Bp","Bp","00","00","00","00","00","00","00","00","Bp","Bp","00","Wq","Bp","Bp","Br","Bn","Bb","Bq","Bk","Bb","Bn","Br" }, false);
            
            double d = 0.0;
            */


        }
    }

    public static class Globals
    {
        public static int MoveCounter = 1;
        public static string PathToSave = "";
        public static string pathToResources = "";
        public static List<Button> AllButtons = new List<Button>();
        public static int CurrentSquareClicked = -1;
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
        public static int CapturedInEnPessant = -1;
        public static int EnPessantDestination = -1;
        public static Dictionary<string, string> FromBoardToPiecePathes;
        public static bool WaitingForSecondClick = false;
        public static int FirstClickIndex = -1;
        public static List<int> PositionOfPawnToBePromotedAndPiece = null;
        public static List<object> ExitThreadInfo = new List<object>();
        public static string AI = "W";
        
       

    }
    

}

