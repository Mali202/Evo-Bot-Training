using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Utils
{
    public class WeightedActionExecutor
    {
        public WeightedActionParam[] Parameters { get; }
        private readonly Random r = new();

        public WeightedActionExecutor(params WeightedActionParam[] parameters)
        {
            Parameters = parameters;
        }
        //public double RatioSum
        //{
        //    get { return Parameters.Sum(p => p.Ratio); }
        //}

        public IAction Execute(List<IAction> actions)
        {
            //Filter out Parameters that aren't present in the list
            WeightedActionParam[] filteredParams = Parameters.Where((param) => actions.Any(param.Func)).ToArray();
            double RatioSum = filteredParams.Sum(p => p.Ratio);

            double numericValue = r.NextDouble() * RatioSum;

            foreach (WeightedActionParam parameter in filteredParams)
            {
                numericValue -= parameter.Ratio;

                if (!(numericValue <= 0))
                    continue;

                List<IAction> filtered = actions.Where(parameter.Func).ToList();
                if (filtered.Count == 0)
                    continue;

                int index = r.Next(0, filtered.Count);
                return filtered[index];
            }

            throw new Exception("No action found");
        }
    }
}