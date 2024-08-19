using Controller;
using Model;
using Model.Bots;
using Model.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIEvents : MonoBehaviour, IListener
{
    UIDocument doc;
    Button button;
    ScrollView triggerList;
    public GameObject evoBotPrefab;
    private Controller.Controller Controller;
    EventCallback<ClickEvent> callback;



    private void Awake()
    {
        doc = GetComponent<UIDocument>();
        triggerList = doc.rootVisualElement.Q<ScrollView>("TriggerList");
        button = doc.rootVisualElement.Q<Button>("Start");
        callback = (ev) => Controller.StartGame();
        button.RegisterCallback(callback);
        
        
        //Build game
        Controller = new(new List<IListener>() { this });

        List<Player> playerList = new()
        {
            //Instantiate players
            new RandomBot(false, "Random 1", 1)
        };

        //Add evobot
        GameObject evo = Instantiate(evoBotPrefab);
        EvoBotAgent evoAgent = evo.GetComponent<EvoBotAgent>();

        playerList.Add(evoAgent.InitializeBot(false, "Evo", 2));

        Controller.SetPlayers(playerList);
        Controller.SetupGame();
    }

    public void AddToList(string trigger)
    {
        Label label = new Label("Clicked");
        label.style.fontSize = 25;
        triggerList.hierarchy.Add(label);
    }

    

    //Game manager
    public void Broadcast(Trigger trigger)
    {
        Label label = new Label(trigger.TriggerName);
        label.style.fontSize = 25;
        triggerList.hierarchy.Add(label);

        //Handle triggers
        switch (trigger.TriggerName)
        {
            case Constants.OnBuilt:
                Controller.UpdateGame();
                break;

            case Constants.OnGameStarted:
                //DisplayGrid((GameGrid)trigger.TriggerData[Constants.CurrentGrid]);
                //Temp
                DisplayPlayers((List<Player>)trigger.TriggerData[Constants.Players]);
                Controller.UpdateGame();
                break;

                //case Constants.OnWaitingPlayerInput:
                //    Player curPlayer = (Player)trigger.TriggerData[Constants.Player];
                //    if (!curPlayer.GetType().IsSubclassOf(typeof(Bot)))
                //    {
                //        GetPlayerInput(curPlayer.Name, (List<IAction>)trigger.TriggerData[Constants.Actions]);
                //    }
                //    AnsiConsole.MarkupLine("[green]Bot turn...[/]");
                //    break;

                //case Constants.OnCardPlaced:
                //    CardPlaced((string)trigger.TriggerData[Constants.Player], (GameGrid)trigger.TriggerData[Constants.CurrentGrid]);
                //    break;

                //case Constants.OnCardDiscarded:
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnCardDrawn:
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnPlayerChanged:
                //    AnsiConsole.MarkupLine("[green]Player has been changed[/]");
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnReceivedPlayerInput:
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnTurnComplete:
                //    AnsiConsole.MarkupLine("[green]Player's turn is complete[/]");
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnStateChange:
                //    AnsiConsole.MarkupLine($"[bold]Moving to [green]{trigger.TriggerData[Constants.ToState]}[/][/]");
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnExecuteCard:
                //    Placement placement = (Placement)trigger.TriggerData[Constants.Placement];
                //    AnsiConsole.MarkupLine($"Executing card '{placement.Instruction.Name}' at (Row: {placement.Row}, Column {placement.Col})");
                //    if (!AnsiConsole.Confirm("Continue?"))
                //    {
                //        AnsiConsole.MarkupLine("Ending");
                //        return;
                //    }
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnGridExecuted:
                //    DisplayPlayers((List<Player>)trigger.TriggerData[Constants.Players]);
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnExecuteEnter:
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnPlacementEnter:
                //    DisplayGrid((GameGrid)trigger.TriggerData[Constants.CurrentGrid]);
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnGoalCardsActivated:
                //    DisplayGoalCardStatus((List<Player>)trigger.TriggerData[Constants.Players]);
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnCalculateEnter:
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnGameCompleted:
                //    Player winner = (Player)trigger.TriggerData[Constants.Player];
                //    AnsiConsole.MarkupLine($"The winner is [green]{winner.Name}[/] with {winner.VP_Count} victory points!");
                //    controller.UpdateGame();
                //    break;

                //case Constants.OnCardRotated:
                //case Constants.OnCardSwap:
                //case Constants.OnPlayerSkipped:
                //case Constants.OnChangeDirection:
                //    controller.UpdateGame();
                //    break;
        }
    }

    private void DisplayPlayers(List<Player> players)
    {
        string labelText = "";
        foreach (Player player in players)
        {
            labelText += player.Name;
        }
    }
}
