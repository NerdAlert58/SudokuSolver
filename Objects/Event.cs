using System.Collections.Generic;

namespace SudokuSolver.Objects
{
    public class Event
    {
        public int Id { get; set; }
        public int[,] Table { get; set; }
        public IDictionary<(int, int), HashSet<int>> Options { get; set; }
        public string MethodUsed { get; set; }
    }
}