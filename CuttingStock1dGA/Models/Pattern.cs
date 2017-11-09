using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingStock1dGA.Models
{
    public class Pattern
    {
        public Pattern()
        {

        }
        public Pattern(Pattern p)
        {
            Items = new List<double>(p.Items);
        }
        public List<double> Items { get; set; }

    }
    public class PatternComparer : EqualityComparer<Pattern>
    {
        public override bool Equals(Pattern x, Pattern y)
        {
            if (x.Items.Count != y.Items.Count)
                return false;

            foreach (var i in x.Items)
            {
                var thisCount = x.Items.Count(a => i == a);
                var otherCount = y.Items.Count(a => i == a);
                if (thisCount != otherCount)
                    return false;
            }
            return true;
        }
        public override int GetHashCode(Pattern obj)
        {
            return obj.Items.GetHashCode();
        }
    }
}

