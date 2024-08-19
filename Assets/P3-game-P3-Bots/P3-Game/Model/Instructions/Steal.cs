using System;

namespace Model.Instructions
{
    public class Steal : Instruction
    {
        int Amount;
        int FromPlayer;
        ResourceType Resource;

        public override Card CloneCard()
        {
            return new Steal() {
                Name = Name,
                Target = Target,
                Deck = Deck,

                Wood = Wood,
                Brick = Brick,
                Straw = Straw,
                Rotatable = Rotatable,
                PlacementRule = PlacementRule,

                Amount = Amount,
                FromPlayer = FromPlayer,
                Resource = Resource
            };
        }

        public override void Execute(Game game, Orientation orientation)
        {
            throw new NotImplementedException();
        }
    }
}
