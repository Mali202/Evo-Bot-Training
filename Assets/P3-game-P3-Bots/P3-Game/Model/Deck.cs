using System.Collections.Generic;

namespace Model
{
    public class Deck
    {
        public DeckType Type {get;}
        public AllowedCardType AllowedCardType {get;}
        public List<Card> Cards;

        public Deck(DeckType type, AllowedCardType allowedCardType)
        {
            Type = type;
            AllowedCardType = allowedCardType;
            Cards = new List<Card>();
        }

        public void AddCard(Card card) { 
            if (AllowedCardType == AllowedCardType.Trick)
            {
                if (card.GetType().IsSubclassOf(typeof(Trick)))
                {
                    Cards.Add(card);
                }
            } else
            {
                if (card.GetType().IsSubclassOf(typeof(Trick)) || card.GetType().IsSubclassOf(typeof(Instruction)))
                {
                    Cards.Add(card);
                }
            }
        }
    }
}
