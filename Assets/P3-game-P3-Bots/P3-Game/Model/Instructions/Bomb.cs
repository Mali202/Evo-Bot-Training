namespace Model.Instructions
{
    public class Bomb : Instruction
    {
        public override Card CloneCard()
        {
            return new Bomb() {
                Name = Name,
                Target = Target,
                Deck = Deck,

                Wood = Wood,
                Brick = Brick,
                Straw = Straw,
                Rotatable = Rotatable,
                PlacementRule = PlacementRule
            };
        }

        public override void Execute(Game game, Orientation orientation)
        {
            game.col = game.NumInstructions + 1;
        }
    }
}
