using System.Collections.Generic;

namespace Model.Generators
{
    public class SwapActionGenerator : IGenerator
    {
        public List<IAction> GenerateActions(Card card)
        {
            return new();
        }
    }
}
