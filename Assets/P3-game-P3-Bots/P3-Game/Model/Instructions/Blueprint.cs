using System;

namespace Model.Instructions
{
    public class Blueprint : Instruction
    {
        public int VP {get; set;}
        public int Player {get; set;}

        public override Card CloneCard()
        {
            Blueprint card = new()
            {
                Name = Name,
                Target = Target,
                Deck = Deck,

                Wood = Wood,
                Brick = Brick,
                Straw = Straw,
                Rotatable = Rotatable,
                PlacementRule = PlacementRule,

                VP = VP,
                Player = Player
            };
            return card;
        }

        public override void Execute(Game game, Orientation orientation)
        {
            ResourceOwner To = game.CalculatePlayer(Player, orientation);
            Console.WriteLine("To: " +  To.Name);
            if (To is Player player)
            {
                if ((To.WoodCount >= Wood) && (To.BrickCount >= Brick) && (To.StrawCount >= Straw))
                {
                    Machine machine = game.Machine;
                    
                    To.WoodCount -= Wood;
                    To.BrickCount -= Brick;
                    To.StrawCount -= Straw;

                    machine.StrawCount += Straw;
                    machine.BrickCount += Brick;
                    machine.WoodCount += Wood;

                    player.VP_Count += VP;
                }
            }
            game.col++;
        }
    }
}
