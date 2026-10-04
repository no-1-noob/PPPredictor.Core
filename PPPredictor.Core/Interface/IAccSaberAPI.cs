using System.Collections.Generic;
using System.Threading.Tasks;
using static PPPredictor.Core.DataType.LeaderBoard.AccSaberDataTypes;

namespace PPPredictor.Core.Interface
{
    internal interface IAccSaberAPI
    {
        Task<List<AccSaberCurve>> GetAccSaberCurves();
        Task<List<AccSaberMapPool>> GetAccSaberMapPools();
        Task<List<AccSaberMap>> GetRankedMaps(string mapPoolId);
        Task<List<AccSaberPlayer>> GetPlayerListForMapPool(double page, string mapPoolId);
        Task<AccSaberUser> GetAccSaberUser(long userId);
        Task<AccSaberScorePage> GetRecentScores(string userId, string poolId, int page, int pageSize);
    }
}
