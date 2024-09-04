using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Training
{
    public static class TrainingConfigs
    {
        public static readonly Lesson PlaceBlueprint = new()
        {
            Name = "Place Blueprint",
            NumPlayers = 2,
            HandConfig = HandConfig.BlueprintOnly,
            StoppingCondition = (game) => game.GameGrid.Placements.Count == 4
        };

        public static readonly Lesson PlaceTransfer = new()
        {
            Name = "Place Transfer",
            NumPlayers = 2,
            HandConfig = HandConfig.TransferOnly,
            StoppingCondition = (game) => game.GameGrid.Placements.Count == 4
        };

        public static readonly Lesson PlaceInstruction = new()
        {
            Name = "Place Instruction",
            NumPlayers = 2,
            HandConfig = HandConfig.InstructionOnly,
            StoppingCondition = (game) => game.GameGrid.Placements.Count == 4
        };

        public static readonly Lesson TwoPlayerGame = new()
        {
            Name = "Two Player game",
            NumPlayers = 2,
            HandConfig = HandConfig.All,
            StoppingCondition = (game) => false
        };
    }

    public record Lesson
    {
        public string Name { get; set; }
        public int NumPlayers { get; set; }
        public HandConfig HandConfig { get; set; }
        public Func<Game, bool> StoppingCondition { get; set; }
        public bool IsWolf { get; set; }
    }

    public enum HandConfig
    {
        BlueprintOnly,
        TransferOnly,
        InstructionOnly,
        All
    }
}
