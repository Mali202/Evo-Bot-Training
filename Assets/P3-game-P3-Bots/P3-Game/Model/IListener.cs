namespace Model
{
    public interface IListener
    {
        public void Broadcast(Trigger trigger);
    }
}
