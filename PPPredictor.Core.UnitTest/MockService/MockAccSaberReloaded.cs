using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.Interface;
namespace PPPredictor.Core.UnitTest.MockService;

internal class MockAccSaberReloaded : IAccSaberReloadedAPI
{
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedCurve>> GetAccSaberCurves()
    {
        return Task.FromResult(new List<AccSaberReloadedDataTypes.AccSaberReloadedCurve>
        {
            new AccSaberReloadedDataTypes.AccSaberReloadedCurve
            {
                id = "score-curve",
                scale = 1,
                shift = 0,
                points = new List<AccSaberReloadedDataTypes.AccSaberReloadedCurvePoint>
                {
                    new AccSaberReloadedDataTypes.AccSaberReloadedCurvePoint { x = 0, y = 0 },
                    new AccSaberReloadedDataTypes.AccSaberReloadedCurvePoint { x = 0.9, y = 300 },
                    new AccSaberReloadedDataTypes.AccSaberReloadedCurvePoint { x = 1, y = 500 }
                }
            }
        });
    }
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedMapPool>> GetAccSaberMapPools()
    {
        return Task.FromResult(new List<AccSaberReloadedDataTypes.AccSaberReloadedMapPool>
        {
            Pool("overall", "overall", "Overall", false),
            Pool("standard_acc", "standard", "Standard", true),
            Pool("tech_acc", "tech", "Tech", true)
        });
    }
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedMap>> GetAllRankedMaps()
    {
        return Task.FromResult(new List<AccSaberReloadedDataTypes.AccSaberReloadedMap>());
    }
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedMap>> GetRankedMaps(string mapPoolId)
    {
        return Task.FromResult(new List<AccSaberReloadedDataTypes.AccSaberReloadedMap>
        {
            new AccSaberReloadedDataTypes.AccSaberReloadedMap
            {
                songHash = "ASRHASH",
                difficulties = new List<AccSaberReloadedDataTypes.AccSaberReloadedRankedMap>
                {
                    new AccSaberReloadedDataTypes.AccSaberReloadedRankedMap
                    {
                        categoryId = mapPoolId,
                        difficulty = "ExpertPlus",
                        complexity = mapPoolId == "tech" ? 11.1 : 14.2
                    },
                    new AccSaberReloadedDataTypes.AccSaberReloadedRankedMap
                    {
                        categoryId = "other",
                        difficulty = "Hard",
                        complexity = 3.5
                    }
                }
            },
            new AccSaberReloadedDataTypes.AccSaberReloadedMap { songHash = "EMPTY", difficulties = null }
        });
    }
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedPlayer>> GetPlayerListForMapPool(double page, string mapPoolId)
    {
        return Task.FromResult(Enumerable.Range(0, 50).Select(i => new AccSaberReloadedDataTypes.AccSaberReloadedPlayer
        {
            categoryId = mapPoolId,
            ranking = ((int)page - 1) * 50 + i + 1,
            countryRanking = ((int)page - 1) * 50 + i + 1,
            ap = 900 - (((int)page - 1) * 50 + i) * 4,
            country = "DE"
        }).ToList());
    }
    public Task<AccSaberReloadedDataTypes.AccSaberReloadedUser> GetAccSaberUser(long userId)
    {
        return Task.FromResult(new AccSaberReloadedDataTypes.AccSaberReloadedUser
        {
            id = userId.ToString(),
            name = "Mock Reloaded Player",
            country = "DE",
            statistics = new List<AccSaberReloadedDataTypes.AccSaberReloadedUserCategoryStatistics>
            {
                new AccSaberReloadedDataTypes.AccSaberReloadedUserCategoryStatistics
                {
                    categoryId = "standard",
                    ranking = 42,
                    countryRanking = 7,
                    ap = 720
                },
                new AccSaberReloadedDataTypes.AccSaberReloadedUserCategoryStatistics
                {
                    categoryId = "tech",
                    ranking = 84,
                    countryRanking = 12,
                    ap = 540
                }
            }
        });
    }
    public Task<AccSaberReloadedDataTypes.AccSaberReloadedPlayer> GetAccSaberUserByPool(long userId, string mapPoolId)
    {
        return Task.FromResult(new AccSaberReloadedDataTypes.AccSaberReloadedPlayer());
    }
    public Task<AccSaberReloadedDataTypes.AccSaberReloadedScorePage> GetRecentScores(string userId, string poolId, int page, int pageSize)
    {
        return Task.FromResult(new AccSaberReloadedDataTypes.AccSaberReloadedScorePage
        {
            totalElements = 2,
            pageable = new AccSaberReloadedDataTypes.AccSaberReloadedPageable
            {
                pageNumber = page,
                pageSize = pageSize
            },
            content = new List<AccSaberReloadedDataTypes.AccSaberReloadedScore>
            {
                new AccSaberReloadedDataTypes.AccSaberReloadedScore
                {
                    songHash = "ASRHASH",
                    difficulty = "ExpertPlus",
                    ap = 320,
                    weightedAp = 320,
                    categoryId = poolId,
                    timeSet = new DateTimeOffset(new DateTime(2024, 2, 1))
                },
                new AccSaberReloadedDataTypes.AccSaberReloadedScore
                {
                    songHash = "ASRHASH2",
                    difficulty = "Expert",
                    ap = 220,
                    weightedAp = 200,
                    categoryId = poolId,
                    timeSet = new DateTimeOffset(new DateTime(2024, 2, 2))
                }
            }
        });
    }

    private static AccSaberReloadedDataTypes.AccSaberReloadedMapPool Pool(string code, string id, string description, bool countForOverall)
    {
        return new AccSaberReloadedDataTypes.AccSaberReloadedMapPool
        {
            code = code,
            id = id,
            description = description,
            countForOverall = countForOverall,
            scoreCurve = new AccSaberReloadedDataTypes.AccSaberReloadedCurve { id = "score-curve" },
            weightCurve = new AccSaberReloadedDataTypes.AccSaberReloadedCurve
            {
                xParameterValue = 0.4,
                yParameterValue = 0.1,
                zParameterValue = 15
            }
        };
    }
}
