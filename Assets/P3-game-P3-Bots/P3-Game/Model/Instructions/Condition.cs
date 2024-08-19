namespace Model.Instructions
{
    public class Condition : Instruction
    {
        public int Player;

        public override Card CloneCard()
        {
            return new Condition() {
                Name = Name,
                Target = Target,
                Deck = Deck,

                Wood = Wood,
                Brick = Brick,
                Straw = Straw,
                Rotatable = Rotatable,
                PlacementRule = PlacementRule,

                Player = Player
            };
        }

        public override void Execute(Game game, Orientation orientation)
        {
            ResourceOwner owner = game.CalculatePlayer(Player, orientation);
            if (owner is Player)
            {
                if ((owner.WoodCount >= Wood) && (owner.BrickCount >= Brick) && (owner.StrawCount >= Straw))
                {
                    game.row = 1;
                }
                else
                {
                    game.row = -1;
                }
            }
            else
            {
                game.row = -1;
            }
            game.col++;
        }
    }
}
