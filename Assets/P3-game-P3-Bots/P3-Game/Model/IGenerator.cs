using System.Collections.Generic;

namespace Model
{
    public interface IGenerator
    {
        List<IAction> GenerateActions(Card card);
    }
}
