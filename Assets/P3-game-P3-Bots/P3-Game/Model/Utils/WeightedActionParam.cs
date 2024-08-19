using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Utils
{
    public class WeightedActionParam
    {
        public Func<IAction, bool> Func { get; }
        public double Ratio { get; }

        public WeightedActionParam(Func<IAction, bool> func, double ratio)
        {
            Func = func;
            Ratio = ratio;
        }
    }
}
