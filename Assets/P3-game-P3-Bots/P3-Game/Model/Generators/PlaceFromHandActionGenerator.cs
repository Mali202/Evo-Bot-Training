using Model.Actions;
using System.Collections.Generic;

namespace Model.Generators
{
    public class PlaceFromHandActionGenerator : IGenerator
    {
        private readonly Game game;

        public PlaceFromHandActionGenerator()
        {
            game = ActionGenerator.Instance.Game;
        }

        public List<IAction> GenerateActions(Card card)
        {
            List<IAction> actions = new();
            foreach (Card handCard in game.CurPlayer.Hand.Cards)
            {
                actions.AddRange(ActionGenerator.Instance.GetAllValid(handCard));
            }

            return actions;
        }
    }
}
