using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Bots
{
    public class RandomBot : Bot
    {
        public RandomBot(bool isWolf, string Name, int playerNumber) : base(isWolf, Name, playerNumber)
        {
        }

        public override void ChooseAction(List<IAction> actions)
        {
            Random random = new();
            int index = random.Next(actions.Count);
            GetGame().OnReceivedPlayerInput(actions[index]);
            Logger.LogDebug("Random Bot [{botName}] => {ActionDescription}", Name, actions[index].GetDescription());
            GetGame().Update();
        }
    }
}
