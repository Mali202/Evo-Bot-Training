using Model.Actions;
using System.Collections.Generic;

namespace Model.Generators
{
    public class DiscardActionGenerator : IGenerator
    {
        private Game game;

        public DiscardActionGenerator()
        {
            game = ActionGenerator.Instance.Game;
        }

        public List<IAction> GenerateActions(Card card)
        {
            List<IAction> discardActions = new();
            foreach (Card handCard in game.CurPlayer.Hand.Cards)
            {
                discardActions.Add(new DiscardAction(game, handCard));
            }

            return discardActions;
        }
    }
}
