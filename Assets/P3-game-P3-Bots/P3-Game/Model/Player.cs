using System.Collections.Generic;

namespace Model
{
    public class Player : ResourceOwner
    {
        public bool IsWolf { get; }
        public int VP_Count { get; set; }
        public int ActionsPerTurn { get; set; }
        public int ActionsRemaining {  get; set; }
        public bool CanEdit;
        public Deck Hand { get; } 
        public Deck Visible { get; }
        public Deck Action { get; }
        private Goal Goal;
        public int playerNumber;

        public Player(bool isWolf, string Name, int playerNumber)
        {
            this.Name = Name;
            IsWolf = isWolf;
            VP_Count = 0;
            ActionsPerTurn = 1; //Might be different for wolf
            ActionsRemaining = 0;
            CanEdit = false;
            Hand = new Deck(DeckType.Hand, AllowedCardType.Combination);
            Visible = new Deck(DeckType.Hand, AllowedCardType.Trick);
            Action = new Deck(DeckType.Hand, AllowedCardType.Action);
            this.playerNumber = playerNumber;
        }

        public void ReduceActions()
        {
            ActionsRemaining--;
        }

        public void SetGoal(Goal Goal)
        {
            this.Goal = Goal;
        }

        public void ActivateGoalCard(Game game) {
            Goal.CalculateVP(game, this);
        }

        public string GetGoalStatus() {
            return Goal.message;
        }

        public string GetGoalName() {
            return Goal.Name;
        }

        public void SetCanEdit(bool CanEdit) 
        { 
            this.CanEdit = CanEdit;
        }

        public List<IAction> GetAllValidActions()
        {
            List<IAction> actionList = new();
            foreach (Card card in Action.Cards)
            {
                actionList.AddRange(ActionGenerator.Instance.GetAllValid(card));
            }
            return actionList;
        }
    }
}
