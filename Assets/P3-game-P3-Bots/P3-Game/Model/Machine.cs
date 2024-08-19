namespace Model
{
    public class Machine : ResourceOwner
    {
        public Machine(int Wood, int Brick, int Straw)
        {
            Name = "Machine";
            WoodCount = Wood;
            BrickCount = Brick;
            StrawCount = Straw;
        }
    }
}
