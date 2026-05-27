using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.MapPool;
using PPPredictor.Core.DataType.Score;
using PPPredictor.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PPPredictor.Core.DataType.Enums;
using static PPPredictor.Core.DataType.LeaderBoard.AccSaberReloadedDataTypes;

namespace PPPredictor.Core.Calculator
{
    class PPCalculatorAccSaberReloaded<ASRAPI> : PPCalculator where ASRAPI : IAccSaberReloadedAPI, new()
    {
        private readonly ASRAPI accSaberReloadedApi;
        private readonly string unsetPoolId = "-1";

        public PPCalculatorAccSaberReloaded(Dictionary<string, PPPMapPool> dctMapPool, Settings settings) : base(dctMapPool, settings, Leaderboard.AccSaberReloaded)
        {
            accSaberReloadedApi = new ASRAPI();
        }
        
        internal override async Task<PPPPlayer> GetPlayerInfo(long userId, PPPMapPool mapPool)
        {
            try
            {
                if (mapPool.Id == unsetPoolId) return new PPPPlayer(true);
                AccSaberReloadedUser player = await accSaberReloadedApi.GetAccSaberUser(userId);
                if (player == null)
                {
                    Logging.ErrorPrint($"PPCalculatorAccSaberReloaded GetPlayerInfo PlayerNotFound: {userId}");
                    return new PPPPlayer(true);
                }
                AccSaberReloadedUserCategoryStatistics stats = player.statistics?.FirstOrDefault(x => x.categoryId == mapPool.Id);
                if (stats == null)
                {
                    Logging.ErrorPrint($"PPCalculatorAccSaberReloaded GetPlayerInfo No Stats for Player {userId} in pool {mapPool.Id}");
                    return new PPPPlayer(true);
                }
                return new PPPPlayer(player, stats);
            }
            catch (Exception ex)
            {
                mapPool.IsPlayerFound = false;
                Logging.ErrorPrint($"PPCalculatorAccSaberReloaded GetPlayerInfo Error: {ex.Message}");
                return new PPPPlayer(true);
            }
        }
        internal override async Task<PPPScoreCollection> GetRecentScores(string userId, int pageSize, int page, PPPMapPool mapPool)
        {
            try
            {
                if (mapPool.Id == unsetPoolId) return new PPPScoreCollection();
                AccSaberReloadedScorePage accSaberReloadedScorePage = await accSaberReloadedApi.GetRecentScores(userId, mapPool.Id, page, pageSize);
                return new PPPScoreCollection(accSaberReloadedScorePage);
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorAccSaberReloaded GetAllScores Error: {ex.Message}");
                return new PPPScoreCollection();
            }
        }
        
