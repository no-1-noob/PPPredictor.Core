using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.MapPool;
using PPPredictor.Core.DataType.Score;
using PPPredictor.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PPPredictor.Core.DataType.Enums;
using static PPPredictor.Core.DataType.LeaderBoard.BeatLeaderDataTypes;

namespace PPPredictor.Core.Calculator
{
    class PPCalculatorBeatLeader<BLAPI> : PPCalculator where BLAPI : IBeatLeaderAPI, new ()
    {
        private readonly BLAPI beatleaderapi;
        internal static float accumulationConstant = 0.965f;
        private const float accumulationConstantEvent = 0.925f;
        private string _userName = string.Empty;

        public PPCalculatorBeatLeader(Dictionary<string, PPPMapPool> dctMapPool, Settings settings) : base(dctMapPool, settings, Leaderboard.BeatLeader) 
        {
            beatleaderapi = new BLAPI();
        }

        internal override async Task<PPPPlayer> GetPlayerInfo(long userId, PPPMapPool mapPool)
        {
            try
            {
                if(mapPool != null && mapPool.MapPoolType == MapPoolType.Default)
                {
                    BeatLeaderPlayer player = await beatleaderapi.GetPlayer(userId, GetLeaderboardContextId(mapPool.LeaderboardContext));
                    return new PPPPlayer(player);
                }
                else if (mapPool != null && mapPool.MapPoolType == MapPoolType.Custom)
                {
                    if (string.IsNullOrEmpty(_userName)) {
                        BeatLeaderPlayer player = await beatleaderapi.GetPlayer(userId, GetLeaderboardContextId(mapPool.LeaderboardContext));
                        _userName = player.name;
                    }
                    BeatLeaderPlayerList playerList = await beatleaderapi.GetPlayerForEvent(mapPool.Id , _userName);
                    if(playerList != null && playerList.data?.FirstOrDefault(x => x.id == userId) is BeatLeaderPlayer eventPlayer)
                    {
                        return new PPPPlayer(eventPlayer);
                    }
                }
                return new PPPPlayer(true);
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorBeatLeader GetPlayerInfo Error: {ex.Message}");
                return new PPPPlayer(true);
            }
        }

        internal override async Task<List<PPPPlayer>> GetPlayers(double fetchIndexPage, PPPMapPool mapPool)
        {
            try
            {
                BeatLeaderPlayerList scoreSaberPlayerCollection = null;
                List<PPPPlayer> lsPlayer = new List<PPPPlayer>();
                if(mapPool != null && mapPool.MapPoolType == MapPoolType.Default)
                {
                    scoreSaberPlayerCollection = await beatleaderapi.GetPlayersInLeaderboard("pp", (int)fetchIndexPage, _leaderboardInfo.PlayerPerPages, "desc", GetLeaderboardContextId(mapPool.LeaderboardContext));
                }
                else if(mapPool != null)
                {
                    scoreSaberPlayerCollection = await beatleaderapi.GetPlayersInEventLeaderboard(mapPool.Id, "pp", (int)fetchIndexPage, _leaderboardInfo.PlayerPerPages, "desc");
                }
                foreach (var scoreSaberPlayer in scoreSaberPlayerCollection.data)
                {
                    lsPlayer.Add(new PPPPlayer(scoreSaberPlayer));
                }
                return lsPlayer;
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorBeatLeader GetPlayers Error: {ex.Message}");
                return new List<PPPPlayer>();
            }
        }

        internal override async Task<PPPScoreCollection> GetRecentScores(string userId, int pageSize, int page, PPPMapPool mapPool)
        {
            try
            {
                long? eventId = null;
                if (mapPool.MapPoolType == MapPoolType.Custom && !string.IsNullOrEmpty(mapPool.Id))
                {
                    eventId = long.Parse(mapPool.Id);
                }
                BeatLeaderPlayerScoreList beatLeaderPlayerScoreList = await beatleaderapi.GetPlayerScores(userId, "date", "desc", page, pageSize, GetLeaderboardContextId(mapPool.LeaderboardContext), eventId);
                return new PPPScoreCollection(beatLeaderPlayerScoreList);
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorBeatLeader GetRecentScores Error: {ex.Message}");
                return new PPPScoreCollection();
            }
        }

