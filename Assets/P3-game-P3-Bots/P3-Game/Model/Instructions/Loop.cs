using System.Linq;

namespace Model.Instructions
{
    public class Loop : Instruction
    {
        public int Player;

        public override Card CloneCard()
        {
            return new Loop() {
                Name = Name,
                Target = Target,
                Deck = Deck,

                Wood = Wood,
                Brick = Brick,
                Straw = Straw,
                Rotatable = Rotatable,
                PlacementRule = PlacementRule,

                Player = Player,
            };
        }

        public override void Execute(Game game, Orientation orientation)
        {
            ResourceOwner owner = game.CalculatePlayer(Player, orientation);
            if (owner is Player)
            {
                if ((owner.WoodCount >= Wood) && (owner.BrickCount >= Brick) && (owner.StrawCount >= Straw))
                {
                    game.loopStart = game.col;
                    game.row = 1;
                    game.col++;
                    game.inLoop = true;
                }
                else
                {
                    int endCol = game.NumInstructions + 1;
                    for (int i = game.col + 1; i < game.NumInstructions; i++)
                    {
                        Placement? nextMiddle = game.GameGrid.Placements.FirstOrDefault(Placement => Placement.Row == 0 && Placement.Col == i);
                        if (nextMiddle is not null)
                        {
                            endCol = i;
                            break;
                        }
                    }
                    game.col = endCol;
                }
            }
            else
            {
                int endCol = game.NumInstructions + 1;
                for (int i = game.col + 1; i < game.NumInstructions; i++)
                {                   
                    Placement? nextMiddle = game.GameGrid.Placements.FirstOrDefault(Placement => Placement.Row == 0 && Placement.Col == i);
                    if (nextMiddle is not null)
                    {
                        endCol = i;
                        break;
                    }               
                }
                game.col = endCol;
            }
        }
    }
}
