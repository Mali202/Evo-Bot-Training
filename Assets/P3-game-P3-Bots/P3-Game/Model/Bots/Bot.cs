using Microsoft.Extensions.Logging;
using Model.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Bots
{
    public abstract class Bot : Player, IListener
    {
        private static Game? game;
        private static ILogger? logger;

        public Bot(bool isWolf, string Name, int playerNumber) : base(isWolf, Name, playerNumber)
        {

        }

        public void Broadcast(Trigger trigger)
        {
            if (trigger.TriggerName != Constants.OnWaitingPlayerInput)
            {
                return;
            }


            string curPlayerName = trigger.TriggerData.TryGetValue(Constants.Player, out var player) ? ((Player) player).Name : "";
            if (Name == curPlayerName)
            {
                ChooseAction((List<IAction>)trigger.TriggerData[Constants.Actions]);
            }
        }

        public static void SetGame(Game _game)
        {
            game = _game;
        }

        protected static Game GetGame()
        {
            if (game == null)
            {
                throw new Exception("Game is not set");
            }
            return game;
        }

        protected static ILogger Logger
        {
            get
            {
                if (logger == null)
                {
                    using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
                    logger = factory.CreateLogger("Bots");
                    logger.LogDebug("Logger created");
                }
                return logger;
            }
        }

        public abstract void ChooseAction(List<IAction> actions);
    }
}