        internal override Task<PPPScoreCollection> GetAllScores(string userId, PPPMapPool mapPool)
        {
            return Task.FromResult(new PPPScoreCollection());
        }

        internal override async Task<PPPBeatMapInfo> GetBeatMapInfoAsync(PPPBeatMapInfo beatMapInfo, PPPMapPool mapPool, DataType.BeatSaberEncapsulation.GameplayModifiers gameplayModifiers)
        {
            try
            {
                if (!string.IsNullOrEmpty(beatMapInfo.CustomLevelHash))
                {
                    string searchString = CreateSeachString(beatMapInfo.CustomLevelHash, "SOLO" + beatMapInfo.BeatmapKey.serializedName, ParsingUtil.ParseDifficultyNameToInt(beatMapInfo.BeatmapKey.difficulty.ToString()));
                    if(mapPool.MapPoolType == MapPoolType.Custom && mapPool.LsLeaderboadInfo.All(x => x.Searchstring != searchString))
                    {
                        return new PPPBeatMapInfo(beatMapInfo, new PPPStarRating(0)); //Currently selected map is not contained in selected MapPool
                    }
                    ShortScore cachedInfo = mapPool.LsLeaderboadInfo?.FirstOrDefault(x => x.Searchstring == searchString && !x.IsPlaceHolder);
                    bool refetchInfo = cachedInfo != null && cachedInfo.FetchTime < DateTime.Now.AddDays(_settings.RefetchMapInfoAfterDays);
                    if (cachedInfo == null || refetchInfo)
                    {
                        if (refetchInfo) mapPool.LsLeaderboadInfo?.Remove(cachedInfo);
                        BeatLeaderSong song = await beatleaderapi.GetSongByHash(beatMapInfo.CustomLevelHash);
                        if (song != null)
                        {
                            BeatLeaderDifficulty diff = song.difficulties.FirstOrDefault(x => x.value == ParsingUtil.ParseDifficultyNameToInt(beatMapInfo.BeatmapKey.difficulty.ToString()) && x.modeName.ToUpper() == beatMapInfo.BeatmapKey.serializedName.ToUpper());
                            if (diff != null)
                            {
                                bool ret = false;
                                PPPStarRating pppStarRating = new PPPStarRating(diff);
                                if(mapPool.MapPoolType == MapPoolType.Custom)
                                {
                                    if (diff.stars.HasValue)
                                    {
                                        ret = true;
                                        pppStarRating = new PPPStarRating(diff, true);
                                    }
                                }
                                else
                                {
                                    if (diff.stars.HasValue && diff.status == (int)BeatLeaderDifficultyStatus.ranked)
                                    {
                                        ret = true;
                                    }
                                }
                                pppStarRating = await GetPPCurve(pppStarRating, gameplayModifiers, mapPool);
                                mapPool.LsLeaderboadInfo.Add(new ShortScore(searchString, pppStarRating, DateTime.Now));
                                if (ret)
                                {
                                    return new PPPBeatMapInfo(beatMapInfo, pppStarRating);
                                }
                            }
                        }
                    }
                    else
                    {
                        return new PPPBeatMapInfo(beatMapInfo, await GetPPCurve(cachedInfo.StarRating, gameplayModifiers, mapPool));
                    }
                }
                return beatMapInfo;
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorBeatLeader GetStarsForBeatmapAsync Error: {ex.Message}");
                return new PPPBeatMapInfo(beatMapInfo, new PPPStarRating(-1));
            }
        }

