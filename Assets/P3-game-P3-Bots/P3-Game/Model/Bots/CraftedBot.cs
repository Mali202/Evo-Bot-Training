using BehaviourTree;
using BehaviourTree.FluentBuilder;
using Microsoft.Extensions.Logging;
using Model.Actions;
using Model.Instructions;
using Model.Tricks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Bots
{
    public class CraftedBot : Bot
    {
        public CraftedBot(bool isWolf, string Name, int playerNumber) : base(isWolf, Name, playerNumber)
        {
        }
        public override void ChooseAction(List<IAction> actions)
        {
            Logger.LogDebug("Crafted Bot [{botName}] => tickng tree", Name);
            //Logger.LogDebug("Crafted Bot => Cards in hand: {cards}", String.Join(", ",);
            IBehaviour<List<IAction>> behaviorTree = BuildTree();
            behaviorTree.Tick(actions);
        }

        private static void ExecuteAction(IAction action)
        {
            GetGame().OnReceivedPlayerInput(action);
            Logger.LogDebug("Crafted Bot => {ActionDescription}", action.GetDescription());
            GetGame().Update();
        }

        private IBehaviour<List<IAction>> BuildTree()
        {
            return FluentBuilder.Create<List<IAction>>()
                .Selector("Select action")
                    //.Sequence("Draw from face up if 2 or less cards in hand")
                    //    .Condition("Check if 2 or less cards in hand", CheckHand)
                    //    .Selector("Draw a card")
                    //        .Do("Draw face up", DrawFaceUp)
                    //        .Do("Draw from draw deck", DrawFaceDown)
                    //    .End()
                    //.End()
                    //.Selector("Place a transfer for any resource that is 1 or less")
                    //    .Do("Place brick transfer if necessary", context => PlaceTransferPriority(context, ResourceType.Brick, 1))
                    //    .Do("Place straw transfer if necessary", context => PlaceTransferPriority(context, ResourceType.Straw, 1))
                    //    .Do("Place wood transfer if necessary", context => PlaceTransferPriority(context, ResourceType.Wood, 1))
                    //.End()
                    //.Do("Play skip to player with most VP", PlaySkipCard)
                    .Do("Rotate a card to a beneficial orientation", RotateCard)
                    .Do("Play any trick", PlayAnyTrick)
                    .Do("Place appropriate blueprint", PlaceBlueprint)
                    .Do("Place appropriate transfer", PlaceTransfer)
                    .Do("Draw face up", DrawFaceUp)
                    .Do("Draw from draw deck", DrawFaceDown)
                    .Do("Random action", RandomAction)     
                .End()
                .Build();               
        }

        private bool CheckHand(List<IAction> list)
        {
            Logger.LogDebug("Crafted Bot => {Count} cards in hand", Hand.Cards.Count);
            return Hand.Cards.Count <= 2;
        }

        //If given resource type is less than min, place a transfer that gives that resource
        private BehaviourStatus PlaceTransferPriority(List<IAction> list, ResourceType resource, int min)
        {
            //Get all transfer actions that give resource
            List<PlaceInstruction> transferActionsResource;
            switch (resource)
            {
                //TODO: cleanup
                case ResourceType.Brick:
                    Logger.LogDebug("Crafted Bot => Brick count is {count}", BrickCount);
                    if (BrickCount > 2)
                    {
                        return BehaviourStatus.Failed;
                    }
                    transferActionsResource = list
                                                .Where((action) => action.GetType() == typeof(PlaceInstruction))
                                                .Select((action) => (PlaceInstruction)action)
                                                .Where((action) => action.placement.Instruction.GetType() == typeof(Transfer))
                                                .Where((action) => action.placement.Instruction.Brick > 0)
                                                .OrderByDescending((action) =>
                                                {
                                                    return action.placement.Instruction.Brick;
                                                }).ToList();
                    break;

                case ResourceType.Straw:
                    Logger.LogDebug("Crafted Bot => Straw count is {count}", StrawCount);
                    if (WoodCount > 2)
                    {
                        return BehaviourStatus.Failed;
                    }
                    transferActionsResource = list
                                                .Where((action) => action.GetType() == typeof(PlaceInstruction))
                                                .Select((action) => (PlaceInstruction)action)
                                                .Where((action) => action.placement.Instruction.GetType() == typeof(Transfer))
                                                .Where((action) => action.placement.Instruction.Straw > 0)
                                                .OrderByDescending((action) =>
                                                {
                                                    return action.placement.Instruction.Straw;
                                                }).ToList();
                    break;

                case ResourceType.Wood:
                    Logger.LogDebug("Crafted Bot => Wood count is {count}", BrickCount);
                    if (WoodCount > 2)
                    {
                        return BehaviourStatus.Failed;
                    }
                    transferActionsResource = list
                                                .Where((action) => action.GetType() == typeof(PlaceInstruction))
                                                .Select((action) => (PlaceInstruction)action)
                                                .Where((action) => action.placement.Instruction.GetType() == typeof(Transfer))
                                                .Where((action) => action.placement.Instruction.Wood > 0)
                                                .OrderByDescending((action) =>
                                                {
                                                    return action.placement.Instruction.Wood;
                                                }).ToList();
                    break;

                default:
                    transferActionsResource = new();
                    break;
            }

            if (transferActionsResource.Count == 0)
            {
                Logger.LogDebug("Crafted Bot => No transfer that gives {resource}", resource.ToString());
                return BehaviourStatus.Failed;
            }

            Logger.LogDebug("Crafted Bot => placing transfer");
            PlaceInstruction transferAction = transferActionsResource.First((action) =>
            {
                return GetGame().CalculatePlayer(((Transfer)action.placement.Instruction).ToPlayer, action.placement.Orientation).Equals(this);
            });

            ExecuteAction(transferAction);
            return BehaviourStatus.Succeeded;
        }

        private BehaviourStatus PlaySkipCard(List<IAction> list)
        {
            List<PlaySkip> skipActions = list.Where((action) => action.GetType() == typeof(PlaySkip)).Select((action) => (PlaySkip)action).ToList();
            if (skipActions.Count == 0)
            {
                Logger.LogDebug("Crafted Bot => No skip to play");
                return BehaviourStatus.Failed;
            }
            
            //PlaySkip? action = skipActions.MaxBy(s => s.target.VP_Count);
            PlaySkip? action = skipActions.OrderByDescending((action) => action.target.VP_Count).FirstOrDefault();
            if (action is null)
            {
                Logger.LogError("Crafted Bot => Error, action is null");
                return BehaviourStatus.Failed;
            }

            Logger.LogDebug("Crafted Bot => Playing skip...");
            ExecuteAction(action);
            return BehaviourStatus.Succeeded;
        }

        private BehaviourStatus RotateCard(List<IAction> list)
        {
            //Get all rotate actions
            IEnumerable<RotateAction> rotateActions = list.OfType<RotateAction>();
            if (!rotateActions.Any())
            {
                Logger.LogDebug("Crafted Bot => No Rotate to play");
                return BehaviourStatus.Failed;
            }

            //Filter for blueprint and transfer cards
            IEnumerable<RotateAction> filteredActions = rotateActions.Where((action) => action.placement.Instruction is Blueprint or Transfer);

            if (!filteredActions.Any())
            {
                Logger.LogDebug("Crafted Bot => No blueprints or transfers to rotate");
                return BehaviourStatus.Failed;
            }

            //Order rotate actions by type and then by amount of resources/vp
            IEnumerable<RotateAction> sortedActions = rotateActions
                .OrderBy((action) => (action.placement.Instruction is Blueprint) ? 1 : 2)
                .ThenByDescending((action) =>
                {
                    Instruction instruction = action.placement.Instruction;
                    return (instruction is Blueprint bl) ? bl.VP : instruction.Brick + instruction.Straw + instruction.Wood;
                });

            RotateAction action = sortedActions.First();

            Logger.LogDebug("Crafted Bot => Playing Rotate...");
            ExecuteAction(action);
            return BehaviourStatus.Succeeded;
        }

        private BehaviourStatus PlayAnyTrick(List<IAction> list)
        {
            IAction? trickAction = list.FirstOrDefault((action) => action is PlayChangeDirection);
            if (trickAction != null)
            {
                Logger.LogDebug("Crafted Bot => trick available to play");
                ExecuteAction(trickAction);
                return BehaviourStatus.Succeeded; 
            }
            else
            {
                Logger.LogDebug("Crafted Bot => No trick to play");
                return BehaviourStatus.Failed;
            }
        }

        private BehaviourStatus RandomAction(List<IAction> list)
        {
            Logger.LogDebug("Crafted Bot => Selecting random action");
            Random random = new();
            int index = random.Next(list.Count);
            ExecuteAction(list[index]);
            return BehaviourStatus.Succeeded;
        }

        //TODO: add blueprint scoring
        private BehaviourStatus PlaceBlueprint(List<IAction> list)
        {
            //Get all place blueprint actions and order by VP
            List<PlaceInstruction> blueprintActions = list
                                                .Where((action) => action.GetType() == typeof(PlaceInstruction))
                                                .Select((action) => (PlaceInstruction)action)
                                                .Where((action) => action.placement.Instruction.GetType() == typeof(Blueprint))
                                                .OrderByDescending((action) => ((Blueprint)action.placement.Instruction).VP).ToList();


            if (blueprintActions.Count == 0)
            {
                Logger.LogDebug("Crafted Bot => No blueprint to place");
                return BehaviourStatus.Failed;
            }


            //Array of pairs with ResourceType and corresponding count
            KeyValuePair<ResourceType, int>[] resourceCounts = new KeyValuePair<ResourceType, int>[]
            {

                new KeyValuePair<ResourceType, int>(ResourceType.Brick, BrickCount),
                new KeyValuePair<ResourceType, int>(ResourceType.Straw, StrawCount),
                new KeyValuePair<ResourceType, int>(ResourceType.Wood, WoodCount)
            };

            //Sort counts in descending order
            KeyValuePair<ResourceType, int>[] sortedResourceCounts = resourceCounts.OrderByDescending(kvp => kvp.Value).ToArray();

            //Get blueprint actions with highest VP and select one that uses the resource with highest count
            List<PlaceInstruction> blueprintActionsResource;
            for (int i = 0; i < sortedResourceCounts.Length; i++)
            {
                blueprintActionsResource = sortedResourceCounts[i].Key switch
                {
                    ResourceType.Brick => blueprintActions
                            .Where((bp) =>bp.placement.Instruction.Brick > 0).ToList(),
                    ResourceType.Straw => blueprintActions
                            .Where((bp) =>bp.placement.Instruction.Straw > 0).ToList(),
                    ResourceType.Wood => blueprintActions
                            .Where((bp) =>bp.placement.Instruction.Wood > 0).ToList(),
                    _ => new(),
                };

                if (blueprintActionsResource.Count == 0)
                {
                    Logger.LogDebug("Crafted Bot => no blueprint that uses {Resource}", sortedResourceCounts[i].Key.ToString());
                    continue;
                }

                //Select first action with beneficial orientation
                Logger.LogDebug("Crafted Bot => placing blueprint");
                PlaceInstruction blueprintAction = blueprintActionsResource.First((action) =>
                {
                    return GetGame().CalculatePlayer(((Blueprint)action.placement.Instruction).Player, action.placement.Orientation).Equals(this);
                });

                ExecuteAction(blueprintAction);
                return BehaviourStatus.Succeeded;
            }

            return BehaviourStatus.Failed;
        }

        private BehaviourStatus PlaceTransfer(List<IAction> list)
        {
            List<PlaceInstruction> transferActions = list
                                                .Where((action) => action.GetType() == typeof(PlaceInstruction))
                                                .Select((action) => (PlaceInstruction)action)
                                                .Where((action) => action.placement.Instruction.GetType() == typeof(Transfer))
                                                .OrderByDescending((action) =>
                                                {
                                                    return action.placement.Instruction.Brick + action.placement.Instruction.Straw + action.placement.Instruction.Wood;
                                                }).ToList();

            if (transferActions.Count == 0)
            {
                Logger.LogDebug("Crafted Bot => No transfer to place");
                return BehaviourStatus.Failed;
            }

            //Array of pairs with ResourceType and corresponding count
            KeyValuePair<ResourceType, int>[] resourceCounts = new KeyValuePair<ResourceType, int>[]
            {

                new KeyValuePair<ResourceType, int>(ResourceType.Brick, BrickCount),
                new KeyValuePair<ResourceType, int>(ResourceType.Straw, StrawCount),
                new KeyValuePair<ResourceType, int>(ResourceType.Wood, WoodCount)
            };

            //Sort counts in descending order
            KeyValuePair<ResourceType, int>[] sortedResourceCounts = resourceCounts.OrderBy(kvp => kvp.Value).ToArray();
            
            //Get transer actions with most resource and select one that uses the resource with lowest count
            int maxTransfer = transferActions[0].placement.Instruction.Brick + transferActions[0].placement.Instruction.Straw + transferActions[0].placement.Instruction.Wood;
            List<PlaceInstruction> maxTransferActions;
            for (int i = maxTransfer; i >= 0; i--)
            {
                maxTransferActions = transferActions.Where((action) =>
                {
                    return i == (action.placement.Instruction.Brick + action.placement.Instruction.Straw + action.placement.Instruction.Wood);
                }).ToList();

                //Get actions that use the resource with lowest count
                for (int j = 0; j < sortedResourceCounts.Length; j++)
                {
                    List<PlaceInstruction> maxTransferActionsResource = sortedResourceCounts[j].Key switch
                    {
                        ResourceType.Brick => maxTransferActions.Where((action) => action.placement.Instruction.Brick > 0).ToList(),
                        ResourceType.Straw => maxTransferActions.Where((action) => action.placement.Instruction.Straw > 0).ToList(),
                        ResourceType.Wood => maxTransferActions.Where((action) => action.placement.Instruction.Wood > 0).ToList(),
                        _ => new(),
                    };

                    if (maxTransferActionsResource.Count == 0)
                    {
                        Logger.LogDebug("Crafted Bot => no transfer that uses {Resource}", sortedResourceCounts[j].Key.ToString());
                        continue;
                    }

                    //Select first action with beneficial orientation
                    Logger.LogDebug("Crafted Bot => placing transfer");
                    PlaceInstruction transferAction = maxTransferActionsResource.First((action) =>
                    {
                        return GetGame().CalculatePlayer(((Transfer)action.placement.Instruction).ToPlayer, action.placement.Orientation).Equals(this);
                    });

                    ExecuteAction(transferAction);
                    return BehaviourStatus.Succeeded;
                }
            }

            return BehaviourStatus.Failed;
        }

        //Draw a card, preferably a trick or a blueprint
        private BehaviourStatus DrawFaceUp(List<IAction> list)
        {
            //get all draw actions
            List<DrawAction> drawActions = list
                                                .Where((action) => action.GetType() == typeof(DrawAction))
                                                .Select((action) => (DrawAction)action).ToList();

            if (drawActions.Count == 0)
            {
                Logger.LogDebug("Crafted Bot => No draw actions available");
                return BehaviourStatus.Failed;
            }

            //get face up cards
            List<Card> faceUpCards = GetGame().FaceUp.Cards;

            //get face up draw actions and sort by preference
            List<DrawAction> faceUpDrawActionsSorted = drawActions
                                                        .Where((action) => action.deckType == DeckType.FaceUp)
                                                        .Where((action) => faceUpCards[action.cardIndex] is Trick || faceUpCards[action.cardIndex] is Blueprint)
                                                        .OrderBy((action) => OrderCard(faceUpCards[action.cardIndex])).ToList();

            if (faceUpDrawActionsSorted.Count == 0)
            {
                Logger.LogDebug("Crafted Bot => No tricks or blueprints in the face up");
                return BehaviourStatus.Failed;
            }

            Logger.LogDebug("Crafted Bot => Drawing from face up");
            ExecuteAction(faceUpDrawActionsSorted.First());
            return BehaviourStatus.Succeeded;
        }

        //Order cards by preference -> Skip, Trick, Blueprint
        private static int OrderCard(Card card)
        {
            if (card is Trick)
            {
                if (card is Skip)
                {
                    Logger.LogDebug("Crafted Bot => found a skip in the face up");
                    return 0;
                }
                Logger.LogDebug("Crafted Bot => found a trick in the face up");
                return 1;
            }
            Logger.LogDebug("Crafted Bot => found a blueprint in the face up");
            return 2;
        }

        private BehaviourStatus DrawFaceDown(List<IAction> list)
        {
            //get all draw actions
            List<DrawAction> drawActions = list
                                                .Where((action) => action.GetType() == typeof(DrawAction))
                                                .Select((action) => (DrawAction)action).ToList();

            if (drawActions.Count == 0)
            {
                Logger.LogDebug("Crafted Bot => No draw actions available");
                return BehaviourStatus.Failed;
            }

            List<DrawAction> faceDownDrawActions = drawActions
                                                        .Where((action) => action.deckType == DeckType.Draw).ToList();

            if (faceDownDrawActions.Count == 0)
            {
                Logger.LogDebug("Crafted Bot => No face down draw actions available");
                return BehaviourStatus.Failed;
            }

            Logger.LogDebug("Crafted Bot => Drawing from face down");
            ExecuteAction(faceDownDrawActions.First());
            return BehaviourStatus.Succeeded;
        }
    }
}