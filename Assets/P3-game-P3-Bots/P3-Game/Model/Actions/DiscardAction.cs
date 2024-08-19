namespace Model.Actions
{
    class DiscardAction : IAction
    {
        public readonly Card cardToDiscard;
        private readonly Game game;

        public DiscardAction(Game game, Card cardToDiscard)
        {
            this.game = game;
            this.cardToDiscard = cardToDiscard;
        }

        public void ExecuteAction()
        {
            game.DiscardCard(cardToDiscard);
        }

        public string GetDescription()
        {
            return $"Discard '{cardToDiscard.Name}'";
        }

        public void UndoAction()
        {
            game.Discard_Undo(cardToDiscard);
        }
    }
}
