using Controller;
using Model;
using Model.Bots;
using Model.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIEvents : MonoBehaviour, IListener
{
    UIDocument doc;
    Button button;
    ScrollView triggerList;
    DropdownField actionDropdown;

    public GameObject evoBotPrefab;
    private Controller.Controller Controller;
    EventCallback<ClickEvent> callback;
    public bool training;



    private void Awake()
    {
        doc = GetComponent<UIDocument>();
        triggerList = doc.rootVisualElement.Q<ScrollView>("TriggerList");
        button = doc.rootVisualElement.Q<Button>("Start");
        actionDropdown = doc.rootVisualElement.Q<DropdownField>("ActionDropdown");

        callback = (ev) => Controller.StartGame();
        button.RegisterCallback(callback);
        
        //Build game
        Controller = new(new List<IListener>() { this });

        List<Player> playerList = new()
        {
            //Instantiate players
            new Player(false, "Random 1", 1)
        };

        //Add evobot
        GameObject evo = Instantiate(evoBotPrefab);
        EvoBotAgent evoAgent = evo.GetComponent<EvoBotAgent>();

        //playerList.Add(evoAgent.InitializeBot(false, "Evo", 2));

        Controller.SetPlayers(playerList);
        Controller.SetupGame();
    }

    public void AddToList(string message)
    {
        Label label = new Label(message);
        label.style.fontSize = 25;
        label.style.color = Color.blue;
        triggerList.hierarchy.Add(label);
    }

    public void RegisterCallback(EventCallback<ClickEvent> newCallback)
    {
        button.UnregisterCallback(callback);
        callback = newCallback;
        button.RegisterCallback(callback);
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

            case Constants.OnWaitingPlayerInput:
                Player curPlayer = (Player)trigger.TriggerData[Constants.Player];
                if (!curPlayer.GetType().IsSubclassOf(typeof(Bot)))
                {
                    GetPlayerInput(curPlayer.Name, (List<IAction>)trigger.TriggerData[Constants.Actions]);
                }
                break;

            case Constants.OnCardPlaced:
                CardPlaced((string)trigger.TriggerData[Constants.Player], (GameGrid)trigger.TriggerData[Constants.CurrentGrid]);
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
                Controller.UpdateGame();
                break;

            case Constants.OnTurnComplete:
                //AnsiConsole.MarkupLine("[green]Player's turn is complete[/]");
                Controller.UpdateGame();
                break;

            case Constants.OnStateChange:
                Controller.UpdateGame();
                break;

            case Constants.OnExecuteCard:
                Placement placement = (Placement)trigger.TriggerData[Constants.Placement];
                Debug.Log($"Executing card '{placement.Instruction.Name}' at (Row: {placement.Row}, Column {placement.Col})");
                button.text = "Execute card";
                void newCallback(ClickEvent ev)
                {
                    Controller.UpdateGame();
                }
                RegisterCallback(newCallback);
                break;

            case Constants.OnGridExecuted:
                DisplayPlayers((List<Player>)trigger.TriggerData[Constants.Players]);
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
                AddToList($"The winner is [green]{winner.Name}[/] with {winner.VP_Count} victory points!");
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

    private void DisplayPlayers(List<Player> players)
    {
        string labelText = "";
        foreach (Player player in players)
        {
            labelText += player.Name;
        }
    }

    private void CardPlaced(string player, GameGrid gameGrid)
    {
        //AnsiConsole.MarkupLine($"Player [green]{player}[/] has placed a card");
        Controller.UpdateGame();
        //DisplayGrid(gameGrid);
    }

    private void GetPlayerInput(string playerName, List<IAction> actions)
    {
        AddToList($"Waiting for input from {playerName}");
        button.text = "Select action";
        actionDropdown.choices = actions.Select(action => action.GetDescription()).ToList();
        void newCallback(ClickEvent ev)
        {
            Debug.Log(actions[actionDropdown.index].GetDescription());
        }
        RegisterCallback(newCallback);
    }
}
