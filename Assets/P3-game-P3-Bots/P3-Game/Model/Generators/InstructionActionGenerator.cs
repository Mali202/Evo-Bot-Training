using Model.Actions;
using System.Collections.Generic;

namespace Model.Generators
{
    public class InstructionActionGenerator : IGenerator
    {
        private Game game;

        public InstructionActionGenerator()
        {
            game = ActionGenerator.Instance.Game;
        }
        public List<IAction> GenerateActions(Card card)
        {
            List<IAction> actions = new();

            List<Placement> placements = InstructionValidator.Instance.GetAllValid((Instruction) card);
            foreach (Placement placement in placements)
            {
                actions.Add(new PlaceInstruction(placement, game));
            }
            return actions;
        }
    }
}
