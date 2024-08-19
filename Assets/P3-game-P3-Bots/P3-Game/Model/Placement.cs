namespace Model
{
    public class Placement
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public Orientation Orientation { get; set; }
        public Instruction Instruction { get; set; }

        public Placement(int row, int col, Orientation orientation, Instruction instruction)
        {
            Row = row;
            Col = col;
            Orientation = orientation;
            Instruction = instruction;
        }


    }
}
