namespace Model.Instructions
{
    public class Transfer : Instruction
    {
        public int FromPlayer {get; set;}
        public int ToPlayer { get; set;}

        public override Card CloneCard()
        {
            return new Transfer()
            {
                Name = Name,
                Target = Target,
                Deck = Deck,

                Wood = Wood,
                Brick = Brick,
                Straw = Straw,
                Rotatable = Rotatable,
                PlacementRule = PlacementRule,

                FromPlayer = FromPlayer,
                ToPlayer = ToPlayer
            };
        }

        public override void Execute(Game game, Orientation orientation)
        {
            ResourceOwner from = game.CalculatePlayer(FromPlayer, orientation);
            ResourceOwner to = game.CalculatePlayer(ToPlayer, orientation);

            int amount;
            if (from.BrickCount < Brick)
            {
                amount = from.BrickCount;
            }
            else
            {
                amount = Brick;
            }
            from.BrickCount -= amount;
            to.BrickCount += amount;

            if (from.WoodCount < Wood)
            {
                amount = from.WoodCount;
            }
            else
            {
                amount = Wood;
            }
            from.WoodCount -= amount;
            to.WoodCount += amount;

            if (from.StrawCount < Straw)
            {
                amount = from.StrawCount;
            }
            else
            {
                amount = Straw;
            }
            from.StrawCount -= amount;
            to.StrawCount += amount;

            game.col++;
        }
    }
}
