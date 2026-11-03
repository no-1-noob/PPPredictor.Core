using PPPredictor.Core.Interface;
using static PPPredictor.Core.DataType.LeaderBoard.AccSaberDataTypes;

namespace PPPredictor.Core.UnitTest.MockService;

internal class MockAccSaber : IAccSaberAPI
{
    public Task<List<AccSaberCurve>> GetAccSaberCurves()
    {
        return Task.FromResult(new List<AccSaberCurve>
        {
            new AccSaberCurve
            {
                id = "score-curve",
                points = new List<AccSaberCurvePoint>
                {
                    new AccSaberCurvePoint { x = 0, y = 0 },
                    new AccSaberCurvePoint { x = 0.9, y = 300 },
                    new AccSaberCurvePoint { x = 1, y = 500 }
                }
            }
        });
    }

    public Task<List<AccSaberMapPool>> GetAccSaberMapPools()
    {
        return Task.FromResult(new List<AccSaberMapPool>
        {
            Pool("overall", "overall", "Overall", false),
            Pool("standard_acc", "standard", "Standard", true),
            Pool("tech_acc", "tech", "Tech", true)
        });
    }

    public Task<List<AccSaberMap>> GetRankedMaps(string mapPoolId)
    {
        return Task.FromResult(new List<AccSaberMap>
        {
            new AccSaberMap
            {
                songHash = "ACCHASH",
                difficulties = new List<AccSaberRankedMap>
                {
                    new AccSaberRankedMap
                    {
                        categoryId = mapPoolId,
                        difficulty = "ExpertPlus",
                        complexity = mapPoolId == "tech" ? 11.1 : 13.5
                    },
                    new AccSaberRankedMap
                    {
                        categoryId = "other",
                        difficulty = "Hard",
                        complexity = 3.5
                    }
                }
            },
            new AccSaberMap { songHash = "EMPTY", difficulties = null! }
        });
    }

    public Task<List<AccSaberPlayer>> GetPlayerListForMapPool(double page, string mapPoolId)
    {
        return Task.FromResult(Enumerable.Range(0, 50).Select(i => new AccSaberPlayer
        {
            categoryId = mapPoolId,
            ranking = (int)page * 50 + i + 1,
            countryRanking = (int)page * 50 + i + 1,
            ap = 900 - ((int)page * 50 + i) * 4,
            country = "DE"
        }).ToList());
    }

    public Task<AccSaberUser> GetAccSaberUser(long userId)
    {
        return Task.FromResult(new AccSaberUser
        {
            id = userId.ToString(),
            name = "Mock AccSaber Player",
            country = "DE",
            statistics = new List<AccSaberUserCategoryStatistics>
            {
                new AccSaberUserCategoryStatistics { categoryId = "standard", ranking = 50, countryRanking = 7, ap = 600 },
                new AccSaberUserCategoryStatistics { categoryId = "tech", ranking = 75, countryRanking = 12, ap = 450 }
            }
        });
    }

    public Task<AccSaberScorePage> GetRecentScores(string userId, string poolId, int page, int pageSize)
    {
        List<AccSaberScore> scores = new List<AccSaberScore>
        {
            new AccSaberScore
            {
                songHash = "ACCHASH", difficulty = "ExpertPlus", ap = 300, weightedAp = 300,
                categoryId = poolId, timeSet = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new AccSaberScore
            {
                songHash = "ACCHASH2", difficulty = "Expert", ap = 240, weightedAp = 216,
                categoryId = poolId, timeSet = new DateTimeOffset(2024, 2, 2, 0, 0, 0, TimeSpan.Zero)
            }
        };
        return Task.FromResult(new AccSaberScorePage
        {
            totalElements = scores.Count,
            pageable = new AccSaberPageable { pageNumber = page, pageSize = pageSize },
            content = scores.Skip(page * pageSize).Take(pageSize).ToList()
        });
    }

    private static AccSaberMapPool Pool(string code, string id, string description, bool countForOverall)
    {
        return new AccSaberMapPool
        {
            code = code,
            id = id,
            description = description,
            countForOverall = countForOverall,
            scoreCurve = new AccSaberCurve { id = "score-curve", scale = 1 },
            weightCurve = new AccSaberCurve { xParameterValue = 0.4, yParameterValue = 0.1, zParameterValue = 15 }
        };
    }
}
