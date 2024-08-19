using System;
using System.Collections.Generic;
using System.Reflection;

namespace Model
{
    public class Rule
    {
        public List<Trigger> Triggers { get; set; }
        public List<Action> Actions { get; set; }
        public Dictionary<string, object> Conditions {get; set;}

        public Rule()
        {
            Triggers = new();
            Actions = new();
            Conditions = new();
        }

        public bool IsTriggered()
        {
            bool triggered = false;
            foreach (var trigger in Triggers)
            {
                if(trigger.IsTriggered())
                    triggered = true;
            }
            return triggered;
        }

        public bool CheckConditions(Game game) {
            foreach (KeyValuePair<string, object> condition in Conditions)
            {
                Console.WriteLine($"Checking if {condition.Key} is equal to {condition.Value}");
                MethodInfo? method = game.GetType().GetMethod(condition.Key);
                object? val = method?.Invoke(game, Array.Empty<object>()) ?? throw new Exception();

                if (!val.Equals(condition.Value))
                {
                    return false;
                }
            }
            return true;
        }

        public void DoActions(Game game)
        {
            foreach (var action in Actions)
            {
                action.Apply(game);
                game.ActionQueue.Enqueue(action);
            }
        }
    }
}
