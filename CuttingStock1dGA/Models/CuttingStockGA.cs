using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingStock1dGA.Models
{
    class CuttingStockGA
    {
        List<Solution> _solutions;
        int _population;
        List<double> _items;
        double _master = 96;
        Dictionary<double, int> _demand;
        public CuttingStockGA()
        {

        }
        void SetRanks()
        {
            foreach (var s in _solutions)
                Rank(s);
        }
        void Rank(Solution s)
        {
            s.Rank = 1;
            var stockUsed = s.GetStockUsed();
            var patternCount = s.GetPatternCount();
            foreach (var sol in _solutions)
            {
                if (sol != s)
                {
                    if (sol.GetStockUsed() < stockUsed)
                        if (sol.GetPatternCount() <= patternCount)
                            s.Rank += 1;
                    if (sol.GetStockUsed() <= stockUsed)
                        if (sol.GetPatternCount() < patternCount)
                            s.Rank += 1;
                }
            }
        }
        void CalcAllFitness()
        {
            foreach (var s in _solutions)
                CalcFitness(s);

            foreach (var s in _solutions)
                CalcFinalFitness(s);
        }
        void CalcFitness(Solution solution)
        {
            solution.HigherRankedCount = 0;
            solution.SameRankCount = 0;
            foreach (var s in _solutions)
            {
                if (s.Rank == solution.Rank)
                    solution.SameRankCount += 1;
                else
                    if (s.Rank < solution.Rank)
                    solution.HigherRankedCount += 1;
            }
            var f = _population - solution.HigherRankedCount - .5 * (solution.SameRankCount - 1);
            solution.InitialFitness = f;
        }
        void CalcFinalFitness(Solution solution)
        {
            solution.FinalFitness = solution.FinalFitness; //temporary
        }
        Func<Solution> GetSelectParentFn()
        {
            //get total weight
            var totalFitness = _solutions.Sum(x => x.FinalFitness);
            Dictionary<Solution, double> weights = new Dictionary<Solution, double>();
            foreach (var s in _solutions)
            {
                var propWeight = s.FinalFitness / totalFitness;
                weights.Add(s, propWeight);
            }
            //Dictionary<int, Solution> segments = new Dictionary<int, Solution>();
            Dictionary<double, Solution> segments = new Dictionary<double, Solution>();

            double segmentCount = 0;
            foreach (var kvp in weights)
            {
                segmentCount += kvp.Value;
                segments.Add(segmentCount, kvp.Key);
            }
            var sortedSegs = segments.Keys.OrderBy(x => x);
            Func<Solution> ReturnParent = () =>
            {
                Random r = new Random();
                Solution Parent = null;
                foreach (var seg in sortedSegs)
                {
                    if (r.NextDouble() < seg)
                    {
                        Parent = segments[seg];
                        break;
                    }
                }
                return Parent;
            };
            return ReturnParent;
        }

        List<double> FFDGetPattern(double remainingMasterLength, Dictionary<double, int> residualDemand)
        {
            List<double> pattern = new List<double>();
            var sortedItems = _items.OrderByDescending(x => x);

            if (remainingMasterLength > sortedItems.Last())
                return pattern;

            foreach (var item in sortedItems)
            {
                if (remainingMasterLength < item)
                    continue;

                int demandLeft = residualDemand[item];
                if (demandLeft == 0)
                    continue;
                int maxItem = Convert.ToInt32(remainingMasterLength / item);

                if (maxItem > demandLeft)
                {
                    maxItem = demandLeft;
                }

                for (int i = 0; i < maxItem; ++i)
                {
                    pattern.Add(item);
                    remainingMasterLength -= item;
                }
            }

            return pattern;
        }

        void FirstFitDecreasing()
        {
            var residualDemand = new Dictionary<double, int>(_demand);
            List<double> patterns = new List<double>();

            var demandMet = false;
            while (!demandMet)
            {
                var pattern = FFDGetPattern(_master, residualDemand);
                if (pattern.Count == 0)
                {
                    demandMet = true;
                    break;
                }

                foreach (var cut in pattern)
                    residualDemand[cut]--;
            }

        }
        int GetMaxCutsFromPatternAndDemand(Pattern pattern, Dictionary<double, int> demand)
        {
            
            var uniqueCuts = pattern.Items.Distinct();
            int count = int.MaxValue;
            foreach (var cut in uniqueCuts)
            {
                var cutCount = pattern.Items.Count(x => x == cut);
                var max = demand[cut];

                var countBuff = Convert.ToInt32(max / cutCount);
                if (countBuff < count)
                    count = countBuff;
            }

            return count;
        }
    }
}
