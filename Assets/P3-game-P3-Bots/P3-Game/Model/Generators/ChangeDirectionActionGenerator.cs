using Model.Actions;
using Model.Tricks;
using System.Collections.Generic;

namespace Model.Generators
{
    public class ChangeDirectionActionGenerator : IGenerator
    {
        private Game game;

        public ChangeDirectionActionGenerator()
        {
            game = ActionGenerator.Instance.Game;
        }

        public List<IAction> GenerateActions(Card card)
        {
            return new List<IAction> { new PlayChangeDirection(game, (ChangeDirection)card)};
        }
    }
}
