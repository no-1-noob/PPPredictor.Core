using Newtonsoft.Json;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.UnitTest.MockService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.Performance
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Dictionary<string, LeaderboardData> dctData = new Dictionary<string, LeaderboardData>();
            ProfileInfo p = JsonConvert.DeserializeObject<ProfileInfo>(File.ReadAllText("../../Data/PPPredictorProfileInfo.json"));
            dctData = p.DctleaderBoardData;
            Settings settings = new Settings(false, true, false, false, "", PPGainCalculationType.Weighted, MapPoolSorting.Alphabetical, "", 7, DateTime.Now.AddDays(-1), 48);

            CalculatorInstance calculatorInstance = await CalculatorInstance.CreateAsyncMock<MockScoreSaberApi, MockBeatLeader, MockHitBloq, MockAccSaber>(
                settings,
                dctData,
                null
            );

            string mappool = "-1";
            double maxPP = 500;
            double stepSize = 10;
            bool foundData = true;

            List<string> lsHashMaps = new List<string>();
            if(dctData.TryGetValue(Leaderboard.BeatLeader.ToString(), out var leaderboardData)){

                if(leaderboardData.DctMapPool.TryGetValue(mappool, out var pPPMapPool)){

                    lsHashMaps = pPPMapPool.LsScores.Where((item, index) => (index + 1) % 10 == 0).Select(x => x.Searchstring).ToList();
                    for (int i = 0; i < 50; i++)
                    {
                        lsHashMaps.Add(i.ToString());
                    }
                    List<PPGainResult> lsGains = new List<PPGainResult>();


                    Stopwatch stopwatch = Stopwatch.StartNew();

                    int calcsteps = 0;

                    for (int iterations = 0; iterations < 5; iterations++)
                    {
                        foreach (var hash in lsHashMaps)
                        {
                            for (double pp = 0; pp < maxPP; pp+=stepSize)
                            {
                                calcsteps++;
                                lsGains.Add(calculatorInstance.GetPlayerScorePPGain(Leaderboard.BeatLeader, mappool, hash, pp));
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
                    string jsonString = JsonConvert.SerializeObject(lsGains, Formatting.Indented);

                    // Write JSON string to file
                    File.WriteAllText(filename, jsonString);

                    List<PPGainResult> lsOldGains = JsonConvert.DeserializeObject<List<PPGainResult>>(File.ReadAllText("Old.json"));

                    int Differences = 0;
                    for (int i = 0; i < lsGains.Count; i++) {
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
            }
            if(!foundData)
            {
                Console.WriteLine("No Data found");
            }

            Console.WriteLine("---DONE---");
            Console.ReadLine();
        }

        class ProfileInfo
        {
            public Dictionary<string, LeaderboardData> DctleaderBoardData { get; set; }
        }
    }
}
