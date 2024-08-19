using System;
using System.Collections.Generic;
using System.Reflection;

namespace Model
{
    public class Action
    {
        public string ActionName { get; set; }
        public List<string> Parameters {get; set;}

        public Action(string actionname)
        {
            ActionName = actionname;
            Parameters = new List<string>();
        }

        public void Apply(Game game) {
            Console.WriteLine(ActionName);
            MethodInfo? method = game.GetType().GetMethod(ActionName);
            method?.Invoke(game, Parameters.ToArray());
        }

        public void undoAction(Game game)
        {
            Console.WriteLine("Undoing" + ActionName);
            MethodInfo? method = game.GetType().GetMethod(ActionName + "_Undo");
            method?.Invoke(game, Parameters.ToArray());
        }
    }
}
