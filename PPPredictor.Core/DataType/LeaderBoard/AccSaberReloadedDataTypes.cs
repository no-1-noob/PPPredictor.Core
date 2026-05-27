using System;
using System.Collections.Generic;

namespace PPPredictor.Core.DataType.LeaderBoard
{
    internal class AccSaberReloadedDataTypes
    {
        internal class AccSaberReloadedUser
        {
            public string id { get; set; }
            public string name { get; set; }
            public string avatarUrl { get; set; }
            public string country { get; set; }
            public double totalXp { get; set; }
            public double totalScoreXp { get; set; }
            public double totalMilestoneXp { get; set; }
            public double totalMilestoneSetBonusXp { get; set; }
            public int xpRanking { get; set; }
            public int xpCountryRanking { get; set; }
            public int level { get; set; }
            public string levelTitle { get; set; }
            public bool banned { get; set; }
            public bool playerInactive { get; set; }
            public string hmd { get; set; }
            public DateTimeOffset lastActiveTime { get; set; }
            public DateTimeOffset createdAt { get; set; }
            public List<AccSaberReloadedUserCategoryStatistics> statistics { get; set; } = new List<AccSaberReloadedUserCategoryStatistics>();
        }

        internal class AccSaberReloadedUserCategoryStatistics
        {
            public string id { get; set; }
            public string userId { get; set; }
            public string categoryId { get; set; }
            public int ranking { get; set; }
            public int countryRanking { get; set; }
            public double ap { get; set; }
            public double scoreXp { get; set; }
            public double averageAcc { get; set; }
            public double averageAp { get; set; }
            public int rankedPlays { get; set; }
            public string topPlayId { get; set; }
            public DateTimeOffset createdAt { get; set; }
        }

        internal class AccSaberReloadedPlayer
        {
            public string id { get; set; }
            public string userId { get; set; }
            public string categoryId { get; set; }
            public int ranking { get; set; }
            public int countryRanking { get; set; }
            public double ap { get; set; }
            public double scoreXp { get; set; }
            public double averageAcc { get; set; }
            public double averageAp { get; set; }
            public int rankedPlays { get; set; }
            public string topPlayId { get; set; }
            public DateTimeOffset createdAt { get; set; }
            public string country { get; set; }
        }

        internal class AccSaberReloadedScore
        {
            public string id { get; set; }
            public string userId { get; set; }
            public string userName { get; set; }
            public string avatarUrl { get; set; }
            public string country { get; set; }
            public string mapDifficultyId { get; set; }
            public string mapId { get; set; }
            public string songHash { get; set; }
            public string songName { get; set; }
            public string songAuthor { get; set; }
            public string mapAuthor { get; set; }
            public string coverUrl { get; set; }
            public string difficulty { get; set; }
            public string categoryId { get; set; }
            public int score { get; set; }
            public int scoreNoMods { get; set; }
            public double accuracy { get; set; }
            public int rank { get; set; }
            public int rankWhenSet { get; set; }
            public double ap { get; set; }
            public double weightedAp { get; set; }
            public long blScoreId { get; set; }
            public int maxCombo { get; set; }
            public int badCuts { get; set; }
            public int misses { get; set; }
            public int wallHits { get; set; }
            public int bombHits { get; set; }
            public int pauses { get; set; }
            public int streak115 { get; set; }
            public int playCount { get; set; }
            public string hmd { get; set; }
            public DateTimeOffset timeSet { get; set; }
            public bool reweightDerivative { get; set; }
            public double xpGained { get; set; }
            public double baseXp { get; set; }
            public double bonusXp { get; set; }
            public bool active { get; set; }
            public List<string> modifierIds { get; set; } = new List<string>();
            public DateTimeOffset createdAt { get; set; }
        }

        internal class AccSaberReloadedScorePage
        {
            public int totalPages { get; set; }
            public long totalElements { get; set; }
            public int size { get; set; }
            public List<AccSaberReloadedScore> content { get; set; } = new List<AccSaberReloadedScore>();
            public int number { get; set; }
            public AccSaberReloadedPageable pageable { get; set; } = new AccSaberReloadedPageable();
            public AccSaberReloadedSort sort { get; set; } = new AccSaberReloadedSort();
            public int numberOfElements { get; set; }
            public bool first { get; set; }
            public bool last { get; set; }
            public bool empty { get; set; }
        }

        internal class AccSaberReloadedPageable
        {
            public long offset { get; set; }
            public bool unpaged { get; set; }
            public bool paged { get; set; }
            public int pageNumber { get; set; }
            public int pageSize { get; set; }
            public AccSaberReloadedSort sort { get; set; } = new AccSaberReloadedSort();
        }

