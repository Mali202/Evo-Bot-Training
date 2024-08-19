using System.Linq;

namespace Model.Goals
{
    public class Max : Goal
    {
        public override void CalculateVP(Game game, Player player)
        {
            int max;
            bool success;
            switch (ResourceType) {
                case ResourceType.Brick:
                    max = game.Players.Max(player => player.BrickCount);
                    if (player.BrickCount == max)
                    {
                        player.VP_Count += VP;
                        success = true;
                    }
                    else
                    {
                        success = true;
                    }                   
                    break;

                case ResourceType.Wood:
                    max = game.Players.Max(player => player.WoodCount);
                    if (player.WoodCount == max)
                    {
                        player.VP_Count += VP;
                        success = true;
                    }
                    else
                    {
                        success = false;
                    }
                    break;

                case ResourceType.Straw:
                    max = game.Players.Max(player => player.StrawCount);
                    if (player.StrawCount == max)
                    {
                        player.VP_Count += VP;
                        success = true;
                    }
                    else
                    {
                        success = false;
                    }
                    break;

                default:
                    success = false;
                    break;
            }

            if (success)
            {
                message = $"{player.Name} has earned {VP} VP for having the most {ResourceType}";
            }
            else
            {
                message = $"{player.Name} did not have enough {ResourceType} to activate their goal card";
            }
        }

        public override Card CloneCard()
        {
            return new Max()
            {
                Name = Name,
                Target = Target,
                Deck = Deck,

                VP = VP,
                NumResources = NumResources,
                ResourceType = ResourceType,

            };
        }

        public override string GetDescription()
        {
            return $"Earn {VP} VP for having the most {ResourceType}";
        }
    }
}
