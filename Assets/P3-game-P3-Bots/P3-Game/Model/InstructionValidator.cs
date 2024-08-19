
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class InstructionValidator
    {
        private static InstructionValidator instance;
        public static Game game {get; set;}

        public static InstructionValidator Instance {
            get
            {
                instance ??= new InstructionValidator();
                return instance;
            }
        }

        private InstructionValidator() {}

        public bool IsValidPlacement(int row, int col, Instruction instruction) {
            return false;
        }

        public List<Placement> GetAllValid(Instruction instruction) {
            if (game.GameGrid.Placements.Count == 0)
            {
                List<Placement> rotations = GetRotations(instruction, 0, 0);
                return rotations;
            }

            bool inLoop = false;
            List<Placement> validPlacements = new();
            for (int i = 1; i < game.NumInstructions; i++)
            {
                List<Placement> prevCol = game.GameGrid.Placements.FindAll(placement => placement.Col == i-1);
                Placement? middle = prevCol.FirstOrDefault(Placement => Placement.Row == 0);
                if (middle is not null)
                {
                    //Previous middle is not null i.e still in middle
                    switch (middle.Instruction.PlacementRule)
                    {
                        case PlacementRuleType.Sequence:
                            Placement? curColMiddle = game.GameGrid.Placements.FirstOrDefault(placement => placement.Row == 0 & placement.Col == i);
                            if (curColMiddle is null)
                            {
                                return GetRotations(instruction, 0, i);
                            }
                            break;

                        case PlacementRuleType.Condition:
                            List<Placement> placements = new();
                            Placement? curColTop = game.GameGrid.Placements.FirstOrDefault(placement => placement.Row == 1 & placement.Col == i);
                            if (curColTop is null && instruction.PlacementRule == PlacementRuleType.Sequence)
                            {
                                placements.AddRange(GetRotations(instruction, 1, i));
                            }

                            Placement? curColBottom = game.GameGrid.Placements.FirstOrDefault(placement => placement.Row == -1 & placement.Col == i);
                            if (curColBottom is null && instruction.PlacementRule == PlacementRuleType.Sequence)
                            {
                                placements.AddRange(GetRotations(instruction, -1, i));
                            }

                            if (placements.Select(x => x.Row).Distinct().Count() == 2)
                            {
                                return placements;
                            }

                            if (curColTop is null && curColBottom is null)
                            {
                                return new();
                            }

                            validPlacements.AddRange(placements);
                            break;

                        case PlacementRuleType.Loop:
                            inLoop = true;
                            Placement? curColTopLoop = game.GameGrid.Placements.FirstOrDefault(placement => placement.Row == 1 & placement.Col == i);
                            if (curColTopLoop is null)
                            {
                                if (instruction.PlacementRule == PlacementRuleType.Sequence)
                                {
                                    return GetRotations(instruction, 1, i); 
                                }
                                else
                                {
                                    return new();
                                }
                            }
                            break;
                    }
                }
                else
                {
                    //Previous middle is null i.e. in condition or loop
                    List<Placement> curCol = game.GameGrid.Placements.FindAll(placement => placement.Col == i);
                    Placement? curColMiddle = curCol.FirstOrDefault(placement => placement.Row == 0);
                    if (curColMiddle is null)
                    {
                        //Current middle is null i.e. condition or loop is ongoing
                        List<Placement> placements = new();
                        Placement? prevColTop = game.GameGrid.Placements.FirstOrDefault(placement => placement.Row == 1 & placement.Col == i - 1);
                        Placement? curColTop = curCol.FirstOrDefault(placement => placement.Row == 1);
                        if (prevColTop is not null && curColTop is null && instruction.PlacementRule == PlacementRuleType.Sequence)
                        {
                            placements.AddRange(GetRotations(instruction, 1, i));
                        }

                        Placement? prevColBottom = game.GameGrid.Placements.FirstOrDefault(placement => placement.Row == -1 & placement.Col == i - 1);
                        Placement? curColBottom = curCol.FirstOrDefault(placement => placement.Row == -1);
                        if (prevColBottom is not null && curColBottom is null && instruction.PlacementRule == PlacementRuleType.Sequence)
                        {
                            placements.AddRange(GetRotations(instruction, -1, i));
                        }

                        if (placements.Select(x => x.Row).Distinct().Count() == 2)
                        {
                            placements.AddRange(GetRotations(instruction, 0, i));
                            return placements;
                        }

                        if (inLoop && curColTop is null)
                        {
                            placements.AddRange(GetRotations(instruction, 0, i));
                            return placements;
                        }

                        validPlacements.AddRange(placements);
                    }
                    else
                    {
                        inLoop = false;
                    }

                }
            }

            return validPlacements;
        }

        private List<Placement> GetRotations(Instruction instruction, int row, int col)
        {
            /* if (row != 0 & instruction.PlacementRule != PlacementRuleType.Sequence)
            {
                return [];
            } */

            if (!instruction.Rotatable)
            {
                return new() { new Placement(row, col, Orientation.Up, instruction) };
            }

            List<Placement> rotations = new()
            {
                new Placement(row, col, Orientation.Up, instruction),
                new Placement(row, col, Orientation.Right, instruction),
                new Placement(row, col, Orientation.Down, instruction),
                new Placement(row, col, Orientation.Left, instruction)
            };
            return rotations;
        }
    }
}
