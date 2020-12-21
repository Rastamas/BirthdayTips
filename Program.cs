﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace BabyTips
{
    class Program
    {
        private const int PerfectScore = 4;

        private static readonly Dictionary<int, int> Scores = new Dictionary<int, int>
        {
            {1, 3 },
            {2, 2 },
            {3, 1 }
        };

        static void Main(string[] args)
        {
            string path;
            Guess actualResult = new();

            do
            {
                Console.WriteLine("Add meg a csv elérési útvonalát:");
                path = Console.ReadLine().Trim('\"');

                if (!File.Exists(path))
                {
                    Console.WriteLine("A megadott file nem található!");
                    continue;
                }

                Console.WriteLine("Add meg Anna születési dátumát (pl. 2020/01/23)");
                actualResult.Birthday = DateTime.Parse(Console.ReadLine());


                Console.WriteLine("Add meg Anna születési súlyát grammban(pl. 3200)");
                actualResult.WeightInGrams = int.Parse(Console.ReadLine());


                Console.WriteLine("Add meg Anna születési hosszát cm-ben (pl. 53)");
                actualResult.HeightInCm = int.Parse(Console.ReadLine());

            } while (!File.Exists(path));

            var csvReader = new CsvReader(new StreamReader(path), new CsvConfiguration (CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            });

            var records = csvReader.GetRecords<Guess>().ToList();

            Console.WriteLine("Tippek száma: " + records.Count);

            records = CalculateBirthdayScore(records, actualResult.Birthday);

            foreach (var record in records)
            {
                Console.WriteLine(record.Name + ": " + record.Score);
            }

            Console.ReadLine();
        }

        private static List<Guess> CalculateBirthdayScore(List<Guess> guesses, DateTime actualBirthday)
        {
            var incorrectGuesses = new Dictionary<string, int>();
            var perfectGuessers = 0;

            foreach (var guess in guesses)
            {
                var diff = Math.Abs((actualBirthday.Date - guess.Birthday.Date).Days);

                if (diff == 0)
                {
                    guess.Score += PerfectScore;
                    perfectGuessers++;
                } else
                {
                    incorrectGuesses.Add(guess.Name, diff);
                }
            }

            var positionsToAward = Scores.Count - perfectGuessers;

            if (positionsToAward <= 0)
            {
                return guesses;
            }

            var groupedGuesses = incorrectGuesses.GroupBy(x => x.Value).OrderBy(x => x.Key);

            foreach (var groupedGuess in groupedGuesses)
            {
                var scoreToGive = Scores[Scores.Count - positionsToAward + 1];

                foreach (var guess in groupedGuess)
                {
                    guesses.Single(g => g.Name == guess.Key).Score += scoreToGive;
                }

                positionsToAward -= groupedGuess.Count();
                if (positionsToAward <= 0)
                {
                    break;
                }
            }


            return guesses;
        }
    }
}