using Assets.Training;
using Model;
using Model.Bots;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class RunTournament : MonoBehaviour
    {
        Tournament tournament;
        public EvoBotAgent evoBotAgent;
        int gamesPlayed;
        int configIndex;
        string[] configs = {
            // Two-player combinations
            "R, P",
            "R, C",
            "R, E",
            "P, C",
            "P, E",
            "C, E",
            
            // Three-player combinations
            "R, P, C",
            "R, P, E",
            "R, C, E",
            "P, C, E",
            
            // Four-player combination
            "R, P, C, E"
        };

        void Start()
        {
            tournament = new Tournament();
            gamesPlayed = 0;
            configIndex = 0;
        }

        void Update()
        {
            string config = configs[configIndex];
            string[] playersConfig = config.Split(", ");
            List<Player> playerList = playersConfig.Count() switch
            {
                2 => GetTwoPlayer(playersConfig[0][0], playersConfig[1][0]),
                3 => GetThreePlayer(playersConfig[0][0], playersConfig[1][0], playersConfig[2][0]),
                4 => GetFourPlayer(playersConfig[0][0], playersConfig[1][0], playersConfig[2][0], playersConfig[3][0]),
                _ => throw new NotImplementedException()
            };
            tournament.RunGame(playerList);
        }

        // get two players
        public List<Player> GetTwoPlayer(char p1, char p2)
        {
            int[] playerNumbers = { 1, 2 };
            playerNumbers.OrderBy(x => Guid.NewGuid()).ToArray();
            List<Player> players = new List<Player>
            {
                ConstructPlayer(p1, playerNumbers[0]),
                ConstructPlayer(p2, playerNumbers[1])
            };

            return players;
        }

        // get three players
        public List<Player> GetThreePlayer(char p1, char p2, char p3)
        {
            int[] playerNumbers = { 1, 2, 3 };
            playerNumbers.OrderBy(x => Guid.NewGuid()).ToArray();
            List<Player> players = new List<Player>
            {
                ConstructPlayer(p1, playerNumbers[0]),
                ConstructPlayer(p2, playerNumbers[1]),
                ConstructPlayer(p3, playerNumbers[2])
            };

            return players;
        }

        // get four players
        public List<Player> GetFourPlayer(char p1, char p2, char p3, char p4)
        {
            int[] playerNumbers = { 1, 2, 3, 4 };
            playerNumbers.OrderBy(x => Guid.NewGuid()).ToArray();

            int wolf = UnityEngine.Random.Range(1, 5);

            List<Player> players = new List<Player>
            {
                ConstructPlayer(p1, playerNumbers[0], playerNumbers[0] == wolf),
                ConstructPlayer(p2, playerNumbers[1], playerNumbers[1] == wolf),
                ConstructPlayer(p3, playerNumbers[2], playerNumbers[2] == wolf),
                ConstructPlayer(p4, playerNumbers[3], playerNumbers[3] == wolf)
            };

            return players;
        }

        public Player ConstructPlayer(char playerType, int playerNumber, bool isWolf = false) => playerType switch
        {        
            'R' => new RandomBot(isWolf, "Random", playerNumber),
            'P' => new ProbabilityBot(isWolf, "Probability", playerNumber),
            'C' => new CraftedBot(isWolf, "Crafted", playerNumber),
            'E' => evoBotAgent.InitializeBot(isWolf, "Evo", playerNumber),
            _ => throw new NotImplementedException(),
        };
    }
}