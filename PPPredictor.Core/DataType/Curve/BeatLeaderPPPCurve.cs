using System;
using System.Collections.Generic;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.DataType.Curve
{
    class BeatLeaderPPPCurve : IPPPCurve
    {
        private List<(double, double)> arrPPCurve = new List<(double, double)>();
        
        public bool IsDummy { get => false; }
        public double CalculatePPatPercentage(PPPBeatMapInfo beatMapInfo, double percentage, bool failed, bool paused, LeaderboardContext leaderboardContext = LeaderboardContext.None)
        {
            try
            {
                if (beatMapInfo.ModifiedStarRating.IsRanked())
                {
                    percentage /= 100.0;
                    arrPPCurve = beatMapInfo.ModifiedStarRating.DctCurves[beatMapInfo.Modifiers]?.ArrPPCurve;
                    
                    //Multipliers are calculated in PPCalculatorBeatLeader.cs, if 0, then the modifiers are invalid for this context (or unranked, or failed).
                    switch (leaderboardContext)
                    {
                        case LeaderboardContext.BeatLeaderDefault:
                            if (beatMapInfo.ModifiedStarRating.Multiplier == 0) return 0;
                            return CalculateMultiplierAtPercentage(percentage);
                        case LeaderboardContext.BeatLeaderNoPauses:
                            if (beatMapInfo.ModifiedStarRating.Multiplier == 0) return 0; 
                            return CalculateMultiplierAtPercentage(percentage);
                        case LeaderboardContext.BeatLeaderGolf:
                            if (percentage > 0.5f) return 0;
                            if (beatMapInfo.ModifiedStarRating.Multiplier == 0) return 0;
                            return CalculateMultiplierAtPercentage(percentage);
                        case LeaderboardContext.BeatLeaderNoModifiers:
                            if (beatMapInfo.ModifiedStarRating.Multiplier == 0) return 0;
                            return CalculateMultiplierAtPercentage(percentage);
                        case LeaderboardContext.BeatLeaderSCPM:
                            if (beatMapInfo.ModifiedStarRating.Multiplier == 0) return 0;
                            return CalculateMultiplierAtPercentage(percentage);
                        default:
                            return 0;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"BeatLeaderPPPCurve CalculatePPatPercentage Error: {ex.Message}");
                return -1;
            }
        }
        
        
        private double CalculateMultiplierAtPercentage(double percentage)
        {
            try
            {
                for (int i = 0; i < arrPPCurve.Count; i++)
                {
                    if (arrPPCurve[i].Item1 == percentage)
                    {
                        return arrPPCurve[i].Item2;
                    }
                    else
                    {
                        if (arrPPCurve[i + 1].Item1 < percentage)
                        {
                            return CalculateMultiplierAtPercentageWithLine((arrPPCurve[i + 1].Item1, arrPPCurve[i + 1].Item2), (arrPPCurve[i].Item1, arrPPCurve[i].Item2), percentage);
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"CustomPPPCurve CalculateMultiplierAtPercentage Error: {ex.Message}");
                return -1;
            }
        }

        private double CalculateMultiplierAtPercentageWithLine((double x, double y) p1, (double x, double y) p2, double percentage)
        {
            try
            {
                double m = (p2.y - p1.y) / (p2.x - p1.x);
                double b = p1.y - (m * p1.x);
                return m * percentage + b;
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"CustomPPPCurve CalculateMultiplierAtPercentageWithLine Error: {ex.Message}");
                return -1;
            }
        }

        public double CalculateMaxPP(PPPBeatMapInfo beatMapInfo, LeaderboardContext leaderboardContext = LeaderboardContext.None)
        {
            return CalculatePPatPercentage(beatMapInfo, 100, false, false, leaderboardContext);
        }

        public CurveInfo ToCurveInfo()
        {
            CurveInfo retCurve = new CurveInfo
            {
                CurveType = CurveType.BeatLeader
            };
            return retCurve;
        }
    }
}
