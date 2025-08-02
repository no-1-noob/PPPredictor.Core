using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PPPredictor.Core.UnitTest.MockService
{
    internal class MockHitBloq : IHitBloqAPI
    {
        public Task<List<HitBloqDataTypes.HitBloqScores>> GetAllScores(string userId, string poolId)
        {
            throw new NotImplementedException();
        }

        public Task<HitBloqDataTypes.HitBloqMapPoolDetails> GetHitBloqMapPoolDetails(string poolIdent, int page)
        {
            throw new NotImplementedException();
        }

        public Task<List<HitBloqDataTypes.HitBloqMapPool>> GetHitBloqMapPools()
        {
            throw new NotImplementedException();
        }

        public Task<HitBloqDataTypes.HitBloqUser> GetHitBloqUserByPool(long userId, string poolIdent)
        {
            throw new NotImplementedException();
        }

        public Task<HitBloqDataTypes.HitBloqUserId> GetHitBloqUserIdByUserId(string id)
        {
            throw new NotImplementedException();
        }

        public Task<HitBloqDataTypes.HitBloqLeaderboardInfo> GetLeaderBoardInfo(string searchString)
        {
            throw new NotImplementedException();
        }

        public Task<HitBloqDataTypes.HitBloqLadder> GetPlayerListForMapPool(double page, string mapPoolId)
        {
            throw new NotImplementedException();
        }

        public Task<HitBloqDataTypes.HitBloqRankFromCr> GetPlayerRankByCr(string mapPoolId, double cr)
        {
            throw new NotImplementedException();
        }

        public Task<List<HitBloqDataTypes.HitBloqScores>> GetRecentScores(string userId, string poolId, int page)
        {
            throw new NotImplementedException();
        }
    }
}
