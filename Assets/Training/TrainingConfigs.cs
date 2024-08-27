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
        public static readonly Lesson PlaceCard = new()
        {
            Name = "PlaceCard",
            NumPlayers = 1,
            HandConfig = HandConfig.InstructionOnly,
            StoppingCondition = (game) => game.GameGrid.Placements.Count == 1
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
