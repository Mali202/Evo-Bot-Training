using Model.Utils;
using System;
using System.Collections.Generic;

namespace Model.Tricks
{
    public class Rotate : Trick
    {
        public Orientation Orientation;
        public Placement Placement;

        public override Card CloneCard()
        {
            return new Rotate()
            {
                Name = Name,
                Target = Target,
                Deck = Deck
            };
        }

        public override void Execute(Game game)
        {
            Placement.Orientation = Orientation;
            game.Discard.AddCard(this);
            game.CurPlayer.Hand.Cards.Remove(this);
            game.AddToTriggerQueue(Constants.OnCardRotated, new KeyValuePair<string, object>("Card", this));
        }

        public override void UndoExecute(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
