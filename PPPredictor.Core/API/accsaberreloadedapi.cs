using PPPredictor.Core.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static PPPredictor.Core.DataType.LeaderBoard.AccSaberReloadedDataTypes;

namespace PPPredictor.Core.API
{
    [ExcludeFromCodeCoverage]
    internal class AccSaberReloadedApi : IAccSaberReloadedAPI
    {
        private static readonly string baseUrl = "https://api.accsaberreloaded.com";
        private const int RankedMapsPageSize = 250;
        private const int LeaderboardPageSize = 50;
        private readonly HttpClient client;

        public AccSaberReloadedApi()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "PPPredictor");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(baseUrl);
        }

        public async Task<AccSaberReloadedScorePage> GetRecentScores(string userId, string poolId, int page, int pageSize)
        {
            return await NetworkUtil.GetDataAsync<AccSaberReloadedScorePage>(client, DataType.Enums.Leaderboard.AccSaberReloaded, "GetRecentScores", $"v1/users/{userId}/scores?categoryId={poolId}&page={page}&size={pageSize}&sort=timeSet,desc");
        }

        public async Task<AccSaberReloadedUser> GetAccSaberUser(long userId)
        {
            return await NetworkUtil.GetDataAsync<AccSaberReloadedUser>(client, DataType.Enums.Leaderboard.AccSaberReloaded, "GetAccSaberUser", $"/v1/users/{userId}?statistics=true");
        }

        public async Task<List<AccSaberReloadedCurve>> GetAccSaberCurves()
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberReloadedCurve>>(client, DataType.Enums.Leaderboard.AccSaberReloaded, "GetAccSaberCurves", "/v1/curves");
        }

        public async Task<List<AccSaberReloadedMapPool>> GetAccSaberMapPools()
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberReloadedMapPool>>(client, DataType.Enums.Leaderboard.AccSaberReloaded, "GetAccSaberMapPools", "/v1/categories");
        }

        public async Task<List<AccSaberReloadedMap>> GetAllRankedMaps()
        {
            List<AccSaberReloadedMap> maps = new List<AccSaberReloadedMap>();
            int page = 0;
            while (true)
            {
                var currentPage = await NetworkUtil.GetDataAsync<AccSaberReloadedMapPage>(client, DataType.Enums.Leaderboard.AccSaberReloaded, "GetAllRankedMapsInternal", $"/v1/maps?page={page}&size={RankedMapsPageSize}&sort=createdAt,desc");
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

        public async Task<List<AccSaberReloadedMap>> GetRankedMaps(string mapPoolId)
        {
            List<AccSaberReloadedMap> maps = new List<AccSaberReloadedMap>();
            int page = 0;
            while (true)
            {
                var currentPage = await NetworkUtil.GetDataAsync<AccSaberReloadedMapPage>(client, DataType.Enums.Leaderboard.AccSaberReloaded, "GetRankedMapsInternal", $"/v1/maps?categoryId={mapPoolId}&page={page}&size={RankedMapsPageSize}&sort=createdAt,desc");
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

        public async Task<List<AccSaberReloadedPlayer>> GetPlayerListForMapPool(double page, string mapPoolId)
        {
            var result = await NetworkUtil.GetDataAsync<AccSaberReloadedLeaderboardPage>(client, DataType.Enums.Leaderboard.AccSaberReloaded, "GetPlayerListForMapPool", $"/v1/leaderboards/{mapPoolId}?page={(int)page}&size={LeaderboardPageSize}");
            return result?.content ?? new List<AccSaberReloadedPlayer>();
        }
    }
}
