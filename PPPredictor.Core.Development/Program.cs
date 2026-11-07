using PPPredictor.Core.API;
using PPPredictor.Core.Calculator;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.MapPool;
using System;
using System.Collections.Generic;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.Development
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string hash = "2E887F6CD0FA896A890D2FB8D023A17DB47F0EA9"; //https://beatleader.com/leaderboard/global/13b2b71/1
            const string characteristic = "Standard";
            const int difficulty = 1; // Easy=1, Normal=3, Hard=5, Expert=7, ExpertPlus=9.
            const double acc = 95.00; // Percentage, not a fraction.
            const string mapPoolId = "-4";
            const LeaderboardContext context = LeaderboardContext.BeatLeaderDefault;
            const bool failed = false;
            const bool paused = false;
            var modifiers = new GameplayModifiers
            {
                songSpeed = GameplayModifiers.SongSpeed.Normal,
                disappearingArrows = false,
                ghostNotes = false,
                proMode = false,
                smallCubes = false,
                noFailOn0Energy = false,
            };

            if (!double.IsFinite(acc) || acc < 0 || acc > 100)
                throw new ArgumentOutOfRangeException(nameof(acc), "Accuracy must be between 0 and 100.");

            string searchString = $"{hash}_SOLO{characteristic}_{difficulty}".ToUpperInvariant();
            var beatMapInfo = new PPPBeatMapInfo
            {
                CustomLevelHash = hash,
                SelectedMapSearchString = searchString,
                BeatmapKey = new BeatmapKey { serializedName = characteristic, difficulty = (BeatMapDifficulty)difficulty }
            };
            var mapPool = new PPPMapPool { Id = mapPoolId, MapPoolType = MapPoolType.Default, LeaderboardContext = context };
            var settings = new Settings(false, true, false, false, "76561197980340660",
                PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, 7, 12);
            var calculator = new PPCalculatorBeatLeader<BeatleaderAPI>(new Dictionary<string, PPPMapPool>(), settings);

            beatMapInfo = await calculator.GetBeatMapInfoAsync(beatMapInfo, mapPool, modifiers);
            beatMapInfo = await calculator.ApplyModifiersToBeatmapInfo(beatMapInfo, mapPool, modifiers, failed, paused);

            var curve = new BeatLeaderPPPCurve();

            // Set a breakpoint here and step into CalculatePPatPercentage.
            double pp = curve.CalculatePPatPercentage(beatMapInfo, acc, failed, paused, context);

            Console.WriteLine($"Map: {searchString}");
            Console.WriteLine($"Accuracy: {acc}% | Modifiers: {beatMapInfo.Modifiers} | Context: {context}");
            Console.WriteLine($"Base ratings: {beatMapInfo.BaseStarRating}");
            Console.WriteLine($"Modified ratings: {beatMapInfo.ModifiedStarRating}");
            Console.WriteLine($"PP-0.01: {curve.CalculatePPatPercentage(beatMapInfo, acc-0.01, failed, paused, context)}");
            Console.WriteLine($"PP+0.00: {curve.CalculatePPatPercentage(beatMapInfo, acc, failed, paused, context)}");
            Console.WriteLine($"PP+0.01: {curve.CalculatePPatPercentage(beatMapInfo, acc+0.01, failed, paused, context)}");
        }

    }
}
