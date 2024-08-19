
namespace Model
{
    public abstract class Card
    {
        public string Name {get; set;}
        public Target Target {get; set;}
        public Deck Deck {get; set;}
                
        public override string ToString()
        {
            return base.ToString();
        }


        public abstract Card CloneCard();
    }
}
