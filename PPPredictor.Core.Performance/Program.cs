using Newtonsoft.Json;
using PPPredictor.Core.Calculator;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.DataType.Score;
using PPPredictor.Core.UnitTest.MockService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.Performance
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Dictionary<string, LeaderboardData> dctData = new Dictionary<string, LeaderboardData>();
            ProfileInfo p = JsonConvert.DeserializeObject<ProfileInfo>(File.ReadAllText("../../../Data/PPPredictorProfileInfo.json"));
            dctData = p.DctleaderBoardData;
            Settings settings = new Settings(false, true, false, false, false, "", PPGainCalculationType.Weighted, MapPoolSorting.Alphabetical, "", 7, 48);

            CalculatorInstance calculatorInstance = await CalculatorInstance.CreateAsyncMock<MockScoreSaberApi, MockBeatLeader, MockHitBloq, MockAccSaber, MockAccSaberReloaded>(
                settings,
                dctData,
                null
            );

            string mappool = "-1";
            double maxPP = 500;
            double stepSize = 10;
            double maxRounds = 5;
            bool foundData = true;

            List<string> lsHashMaps = new List<string>();
            if (dctData.TryGetValue(Leaderboard.BeatLeader.ToString(), out var leaderboardData))
            {
                //lsHashMaps = PerformancePPGain(calculatorInstance, mappool, maxPP, stepSize, lsHashMaps, leaderboardData);

                if (leaderboardData.DctMapPool.TryGetValue(mappool, out var pPPMapPool))
                {
                    lsHashMaps = PPGainCalculation(calculatorInstance, mappool, maxPP, stepSize, maxRounds, pPPMapPool);

                    //PPPBeatMapInfo beatMapInfo = new PPPBeatMapInfo(new PPPBeatMapInfo(), new PPPStarRating(1, 11.82055, 7.9189496, 5.424088, true));

                    //int calcCount = 0;
                    //Stopwatch stopwatch = Stopwatch.StartNew();

                    //for (int rounds = 0; rounds < maxRounds; rounds++)
                    //{
                    //    bool failed = false;
                    //    bool paused = false;
                    //    Calculate(pPPMapPool, beatMapInfo, failed, paused, ref calcCount);
                    //    failed = true;
                    //    Calculate(pPPMapPool, beatMapInfo, failed, paused, ref calcCount);
                    //    paused = true;
                    //    Calculate(pPPMapPool, beatMapInfo, failed, paused, ref calcCount);
                    //    failed = false;
                    //    Calculate(pPPMapPool, beatMapInfo, failed, paused, ref calcCount);
                    //}

                    //stopwatch.Stop();
                    //double avg = stopwatch.Elapsed.TotalMilliseconds / (calcCount);
                    //Console.WriteLine($"Avg time per call: {avg:F4} ms");
                    //Console.WriteLine($"Time elapsed {stopwatch.Elapsed.TotalMinutes}");

                }
            }
            if (!foundData)
            {
                Console.WriteLine("No Data found");
            }

            Console.WriteLine("---DONE---");
            //Console.ReadLine();
        }

        private static void Calculate(DataType.MapPool.PPPMapPool pPPMapPool, PPPBeatMapInfo beatMapInfo, bool failed, bool paused, ref int calcCount)
        {
            for (int i = 0; i < 10000; i++)
            {
                calcCount++;
                var pp = pPPMapPool.Curve.CalculatePPatPercentage(beatMapInfo, (i / 10000.0), failed, paused, LeaderboardContext.BeatLeaderDefault);
                //Console.WriteLine($"{(i / 10000.0)} - {pp}");
                //Console.WriteLine($"{(i / 10.0)} - {pPPMapPool.Curve.CalculatePPatPercentage(beatMapInfo, (i / 10.0), failed, paused, LeaderboardContext.BeatLeaderDefault)}");
            }
        }

        private static List<string> PPGainCalculation(CalculatorInstance calculatorInstance, string mappool, double maxPP, double stepSize, double maxRounds, DataType.MapPool.PPPMapPool pPPMapPool)
        {
            List<string> lsHashMaps;
            int takeEvery = 15;
            List<ShortScore> newList = pPPMapPool.LsScores.Where((item, index) => (index + 1) % takeEvery == 0 || index == 0).ToList();

            PPCalculator c = calculatorInstance.GetCalculator(Leaderboard.BeatLeader);
            c.RecalculateScoreWeightedSum(pPPMapPool);
            pPPMapPool.DctWeightLookup.Clear();

            //Add new scores to list
            for (int i = 0; i < 25; i++)
            {
                newList.Add(new ShortScore($"NEWSCORE{i}", 0));
            }
            lsHashMaps = newList.Select(x => x.Searchstring).ToList();

            int callCount = 0;
            List<PPGainResult> lsGains = new List<PPGainResult>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int rounds = 0; rounds < maxRounds; rounds++)
            {
                Console.WriteLine($"Round {rounds + 1}/{maxRounds}");
                int hashIndex = 0;
                foreach (var hash in lsHashMaps)
                {
                    hashIndex++;
                    //Console.WriteLine($"Round {rounds + 1}/{maxRounds}, Hash {hash} ({hashIndex}/{lsHashMaps.Count}), Total Calls {callCount}");
                    for (int roundsInternal = 0; roundsInternal < maxRounds; roundsInternal++)
                    {
                        for (double pp = 0; pp < maxPP; pp += stepSize)
                        {
                            callCount++;
                            stopwatch.Start();
                            lsGains.Add(calculatorInstance.GetPlayerScorePPGain(Leaderboard.BeatLeader, mappool, hash, pp));
                            stopwatch.Stop();
                        }
                    }
                }
            }

            double avg = stopwatch.Elapsed.TotalMilliseconds / (callCount);
            Console.WriteLine($"Avg time per call: {avg:F4} ms");
            Console.WriteLine($"Time elapsed {stopwatch.Elapsed.TotalMinutes}");

            string jsonString = JsonConvert.SerializeObject(lsGains, Newtonsoft.Json.Formatting.Indented);

            // Write JSON string to file
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"OUT_{timestamp}.json";

            string outFileName = Path.Combine("../../../Data", filename);
            File.WriteAllText(outFileName, jsonString);
            return lsHashMaps;
        }

        private static List<string> PerformancePPGain(CalculatorInstance calculatorInstance, string mappool, double maxPP, double stepSize, List<string> lsHashMaps, LeaderboardData leaderboardData)
        {
            if (leaderboardData.DctMapPool.TryGetValue(mappool, out var pPPMapPool))
            {

                lsHashMaps = pPPMapPool.LsScores.Where((item, index) => (index + 1) % 10 == 0).Select(x => x.Searchstring).ToList();
                for (int i = 0; i < 50; i++)
                {
                    lsHashMaps.Add(i.ToString());
                }
                List<PPGainResult> lsGains = new List<PPGainResult>();


                Stopwatch stopwatch = new Stopwatch();

                int calcsteps = 0;

                for (int iterations = 0; iterations < 5; iterations++)
                {
                    foreach (var hash in lsHashMaps)
                    {
                        for (double pp = 0; pp < maxPP; pp += stepSize)
                        {
                            calcsteps++;
                            stopwatch.Start();
                            lsGains.Add(calculatorInstance.GetPlayerScorePPGain(Leaderboard.BeatLeader, mappool, hash, pp));
                            stopwatch.Stop();
                        }
                    }

                }


                stopwatch.Stop();
                double avg = stopwatch.Elapsed.TotalMilliseconds / (calcsteps);
                Console.WriteLine($"Avg time per call: {avg:F4} ms");
                Console.WriteLine($"Time elapsed {stopwatch.Elapsed.TotalMinutes}");

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Create filename with timestamp
                string filename = $"New.json";

                // Serialize list to JSON string
                string jsonString = JsonConvert.SerializeObject(lsGains, Newtonsoft.Json.Formatting.Indented);

                // Write JSON string to file
                File.WriteAllText(filename, jsonString);

                List<PPGainResult> lsOldGains = JsonConvert.DeserializeObject<List<PPGainResult>>(File.ReadAllText("Old.json"));

                int Differences = 0;
                for (int i = 0; i < lsGains.Count; i++)
                {
                    if (lsOldGains[i].PpDisplayValue != lsGains[i].PpDisplayValue)
                    {
                        Console.WriteLine($"PpDisplayValue {lsOldGains[i].PpDisplayValue} != {lsGains[i].PpDisplayValue}");
                        Differences++;
                    }
                    if (lsOldGains[i].PpGainWeighted != lsGains[i].PpGainWeighted)
                    {
                        Console.WriteLine($"PpGainWeighted {lsOldGains[i].PpGainWeighted} != {lsGains[i].PpGainWeighted}");
                        Differences++;
                    }
                    if (lsOldGains[i].PpTotal != lsGains[i].PpTotal)
                    {
                        Console.WriteLine($"PpTotal {lsOldGains[i].PpTotal} != {lsGains[i].PpTotal}");
                        Differences++;
                    }
                    if (lsOldGains[i].PpGainRaw != lsGains[i].PpGainRaw)
                    {
                        Console.WriteLine($"PpGainRaw {lsOldGains[i].PpGainRaw} != {lsGains[i].PpGainRaw}");
                        Differences++;
                    }

                }
                Console.WriteLine($"{Differences} Differences");

                //Console.WriteLine($"List saved to file: {filename}");
            }

            return lsHashMaps;
        }

        class ProfileInfo
        {
            public Dictionary<string, LeaderboardData> DctleaderBoardData { get; set; }
        }
    }
}
