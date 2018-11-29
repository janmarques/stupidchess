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

            var board = new Board();
            PrintBoard(board);
        }

        static void PrintBoard(Board board)
        {
            var rowNumber = 8;
            foreach (var row in board.BoardArray)
            {
                Console.Write(rowNumber--);
                foreach (var square in row)
                {
                    if (square.Piece != null)
                    {
                        Console.Write(square.Piece.Symbol);
                    }
                    else
                    {
                        Console.Write(". ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine(" A B C D E F G H");

        }
    }

    class Game
    {
        public void InitializeBoard()
        {
        }

    }

    class Board
    {
        public Square[][] BoardArray;
        public Board()
        {
            BoardArray = new Square[8][];
            for (int i = 0; i < 8; i++)
            {
                var row = new Square[8];
                for (int j = 0; j < 8; j++)
                {
                    row[j] = new Square();
                }
                BoardArray[i] = row;
            }

            var whiteBackRow = GetBackRow(Color.White);
            var blackBackRow = GetBackRow(Color.Black);
            var whitePawns = GetPawnRow(Color.White);
            var blackPawns = GetPawnRow(Color.Black);
            for (int i = 0; i < 8; i++)
            {
                BoardArray[0][i].Piece = whiteBackRow[i];
                BoardArray[1][i].Piece = whitePawns[i];
                BoardArray[6][i].Piece = blackPawns[i];
                BoardArray[7][i].Piece = blackBackRow[i];
            }
        }
        public Piece[] GetPawnRow(Color color)
        {
            var row = new Piece[8];
            for (int i = 0; i < 8; i++)
            {
                row[i] = new Pawn { Color = color };
            }
            return row;
        }

        public Piece[] GetBackRow(Color color) => new Piece[8]
            {
                new Rook(){Color = color },
                new Knight(){Color = color },
                new Bishop(){Color = color },
                new Queen(){Color = color },
                new King(){Color = color },
                new Bishop(){Color = color },
                new Knight(){Color = color },
                new Rook(){Color = color },
            };


    }

    class Square
    {
        public Piece Piece { get; set; }
    }

    enum Color
    {
        Black, White
    }

    abstract class Piece
    {
        public char Symbol => Color == Color.White ? WhiteSymbol : BlackSymbol;
        public abstract char BlackSymbol { get; }
        public abstract char WhiteSymbol { get; }
        public Color Color { get; set; }
    }

    class King : Piece
    {
        public override char WhiteSymbol => '♔';
        public override char BlackSymbol => '♚';
    }
    class Queen : Piece
    {
        public override char WhiteSymbol => '♕';
        public override char BlackSymbol => '♛';
    }
    class Rook : Piece
    {
        public override char WhiteSymbol => '♖';
        public override char BlackSymbol => '♜';
    }
    class Bishop : Piece
    {
        public override char WhiteSymbol => '♗';
        public override char BlackSymbol => '♝';
    }
    class Knight : Piece
    {
        public override char WhiteSymbol => '♘';
        public override char BlackSymbol => '♞';
    }
    class Pawn : Piece
    {
        public override char WhiteSymbol => '♙';
        public override char BlackSymbol => '♟';
    }
}
