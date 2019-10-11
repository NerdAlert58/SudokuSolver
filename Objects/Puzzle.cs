using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SudokuSolver.Utilities;

namespace SudokuSolver.Objects
{
    public class Puzzle
    {
        private bool _finished { get; set; }
        private bool _isSolvable { get; set; }
        private int[,] _table { get; set; }
        private IList<string> _testPuzzles { get; set; }
        private IDictionary<(int, int), HashSet<int>> _options { get; set; }
        private IDictionary<int, HashSet<int>> _vertical { get; set; }
        private IDictionary<int, HashSet<int>> _horizontal { get; set; }
        private IDictionary<int, HashSet<int>> _squares { get; set; }
        private IList<Event> _events { get; set; }

        public Puzzle()
        {
            _testPuzzles = new List<string>();
            ResetPuzzle();
        }

        public void ResetPuzzle()
        {
            _finished = false;
            _isSolvable = true;
            _table = new int[9, 9];
            _options = new Dictionary<(int, int), HashSet<int>>();
            _vertical = new Dictionary<int, HashSet<int>>();
            _horizontal = new Dictionary<int, HashSet<int>>();
            _squares = new Dictionary<int, HashSet<int>>();
            _events = new List<Event>();
            for (int i = 0; i < 9; i++)
            {
                _vertical[i] = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                _horizontal[i] = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                _squares[i] = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                for (int j = 0; j < 9; j++)
                {
                    _options[(i, j)] = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                }
            }
        }

