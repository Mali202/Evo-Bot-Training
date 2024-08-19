using Model.Tricks;

namespace Model.Actions
{
    class PlaySkip : IAction
    {
        private readonly Player from;
        public readonly Player target;
        private readonly Skip skipCard;

        public PlaySkip(Player from, Player target, Skip card)
        {
            this.from = from;
            this.target = target;
            skipCard = card;
        }

        public void ExecuteAction()
        {
            from.Hand.Cards.Remove(skipCard);
            target.Visible.AddCard(skipCard);
        }

        public string GetDescription()
        {
            return $"Give {target.Name} a skip card";
        }

        public void UndoAction()
        {
            from.Hand.Cards.Add(skipCard);
            target.Visible.Cards.Remove(skipCard);

        }
    }
}
