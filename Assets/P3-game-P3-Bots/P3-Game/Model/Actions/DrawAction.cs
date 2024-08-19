namespace Model.Actions
{
    class DrawAction : IAction
    {
        public DeckType deckType;
        private Game game;
        public int cardIndex;

        public DrawAction(DeckType deckType, Game game, int cardIndex)
        {
            this.deckType = deckType;
            this.game = game;
            this.cardIndex = cardIndex;
        }

        public void ExecuteAction() {
            game.DrawCard(deckType, cardIndex);
        }

        public string GetDescription()
        {
            if (deckType == DeckType.Draw)
            {
                return "Draw a card from the draw deck";
            }

            return $"Draw '{game.FaceUp.Cards[cardIndex].Name}' card";
        }

        public void UndoAction()
        {
            game.DrawCard_Undo(deckType, cardIndex);
        }
    }
}
