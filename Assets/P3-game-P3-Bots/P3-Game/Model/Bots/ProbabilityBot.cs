using Microsoft.Extensions.Logging;
using Model.Actions;
using Model.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model.Bots
{
    public class ProbabilityBot : Bot
    {
        private readonly WeightedActionExecutor executor;

        public ProbabilityBot(bool isWolf, string Name, int playerNumber, params WeightedActionParam[] parameters) : base(isWolf, Name, playerNumber)
        {
            // Initialize executor with weights
            executor = new WeightedActionExecutor(parameters);
        }

        public override void ChooseAction(List<IAction> actions)
        {
            IAction action = executor.Execute(actions);
            
            GetGame().OnReceivedPlayerInput(action);
            Logger.LogDebug("Probability Bot [{botName}] => {ActionDescription}", Name, action.GetDescription());
            GetGame().Update();
        }

        //Return bot with pre-set weights
        public static ProbabilityBot Trickster(bool isWolf, string Name, int playerNumber)
        {
            // 20% chance to place, 20% to draw, 20% to discard, 40% to play
            WeightedActionParam[] weights = new WeightedActionParam[] {
                new WeightedActionParam((action) => action is PlaceInstruction, 20),
                new WeightedActionParam((action) => action is DrawAction, 20),
                new WeightedActionParam((action) => action is DiscardAction, 20),
                new WeightedActionParam((action) => action is (PlaySkip or PlayChangeDirection or RotateAction), 40)
            };

            return new ProbabilityBot(isWolf, Name, playerNumber, weights);
        }
    }
}
