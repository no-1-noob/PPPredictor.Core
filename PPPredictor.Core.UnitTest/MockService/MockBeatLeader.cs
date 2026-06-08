using PPPredictor.Core.DataType.LeaderBoard;
using PPPredictor.Core.Interface;

namespace PPPredictor.Core.UnitTest.MockService
{
    internal class MockBeatLeader : IBeatLeaderAPI
    {
        public Task<BeatLeaderDataTypes.BeatLeaderEventList> GetEvents()
        {
            return Task.FromResult(new BeatLeaderDataTypes.BeatLeaderEventList
            {
                data = new List<BeatLeaderDataTypes.BeatLeaderEvent>
                {
                    new BeatLeaderDataTypes.BeatLeaderEvent
                    {
                        id = 987,
                        name = "Mock Event",
                        playListId = 654,
                        image = "event.png",
                        endDate = new DateTimeOffset(DateTime.UtcNow.AddDays(10)).ToUnixTimeSeconds()
                    }
                }
            });
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayer> GetPlayer(long userId, long leaderboardContextId)
        {
            return Task.FromResult(new BeatLeaderDataTypes.BeatLeaderPlayer
            {
                id = userId,
                name = "Mock BeatLeader Player",
                country = "DE",
                rank = 150,
                countryRank = 25,
                pp = 12345
            });
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayerList> GetPlayerForEvent(string eventId, string userName)
        {
            return Task.FromResult(new BeatLeaderDataTypes.BeatLeaderPlayerList
            {
                data = new List<BeatLeaderDataTypes.BeatLeaderPlayer>
                {
                    new BeatLeaderDataTypes.BeatLeaderPlayer
                    {
                        id = 123,
                        name = userName,
                        country = "DE",
                        rank = 3,
                        countryRank = 1,
                        pp = 777
                    }
                }
            });
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayerScoreList> GetPlayerScores(string userId, string sortBy, string order, int page, int count, long leaderboardContextId, long? eventId = null)
        {
            return Task.FromResult(new BeatLeaderDataTypes.BeatLeaderPlayerScoreList
            {
                metadata = new BeatLeaderDataTypes.BeatLeaderPlayerScoreListMetaData
                {
                    page = page,
                    itemsPerPage = count,
                    total = 2
                },
                data = new List<BeatLeaderDataTypes.BeatLeaderPlayerScore>
                {
                    Score("BLHASH", 9, 450, 3, 10),
                    Score("BLUNRANKED", 7, 250, 0, 20)
                }
            });
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayerList> GetPlayersInEventLeaderboard(long eventId, string sortBy, int page, int? count, string order)
        {
            return GetPlayersInEventLeaderboard(eventId.ToString(), sortBy, page, count, order);
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayerList> GetPlayersInEventLeaderboard(string eventId, string sortBy, int page, int? count, string order)
        {
            return Task.FromResult(PlayerList(page, count ?? 50, 600));
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayerList> GetPlayersInLeaderboard(string sortBy, int page, int? count, string order, long leaderboardContextId)
        {
            return Task.FromResult(PlayerList(page, count ?? 50, 20000));
        }

        public Task<BeatLeaderDataTypes.BeatLeaderSong> GetSongByHash(string hash)
        {
            return Task.FromResult(new BeatLeaderDataTypes.BeatLeaderSong
            {
                hash = hash,
                difficulties = new List<BeatLeaderDataTypes.BeatLeaderDifficulty>
                {
                    Difficulty(9, 12.5, 3, "Standard"),
                    Difficulty(7, 8.25, 0, "Standard")
                }
            });
        }

        public Task<BeatLeaderDataTypes.BeatLeaderPlayListSongList> GetSongsInPlaylistById(long playListId)
        {
            return Task.FromResult(new BeatLeaderDataTypes.BeatLeaderPlayListSongList
            {
                data = new List<BeatLeaderDataTypes.BeatLeaderSong>
                {
                    new BeatLeaderDataTypes.BeatLeaderSong
                    {
                        hash = "BLHASH",
                        difficulties = new List<BeatLeaderDataTypes.BeatLeaderDifficulty>
                        {
                            Difficulty(9, 12.5, 6, "Standard")
                        }
                    }
                }
            });
        }

        private static BeatLeaderDataTypes.BeatLeaderPlayerScore Score(string hash, int difficulty, float pp, int status, int day)
        {
            return new BeatLeaderDataTypes.BeatLeaderPlayerScore
            {
                timeset = new DateTimeOffset(new DateTime(2024, 3, day)).ToUnixTimeSeconds().ToString(),
                pp = pp,
                leaderboard = new BeatLeaderDataTypes.BeatLeaderLeaderboard
                {
                    song = new BeatLeaderDataTypes.BeatLeaderSong { hash = hash },
                    difficulty = Difficulty(difficulty, 10, status, "Standard")
                }
            };
        }

        private static BeatLeaderDataTypes.BeatLeaderDifficulty Difficulty(int value, double stars, int status, string modeName)
        {
            return new BeatLeaderDataTypes.BeatLeaderDifficulty
            {
                value = value,
                stars = stars,
                predictedAcc = 0.96,
                passRating = stars / 2,
                accRating = stars / 3,
                techRating = stars / 4,
                status = status,
                modeName = modeName,
                modifierValues = new Dictionary<string, double>
                {
                    { "fs", 0.08 },
                    { "ss", -0.15 },
                    { "sc", 0.1 },
                    { "pm", 0.05 },
                    { "gn", 0.11 },
                    { "nf", -0.5 }
                },
                modifiersRating = new Dictionary<string, double>
                {
                    { "fsAccRating", 5.1 },
                    { "fsPassRating", 6.1 },
                    { "fsTechRating", 7.1 }
                }
            };
        }

        private static BeatLeaderDataTypes.BeatLeaderPlayerList PlayerList(int page, int count, float startPp)
        {
            return new BeatLeaderDataTypes.BeatLeaderPlayerList
            {
                data = Enumerable.Range(0, count).Select(i => new BeatLeaderDataTypes.BeatLeaderPlayer
                {
                    id = i + 1,
                    name = $"Player {i}",
                    country = i % 2 == 0 ? "DE" : "US",
                    rank = (page - 1) * count + i + 1,
                    countryRank = (page - 1) * count + i + 1,
                    pp = startPp - (((page - 1) * count + i) * 5)
                }).ToList()
            };
        }
    }
}
