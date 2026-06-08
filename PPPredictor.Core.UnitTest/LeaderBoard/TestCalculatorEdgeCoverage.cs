using PPPredictor.Core.Calculator;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.DataType.MapPool;
using PPPredictor.Core.UnitTest.MockService;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.UnitTest.LeaderBoard
{
    [TestClass]
    public class TestCalculatorEdgeCoverage
    {
        [TestMethod]
        public async Task BeatLeaderCoversCachedMissingAndModifierContextBranches()
        {
            CalculatorInstance ci = await CreateCalculator(new Settings(false, true, false, false, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "", 7, 12));

            PPPBeatMapInfo first = await ci.GetBeatMapInfoAsync(Leaderboard.BeatLeader, "-1", MapInfo("BLHASH", BeatMapDifficulty.ExpertPlus, 0));
            PPPBeatMapInfo cached = await ci.GetBeatMapInfoAsync(Leaderboard.BeatLeader, "-1", MapInfo("BLHASH", BeatMapDifficulty.ExpertPlus, 0));
            Assert.AreEqual(first.ModifiedStarRating.PassRating, cached.ModifiedStarRating.PassRating);

            PPPBeatMapInfo notRanked = await ci.GetBeatMapInfoAsync(Leaderboard.BeatLeader, "-1", MapInfo("BLHASH", BeatMapDifficulty.Expert, 0));
            Assert.AreEqual(0, notRanked.ModifiedStarRating.Stars);

            PPPBeatMapInfo baseInfo = MapInfo("BLHASH", BeatMapDifficulty.ExpertPlus, 10);
            baseInfo.BaseStarRating.ModifierValues = new Dictionary<string, double>
            {
                { "da", 0.01 },
                { "ss", -0.15 },
                { "sf", 0.2 },
                { "na", 0.03 },
                { "nb", 0.04 },
                { "nf", -0.5 },
                { "no", 0.05 },
                { "pm", 0.06 },
                { "sc", 0.07 },
                { "if", 0.08 },
                { "be", 0.09 },
                { "sa", 0.1 },
                { "zm", 0.11 }
            };
            baseInfo.BaseStarRating.ModifiersRating = null;

            GameplayModifiers allModifiers = new GameplayModifiers
            {
                disappearingArrows = true,
                songSpeed = GameplayModifiers.SongSpeed.SuperFast,
                noArrows = true,
                noBombs = true,
                noFailOn0Energy = true,
                enabledObstacleType = GameplayModifiers.EnabledObstacleType.NoObstacles,
                proMode = true,
                smallCubes = true,
                instaFail = true,
                energyType = GameplayModifiers.EnergyType.Battery,
                strictAngles = true,
                zenMode = true
            };

            Assert.AreEqual(0, ci.ApplyModifiersToBeatmapInfo(Leaderboard.BeatLeader, "-2", baseInfo, allModifiers).ModifiedStarRating.Multiplier);
            Assert.AreEqual(0, ci.ApplyModifiersToBeatmapInfo(Leaderboard.BeatLeader, "-4", baseInfo, new GameplayModifiers { songSpeed = GameplayModifiers.SongSpeed.Slower }).ModifiedStarRating.Multiplier);
            Assert.AreEqual(0, ci.ApplyModifiersToBeatmapInfo(Leaderboard.BeatLeader, "-5", baseInfo, new GameplayModifiers { smallCubes = true }).ModifiedStarRating.Multiplier);
            Assert.IsTrue(ci.ApplyModifiersToBeatmapInfo(Leaderboard.BeatLeader, "-5", baseInfo, new GameplayModifiers { smallCubes = true, proMode = true }).ModifiedStarRating.Multiplier > 1);
            Assert.IsTrue(ci.ApplyModifiersToBeatmapInfo(Leaderboard.BeatLeader, "-1", baseInfo, allModifiers, true).ModifiedStarRating.Multiplier < ci.ApplyModifiersToBeatmapInfo(Leaderboard.BeatLeader, "-1", baseInfo, allModifiers, false).ModifiedStarRating.Multiplier);
        }

        [TestMethod]
        public async Task HitBloqCoversCachedSortingDefaultAndParseBranches()
        {
            CalculatorInstance ci = await CreateCalculator(new Settings(false, false, true, false, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "765", 7, 12));

            List<PPPMapPoolShort> alphabeticalPools = ci.GetMapPools(Leaderboard.HitBloq);
            Assert.AreEqual("Alpha Pool", alphabeticalPools[1].MapPoolName);

            await ci.UpdateMapPoolDetails(Leaderboard.HitBloq, "pool-a");
            PPPBeatMapInfo first = await ci.GetBeatMapInfoAsync(Leaderboard.HitBloq, "pool-a", MapInfo("HBHASH", BeatMapDifficulty.ExpertPlus, 0));
            PPPBeatMapInfo cached = await ci.GetBeatMapInfoAsync(Leaderboard.HitBloq, "pool-a", MapInfo("HBHASH", BeatMapDifficulty.ExpertPlus, 0));
            Assert.AreEqual(first.ModifiedStarRating.Stars, cached.ModifiedStarRating.Stars);

            (PPPPlayer sessionPlayer, PPPPlayer currentPlayer) = await ci.UpdatePlayer(Leaderboard.HitBloq, "-1", true);
            Assert.IsTrue(sessionPlayer.IsErrorUser);
            Assert.IsTrue(currentPlayer.IsErrorUser);
            await ci.GetPlayerScores(Leaderboard.HitBloq, "-1", 10, 100, true);

            CalculatorInstance invalidSort = await CreateCalculator(new Settings(false, false, true, false, false, "123", PPGainCalculationType.Raw, (MapPoolSorting)999, "765", 7, 12));
            Assert.AreEqual(0, invalidSort.GetMapPools(Leaderboard.HitBloq).Count);

            Assert.AreEqual(("HBHASH", "ExpertPlus", "SoloStandard"), PPCalculatorHitBloq<MockHitBloq>.ParseHashDiffAndMode("HBHASH_ep_s"));
            Assert.AreEqual((string.Empty, string.Empty, string.Empty), PPCalculatorHitBloq<MockHitBloq>.ParseHashDiffAndMode(null));
            Assert.AreEqual((string.Empty, string.Empty, string.Empty), PPCalculatorHitBloq<MockHitBloq>.ParseHashDiffAndMode("invalid"));
            Assert.AreEqual((string.Empty, string.Empty, string.Empty), PPCalculatorHitBloq<MockHitBloq>.ParseHashDiffAndMode("HBHASH_unknown_s"));
        }

        [TestMethod]
        public async Task AccSaberReloadedCoversExistingPoolsFallbacksAndOverallCalculation()
        {
            Dictionary<string, PPPMapPool> pools = new Dictionary<string, PPPMapPool>
            {
                {
                    "standard",
                    new PPPMapPool("standard", "standard_acc", MapPoolType.Custom, "Old Standard", new PPPWeightingInfo(0), 0, CurveParser.ParseToCurve(new CurveInfo(CurveType.AccSaberReloaded)), string.Empty)
                    {
                        DtUtcLastRefresh = new DateTime(2000, 1, 1)
                    }
                }
            };
            Dictionary<string, LeaderboardData> savedData = new Dictionary<string, LeaderboardData>
            {
                { Leaderboard.AccSaberReloaded.ToString(), new LeaderboardData { DctMapPool = pools } }
            };

            CalculatorInstance ci = await CreateCalculator(new Settings(false, false, false, false, true, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "", 7, 12), savedData);

            PPPBeatMapInfo emptyHash = MapInfo(string.Empty, BeatMapDifficulty.ExpertPlus, 3);
            Assert.AreSame(emptyHash, await ci.GetBeatMapInfoAsync(Leaderboard.AccSaberReloaded, "standard", emptyHash));
            Assert.AreEqual(0, (await ci.GetBeatMapInfoAsync(Leaderboard.AccSaberReloaded, "standard", MapInfo("MISSING", BeatMapDifficulty.ExpertPlus, 0))).ModifiedStarRating.Stars);

            PPPBeatMapInfo standardInfo = await ci.GetBeatMapInfoAsync(Leaderboard.AccSaberReloaded, "standard", MapInfo("ASRHASH", BeatMapDifficulty.ExpertPlus, 0));
            Assert.AreNotEqual(-2, ci.CalculatePPatPercentage(Leaderboard.AccSaberReloaded, "overall", standardInfo, 95, false, false));
            Assert.AreNotEqual(-2, ci.CalculateMaxPP(Leaderboard.AccSaberReloaded, "overall", standardInfo));
            Assert.IsFalse(ci.IsScoreSetOnCurrentMapPool(Leaderboard.AccSaberReloaded, "standard", new PPPScoreSetData { hash = "MISSING" }));
            Assert.AreEqual(0, ci.GetPlayerScorePPGain(Leaderboard.AccSaberReloaded, "overall", "MISSING_SOLOSTANDARD_9", 123).PpTotal);
        }

        [TestMethod]
        public async Task AccSaberCoversFallbackAndDefaultPoolBranches()
        {
            CalculatorInstance ci = await CreateCalculator(new Settings(false, false, false, true, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "", 7, 12));

            PPPBeatMapInfo emptyHash = MapInfo(string.Empty, BeatMapDifficulty.ExpertPlus, 3);
            Assert.AreSame(emptyHash, await ci.GetBeatMapInfoAsync(Leaderboard.AccSaber, "standard", emptyHash));
            Assert.AreEqual(0, (await ci.GetBeatMapInfoAsync(Leaderboard.AccSaber, "standard", MapInfo("MISSING", BeatMapDifficulty.ExpertPlus, 0))).ModifiedStarRating.Stars);

            (PPPPlayer sessionPlayer, PPPPlayer currentPlayer) = await ci.UpdatePlayer(Leaderboard.AccSaber, "-1", true);
            Assert.IsTrue(sessionPlayer.IsErrorUser);
            Assert.IsTrue(currentPlayer.IsErrorUser);
            await ci.GetPlayerScores(Leaderboard.AccSaber, "-1", 10, 100, true);

            Assert.AreEqual(0, ci.GetPlayerScorePPGain(Leaderboard.AccSaber, "overall", "MISSING_SOLOSTANDARD_9", 123).PpTotal);
        }

        [TestMethod]
        public async Task DirectInternalCalculatorsCoverFallbackMethods()
        {
            Settings settings = new Settings(false, false, false, false, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "", 7, 12);
            PPPMapPool defaultPool = new PPPMapPool();
            PPPMapPool customPool = new PPPMapPool("987", "654", MapPoolType.Custom, "Event Mock", new PPPWeightingInfo(0.925f), 10, new BeatLeaderPPPCurve(), "event.png");

            PPCalculatorNoLeaderboard noLeaderboard = new PPCalculatorNoLeaderboard(new Dictionary<string, PPPMapPool>(), settings);
            Assert.AreEqual(0, (await noLeaderboard.GetPlayers(1, defaultPool)).Count);
            Assert.AreEqual(0, (await noLeaderboard.GetRecentScores("123", 10, 1, defaultPool)).LsPPPScore.Count);
            Assert.AreEqual(0, (await noLeaderboard.GetAllScores("123", defaultPool)).LsPPPScore.Count);

            PPCalculatorScoreSaber<MockScoreSaberApi> scoreSaber = new PPCalculatorScoreSaber<MockScoreSaberApi>(new Dictionary<string, PPPMapPool>(), settings, _ => MapInfo("LOOKUP", BeatMapDifficulty.ExpertPlus, 8));
            Assert.AreEqual(2000, (await scoreSaber.GetPlayerInfo(123, defaultPool)).Rank);
            Assert.AreEqual(50, (await scoreSaber.GetPlayers(1, defaultPool)).Count);
            Assert.AreEqual(0, (await scoreSaber.GetAllScores("123", defaultPool)).LsPPPScore.Count);
            Assert.AreEqual(0, (await scoreSaber.GetRecentScores("123", 10, 1, defaultPool)).LsPPPScore.Count);
            Assert.AreEqual(0, (await scoreSaber.GetBeatMapInfoAsync(MapInfo(string.Empty, BeatMapDifficulty.ExpertPlus, 8), defaultPool)).ModifiedStarRating.Stars);
            PPPBeatMapInfo oldDotsInfo = MapInfo("HASH", BeatMapDifficulty.ExpertPlus, 8);
            oldDotsInfo.OldDotsEnabled = true;
            Assert.AreEqual(0, scoreSaber.ApplyModifiersToBeatmapInfo(oldDotsInfo, defaultPool, new GameplayModifiers()).ModifiedStarRating.Stars);
            await scoreSaber.UpdateAvailableMapPools();
            Assert.AreEqual(1, scoreSaber.GetMapPools().Count);

            PPCalculatorAccSaberReloaded<MockAccSaberReloaded> accSaberReloaded = new PPCalculatorAccSaberReloaded<MockAccSaberReloaded>(new Dictionary<string, PPPMapPool>(), settings);
            Assert.AreEqual(0, (await accSaberReloaded.GetRecentScores("123", 10, 0, defaultPool)).LsPPPScore.Count);
            Assert.AreEqual(0, (await accSaberReloaded.GetAllScores("123", defaultPool)).LsPPPScore.Count);
            Assert.AreEqual(0, (await accSaberReloaded.GetPlayers(1, defaultPool)).Count);
            Assert.IsTrue((await accSaberReloaded.GetPlayerInfo(123, new PPPMapPool("missing", "missing", MapPoolType.Custom, "Missing", new PPPWeightingInfo(0), 0, CurveParser.ParseToCurve(new CurveInfo(CurveType.AccSaberReloaded)), string.Empty))).IsErrorUser);
            Assert.AreEqual(string.Empty, accSaberReloaded.CreateSeachString(null, BeatKey(BeatMapDifficulty.ExpertPlus)));
            Assert.AreEqual("HASH", accSaberReloaded.CreateSeachString("hash", null));
            Assert.AreEqual(8, accSaberReloaded.ApplyModifiersToBeatmapInfo(MapInfo("HASH", BeatMapDifficulty.ExpertPlus, 8), defaultPool, new GameplayModifiers()).ModifiedStarRating.Stars);

            PPCalculatorAccSaber<MockAccSaber> accSaber = new PPCalculatorAccSaber<MockAccSaber>(new Dictionary<string, PPPMapPool>(), settings);
            Assert.AreEqual(0, (await accSaber.GetRecentScores("123", 10, 1, defaultPool)).LsPPPScore.Count);
            Assert.AreEqual(0, (await accSaber.GetAllScores("123", defaultPool)).LsPPPScore.Count);
            Assert.AreEqual(0, (await accSaber.GetPlayers(1, defaultPool)).Count);
            Assert.IsTrue((await accSaber.GetPlayerInfo(123, defaultPool)).IsErrorUser);
            PPPMapPool playerNotFoundPool = new PPPMapPool("standard", "standard", MapPoolType.Custom, "Standard", new PPPWeightingInfo(0), 0, CurveParser.ParseToCurve(new CurveInfo(CurveType.AccSaber)), string.Empty);
            playerNotFoundPool.IsPlayerFound = false;
            Assert.AreEqual(0, (await accSaber.GetPlayers(1, playerNotFoundPool)).Count);
            Assert.AreEqual(0, (await accSaber.GetAllScores("123", playerNotFoundPool)).LsPPPScore.Count);

            PPCalculatorBeatLeader<MockBeatLeader> beatLeader = new PPCalculatorBeatLeader<MockBeatLeader>(new Dictionary<string, PPPMapPool>(), settings);
            Assert.AreEqual(0, (await beatLeader.GetAllScores("123", defaultPool)).LsPPPScore.Count);
            Assert.AreEqual(50, (await beatLeader.GetPlayers(1, customPool)).Count);
            Assert.AreEqual(2, (await beatLeader.GetRecentScores("123", 10, 1, customPool)).LsPPPScore.Count);
            Assert.IsTrue((await beatLeader.GetPlayerInfo(999, customPool)).IsErrorUser);
            PPPBeatMapInfo missingCustomInfo = await beatLeader.GetBeatMapInfoAsync(MapInfo("MISSING", BeatMapDifficulty.ExpertPlus, 0), customPool);
            Assert.AreEqual(0, missingCustomInfo.ModifiedStarRating.Stars);

            PPCalculatorHitBloq<MockHitBloq> hitBloq = new PPCalculatorHitBloq<MockHitBloq>(new Dictionary<string, PPPMapPool>(), settings);
            PPPMapPool hitBloqPool = new PPPMapPool("pool-a", "pool-a", MapPoolType.Custom, "Alpha", new PPPWeightingInfo(0), 0, CustomPPPCurve.CreateDummyPPPCurve(), string.Empty);
            Assert.AreEqual(2, (await hitBloq.GetRecentScores("123", 10, 1, hitBloqPool)).LsPPPScore.Count);
            Assert.AreEqual(2, (await hitBloq.GetAllScores("123", hitBloqPool)).LsPPPScore.Count);
            Assert.AreEqual(-1, await hitBloq.GetPPToRank(string.Empty, 100));
            Assert.IsTrue(hitBloq.IsScoreSetOnCurrentMapPool(hitBloqPool, new PPPScoreSetData()));
        }

        private static Task<CalculatorInstance> CreateCalculator(Settings settings, Dictionary<string, LeaderboardData>? data = null)
        {
            return CalculatorInstance.CreateAsyncMock<MockScoreSaberApi, MockBeatLeader, MockHitBloq, MockAccSaber, MockAccSaberReloaded>(
                settings,
                data ?? new Dictionary<string, LeaderboardData>(),
                _ => new PPPBeatMapInfo());
        }

        private static PPPBeatMapInfo MapInfo(string hash, BeatMapDifficulty difficulty, double stars)
        {
            return new PPPBeatMapInfo(new PPPBeatMapInfo(hash, BeatKey(difficulty)), new PPPStarRating(stars));
        }

        private static BeatmapKey BeatKey(BeatMapDifficulty difficulty)
        {
            return new BeatmapKey
            {
                serializedName = "Standard",
                difficulty = difficulty
            };
        }
    }
}
