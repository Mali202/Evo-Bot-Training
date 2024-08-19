using Model;
using Model.Goals;
using Model.Instructions;
using Model.PlayerActions;
using Model.Tricks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace Controller
{
    public class GameBuilder
    {
        readonly Game Game;
        public List<Card> LoadedCards { get; set; }
        public List<Card> ActionCards { get; set; }
        public List<Goal> LoadedGoalCards { get; set; }
        public List<Trick> LoadedTrickCards { get; set; }
        public List<Goal> Goals { get; set; }

        public GameBuilder() {
            Game = new Game();
        }

        public GameBuilder SetNumOfInstuctions(int NumOfInstructions) { 
            Game.NumInstructions = NumOfInstructions; 
            return this;
        }

        public GameBuilder InitialisePlayers(List<Player> players)
        {
            Game.NumPlayers = players.Count;
            Game.Players = players;

            foreach (Player player in Game.Players)
            {
                player.BrickCount = 3;
                player.WoodCount = 3;
                player.StrawCount = 3;
            }

            Game.CurPlayer = Game.Players.First(player => player.playerNumber == 1);

            if (Game.NumPlayers == 4) { 
                Game.WolfPresent = true;
            }
            return this;
        }

        public GameBuilder SetMaxWood(int MaxWood) { 
            Game.MaxWood = MaxWood ;
            return this;
        }

        public GameBuilder SetMaxStraw(int MaxStraw)
        {
            Game.MaxStraw = MaxStraw ;
            return this;
        }

        public GameBuilder SetMaxBrick(int MaxBrick)
        {
            Game.MaxBrick = MaxBrick ;
            return this;
        }

        public GameBuilder SetFinalRound(int FinalRound) { 
            Game.FinalRound = FinalRound ;
            return this;
        }

        public Game Build() { 
            Game.CurRound = 1;
            Game.Direction = DirectionOfPlay.Clockwise;
            Game.CurState = State.GameSetup;

            Game.Machine = new Machine(Game.MaxWood, Game.MaxBrick, Game.MaxStraw);
            Game.depot = new ResourceOwner();
            
            Game.GameGrid = new GameGrid(Game.NumInstructions);

            Game.TriggerQueue = new Queue<Trigger>();
            Game.ActionQueue = new Queue<Model.Action>();
            Game.PlayerActionQueue = new Queue<IAction>();

            return Game;
        }

        public GameBuilder LoadTriggers(string path)
        {
            Game.TriggerList = new List<Trigger>();
            //Game.TriggerList.Add();
            JsonNode? json = JsonNode.Parse(File.ReadAllText(path)) ?? throw new ArgumentException("JSON error");
            foreach (JsonNode? trigger in json.AsArray())
            {
                if (trigger is null)
                {
                    throw new ArgumentException("JSON error");
                }

                Game.TriggerList.Add(new Trigger(trigger.GetValue<string>()));
            }

            Console.WriteLine("Triggers");
            foreach (Trigger trigger in Game.TriggerList)
            {
                Console.WriteLine(trigger.TriggerName);
            }

            Console.WriteLine();
            return this;
        }

        public GameBuilder LoadRules(string path)
        {
            //Initialize Game.rules
            Game.Rules = new List<Rule>();

            JsonNode? json = JsonNode.Parse(File.ReadAllText(path)) ?? throw new ArgumentException("JSON error");
            json.AsObject().TryGetPropertyValue("Rules", out JsonNode? value);

            if (value is null) {
                throw new ArgumentException("JSON error");
            }

            //Iterate through rules
            foreach (var rule in value.AsArray())
            {
                Rule newRule = new();

                if (rule is null)
                {
                    throw new ArgumentException("JSON error");
                }

                //Iterate through triggers
                rule.AsObject().TryGetPropertyValue("Triggers", out JsonNode? jsonTriggers);
                if (jsonTriggers is null)
                {
                    throw new ArgumentException("JSON error");
                }
                //Trigger newTrigger = new() { TriggerName = jsonTriggers.AsValue().ToString() };
                foreach (var trigger in jsonTriggers.AsArray())
                {
                    if (trigger is null)
                    {
                        throw new ArgumentException("JSON error");
                    }
                    
                    if (trigger.GetValueKind() == System.Text.Json.JsonValueKind.String)
                    {
                        Trigger newTrigger = Game.TriggerList.First(tr => tr.TriggerName == trigger.AsValue().ToString());
                        newRule.Triggers.Add(newTrigger);
                    } else if(trigger.GetValueKind() == System.Text.Json.JsonValueKind.Object) {
                        IEnumerator<KeyValuePair<string, JsonNode?>> enumerator = trigger.AsObject().GetEnumerator();
                        enumerator.MoveNext();
                        newRule.Triggers.Add(new Trigger(enumerator.Current.Key));
                    }    
                }
                Console.WriteLine("Rule triggers:");
                Console.WriteLine(string.Join(',', newRule.Triggers.Select(trigger => trigger.TriggerName).ToList()));
                Console.WriteLine();
                
                //Iterate through actions
                rule.AsObject().TryGetPropertyValue("Actions", out JsonNode? jsonActions);
                if (jsonActions is null)
                {
                    throw new ArgumentException("JSON error");
                }

                IEnumerator<KeyValuePair<string, JsonNode?>> actions = jsonActions.AsObject().GetEnumerator();
                while (actions.MoveNext())
                {
                    Model.Action newAction = new(actions.Current.Key);
                    IEnumerator<KeyValuePair<string, JsonNode?>> parameters = actions.Current.Value.AsObject().GetEnumerator();
                    while (parameters.MoveNext())
                    {
                        newAction.Parameters.Add(parameters.Current.Value.GetValue<string>());
                    }
                    newRule.Actions.Add(newAction);

                }

                //Iterate through rule conditions
                rule.AsObject().TryGetPropertyValue("Conditions", out JsonNode? jsonConditions);
                if (jsonConditions is null)
                {
                    throw new ArgumentException("JSON error");
                }

                IEnumerator<KeyValuePair<string, JsonNode?>> conditions = jsonConditions.AsObject().GetEnumerator();
                while (conditions.MoveNext())
                {
                    switch (conditions.Current.Value.GetValueKind())
                    {
                        case System.Text.Json.JsonValueKind.String:
                            newRule.Conditions.Add(conditions.Current.Key, conditions.Current.Value.AsValue().GetValue<string>());
                            break;

                        case System.Text.Json.JsonValueKind.True:
                        case System.Text.Json.JsonValueKind.False:
                            newRule.Conditions.Add(conditions.Current.Key, conditions.Current.Value.AsValue().GetValue<bool>());
                            break;                      
                    }
                    
                }

                Console.WriteLine("Actions:");
                Console.WriteLine(string.Join(',', newRule.Actions.Select(action => action.ActionName).ToList()));
                Console.WriteLine("Conditions:");
                Console.WriteLine(string.Join(',', newRule.Conditions.Select(condition => condition.Key + " = " + condition.Value).ToList()));
                Console.WriteLine();
    
                Game.Rules.Add(newRule);
            }

            return this;
        }

        //../../../Cards
        public GameBuilder LoadInstructionCards(string folderPath)
        {
            LoadedCards = new();
            Console.WriteLine("Loading Instruction Cards");
            Console.WriteLine();

            //Iterate through instructions and add to global list of cards
            foreach (string filename in Directory.EnumerateFiles(folderPath + "/Instruction", "*.json"))
            {
                JsonNode? json = JsonNode.Parse(File.ReadAllText(filename)) ?? throw new ArgumentException("JSON error"); 
                json.AsObject().TryGetPropertyValue("Id", out JsonNode? Id);

                if (json is null)
                {
                    throw new ArgumentException("JSON error");
                }


                Dictionary<string, JsonNode?> pairs = json.AsObject().ToDictionary((kv) => kv.Key, (kv) => kv.Value);
                
                JsonNode? value;
                Instruction newInstruction;
                switch (pairs["Id"].ToString()[..3])
                {
                    case "Ibl":
                        Blueprint blueprint = new();
                        value = pairs["VP"];
                        if (value is not null)
                        {
                            blueprint.VP = value.GetValue<int>();
                        }

                        value = pairs["Player"];
                        if (value is not null)
                        {
                            blueprint.Player = value.GetValue<int>();
                        }
                        
                        newInstruction = blueprint;
                        break;

                    case "Ico":
                        Condition condition = new();
                        value = pairs["Player"];
                        if (value is not null)
                        {
                            condition.Player = value.GetValue<int>();
                        }

                        newInstruction = condition;
                        break;

                    case "Ilo":
                        Loop loop = new();
                        value = pairs["Player"];
                        if (value is not null)
                        {
                            loop.Player = value.GetValue<int>();
                        }
                        newInstruction = loop;
                        break;

                    case "Ist":
                        newInstruction = new Steal();
                        break;

                    case "Itr":
                        Transfer transfer = new();
                        value = pairs["From"];
                        if (value is not null)
                        {
                            transfer.FromPlayer = value.GetValue<int>();
                        }

                        value = pairs["To"];
                        if (value is not null)
                        {
                            transfer.ToPlayer = value.GetValue<int>();
                        }
                        newInstruction = transfer;
                        break;
                    default:
                        throw new ArgumentException("ID is not in correct format"); 
                }

                value = pairs.GetValueOrDefault("Brick", null);
                if (value is not null)
                {
                    newInstruction.Brick = value.GetValue<int>();
                }

                value = pairs.GetValueOrDefault("Wood", null);
                if (value is not null)
                {
                    newInstruction.Wood = value.GetValue<int>();
                }

                value = pairs.GetValueOrDefault("Straw", null);;
                if (value is not null)
                {
                    newInstruction.Straw = value.GetValue<int>();
                }

                value = pairs["Rotatable"];
                if (value is not null)
                {
                    newInstruction.Rotatable = value.GetValue<bool>();
                }

                value = pairs["PlacementRuleType"];
                if (value is not null)
                {
                    PlacementRuleType placement = Enum.Parse<PlacementRuleType>(value.GetValue<string>());
                    newInstruction.PlacementRule = placement;
                }
                newInstruction.Name = pairs["Name"].ToString();

                Console.WriteLine("Loaded: " + pairs["Name"].ToString());
                LoadedCards.Add(newInstruction);
            }
            return this;
        }

        public GameBuilder LoadTrickCards(string folderPath)
        {
            Console.WriteLine("Loading Trick Cards");
            Console.WriteLine();

            foreach (string filename in Directory.EnumerateFiles(folderPath + "/Tricks", "*.json")) {
                JsonNode? json = JsonNode.Parse(File.ReadAllText(filename)) ?? throw new ArgumentException("JSON error");
                json.AsObject().TryGetPropertyValue("Id", out JsonNode? Id);

                if (json is null)
                {
                    throw new ArgumentException("JSON error");
                }

                Dictionary<string, JsonNode?> pairs = json.AsObject().ToDictionary((kv) => kv.Key, (kv) => kv.Value);
                Trick newTrick = pairs["Id"].ToString() switch
                {
                    "Tsk" => new Skip(),
                    "Tcd" => new ChangeDirection(),
                    "Tro" => new Rotate(),
                    "Tsw" => new Swap(),
                    _ => throw new ArgumentException("ID is not in correct format"),
                };

                Console.WriteLine("Loaded: " + pairs["Name"].ToString());
                newTrick.Name = pairs["Name"].ToString();

                LoadedCards.Add(newTrick);
            }
            return this;
        }

        public GameBuilder LoadActionCards(string folderPath) {
            ActionCards = new List<Card>();
            Console.WriteLine("Loading Action Cards");
            Console.WriteLine();

            foreach (string filename in Directory.EnumerateFiles(folderPath + "/PlayerAction", "*.json")) {
                JsonNode? json = JsonNode.Parse(File.ReadAllText(filename)) ?? throw new ArgumentException("JSON error"); 
                json.AsObject().TryGetPropertyValue("Id", out JsonNode? Id);

                if (json is null)
                {
                    throw new ArgumentException("JSON error");
                }

                Dictionary<string, JsonNode?> pairs = json.AsObject().ToDictionary((kv) => kv.Key, (kv) => kv.Value);


                switch (pairs["Id"].ToString())
                {
                    case "PAph":
                        ActionCards.Add(new PlaceFromHand());
                        break;

                    case "PAdi":
                        ActionCards.Add(new Discard());
                        break;

                    case "PAdr":
                        ActionCards.Add(new Draw());
                        break;
                    default:
                        throw new ArgumentException("ID is not in correct format");
                }              
            }

            return this;
        }

        public GameBuilder LoadGoalCards(string folderPath) {
            LoadedGoalCards = new List<Goal>();
            Console.WriteLine("Loading Goal Cards");
            Console.WriteLine();

            foreach (string filename in Directory.EnumerateFiles(folderPath + "/Goal", "*.json")) {
                JsonNode? json = JsonNode.Parse(File.ReadAllText(filename)) ?? throw new ArgumentException("JSON error"); 
                json.AsObject().TryGetPropertyValue("Id", out JsonNode? Id);

                if (json is null)
                {
                    throw new ArgumentException("JSON error");
                }

                Dictionary<string, JsonNode?> pairs = json.AsObject().ToDictionary((kv) => kv.Key, (kv) => kv.Value);

                Goal newGoal;
                JsonNode? value;
                switch (pairs["Id"].ToString()[..2])
                {
                    case "Gm":
                        newGoal = new Max();
                        break;

                    case "Gc":
                        newGoal = new Model.Goals.Convert();
                        value = pairs["NumResources"];
                        if (value is not null)
                        {
                            newGoal.NumResources = value.GetValue<int>();
                        }
                        break;
                    default:
                        throw new ArgumentException("ID is not in correct format");
                }
                
                value = pairs["Name"];
                if (value is not null)
                {
                    newGoal.Name = value.GetValue<string>();
                }

                value = pairs["VP"];
                if (value is not null)
                {
                    newGoal.VP = value.GetValue<int>();
                }

                value = pairs["ResourceType"];
                if (value is not null)
                {
                    newGoal.ResourceType = Enum.Parse<ResourceType>(value.GetValue<string>());
                }

                LoadedGoalCards.Add(newGoal);
            }

            Console.WriteLine("Goal cards:");
            foreach (Goal goal in LoadedGoalCards)
            {
                Console.WriteLine(goal.Name);
            }

            return this;
        }

        public GameBuilder DistributeCards()
        {
            Game.FaceUp = new Deck(DeckType.FaceUp, AllowedCardType.Combination);
            Game.Discard = new Deck(DeckType.Discard, AllowedCardType.Combination);
            Game.Draw = new Deck(DeckType.Draw, AllowedCardType.Combination);

            Game.GameCards = new List<Card>();

            //Cloning of cards
            foreach (Card card in LoadedCards)
            {
                for (int i = 0; i < 3; i++)
                {
                    Game.GameCards.Add(card.CloneCard());
                }
            }

            //Distribute action cards
            //foreach (Player player in Game.Players)
            //{
            //    foreach (Card card in ActionCards)
            //    {
            //        player.Action.AddCard(card.CloneCard());
            //    }
            //}

            //Distribute remaining cards --> MUST USE RANDOM NUMBER TABLE
            Game.GameCards = Game.GameCards;

            foreach (Player player in Game.Players)
            {
                for (int i = 0; i < 5; i++)
                {
                    player.Hand.AddCard(Game.GameCards[0]);
                    Game.GameCards.RemoveAt(0);
                }

                player.Action.Cards.AddRange(ActionCards);
            }

            //face up - 3 cards all three visible to player
            for (int i = 0; i < 3; i++)
            {
                Game.FaceUp.AddCard(Game.GameCards[0]);
                Game.GameCards.RemoveAt(0);
            }

            for (int i = 0; i < Game.GameCards.Count(); i++)
            {
                Game.Draw.AddCard(Game.GameCards[0]);
                Game.GameCards.RemoveAt(0);
            }

            //duplicate goal cards
            Goals = new List<Goal>();
            foreach (Goal goalCard in LoadedGoalCards)
            {
                for (int i = 0; i < 3; i++)
                {
                    Goals.Add((Goal)goalCard.CloneCard());
                }
            }

            Goals = Goals.OrderBy(x => Guid.NewGuid()).ToList();
            //distribute goal cards
            foreach (Player player in Game.Players)
            {
                Goal goal = Goals[0];
                Goals.RemoveAt(0);
                player.SetGoal(goal);
            }

            Console.WriteLine("Player cards:");
            foreach (Player player in Game.Players)
            {
                Console.WriteLine(player.Name);
                foreach (Card card in player.Hand.Cards)
                {
                    Console.WriteLine(card.Name);
                }
                Console.WriteLine();
            }

            Console.WriteLine("Face up cards:");
            foreach (Card card in Game.FaceUp.Cards)
            {
                Console.WriteLine(card.Name);
            }

            return this;
        }
    }
}