        internal override Task<PPPScoreCollection> GetAllScores(string userId, PPPMapPool mapPool)
        {
            return Task.FromResult(new PPPScoreCollection());
        }
        internal override async Task<List<PPPPlayer>> GetPlayers(double fetchIndexPage, PPPMapPool mapPool)
        {
            try
            {
                List<PPPPlayer> lsPlayer = new List<PPPPlayer>();
                if (mapPool.Id == unsetPoolId) return lsPlayer;
                List<AccSaberReloadedPlayer> lsAccPlayer = await accSaberReloadedApi.GetPlayerListForMapPool(fetchIndexPage, mapPool.Id);
                foreach (AccSaberReloadedPlayer accPlayer in lsAccPlayer)
                {
                    lsPlayer.Add(new PPPPlayer(accPlayer));
                }
                return lsPlayer;
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorAccSaberReloaded GetPlayers Error: {ex.Message}");
                return new List<PPPPlayer>();
            }
        }
        internal override Task<PPPBeatMapInfo> GetBeatMapInfoAsync(PPPBeatMapInfo beatMapInfo, PPPMapPool mapPool)
        {
            try
            {
                if (!string.IsNullOrEmpty(beatMapInfo.CustomLevelHash))
                {
                    string searchString = CreateSeachString(beatMapInfo.CustomLevelHash, beatMapInfo.BeatmapKey);
                    var cachedInfo = mapPool.LsLeaderboadInfo.FirstOrDefault(x => x.Searchstring == searchString);
                    //Search in all pools if mapPool is default
                    if (mapPool.MapPoolType == MapPoolType.Default)
                    {
                        foreach (var keyValuePair in _dctMapPool)
                        {
                            if (keyValuePair.Value.LsLeaderboadInfo.Exists(x => x.Searchstring == searchString))
                            {
                                cachedInfo = keyValuePair.Value.LsLeaderboadInfo.FirstOrDefault(x => x.Searchstring == searchString);
                                break;
                            }
                        }
                    }
                    if(cachedInfo != null)
                    {
                        return Task.FromResult(new PPPBeatMapInfo(beatMapInfo, new PPPStarRating(cachedInfo.StarRating.Stars)));
                    }
                    return Task.FromResult(new PPPBeatMapInfo(beatMapInfo, new PPPStarRating(0)));
                }
                return Task.FromResult(beatMapInfo);
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorAccSaberReloaded GetBeatMapInfoAsync Error: {ex.Message}");
                return Task.FromResult(new PPPBeatMapInfo(beatMapInfo, new PPPStarRating(-1)));
            }
        }
        internal override PPPBeatMapInfo ApplyModifiersToBeatmapInfo(PPPBeatMapInfo beatMapInfo, PPPMapPool mapPool, DataType.BeatSaberEncapsulation.GameplayModifiers gameplayModifiers, bool levelFailed = false, bool levelPaused = false)
        {
            beatMapInfo.ModifiedStarRating = new PPPStarRating(beatMapInfo.BaseStarRating.Stars);
            return beatMapInfo;
        }
        public override string CreateSeachString(string hash, DataType.BeatSaberEncapsulation.BeatmapKey beatmapKey)
        {
            if(hash == null) return string.Empty;
            if(beatmapKey == null) return hash.ToUpper();
            return $"{hash}_SOLO{beatmapKey.serializedName}_{ParsingUtil.ParseDifficultyNameToInt(beatmapKey.difficulty.ToString())}".ToUpper();
        }

        internal override async Task InternalUpdateMapPoolDetails(PPPMapPool mapPool)
        {
            if (!IsPlayerFound(mapPool)) return;
            try
            {
                if (mapPool.Id == unsetPoolId) return;
                mapPool.LsLeaderboadInfo.Clear();
                List<AccSaberReloadedMap> rankedMaps = await this.accSaberReloadedApi.GetRankedMaps(mapPool.Id);

                List<AccSaberReloadedRankedMap> rankedSongs = FlattenRankedMaps(rankedMaps, mapPool.Id);
                foreach (AccSaberReloadedRankedMap song in rankedSongs)
                {
                    mapPool.LsLeaderboadInfo.Add(new ShortScore(CreateSeachString(song.songHash, "SoloStandard", (int)ParsingUtil.ParseDifficultyNameToInt(song.difficulty)), new PPPStarRating(song.complexity), DateTime.Now, song.categoryId));
                }
                await GetPlayerScores(mapPool, 10, _leaderboardInfo.LargePageSize, false);
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorAccSaberReloaded UpdateMapPoolDetails Error: {ex.Message}");
            }
        }
        public override async Task UpdateAvailableMapPools()
        {
            try
            {
                List<AccSaberReloadedCurve> lsCurves = await accSaberReloadedApi.GetAccSaberCurves();
                
                var defaultMapPool = new PPPMapPool(MapPoolType.Default, $"☞ Select a map pool ☜", new PPPWeightingInfo(0), 0, CurveParser.ParseToCurve(new CurveInfo(CurveType.AccSaberReloaded)), 0);
                if (!_dctMapPool.ContainsKey(defaultMapPool.Id)) _dctMapPool.Add(defaultMapPool.Id, defaultMapPool);

                List<AccSaberReloadedMapPool> lsMapPool = await accSaberReloadedApi.GetAccSaberMapPools();
                //check if this map pool is already in list
                foreach (AccSaberReloadedMapPool newMapPool in lsMapPool)
                {
                    AccSaberReloadedCurve curvePoints =  lsCurves.FirstOrDefault(x => x.id == newMapPool.scoreCurve.id);
                    if (curvePoints == null && newMapPool.countForOverall)
                    {
                        Logging.ErrorPrint($"PPCalculatorAccSaberReloaded UpdateAvailableMapPools Error: Curve {newMapPool.scoreCurve.id} not found");
                        continue;
                    }
                    List<(double, double)> lsCurvePoints = curvePoints?.GetPointsAsTuples() ?? new List<(double, double)>();
                    lsCurvePoints.Reverse();
                    IPPPCurve newCurve = CurveParser.ParseToCurve(new CurveInfo(CurveType.AccSaberReloaded, lsCurvePoints, newMapPool.scoreCurve.scale, -newMapPool.scoreCurve.shift));
                    PPPWeightingInfo weightInfo = new PPPWeightingInfo(newMapPool.weightCurve.xParameterValue, newMapPool.weightCurve.yParameterValue, newMapPool.weightCurve.zParameterValue);
                    if (_dctMapPool.TryGetValue(newMapPool.id, out PPPMapPool mapPool))
                    {
                        mapPool.Curve = newCurve;
                        mapPool.WeightingInfo = weightInfo;
                    }
                    else
                    {
                        int sortindex = Array.IndexOf(new object[4] { "overall", "standard_acc", "true_acc", "tech_acc" }, newMapPool.code) + 1;
                        MapPoolType mapPoolType = newMapPool.countForOverall ? MapPoolType.Custom : MapPoolType.Default;
                        string description = newMapPool.code == "overall" ? "Overall" : newMapPool.description;
                        mapPool = new PPPMapPool(newMapPool.id, newMapPool.code, mapPoolType, description, weightInfo, sortindex, newCurve, string.Empty, syncUrl: $"https://api.accsaberreloaded.com/v1/playlists/{newMapPool.code}");
                        if (!_dctMapPool.ContainsKey(mapPool.Id)) _dctMapPool.Add(mapPool.Id, mapPool);
                    }
                    await InternalUpdateMapPoolDetails(mapPool);
                    await UpdatePlayer(mapPool, true);
                }
                SendMapPoolRefreshed();
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"PPCalculatorAccSaberReloaded UpdateAvailableMapPools Error: {ex.Message}");
            }
        }
        
