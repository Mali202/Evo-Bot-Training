using Model.Instructions;

namespace Model.Actions
{
    class PlaceInstruction : IAction
    {
        public Placement placement;
        private Game game;

        public PlaceInstruction(Placement placement, Game game)
        {
            this.placement = placement;
            this.game = game;
        }

        public void ExecuteAction()
        {
            game.PlaceInstruction(placement);           
        }

        public string GetDescription()
        {
            return $"Place '{placement.Instruction.Name}' in (Row: {placement.Row}, Column {placement.Col}) with orientation {placement.Orientation} {GetToFrom()}";
        }

        public void UndoAction()
        {
            game.PlaceInstruction_Undo(placement);
        }

        //Temp
        private string GetToFrom()
        {
            return placement.Instruction switch
            {
                Blueprint k => $"(To: {game.CalculatePlayer(k.Player, placement.Orientation).Name})",
                Transfer k => $"(To: {game.CalculatePlayer(k.ToPlayer, placement.Orientation).Name}, From: {game.CalculatePlayer(k.FromPlayer, placement.Orientation).Name})",
                Loop k => $"(Player: {game.CalculatePlayer(k.Player, placement.Orientation).Name})",
                Condition k => $"(Player: {game.CalculatePlayer(k.Player, placement.Orientation).Name})",
                _ => "",
            };
        }
    }
}
