using Microsoft.VisualBasic;
using Newtonsoft.Json;
using PPPredictor.Core.API;
using PPPredictor.Core.Calculator;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.DataType.MapPool;
using PPPredictor.Core.UnitTest.MockService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.Development
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var v1 = File.ReadAllText("..\\..\\..\\Data\\PPPredictorProfileInfo.json");

            var ProfileInfo = JsonConvert.DeserializeObject<ProfileInfo>(v1);

            CalculatorInstance ci = await CalculatorInstance.CreateAsyncMock<MockScoreSaberApi, MockBeatLeader, MockHitBloq, MockAccSaber>(
                new Settings(false, true, false, false, "76561197980340660", DataType.Enums.PPGainCalculationType.Raw, DataType.Enums.MapPoolSorting.Alphabetical, "", 7, DateTime.Now, 12),
                ProfileInfo.DctleaderBoardData,
                null
                );

            string hash = "BE49148059822B60C9AC0E7883B5F3534225A35F_SOLOSTANDARD_7";
            var test = ProfileInfo.DctleaderBoardData[Leaderboard.BeatLeader.ToString()].DctMapPool["-1"].LsLeaderboadInfo;
            var starRating = ProfileInfo.DctleaderBoardData[Leaderboard.BeatLeader.ToString()].DctMapPool["-1"].LsLeaderboadInfo.FirstOrDefault(x => x.Searchstring == hash)?.StarRating;
            var beatMapKey = new BeatmapKey();
            beatMapKey.serializedName = "SOLOSTANDARD";
            beatMapKey.difficulty = BeatMapDifficulty.Expert;
            var s = new PPPBeatMapInfo(new PPPBeatMapInfo("BE49148059822B60C9AC0E7883B5F3534225A35F", beatMapKey), starRating);
            s.SelectedMapSearchString = $"BE49148059822B60C9AC0E7883B5F3534225A35F_SOLOSTANDARD_7";
            var v = ci.CalculatePercentageNeededForPP(Enums.Leaderboard.BeatLeader, "-1", s, 10);


            //var v2 = await ci.CalculatePercentageNeededForRankGain(Leaderboard.BeatLeader, "-1", s, 10);
            //await ci.GetPlayerScores(Enums.Leaderboard.BeatLeader, "73", 1, 100);
            //await ci.GetPlayerScores(Enums.Leaderboard.BeatLeader, "68", 1, 100);
            //await ci.UpdatePlayer(Enums.Leaderboard.BeatLeader, "68", false);
            //while (true)
            //{
            //    var pp = 337.21;
            //    var gain = ci.GetPlayerScorePPGain(Enums.Leaderboard.BeatLeader, "68", "3402AA430181F7C254007A28E15FC397C48086B0_SOLOSTANDARD_7".ToUpper(), pp);
            //}
            //
        }

    }

    class ProfileInfo
    {
        public Dictionary<string, LeaderboardData> DctleaderBoardData { get; set; }
    }
}