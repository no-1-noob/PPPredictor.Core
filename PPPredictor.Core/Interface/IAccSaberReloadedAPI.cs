using System.Collections.Generic;
using System.Threading.Tasks;
using static PPPredictor.Core.DataType.LeaderBoard.AccSaberReloadedDataTypes;

namespace PPPredictor.Core.Interface
{
    internal interface IAccSaberReloadedAPI
    {
        Task<List<AccSaberReloadedCurve>> GetAccSaberCurves();
        Task<List<AccSaberReloadedMapPool>> GetAccSaberMapPools();
        Task<List<AccSaberReloadedMap>> GetRankedMaps(string mapPoolId);
        Task<List<AccSaberReloadedPlayer>> GetPlayerListForMapPool(double page, string mapPoolId);
        Task<AccSaberReloadedPlayer> GetAccSaberUserByPool(long userId, string mapPoolId);
        Task<AccSaberReloadedScorePage> GetRecentScores(string userId, string poolId, int page, int pageSize);
    }
}
