using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SudokuSolver.Objects
{
    public class Puzzle
    {
        private bool _finished { get; set; }
        private bool _isSolvable { get; set; }
        private int[,] _table { get; set; }
        private IDictionary<(int, int), HashSet<int>> _options { get; set; }

        public Puzzle()
        {
            _finished = false;
            _isSolvable = true;
            _table = new int[9, 9];
            _options = new Dictionary<(int, int), HashSet<int>>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    _options[(i, j)] = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                }
            }
        }

        public void Display()
        {
            Console.Clear();
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
            Thread.Sleep(1);
        }

        public void SeeAllOptions(int i, int j)
        {
            var sb = new StringBuilder();
            sb.Append($"[({i},{j})]: ");
            foreach (var value in _options[(i, j)])
            {
                sb.Append(value);
                sb.Append(", ");
            }
            System.Console.WriteLine(sb.ToString());
        }
        private void RemoveOptions(int row, int column)
        {
            if (!_options.TryGetValue((row, column), out var set) || set.Count > 1) return;

            var optionToRemove = set.ToList()[0];
            for (int i = 0; i < 9; i++)
            {
                if (i == row) continue;
                if (_options.TryGetValue((i, column), out var rowSet))
                {
                    rowSet.Remove(optionToRemove);
                }
            }
            for (int i = 0; i < 9; i++)
            {
                if (i == column) continue;
                if (_options.TryGetValue((row, i), out var columnSet))
                {
                    columnSet.Remove(optionToRemove);
                }
            }

            var (rowCorner, columnCorner) = GetSquare_TopLeft(row, column);
            for (int m = rowCorner; m < (rowCorner + 3); m++)
            {
                for (int n = columnCorner; n < (columnCorner + 3); n++)
                {
                    if (m == row && n == column) continue;
                    if (_options.TryGetValue((m, n), out var squareSet) && squareSet.Count > 1)
                    {
                        squareSet.Remove(optionToRemove);
                    }
                }
            }
        }

        public bool LoadTable(string values)
        {
            try
            {
                var strArray = values.Split(',');
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(strArray[i])) continue;

                    var row = i / 9;
                    var column = i % 9;
                    var num = Convert.ToInt32(strArray[i]);
                    if (num == 0)
                    {
                        System.Console.WriteLine("WTF?!");
                    }
                    _table[row, column] = num;
                    _options[(row, column)] = new HashSet<int>() { num };
                    RemoveOptions(row, column);
                }
                Display();
            }
            catch (System.Exception)
            {
                return false;
            }
            return Validate();
        }

        public void Solve()
        {
            while (!_finished)
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        SeeAllOptions(i, j);
                        var remaining = SimpleFills(i, j);
                        // System.Console.WriteLine(remaining.Count);
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
                                // For cases 2-9, we need to run the gauntlet of methods to "logic" a square to fill.
                                // Each square:
                                // Assign all available options to the blank squares and then remove values if they are matched in row or column
                                if (_options.TryGetValue((i, j), out var spaceSet) && spaceSet.Count == 1)
                                {
                                    var found = spaceSet.ToList()[0];
                                    if (found == 0)
                                    {
                                        System.Console.WriteLine("HOW?!");
                                    }
                                    if (_table[i, j] != 0)
                                        _table[i, j] = found;

                                    RemoveOptions(i, j);
                                    Display();
                                }
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
            if (_table[i, j] != 0)
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
            (row, column) = GetSquare_TopLeft(i, j);
            if (row < 0 || column < 0)
            {
                throw new ArgumentException($"GetSquare_TopLeft - IndexOutOfBound: {i}:{j}");
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

        private (int, int) GetSquare_TopLeft(int i, int j)
        {
            if (i < 0 || i > 9 || j < 0 || j > 9)
            {
                return (-1, -1);
            }
            int row, column;
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
            return (row, column);
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