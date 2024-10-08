using Assets.Training;
using Controller;
using Model;
using Model.Bots;
using Model.Instructions;
using Model.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;
using UnityEngine.UIElements;

public class UIEvents : MonoBehaviour, IListener
{
    UIDocument doc;
    Button button;
    ScrollView triggerList;
    DropdownField actionDropdown;
    Label playerStats;

    public EvoBotAgent evoBot;
    private Controller.Controller Controller;
    EventCallback<ClickEvent> callback;
    public bool isTraining;
    private Lesson lesson;

    private bool stop = false;
    private int updates;

    private void Awake()
    {
        doc = GetComponent<UIDocument>();
        triggerList = doc.rootVisualElement.Q<ScrollView>("TriggerList");
        button = doc.rootVisualElement.Q<Button>("Start");
        actionDropdown = doc.rootVisualElement.Q<DropdownField>("ActionDropdown");
        playerStats = doc.rootVisualElement.Q<Label>("Info");

        callback = (ev) => Controller.StartGame();
        button.RegisterCallback(callback);        

        if (isTraining)
        {
            Debug.Log("Training mode");
            Academy.Instance.AutomaticSteppingEnabled = false;
            evoBot.OnStartGame += TrainingLoop;
            //Academy.Instance.EnvironmentStep();
            //ResetEnvironment();
        }
        else
        {
            Debug.Log("Evaluation mode");
            Controller = new(new List<IListener>() { this });
            List<Player> playerList = new()
            {
                //Instantiate players
                evoBot.InitializeBot(lesson.IsWolf, "Evo", 1),
            };
        }

    }

    private void Start()
    {
        if (isTraining)
        {        
            StartCoroutine(StartTraining());
        }
    }

    IEnumerator StartTraining()
    {
        while (evoBot.CompletedEpisodes <= evoBot.MaxEpisodes)
        {
            TrainingLoop();
            yield return null;
        }
        Debug.Log("Training complete");
    }

    private void TrainingLoop()
    {
        ResetEnvironment();
        StartGame();
    }

    private void TrainingGame()
    {
        StartGame();
        //evoBot.EndEpisode();
    }

    private void EndGame()
    {
        evoBot.EpisodeInterrupted();
    }

    private void ResetEnvironment()
    {
        Debug.Log($"Resetting Environment for episode {evoBot.CompletedEpisodes}");

        //Build game
        Controller = new(new List<IListener>() { this });

        lesson = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("game_config", 0) switch
        {
            0 => TrainingConfigs.PlaceBlueprint,
            1 => TrainingConfigs.PlaceTransfer,
            2 => TrainingConfigs.PlaceInstruction,
            3 => TrainingConfigs.TwoPlayerGame,
            _ => TrainingConfigs.TwoPlayerGame,
        };

        Debug.Log("Lesson: " + lesson.Name);

        List<Player> playerList = new()
        {
            //Instantiate players
            evoBot.InitializeBot(lesson.IsWolf, "Evo", 1),
        };

        for (int i = 2; i <= lesson.NumPlayers; i++)
        {
            playerList.Add(new RandomBot(!lesson.IsWolf && i == 4, $"Random {i}", i));
        }

        Controller.SetPlayers(playerList);
        Controller.SetupGame();
        ConfigureGame();
    }

    public void ConfigureGame()
    {
        switch (lesson.HandConfig)
        {
            case HandConfig.BlueprintOnly:
                MoveCards(evoBot.bot.Hand.Cards, Controller.Game.Draw.Cards, (card) => card is Blueprint);
                break;
            case HandConfig.TransferOnly:
                MoveCards(evoBot.bot.Hand.Cards, Controller.Game.Draw.Cards, (card) => card is Transfer);
                break;
            case HandConfig.InstructionOnly:
                MoveCards(evoBot.bot.Hand.Cards, Controller.Game.Draw.Cards, (card) => card is Instruction);
                break;
        }
    }

    private void MoveCards(List<Card> source, List<Card> destination, Predicate<Card> match)
    {
        //Remove cards from hand that don't match the predicate
        for (int i = source.Count - 1; i >= 0; i--)
        {
            if (!match(source[i]))
            {
                destination.Add(source[i]);
                source.RemoveAt(i);
            }
        }

        //refill hand with cards that match the predicate
        List<Card> cards = destination.Where((card) => match(card)).ToList();
        while (source.Count < 5)
        {
            Card card = cards[0];
            cards.Remove(card);
            destination.Remove(card);
            source.Add(card);
        }
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
        if (!isTraining)
        {
            Label label = new Label(trigger.TriggerName);
            label.style.fontSize = 25;
            triggerList.hierarchy.Add(label); 
        }

        evoBot.CheckTrigger(trigger);
        updates++;
        if (updates > 420)
        {
            Debug.Log("Too many updates, stopping");
            return;
        }
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
                Debug.Log("Game has started");
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
                string playerName = (string)trigger.TriggerData[Constants.Player];
                Debug.Log($"Received input from {playerName}");
                Controller.UpdateGame();
                break;

            case Constants.OnTurnComplete:
                Debug.Log("[green]Player's turn is complete[/]");
                if (isTraining)
                {
                    if (lesson.StoppingCondition(Controller.Game))
                    {
                        Debug.Log("Stopping condition met");
                        stop = true;
                        EndGame();
                    }
                    else
                    {
                        Controller.UpdateGame();
                    } 
                }
                else
                {
                    Controller.UpdateGame();
                }
                break;

            case Constants.OnStateChange:
                Controller.UpdateGame();
                break;

            case Constants.OnExecuteCard:
                Placement placement = (Placement)trigger.TriggerData[Constants.Placement];
                Debug.Log($"Executing card '{placement.Instruction.Name}' at (Row: {placement.Row}, Column {placement.Col})");
                button.text = "Execute card";
                Controller.UpdateGame();
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
                AddToList($"The winner is {winner.Name} with {winner.VP_Count} victory points!");
                Debug.Log($"The winner is {winner.Name} with {winner.VP_Count} victory points!");
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

    private void StartGame()
    {
        updates = 0;
        try
        {
            Controller.StartGame();
        }
        catch (Exception e)
        {
            Debug.Log("exception: " + e.Message + ", " + updates);
        }
    }

    private void DisplayPlayers(List<Player> players)
    {
        string labelText = "Player Stats:";
        foreach (Player player in players)
        {
            labelText += player.Name + ":\n VP: " + player.VP_Count + "\n Brick: " + player.BrickCount + "\n Wood: " + player.WoodCount + "\n Straw: " + player.StrawCount + "\n\n";
        }
        playerStats.text = labelText;
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
