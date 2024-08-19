using Model.Actions;
using System.Collections.Generic;

namespace Model.Generators
{
    public class DrawActionGenerator : IGenerator
    {
        private Game game;

        public DrawActionGenerator()
        {
            game = ActionGenerator.Instance.Game;
        }

        public List<IAction> GenerateActions(Card card)
        {
            if (game.CurPlayer.Hand.Cards.Count == 5)
            {
                return new();
            }

            List<IAction> drawActions = new()
            {
                new DrawAction(DeckType.Draw, game, 0),
                new DrawAction(DeckType.FaceUp, game, 0),
                new DrawAction(DeckType.FaceUp, game, 1),
                new DrawAction(DeckType.FaceUp, game, 2)
            };
            return drawActions;
        }
    }
}
