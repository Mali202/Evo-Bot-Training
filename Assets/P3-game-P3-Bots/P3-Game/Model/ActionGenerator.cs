using System;
using System.Collections.Generic;

namespace Model
{
    public class ActionGenerator
    {
        public Dictionary<Type, IGenerator> generatorKey = new();

        public Game Game {get; set;}
        private static ActionGenerator instance;
        public static ActionGenerator Instance
        {
            get
            {
                instance ??= new ActionGenerator();
                return instance;
            }
        }

        private ActionGenerator() {}
        
        public List<IAction> GetAllValid(Card card) {
            if (card.GetType().IsSubclassOf(typeof(Instruction)))
            {
                return generatorKey[typeof(Instruction)].GenerateActions(card);
            }
            return generatorKey[card.GetType()].GenerateActions(card);
        }
    }


}