        public void Display()
        {
            // Console.Clear();
            if (_table == null || _table.Length == 0)
            {
                return;
            }
            var sb = new StringBuilder();
            sb.Append("=======================================\n");
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
                        if ((j + 1) % 3 == 0)
                        {
                            sb.Append(" || ");
                        }
                        else
                        {
                            sb.Append(" | ");
                        }
                    }
                    else
                    {
                        sb.Append(" |\n");
                    }
                }
                if ((i + 1) % 3 == 0)
                {
                    sb.Append("=======================================\n");
                }
                else
                {
                    sb.Append("---------------------------------------\n");
                }
            }

            System.Console.WriteLine(sb.ToString());
            // Thread.Sleep(1);
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
        ///
        /// <summary>
        /// Takes a single cell's coordinates.  Removing the cell's value from all other cells in the row, column and square.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        private void UpdateOptions(int row, int column)
        {
            if (!_options.TryGetValue((row, column), out var set) || set.Count > 1) return;

            var optionToRemove = set.ToList()[0];
            if (optionToRemove == 0)
            {
                System.Console.WriteLine("This should never happen! 0 should never have been added to any set");
                System.Console.WriteLine("Stop here.");
            }
            for (int i = 0; i < 9; i++)
            {
                if (i == row) continue;
                RemoveFromOptions(i, column, optionToRemove);
            }
            for (int i = 0; i < 9; i++)
            {
                if (i == column) continue;
                RemoveFromOptions(row, i, optionToRemove);
            }

            var (rowCorner, columnCorner) = GetSquare_TopLeft(row, column);
            for (int m = rowCorner; m < (rowCorner + 3); m++)
            {
                for (int n = columnCorner; n < (columnCorner + 3); n++)
                {
                    if (m == row && n == column) continue;
                    RemoveFromOptions(m, n, optionToRemove);
                }
            }
        }

        private void RemoveFromOptions(int row, int column, int numberToRemove)
        {
            if (_options.TryGetValue((row, column), out var set) && set.Count > 1)
            {
                set.Remove(numberToRemove);
                // UpdateOptions(row, column);
            }
        }

        public string[] ConvertPuzzleFromCSV(string values)
        {
            return values.Split(',');
        }
        public bool LoadTable(string values)
        {
            if (string.IsNullOrWhiteSpace(values)) return false;
            string[] strArray;
            if (values.Contains(","))
            {
                strArray = ConvertPuzzleFromCSV(values);
            }
            else
            {
                strArray = values.ToCharArray().Select(c => c.ToString()).ToArray();
            }
            try
            {
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (string.Equals(strArray[i], "0") || string.IsNullOrWhiteSpace(strArray[i])) continue;

                    var row = i / 9;
                    var column = i % 9;
                    var num = Convert.ToInt32(strArray[i]);
                    _table[row, column] = num;
                    _options[(row, column)] = new HashSet<int>() { num };
                    if (_horizontal.TryGetValue(row, out var columnSet) && columnSet.Contains(num))
                    {
                        columnSet.Remove(num);
                    }
                    if (_vertical.TryGetValue(column, out var verticalSet) && verticalSet.Contains(num))
                    {
                        verticalSet.Remove(num);
                    }

                    var squarePosition = GetSquarePosition(row, column);
                    if (_squares.TryGetValue(squarePosition, out var squareSet) && squareSet.Contains(num))
                    {
                        squareSet.Remove(num);
                    }
                    UpdateOptions(row, column);
                }
            }
            catch (System.Exception)
            {
                return false;
            }
            return Validate();
        }

        public void RunTheGauntlet()
        {
            var good = 0;
            var bad = 0;
            for (int i = 0; i < _testPuzzles.Count; i++)
            {
                var values = _testPuzzles[i];
                ResetPuzzle();
                if (LoadTable(values))
                {
                    if (Solve())
                    {
                        good += 1;
                        System.Console.WriteLine($"{values} YES! good - {good} bad - {bad}");
                    }
                    else
                    {
                        bad += 1;
                        System.Console.WriteLine($"{values} NO! good - {good} bad - {bad}");
                    }
                }
                else
                {
                    System.Console.WriteLine(values);
                    System.Console.WriteLine("Puzzle Failed!!!");
                }

            }
        }
        public void LoadTestPuzzles()
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@"TestPuzzles.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                {
                    _testPuzzles.Add(line);
                }
            }
        }

        private int GetSquarePosition(int row, int column)
        {
            var (x, y) = GetSquare_TopLeft(row, column);

            return x + (y / 3);
        }

        /// <summary>
        /// Kicks off the process of attempting to solve an incomplete Sudoku puzzle.
        /// </summary>
        public bool Solve()
        {
            // Display();
            var rootStopWatch = new Stopwatch();
            rootStopWatch.Start();
            var successful = false;
            int maxLoops = 10000;
            int counter = 0;
            while (!_finished)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                //Do Stuff
                for (int i = 0; i < 9; i++)
                {
                    if (_finished) break;
                    for (int j = 0; j < 9; j++)
                    {
                        if (_finished) break;
                        // SeeAllOptions(i, j);
                        var remaining = SimpleFills(i, j);
                        switch (remaining.Count)
                        {
                            case 1:
                                AddNewValue(i, j, remaining.ToList()[0], "SimpleFill");
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
                                    AddNewValue(i, j, found, "NoOtherOption");
                                }
                                break;
                            default:
                                // Do nothing
                                var missing = new List<(int, int)>();
                                for (int x = 0; x < 9; x++)
                                {
                                    for (int y = 0; y < 9; y++)
                                    {
                                        if (_table[x, y] == 0)
                                        {
                                            missing.Add((x, y));
                                        }
                                    }
                                }
                                if (missing.Count == 0)
                                {
                                    _finished = true;
                                    // Display();
                                    // System.Console.WriteLine("Finished!!!");
                                    successful = true;
                                }
                                break;
                        }
                    }
                }
                TestByNumbers();
                // TestByRow();
                while (TestByCrossReference())
                {

                }

                TestLastAvailableInRCS();

                TestCrissCross_MultipleSolutions();

                stopWatch.Stop();
                var ts = stopWatch.ElapsedMilliseconds;
                // System.Console.WriteLine($"Loop time: {ts} ms.");

                counter++;
                if (counter >= maxLoops)
                {
                    _finished = true;
                    // System.Console.WriteLine("Incomplete!!!");
                    Display();
                }
            }
            rootStopWatch.Stop();
            var rootSpan = rootStopWatch.ElapsedMilliseconds;
            System.Console.WriteLine($"Total time: {rootSpan} ms.");
            return successful;
        }

        /// <summary>
        /// Only proper method to add a found value to the _table variable.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="number"></param>
        private void AddNewValue(int row, int column, int number, string method)
        {
            if (_table[row, column] != 0) return;

            if (number == 0)
            {
                System.Console.WriteLine($"This is where we broke! MethodName: {method}");
                Display();
                System.Console.WriteLine("Stop here.");
            }

            _table[row, column] = number;
            _options[(row, column)] = new HashSet<int>() { number };
            if (_horizontal.TryGetValue(row, out var columnSet) && columnSet.Contains(number))
            {
                columnSet.Remove(number);
            }
            if (_vertical.TryGetValue(column, out var verticalSet) && verticalSet.Contains(number))
            {
                verticalSet.Remove(number);
            }

            var squarePosition = GetSquarePosition(row, column);
            if (_squares.TryGetValue(squarePosition, out var squareSet) && squareSet.Contains(number))
            {
                squareSet.Remove(number);
            }

            if (!Validate())
            {
                System.Console.WriteLine($"This is where we broke! MethodName: {method}");
                Display();
                System.Console.WriteLine("Stop here.");
            }
            UpdateOptions(row, column);
            AddEvent(method);
            // System.Console.WriteLine($"MethodName: {method} Value: {number} ({row},{column})");
            // Display();
            // System.Console.WriteLine($"Method used: {method} row: {row} column: {column} value: {number}");
        }

        private void AddEvent(string method)
        {
            var id = _events.Count();
            var options = new Dictionary<(int, int), HashSet<int>>();
            foreach (var key in _options.Keys)
            {
                options[key] = new HashSet<int>(_options[key]);
            }
            _events.Add(new Event()
            {
                Id = id,
                Table = (int[,])_table.Clone(),
                Options = options,
                MethodUsed = method
            });
        }

        /// <summary>
        /// Tests every cell that does not have a value yet.
        /// Uses the vertical, horizontal and square sets of remaining values to intersect
        /// </summary>
        private bool TestByCrossReference()
        {
            var hadSuccess = false;
            for (int row = 0; row < 9; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    if (_table[row, column] != 0) continue;

                    if (!_horizontal.TryGetValue(row, out var horizontal))
                    {
                        throw new Exception("Invalid Value in TestByCrossReference.horizontal");
                    }
                    if (!_vertical.TryGetValue(column, out var vertical))
                    {
                        throw new Exception("Invalid Value in TestByCrossReference.veritcal");
                    }
                    if (!_squares.TryGetValue(GetSquarePosition(row, column), out var square))
                    {
                        throw new Exception("Invalid Value in TestByCrossReference.square");
                    }

                    // Only 1 value remaining means it must go here.
                    if (vertical.Count == 1)
                    {
                        AddNewValue(row, column, vertical.ToList()[0], "TestByCrossReference - Vertical");
                        hadSuccess = true;
                        continue;
                    }
                    if (horizontal.Count == 1)
                    {
                        AddNewValue(row, column, horizontal.ToList()[0], "TestByCrossReference - Horizontal");
                        hadSuccess = true;
                        continue;
                    }
                    if (square.Count == 1)
                    {
                        AddNewValue(row, column, square.ToList()[0], "TestByCrossReference - Square");
                        hadSuccess = true;
                        continue;
                    }

                    if (!_options.TryGetValue((row, column), out var set) && set.Count > 0) continue;

                    var possibles = new HashSet<int>(set);
                    possibles.IntersectWith(vertical);
                    possibles.IntersectWith(horizontal);
                    possibles.IntersectWith(square);

                    if (possibles.Count == 1)
                    {
                        AddNewValue(row, column, possibles.ToList()[0], "TestByCrossReference - Intersection");
                        hadSuccess = true;
                    }

                }
            }
            return hadSuccess;
        }

        /// <summary>
        /// Cycles through all rows, then all columns, then all squares searching for all remaining empty spot. 
        /// Then it will iterate over one RCS options, counting how many times that value is possible.
        /// If that count is 1 -> we have found a value to insert.
        /// </summary>
        private void TestLastAvailableInRCS()
        {
            for (int i = 0; i < 9; i++)
            {
                if (!_horizontal.TryGetValue(i, out var rowSet)) return;
                if (!_vertical.TryGetValue(i, out var columnSet)) return;
                if (!_squares.TryGetValue(i, out var squareSet)) return;

                // Row Cycle
                foreach (var available in rowSet)
                {
                    var possibles = new Dictionary<(int, int), HashSet<int>>();
                    for (int number = 0; number < 9; number++)
                    {
                        if (!_options.TryGetValue((i, number), out var options)) return;
                        if (options.Contains(available))
                        {
                            possibles[(i, number)] = new HashSet<int>(options);
                        }
                    }
                    if (possibles.Count == 1)
                    {
                        var key = possibles.Keys.First();
                        AddNewValue(key.Item1, key.Item2, available, nameof(TestLastAvailableInRCS) + "_Row");
                        return;
                    }
                }

                // Column Cycle
                foreach (var available in columnSet)
                {
                    var possibles = new Dictionary<(int, int), HashSet<int>>();
                    for (int number = 0; number < 9; number++)
                    {
                        if (!_options.TryGetValue((number, i), out var options)) return;
                        if (options.Contains(available))
                        {
                            possibles[(number, i)] = new HashSet<int>(options);
                        }
                    }
                    if (possibles.Count == 1)
                    {
                        var key = possibles.Keys.First();
                        AddNewValue(key.Item1, key.Item2, available, nameof(TestLastAvailableInRCS) + "_Column");
                        return;
                    }
                }
                // Square Cycle
                var (row, column) = GetSquare_TopLeft(i);
                if (row < 0 || column < 0)
                {
                    throw new ArgumentException($"GetSquare_TopLeft - IndexOutOfBound: {i}");
                }

                foreach (var available in squareSet)
                {
                    var possibles = new Dictionary<(int, int), HashSet<int>>();
                    for (int m = row; m < (row + 3); m++)
                    {
                        for (int n = column; n < (column + 3); n++)
                        {
                            if (!_options.TryGetValue((m, n), out var options)) return;
                            if (options.Contains(available))
                            {
                                possibles[(m, n)] = new HashSet<int>(options);
                            }
                        }
                    }
                    if (possibles.Count == 1)
                    {
                        var key = possibles.Keys.First();
                        AddNewValue(key.Item1, key.Item2, available, nameof(TestLastAvailableInRCS) + "_Square");
                        return;
                    }
                }
            }
        }
        private void TestByRow()
        {
            // var vertical = new Dictionary<int, HashSet<int>>();
            // var horizontal = new Dictionary<int, HashSet<int>>();
            // var squares = new Dictionary<int, HashSet<int>>();
            // for (int i = 1; i < 10; i++)
            // {
            //     vertical[i] = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            //     horizontal[i] = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            //     squares[i] = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            // }
            // var known = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            // for (int checkNum = 1; checkNum < 10; checkNum++)
            // {
            //     var found = new List<(int, int, int)>();
            //     for (int column = 0; column < 9; column++)
            //     {
            //         for (int row = 0; row < 9; row++)
            //         {
            //             if (_options.TryGetValue((row, column), out var set))
            //             {

            //             }



            //             // Do I need to loop over the rows again so I can test them?  I'm thinking yes...  O.o
            //         }
            //     }
            // }
        }

        /// <summary>
        /// Cycles through the range 1-9, cross checking to see if the current value is the ONLY remaining option for the current 3x3 square.
        /// This is comparing against the available options remaining in all cells of the 3x3 square.  If we only find a single instance of a value available,
        /// that means this is the only place within the current 3x3 square where the number could be.
        /// </summary>
        private bool TestByNumbers()
        {
            // For every square, there are 3 rows and 3 columns that should be cross referenced 
            // to eliminate positions where a number may possibly belong in multiples.

            var hadSuccess = false;
            int rowStart, rowStop, columnStart, columnStop;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    rowStart = x * 3;
                    rowStop = rowStart + 3;
                    columnStart = y * 3;
                    columnStop = columnStart + 3;
                    for (int checkNum = 1; checkNum < 10; checkNum++)
                    {
                        var found = new List<(int, int, int)>();
                        for (int i = rowStart; i < rowStop; i++)
                        {
                            for (int j = columnStart; j < columnStop; j++)
                            {
                                if (_options.TryGetValue((i, j), out var rowSet))
                                {
                                    if (rowSet.Contains(checkNum))
                                    {
                                        found.Add((i, j, checkNum));
                                    }
                                }
                            }
                        }
                        if (found.Count == 1)
                        {
                            var (rowPos, columnPos, num) = found[0];
                            hadSuccess = true;
                            AddNewValue(rowPos, columnPos, num, nameof(TestByNumbers));
                        }
                        else
                        {
                            CheckAbsentValueTrends(found);
                        }
                    }
                }
            }
            return hadSuccess;
        }

        /// <summary>
        /// It's possible that 2 rows will be missing the same 2 positions that need the same values, so 2 solutions exist.
        /// This method should look for that case and will add the lowest missing value to the top left corner of the four missing spots.
        /// </summary>
        private void TestCrissCross_MultipleSolutions()
        {
            var rows = new HashSet<int>();
            var positions = new Dictionary<(int, int), HashSet<int>>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (_table[i, j] == 0)
                    {
                        rows.Add(i);
                        if (_options.TryGetValue((i, j), out var optionSet) && optionSet.Count > 1)
                        {
                            positions[(i, j)] = new HashSet<int>(optionSet);
                        }
                    }
                }
            }
            if (rows.Count > 2) return;
            if (rows.Count == 2 && positions.Count == 4)
            {
                System.Console.WriteLine("Got something");
                // Get first key from positions
                var first = positions.Keys.First();

                // Look through other rows for matching column value
                var other = 0;
                foreach (var val in rows)
                {
                    if (val != first.Item1)
                    {
                        other = val;
                        break;
                    }
                }
                if (other == 0) return;

                // If we find one, check that the options match.
                if (!positions.TryGetValue(first, out var firstOptions) || firstOptions.Count > 2) return;
                if (!positions.TryGetValue((other, first.Item2), out var secondOptions) || secondOptions.Count > 2) return;

                foreach (var item in firstOptions)
                {
                    if (!secondOptions.Contains(item)) return;
                }

                // If they do, look for a second position in the original row.
                var fourthOptions = new HashSet<int>();
                for (int i = 1; i < 10; i++)
                {
                    if (i == first.Item2) continue;

                    // If found, make sure they have the same pair of options.
                    if (positions.TryGetValue((first.Item1, i), out var thirdOptions))
                    {
                        if (!positions.TryGetValue((other, i), out fourthOptions)) return;

                        // If the "fourth" position is present and that the options are there.
                        foreach (var item in thirdOptions)
                        {
                            if (!fourthOptions.Contains(item)) return;
                        }
                    }
                }

                // Use the original position to AddNewValue
                AddNewValue(first.Item1, first.Item2, firstOptions.Min(), "TestCrissCross_MultipleSolutions");
            }
        }

        /// <summary>
        /// Compares cell options for a given value and checks if they all share a row and/or column.  This would mean that the cells not in this set of found values
        /// can remove this value from their options since we know it must occur in the current cell's square.
        /// </summary>
        private void CheckAbsentValueTrends(IList<(int, int, int)> found)
        {
            if (found.Count < 1) return;

            var row = new HashSet<int>();
            var column = new HashSet<int>();
            var number = found[0].Item3;

            foreach (var item in found)
            {
                if (number != item.Item3) return;
                row.Add(item.Item1);
                column.Add(item.Item2);
            }

            if (row.Count == 1)
            {
                // If there's only one row, then we know that this column's value lives in this square.
                var i = row.ToList()[0];
                for (int j = 0; j < 9; j++)
                {
                    if (column.Contains(j)) continue;
                    RemoveFromOptions(i, j, number);
                }
            }

            if (column.Count == 1)
            {
                // If there's only one column, then we know that this row's value lives in this square.
                var j = column.ToList()[0];
                for (int i = 0; i < 9; i++)
                {
                    if (row.Contains(i)) continue;
                    RemoveFromOptions(i, j, number);
                }
            }
        }

        /// <summary>
        /// Cycles through the entire row, column and square, removing options from the possible values.
        /// Returns a HashSet of the remaining value options.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private HashSet<int> SimpleFills(int i, int j)
        {
            var known = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            if (_table[i, j] != 0)
            {
                known.Clear();
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

        private (int, int) GetSquare_TopLeft(int squareNumber)
        {
            switch (squareNumber)
            {
                case 0:
                    return (0, 0);
                case 1:
                    return (0, 3);
                case 2:
                    return (0, 6);
                case 3:
                    return (3, 0);
                case 4:
                    return (3, 3);
                case 5:
                    return (3, 6);
                case 6:
                    return (6, 0);
                case 7:
                    return (6, 3);
                case 8:
                    return (6, 6);
                default:
                    return (-1, -1);
            }
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