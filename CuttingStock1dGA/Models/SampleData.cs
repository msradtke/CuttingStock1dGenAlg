using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingStock1dGA.Models
{
    public static class SampleData
    {
        static public Dictionary<double, int> GetSampleData1()
        {
            Dictionary<double, int> data = new Dictionary<double, int>();

            var cuts = new double[] { 12.5, 11.625, 55, 96, 22, 36.625, 89 };
            var demands = new int[] { 100, 150, 6, 75, 37, 55, 17, 90, 105, 37 };

            for(int i=0;i<cuts.Length;++i)
            {
                data.Add(cuts[i], demands[i]);
            }

            return data;
        }

        public static double MasterLength1 = 96;

    }
}
