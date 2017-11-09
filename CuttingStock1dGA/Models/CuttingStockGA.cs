using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingStock1dGA.Models
{
    public class CuttingStockGA
    {
        List<Solution> _solutions;
        int _population;
        List<double> _items;
        //double _master = 96;
        List<double> _masters;
        Dictionary<double, int> _demand;
        List<PatternDemand> _patternDemands;
        double additionalPatternSelection = .10;
        double mutateChance = .1;
        int _bestPatternCount = 3;
        Solution _dominator;
        public void UseSampleData()
        {
            _demand = SampleData.GetSampleData1();
            _items = _demand.Keys.ToList();
            _population = 10;
            //_master = SampleData.MasterLength1;

        }
        public List<Solution> Process(Dictionary<double, int> demand, List<double> masters, int population)
        {
            _solutions = new List<Solution>();
            if (demand.Count <= 0)
                return _solutions;
            if (demand.Keys.Sum() <= 0)
                return _solutions;
            if (demand.Values.Sum() <= 0)
                return _solutions;

            _demand = demand;
            _items = _demand.Keys.ToList();
            _population = population;

            _masters = masters;
            Process();
            return _solutions.OrderBy(x => x.Rank).ToList();
        }
        public void Process()
        {
            _patternDemands = new List<PatternDemand>();
            _solutions = new List<Solution>();



            foreach (var master in _masters)
            {
                var pd = FirstFitDecreasing(new Dictionary<double, int>(_demand), master);
                _solutions.Add(new Solution { PatternDemands = pd });

            }
            while (_solutions.Count < _population)
            {
                var rpd = RandomPatternWithDemandMatching(new Dictionary<double, int>(_demand));
                _solutions.Add(new Solution { PatternDemands = rpd });
            }
            SetRanks();
            CalcAllFitness();
            if (_solutions.Count(x => x.Rank == 1) == 1)
                _dominator = _solutions.Where(x => x.Rank == 1).FirstOrDefault();

            for (int i = 0; i < 50; ++i)
            {

                var child = CreateOffspring();
                _solutions.Add(child);
                SetRanks();
                var lowestRanked = _solutions.OrderByDescending(x => x.Rank).FirstOrDefault();
                if (child.Rank == lowestRanked.Rank)
                    _solutions.Remove(child);
                else
                    _solutions.Remove(lowestRanked);

                Mutate();


                CheckIfNewSolutionIsBest(child);
                SetRanks();
                CalcAllFitness();

            }


        }

        void Mutate()
        {
            var ran = new Random();
            if(ran.NextDouble() < mutateChance)
            {
                var bestPatterns = GetBestPatterns();
                var index = ran.Next(0, _bestPatternCount);
                var pattern = new PatternDemand(bestPatterns[index]);

                index = ran.Next(0, _solutions.Count);
                var solution = _solutions[index];

                index = ran.Next(0, solution.PatternDemands.Count);
                var oldPattern = solution.PatternDemands[index];
                solution.Patterns.Remove(oldPattern.Pattern);
                solution.PatternDemands.Remove(oldPattern);

                solution.PatternDemands.Insert(0,pattern);

                RepairSolution(solution);
            }
        }
        void RepairSolution(Solution solution)
        {
            var newPatternDemands = new List<PatternDemand>();
            var removePatternDemands = new List<PatternDemand>();
            var residualDemand = new Dictionary<double, int>(_demand);
            foreach (var pattern in solution.PatternDemands)
            {
                var maxCuts = GetMaxCutsFromPatternAndDemand(pattern.Pattern, residualDemand);
                if (maxCuts == 0)
                {
                    removePatternDemands.Add(pattern);
                    continue;
                }
                DeductDemand(maxCuts, pattern.Pattern, residualDemand);
                var pd = new PatternDemand { Pattern = pattern.Pattern, Demand = pattern.Demand, StockLength = pattern.StockLength };
                newPatternDemands.Add(pd);
            }
            foreach (var remove in removePatternDemands)
                newPatternDemands.Remove(remove);

            if (!IsDemandMet(residualDemand))
            {               
                var pdemands = FirstFitDecreasing(residualDemand, GetRandomMaster());
                newPatternDemands.AddRange(pdemands);
            }
            solution.PatternDemands = newPatternDemands;        
        }
        List<PatternDemand> GetBestPatterns()
        {
            var allPatterns = _solutions.SelectMany(x => x.PatternDemands);
            var orderedPatterns = allPatterns.OrderBy(x => x.StockLength - x.Pattern.Items.Sum()); //waste getting bigger
            var bestPatterns = orderedPatterns.Take(_bestPatternCount);
            return bestPatterns.ToList();
        }
        void CheckIfNewSolutionIsBest(Solution child)
        {
            if (!_solutions.Contains(child))
            {
                Console.WriteLine("Not added");
                return;
            }
            else if (child.Rank == 1)
            {
                Console.WriteLine("New non-dominated");
                if (_solutions.Count(x => x.Rank == 1) == 1)
                {
                    _dominator = child;
                    Console.WriteLine("New dominator");
                }
            }
            Console.WriteLine("Rank: {0}", child.Rank);
            Console.WriteLine("Stock Used: {0}", child.StockUsed);
            Console.WriteLine("Pattern Count: {0}", child.PatternCount);
        }
        public CuttingStockGA()
        {

        }
        void SetRanks()
        {
            foreach (var s in _solutions)
                Rank(s);
        }
        void Rank(Solution subjectSol)
        {
            subjectSol.Rank = 1;
            var stockUsed = subjectSol.GetTotalMasterLengthUsage();
            var patternCount = subjectSol.GetPatternCount();

            var score = ScoreSolution(subjectSol);
            foreach (var otherSol in _solutions) //test to see if OTHER sol has no worse (ranking) and at least 1 is better, then THIS sol rank++ (ranked worse)
            {
                if (otherSol != subjectSol)
                {
                    var otherScore = ScoreSolution(otherSol);
                    if (otherScore < score)
                    {
                        subjectSol.Rank += 1;
                    }
                }
            }

            /*
            foreach (var otherSol in _solutions) //test to see if OTHER sol has no worse (ranking) and at least 1 is better, then THIS sol rank++ (ranked worse)
            {
                if (otherSol != subjectSol)
                {
                    if (otherSol.GetTotalMasterLengthUsage() < stockUsed)
                    {
                        if (otherSol.GetPatternCount() <= patternCount)
                            subjectSol.Rank += 1;
                    }
                    //else if (otherSol.GetTotalMasterLengthUsage() == stockUsed)
                    // if (otherSol.GetPatternCount() < patternCount)
                    //subjectSol.Rank += 1;
                }
            }
            */
        }

        double ScoreSolution(Solution subjectSol) //higher score is better
        {
            
            var avgCutTypePerPattern = subjectSol.Patterns.Average(x => x.Items.Distinct().Count());
            var sumLengthOfMasters = subjectSol.GetTotalMasterLengthUsage();
            var patternCount = subjectSol.GetPatternCount();

            var totalAvgCutTypePerPattern = _solutions.Sum(x => x.Patterns.Average(y => y.Items.Distinct().Count()));
            var totalSumLengthOfMasters = _solutions.Sum(x => x.GetTotalMasterLengthUsage());
            var totalPatternCount = _solutions.Sum(x => x.GetPatternCount());

            //weight of each

            double avgCutTypeWeight = .5;
            double totalLengthOfMastersWeight = .3;
            double patternCountWeight = 1 - avgCutTypeWeight - totalLengthOfMastersWeight;

            var score = ((avgCutTypePerPattern / totalAvgCutTypePerPattern) * avgCutTypeWeight) +
                (sumLengthOfMasters / totalSumLengthOfMasters) * totalLengthOfMastersWeight +
                (patternCount / totalPatternCount) * patternCountWeight;

            return score;

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
            solution.FinalFitness = solution.InitialFitness; //temporary
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

        Pattern FFDGetPattern(double remainingMasterLength, Dictionary<double, int> residualDemand)
        {
            Pattern pattern = new Pattern();
            pattern.Items = new List<double>();

            var sortedItems = _items.OrderByDescending(x => x);

            if (remainingMasterLength < sortedItems.Last())
                return pattern;

            foreach (var item in sortedItems)
            {
                if (remainingMasterLength < item)
                    continue;
                if (remainingMasterLength < sortedItems.Last())
                    return pattern;


                int demandLeft = residualDemand[item];
                if (demandLeft == 0)
                    continue;
                int maxItem = (int)(remainingMasterLength / item);

                if (maxItem > demandLeft)
                {
                    maxItem = demandLeft;
                }

                for (int i = 0; i < maxItem; ++i)
                {
                    pattern.Items.Add(item);
                    remainingMasterLength -= item;
                }
            }

            return pattern;
        }

        List<PatternDemand> FirstFitDecreasing(Dictionary<double, int> residualDemand, double masterLength)
        {
            List<double> patterns = new List<double>();
            var solution = new Solution();
            List<PatternDemand> patternDemands = new List<PatternDemand>();
            var demandMet = false;
            while (!demandMet)
            {
                var pattern = FFDGetPattern(masterLength, residualDemand);
                if (pattern.Items.Count == 0)
                {
                    demandMet = true;
                    break;
                }

                var patternCount = GetMaxCutsFromPatternAndDemand(pattern, residualDemand);
                DeductDemand(patternCount, pattern, residualDemand);
                patternDemands.Add(new PatternDemand { Pattern = pattern, Demand = patternCount, StockLength = masterLength });
            }

            return patternDemands;
        }
        void DeductDemand(int count, Pattern pattern, Dictionary<double, int> demand)
        {
            var uniqueCuts = pattern.Items.Distinct();
            foreach (var cut in uniqueCuts)
            {
                var cutCount = pattern.Items.Count(x => x == cut);
                demand[cut] -= cutCount * count;
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

                var countBuff = max / cutCount;
                if (countBuff < count)
                    count = countBuff;
            }
            return count;
        }


        List<PatternDemand> RandomPatternWithDemandMatching(Dictionary<double, int> residualDemand)
        {
            List<PatternDemand> patternDemands = new List<PatternDemand>();
            var items = residualDemand.Keys.ToList();
            var master = GetRandomMaster();
            var remainingLength = master;

            bool demandMet = false;
            while (!demandMet)
            {
                var pattern = GetPatternForRandom(master, residualDemand);
                if (pattern.Items.Count == 0)
                {
                    demandMet = true;
                    break;
                }

                var demand = GetMaxCutsFromPatternAndDemand(pattern, residualDemand);
                DeductDemand(demand, pattern, residualDemand);

                patternDemands.Add(new PatternDemand { Demand = demand, Pattern = pattern, StockLength = master });


            }
            return patternDemands;
        }

        Pattern GetPatternForRandom(double masterLength, Dictionary<double, int> residualDemand)
        {
            Pattern p = new Pattern();
            p.Items = new List<double>();

            List<int> indexUsed = new List<int>();
            var items = residualDemand.Keys.ToList();
            bool usableItemRemains = true;
            var remainingLength = masterLength;
            while (usableItemRemains)
            {
                var availDemands = residualDemand.Where(x => x.Value > 0);
                var minAvailableLength = availDemands.OrderBy(x => x.Key).FirstOrDefault().Key;

                if (remainingLength < minAvailableLength)
                {
                    usableItemRemains = false;
                    break;
                }

                Random IndexRandom = new Random();
                Random percentRandom = new Random();
                var percent = percentRandom.Next(10, 100);
                percent = percent / 100;
                var index = IndexRandom.Next(0, residualDemand.Count);
                var item = items[index];

                if (indexUsed.Contains(index))
                    continue;

                indexUsed.Add(index);

                int countToUse = 0;
                var remaining = residualDemand[item];
                var maxFitInPattern = (int)(remainingLength / item);
                var percentageUsage = percent * remaining;

                if (maxFitInPattern > remaining)
                    maxFitInPattern = remaining;

                countToUse = maxFitInPattern;

                if (percentageUsage >= 1)
                    if (percentageUsage < maxFitInPattern)
                        countToUse = percentageUsage;
                for (int i = 0; i < countToUse; ++i)
                {
                    p.Items.Add(item);
                    remainingLength -= item;
                }


                if (indexUsed.Count == items.Count)
                    usableItemRemains = false;
            }
            return p;
        }

        Solution CreateOffspring()
        {
            var selectedParentFunc = GetSelectParentFn();

            var unusablePatterns = new List<Pattern>();

            var averagePatternCount = _solutions.Average(x => x.PatternCount);
            int maxPatternCount = (int)(averagePatternCount * additionalPatternSelection + averagePatternCount);
            //testing
            //maxPatternCount /= 2; //test

            var child = new Solution();
            child.PatternDemands = new List<PatternDemand>();
            var master = GetRandomMaster();

            var residualDemand = new Dictionary<double, int>(_demand);

            var parent = selectedParentFunc();

            var parentPatterns = parent.PatternDemands;
            var parentPatternCount = parent.PatternCount;
            bool demandIsMet = false;
            while (child.PatternCount < maxPatternCount)
            {
                if (unusablePatterns.Count == parentPatternCount)
                    break;
                var r = new Random();
                var patternIndex = r.Next(0, parentPatternCount);
                var pattern = parentPatterns[patternIndex];
                var maxCuts = 0;
                if (!unusablePatterns.Contains(pattern.Pattern))
                {
                    unusablePatterns.Add(pattern.Pattern);
                    maxCuts = GetMaxCutsFromPatternAndDemand(pattern.Pattern, residualDemand);
                }

                if (maxCuts == 0)
                    continue;
                else
                {
                    child.PatternDemands.Add(new PatternDemand { Demand = maxCuts, Pattern = pattern.Pattern, StockLength = pattern.StockLength });
                    DeductDemand(maxCuts, pattern.Pattern, residualDemand);
                }
                if (IsDemandMet(residualDemand))
                {
                    demandIsMet = true;
                    break;
                }
            }

            if (!demandIsMet)
            {
                var pdemands = FirstFitDecreasing(residualDemand, GetRandomMaster());
                child.PatternDemands.AddRange(pdemands);

            }
            return child;
        }
        double GetRandomMaster()
        {
            var ran = new Random();
            var index = ran.Next(0, _masters.Count);
            return _masters[index];
        }
        bool IsDemandMet(Dictionary<double, int> demand)
        {

            if (demand.Count(x => x.Value > 0) == 0)
                return true;
            return false;
        }

    }
}