using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class GameGrid
    {
        public List<Placement> Placements { get; }
        public int NumInstructions;

        public GameGrid(int NumInstructions)
        {
            Placements = new List<Placement>();
            this.NumInstructions = NumInstructions;
        }

        public bool isFull()
        {
            //May not be necessary
            if (Placements.FirstOrDefault(Placement => Placement.Row == 0 && Placement.Col == 0) is null)
            {
                return false;
            }

            bool inCondition = false;
            for (int i = 1; i < NumInstructions; i++)
            {
                List<Placement> prevCol = Placements.FindAll(placement => placement.Col == i - 1);
                Placement? middle = prevCol.FirstOrDefault(Placement => Placement.Row == 0);

                if (middle is not null)
                {
                    switch (middle.Instruction.PlacementRule) {
                        case PlacementRuleType.Sequence:
                            Placement curColMiddle = Placements.FirstOrDefault(placement => placement.Row == 0 & placement.Col == i);
                            if (curColMiddle is null)
                            {
                                return false;
                            }
                            break;

                        case PlacementRuleType.Condition:
                            inCondition = true;
                            Placement? curColTop = Placements.FirstOrDefault(placement => placement.Row == 1 & placement.Col == i);
                            if (curColTop is null)
                            {
                                return false;
                            }

                            Placement? curColBottom = Placements.FirstOrDefault(placement => placement.Row == -1 & placement.Col == i);
                            if (curColBottom is null)
                            {
                                return false;
                            }
                            break;

                        case PlacementRuleType.Loop:
                            inCondition = false;
                            Placement? curColTopLoop = Placements.FirstOrDefault(placement => placement.Row == 1 & placement.Col == i);
                            if (curColTopLoop is null)
                            {
                                return false;
                            }
                            break;
                    }
                }
                else
                {
                    Placement? curColMiddle = Placements.FirstOrDefault(placement => placement.Row == 0 & placement.Col == i);
                    if (curColMiddle is null)
                    {
                        if (inCondition)
                        {
                            Placement? curColTop = Placements.FirstOrDefault(placement => placement.Row == 1 & placement.Col == i);
                            if (curColTop is null)
                            {
                                return false;
                            }

                            Placement? curColBottom = Placements.FirstOrDefault(placement => placement.Row == -1 & placement.Col == i);
                            if (curColBottom is null)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Placement? curColTop = Placements.FirstOrDefault(placement => placement.Row == 1 & placement.Col == i);
                            if (curColTop is null)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
