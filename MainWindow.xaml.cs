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
            
            

            
            GetAllButtonElements();
            SortButtons();
            Globals.FromBoardToPiecePathes = PopulateADictionary();
            Globals.pathToResources = GetPathToResources();
            GameWindow.Icon = new BitmapImage(new Uri(Globals.pathToResources + "\\ChessIcon.png"));
            Board.Source = new BitmapImage(new Uri(Globals.pathToResources + "\\Board.png"));
            SaveGameImage.Source = new BitmapImage(new Uri(Globals.pathToResources + "\\SaveGameIcon.png"));
            SaveGameImage.MouseDown += SaveGameImage_MouseDown;
            foreach (Button btn in Globals.AllButtons)
            {
                btn.Click += UniversalSquareClickEventHandle;
            }
            
            /*foreach(string item in Globals.PathsToPieces)
            {
                comboBox.Items.Add(item);
            }
            */
            InitializeBoard();
            DisplayBoardOnInterface();
            Globals.WhitesTurn = true;
                
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
        public static string CheckGameEndConditions(bool whitesTurn)
        {
            List<int> PositionsOfWhiteFigures = new List<int>();
            List<int> PositionsOfBlackFigures = new List<int>();
            List<int> OutList = new List<int>();


            for (int i = 0; i < Globals.Board.Count; i++)
            {
                char temp = Globals.Board[i][0];
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
                    LegalMovesForScopedPiece = IndexesOfPossibleMoves(Globals.Board[PositionsOfWhiteFigures[i]], PositionsOfWhiteFigures[i]);
                    if (LegalMovesForScopedPiece.Count > 0)
                    {
                        AtLeastOneLegalMove = true;
                        break;
                    }
                }
                if (AtLeastOneLegalMove == false)
                {
                    List<int> WhatPiecesCheckingKing = KingInCheckAndByWhichFigures(Globals.Board);
                    if (WhatPiecesCheckingKing.Count == 0)
                    {
                        //DRAW
                        return "Stalemate";
                    }
                    else
                    {
                        //WHITE CHECKMATED
                        return "White king checkmated";
                    }
                }
            }
            else
            {
                bool AtLeastOneLegalMove = false;
                List<int> LegalMovesForScopedPiece = new List<int>();
                for (int i = 0; i < PositionsOfBlackFigures.Count; i++)
                {
                    LegalMovesForScopedPiece = IndexesOfPossibleMoves(Globals.Board[PositionsOfBlackFigures[i]], PositionsOfBlackFigures[i]);
                    if (LegalMovesForScopedPiece.Count > 0)
                    {
                        AtLeastOneLegalMove = true;
                        break;
                    }
                }
                if (AtLeastOneLegalMove == false)
                {
                    List<int> WhatPiecesCheckingKing = KingInCheckAndByWhichFigures(Globals.Board);
                    if (WhatPiecesCheckingKing.Count == 0)
                    {
                        //DRAW
                        return "Stalemate";
                    }
                    else
                    {
                        //BLACK CHECKMATED
                        return "Black king checkmated";
                    }
                }
            }
            return "";
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
                }
            }


            /*Globals.AllButtons[index].Content = new Image
            {
                Source = new BitmapImage(new Uri(Globals.pathToResources + pathToPiece)),
                VerticalAlignment = VerticalAlignment.Center

            };
            */
            
        }
        public static bool CanProceedWithSecondClick(int startIndex,int endIndex) 
        {
            List<int> PossibleMovesOfSelectedPiece = IndexesOfPossibleMoves(Globals.Board[startIndex], startIndex);
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
        public static List<int> IndexesOfPossibleMoves(string piece, int position)
        {

            List<int> OutList = new List<int>();
            List<string> bb = new List<string>();
            foreach (string item in Globals.Board)
            {
                bb.Add(item);
            }
            OutList = IndexesOfPossibleMovesKing(piece, position, bb);
            List<string> CopyOf = new List<string>();

            List<int> PiecesCheckingKing = new List<int>();
            List<int> PossibleMoves = new List<int>();
            foreach (int item in OutList)
            {
                PossibleMoves.Add(item);
            }
            for (int i = 0; i < PossibleMoves.Count; i++)
            {
                foreach (string item in Globals.Board)
                {
                    CopyOf.Add(item);
                }
                CopyOf = MovePieceLocal(ConvertAbsoluteToBoardNotation(position), ConvertAbsoluteToBoardNotation(PossibleMoves[i]), CopyOf);
                PiecesCheckingKing = KingInCheckAndByWhichFigures(CopyOf);
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
        public static List<int> IndexesOfPossibleMovesKing(string piece, int position, List<string> bb)
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
            foreach (string item in bb)
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
                    Match matchForDestination = Regex.Match(lastMove, @"^\d: Bp.7 => (?<capture>.5)$");
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
                    Match matchForDestination = Regex.Match(lastMove, @"^\d: Wp.2 => (?<capture>.4)$");
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
            List<int> IndexesOfPossibleMovess = IndexesOfPossibleMoves(piece, AbsoluteInitialPos);

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
                                    string PromotedTo = PromotePawn(DestinationAbsolute, color);
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
                                    string PromotedTo = PromotePawn(DestinationAbsolute, color);
                                    Globals.MoveRecord[Globals.MoveRecord.Count - 1] += $"(Promoted to {PromotedTo.ToUpper()})";

                                }
                            }
                        }
                    }
                }
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
                Copu[DestinationAbsolute] = piece;
                Copu[AbsoluteInitialPos] = Empty;
            }
            else
            {
                Copu[DestinationAbsolute] = piece;
                Copu[AbsoluteInitialPos] = Empty;
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
                        PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                        if (PiecesCheckingKing.Count == 0)
                        {


                            ScopedBoard = MovePieceLocal("E1", "F1", ScopedBoard);
                            PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                            if (PiecesCheckingKing.Count == 0)
                            {
                                ScopedBoard = MovePieceLocal("F1", "G1", ScopedBoard);
                                PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
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
                        PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                        if (PiecesCheckingKing.Count == 0)
                        {
                            ScopedBoard = MovePieceLocal("E1", "D1", ScopedBoard);
                            PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                            if (PiecesCheckingKing.Count == 0)
                            {
                                ScopedBoard = MovePieceLocal("D1", "C1", ScopedBoard);
                                PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                                if (PiecesCheckingKing.Count == 0)
                                {
                                    ScopedBoard = MovePieceLocal("C1", "B1", ScopedBoard);
                                    PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
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
                        PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                        if (PiecesCheckingKing.Count == 0)
                        {
                            ScopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(GetAbolutePosition(column, row)), "F8", ScopedBoard);
                            PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                            if (PiecesCheckingKing.Count == 0)
                            {
                                ScopedBoard = MovePieceLocal("F8", "G8", ScopedBoard);
                                PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
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
                        PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                        if (PiecesCheckingKing.Count == 0)
                        {
                            ScopedBoard = MovePieceLocal(ConvertAbsoluteToBoardNotation(GetAbolutePosition(column, row)), "D8", ScopedBoard);
                            PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                            if (PiecesCheckingKing.Count == 0)
                            {
                                ScopedBoard = MovePieceLocal("D8", "C8", ScopedBoard);
                                PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
                                if (PiecesCheckingKing.Count == 0)
                                {
                                    ScopedBoard = MovePieceLocal("C8", "B8", ScopedBoard);
                                    PiecesCheckingKing = KingInCheckAndByWhichFigures(ScopedBoard);
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

            return ForOut;
        }
        public static List<int> KingInCheckAndByWhichFigures(List<string> Board)
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

            if (Globals.WhitesTurn == true)
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
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                Globals.Board.Add(Empty);
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
        public static string PromotePawn(int position, string color)
        {
            //Console.WriteLine($"To which piece do you wish to promote that pawn? (r|q|b|n): ");
            string ans = "";
            bool close = false;
            while (close == false)
            {
                if (Regex.IsMatch(ans, @"[rqbn]") == true)
                {
                    close = true;
                    switch (ans)
                    {
                        case "q":
                            Globals.Board[position] = $"{color}{Queen}";
                            break;
                        case "r":
                            Globals.Board[position] = $"{color}{Rook}";
                            break;
                        case "b":
                            Globals.Board[position] = $"{color}{Bishop}";
                            break;
                        case "n":
                            Globals.Board[position] = $"{color}{Knight}";
                            break;
                    }

                }
                else
                {

                }
            }
            return ans;
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
        



    }
    

}

