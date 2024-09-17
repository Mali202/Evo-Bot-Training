using Model;
using Model.Bots;
using Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Training
{
    public class Tournament : IListener
    {
        private Controller.Controller Controller;
        public Dictionary<string, List<string>> winners;
        public string gameType;

        public Tournament()
        {
            winners = new Dictionary<string, List<string>>();
        }

        public void RunGame(List<Player> playerList) {
            Controller = new(new List<IListener>() { this });
            gameType = playerList.Count switch
            {
                2 => "2P-",
                3 => "3P-",
                4 => "4P-",
                _ => "Unknown"
            };
            gameType += string.Join(", ", playerList.Select(p => p.Name));
            Controller.SetPlayers(playerList);
            Controller.SetupGame();
            Controller.StartGame();
        }

        public void SaveStats()
        {
            StringBuilder sb = new();
            string path = "Assets/Resources/stats.txt";
            StreamWriter writer = new StreamWriter(path, true);
            
            foreach (var (key, value) in winners)
            {
                sb.AppendLine($"Game Type: {key}");
                foreach (var winner in value)
                {
                    sb.AppendLine($"Winner: {winner}");
                }
            }
            writer.Write(sb.ToString());
            writer.Close();
            Debug.Log(sb.ToString());
        }

        public void Broadcast(Trigger trigger)
        {
            switch (trigger.TriggerName)
            {
                case Constants.OnBuilt:
                    Controller.UpdateGame();
                    break;

                case Constants.OnGameStarted:
                    //DisplayGrid((GameGrid)trigger.TriggerData[Constants.CurrentGrid]);
                    //Temp
                    //DisplayPlayers((List<Player>)trigger.TriggerData[Constants.Players]);
                    Debug.Log("Game has started");
                    Controller.UpdateGame();
                    break;

                case Constants.OnWaitingPlayerInput:
                    Player curPlayer = (Player)trigger.TriggerData[Constants.Player];
                    //if (!curPlayer.GetType().IsSubclassOf(typeof(Bot)))
                    //{
                    //    GetPlayerInput(curPlayer.Name, (List<IAction>)trigger.TriggerData[Constants.Actions]);
                    //}
                    break;

                case Constants.OnCardPlaced:
                    //CardPlaced((string)trigger.TriggerData[Constants.Player], (GameGrid)trigger.TriggerData[Constants.CurrentGrid]);
                    Controller.UpdateGame();
                    break;

                case Constants.OnCardDiscarded:
                    Controller.UpdateGame();
                    break;

                case Constants.OnCardDrawn:
                    Controller.UpdateGame();
                    break;

                case Constants.OnPlayerChanged:
                    //AnsiConsole.MarkupLine("[green]Player has been changed[/]");
                    Controller.UpdateGame();
                    break;

                case Constants.OnReceivedPlayerInput:
                    string playerName = (string)trigger.TriggerData[Constants.Player];
                    Debug.Log($"Received input from {playerName}");
                    Controller.UpdateGame();
                    break;

                case Constants.OnTurnComplete:
                    Debug.Log("[green]Player's turn is complete[/]");
                    Controller.UpdateGame();
                    break;

                case Constants.OnStateChange:
                    Controller.UpdateGame();
                    break;

                case Constants.OnExecuteCard:
                    Placement placement = (Placement)trigger.TriggerData[Constants.Placement];
                    Debug.Log($"Executing card '{placement.Instruction.Name}' at (Row: {placement.Row}, Column {placement.Col})");
                    Controller.UpdateGame();
                    break;

                case Constants.OnGridExecuted:
                    //DisplayPlayers((List<Player>)trigger.TriggerData[Constants.Players]);
                    Controller.UpdateGame();
                    break;

                case Constants.OnExecuteEnter:
                    Controller.UpdateGame();
                    break;

                case Constants.OnPlacementEnter:
                    //DisplayGrid((GameGrid)trigger.TriggerData[Constants.CurrentGrid]);
                    Controller.UpdateGame();
                    break;

                case Constants.OnGoalCardsActivated:
                    //DisplayGoalCardStatus((List<Player>)trigger.TriggerData[Constants.Players]);
                    Controller.UpdateGame();
                    break;

                case Constants.OnCalculateEnter:
                    Controller.UpdateGame();
                    break;

                case Constants.OnGameCompleted:
                    Player winner = (Player)trigger.TriggerData[Constants.Player];
                    //AddToList($"The winner is {winner.Name} with {winner.VP_Count} victory points!");
                    Debug.Log($"The winner is {winner.Name} with {winner.VP_Count} victory points!");
                    if (winners.ContainsKey(gameType))
                    {
                        winners[gameType].Add(winner.Name);
                    }
                    else
                    {
                        winners.Add(gameType, new List<string> { winner.Name });
                    }
                    Controller.UpdateGame();
                    break;

                case Constants.OnCardRotated:
                case Constants.OnCardSwap:
                case Constants.OnPlayerSkipped:
                case Constants.OnChangeDirection:
                    Controller.UpdateGame();
                    break;
            }
        }
    }
}
