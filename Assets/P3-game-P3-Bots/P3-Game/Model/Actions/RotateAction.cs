using Model.Tricks;
using System;

namespace Model.Actions
{
    class RotateAction : IAction
    {
        public Orientation orientation;
        public Placement placement;
        private Rotate card;
        private Game game;

        public RotateAction(Rotate card, Game game,Placement placement, Orientation orientation) {
            this.orientation = orientation;
            this.placement = placement;
            this.card = card;
            this.game = game;
        }

        public void ExecuteAction()
        {
            card.Placement = placement;
            card.Orientation = orientation;
            card.Execute(game);
        }

        public string GetDescription()
        {
            return $"Rotate {placement.Instruction.Name} to face {orientation}";
        }

        public void UndoAction()
        {
            throw new NotImplementedException();
        }
    }
}
