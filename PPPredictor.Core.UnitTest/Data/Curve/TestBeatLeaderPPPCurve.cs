using PPPredictor.Core;
using PPPredictor.Core.Calculator;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.MapPool;
using PPPredictor.Core.UnitTest.MockService;
using static PPPredictor.Core.DataType.Enums;

namespace UnitTests.Data.Curve
{
    [TestClass]
    public class TestBeatLeaderPPPCurve
    {
        private readonly PPPBeatMapInfo beatMapInfo = new PPPBeatMapInfo(new PPPBeatMapInfo(),
            new PPPStarRating(1, 11.82055, 7.9189496, 5.424088, 0.96, 1, 9, "mock", new Dictionary<string, CurveInfo>
            {
                { string.Empty, new CurveInfo(CurveType.Linear, new List<(double, double)> { (1, 1000), (0, 0) }, 0) }
            }, true));
        private readonly BeatLeaderPPPCurve curve = new BeatLeaderPPPCurve();

        [TestMethod]
        public void TestCalculatePPatPercentage()
        {
            double result = curve.CalculatePPatPercentage(beatMapInfo, 96.39, false, false, LeaderboardContext.BeatLeaderDefault);
            Assert.AreEqual(963.9, result, 0.001);
        }

        [TestMethod]
        [DataRow(LeaderboardContext.BeatLeaderDefault)]
        [DataRow(LeaderboardContext.BeatLeaderNoPauses)]
        [DataRow(LeaderboardContext.BeatLeaderNoModifiers)]
        [DataRow(LeaderboardContext.BeatLeaderGolf)]
        public async Task FailedLevelsHaveZeroPP(LeaderboardContext context)
        {
            await ApplyModifiers(context, new GameplayModifiers(), true, false);
            Assert.AreEqual(0, curve.CalculatePPatPercentage(beatMapInfo, 30, true, false, context));
        }

        [TestMethod]
        [DataRow(LeaderboardContext.BeatLeaderDefault)]
        [DataRow(LeaderboardContext.BeatLeaderNoPauses)]
        [DataRow(LeaderboardContext.BeatLeaderNoModifiers)]
        [DataRow(LeaderboardContext.BeatLeaderGolf)]
        public async Task TestCalculatePPatPercentageWhenPaused(LeaderboardContext context)
        {
            await ApplyModifiers(context, new GameplayModifiers(), false, false);
            double unpaused = curve.CalculatePPatPercentage(beatMapInfo, 30, false, false, context);
            Assert.IsTrue(unpaused > 0);

            await ApplyModifiers(context, new GameplayModifiers(), false, true);
            double paused = curve.CalculatePPatPercentage(beatMapInfo, 30, false, true, context);
            Assert.AreEqual(context == LeaderboardContext.BeatLeaderNoPauses ? 0 : unpaused, paused, 0.001);
        }

        [TestMethod]
        public async Task TestCalculatePPatPercentageModifier()
        {
            await ApplyModifiers(LeaderboardContext.BeatLeaderDefault, new GameplayModifiers(), false, false);
            double unmodified = curve.CalculatePPatPercentage(beatMapInfo, 96.39, false, false, LeaderboardContext.BeatLeaderDefault);
            beatMapInfo.BaseStarRating.ModifierValues["gn"] = 0.11;
            await ApplyModifiers(LeaderboardContext.BeatLeaderDefault, new GameplayModifiers { ghostNotes = true }, false, false);
            double modified = curve.CalculatePPatPercentage(beatMapInfo, 96.39, false, false, LeaderboardContext.BeatLeaderDefault);
            Assert.IsTrue(modified > unmodified);
        }

        [TestMethod]
        public async Task TestCalculatePPatPercentageGolf()
        {
            beatMapInfo.BaseStarRating.DctCurves.Clear();
            await ApplyModifiers(LeaderboardContext.BeatLeaderGolf, new GameplayModifiers(), false, false);
            double result = curve.CalculatePPatPercentage(beatMapInfo, 49.77, false, false, LeaderboardContext.BeatLeaderGolf);
            Assert.AreEqual(200.92, result, 0.001);
            Assert.AreEqual(0, curve.CalculatePPatPercentage(beatMapInfo, 51, false, false, LeaderboardContext.BeatLeaderGolf));
        }

        [TestMethod]
        public void GolfCurveUsesOriginalAccuracy()
        {
            // Snapshot of the Golf API curve for leaderboard 13b2b11, without modifiers.
            beatMapInfo.ModifiedStarRating.DctCurves[string.Empty] = new CurveInfo(
                CurveType.Linear, new List<(double, double)> { (0.35, 240.8), (0.3, 256.11) }, 0);

            double result = curve.CalculatePPatPercentage(beatMapInfo, 30.82, false, false, LeaderboardContext.BeatLeaderGolf);
            Assert.AreEqual(253.60, result, 0.01);
        }

        [TestMethod]
        public void TestToCurveInfo()
        {
            Assert.AreEqual(CurveType.BeatLeader, curve.ToCurveInfo().CurveType);
        }

        [TestMethod]
        public void NegativePassRatingDoesNotAffectFetchedCurve()
        {
            beatMapInfo.BaseStarRating.PassRating = -7.9189496;
            double result = curve.CalculatePPatPercentage(beatMapInfo, 96.39, false, false, LeaderboardContext.BeatLeaderDefault);
            Assert.AreEqual(963.9, result, 0.001);
        }

        [TestMethod]
        public void TestExceptionHandling()
        {
            Assert.AreEqual(-1, curve.CalculatePPatPercentage(null!, 95, false, false, LeaderboardContext.BeatLeaderDefault));
        }

        [TestMethod]
        public void TestCalculateMaxPP()
        {
            double result = curve.CalculateMaxPP(beatMapInfo, LeaderboardContext.BeatLeaderDefault);
            Assert.AreEqual(1000, result, 0.001);
            Assert.AreEqual(curve.CalculatePPatPercentage(beatMapInfo, 100, false, false, LeaderboardContext.BeatLeaderDefault), result);
        }

        private async Task ApplyModifiers(LeaderboardContext context, GameplayModifiers modifiers, bool failed, bool paused)
        {
            Settings settings = new Settings(false, true, false, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, 7, 12);
            PPCalculatorBeatLeader<MockBeatLeader> calculator = new PPCalculatorBeatLeader<MockBeatLeader>(new Dictionary<string, PPPMapPool>(), settings);
            await calculator.ApplyModifiersToBeatmapInfo(beatMapInfo, new PPPMapPool { LeaderboardContext = context }, modifiers, failed, paused);
        }
    }
}