        private async Task<PPPStarRating> GetPPCurve(PPPStarRating pppStarRating, DataType.BeatSaberEncapsulation.GameplayModifiers gameplayModifiers, PPPMapPool mapPool)
        {
            try
            {
                if(!pppStarRating.IsRanked()) return pppStarRating;
                string modifierString = String.Join(",", ParseModifiers(gameplayModifiers).Where(x => x != "NF"));
                if (pppStarRating.DctCurves.TryGetValue(modifierString, out CurveInfo curve))
                {
                    if (curve.LastUpdated > DateTime.Now.AddDays(_settings.RefetchMapInfoAfterDays))
                    {
                        return pppStarRating;
                    }
                    pppStarRating.DctCurves.Remove(modifierString);
                }
                string leaderboardId = $"{pppStarRating.SongId}{pppStarRating.Value}{pppStarRating.Mode}";
                BeatLeaderCurve blCurve = await beatleaderapi.GetCurveByLeaderboardId(leaderboardId, modifierString, GetLeaderboardContextId(mapPool.LeaderboardContext));
                var curvePoints = blCurve.points?.Reverse().Select(p => (p[0], p[1])).ToList() ?? new List<(double, double)>();
                curve = new CurveInfo(CurveType.Linear, curvePoints, 0);
                curve.LastUpdated = DateTime.Now;
            
                pppStarRating.DctCurves.Add(modifierString, curve);
                return pppStarRating;
            }
            catch (Exception e)
            {
                Logging.ErrorPrint($"PPCalculatorBeatLeader GetPPCurve Error: {e.Message}");
                return pppStarRating;
            }
        }

        internal override async Task<PPPBeatMapInfo> ApplyModifiersToBeatmapInfo(PPPBeatMapInfo beatMapInfo, PPPMapPool mapPool, DataType.BeatSaberEncapsulation.GameplayModifiers gameplayModifiers, bool levelFailed, bool levelPaused)
        {
            try
            {
                beatMapInfo.BaseStarRating = await GetPPCurve(beatMapInfo.BaseStarRating, gameplayModifiers, mapPool);
                double accRating = beatMapInfo.BaseStarRating.AccRating;
                double passRating = beatMapInfo.BaseStarRating.PassRating;
                double techRating = beatMapInfo.BaseStarRating.TechRating;
                double predictedAcc = beatMapInfo.BaseStarRating.PredictedAcc;
                
                List<string> lsModifiers = ParseModifiers(gameplayModifiers);
                double multiplier = GenerateModifierMultiplier(lsModifiers, beatMapInfo, mapPool, levelFailed, levelPaused, beatMapInfo.BaseStarRating.ModifiersRating != null);
                beatMapInfo.Modifiers = GenerateModifierString(lsModifiers, mapPool);
                beatMapInfo.ModifiedStarRating = new PPPStarRating(multiplier, 
                    accRating, passRating, techRating, predictedAcc,
                    beatMapInfo.BaseStarRating.Mode, beatMapInfo.BaseStarRating.Value, beatMapInfo.BaseStarRating.SongId, beatMapInfo.BaseStarRating.DctCurves,
                    beatMapInfo.BaseStarRating.RankedBeatLeader);
                return beatMapInfo;
            }
            catch (Exception e)
            {
                Logging.ErrorPrint($"PPCalculatorBeatLeader ApplyModifiersToBeatmapInfo Error: {e.Message}");
                return beatMapInfo;
            }
        }

        private string GenerateModifierString(List<string> lsModifiers, PPPMapPool mapPool)
        {
            List<string> lsModifiersExculded = new List<string>();
            if (mapPool.LeaderboardContext == LeaderboardContext.BeatLeaderSCPM)
            {
                lsModifiersExculded.Add("SC");
                lsModifiersExculded.Add("PM");
            }
            lsModifiersExculded.Add("NF");
            return String.Join(",", lsModifiers.Where(x => lsModifiersExculded.All(y => y != x)));
        }

