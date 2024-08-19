namespace Model
{
    public abstract class Goal: Card
    {
        public int VP {get; set;}
        public int NumResources {get; set;}
        public ResourceType ResourceType {get; set;}
        public string message { get; set;}


        public abstract void CalculateVP(Game game, Player player);
        public abstract string GetDescription();
    }
}
