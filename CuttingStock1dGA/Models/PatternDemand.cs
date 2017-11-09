using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingStock1dGA.Models
{
    public class PatternDemand
    {
        public PatternDemand()
        {

        }
        public PatternDemand(PatternDemand pd)
        {
            Pattern = new Pattern(pd.Pattern);
            Demand = pd.Demand;
            StockLength = pd.StockLength;
        }
        public Pattern Pattern { get; set; }
        public int Demand { get; set; }
        public double StockLength { get; set; }
    }
}
