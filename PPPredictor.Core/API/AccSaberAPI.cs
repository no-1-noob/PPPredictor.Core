using PPPredictor.Core.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static PPPredictor.Core.DataType.LeaderBoard.AccSaberDataTypes;

namespace PPPredictor.Core.API
{
    [ExcludeFromCodeCoverage]
    internal class AccSaberAPI : IAccSaberAPI
    {
#if !MOCK_API
        private static readonly string baseUrl = "https://api.accsaber.com";
#else
        private static readonly string baseUrl = "http://localhost:5080";
#endif
        
        private const int RankedMapsPageSize = 250;
        private const int LeaderboardPageSize = 50;
        private readonly HttpClient client;

        public AccSaberAPI()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "PPPredictor");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(baseUrl);
        }

        public async Task<AccSaberScorePage> GetRecentScores(string userId, string poolId, int page, int pageSize)
        {
            return await NetworkUtil.GetDataAsync<AccSaberScorePage>(client, DataType.Enums.Leaderboard.AccSaber, "GetRecentScores", $"v1/users/{userId}/scores?categoryId={poolId}&page={page}&size={pageSize}&sort=timeSet,desc");
        }

        public async Task<AccSaberUser> GetAccSaberUser(long userId)
        {
            return await NetworkUtil.GetDataAsync<AccSaberUser>(client, DataType.Enums.Leaderboard.AccSaber, "GetAccSaberUser", $"/v1/users/{userId}?statistics=true");
        }

        public async Task<List<AccSaberCurve>> GetAccSaberCurves()
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberCurve>>(client, DataType.Enums.Leaderboard.AccSaber, "GetAccSaberCurves", "/v1/curves");
        }

        public async Task<List<AccSaberMapPool>> GetAccSaberMapPools()
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberMapPool>>(client, DataType.Enums.Leaderboard.AccSaber, "GetAccSaberMapPools", "/v1/categories");
        }

        public async Task<List<AccSaberMap>> GetAllRankedMaps()
        {
            List<AccSaberMap> maps = new List<AccSaberMap>();
            int page = 0;
            while (true)
            {
                var currentPage = await NetworkUtil.GetDataAsync<AccSaberMapPage>(client, DataType.Enums.Leaderboard.AccSaber, "GetAllRankedMapsInternal", $"/v1/maps?page={page}&size={RankedMapsPageSize}&sort=createdAt,desc");
                if (currentPage?.content == null || currentPage.content.Count == 0)
                {
                    break;
                }

                maps.AddRange(currentPage.content);
                if (page + 1 >= currentPage.totalPages)
                {
                    break;
                }
                page++;
            }

            return maps;
        }

        public async Task<List<AccSaberMap>> GetRankedMaps(string mapPoolId)
        {
            List<AccSaberMap> maps = new List<AccSaberMap>();
            int page = 0;
            while (true)
            {
                var currentPage = await NetworkUtil.GetDataAsync<AccSaberMapPage>(client, DataType.Enums.Leaderboard.AccSaber, "GetRankedMapsInternal", $"/v1/maps?categoryId={mapPoolId}&page={page}&size={RankedMapsPageSize}&sort=createdAt,desc");
                if (currentPage?.content == null || currentPage.content.Count == 0)
                {
                    break;
                }

                maps.AddRange(currentPage.content);
                if (page + 1 >= currentPage.totalPages)
                {
                    break;
                }
                page++;
            }

            return maps;
        }

        public async Task<List<AccSaberPlayer>> GetPlayerListForMapPool(double page, string mapPoolId)
        {
            var result = await NetworkUtil.GetDataAsync<AccSaberLeaderboardPage>(client, DataType.Enums.Leaderboard.AccSaber, "GetPlayerListForMapPool", $"/v1/leaderboards/{mapPoolId}?page={(int)page}&size={LeaderboardPageSize}");
            return result?.content ?? new List<AccSaberPlayer>();
        }
    }
}
