using System;

namespace Model.Tricks
{
    public class Swap : Trick
    {
        public override Card CloneCard()
        {
            return new Swap();
        }

        public override void Execute(Game game)
        {
            throw new NotImplementedException();
        }

        public override void UndoExecute(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
