using System;

namespace Model
{
    public abstract class PlayerAction : Card
    {
        public override Card CloneCard()
        {
            throw new NotImplementedException();
        }

        public abstract void Apply();
    }
}
