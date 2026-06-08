using PPPredictor.Core.Calculator;
using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.DataType.MapPool;
using PPPredictor.Core.DataType.Score;
using PPPredictor.Core.UnitTest.MockService;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.UnitTest.LeaderBoard
{
    [TestClass]
    public class TestAdditionalLeaderboards
    {
        [TestMethod]
        public async Task NoLeaderboardCoversFallbackCalculator()
        {
            CalculatorInstance ci = await CreateCalculator(new Settings(false, false, false, false, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "", 7, 12));

            List<PPPMapPoolShort> pools = ci.GetMapPools(Leaderboard.NoLeaderboard);
            Assert.AreEqual(1, pools.Count);
            Assert.AreEqual("No Pool", pools[0].MapPoolName);

            PPPBeatMapInfo mapInfo = MapInfo("NOHASH", BeatMapDifficulty.ExpertPlus, 5);
            PPPBeatMapInfo returnedInfo = await ci.GetBeatMapInfoAsync(Leaderboard.NoLeaderboard, "-1", mapInfo);
            Assert.AreSame(mapInfo, returnedInfo);
            Assert.AreSame(mapInfo, ci.ApplyModifiersToBeatmapInfo(Leaderboard.NoLeaderboard, "-1", mapInfo, new GameplayModifiers(), true, true));

            await ci.GetPlayerScores(Leaderboard.NoLeaderboard, "-1", 10, 100, true);
            Assert.IsNull(ci.GetPersonalBest(Leaderboard.NoLeaderboard, "-1", "NOHASH_ExpertPlus"));
            Assert.IsTrue(ci.IsScoreSetOnCurrentMapPool(Leaderboard.NoLeaderboard, "-1", new PPPScoreSetData()));
            (PPPPlayer sessionPlayer, PPPPlayer currentPlayer) = await ci.UpdatePlayer(Leaderboard.NoLeaderboard, "-1", true);
            Assert.AreEqual(0, sessionPlayer.Pp);
            Assert.AreEqual(0, currentPlayer.Rank);

            RankGainResult rankGain = await ci.GetPlayerRankGain(Leaderboard.NoLeaderboard, "-1", 500);
            Assert.AreEqual(0, rankGain.RankGlobal);
            Assert.IsTrue(ci.GetSaveData().ContainsKey(Leaderboard.NoLeaderboard.ToString()));
        }

        [TestMethod]
        public async Task AccSaberCoversCustomAndOverallPoolFlows()
        {
            CalculatorInstance ci = await CreateCalculator(new Settings(false, false, false, true, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "", 7, 12));

            List<PPPMapPoolShort> pools = ci.GetMapPools(Leaderboard.AccSaber);
            Assert.AreEqual(5, pools.Count);
            Assert.IsTrue(pools.Exists(x => x.Id == "standard"));

            await ci.UpdateMapPoolDetails(Leaderboard.AccSaber, "standard");
            PPPBeatMapInfo mapInfo = await ci.GetBeatMapInfoAsync(Leaderboard.AccSaber, "standard", MapInfo("ACCHASH", BeatMapDifficulty.ExpertPlus, 0));
            Assert.AreEqual(13.5, mapInfo.ModifiedStarRating.Stars);

            PPPBeatMapInfo modifiedInfo = ci.ApplyModifiersToBeatmapInfo(Leaderboard.AccSaber, "standard", mapInfo, new GameplayModifiers { disappearingArrows = true }, true, true);
            Assert.AreEqual(13.5, modifiedInfo.ModifiedStarRating.Stars);
            Assert.IsTrue(ci.IsScoreSetOnCurrentMapPool(Leaderboard.AccSaber, "standard", new PPPScoreSetData { hash = "ACCHASH" }));
            Assert.IsFalse(ci.IsScoreSetOnCurrentMapPool(Leaderboard.AccSaber, "standard", new PPPScoreSetData { hash = "MISSING" }));

            (PPPPlayer sessionPlayer, PPPPlayer currentPlayer) = await ci.UpdatePlayer(Leaderboard.AccSaber, "standard", true);
            Assert.AreEqual(50, sessionPlayer.Rank);
            Assert.AreEqual(600, currentPlayer.Pp);

            await ci.GetPlayerScores(Leaderboard.AccSaber, "standard", 10, 100, true);
            Assert.AreEqual(300, ci.GetPersonalBest(Leaderboard.AccSaber, "standard", "ACCHASH_SOLOSTANDARD_9"));

            await ci.UpdateMapPoolDetails(Leaderboard.AccSaber, "overall");
            await ci.GetPlayerScores(Leaderboard.AccSaber, "overall", 10, 100, true);
            PPGainResult gain = ci.GetPlayerScorePPGain(Leaderboard.AccSaber, "overall", "ACCHASH_SOLOSTANDARD_9", 350);
            Assert.IsTrue(gain.PpGainWeighted > 0);

            RankGainResult rankGain = await ci.GetPlayerRankGain(Leaderboard.AccSaber, "standard", 700);
            Assert.AreEqual(-1, rankGain.RankGlobal);
        }

        [TestMethod]
        public async Task AccSaberReloadedCoversPoolDiscoveryScoresAndOverallLookup()
        {
            CalculatorInstance ci = await CreateCalculator(new Settings(false, false, false, false, true, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "", 7, 12));

            List<PPPMapPoolShort> pools = ci.GetMapPools(Leaderboard.AccSaberReloaded);
            Assert.AreEqual(4, pools.Count);
            Assert.IsNotNull(ci.FindPoolWithSyncURL(Leaderboard.AccSaberReloaded, "https://api.accsaberreloaded.com/v1/playlists/standard_acc"));

            PPPBeatMapInfo standardInfo = await ci.GetBeatMapInfoAsync(Leaderboard.AccSaberReloaded, "standard", MapInfo("ASRHASH", BeatMapDifficulty.ExpertPlus, 0));
            Assert.AreEqual(14.2, standardInfo.ModifiedStarRating.Stars);
            PPPBeatMapInfo overallInfo = await ci.GetBeatMapInfoAsync(Leaderboard.AccSaberReloaded, "overall", MapInfo("ASRHASH", BeatMapDifficulty.ExpertPlus, 0));
            Assert.AreEqual(14.2, overallInfo.ModifiedStarRating.Stars);

            (PPPPlayer sessionPlayer, PPPPlayer currentPlayer) = await ci.UpdatePlayer(Leaderboard.AccSaberReloaded, "standard", true);
            Assert.AreEqual(42, sessionPlayer.Rank);
            Assert.AreEqual(720, currentPlayer.Pp);

            await ci.GetPlayerScores(Leaderboard.AccSaberReloaded, "standard", 10, 100, true);
            Assert.AreEqual(320, ci.GetPersonalBest(Leaderboard.AccSaberReloaded, "standard", "ASRHASH_SOLOSTANDARD_9"));
            Assert.IsTrue(ci.IsScoreSetOnCurrentMapPool(Leaderboard.AccSaberReloaded, "overall", new PPPScoreSetData { hash = "ASRHASH" }));

            PPGainResult gain = ci.GetPlayerScorePPGain(Leaderboard.AccSaberReloaded, "overall", "ASRHASH_SOLOSTANDARD_9", 350);
            Assert.IsTrue(gain.PpGainWeighted > 0);
            Assert.AreNotEqual(-2, ci.CalculatePPatPercentage(Leaderboard.AccSaberReloaded, "standard", standardInfo, 95, false, false));
            Assert.AreNotEqual(-2, ci.CalculateMaxPP(Leaderboard.AccSaberReloaded, "standard", standardInfo));
        }

        [TestMethod]
        public async Task BeatLeaderCoversDefaultAndCustomPoolFlows()
        {
            Dictionary<string, PPPMapPool> customPools = new Dictionary<string, PPPMapPool>
            {
                {
                    "987",
                    new PPPMapPool("987", "654", MapPoolType.Custom, "Event Mock", new PPPWeightingInfo(0.925f), 10, new BeatLeaderPPPCurve(), "event.png")
                }
            };
            Dictionary<string, LeaderboardData> savedData = new Dictionary<string, LeaderboardData>
            {
                { Leaderboard.BeatLeader.ToString(), new LeaderboardData { DctMapPool = customPools } }
            };
            CalculatorInstance ci = await CreateCalculator(new Settings(false, true, false, false, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "", 7, 12), savedData);

            Assert.AreEqual(6, ci.GetMapPools(Leaderboard.BeatLeader).Count);
            Assert.IsNotNull(ci.FindPoolWithPlayListId(Leaderboard.BeatLeader, "654"));

            PPPBeatMapInfo mapInfo = await ci.GetBeatMapInfoAsync(Leaderboard.BeatLeader, "-1", MapInfo("BLHASH", BeatMapDifficulty.ExpertPlus, 0));
            Assert.AreEqual(6.25, mapInfo.ModifiedStarRating.PassRating);

            GameplayModifiers modifiers = new GameplayModifiers { songSpeed = GameplayModifiers.SongSpeed.Faster, ghostNotes = true };
            PPPBeatMapInfo modifiedInfo = ci.ApplyModifiersToBeatmapInfo(Leaderboard.BeatLeader, "-1", mapInfo, modifiers, false, false);
            Assert.IsTrue(modifiedInfo.ModifiedStarRating.Multiplier > 1);
            Assert.AreEqual(5.1, modifiedInfo.ModifiedStarRating.AccRating);
            Assert.IsTrue(ci.GetStarDisplayForCalculator(Leaderboard.BeatLeader, "-1", modifiedInfo).Contains("5.10"));
            Assert.IsFalse(ci.IsScoreSetOnCurrentMapPool(Leaderboard.BeatLeader, "-2", new PPPScoreSetData { context = 2 }));
            Assert.IsTrue(ci.IsScoreSetOnCurrentMapPool(Leaderboard.BeatLeader, "-2", new PPPScoreSetData { context = 4 }));

            await ci.GetPlayerScores(Leaderboard.BeatLeader, "-1", 10, 100, true);
            Assert.AreEqual(450, ci.GetPersonalBest(Leaderboard.BeatLeader, "-1", "BLHASH_SOLOSTANDARD_9"));
            (PPPPlayer sessionPlayer, PPPPlayer currentPlayer) = await ci.UpdatePlayer(Leaderboard.BeatLeader, "-1", true);
            Assert.AreEqual(150, sessionPlayer.Rank);
            Assert.AreEqual(12345, currentPlayer.Pp);

            await ci.UpdateMapPoolDetails(Leaderboard.BeatLeader, "987");
            PPPBeatMapInfo customInfo = await ci.GetBeatMapInfoAsync(Leaderboard.BeatLeader, "987", MapInfo("BLHASH", BeatMapDifficulty.ExpertPlus, 0));
            Assert.AreEqual(6.25, customInfo.ModifiedStarRating.PassRating);
            (sessionPlayer, currentPlayer) = await ci.UpdatePlayer(Leaderboard.BeatLeader, "987", true);
            Assert.AreEqual(3, currentPlayer.Rank);

            RankGainResult rankGain = await ci.GetPlayerRankGain(Leaderboard.BeatLeader, "-1", 12500);
            Assert.IsTrue(rankGain.RankGainGlobal > 0);
        }

        [TestMethod]
        public async Task HitBloqCoversPoolSortingScoresAndRankLookup()
        {
            CalculatorInstance ci = await CreateCalculator(new Settings(false, false, true, false, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Popularity, "765", 7, 12));

            List<PPPMapPoolShort> pools = ci.GetMapPools(Leaderboard.HitBloq);
            Assert.AreEqual(3, pools.Count);
            Assert.AreEqual("Beta Pool", pools[1].MapPoolName);
            Assert.IsNotNull(ci.FindPoolWithSyncURL(Leaderboard.HitBloq, "https://mock/hitbloq/beta"));

            await ci.UpdateMapPoolDetails(Leaderboard.HitBloq, "pool-a");
            PPPBeatMapInfo mapInfo = await ci.GetBeatMapInfoAsync(Leaderboard.HitBloq, "pool-a", MapInfo("HBHASH", BeatMapDifficulty.ExpertPlus, 0));
            Assert.AreEqual(8.75, mapInfo.ModifiedStarRating.Stars);
            Assert.AreEqual(0, ci.ApplyModifiersToBeatmapInfo(Leaderboard.HitBloq, "pool-a", mapInfo, new GameplayModifiers(), true).ModifiedStarRating.Stars);

            await ci.GetPlayerScores(Leaderboard.HitBloq, "pool-a", 10, 100, true);
            Assert.AreEqual(500, ci.GetPersonalBest(Leaderboard.HitBloq, "pool-a", "HBHASH_SOLOSTANDARD_9"));
            (PPPPlayer sessionPlayer, PPPPlayer currentPlayer) = await ci.UpdatePlayer(Leaderboard.HitBloq, "pool-a", true);
            Assert.AreEqual(33, sessionPlayer.Rank);
            Assert.AreEqual(900, currentPlayer.Pp);

            PPGainResult gain = ci.GetPlayerScorePPGain(Leaderboard.HitBloq, "pool-a", "HBHASH_SOLOSTANDARD_9", 650);
            Assert.IsTrue(gain.PpGainWeighted > 0);
            RankGainResult rankGain = await ci.GetPlayerRankGain(Leaderboard.HitBloq, "pool-a", 1100);
            Assert.AreEqual(34, rankGain.RankGainGlobal);
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
            BeatmapKey key = new BeatmapKey
            {
                serializedName = "Standard",
                difficulty = difficulty
            };

            return new PPPBeatMapInfo(new PPPBeatMapInfo(hash, key), new PPPStarRating(stars));
        }
    }
}
