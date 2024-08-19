
namespace Model
{
    public abstract class Instruction: Card
    {
        public int Wood {get; set;}
        public int Straw {get; set;}
        public int Brick {get; set;}
        public bool Rotatable {get; set;}
        //public int From { get; set; }
        //public int To { get; set; }
        public PlacementRuleType PlacementRule {get; set;}

        public abstract void Execute(Game game, Orientation orientation);
    }
}
