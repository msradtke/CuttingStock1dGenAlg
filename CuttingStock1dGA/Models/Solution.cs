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
        public List<PatternDemand> PatternDemands { get; set; }
        public Solution()
        {

        }
        public int GetStockUsed()
        {
            return PatternDemands.Sum(x => x.Demand);
        }
        public double GetTotalMasterLengthUsage()
        {
            var total = PatternDemands.Sum(x => x.Demand * x.StockLength);
            return total;
        }
        public int GetPatternCount()
        {
            
            return PatternDemands.Count();
        }

        public int StockUsed
        {
            get { return PatternDemands.Sum(x => x.Demand); }
        }
        public int PatternCount
        {
            get { return GetPatternCount(); }
        }

        public List<Pattern> Patterns
        {
            get { return PatternDemands.Select(x=>x.Pattern).ToList(); }
        }
    }


}
