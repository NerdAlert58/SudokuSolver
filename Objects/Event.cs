using System.Collections.Generic;

namespace SudokuSolver.Objects
{
    public class Event
    {
        public int Id { get; set; }
        public int[,] Table { get; set; }
        public string MethodUsed { get; set; }
    }
}