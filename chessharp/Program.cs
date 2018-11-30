using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chessharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var turn = 0;
            var game = new Game();
            while (true)
            {
                if (game.GameOver)
                {
                    break;
                }
                Console.WriteLine($"Turn {turn++}");
                var move = game.GetLegalMovesForColor(game.Turn).ToList().RandomElement();
                game.ExecuteMove(move);
                game.NextTurn();
                PrintBoard(game.Board);
                Console.WriteLine(game.LastMove);
                Console.WriteLine($"White captured {string.Join(" ", game.WhiteCaptured)}");
                Console.WriteLine($"Black captured {string.Join(" ", game.BlackCaptured)}");
            }
            Console.ReadLine();
        }

        static void PrintBoard(Board board)
        {
            for (int row = 7; row >= 0; row--)
            {
                Console.Write(row);
                for (int column = 0; column < 8; column++)
                {
                    var square = board.GetSquare(row, column);
                    if (square.Piece != null)
                    {
                        Console.Write(square.Piece.Symbol);
                    }
                    else
                    {
                        //Console.Write($"{row}{column}");
                        Console.Write($". ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine(" A B C D E F G H");
        }
    }

    class Game
    {
        public Board Board { get; set; }
        public Color Turn { get; set; }
        public Move LastMove { get; set; }
        public List<Piece> WhiteCaptured { get; set; }
        public List<Piece> BlackCaptured { get; set; }
        public bool GameOver { get; set; }

        public Game()
        {
            Board = new Board();
            Turn = Color.White;
            WhiteCaptured = new List<Piece>();
            BlackCaptured = new List<Piece>();
        }

        public void NextTurn() => Turn = GetInverseColor(Turn);

        public IEnumerable<Move> GetLegalMovesForColor(Color color)
        {
            var mySquares = Board.GetSquaresForColor(color);
            foreach (var mySquare in mySquares)
            {
                foreach (var destination in Board.GetLegalDestinationsForSquare(mySquare))
                {
                    yield return new Move { From = mySquare, To = destination };
                }
            }
        }

        public void ExecuteMove(Move move)
        {
            LastMove = move.Copy();

            var from = Board.GetSquare(move.From.Row, move.From.Column);
            var to = Board.GetSquare(move.To.Row, move.To.Column);

            var capturedPiece = to.Piece;

            to.Piece = from.Piece;
            from.Piece = null;

            if (capturedPiece != null)
            {
                if (capturedPiece.Color == Color.White)
                {
                    BlackCaptured.Add(capturedPiece);
                }
                else
                {
                    WhiteCaptured.Add(capturedPiece);
                }
                if (capturedPiece is King)
                {
                    GameOver = true;
                }
            }

        }

        public bool IsCheck(Color color)
        {
            var kingSquare = Board.BoardArray.Single(x => x.Piece is King && x.Piece.Color == color);
            var opponentMoves = GetLegalMovesForColor(GetInverseColor(color));
            return opponentMoves.Any(x => x.To == kingSquare);
        }

        private static Color GetInverseColor(Color color) => color == Color.Black ? Color.White : Color.Black;
    }

    class Move
    {
        public Square From { get; set; }
        public Square To { get; set; }
        public override string ToString() => $"{From}->{To}";

        public Move Copy() => new Move { From = From.Copy(), To = To.Copy() };
    }

    class Board
    {
        public List<Square> BoardArray;
        public IEnumerable<Square> GetSquaresForColor(Color color) => BoardArray.Where(x => x.Piece?.Color == color);

        public IEnumerable<Square> GetLegalDestinationsForSquare(Square square)
        {
            var piece = square.Piece;
            if (piece is Pawn pawn)
            {
                // TODO en passant
                // TODO promotion
                var direction = pawn.Color == Color.White ? 1 : -1;
                var startRow = pawn.Color == Color.White ? 1 : 6;
                var promoteRow = pawn.Color == Color.White ? 7 : 0;

                var oneForward = GetSquare(square.Row + direction, square.Column);
                if (oneForward != null && oneForward.Piece == null)
                {
                    yield return oneForward;
                }

                if (square.Row == startRow)
                {
                    var twoForward = GetSquare(square.Row + 2 * direction, square.Column);
                    if (twoForward.Piece == null)
                    {
                        yield return twoForward;
                    }
                }

                var captureLeft = GetSquare(square.Row + direction, square.Column + 1);
                if (captureLeft?.Piece != null && captureLeft.Piece.Color != pawn.Color)
                {
                    yield return captureLeft;
                }

                var captureRight = GetSquare(square.Row + direction, square.Column - 1);
                if (captureRight?.Piece != null && captureRight.Piece.Color != pawn.Color)
                {
                    yield return captureRight;
                }
            }
            if (piece is IRook rook)
            {
                var leftStopped = false;
                var rightStopped = false;
                var upStopped = false;
                var downStopped = false;
                for (int i = 1; i < 8; i++)
                {
                    if (!leftStopped)
                    {
                        var left = GetSquare(square.Row, square.Column - i);
                        if (left == null)
                        {
                            leftStopped = true;
                        }
                        else if (left.Piece == null)
                        {
                            yield return left;
                        }
                        else
                        {
                            leftStopped = true;
                            if (left.Piece.Color == rook.Color)
                            {
                                // cant move here
                            }
                            else
                            {
                                yield return left;
                            }
                        }
                    }

                    if (!rightStopped)
                    {
                        var right = GetSquare(square.Row, square.Column + i);
                        if (right == null)
                        {
                            rightStopped = true;
                        }
                        else if (right.Piece == null)
                        {
                            yield return right;
                        }
                        else
                        {
                            rightStopped = true;
                            if (right.Piece.Color == rook.Color)
                            {
                                // cant move here
                            }
                            else
                            {
                                yield return right;
                            }
                        }
                    }

                    if (!upStopped)
                    {
                        var up = GetSquare(square.Row + i, square.Column);
                        if (up == null)
                        {
                            upStopped = true;
                        }
                        else if (up.Piece == null)
                        {
                            yield return up;
                        }
                        else
                        {
                            upStopped = true;
                            if (up.Piece.Color == rook.Color)
                            {
                                // cant move here
                            }
                            else
                            {
                                yield return up;
                            }
                        }
                    }

                    if (!downStopped)
                    {
                        var down = GetSquare(square.Row - i, square.Column);
                        if (down == null)
                        {
                            downStopped = true;
                        }
                        else if (down.Piece == null)
                        {
                            yield return down;
                        }
                        else
                        {
                            downStopped = true;
                            if (down.Piece.Color == rook.Color)
                            {
                                // cant move here
                            }
                            else
                            {
                                yield return down;
                            }
                        }
                    }

                }
            }
            if (piece is IBishop bishop)
            {
                var northEastStopped = false;
                var northWestStopped = false;
                var southEastStopped = false;
                var southWestStopped = false;
                for (int i = 1; i < 8; i++)
                {
                    if (!northEastStopped)
                    {
                        var northEast = GetSquare(square.Row + i, square.Column + i);
                        if (northEast == null)
                        {
                            northEastStopped = true;
                        }
                        else if (northEast.Piece == null)
                        {
                            yield return northEast;
                        }
                        else
                        {
                            northEastStopped = true;
                            if (northEast.Piece.Color == bishop.Color)
                            {
                                // cant move here
                            }
                            else
                            {
                                yield return northEast;
                            }
                        }
                    }

                    if (!northWestStopped)
                    {
                        var northWest = GetSquare(square.Row + i, square.Column - i);
                        if (northWest == null)
                        {
                            northWestStopped = true;
                        }
                        else if (northWest.Piece == null)
                        {
                            yield return northWest;
                        }
                        else
                        {
                            northWestStopped = true;
                            if (northWest.Piece.Color == bishop.Color)
                            {
                                // cant move here
                            }
                            else
                            {
                                yield return northWest;
                            }
                        }
                    }

                    if (!southEastStopped)
                    {
                        var southEast = GetSquare(square.Row - i, square.Column + i);
                        if (southEast == null)
                        {
                            southEastStopped = true;
                        }
                        else if (southEast.Piece == null)
                        {
                            yield return southEast;
                        }
                        else
                        {
                            southEastStopped = true;
                            if (southEast.Piece.Color == bishop.Color)
                            {
                                // cant move here
                            }
                            else
                            {
                                yield return southEast;
                            }
                        }
                    }

                    if (!southWestStopped)
                    {
                        var southWest = GetSquare(square.Row - i, square.Column - i);
                        if (southWest == null)
                        {
                            southWestStopped = true;
                        }
                        else if (southWest.Piece == null)
                        {
                            yield return southWest;
                        }
                        else
                        {
                            southWestStopped = true;
                            if (southWest.Piece.Color == bishop.Color)
                            {
                                // cant move here
                            }
                            else
                            {
                                yield return southWest;
                            }
                        }
                    }

                }
            }
            if (piece is Knight knight)
            {
                var destinations = new List<Square>
                {
                    GetSquare(square.Row+2, square.Column+1),
                    GetSquare(square.Row+2, square.Column-1),
                    GetSquare(square.Row-2, square.Column+1),
                    GetSquare(square.Row-2, square.Column-1),
                    GetSquare(square.Row+1, square.Column+2),
                    GetSquare(square.Row+1, square.Column-2),
                    GetSquare(square.Row-1, square.Column+2),
                    GetSquare(square.Row-1, square.Column-2),
                };

                foreach (var destination in destinations)
                {
                    if (destination == null)
                    {
                        continue;
                    }
                    else if (destination.Piece == null)
                    {
                        yield return destination;
                    }
                    else if (destination.Piece.Color != knight.Color)
                    {
                        yield return destination;
                    }
                }
            }
            if (piece is King king)
            {
                var destinations = new List<Square>
                {
                    GetSquare(square.Row+1, square.Column+1),
                    GetSquare(square.Row+1, square.Column-1),
                    GetSquare(square.Row+1, square.Column),
                    GetSquare(square.Row-1, square.Column+1),
                    GetSquare(square.Row-1, square.Column-1),
                    GetSquare(square.Row-1, square.Column),
                    GetSquare(square.Row, square.Column-1),
                    GetSquare(square.Row, square.Column+1),
                };

                foreach (var destination in destinations)
                {
                    if (destination == null)
                    {
                        continue;
                    }
                    else if (destination.Piece == null)
                    {
                        yield return destination;
                    }
                    else if (destination.Piece.Color != king.Color)
                    {
                        yield return destination;
                    }
                }
            }
        }

        public Board()
        {
            BoardArray = new List<Square>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    BoardArray.Add(new Square { Row = i, Column = j });
                }
            }

            var whiteBackRow = GetBackRow(Color.White).ToList();
            var blackBackRow = GetBackRow(Color.Black).ToList();
            var whitePawns = GetPawnRow(Color.White).ToList();
            var blackPawns = GetPawnRow(Color.Black).ToList();
            for (int i = 0; i < 8; i++)
            {
                GetSquare(0, i).Piece = whiteBackRow[i];
                GetSquare(1, i).Piece = whitePawns[i];
                GetSquare(6, i).Piece = blackPawns[i];
                GetSquare(7, i).Piece = blackBackRow[i];
            }
        }

        public Square GetSquare(int row, int column) => BoardArray.SingleOrDefault(a => a.Row == row && a.Column == column);

        public IEnumerable<Piece> GetPawnRow(Color color)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return new Pawn { Color = color };
            }
        }

        public IEnumerable<Piece> GetBackRow(Color color)
        {
            yield return new Rook() { Color = color };
            yield return new Knight() { Color = color };
            yield return new Bishop() { Color = color };
            yield return new Queen() { Color = color };
            yield return new King() { Color = color };
            yield return new Bishop() { Color = color };
            yield return new Knight() { Color = color };
            yield return new Rook() { Color = color };
        }
    }

    class Square
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public Piece Piece { get; set; }
        public override string ToString() => $"{Row}{(char)(65 + Column)},{Piece}";

        public Square Copy() => new Square { Column = Column, Row = Row, Piece = Piece?.Copy() };
    }

    enum Color
    {
        Black, White
    }

    interface IRook : IPiece { }
    interface IBishop : IPiece { }
    interface IPiece
    {
        Color Color { get; set; }
    }
    abstract class Piece : IPiece
    {
        public char Symbol => Color == Color.White ? WhiteSymbol : BlackSymbol;
        public abstract char BlackSymbol { get; }
        public abstract char WhiteSymbol { get; }
        public Color Color { get; set; }
        public override string ToString() => Symbol.ToString();

        public abstract Piece Copy();
    }

    class King : Piece
    {
        public override char WhiteSymbol => '♔';
        public override char BlackSymbol => '♚';

        public override Piece Copy() => new King { Color = Color };
    }
    class Queen : Piece, IRook, IBishop
    {
        public override char WhiteSymbol => '♕';
        public override char BlackSymbol => '♛';
        public override Piece Copy() => new Queen { Color = Color };
    }
    class Rook : Piece, IRook
    {
        public override char WhiteSymbol => '♖';
        public override char BlackSymbol => '♜';
        public override Piece Copy() => new Rook { Color = Color };
    }
    class Bishop : Piece, IBishop
    {
        public override char WhiteSymbol => '♗';
        public override char BlackSymbol => '♝';
        public override Piece Copy() => new Bishop { Color = Color };
    }
    class Knight : Piece
    {
        public override char WhiteSymbol => '♘';
        public override char BlackSymbol => '♞';
        public override Piece Copy() => new Knight { Color = Color };
    }
    class Pawn : Piece
    {
        public override char WhiteSymbol => '♙';
        public override char BlackSymbol => '♟';
        public override Piece Copy() => new Pawn { Color = Color };
    }

    public static class CollectionExtension
    {
        private static Random rng = new Random();

        public static T RandomElement<T>(this IList<T> list)
        {
            return list[rng.Next(list.Count)];
        }
    }
}
