using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.Interface;

namespace PPPredictor.Core.UnitTest.MockService
{
    internal class MockBeatLeader : IBeatLeaderAPI
    {
        public Task<BeatLeaderDataTypes.BeatLeaderEventList> GetEvents()
        {
            throw new NotImplementedException();
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayer> GetPlayer(long userId, long leaderboardContextId)
        {
            throw new NotImplementedException();
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayerScoreList> GetPlayerScores(string userId, string sortBy, string order, int page, int count, long leaderboardContextId, long? eventId = null)
        {
            throw new NotImplementedException();
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayerList> GetPlayersInEventLeaderboard(long eventId, string sortBy, int page, int? count, string order)
        {
            throw new NotImplementedException();
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayerList> GetPlayersInLeaderboard(string sortBy, int page, int? count, string order, long leaderboardContextId)
        {
            throw new NotImplementedException();
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayList> GetPlayList(long playListId)
        {
            throw new NotImplementedException();
        }

        public Task<BeatLeaderDataTypes.BeatLeaderSong> GetSongByHash(string hash)
        {
            throw new NotImplementedException();
        }
    }
}
