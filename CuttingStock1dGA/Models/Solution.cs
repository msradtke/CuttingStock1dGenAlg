using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingStock1dGA.Models
{
    public class Solution
    {
        public int Rank { get; set; }
        public int HigherRankedCount { get; set; }
        public int SameRankCount { get; set; }
        public double InitialFitness { get; set; }
        public double FinalFitness { get; set; }
        public Solution()
        {

        }

        public int GetStockUsed()
        {
            return 0;
        }
        public int GetPatternCount()
        {
            return 0;
        }
    }
}
