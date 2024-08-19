using Model.Tricks;

namespace Model.Actions
{
    public class PlayChangeDirection : IAction
    {
        private Game game;
        private ChangeDirection card;

        public PlayChangeDirection(Game game, ChangeDirection card)
        {
            this.game = game;
            this.card = card;
        }

        public void ExecuteAction()
        {
            card.Execute(game);
        }

        public string GetDescription()
        {
            return game.Direction switch
            {
                DirectionOfPlay.Clockwise => "Change the direction of play from clockwise to counter-clockwise",
                DirectionOfPlay.CounterClockwise => "Change the direction of play from counter-clockwise to clockwise",
                _ => "",
            };
        }

        public void UndoAction()
        {
            card.UndoExecute(game);
        }
    }
}