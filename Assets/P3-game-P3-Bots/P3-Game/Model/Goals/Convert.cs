namespace Model.Goals
{
    public class Convert : Goal
    {
        public override void CalculateVP(Game game, Player player)
        {
            int vp;
            switch (ResourceType)
            {
                case ResourceType.Brick:
                    vp = player.BrickCount / NumResources * VP;
                    player.VP_Count += vp;
                    message = $"{player.Name} has earned {vp} VP for having {player.BrickCount} Brick";
                    break;

                case ResourceType.Wood:
                    vp = player.WoodCount / NumResources * VP;
                    player.VP_Count += vp;
                    message = $"{player.Name} earned {vp} VP for having {player.WoodCount} Wood";
                    break;

                case ResourceType.Straw:
                    vp = player.StrawCount / NumResources * VP;
                    player.VP_Count += vp;
                    message = $"{player.Name} earned {vp} VP for having {player.StrawCount} Straw";
                    break;

                default:
                    break;
            }
        }

        public override Card CloneCard()
        {
            return new Convert()
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
            return $"Earn {VP} VP for every {NumResources} {ResourceType}";
        }
    }
}
