using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using PPPredictor.Core.API;
using PPPredictor.Core.Calculator;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.MapPool;

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
            CalculatorInstance ci = await CalculatorInstance.CreateAsyncMock<ScoresaberAPI, BeatleaderAPI, HitbloqAPI, AccSaberApi>(
                new Settings(false, true, false, false, "76561197980340660", DataType.Enums.PPGainCalculationType.Raw, DataType.Enums.MapPoolSorting.Alphabetical, "", 7, DateTime.Now, 12),
                new Dictionary<string, DataType.LeaderBoard.LeaderboardData>(),
                null
                );

            //await ci.GetPlayerScores(Enums.Leaderboard.BeatLeader, "73", 1, 100);
            await ci.GetPlayerScores(Enums.Leaderboard.BeatLeader, "68", 1, 100);
            await ci.UpdatePlayer(Enums.Leaderboard.BeatLeader, "68", false);
            while (true)
            {
                var pp = 337.21;
                var gain = ci.GetPlayerScorePPGain(Enums.Leaderboard.BeatLeader, "68", "3402AA430181F7C254007A28E15FC397C48086B0_SOLOSTANDARD_7".ToUpper(), pp);
            }
            //
        }

    }
}
