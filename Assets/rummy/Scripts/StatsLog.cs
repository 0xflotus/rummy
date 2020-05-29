﻿using System.IO;
using System.Linq;
using UnityEngine;
using rummy.Utility;

namespace rummy
{

    public class StatsLog : MonoBehaviour
    {
        private static string folderName = "data";
        private static string path = folderName + "/stats_";
        private GameMaster gameMaster;

        private void Start()
        {
            if (!System.IO.Directory.Exists(folderName))
                System.IO.Directory.CreateDirectory(folderName);

            path += System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
            gameMaster = Tb.I.GameMaster;
            gameMaster.GameOver.AddListener(GameOver);
            WriteToFile("Seed\tPlayerNr\tRound\tLoserHandCardCount\tLoserHandValue\tLoserFilledCardSpots\tLoserCardSpotsValue");
        }

        private void GameOver(Player player)
        {
            if (player == null)
            {
                WriteToFile(gameMaster.Seed + "\t-1\t" + gameMaster.RoundCount + "\t-1\t-1\t-1\t-1");
                return;
            }

            string output = gameMaster.Seed + "\t" +
                            gameMaster.Players.IndexOf(player) + "\t" +
                            gameMaster.RoundCount + "\t" +
                            player.PlayerCardCount + "\t" +
                            player.PlayerHandValue + "\t" +
                            player.GetPlayerCardSpots().Count(spot => spot.HasCards) + "\t" +
                            player.GetLaidCardsSum();
            WriteToFile(output);
        }

        private void WriteToFile(string output)
        {
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(output);
            writer.Close();
        }
    }

}