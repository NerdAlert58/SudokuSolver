using System;
using System.Collections.Generic;
using System.Text;

namespace SudokuSolver.Objects
{
    public class Puzzle
    {
        private bool _finished { get; set; }
        private bool _isSolvable { get; set; }
        private int[,] _table { get; set; }

        public Puzzle()
        {
            _finished = false;
            _isSolvable = true;
            _table = new int[9, 9];
        }

        public void Display()
        {
            if (_table == null || _table.Length == 0)
            {
                return;
            }
            var sb = new StringBuilder();
            sb.Append("-------------------------------------\n");
            for (int i = 0; i < 9; i++)
            {
                sb.Append("| ");
                for (int j = 0; j < 9; j++)
                {
                    var val = _table[i, j];
                    if (val == 0)
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(val);
                    }

                    if (j < 8)
                    {
                        sb.Append(" | ");
                    }
                    else
                    {
                        sb.Append(" |\n");
                    }
                }
                sb.Append("-------------------------------------\n");
            }

            System.Console.WriteLine(sb.ToString());
        }

        public bool LoadTable(string values)
        {
            var strArray = values.Split(',');
            for (int i = 0; i < strArray.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(strArray[i])) continue;

                _table[i / 9, i % 9] = Convert.ToInt32(strArray[i]);
            }

            return true;
        }

        private bool Validate()
        {
            var valSet = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            // Check rows
            for (int i = 0; i < 9; i++)
            {
                var found = new List<int>();
                for (int j = 0; j < 9; j++)
                {
                    if (found.Contains(_table[i, j])) return false;
                    found.Add(_table[i, j]);
                }
            }

            // Check columns
            for (int i = 0; i < 9; i++)
            {
                var found = new List<int>();
                for (int j = 0; j < 9; j++)
                {
                    if (found.Contains(_table[j, i])) return false;
                    found.Add(_table[j, i]);
                }
            }

            // Check squares

            return true;
        }
    }
}