using System;
using System.Collections.Generic;
using System.Linq;
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

            Validate();

            return true;
        }

        public void Solve()
        {
            while (!_finished)
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        var remaining = SimpleFills(i, j);
                        System.Console.WriteLine(remaining.Count);
                        switch (remaining.Count)
                        {
                            case 1:
                                _table[i, j] = remaining.ToList()[0];
                                Display();
                                break;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:

                                break;
                            default:
                                // Do nothing
                                break;
                        }
                    }
                }
            }
        }

        private HashSet<int> SimpleFills(int i, int j)
        {
            var known = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            if (_table[i, j] == 0)
            {
                return known;
            }
            // check row
            var row = i;
            var column = j;
            for (int k = 0; k < 9; k++)
            {
                if (_table[row, k] == 0) continue;
                known.Remove(_table[row, k]);
            }
            // check column
            for (int l = 0; l < 9; l++)
            {
                if (_table[l, column] == 0) continue;
                known.Remove(_table[l, column]);
            }
            // check square
            if (i < 3)
            {
                row = 0;
            }
            else if (i >= 3 && i < 6)
            {
                row = 3;
            }
            else
            {
                row = 6;
            }
            if (j < 3)
            {
                column = 0;
            }
            else if (j >= 3 && j < 6)
            {
                column = 3;
            }
            else
            {
                column = 6;
            }

            for (int m = row; m < (row + 3); m++)
            {
                for (int n = column; n < (column + 3); n++)
                {
                    if (_table[m, n] == 0) continue;
                    known.Remove(_table[m, n]);
                }
            }
            return known;
        }

        private bool Validate()
        {
            var valSet = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            // Check rows
            for (int i = 0; i < 9; i++)
            {
                var rows = new List<int>();
                var columns = new List<int>();
                for (int j = 0; j < 9; j++)
                {
                    if (_table[i, j] != 0 && rows.Contains(_table[i, j])) return false;
                    if (_table[j, i] != 0 && columns.Contains(_table[j, i])) return false;
                    rows.Add(_table[i, j]);
                    columns.Add(_table[j, i]);
                }
            }

            // Check squares
            var leftSquare = new List<int>();
            var middleSquare = new List<int>();
            var rightSquare = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                if (i % 3 == 0)
                {
                    leftSquare.Clear();
                    middleSquare.Clear();
                    rightSquare.Clear();
                }
                for (int j = 0; j < 3; j++)
                {
                    if (_table[i, j] != 0 && leftSquare.Contains(_table[i, j])) return false;
                    leftSquare.Add(_table[i, j]);
                }
                for (int j = 3; j < 6; j++)
                {
                    if (_table[i, j] != 0 && middleSquare.Contains(_table[i, j])) return false;
                    middleSquare.Add(_table[i, j]);
                }
                for (int j = 6; j < 9; j++)
                {
                    if (_table[i, j] != 0 && rightSquare.Contains(_table[i, j])) return false;
                    rightSquare.Add(_table[i, j]);
                }
            }

            return true;
        }
    }
}