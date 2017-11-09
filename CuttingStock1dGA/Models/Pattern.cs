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
}