        internal class AccSaberReloadedSort
        {
            public bool empty { get; set; }
            public bool unsorted { get; set; }
            public bool sorted { get; set; }
        }

        internal class AccSaberReloadedMapPool
        {
            public string code { get; set; }
            public bool countForOverall { get; set; }
            public string description { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public AccSaberReloadedCurve scoreCurve { get; set; } = new AccSaberReloadedCurve();
            public AccSaberReloadedCurve weightCurve { get; set; } = new AccSaberReloadedCurve();
        }

        internal class AccSaberReloadedCurve
        {
            public string formula { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public double scale { get; set; }
            public double shift { get; set; }
            public List<AccSaberReloadedCurvePoint> points { get; set; } = new List<AccSaberReloadedCurvePoint>();
            public string xParameterName { get; set; }
            public double xParameterValue { get; set; }
            public string yParameterName { get; set; }
            public double yParameterValue { get; set; }
            public string zParameterName { get; set; }
            public double zParameterValue { get; set; }

            internal List<(double, double)> GetPointsAsTuples()
            {
                List<(double, double)> tuplePoints = new List<(double, double)>();
                foreach (AccSaberReloadedCurvePoint point in points)
                {
                    tuplePoints.Add((point.x, point.y));
                }

                return tuplePoints;
            }
        }

        internal class AccSaberReloadedCurvePoint
        {
            public double x { get; set; }
            public double y { get; set; }
        }

        internal class AccSaberReloadedTopScoreSnapshot
        {
            public string scoreId { get; set; }
            public string userId { get; set; }
            public string userName { get; set; }
            public string avatarUrl { get; set; }
            public int score { get; set; }
            public double accuracy { get; set; }
            public double ap { get; set; }
            public DateTimeOffset timeSet { get; set; }
        }

        internal class AccSaberReloadedMapDifficultyStatistics
        {
            public string id { get; set; }
            public double maxAp { get; set; }
            public double minAp { get; set; }
            public double averageAp { get; set; }
            public int totalScores { get; set; }
            public AccSaberReloadedTopScoreSnapshot topScore { get; set; } = new AccSaberReloadedTopScoreSnapshot();
            public DateTimeOffset createdAt { get; set; }
        }

        internal class AccSaberReloadedRankedMap
        {
            public string id { get; set; }
            public string mapId { get; set; }
            public string songHash { get; set; }
            public string songName { get; set; }
            public string songSubName { get; set; }
            public string songAuthor { get; set; }
            public string mapAuthor { get; set; }
            public string coverUrl { get; set; }
            public string beatsaverCode { get; set; }
            public string categoryId { get; set; }
            public string difficulty { get; set; }
            public string characteristic { get; set; }
            public string status { get; set; }
            public string ssLeaderboardId { get; set; }
            public string blLeaderboardId { get; set; }
            public int maxScore { get; set; }
            public DateTimeOffset rankedAt { get; set; }
            public DateTimeOffset createdAt { get; set; }
            public double complexity { get; set; }
            public int rankUpvotes { get; set; }
            public int rankDownvotes { get; set; }
            public string criteriaStatus { get; set; }
            public AccSaberReloadedMapDifficultyStatistics statistics { get; set; } = new AccSaberReloadedMapDifficultyStatistics();
        }

        internal class AccSaberReloadedMap
        {
            public string id { get; set; }
            public string songName { get; set; }
            public string songSubName { get; set; }
            public string songAuthor { get; set; }
            public string songHash { get; set; }
            public string mapAuthor { get; set; }
            public string beatsaverCode { get; set; }
            public string coverUrl { get; set; }
            public List<AccSaberReloadedRankedMap> difficulties { get; set; } = new List<AccSaberReloadedRankedMap>();
            public DateTimeOffset createdAt { get; set; }
        }

        internal class AccSaberReloadedMapPage
        {
            public int totalPages { get; set; }
            public long totalElements { get; set; }
            public int size { get; set; }
            public List<AccSaberReloadedMap> content { get; set; } = new List<AccSaberReloadedMap>();
            public int number { get; set; }
            public int numberOfElements { get; set; }
            public bool first { get; set; }
            public bool last { get; set; }
            public bool empty { get; set; }
        }

        internal class AccSaberReloadedLeaderboardPage
        {
            public int totalPages { get; set; }
            public long totalElements { get; set; }
            public int size { get; set; }
            public List<AccSaberReloadedPlayer> content { get; set; } = new List<AccSaberReloadedPlayer>();
            public int number { get; set; }
            public int numberOfElements { get; set; }
            public bool first { get; set; }
            public bool last { get; set; }
            public bool empty { get; set; }
        }
    }
}
