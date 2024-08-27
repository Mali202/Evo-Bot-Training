using Model;
using Model.Actions;
using Model.Bots;
using Model.Instructions;
using Model.Tricks;
using Model.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class EvoBotAgent : Agent
{
    public EvoBot bot;
    private List<IAction> validActions;

    public EvoBot InitializeBot(bool isWolf, string Name, int playerNumber)
    {
        bot = new EvoBot(isWolf, Name, playerNumber);
        bot.RequestAction = (actions) =>
        {
            validActions = actions;
            Debug.Log("Requesting decision with action count " + actions.Count);
            RequestDecision();
            //Academy.Instance.EnvironmentStep();
        };

        Debug.Log("Evo Bot initialized");
        return bot;
    }

    public void CheckTrigger(Trigger trigger)
    {
        switch (trigger.TriggerName)
        {
            case Constants.OnExecuteCard:
                Placement placement = (Placement)trigger.TriggerData[Constants.Placement];
                break;
            case Constants.OnGoalCardsActivated:
            case Constants.OnGameCompleted:
                break;
        }
    }

    //Agent override methods

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }

    public override void Initialize()
    {
        Debug.Log("Evo Bot agent initialized");
        Academy.Instance.AgentPreStep += (step) => Debug.Log($"Stepping {step}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log("Collecting observations");
        Game game = bot.Game;

        // player number: 1
        sensor.AddObservation(bot.playerNumber);

        // number of players: 1
        sensor.AddObservation(game.NumPlayers);

        // grid: 19
        sensor.AddObservation(EncodeGrid(game.GameGrid.Placements).Select(x => (float)x).ToArray());

        // face up deck cards: 3
        sensor.AddObservation(EncodeDeck(game.FaceUp.Cards, 3).Select(x => (float)x).ToArray());

        // VP count: 1
        sensor.AddObservation(bot.VP_Count);

        // resource counts: 3
        sensor.AddObservation(new float[] { bot.BrickCount, bot.StrawCount, bot.WoodCount});

        // hand count: 1
        sensor.AddObservation(bot.Hand.Cards.Count);

        // cards in hand: 5
        sensor.AddObservation(EncodeDeck(bot.Hand.Cards, 5).Select(x => (float)x).ToArray());

        //TODO: Goal card observation
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("OnActionReceived called");
        ActionSegment<int> discreteActions = actions.DiscreteActions;

        int actionType = discreteActions[0];
        switch (actionType)
        {
            //place from hand
            case 0:
                HandlePlaceFromHand(discreteActions);
                break;

            //draw
            case 1:
                HandleDraw(discreteActions);
                break;

            //discard
            case 2:
                HandleDiscard(discreteActions);
                break;
        }
        //Academy.Instance.EnvironmentStep();
    }

    // encode grid
    private int[] EncodeGrid(List<Placement> gridPlacements)
    {
        int gridSize = 19; // 19 valid placements in the grid
        int[] gridObservation = new int[gridSize];

        for (int i = 0; i < gridPlacements.Count; i++)
        {
            gridObservation[i] = EncodePlacementAsBinary(gridPlacements[i]);
        }

        return gridObservation;
    }

    // encode placement
    private int EncodePlacementAsBinary(Placement placement)
    {
        int encodedValue = 0;

        // Column (3 bits)
        encodedValue |= (placement.Col & 0b111) << 12;

        // Row (2 bits)
        encodedValue |= (placement.Row & 0b11) << 10;

        // Orientation (2 bits)
        int orientationBits = EncodeOrientation(placement.Orientation);
        encodedValue |= (orientationBits & 0b11) << 8;

        // Instruction (10 bits)
        int instructionBits = EncodeCard(placement.Instruction);
        encodedValue |= (instructionBits & 0b1111111111);

        return encodedValue;
    }

    private int EncodeOrientation(Orientation orientation)
    {
        return orientation switch
        {
            Orientation.Up => 0b00,
            Orientation.Right => 0b01,
            Orientation.Down => 0b10,
            Orientation.Left => 0b11,
            _ => 0b00 // Default to Up if something goes wrong
        };
    }

    // encode hard
    private int[] EncodeDeck(List<Card> cards, int maxSize)
    {
        int[] encodedDeck = new int[maxSize];
        for (int i = 0; i < cards.Count; i++)
        {
            encodedDeck[i] = EncodeCard(cards[i]);
        }
        return encodedDeck;
    }

    // encode card
    private int EncodeCard(Card card)
    {
        int encodedValue = 0;

        // Card Type (3 bits)
        if (card is Transfer) encodedValue |= 0b000 << 7;
        else if (card is Blueprint) encodedValue |= 0b001 << 7;
        else if (card is Loop) encodedValue |= 0b010 << 7;
        else if (card is Condition) encodedValue |= 0b011 << 7;
        else if (card is Skip) encodedValue |= 0b100 << 7;
        else if (card is ChangeDirection) encodedValue |= 0b101 << 7;

        // From Player (2 bits) - Used by relevant cards
        if (card is Transfer transferCard)
        {
            int fromPlayerBits = GetPlayerBits(transferCard.FromPlayer);
            encodedValue |= fromPlayerBits << 5;

            // To Player (2 bits) - Used by Transfer cards
            int toPlayerBits = GetPlayerBits(transferCard.ToPlayer);
            encodedValue |= toPlayerBits << 3;
        }
        else if (card is Blueprint blueprintCard)
        {
            int toPlayerBits = GetPlayerBits(blueprintCard.Player);
            encodedValue |= toPlayerBits << 5;
        }

        // Resources (Wood, Brick, Straw) (3 bits)
        Instruction instruction = card as Instruction;
        encodedValue |= (instruction.Wood > 0 ? 0b1 : 0b0) << 2;
        encodedValue |= (instruction.Brick > 0 ? 0b1 : 0b0) << 1;
        encodedValue |= (instruction.Straw > 0 ? 0b1 : 0b0);

        return encodedValue;
    }

    private int GetPlayerBits(int playerId)
    {
        return playerId switch
        {
            1 => 0b00,
            2 => 0b01,
            3 => 0b10,
            4 => 0b11,
            _ => 0b00 // Default to Player 1 if something goes wrong
        };
    }

    private void HandlePlaceFromHand(ActionSegment<int> discreteActions)
    {
        int cardIndex = discreteActions[1];
        Card card = bot.Hand.Cards[cardIndex];
        IAction action;
        switch (card)
        {
            case Instruction:
                int column = discreteActions[2];
                int row = discreteActions[3];
                int orientationInt = discreteActions[4];
                Orientation orientation = DecodeOrientation(orientationInt);

                action = validActions
                    .Where((action) => action is PlaceInstruction)
                    .Select((action) => (PlaceInstruction)action)
                    .FirstOrDefault((action) => action.placement.Row == row && action.placement.Col == column && action.placement.Orientation == orientation);

                if (action != null)
                {
                    RewardForPlacement(action);
                    bot.ExecuteAction(action);
                }
                else
                {
                    Debug.Log("Action is null");
                    AddReward(-1.0f);
                }
                break;

            case ChangeDirection:
                bot.ExecuteAction(validActions.First((action) => action is PlayChangeDirection));
                break;

            case Skip:
                int target = discreteActions[2];
                action = validActions
                    .Where((action) => action is PlaySkip)
                    .Select((action) => (PlaySkip)action)
                    .FirstOrDefault((action) => action.target.playerNumber == target);

                if (action != null)
                {
                    bot.ExecuteAction(action);
                }
                else
                {
                    Debug.Log("Action is null");
                    AddReward(-1.0f);
                }                
                break;

            default:
                Debug.Log("Unsupported card type: " + card.GetType().Name);
                break;
        }
    }

    private void HandleDraw(ActionSegment<int> discreteActions)
    {
        int deckTypeInt = discreteActions[1];
        DeckType deckType = deckTypeInt switch
        {
            0 => DeckType.Draw,
            1 => DeckType.FaceUp,
            _ => DeckType.Draw
        };

        int drawIndex = discreteActions[2];

        IAction action = validActions
            .Where((action) => action is DrawAction)
            .Select((action) => (DrawAction)action)
            .FirstOrDefault((action) => action.deckType == deckType && action.cardIndex == drawIndex);


        if (action != null)
        {
            bot.ExecuteAction(action);
        }
        else
        {
            Debug.Log("Action is null");
            AddReward(-1.0f);
        }
    }

    private void HandleDiscard(ActionSegment<int> discreteActions)
    {
        int cardindex = discreteActions[1];
        Card card = bot.Hand.Cards.ElementAtOrDefault(cardindex);

        if (card != null)
        {
            IAction action = validActions
                .Where((action) => action is DiscardAction)
                .Select((action) => (DiscardAction)action)
                .FirstOrDefault((action) => action.cardToDiscard == card);

            bot.ExecuteAction(action);
        }
        else
        {
            Debug.Log("Action is null");
            AddReward(-1.0f);
        }
    }

    private Orientation DecodeOrientation(int orientation)
    {
        return orientation switch
        {
            0 => Orientation.Up,
            1 => Orientation.Right,
            2 => Orientation.Down,
            3 => Orientation.Left,
            _ => Orientation.Up, // Default to Up if something goes wrong
        };
    }

    private void RewardForPlacement(IAction action)
    {
        if (action is PlaceInstruction placeInstruction)
        {
            int starting = 0;
            switch (placeInstruction.placement.Instruction)
            {
                case Transfer tr:
                    starting = tr.ToPlayer;
                    break;

                case Blueprint bl:
                    starting = bl.Player;
                    break;
            }

            if (placeInstruction.placement.Instruction is Transfer or Blueprint)
            {
                if (bot.Game.CalculatePlayer(starting, placeInstruction.placement.Orientation) == bot)
                {
                    Debug.Log("Rewarding for correct placement");
                    AddReward(0.5f);
                } else 
                {
                    Debug.Log("Punishing for incorrect placement");
                    AddReward(-0.5f);
                }            
            }
            
        }
    }
}
