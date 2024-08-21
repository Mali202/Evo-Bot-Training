using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Bots
{
    public class EvoBot : Bot
    {
        public Action<List<IAction>> RequestAction;
        public EvoBot(bool isWolf, string Name, int playerNumber) : base(isWolf, Name, playerNumber)
        {
        }

        public override void ChooseAction(List<IAction> actions)
        {
            RequestAction.Invoke(actions);
        }

        public void ExecuteAction(IAction action)
        {
            GetGame().OnReceivedPlayerInput(action);
            //Logger.LogDebug("Crafted Bot => {ActionDescription}", action.GetDescription());
            GetGame().Update();
        }

        public Game Game { get { return GetGame();} }
    }

}
