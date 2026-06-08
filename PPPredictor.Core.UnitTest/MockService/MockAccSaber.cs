using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.Interface;

namespace PPPredictor.Core.UnitTest.MockService
{
    internal class MockAccSaber : IAccSaberAPI
    {
        public Task<List<AccSaberDataTypes.AccSaberMapPool>> GetAccSaberMapPools()
        {
            return Task.FromResult(new List<AccSaberDataTypes.AccSaberMapPool>
            {
                new AccSaberDataTypes.AccSaberMapPool { categoryName = "standard", categoryDisplayName = "Standard" },
                new AccSaberDataTypes.AccSaberMapPool { categoryName = "tech", categoryDisplayName = "Tech" },
                new AccSaberDataTypes.AccSaberMapPool { categoryName = "true", categoryDisplayName = "True Acc" }
            });
        }

        public Task<AccSaberDataTypes.AccSaberPlayer> GetAccSaberUserByPool(long userId, string poolIdent)
        {
            return Task.FromResult(new AccSaberDataTypes.AccSaberPlayer
            {
                rank = poolIdent == "tech" ? 75 : 50,
                ap = poolIdent == "tech" ? 450 : 600
            });
        }

        public Task<List<AccSaberDataTypes.AccSaberRankedMap>> GetAllRankedMaps()
        {
            return Task.FromResult(new List<AccSaberDataTypes.AccSaberRankedMap>
            {
                RankedMap("ACCHASH", "ExpertPlus", 13.5, "Standard"),
                RankedMap("TECHHASH", "Expert", 9.25, "Tech"),
                RankedMap("TRUEHASH", "Hard", 7.75, "True Acc")
            });
        }

        public Task<List<AccSaberDataTypes.AccSaberScores>> GetAllScores(string userId)
        {
            return Task.FromResult(new List<AccSaberDataTypes.AccSaberScores>
            {
                Score("ACCHASH", "ExpertPlus", 300, 300, "Standard", 10),
                Score("TECHHASH", "Expert", 200, 180, "Tech", 20),
                Score("TRUEHASH", "Hard", 100, 90, "True Acc", 30)
            });
        }

        public Task<List<AccSaberDataTypes.AccSaberScores>> GetAllScoresByPool(string userId, string poolId)
        {
            return Task.FromResult(new List<AccSaberDataTypes.AccSaberScores>
            {
                Score("ACCHASH", "ExpertPlus", 300, 300, "Standard", 10),
                Score("ACCHASH2", "Expert", 240, 216, "Standard", 20)
            });
        }

        public Task<List<AccSaberDataTypes.AccSaberPlayer>> GetPlayerListForMapPool(double page, string mapPoolId)
        {
            return Task.FromResult(Enumerable.Range(0, 50).Select(i => new AccSaberDataTypes.AccSaberPlayer
            {
                rank = ((int)page - 1) * 50 + i + 1,
                ap = 1000 - (((int)page - 1) * 50 + i) * 5
            }).ToList());
        }

        public Task<List<AccSaberDataTypes.AccSaberRankedMap>> GetRankedMaps(string mapPool)
        {
            return Task.FromResult(new List<AccSaberDataTypes.AccSaberRankedMap>
            {
                RankedMap("ACCHASH", "ExpertPlus", 13.5, "Standard"),
                RankedMap("ACCHASH2", "Expert", 10.25, "Standard")
            });
        }

        private static AccSaberDataTypes.AccSaberRankedMap RankedMap(string hash, string difficulty, double complexity, string category)
        {
            return new AccSaberDataTypes.AccSaberRankedMap
            {
                songHash = hash,
                difficulty = difficulty,
                complexity = complexity,
                categoryDisplayName = category
            };
        }

        private static AccSaberDataTypes.AccSaberScores Score(string hash, string difficulty, double ap, double weightedAp, string category, int day)
        {
            return new AccSaberDataTypes.AccSaberScores
            {
                songHash = hash,
                difficulty = difficulty,
                ap = ap,
                weightedAp = weightedAp,
                categoryDisplayName = category,
                timeSet = new DateTime(2024, 1, day)
            };
        }
    }
}
