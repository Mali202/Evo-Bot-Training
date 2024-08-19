namespace Model
{
    public interface IAction
    {
        public string GetDescription();
        public void ExecuteAction();
        public void UndoAction();
    }
}
