﻿using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using romme.Utility;

namespace romme
{

    public class StatsLog : MonoBehaviour
    {
        private string path = "data/stats_";
        private GameMaster gameMaster;

        private void Start()
        {
            path += System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
            gameMaster = Tb.I.GameMaster;
            gameMaster.GameOver.Subscribe(GameOver);
            WriteToFile("Seed\tRound\tLoserHandCardCount\tLoserHandValue\tLoserFilledCardSpots\tLoserCardSpotsValue");
        }

        private void GameOver(Player player)
        {
            string output = gameMaster.Seed + "\t" +
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