        internal override PPGainResult GetPlayerScorePPGain(string mapSearchString, double pp, PPPMapPool mapPool)
        {
            if(mapPool.MapPoolType != MapPoolType.Default)
            {
                return GetPlayerScorePPGainInternal(mapPool.LsScores, mapSearchString, pp, mapPool.CurrentPlayer.Pp, mapPool);
            }
            else
            {
                var mapPoolEntry = GetMapPoolWithBeatMapInfo(mapSearchString);
                if (mapPoolEntry != null)
                {
                    var rankedMapInfo = mapPoolEntry.LsLeaderboadInfo.FirstOrDefault(x => x.Searchstring == mapSearchString);
                    if (rankedMapInfo != null)
                    {
                        PPGainResult ppGain = GetPlayerScorePPGainInternal(mapPoolEntry.LsScores, mapSearchString, pp, mapPoolEntry.TotalWeightedSum, mapPoolEntry);
                        double otherSum = 0;
                        foreach (var kvpPool in _dctMapPool)
                        {
                            if (kvpPool.Value.MapPoolType != MapPoolType.Default && kvpPool.Value.Id != rankedMapInfo.Category)
                            {
                                otherSum += kvpPool.Value.TotalWeightedSum;
                            }
                        }
                        return new PPGainResult(ppGain.PpTotal + otherSum, ppGain.PpGainWeighted, ppGain.PpGainRaw, _settings.PpGainCalculationType);
                    }
                    return new PPGainResult(mapPoolEntry.CurrentPlayer.Pp, pp, pp, _settings.PpGainCalculationType);
                }
                return new PPGainResult(mapPool.CurrentPlayer.Pp, pp, pp, _settings.PpGainCalculationType);
            }
        }
        
