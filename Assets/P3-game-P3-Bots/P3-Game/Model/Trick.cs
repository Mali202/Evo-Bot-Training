namespace Model
{
    public abstract class Trick : Card
    {
        public abstract void Execute(Game game);
        public abstract void UndoExecute(Game game);
    }
}