        private List<string> ParseModifiers(DataType.BeatSaberEncapsulation.GameplayModifiers gameplayModifiers)
        {
            try
            {
                List<string> lsModifiers = new List<string>();
                if (gameplayModifiers.disappearingArrows) lsModifiers.Add("DA");
                if (gameplayModifiers.songSpeed == DataType.BeatSaberEncapsulation.GameplayModifiers.SongSpeed.Faster) lsModifiers.Add("FS");
                if (gameplayModifiers.songSpeed == DataType.BeatSaberEncapsulation.GameplayModifiers.SongSpeed.Slower) lsModifiers.Add("SS");
                if (gameplayModifiers.songSpeed == DataType.BeatSaberEncapsulation.GameplayModifiers.SongSpeed.SuperFast) lsModifiers.Add("SF");
                if (gameplayModifiers.ghostNotes) lsModifiers.Add("GN");
                if (gameplayModifiers.noArrows) lsModifiers.Add("NA");
                if (gameplayModifiers.noBombs) lsModifiers.Add("NB");
                if (gameplayModifiers.noFailOn0Energy) lsModifiers.Add("NF");
                if (gameplayModifiers.enabledObstacleType == DataType.BeatSaberEncapsulation.GameplayModifiers.EnabledObstacleType.NoObstacles) lsModifiers.Add("NO");
                if (gameplayModifiers.proMode) lsModifiers.Add("PM");
                if (gameplayModifiers.smallCubes) lsModifiers.Add("SC");
                if (gameplayModifiers.instaFail) lsModifiers.Add("IF");
                if (gameplayModifiers.energyType == DataType.BeatSaberEncapsulation.GameplayModifiers.EnergyType.Battery) lsModifiers.Add("BE");
                if (gameplayModifiers.strictAngles) lsModifiers.Add("SA");
                if (gameplayModifiers.zenMode) lsModifiers.Add("ZM");
                return lsModifiers;
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorBeatLeader ParseModifiers Error: {ex.Message}");
                return new List<string>();
            }
        }

        private double GenerateModifierMultiplier(List<string> lsModifier, PPPBeatMapInfo beatMapInfo, PPPMapPool mapPool, bool levelFailed, bool levelPaused, bool ignoreSpeedMultiplier)
        {
            try
            {
                double multiplier = 1;
                if (mapPool.LeaderboardContext == LeaderboardContext.BeatLeaderSCPM && !(lsModifier.Contains("SC") && lsModifier.Contains("PM"))) return 0;
                if(mapPool.LeaderboardContext == LeaderboardContext.BeatLeaderNoPauses && levelPaused) return 0;
                if(levelFailed) return 0;
                foreach (string modifier in lsModifier)
                {
                    if (!levelFailed && modifier == "NF") continue; //Ignore nofail until the map is actually failed in gameplay and applied
                    if (mapPool.LeaderboardContext == LeaderboardContext.BeatLeaderNoModifiers && !(modifier == "BE" || modifier == "IF")) return 0;
                    if(mapPool.LeaderboardContext == LeaderboardContext.BeatLeaderSCPM && (modifier == "SC" || modifier == "PM")) continue;
                    if (ignoreSpeedMultiplier && (modifier == "SF" || modifier == "SS" || modifier == "FS")) continue; //Ignore speed multies and use the precomputed values from backend
                    if(beatMapInfo.BaseStarRating.ModifierValues.TryGetValue(modifier.ToLower(), out double value))
                    {
                        if (mapPool.LeaderboardContext == LeaderboardContext.BeatLeaderGolf && value < 0) return 0;
                        multiplier += value;
                    }
                }
                return multiplier;
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorBeatLeader GenerateModifierMultiplier Error: {ex.Message}");
                return -1;
            }
        }

        internal override async Task InternalUpdateMapPoolDetails(PPPMapPool mapPool)
        {
            if (mapPool.MapPoolType != MapPoolType.Default)
            {
                mapPool.LsLeaderboadInfo.Clear();
                BeatLeaderPlayListSongList lsPlayList = await this.beatleaderapi.GetSongsInPlaylistById(long.Parse(mapPool.PlayListId));
                foreach (BeatLeaderSong song in lsPlayList.data)
                {
                    foreach (BeatLeaderDifficulty diff in song.difficulties)
                    {
                        string searchString = CreateSeachString(song.hash, "SOLO" + diff.modeName, diff.value);
                        mapPool.LsLeaderboadInfo.Add(new ShortScore(searchString, true));
                    }
                }
            }
        }

        public override string CreateSeachString(string hash, DataType.BeatSaberEncapsulation.BeatmapKey beatmapKey)
        {
            return $"{hash}_{ParsingUtil.ParseDifficultyNameToInt(beatmapKey.difficulty.ToString())}";
        }

        private long GetLeaderboardContextId(LeaderboardContext leaderboardContext)
        {
            switch (leaderboardContext)
            {
                case LeaderboardContext.BeatLeaderDefault: return 2;
                case LeaderboardContext.BeatLeaderNoModifiers: return 4;
                case LeaderboardContext.BeatLeaderNoPauses: return 8;
                case LeaderboardContext.BeatLeaderGolf: return 16;
                case LeaderboardContext.BeatLeaderSCPM: return 32;
                default: return 0;
            }
        }

