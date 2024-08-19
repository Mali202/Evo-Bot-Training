namespace Model.Tricks
{
    public class ChangeDirection : Trick
    {
        public override Card CloneCard()
        {
            return new ChangeDirection()
            {
                Name = Name,
                Target = Target,
                Deck = Deck
            };
        }

        public override void Execute(Game game)
        {
            switch (game.Direction)
            {
                case DirectionOfPlay.Clockwise:
                    game.Direction = DirectionOfPlay.CounterClockwise;
                    break;

                case DirectionOfPlay.CounterClockwise:
                    game.Direction = DirectionOfPlay.Clockwise;
                    break;
            }

            game.CurPlayer.Hand.Cards.Remove(this);
            game.Discard.Cards.Add(this);          
        }

        public override void UndoExecute(Game game)
        {
            switch (game.Direction)
            {
                case DirectionOfPlay.Clockwise:
                    game.Direction = DirectionOfPlay.CounterClockwise;
                    break;

                case DirectionOfPlay.CounterClockwise:
                    game.Direction = DirectionOfPlay.Clockwise;
                    break;
            }

            game.CurPlayer.Hand.Cards.Add(this);
            game.Discard.Cards.Remove(this);
        }
    }
}
