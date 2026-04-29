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
        private Dictionary<string, List<ShortScore>> dctScores = new Dictionary<string, List<ShortScore>>();
        private Dictionary<string, double> dctScoresSum = new Dictionary<string, double>();

        public PPCalculatorAccSaberReloaded(Dictionary<string, PPPMapPool> dctMapPool, Settings settings) : base(dctMapPool, settings, Leaderboard.AccSaberReloaded)
        {
            accSaberReloadedApi = new ASRAPI();
        }
        
        internal override async Task<PPPPlayer> GetPlayerInfo(long userId, PPPMapPool mapPool)
        {
            try
            {
                if (mapPool.Id == unsetPoolId) return new PPPPlayer(true);
                AccSaberReloadedPlayer player = await accSaberReloadedApi.GetAccSaberUserByPool(userId, mapPool.Id);
                return new PPPPlayer(player);
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
        internal override PPPBeatMapInfo ApplyModifiersToBeatmapInfo(PPPBeatMapInfo beatMapInfo, PPPMapPool mapPool, GameplayModifiers gameplayModifiers, bool levelFailed = false, bool levelPaused = false)
        {
            beatMapInfo.ModifiedStarRating = new PPPStarRating(beatMapInfo.BaseStarRating.Stars);
            return beatMapInfo;
        }
        public override string CreateSeachString(string hash, BeatmapKey beatmapKey)
        {
            return $"{hash}_SOLO{beatmapKey.serializedName}_{ParsingUtil.ParseDifficultyNameToInt(beatmapKey.difficulty.ToString())}".ToUpper();
        }

        internal override async Task InternalUpdateMapPoolDetails(PPPMapPool mapPool)
        {
            if (!IsPlayerFound(mapPool)) return;
            try
            {
                if (mapPool.Id == unsetPoolId) return;
                mapPool.LsMapPoolEntries.Clear();
                List<AccSaberReloadedMap> rankedMaps = await this.accSaberReloadedApi.GetRankedMaps(mapPool.Id);

                List<AccSaberReloadedRankedMap> rankedSongs = FlattenRankedMaps(rankedMaps, mapPool.Id);
                foreach (AccSaberReloadedRankedMap song in rankedSongs)
                {
                    mapPool.LsLeaderboadInfo.Add(new ShortScore(CreateSeachString(song.songHash, "SoloStandard", (int)ParsingUtil.ParseDifficultyNameToInt(song.difficulty)), new PPPStarRating(song.complexity), DateTime.Now, song.categoryId));
                }
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

                List<AccSaberReloadedMapPool> mapPool = await accSaberReloadedApi.GetAccSaberMapPools();
                //check if this map pool is already in list
                foreach (AccSaberReloadedMapPool newMapPool in mapPool)
                {
                    AccSaberReloadedCurve curvePoints =  lsCurves.FirstOrDefault(x => x.id == newMapPool.scoreCurve.id);
                    if (curvePoints == null && newMapPool.countForOverall)
                    {
                        Logging.ErrorPrint($"PPCalculatorAccSaberReloaded UpdateAvailableMapPools Error: Curve {newMapPool.scoreCurve.id} not found");
                        continue;
                    }
                    IPPPCurve newCurve = CurveParser.ParseToCurve(new CurveInfo(CurveType.AccSaberReloaded, curvePoints.GetPointsAsTuples(), newMapPool.scoreCurve.scale, newMapPool.scoreCurve.shift));
                    PPPWeightingInfo weightInfo = new PPPWeightingInfo(newMapPool.weightCurve.xparameterValue, newMapPool.weightCurve.yparameterValue, newMapPool.weightCurve.zparameterValue);
                    if (_dctMapPool.TryGetValue(newMapPool.id, out PPPMapPool oldPool))
                    {
                        oldPool.Curve = newCurve;
                    }
                    else
                    {
                        int sortindex = Array.IndexOf(new object[4] { "overall", "standard_acc", "true_acc", "tech_acc" }, newMapPool.code) + 1;
                        MapPoolType mapPoolType = newMapPool.countForOverall ? MapPoolType.Custom : MapPoolType.Default;
                        oldPool = new PPPMapPool(newMapPool.id, newMapPool.code, mapPoolType, newMapPool.description, weightInfo, sortindex, newCurve, string.Empty, syncUrl: $"https://api.accsaberreloaded.com/v1/playlists/{newMapPool.code}");
                        if (!_dctMapPool.ContainsKey(oldPool.Id)) _dctMapPool.Add(oldPool.Id, oldPool);
                    }
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
                var rankedMapInfo = mapPool.LsLeaderboadInfo.FirstOrDefault(x => x.Searchstring == mapSearchString);
                if (rankedMapInfo != null)
                {
                    PPGainResult ppGain = GetPlayerScorePPGainInternal(dctScores[rankedMapInfo.Category], mapSearchString, pp, dctScoresSum[rankedMapInfo.Category], mapPool);
                    double otherSum = dctScoresSum.Where(kvp => kvp.Key != rankedMapInfo.Category).Sum(kvp => kvp.Value);
                    return new PPGainResult(ppGain.PpTotal + otherSum, ppGain.PpGainWeighted, ppGain.PpGainRaw, _settings.PpGainCalculationType);
                }
                return new PPGainResult(mapPool.CurrentPlayer.Pp, pp, pp, _settings.PpGainCalculationType);
            }
        }
        
        internal override bool IsScoreSetOnCurrentMapPool(PPPMapPool mapPool, PPPScoreSetData score)
        {
            return mapPool.LsLeaderboadInfo.Exists(x => x.Searchstring.Contains(score.hash.ToUpper()));
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
            double x0 = -(Math.Log((1 - y1) / (y1 * Math.Exp(k * x1) - 1)) / k);
            return (1 + Math.Exp(-k * x0)) / (1 + Math.Exp(k * (index - 1 - x0)));
        }
    }
}
