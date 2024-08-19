using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Bots
{
    public class EvoBot : Bot
    {
        public EvoBot(bool isWolf, string Name, int playerNumber) : base(isWolf, Name, playerNumber)
        {
        }

        public override void ChooseAction(List<IAction> actions)
        {
            throw new NotImplementedException();
        }
    }

}
