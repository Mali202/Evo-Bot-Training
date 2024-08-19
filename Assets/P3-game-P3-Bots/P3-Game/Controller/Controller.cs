using Model;
using Model.Bots;
using Model.Generators;
using Model.PlayerActions;
using Model.Tricks;
using Model.Utils;
using System;
using System.Collections.Generic;

namespace Controller
{
    public class Controller
    {
        private Game Game;
        private List<Player> Players;
        private List<IListener> ViewListeners;

        public Controller(List<IListener> listeners) {
            ViewListeners = listeners;
        }

        public void SetupGame() {
            Game = new GameBuilder()
                .LoadTriggers("Assets/Config/Triggers.json")
                .LoadRules("Assets/Config/Dummy.json")
                .LoadActionCards("Assets/Config/Cards")
                .LoadGoalCards("Assets/Config/Cards")
                .LoadInstructionCards("Assets/Config/Cards")
                .LoadTrickCards("Assets/Config/Cards")
                .InitialisePlayers(Players)
                .DistributeCards()
                .SetNumOfInstuctions(7)
                .SetMaxBrick(10)
                .SetMaxWood(10)
                .SetMaxStraw(10)
                .SetFinalRound(2)
                .Build();

            //Register listeners from view, possibly more than one
            ViewListeners.ForEach(RegisterListener);

            //Load action generator
            ActionGenerator actionGenerator = ActionGenerator.Instance;
            actionGenerator.Game = Game;
            actionGenerator.generatorKey.Add(typeof(Skip), new SkipActionGenerator());
            actionGenerator.generatorKey.Add(typeof(Swap), new SwapActionGenerator());
            actionGenerator.generatorKey.Add(typeof(Replace), new ReplaceActionGenerator());
            actionGenerator.generatorKey.Add(typeof(Rotate), new RotateActionGenerator());
            actionGenerator.generatorKey.Add(typeof(ChangeDirection), new ChangeDirectionActionGenerator());
            actionGenerator.generatorKey.Add(typeof(Instruction), new InstructionActionGenerator());
            actionGenerator.generatorKey.Add(typeof(Draw), new DrawActionGenerator());
            actionGenerator.generatorKey.Add(typeof(Discard), new DiscardActionGenerator());
            actionGenerator.generatorKey.Add(typeof(PlaceFromHand), new PlaceFromHandActionGenerator());

            //Setup Instruction validator
            InstructionValidator.game = Game;
            Bot.SetGame(Game);

            //Trigger onBuilt
            Game.AddToTriggerQueue(Constants.OnBuilt);
        }

        public void PopulatePlayers(List<string> Playerlist, string Wolf) {
            Players = new List<Player>();
            for (int i = 0; i < Playerlist.Count; i++)
            {
                Player Player;
                string[] parts = Playerlist[i].Split(':', StringSplitOptions.None);
                switch (parts[1])
                {
                    case Constants.RandomBot:
                        RandomBot randomBot = new RandomBot(Wolf != "", parts[0], i + 1);
                        Player = randomBot;
                        ViewListeners.Add(randomBot);
                        break;

                    case Constants.ProbabilityBot:
                        ProbabilityBot probabilityBot = ProbabilityBot.Trickster(Wolf != "", parts[0], i + 1);
                        Player = probabilityBot;
                        ViewListeners.Add(probabilityBot);
                        break;

                    case Constants.CraftedBot:
                        CraftedBot craftedBot = new CraftedBot(Wolf != "", parts[0], i + 1);
                        Player = craftedBot;
                        ViewListeners.Add(craftedBot);
                        break;

                    case Constants.EvoBot:
                        Console.WriteLine("Evo Bot not implemented yet");
                        Player = new Player(Wolf != "", parts[0], i + 1);
                        break;

                    case "":
                        Player = new Player(Wolf != "", parts[0], i + 1);
                        break;

                    default:
                        throw new NotImplementedException();
                }
                Players.Add(Player);
            }
        }

        public void SetPlayers(List<Player> Playerlist)
        {
            Players = new List<Player>();
            foreach (Player player in Playerlist)
            {
                if (player is IListener)
                {
                    ViewListeners.Add(player as IListener);
                }
                Players.Add(player);
            }
        }

        public void RegisterListener(IListener listener)
        {
            Game.AddListener(listener);
        }

        public void UpdateGame()
        {
            Game.Update();
        }

        public void StartGame()
        {
            Game.AddToTriggerQueue(Constants.OnGameStarted, new KeyValuePair<string, object>(Constants.CurrentGrid, Game.GameGrid), new KeyValuePair<string, object>(Constants.Players, Game.Players));
        }

        public void PlayerInput(IAction action)
        {
            Game.OnReceivedPlayerInput(action);
        }
    }
}