        override public async Task UpdateAvailableMapPools()
        {
            if (!_dctMapPool.ContainsKey("-1")) _dctMapPool.Add("-1", new PPPMapPool("-1", MapPoolType.Default, $"General", new PPPWeightingInfo(accumulationConstant), 0, new BeatLeaderPPPCurve(), LeaderboardContext.BeatLeaderDefault));
            if (!_dctMapPool.ContainsKey("-2")) _dctMapPool.Add("-2", new PPPMapPool("-2", MapPoolType.Default, $"No modifiers", new PPPWeightingInfo(accumulationConstant), 1, new BeatLeaderPPPCurve(), LeaderboardContext.BeatLeaderNoModifiers));
            if (!_dctMapPool.ContainsKey("-3")) _dctMapPool.Add("-3", new PPPMapPool("-3", MapPoolType.Default, $"No pauses", new PPPWeightingInfo(accumulationConstant), 2, new BeatLeaderPPPCurve(), LeaderboardContext.BeatLeaderNoPauses));
            if (!_dctMapPool.ContainsKey("-4")) _dctMapPool.Add("-4", new PPPMapPool("-4", MapPoolType.Default, $"Golf", new PPPWeightingInfo(accumulationConstant), 3, new BeatLeaderPPPCurve(), LeaderboardContext.BeatLeaderGolf));
            if (!_dctMapPool.ContainsKey("-5")) _dctMapPool.Add("-5", new PPPMapPool("-5", MapPoolType.Default, $"SCPM", new PPPWeightingInfo(accumulationConstant), 4, new BeatLeaderPPPCurve(), LeaderboardContext.BeatLeaderSCPM));

            //BeatLeaderEventList beatLeaderEventList = await beatleaderapi.GetEvents();
            BeatLeaderEventList beatLeaderEventList = new BeatLeaderEventList();
            DateTime dateCutoff = DateTime.UtcNow;
#if DEBUG
            //dateCutoff = dateCutoff.AddDays(-120);
#endif
            foreach (BeatLeaderEvent blEvent in beatLeaderEventList.data)
            {
                if (_dctMapPool.TryGetValue(blEvent.id.ToString(), out PPPMapPool mapPool))
                {
                    if (blEvent.dtEndDate < dateCutoff)
                    {
                        _dctMapPool.Remove(blEvent.id.ToString());
                    }
                }
                else
                {
                    if (blEvent.dtEndDate >= dateCutoff)
                    {
                        PPPMapPool insertMapPool = new PPPMapPool(blEvent.id.ToString(), blEvent.playListId.ToString(), MapPoolType.Custom, $"Event {blEvent.name}", new PPPWeightingInfo(accumulationConstantEvent), (int.MaxValue - blEvent.id), new BeatLeaderPPPCurve(), blEvent.image);
                        if (!_dctMapPool.ContainsKey(insertMapPool.Id)) _dctMapPool.Add(insertMapPool.Id, insertMapPool);
                    }
                }
            }
            SendMapPoolRefreshed();
            return;
        }

        internal override bool IsScoreSetOnCurrentMapPool(PPPMapPool mapPool, PPPScoreSetData score)
        {
            return (GetLeaderboardContextId(mapPool.LeaderboardContext) & score.context) > 0;
        }

        internal override List<PPPMapPoolShort> GetMapPools()
        {
            return _dctMapPool.Values.OrderBy(x => x.SortIndex).Select(x => (PPPMapPoolShort)x).ToList();
        }

        internal override string GetStarDisplay(PPPBeatMapInfo beatMapInfo)
        {
            return $"💪{FormatStarRating(beatMapInfo.ModifiedStarRating?.PassRating)} 🎯{FormatStarRating(beatMapInfo.ModifiedStarRating?.AccRating)} 🔧{FormatStarRating(beatMapInfo.ModifiedStarRating?.TechRating)}";
        }

        internal override PPPMapPoolShort FindPoolWithPlayListId(string playlistId)
        {
            return _dctMapPool.Values.FirstOrDefault(x => x.PlayListId == playlistId);
        }
    }
}
