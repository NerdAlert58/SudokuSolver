using System;
using SudokuSolver.Objects;

namespace SudokuSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var puzzle = new Puzzle();

            puzzle.LoadTestPuzzles();
            puzzle.RunTheGauntlet();
            // puzzle.LoadTable(",,,5,,,4,,7,,4,,,6,,,,,,6,8,,,3,,,5,,,3,,,4,,,,5,,2,,,,8,,4,,,,1,,,7,,,9,,,3,,,1,7,,,,,,2,,,9,,2,,1,,,9,,,");
            // puzzle.Solve();
        }
    }
}
