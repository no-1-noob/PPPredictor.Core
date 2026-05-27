using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.Interface;
namespace PPPredictor.Core.UnitTest.MockService;

internal class MockAccSaberReloaded : IAccSaberReloadedAPI
{
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedCurve>> GetAccSaberCurves()
    {
        return Task.FromResult(new List<AccSaberReloadedDataTypes.AccSaberReloadedCurve>());
    }
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedMapPool>> GetAccSaberMapPools()
    {
        return Task.FromResult(new List<AccSaberReloadedDataTypes.AccSaberReloadedMapPool>());
    }
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedMap>> GetAllRankedMaps()
    {
        return Task.FromResult(new List<AccSaberReloadedDataTypes.AccSaberReloadedMap>());
    }
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedMap>> GetRankedMaps(string mapPoolId)
    {
        return Task.FromResult(new List<AccSaberReloadedDataTypes.AccSaberReloadedMap>());
    }
    public Task<List<AccSaberReloadedDataTypes.AccSaberReloadedPlayer>> GetPlayerListForMapPool(double page, string mapPoolId)
    {
        return Task.FromResult(new List<AccSaberReloadedDataTypes.AccSaberReloadedPlayer>());
    }
    public Task<AccSaberReloadedDataTypes.AccSaberReloadedUser> GetAccSaberUser(long userId)
    {
        throw new NotImplementedException();
    }
    public Task<AccSaberReloadedDataTypes.AccSaberReloadedPlayer> GetAccSaberUserByPool(long userId, string mapPoolId)
    {
        return Task.FromResult(new AccSaberReloadedDataTypes.AccSaberReloadedPlayer());
    }
    public Task<AccSaberReloadedDataTypes.AccSaberReloadedScorePage> GetRecentScores(string userId, string poolId, int page, int pageSize)
    {
        return Task.FromResult(new AccSaberReloadedDataTypes.AccSaberReloadedScorePage());
    }
}
