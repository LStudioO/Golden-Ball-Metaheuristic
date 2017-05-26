﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldenBall.Classes
{
    class Season
    {
        List<Team> tList;

        Dictionary<Team, int> rating;

        public Season(List<Team> teams)
        {
            tList = teams;

            dropRating();
        }

        private void dropRating()
        {
            if (rating == null)
            {
                rating = new Dictionary<Team, int>();
                tList.ForEach(i => rating.Add(i, 0));
            }
            else
            {
                foreach (var itm in tList)
                    rating[itm] = 0;
            }
        }

        private int[,] generateShedule()
        {

            var tCount = tList.Count - 1;

            int[,] table = new int[tCount, tCount + 1];

            var curTeamIndex = 0;

            for (int i = 0; i < tCount; i++)
            {
                int p;

                if (i % 2 == 0)
                {
                    p = 1;
                    table[i, 0] = tList[curTeamIndex++].Id;
                }
                else
                {
                    p = 0;
                    table[i, 1] = tList[curTeamIndex++].Id;
                }

                table[i, p] = tList.Last().Id;

                if (curTeamIndex >= tList.Count - 1)
                    curTeamIndex = 0;

                for (int k = 2; k < tCount + 1; k += 2)
                {
                    if (table[i, k] == 0)
                    {
                        table[i, k] = tList[curTeamIndex++].Id;
                        if (curTeamIndex >= tList.Count - 1)
                            curTeamIndex = 0;
                    }
                }

            }

            if (tCount > 2)
            {
                curTeamIndex = tList.Count - 2;
                for (int i = 0; i < tCount; i++)
                {
                    for (int k = 3; k < tCount + 1; k += 2)
                    {
                        table[i, k] = tList[curTeamIndex--].Id;
                        if (curTeamIndex < 0)
                            curTeamIndex = tList.Count - 2;
                    }
                }
            }
            return table;
        }

        private void playMatch(Team t1, Team t2)
        {
            int firstTeamMark = 0;
            int secondTeamMark = 0;

            var playersTeamOne = t1.Players.OrderByDescending(p => p.Mark).ToList();
            var playersTeamTwo = t2.Players.OrderByDescending(p => p.Mark).ToList();

            for (int i = 0; i < t1.Players.Count; i++)
            {
                if (playersTeamOne[i].Mark > playersTeamTwo[i].Mark)
                {
                    firstTeamMark++;
                }
                else
                {
                    secondTeamMark++;
                }
            }

            if (firstTeamMark > secondTeamMark)
            {
                rating[t1] += 3;
            }
            else if (firstTeamMark < secondTeamMark)
            {
                rating[t2] += 3;
            }
            else
            {
                rating[t1] += 1;
                rating[t2] += 1;
            }
        }        

        private void transfer()
        {
            var sorted = rating.OrderByDescending(a => a.Value).Select(a => a.Key).ToList();

            for (int i = 0; i < sorted.Count / 2; i++)
            {
                var currentTop = sorted[i];
                var currentDown = sorted[sorted.Count - 1 - i];

                var topTeamPlayer = currentTop.Players.OrderBy(p => p.Mark).ToArray()[i];
                var downTeamPlayer = currentTop.Players.OrderByDescending(p => p.Mark).ToArray()[i];

                var tempPlayer = topTeamPlayer;
                topTeamPlayer = downTeamPlayer;
                downTeamPlayer = tempPlayer;
            }
        }

        private void individualTransfer()
        {

            foreach (var itm in tList)
            {
                itm.Players.ForEach(p =>
                {
                    if (p.NeedIndividualTransfer)
                    {
                        var teams = tList.Where(a => a != itm).ToList();
                        var rTeamIndex = new Random().Next(0, teams.Count);
                        var rPlayerIndex = new Random().Next(0, itm.Players.Count);
                        var tmpPlayer = p;
                        p.Success();
                        p = teams[rTeamIndex].Players[rPlayerIndex];
                        teams[rTeamIndex].Players[rPlayerIndex] = tmpPlayer;
                    }
                });
            }
        }

        private void findCoach()
        {
            var sorted = rating.OrderBy(a => a.Value).Select(a => a.Key).ToList();

            for (int i = 0; i < sorted.Count / 2; i++)
            {
                Train oldCoach = sorted[i].TrainingMethod;
                Train newCoach;

                do
                {
                    newCoach = TrainingMethods.GetRandomTrainingMethod();
                } while (Delegate.Equals(oldCoach, newCoach));

                oldCoach = newCoach;
            }
        }

        public void Start()
        {
            // generate table
            int[,] table = generateShedule();

            var tCount = tList.Count - 1;

            for (int j = 0; j < 2; j++)
            {

                for (int i = 0; i < tCount; i++)
                {
                    foreach (var itm in tList)
                    {
                        itm.Train();
                        itm.PersonalTrain();
                        itm.DetermineMark();
                    }

                    individualTransfer();

                    for (int k = 0; k < tCount + 1; k += 2)
                    {
                        var teamFirstId = table[i, k];
                        var teamSecondId = table[i, k + 1];

                        playMatch(tList[teamFirstId], tList[teamSecondId]);
                    }
                }

                transfer();
                findCoach();
            }

        }
    }
}