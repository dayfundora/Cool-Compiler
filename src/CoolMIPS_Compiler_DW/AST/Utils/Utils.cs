using System;
using System.Collections.Generic;
using System.Text;

namespace CoolMIPS_Compiler_DW.AST
{
    public class Coord
    {
        int row, col;
        public Coord(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
        public int Row { get { return row; } private set { row = value; } }
        public int Col { get { return col; } private set { col = value; } }

    }
    public class Error
    {
        string msg;
        Coord c;
        public Error(string msg, Coord c)
        {
            this.msg = msg;
            this.c = c;
        }
        public string Message { get { return msg; } private set { msg = value; } }
        public Coord ErrCoord { get { return c; } private set { c = value; } }

        public override string ToString()
        {
            return "[" + ErrCoord.Row + ":" + ErrCoord.Col + "] -> " + Message;
        }
    }
}
