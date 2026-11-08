using PPPredictor.Core.Calculator;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.DataType.MapPool;
using PPPredictor.Core.DataType.Score;
using PPPredictor.Core.Interface;
using PPPredictor.Core.UnitTest.MockService;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.UnitTest.LeaderBoard
{
    [TestClass]
    [DoNotParallelize]
    public class TestBeatLeaderCurveCaching
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task SwitchingDifficultiesReusesCachedCurves(bool customEvent)
        {
            var (calculator, pool) = CreateCalculator(customEvent);
            var modifiers = new GameplayModifiers();

            PPPBeatMapInfo first = await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.ExpertPlus), pool, modifiers);
            Assert.AreEqual(1, CountingBeatLeader.CurveRequests);
            Assert.AreSame(pool.LsLeaderboadInfo.Single(x => !x.IsPlaceHolder).StarRating, first.BaseStarRating);
            CurveInfo curve = first.BaseStarRating.DctCurves[""];

            PPPBeatMapInfo other = await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.Expert), pool, modifiers);
            Assert.AreEqual(customEvent, other.BaseStarRating.IsRanked());
            Assert.AreEqual(customEvent ? 2 : 1, CountingBeatLeader.CurveRequests);

            PPPBeatMapInfo returned = await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.ExpertPlus), pool, modifiers);
            PPPBeatMapInfo otherReturned = await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.Expert), pool, modifiers);
            Assert.AreSame(first.BaseStarRating, returned.BaseStarRating);
            Assert.AreSame(curve, returned.BaseStarRating.DctCurves[""]);
            Assert.AreEqual(customEvent, otherReturned.BaseStarRating.IsRanked());
            if (customEvent)
            {
                Assert.AreSame(other.BaseStarRating, otherReturned.BaseStarRating);
                Assert.AreSame(other.BaseStarRating.DctCurves[""], otherReturned.BaseStarRating.DctCurves[""]);
            }
            Assert.AreEqual(customEvent ? 2 : 1, CountingBeatLeader.CurveRequests);
            Assert.AreEqual(2, CountingBeatLeader.SongRequests);
        }

        [TestMethod]
        public async Task ExpiredCurveIsRefetchedWithoutRefetchingMapInfo()
        {
            var (calculator, pool) = CreateCalculator(false);
            var modifiers = new GameplayModifiers();
            PPPBeatMapInfo first = await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.ExpertPlus), pool, modifiers);
            CurveInfo expired = first.BaseStarRating.DctCurves[""];
            expired.LastUpdated = DateTime.Now.AddDays(-8);

            PPPBeatMapInfo refreshed = await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.ExpertPlus), pool, modifiers);
            Assert.AreNotSame(expired, refreshed.BaseStarRating.DctCurves[""]);
            Assert.IsTrue(refreshed.BaseStarRating.DctCurves[""].LastUpdated > DateTime.Now.AddDays(-7));
            Assert.AreSame(first.BaseStarRating, refreshed.BaseStarRating);
            Assert.AreEqual(2, CountingBeatLeader.CurveRequests);
            Assert.AreEqual(1, CountingBeatLeader.SongRequests);

            await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.ExpertPlus), pool, modifiers);
            Assert.AreEqual(2, CountingBeatLeader.CurveRequests);
        }

        [TestMethod]
        public async Task ModifierCurvesAreCachedSeparatelyAndNoFailReusesExistingCurve()
        {
            var (calculator, pool) = CreateCalculator(false);
            PPPBeatMapInfo first = await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.ExpertPlus), pool, new GameplayModifiers());
            var ghostNotes = new GameplayModifiers { ghostNotes = true };
            PPPBeatMapInfo modified = await calculator.ApplyModifiersToBeatmapInfo(first, pool, ghostNotes, false, false);
            Assert.AreEqual(2, CountingBeatLeader.CurveRequests);
            Assert.AreEqual(2, first.BaseStarRating.DctCurves.Count);
            CurveInfo ghostCurve = modified.BaseStarRating.DctCurves["GN"];

            await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.ExpertPlus), pool, new GameplayModifiers());
            PPPBeatMapInfo returned = await calculator.GetBeatMapInfoAsync(MapInfo(BeatMapDifficulty.ExpertPlus), pool,
                new GameplayModifiers { ghostNotes = true, noFailOn0Energy = true });
            Assert.AreSame(ghostCurve, returned.BaseStarRating.DctCurves["GN"]);
            Assert.AreEqual(2, CountingBeatLeader.CurveRequests);
            Assert.AreEqual(1, CountingBeatLeader.SongRequests);
        }

        private static (PPCalculatorBeatLeader<CountingBeatLeader>, PPPMapPool) CreateCalculator(bool customEvent)
        {
            CountingBeatLeader.SongRequests = 0;
            CountingBeatLeader.CurveRequests = 0;
            var settings = new Settings(false, true, false, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, -7, 12);
            var pool = new PPPMapPool("test", customEvent ? MapPoolType.Custom : MapPoolType.Default,
                "Test", new PPPWeightingInfo(0.965f), 0, new BeatLeaderPPPCurve(), LeaderboardContext.BeatLeaderDefault);
            if (customEvent)
            {
                pool.LsLeaderboadInfo.Add(new ShortScore("BLHASH_SOLOSTANDARD_9", true));
                pool.LsLeaderboadInfo.Add(new ShortScore("BLHASH_SOLOSTANDARD_7", true));
            }
            return (new PPCalculatorBeatLeader<CountingBeatLeader>(new Dictionary<string, PPPMapPool>(), settings), pool);
        }

        private static PPPBeatMapInfo MapInfo(BeatMapDifficulty difficulty)
        {
            return new PPPBeatMapInfo("BLHASH", new BeatmapKey { serializedName = "Standard", difficulty = difficulty });
        }

        internal class CountingBeatLeader : MockBeatLeader, IBeatLeaderAPI
        {
            public static int SongRequests;
            public static int CurveRequests;

            public new Task<BeatLeaderDataTypes.BeatLeaderSong> GetSongByHash(string hash)
            {
                SongRequests++;
                return base.GetSongByHash(hash);
            }

            public new Task<BeatLeaderDataTypes.BeatLeaderCurve> GetCurveByLeaderboardId(string leaderboardId, string modifiers, long leaderboardContextId)
            {
                CurveRequests++;
                return base.GetCurveByLeaderboardId(leaderboardId, modifiers, leaderboardContextId);
            }
        }
    }
}
