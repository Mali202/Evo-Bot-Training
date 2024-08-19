using System.Collections.Generic;

namespace Model
{
    public class Trigger
    {
        private bool triggered;
        public string TriggerName { get; set; }
        public IDictionary<string, object> TriggerData { get; set; }

        public Trigger(string triggerName)
        {
            triggered = false;
            TriggerName = triggerName;
            TriggerData = new Dictionary<string, object>();
        }

        public bool IsTriggered() {
            return triggered;
        }

        public void TriggerRule ()
        {
            triggered = true;
        }
        
        public void UntriggerRule() {
            triggered = false;
        }
    }
}
