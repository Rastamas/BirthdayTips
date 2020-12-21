using System;
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

        private const int PositionsToAward = 3;
        private static readonly Dictionary<int, int> Scores = new Dictionary<int, int>
        {
            { 1, 3 },
            { 2, 2 },
            { 3, 1 }
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

            var csvReader = new CsvReader(new StreamReader(path), new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            });

            var records = csvReader.GetRecords<Guess>().ToList();

            Console.WriteLine("Tippek száma: " + records.Count);

            records = CalculateBirthdayScore(records, actualResult.Birthday);
            records = CalculateWeightScore(records, actualResult.WeightInGrams);
            records = CalculateHeightScore(records, actualResult.HeightInCm);

            int i = 1;

            foreach (var record in records.OrderByDescending(r => r.Scores.Sum(score => score.Value)))
            {
                Console.Write($"{i++}.{GetPadding(i)} {record.Name}: {record.Scores.Sum(score => score.Value)} ");
                Console.WriteLine(string.Join(' ', record.Scores.Select(s => $"({s.Key} - {s.Value})")));
            }

            Console.ReadLine();
        }

        private static string GetPadding(int i) => i < 11 ? " " : string.Empty;

        private static List<Guess> CalculateHeightScore(List<Guess> guesses, int heightInCm)
        {
            var incorrectGuesses = new Dictionary<string, int>();

            foreach (var guess in guesses)
            {
                var diff = Math.Abs(heightInCm - guess.HeightInCm);

                incorrectGuesses.Add(guess.Name, diff);
            }

            guesses = CalculateScores(incorrectGuesses, guesses, "Magasság");

            return guesses;
        }

        private static List<Guess> CalculateWeightScore(List<Guess> guesses, int weightInGrams)
        {
            var incorrectGuesses = new Dictionary<string, int>();

            foreach (var guess in guesses)
            {
                var diff = Math.Abs(weightInGrams - guess.WeightInGrams);

                incorrectGuesses.Add(guess.Name, diff);
            }

            guesses = CalculateScores(incorrectGuesses, guesses, "Súly");

            return guesses;
        }

        private static List<Guess> CalculateBirthdayScore(List<Guess> guesses, DateTime actualBirthday)
        {
            var incorrectGuesses = new Dictionary<string, int>();

            foreach (var guess in guesses)
            {
                var diff = Math.Abs((actualBirthday.Date - guess.Birthday.Date).Days);

                incorrectGuesses.Add(guess.Name, diff);
            }

            guesses = CalculateScores(incorrectGuesses, guesses, "Születésnap");

            return guesses;
        }

        private static List<Guess> CalculateScores(Dictionary<string, int> incorrectGuesses, List<Guess> guesses, string category)
        {
            var positionsToAward = PositionsToAward;

            var groupedGuesses = incorrectGuesses.GroupBy(x => x.Value).OrderBy(x => x.Key);

            foreach (var groupedGuess in groupedGuesses)
            {
                var isPerfectGuess = groupedGuess.Key == 0;

                var scoreToGive = isPerfectGuess
                    ? PerfectScore
                    : Scores[PositionsToAward - positionsToAward + 1];

                foreach (var guess in groupedGuess)
                {
                    guesses.Single(g => g.Name == guess.Key).Scores.Add(category, scoreToGive);
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
