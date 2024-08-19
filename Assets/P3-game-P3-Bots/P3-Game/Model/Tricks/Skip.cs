using System;

namespace Model.Tricks
{
    public class Skip : Trick
    {
        public override Card CloneCard()
        {
            return new Skip()
            {
                Name = Name,
                Target = Target,
                Deck = Deck
            };
        }

        public override void Execute(Game game)
        {
            if (game.CurPlayer.ActionsRemaining > 0)
            {
                game.CurPlayer.ActionsRemaining--;
                Console.WriteLine($"{game.CurPlayer} has a skip card");
                game.CurPlayer.Visible.Cards.Remove(this);
                game.Discard.Cards.Add(this);
            }
        }

        public override void UndoExecute(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
