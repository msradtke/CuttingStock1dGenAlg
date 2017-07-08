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
                if(sol != s)
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

        void SelectParent()
        {
            //get total weight
            var totalFitness = _solutions.Sum(x => x.FinalFitness);
            Dictionary<Solution, double> weights = new Dictionary<Solution, double>();
            foreach(var s in _solutions)
            {
                var propWeight = s.FinalFitness / totalFitness;
                weights.Add(s, propWeight);
            }
            //Dictionary<int, Solution> segments = new Dictionary<int, Solution>();
            Dictionary<Solution, double> segments = new Dictionary<Solution, double>();

            double segmentCount = 0;
            foreach (var kvp in weights)
            {
                segmentCount += kvp.Value;
                segments.Add(kvp.Key, segmentCount);
            }

            Random r = new Random();
            r.NextDouble();


        }
    }
}
