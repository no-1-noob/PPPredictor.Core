using Newtonsoft.Json.Linq;
using PPPredictor.Core.API;
using PPPredictor.Core.Calculator;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.MapPool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.Development
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string profilePath = "/var/home/nub/.local/share/Steam/steamapps/common/BeatSaberDev/1_40_7/Beat Saber/UserData/PPPredictorProfileInfo.json";
            const string hash = "2E887F6CD0FA896A890D2FB8D023A17DB47F0EA9"; //https://beatleader.com/leaderboard/global/13b2b71/1
            const string characteristic = "Standard";
            const int difficulty = 1; // Easy=1, Normal=3, Hard=5, Expert=7, ExpertPlus=9.
            const double acc = 98.34; // Percentage, not a fraction.
            const string mapPoolId = "-1";
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
                noFailOn0Energy = true
            };

            if (!double.IsFinite(acc) || acc < 0 || acc > 100)
                throw new ArgumentOutOfRangeException(nameof(acc), "Accuracy must be between 0 and 100.");

            var profile = JObject.Parse(File.ReadAllText(profilePath));
            var cachedMaps = profile["DctleaderBoardData"]?["BeatLeader"]?["DctMapPool"]?[mapPoolId]?["LsLeaderboadInfo"] as JArray
                ?? throw new InvalidOperationException($"No cached BeatLeader map data in pool {mapPoolId}.");
            string searchString = $"{hash}_SOLO{characteristic}_{difficulty}".ToUpperInvariant();
            var cachedMap = cachedMaps.FirstOrDefault(map =>
                string.Equals((string?)map["Searchstring"], searchString, StringComparison.OrdinalIgnoreCase));
            var rating = cachedMap?["StarRating"]?.ToObject<PPPStarRating>()
                ?? throw new InvalidOperationException($"No cached ratings for {searchString} in pool {mapPoolId}. Select a map present in LsLeaderboadInfo.");

            var beatMapInfo = new PPPBeatMapInfo
            {
                CustomLevelHash = hash,
                SelectedMapSearchString = searchString,
                BaseStarRating = rating
            };
            var mapPool = new PPPMapPool { Id = mapPoolId, LeaderboardContext = context };
            var settings = new Settings(false, true, false, false, "76561197980340660",
                PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, 7, 12);
            var calculator = new PPCalculatorBeatLeader<BeatleaderAPI>(new Dictionary<string, PPPMapPool>(), settings);

            // Use the production modifier preparation, without fetching or refreshing cached ratings.
            beatMapInfo = calculator.ApplyModifiersToBeatmapInfo(beatMapInfo, mapPool, modifiers, failed, paused);
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
