using Model.Actions;
using Model.Tricks;
using System.Collections.Generic;

namespace Model.Generators
{
    public class SkipActionGenerator : IGenerator
    {
        private Game game;

        public SkipActionGenerator()
        {
            game = ActionGenerator.Instance.Game;
        }

        public List<IAction> GenerateActions(Card card)
        {
            List<IAction> actions = new();
            List<Player> targets = game.Players.FindAll(player => player != game.CurPlayer);
            foreach (Player target in targets)
            {
                actions.Add(new PlaySkip(game.CurPlayer, target, (Skip)card));
            }
            return actions;
        }
    }
}