        internal override bool IsScoreSetOnCurrentMapPool(PPPMapPool mapPool, PPPScoreSetData score)
        {
            if (mapPool.MapPoolType != MapPoolType.Default)
            {
                return mapPool.LsLeaderboadInfo.Exists(x => x.Searchstring.Contains(score.hash.ToUpper()));
            }
            foreach (var keyValuePair in _dctMapPool)
            {
                if (keyValuePair.Value.LsLeaderboadInfo.Exists(x => x.Searchstring.Contains(score.hash.ToUpper())))
                {
                    return true;
                }
            }
            return false;
        }
        
        internal override List<PPPMapPoolShort> GetMapPools()
        {
            return _dctMapPool.Values.OrderBy(x => x.SortIndex).Select(x => (PPPMapPoolShort)x).ToList();
        }

        private static List<AccSaberReloadedRankedMap> FlattenRankedMaps(List<AccSaberReloadedMap> rankedMaps, string mapPoolId)
        {
            List<AccSaberReloadedRankedMap> flattenedMaps = new List<AccSaberReloadedRankedMap>();
            foreach (AccSaberReloadedMap rankedMap in rankedMaps)
            {
                if (rankedMap?.difficulties == null)
                {
                    continue;
                }

                foreach (AccSaberReloadedRankedMap difficulty in rankedMap.difficulties)
                {
                    if (difficulty == null)
                    {
                        continue;
                    }

                    if (difficulty.categoryId != mapPoolId)
                    {
                        continue;
                    }

                    difficulty.songHash = rankedMap.songHash;
                    flattenedMaps.Add(difficulty);
                }
            }

            return flattenedMaps;
        }
        
        protected override double CalculateWeightMulitplier(int index, PPPWeightingInfo weightingInfo)
        {
            //Weird mapping :D
            double y1 = weightingInfo.YParameter;
            double x1 = weightingInfo.ZParameter;
            double k = weightingInfo.XParameter;
            Logging.ErrorPrint($"PPCalculatorAccSaberReloaded CalculateWeightMulitplier: {y1} {x1} {k}");
            double x0 = -(Math.Log((1 - y1) / (y1 * Math.Exp(k * x1) - 1)) / k);
            return (1 + Math.Exp(-k * x0)) / (1 + Math.Exp(k * (index - 1 - x0)));
        }
        
        internal override double InternalCalculatePPatPercentage(PPPBeatMapInfo _currentBeatMapInfo, PPPMapPool mapPool, double percentage, bool failed, bool paused)
        {
            if (mapPool.MapPoolType != MapPoolType.Default)
            {
                return mapPool.Curve.CalculatePPatPercentage(_currentBeatMapInfo, percentage, failed, paused, mapPool.LeaderboardContext);    
            }
            return GetMapPoolWithBeatMapInfo(_currentBeatMapInfo)?.Curve?.CalculatePPatPercentage(_currentBeatMapInfo, percentage, failed, paused, mapPool.LeaderboardContext) ?? -2;
        }
        
        internal override double InternalCalculateMaxPP(PPPBeatMapInfo _currentBeatMapInfo, PPPMapPool mapPool)
        {
                if (mapPool.MapPoolType != MapPoolType.Default)
                {
                    return mapPool.Curve.CalculateMaxPP(_currentBeatMapInfo, mapPool.LeaderboardContext);    
                }
                return GetMapPoolWithBeatMapInfo(_currentBeatMapInfo)?.Curve?.CalculateMaxPP(_currentBeatMapInfo, mapPool.LeaderboardContext) ?? -2;
        }

        private PPPMapPool GetMapPoolWithBeatMapInfo(PPPBeatMapInfo beatMapInfo)
        {
            return GetMapPoolWithBeatMapInfo(CreateSeachString(beatMapInfo?.CustomLevelHash, beatMapInfo?.BeatmapKey));
        }
        private PPPMapPool GetMapPoolWithBeatMapInfo(string searchString)
        {
            if(string.IsNullOrEmpty(searchString)) return null;
            if(_dctMapPool == null) return null;
            foreach (var keyValuePair in _dctMapPool)
            {
                if(keyValuePair.Value.LsLeaderboadInfo == null) continue;
                if (keyValuePair.Value.LsLeaderboadInfo.Exists(x => x.Searchstring.Contains(searchString.ToUpper())))
                {
                    return keyValuePair.Value;
                }
            }
            return null;
        }
    }
}
