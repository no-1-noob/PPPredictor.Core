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
    internal class AccSaberApi : IAccSaberAPI
    {
        private static readonly string baseUrl = "http://api.accsaber.com";
        private readonly HttpClient client;

        public AccSaberApi()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "PPPredictor");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(baseUrl);
        }

        public async Task<List<AccSaberScores>> GetAllScores(string userId)
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberScores>>(client, DataType.Enums.Leaderboard.AccSaber, "GetAllScores", $"/players/{userId}/scores?pageSize=9999");
        }

        public async Task<List<AccSaberScores>> GetAllScoresByPool(string userId, string poolId)
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberScores>>(client, DataType.Enums.Leaderboard.AccSaber, "GetAllScoresByPool", $"/players/{userId}/{poolId}/scores");
        }

        public async Task<AccSaberPlayer> GetAccSaberUserByPool(long userId, string poolIdent)
        {
            return await NetworkUtil.GetDataAsync<AccSaberPlayer>(client, DataType.Enums.Leaderboard.AccSaber, "GetAccSaberUserByPool", $"players/{userId}/{poolIdent}");
        }

        public async Task<List<AccSaberMapPool>> GetAccSaberMapPools()
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberMapPool>>(client, DataType.Enums.Leaderboard.AccSaber, "GetAccSaberMapPools", "categories");
        }

        public async Task<List<AccSaberRankedMap>> GetAllRankedMaps()
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberRankedMap>>(client, DataType.Enums.Leaderboard.AccSaber, "GetAllRankedMaps", $"ranked-maps");
        }

        public async Task<List<AccSaberRankedMap>> GetRankedMaps(string mapPool)
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberRankedMap>>(client, DataType.Enums.Leaderboard.AccSaber, "GetRankedMaps", $"ranked-maps/category/{mapPool}");
        }

        public async Task<List<AccSaberPlayer>> GetPlayerListForMapPool(double page, string mapPoolId)
        {
            return await NetworkUtil.GetDataAsync<List<AccSaberPlayer>>(client, DataType.Enums.Leaderboard.AccSaber, "GetPlayerListForMapPool", $"categories/{mapPoolId}/standings");
        }
    }
}
