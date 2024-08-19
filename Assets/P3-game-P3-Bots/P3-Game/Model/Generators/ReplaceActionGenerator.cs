using System.Collections.Generic;

namespace Model.Generators
{
    public class ReplaceActionGenerator : IGenerator
    {
        public List<IAction> GenerateActions(Card card)
        {
            return new();
        }
    }
}
