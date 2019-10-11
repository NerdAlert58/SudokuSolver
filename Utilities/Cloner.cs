using System.Collections.Generic;

namespace SudokuSolver.Utilities
{
    public class Cloner
    {
        public int[,] CloneIntArray(int[,] original)
        {
            var clone = (int[,])original.Clone();

            return clone;
        }

        public IDictionary<int, HashSet<int>> CloneOptions(IDictionary<int, HashSet<int>> original)
        {
            var clone = new Dictionary<int, HashSet<int>>();
            foreach (var kvp in original)
            {
                clone[kvp.Key] = new HashSet<int>(kvp.Value);
            }

            return clone;
        }
    }
}