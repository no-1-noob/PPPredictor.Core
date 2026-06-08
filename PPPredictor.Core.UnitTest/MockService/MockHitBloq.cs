using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.Interface;

namespace PPPredictor.Core.UnitTest.MockService
{
    internal class MockHitBloq : IHitBloqAPI
    {
        public Task<List<HitBloqDataTypes.HitBloqScores>> GetAllScores(string userId, string poolId)
        {
            return Task.FromResult(new List<HitBloqDataTypes.HitBloqScores>
            {
                Score("HBHASH_ep_s", 500, 1),
                Score("HBHASH2_ex_s", 350, 2)
            });
        }

        public Task<HitBloqDataTypes.HitBloqMapPoolDetails> GetHitBloqMapPoolDetails(string poolIdent, int page)
        {
            return Task.FromResult(new HitBloqDataTypes.HitBloqMapPoolDetails
            {
                accumulation_constant = 0.95f,
                cr_curve = new HitBloqDataTypes.HitBloqCrCurve
                {
                    type = "linear",
                    points = new List<double[]>
                    {
                        new[] { 0d, 0d },
                        new[] { 0.9d, 400d },
                        new[] { 1d, 600d }
                    }
                },
                leaderboard_id_list = new List<string> { "HBHASH_ep_s" }
            });
        }

        public Task<List<HitBloqDataTypes.HitBloqMapPool>> GetHitBloqMapPools()
        {
            return Task.FromResult(new List<HitBloqDataTypes.HitBloqMapPool>
            {
                new HitBloqDataTypes.HitBloqMapPool
                {
                    id = "pool-a",
                    title = "Alpha Pool",
                    image = "alpha.png",
                    popularity = 20,
                    download_url = "https://mock/hitbloq/alpha"
                },
                new HitBloqDataTypes.HitBloqMapPool
                {
                    id = "pool-b",
                    title = "Beta Pool",
                    image = "beta.png",
                    popularity = 50,
                    download_url = "https://mock/hitbloq/beta"
                }
            });
        }

        public Task<HitBloqDataTypes.HitBloqUser> GetHitBloqUserByPool(long userId, string poolIdent)
        {
            return Task.FromResult(new HitBloqDataTypes.HitBloqUser
            {
                cr = 900,
                rank = 33,
                username = "Mock HitBloq Player"
            });
        }

        public Task<HitBloqDataTypes.HitBloqUserId> GetHitBloqUserIdByUserId(string id)
        {
            return Task.FromResult(new HitBloqDataTypes.HitBloqUserId { id = 123 });
        }

        public Task<HitBloqDataTypes.HitBloqLeaderboardInfo> GetLeaderBoardInfo(string searchString)
        {
            return Task.FromResult(new HitBloqDataTypes.HitBloqLeaderboardInfo
            {
                star_rating = new Dictionary<string, double>
                {
                    { "pool-a", 8.75 },
                    { "pool-b", 9.25 }
                }
            });
        }

        public Task<HitBloqDataTypes.HitBloqLadder> GetPlayerListForMapPool(double page, string mapPoolId)
        {
            return Task.FromResult(new HitBloqDataTypes.HitBloqLadder
            {
                ladder = Enumerable.Range(0, 50).Select(i => new HitBloqDataTypes.HitBloqUser
                {
                    rank = ((int)page - 1) * 50 + i + 1,
                    cr = 1200 - (((int)page - 1) * 50 + i) * 4,
                    username = $"HitBloq {i}"
                }).ToList()
            });
        }

        public Task<HitBloqDataTypes.HitBloqRankFromCr> GetPlayerRankByCr(string mapPoolId, double cr)
        {
            return Task.FromResult(new HitBloqDataTypes.HitBloqRankFromCr
            {
                rank = cr > 1000 ? 1 : 25
            });
        }

        public Task<List<HitBloqDataTypes.HitBloqScores>> GetRecentScores(string userId, string poolId, int page)
        {
            return Task.FromResult(new List<HitBloqDataTypes.HitBloqScores>
            {
                Score("HBHASH_ep_s", 500, 3),
                Score("HBHASH3_h_s", 250, 4)
            });
        }

        private static HitBloqDataTypes.HitBloqScores Score(string songId, float cr, int day)
        {
            return new HitBloqDataTypes.HitBloqScores
            {
                song_id = songId,
                cr_received = cr,
                time = new DateTimeOffset(new DateTime(2024, 4, day)).ToUnixTimeSeconds()
            };
        }
    }
}
