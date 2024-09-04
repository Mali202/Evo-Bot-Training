using Model.Tricks;
using Model.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class Game
    {
        public ResourceOwner depot;
        public int NumInstructions {get; set;}
        public int NumPlayers {get; set;}

        public int MaxWood {get; set;}
        public int MaxStraw {get; set;}
        public int MaxBrick {get; set;}

        public bool WolfPresent;
        public Player CurPlayer;
        public int CurRound;
        public int FinalRound;

        public DirectionOfPlay Direction {get; set;}
        public State CurState {get; set;}

        public GameGrid GameGrid {get; set;}
        public Machine Machine {get; set;}
        public List<Player> Players {get; set;}

        public Deck Draw {get; set;}
        public Deck Discard {get; set;}
        public Deck FaceUp {get; set;}
        public List<Rule> Rules {get; set;}

        private List<IListener> ListenerList {get; set;}

        public List<Trigger> TriggerList { get; set;}
        public Queue<Trigger> TriggerQueue { get; set;}

        public List<Card> GameCards { get; set;}

        public int col;
        public int row;
        private Placement curPlacement;
        public bool inLoop;
        public int loopStart;

        private IAction? curAction;

        public Queue<Action> ActionQueue;
        public Queue<IAction> PlayerActionQueue;

        public void AddPlayer(string PlayerName, bool IsWolf, int playerNum) {
            Player Player = new Player(IsWolf, PlayerName, playerNum);
            Players.Add(Player);
        }

        public void AddListener(IListener listener) {
            ListenerList ??= new();
            ListenerList.Add(listener);
        }

        public void Update() {
            if (!TriggerQueue.Any())
            {
                NotifyListeners();
                return;
            }
            
            Trigger curTrigger = TriggerQueue.Dequeue();
            curTrigger.TriggerRule();

            //NotifyListeners(curTrigger);

            foreach (Rule rule in Rules)
            {
                if (rule.IsTriggered())
                {                    
                    if (rule.CheckConditions(this))
                    {
                        curTrigger.UntriggerRule();
                        //Console.WriteLine("Condition is true");
                        rule.DoActions(this);
                        //Console.WriteLine("Action Queue: " + ActionQueue.ToString());
                    }
                }
            }
            curTrigger.UntriggerRule();
            curTrigger.TriggerData.Clear();
        }

        public void NotifyListeners()
        {
            foreach (IListener listener in ListenerList)
            {
                listener.Broadcast(new Trigger("Empty"));
            }
        }

        public void NotifyListeners(Trigger trigger)
        {
            foreach (IListener listener in ListenerList)
            {
                listener.Broadcast(trigger);
            }
        }

        public void AddToTriggerQueue(string triggerName, params KeyValuePair<string, object>[] data) {
            Trigger trigger = TriggerList.First(tr => tr.TriggerName.Equals(triggerName));
            foreach (KeyValuePair<string, object> pair in data)
            {
                trigger.TriggerData[pair.Key] = pair.Value;
            }

            TriggerQueue.Enqueue(trigger);
            NotifyListeners(trigger);
        }

        public ResourceOwner CalculatePlayer(int starting, Orientation orientation)
        {
            if (starting == 0)
            {
                return Machine;
            }

            int playerNum = starting + (int)orientation;
            //int wrappedIndex = ((index % array.Length) + array.Length) % array.Length;
            playerNum = ((playerNum % 4) + 4) % 4;

            Player? player = Players.FirstOrDefault(player => player.playerNumber == playerNum);
            if (player is not null)
            {
                return player;
            }
            else
            {
                return depot;
            }
        }

        //Rule Actions
        public void SetState(string From, string To) {
            State fromState = Enum.Parse<State>(From);
            State toState = Enum.Parse<State>(To);
            CurState = toState;
            AddToTriggerQueue(Constants.OnStateChange, new KeyValuePair<string, object>(Constants.ToState, toState));
        }

        public void SetState_Undo(string From, string To)
        {
            State fromState = Enum.Parse<State>(From);
            CurState = fromState;
        }

        //use the current player + the direction of play to get the next player
        public void GetNextPlayer() {
            //int Pos = Players.IndexOf(CurPlayer);
            int newPos = 0;
            if (Direction == DirectionOfPlay.CounterClockwise) {
                newPos = CurPlayer.playerNumber - 1;
                if (newPos <= 0)
                {
                    newPos = NumPlayers;
                }
            } else
            {
                newPos = CurPlayer.playerNumber + 1;
                if (newPos > NumPlayers)
                {
                    newPos = 1;
                }
            }

            foreach (Player player in Players)
            {
                if (player.playerNumber == newPos)
                {
                    CurPlayer = player;                 
                    if (CurPlayer.IsWolf)
                    {
                        CurPlayer.ActionsRemaining = 2;
                    }
                    else
                    {
                        CurPlayer.ActionsRemaining = 1;
                    }
                }
            }
            AddToTriggerQueue(Constants.OnPlayerChanged);
        }

        public void GetNextPlayer_Undo() {
            CurPlayer.ActionsRemaining = 0;

            int newPos;
            if (Direction == DirectionOfPlay.Clockwise)
            {
                newPos = CurPlayer.playerNumber - 1;
                if (newPos <= 0)
                {
                    newPos = NumPlayers;
                }
            }
            else
            {
                newPos = CurPlayer.playerNumber + 1;
                if (newPos > NumPlayers)
                {
                    newPos = 1;
                }
            }

            foreach (Player player in Players)
            {
                if (player.playerNumber == newPos)
                {
                    CurPlayer = player;
                }
            }
        }

        public void CanPlay()
        {
            //if the player has a skip card in their visible deck -> apply + discard
            for (int i = 0; i < CurPlayer.Visible.Cards.Count; i++)
            {
                ((Trick)CurPlayer.Visible.Cards[i]).Execute(this);
            }

            if (CurState == State.EditMode && !CurPlayer.CanEdit)
            {
                GetNextPlayer();
            }


            if (CurPlayer.ActionsRemaining == 0)
            {
                GetNextPlayer();
                return;
            }
            //Get name and valid actions of curPlayer
            KeyValuePair<string, object> player = new(Constants.Player, CurPlayer);
            KeyValuePair<string, object> actions = new(Constants.Actions, CurPlayer.GetAllValidActions());
            AddToTriggerQueue(Constants.OnWaitingPlayerInput, player, actions);
        }

        public void CanPlay_Undo()
        {

        } 

        public void OnReceivedPlayerInput(IAction action) { 
            //get input from view
            Console.WriteLine("Received input from player: " + CurPlayer.Name);
            curAction = action;
            AddToTriggerQueue(Constants.OnReceivedPlayerInput, new KeyValuePair<string, object>(Constants.Player, CurPlayer.Name));
        }

        public void GiveEdit() {
            //give all the players except the current player the ability to edit 
            foreach (Player Player in Players)
            {
                if (!Player.Equals(CurPlayer)) {
                    Player.SetCanEdit(true);
                }
            }
        }

        public void GiveEdit_Undo()
        {
            foreach (Player Player in Players)
            {
                Player.SetCanEdit(false);
            }
        }

        public void GetNextEdit() {
            CurPlayer.SetCanEdit(false);
            if (Players.Any(player => player.CanEdit))
            {
                GetNextPlayer();
            } else
            {
                SetState(State.EditMode.ToString(), State.ExecuteMode.ToString());
                AddToTriggerQueue(Constants.OnExecuteEnter);
            }
        }

        public void ExecuteGrid() {
            col = 0;
            row = 0;
            inLoop = false;
            curPlacement = GameGrid.Placements.First(placement => placement.Col == col && placement.Row == row);
            Console.WriteLine("Starting execution");
            AddToTriggerQueue(Constants.OnExecuteCard, new KeyValuePair<string, object>(Constants.Placement, curPlacement));
        }
        public void ExecuteGrid_Undo()
        {
            
        }

        public void ExecuteCard()
        {
            curPlacement.Instruction.Execute(this, curPlacement.Orientation);
            if (curPlacement.Instruction.PlacementRule == PlacementRuleType.Sequence)
            {
                List<Placement> nextCol = GameGrid.Placements.FindAll(placement => placement.Col == col);
                Placement? nextMiddle = nextCol.FirstOrDefault(Placement => Placement.Row == 0);
                if (nextMiddle is not null)
                {
                    row = 0;
                    if (inLoop)
                    {
                        col = loopStart;
                    }
                }
            }

            if (col < NumInstructions)
            {
                curPlacement = GameGrid.Placements.First(placement => placement.Row == row && placement.Col == col);
                AddToTriggerQueue(Constants.OnExecuteCard, new KeyValuePair<string, object>(Constants.Placement, curPlacement)); 
            }
            else
            {
                AddToTriggerQueue(Constants.OnGridExecuted, new KeyValuePair<string, object>(Constants.Players, Players));
            }
        }
        public void ExecuteCard_Undo()
        {

        }

        public void CheckFinal()
        {
            if (CurRound == FinalRound)
            {
                AddToTriggerQueue(Constants.OnCalculateEnter);
            }
            else
            {
                CurRound++;
                AddToTriggerQueue(Constants.OnPlacementEnter, new KeyValuePair<string, object>(Constants.CurrentGrid, GameGrid));
            }
        }

        public void CheckFinal_Undo()
        {

        }

        //public void CheckFinalRound() {
        //    if (CurRound == FinalRound)
        //    {
        //        Trigger trigger = TriggerList.First(tr => tr.Equals("isFinalRound"));
        //        TriggerQueue.Enqueue(trigger);
        //    } else
        //    {
        //        Trigger trigger = TriggerList.First(tr => tr.Equals("NotFinalRound"));
        //        TriggerQueue.Enqueue(trigger);
        //    }
        //}

        public void ClearGrid() { 
            GameGrid.Placements.Clear();
        }

        public void ClearGrid_Undo()
        {
          
        }

        //Execute player action and reduce actions remaining
        public void ExecuteAction()
        {
            curAction?.ExecuteAction();
            PlayerActionQueue.Enqueue(curAction);
            CurPlayer.ReduceActions();
            Console.WriteLine("Executed Action");
        }

        public void ExecuteAction_Undo()
        {
            PlayerActionQueue.Dequeue().UndoAction();
            CurPlayer.ActionsRemaining++;
        }

        public void CalculateVP()
        {
            foreach (Player player in Players)
            {
                player.ActivateGoalCard(this);
            }
            AddToTriggerQueue(Constants.OnGoalCardsActivated, new KeyValuePair<string, object>(Constants.Players, Players));

            int maxVP = Players.Max(player => player.VP_Count);
            Player winner = Players.First(player => player.VP_Count == maxVP);
            AddToTriggerQueue(Constants.OnGameCompleted, new KeyValuePair<string, object>(Constants.Player, winner));
        }

        public void CalculateVP_Undo()
        {
            
        }

        //Check actions remaining, if >0 -> wait for input again else get next player
        public void CheckRemainingActions()
        {
            Console.WriteLine($"{CurPlayer.Name} has {CurPlayer.ActionsRemaining} actions remaining");
            if (CurPlayer.ActionsRemaining > 0)
            {
                //Get actions and wait for input
                KeyValuePair<string, object> player = new(Constants.Player, CurPlayer);
                KeyValuePair<string, object> actions = new(Constants.Actions, CurPlayer.GetAllValidActions());
                AddToTriggerQueue(Constants.OnWaitingPlayerInput, player, actions);
            } else
            {
                //GetNextPlayer();
                AddToTriggerQueue(Constants.OnTurnComplete);
            }
            
        }

        public void CheckRemainingActions_Undo()
        {

        }

        public void UpdateDecks()
        {
            if (FaceUp.Cards.Count < 3)
            {
                Card card = Draw.Cards[0];
                Draw.Cards.Remove(card);
                FaceUp.AddCard(card);
            }

            if (Draw.Cards.Count == 0)
            {
                List<Card> discarded = Discard.Cards;
                Discard.Cards = new List<Card>();
                discarded = discarded.OrderBy(x => Guid.NewGuid()).ToList();
                Draw.Cards = discarded;
            }
        }

        public void UpdateDecks_Undo()
        {
            if (true)
            {
                
            }
        }

        //when a card is discarded and the players hand is empty -> draw 3 cards from draw deck
        public void RestockHand()
        {
            if (CurPlayer.Hand.Cards.Count == 0)
            {
                Card cardToDraw = Draw.Cards[0];
                Draw.Cards.Remove(cardToDraw);
                CurPlayer.Hand.AddCard(cardToDraw);

                cardToDraw = Draw.Cards[0];
                Draw.Cards.Remove(cardToDraw);
                CurPlayer.Hand.AddCard(cardToDraw);

                cardToDraw = Draw.Cards[0];
                Draw.Cards.Remove(cardToDraw);
                CurPlayer.Hand.AddCard(cardToDraw);
            }
        }
        public void RestockHand_Undo()
        {
            
        }

        //Rule Conditions
        public bool isFull()
        {
            return GameGrid.isFull();
        }

        public string StateIs()
        {
            return CurState.ToString();
        }

        public bool isHandEmpty()
        {
            return CurPlayer.Hand.Cards.Count == 0;
        }

        //Player Actions - all actions should send out a trigger
        public void DiscardCard(Card cardToDiscard) {
            CurPlayer.Hand.Cards.Remove(cardToDiscard);
            Discard.AddCard(cardToDiscard);
            AddToTriggerQueue(Constants.OnCardDiscarded);
        }

        public void Discard_Undo(Card cardToDiscard)
        {
            CurPlayer.Hand.Cards.Add(cardToDiscard);
            Discard.Cards.Remove(cardToDiscard);
        }

        public void DrawCard(DeckType deckType, int cardIndex)
        {
            Card cardToDraw;
            if (deckType == DeckType.FaceUp)
            {
                cardToDraw = FaceUp.Cards[cardIndex];
                FaceUp.Cards.Remove(cardToDraw);
                CurPlayer.Hand.AddCard(cardToDraw);

                //Refill face up from draw deck
                /* Card card = Draw.Cards[0];
                Draw.Cards.Remove(card);
                FaceUp.AddCard(card); */
            } else
            {
                cardToDraw = Draw.Cards[0];
                Draw.Cards.Remove(cardToDraw);
                CurPlayer.Hand.AddCard(cardToDraw);
            }
            AddToTriggerQueue(Constants.OnCardDrawn);
        }

        public void DrawCard_Undo(DeckType deckType, int cardIndex)
        {
            Card drawnCard = CurPlayer.Hand.Cards[CurPlayer.Hand.Cards.Count - 1];
            if (deckType == DeckType.FaceUp)
            {
                FaceUp.Cards.Insert(cardIndex, drawnCard);
                CurPlayer.Hand.Cards.Remove(drawnCard);
            }
            else
            {
                Draw.Cards.Insert(0, drawnCard);
                CurPlayer.Hand.Cards.Remove(drawnCard);
            }
        }

        public void PlaceInstruction(Placement placement) {
            GameGrid.Placements.Add(placement);
            CurPlayer.Hand.Cards.Remove(placement.Instruction);
            AddToTriggerQueue(Constants.OnCardPlaced, new KeyValuePair<string, object>(Constants.Player, CurPlayer.Name), new KeyValuePair<string, object>(Constants.CurrentGrid, GameGrid));
        }

        public void PlaceInstruction_Undo(Placement placement)
        {
            GameGrid.Placements.Remove(placement);
            CurPlayer.Hand.Cards.Add(placement.Instruction);
        }
    }
}
