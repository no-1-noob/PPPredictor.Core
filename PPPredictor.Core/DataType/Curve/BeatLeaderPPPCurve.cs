// Portions adapted from BeatLeader/beatleader-server (rebalanced-pp).
// Copyright (c) 2022 BeatLeader
// Licensed under the MIT License; see LICENSES/BeatLeader.txt in the Core repository.
// Source: https://github.com/BeatLeader/beatleader-server/tree/52a50edfd172c37bab4c5d278b10aae47ccbc8f6
// Adapted for PPPredictor.Core.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core.DataType.Curve
{
    class BeatLeaderPPPCurve : IPPPCurve
    {
        private static readonly ConcurrentDictionary<CurveInputs, List<(double, double)>> Cache = new ConcurrentDictionary<CurveInputs, List<(double, double)>>();
        private const float StarAccuracy = 0.96f;
        private const int MaxCached = 50000;
        
        private readonly List<(double, double)> accPointList = new List<(double, double)> {
                (1.0, 7.424),
                (0.999, 6.241),
                (0.9975, 5.158),
                (0.995, 4.010),
                (0.9925, 3.241),
                (0.99, 2.700),
                (0.9875, 2.303),
                (0.985, 2.007),
                (0.9825, 1.786),
                (0.98, 1.618),
                (0.9775, 1.490),
                (0.975, 1.392),
                (0.9725, 1.315),
                (0.97, 1.256),
                (0.965, 1.167),
                (0.96, 1.094),
                (0.955, 1.039),
                (0.95, 1.000),
                (0.94, 0.931),
                (0.93, 0.867),
                (0.92, 0.813),
                (0.91, 0.768),
                (0.9, 0.729),
                (0.875, 0.650),
                (0.85, 0.581),
                (0.825, 0.522),
                (0.8, 0.473),
                (0.75, 0.404),
                (0.7, 0.345),
                (0.65, 0.296),
                (0.6, 0.256),
                (0.0, 0.000), };
        public static readonly (double x, double y)[] BaseTable =
        {
            (1.0, 7.424),
            (0.999, 6.241),
            (0.9975, 5.158),
            (0.995, 4.010),
            (0.9925, 3.241),
            (0.99, 2.700),
            (0.9875, 2.303),
            (0.985, 2.007),
            (0.9825, 1.786),
            (0.98, 1.618),
            (0.9775, 1.490),
            (0.975, 1.392),
            (0.9725, 1.315),
            (0.97, 1.256),
            (0.965, 1.167),
            (0.96, 1.094),
            (0.955, 1.039),
            (0.95, 1.000),
            (0.94, 0.931),
            (0.93, 0.867),
            (0.92, 0.813),
            (0.91, 0.781),
            (0.9, 0.753),
            (0.875, 0.699),
            (0.85, 0.649),
            (0.825, 0.605),
            (0.8, 0.568),
            (0.75, 0.485),
            (0.7, 0.414),
            (0.65, 0.355),
            (0.6, 0.307),
            (0.0, 0.000)
        };
        public bool IsDummy { get => false; }
        public double CalculatePPatPercentage(PPPBeatMapInfo beatMapInfo, double percentage, bool failed, bool paused, LeaderboardContext leaderboardContext = LeaderboardContext.None)
        {
            try
            {
                if (beatMapInfo.ModifiedStarRating.IsRanked())
                {
                    percentage /= 100.0;
                    
                    //Multipliers are calculated in PPCalculatorBeatLeader.cs, if 0 then the modifiers are invalid for this context (or unranked).
                    switch (leaderboardContext)
                    {
                        case LeaderboardContext.BeatLeaderDefault:
                            return PpFromScore(percentage, leaderboardContext, beatMapInfo.Modifiers, beatMapInfo, GetCurveInputs(beatMapInfo), false).Item1;
                        case LeaderboardContext.BeatLeaderNoPauses:
                            if (beatMapInfo.ModifiedStarRating.Multiplier == 0) return 0; 
                            return PpFromScore(percentage, leaderboardContext, beatMapInfo.Modifiers, beatMapInfo, GetCurveInputs(beatMapInfo), false).Item1;
                        case LeaderboardContext.BeatLeaderGolf:
                            if (percentage > 0.5f) return 0;
                            if (beatMapInfo.ModifiedStarRating.Multiplier == 0) return 0;
                            return PpFromScore(1-percentage, leaderboardContext, beatMapInfo.Modifiers, beatMapInfo, GetCurveInputs(beatMapInfo), false).Item1;
                        case LeaderboardContext.BeatLeaderNoModifiers:
                            if (beatMapInfo.ModifiedStarRating.Multiplier == 0) return 0;
                            return PpFromScore(percentage, leaderboardContext, beatMapInfo.Modifiers, beatMapInfo, GetCurveInputs(beatMapInfo), false).Item1;
                        case LeaderboardContext.BeatLeaderSCPM:
                            if (beatMapInfo.ModifiedStarRating.Multiplier == 0) return 0;
                            return PpFromScore(percentage, leaderboardContext, beatMapInfo.Modifiers, beatMapInfo, GetCurveInputs(beatMapInfo), false).Item1;
                        default:
                            return 0;
                    }
                    
                    // if (leaderboardContext == LeaderboardContext.BeatLeaderGolf)
                    // {
                    //     if (percentage > 0.5f) return 0;
                    //     percentage = 1f - percentage;
                    // }
                    // if (!failed && !(leaderboardContext == LeaderboardContext.BeatLeaderNoPauses && paused))
                    // {
                    //     var (passPP, accPP, techPP) = CalculatePP(percentage, beatMapInfo.ModifiedStarRating.AccRating * beatMapInfo.ModifiedStarRating.Multiplier, beatMapInfo.ModifiedStarRating.PassRating * beatMapInfo.ModifiedStarRating.Multiplier, beatMapInfo.ModifiedStarRating.TechRating * beatMapInfo.ModifiedStarRating.Multiplier, leaderboardContext);
                    //     var rawPP = Inflate(passPP + accPP + techPP);
                    //     return rawPP;
                    // }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logging.ErrorPrint($"BeatLeaderPPPCurve CalculatePPatPercentage Error: {ex.Message}");
                return -1;
            }
        }
        
        #region new
        public static (double, double, double, double, double) PpFromScore(
            double accuracy, 
            LeaderboardContext context,
            string modifiers, 
            // ModifiersMap modifierValues, 
            // ModifiersRating? modifiersRating,
            PPPBeatMapInfo beatMapInfo,
            CurveInputs curve,
            bool timing)
        {
            if (accuracy <= 0 || accuracy > 1) return (0, 0, 0, 0, 0);
        
            if (timing) {
                double timingPP = accuracy * beatMapInfo.BaseStarRating.PassRating * 55f;
                if (double.IsInfinity(timingPP) || double.IsNaN(timingPP)) {
                    timingPP = 0;
                }
                return (timingPP, 0, 0, 0, 0);
            }
        
            // float mp = modifierValues.GetTotalMultiplier(modifiers, modifiersRating == null, context);
            double mp = beatMapInfo.ModifiedStarRating.Multiplier;
        
            var ratings = new MapRatings(beatMapInfo.BaseStarRating.AccRating, beatMapInfo.BaseStarRating.PassRating, beatMapInfo.BaseStarRating.TechRating, GetCurve(curve));
            MapRatings speedRatings = null;
            foreach (var modifier in modifiers.ToLower().Split(';'))
            {
                if (beatMapInfo.BaseStarRating.ModifiersRating?.ContainsKey(modifier+"AccRating") ?? false)
                {
                    speedRatings = new MapRatings(
                        beatMapInfo.ModifiedStarRating.AccRating,
                        beatMapInfo.ModifiedStarRating.PassRating,
                        beatMapInfo.ModifiedStarRating.TechRating,
                        GetCurve(new CurveInputs(beatMapInfo.ModifiedStarRating.PredictedAcc, beatMapInfo.ModifiedStarRating.SliderPercentage, curve.NoteCount, curve.NoteSpan / beatMapInfo.ModifiedStarRating.NoteSpanMult)));
                    break;
                }
            }
            return ScorePp(accuracy, modifiers, mp, ratings, speedRatings, context == LeaderboardContext.BeatLeaderGolf);
        }
        
        public static (double, double, double, double, double) ScorePp(
            double accuracy,
            string modifiers,
            double mp,
            MapRatings ratings,
            MapRatings speedRatings = null,
            bool golf = false)
        {
            if (accuracy <= 0 || accuracy > 1) return (0, 0, 0, 0, 0);

            double rawPP = 0; double fullPP = 0; double passPP = 0; double accPP = 0; double techPP = 0; double increase = 0;
            if (!modifiers.Contains("NF"))
            {
                var formula = PpFormula.Current;
                var prepared = Prepare(ratings);
                var map = prepared.Ratings;
                double stars = prepared.Stars;
                (passPP, accPP, techPP) = GetPp(accuracy, map.AccRating, map.PassRating, map.TechRating, map.Curve, formula.CurvePowerAt(stars), prepared.StarCurveValue, golf);
                rawPP = Scale(passPP + accPP + techPP) * formula.LowStarMultiplier(stars);

                var speedPrepared = speedRatings is MapRatings speed ? Prepare(speed) : prepared;
                var score = speedPrepared.Ratings;
                double scoreStars = speedPrepared.Stars;
                (passPP, accPP, techPP) = GetPp(accuracy, score.AccRating * mp, score.PassRating * mp, score.TechRating * mp, score.Curve, formula.CurvePowerAt(scoreStars), speedPrepared.StarCurveValue, golf);
                fullPP = Scale(passPP + accPP + techPP) * formula.LowStarMultiplier(scoreStars);
                if (passPP + accPP + techPP > 0) {
                    increase = fullPP / (passPP + accPP + techPP);
                }
            }

            if (double.IsInfinity(rawPP) || double.IsNaN(rawPP) || double.IsNegativeInfinity(rawPP)) {
                rawPP = 0;
            }

            if (double.IsInfinity(fullPP) || double.IsNaN(fullPP) || double.IsNegativeInfinity(fullPP)) {
                fullPP = 0;
            }

            return (fullPP, fullPP - rawPP, passPP * increase, accPP * increase, techPP * increase);
        }
        
        public static List<(double, double)> GetCurve(CurveInputs inputs)
        {
            if (Cache.TryGetValue(inputs, out var cached)) return cached;

            var result = GetCurve(inputs.PredictedAcc, inputs.SliderPercentage, inputs.NoteCount, inputs.NoteSpan)
                // .Select(p => new Vector2((float)p.x, (float)p.y))
                .ToList();

            if (Cache.Count >= MaxCached) Cache.Clear();
            Cache[inputs] = result;
            return result;
        }
        
        public static List<(double x, double y)> GetCurve(double predictedAcc, double sliderPercentage, int noteCount, double noteSpan)
        {
            var curves = PerMapCurves(predictedAcc);
            double tilt = SliderTilt(sliderPercentage, predictedAcc);
            var settings = CurveSettings.Current;
            double nerf = settings.LowNoteMultiplier(noteCount, noteSpan);
            if (curves == null && tilt == 0)
            {
                return GetCurve(noteCount, noteSpan);
            }
            double a = curves?.modified.A ?? 1.0;

            var xs = BaseTable.Select(p => p.x)
                .Concat(Enumerable.Range(0, 71).Select(i => 0.6 + i * 0.005))
                .Concat(Enumerable.Range(0, 101).Select(i => 0.95 + i * 0.0005))
                .Append(a)
                .Select(x => Math.Round(x, 6))
                .Where(x => x >= 0 && x <= 1)
                .Distinct()
                .OrderByDescending(x => x);

            double Raw(double x)
            {
                double value = curves == null ? BaseValue(x) : PerMapValue(curves.Value, x);
                if (tilt != 0)
                {
                    double distance = MyClamp(x - predictedAcc, -settings.SliderTiltRange, settings.SliderTiltRange);
                    value *= Math.Exp(tilt * distance * 100);
                }
                return value;
            }
            return xs.Select(x => (x, Raw(x) * nerf)).ToList();
        }
        
        public static List<(double x, double y)> GetCurve(int noteCount, double noteSpan)
        {
            double nerf = CurveSettings.Current.LowNoteMultiplier(noteCount, noteSpan);
            return BaseTable.Select(p => (p.x, p.y * nerf)).ToList();
        }
        
        private static double SliderTilt(double sliderPercentage, double predictedAcc)
        {
            var settings = CurveSettings.Current;
            if (!settings.Enabled || settings.SliderTilt == 0 || predictedAcc <= 0.6 || predictedAcc >= 0.9995) return 0;
            return settings.SliderTilt * sliderPercentage;
        }
        
        public static double Scale(double pp) => PpFormula.Current.Scale(pp);
        
        private static (ModifiableCurve modified, ModifiableCurve standard)? PerMapCurves(double predictedAcc)
        {
            var settings = CurveSettings.Current;
            if (!settings.Enabled || predictedAcc <= 0.6 || predictedAcc >= 0.9995)
            {
                return null;
            }
            var row = settings.ParamsAt(predictedAcc);
            if (row.IsDefault)
            {
                return null;
            }
            double a = Math.Min(Math.Max(row.A, 0.951), 1.0);
            return (new ModifiableCurve { A = a, C = row.C, K = row.K, W = row.W, K2 = row.K2, P2 = row.P2 }, new ModifiableCurve());
        }
        
        public static double PerMapValue(double predictedAcc, double acc)
        {
            var curves = PerMapCurves(predictedAcc);
            return curves == null ? BaseValue(acc) : PerMapValue(curves.Value, acc);
        }
        
        private static double PerMapValue((ModifiableCurve modified, ModifiableCurve standard) curves, double x)
        {
            double baseValue = SmoothBaseValue(x);
            double reference = curves.standard.GetValue(x);
            double value = reference > 0 ? baseValue * curves.modified.GetValue(x) / reference : baseValue;
            return Math.Min(baseValue, value);
        }
        
        public static double SmoothBaseValue(double acc)
        {
            var (xs, ys, slopes) = Smooth.Value;
            if (acc <= xs[0]) return ys[0];
            if (acc >= xs[xs.Length - 1]) return ys[ys.Length - 1];
            int i = 0;
            while (xs[i + 1] < acc) i++;
            double h = xs[i + 1] - xs[i];
            double t = (acc - xs[i]) / h;
            double t2 = t * t, t3 = t2 * t;
            return (2 * t3 - 3 * t2 + 1) * ys[i] + (t3 - 2 * t2 + t) * h * slopes[i]
                + (-2 * t3 + 3 * t2) * ys[i + 1] + (t3 - t2) * h * slopes[i + 1];
        }
        
        private static readonly Lazy<(double[] xs, double[] ys, double[] slopes)> Smooth =
            new Lazy<(double[] xs, double[] ys, double[] slopes)>(() =>
            {
                var points = BaseTable.OrderBy(p => p.x).ToArray();
                int n = points.Length;
                double[] xs = points.Select(p => p.x).ToArray(), ys = points.Select(p => p.y).ToArray();
                double[] h = new double[n - 1], d = new double[n - 1], slopes = new double[n];
                for (int i = 0; i < n - 1; i++)
                {
                    h[i] = xs[i + 1] - xs[i];
                    d[i] = (ys[i + 1] - ys[i]) / h[i];
                }
                slopes[0] = d[0];
                slopes[n - 1] = d[n - 2];
                for (int i = 1; i < n - 1; i++)
                {
                    if (d[i - 1] * d[i] <= 0) continue;
                    double w1 = 2 * h[i] + h[i - 1], w2 = h[i] + 2 * h[i - 1];
                    slopes[i] = (w1 + w2) / (w1 / d[i - 1] + w2 / d[i]);
                }
                return (xs, ys, slopes);
            });

        public static double BaseValue(double acc)
        {
            int i = 0;
            for (; i < BaseTable.Length; i++)
            {
                if (BaseTable[i].x <= acc) break;
            }
            if (i == 0) i = 1;
            if (i >= BaseTable.Length) i = BaseTable.Length - 1;
            double middle_dis = (acc - BaseTable[i - 1].x) / (BaseTable[i].x - BaseTable[i - 1].x);
            return BaseTable[i - 1].y + middle_dis * (BaseTable[i].y - BaseTable[i - 1].y);
        }
        
        public class CurveSettings
        {
            public bool Enabled { get; set; } = true;

            public List<CurveRow> Table { get; set; } = new List<CurveRow>()
            {
                new CurveRow
                {
                    PredictedAcc = 0.955,
                    A = 1.0,
                    C = -5.30,
                    K = 4.86,
                    W = 0.60,
                    K2 = 2.88,
                    P2 = 2.46
                },
                new CurveRow
                {
                    PredictedAcc = 0.960,
                    A = 0.99,
                    C = -3.00,
                    K = 4.30,
                    W = 0.66,
                    K2 = 4.80,
                    P2 = 1.85
                },
                new CurveRow
                {
                    PredictedAcc = 0.965,
                    A = 0.988,
                    C = -2.40,
                    K = 4.50,
                    W = 0.64,
                    K2 = 4.50,
                    P2 = 1.95
                },
                new CurveRow
                {
                    PredictedAcc = 0.970,
                    A = 0.986,
                    C = -2.20,
                    K = 4.70,
                    W = 0.62,
                    K2 = 4.20,
                    P2 = 2.05
                },
                new CurveRow
                {
                    PredictedAcc = 0.975,
                    A = 0.985,
                    C = -1.80,
                    K = 4.86,
                    W = 0.60,
                    K2 = 3.80,
                    P2 = 2.15
                },
                new CurveRow
                {
                    PredictedAcc = 0.980,
                    A = 0.985,
                    C = -1.40,
                    K = 4.86,
                    W = 0.60,
                    K2 = 3.35,
                    P2 = 2.30
                },
                new CurveRow
                {
                    PredictedAcc = 0.985,
                    A = 0.985,
                    C = -1.20,
                    K = 4.86,
                    W = 0.60,
                    K2 = 2.88,
                    P2 = 2.46
                },
                new CurveRow
                {
                    PredictedAcc = 0.990,
                    A = 0.9915,
                    C = -1.20,
                    K = 4.86,
                    W = 0.60,
                    K2 = 2.88,
                    P2 = 2.46
                },
                new CurveRow
                {
                    PredictedAcc = 0.995,
                    A = 0.9915,
                    C = -1.20,
                    K = 4.86,
                    W = 0.60,
                    K2 = 2.88,
                    P2 = 2.46
                },
            };

            public double MaxC { get; set; } = 1;

            public double SliderTilt { get; set; } = 0.2;
            public double SliderTiltRange { get; set; } = 0.015;

            public double LowNoteStart { get; set; } = 115;
            public double LowNoteRefNotes { get; set; } = 20;
            public double LowNoteRefMultiplier { get; set; } = 0.95;
            public double LowNoteMinMultiplier { get; set; } = 0.95;

            public double ShortStart { get; set; } = 75;
            public double ShortRefSpan { get; set; } = 12;
            public double ShortRefMultiplier { get; set; } = 0.9;
            public double ShortMinMultiplier { get; set; } = 0.9;

            public double LowNoteMultiplier(int notes, double span = 0) => LowNoteFactor(notes) * ShortFactor(span);

            public double LowNoteFactor(int notes)
            {
                if (LowNoteStart <= 0 || notes <= 0 || notes >= LowNoteStart || LowNoteRefNotes >= LowNoteStart) return 1;
                double nerf = (1 - LowNoteRefMultiplier) * Math.Log(LowNoteStart / notes) / Math.Log(LowNoteStart / LowNoteRefNotes);
                return MyClamp(1 - nerf, LowNoteMinMultiplier, 1);
            }

            public double ShortFactor(double span)
            {
                if (ShortStart <= 0 || span <= 0 || span >= ShortStart || ShortRefSpan >= ShortStart) return 1;
                double nerf = (1 - ShortRefMultiplier) * Math.Log(ShortStart / span) / Math.Log(ShortStart / ShortRefSpan);
                return MyClamp(1 - nerf, ShortMinMultiplier, 1);
            }

            public CurveRow ParamsAt(double predictedAcc)
            {
                var rows = Table.OrderBy(r => r.PredictedAcc).ToList();
                if (rows.Count == 0)
                    return new CurveRow
                    {
                        PredictedAcc = predictedAcc
                    };

                CurveRow Clamp(CurveRow r) => new CurveRow
                {
                    PredictedAcc = predictedAcc,
                    A = r.A,
                    C = Math.Min(r.C, MaxC),
                    K = r.K,
                    W = r.W,
                    K2 = r.K2,
                    P2 = r.P2
                };

                if (predictedAcc <= rows[0].PredictedAcc) return Clamp(rows[0]);
                for (int i = 1; i < rows.Count; i++)
                {
                    if (predictedAcc <= rows[i].PredictedAcc)
                    {
                        var p = rows[i - 1];
                        var n = rows[i];
                        double t = (predictedAcc - p.PredictedAcc) / (n.PredictedAcc - p.PredictedAcc);
                        double L(double x, double y) => x + t * (y - x);
                        return Clamp(new CurveRow
                        {
                            A = L(p.A, n.A),
                            C = L(p.C, n.C),
                            K = L(p.K, n.K),
                            W = L(p.W, n.W),
                            K2 = L(p.K2, n.K2),
                            P2 = L(p.P2, n.P2)
                        });
                    }
                }
                return Clamp(rows[rows.Count - 1]);
            }

            public static CurveSettings Current { get; set; } = new CurveSettings();
        }
        
        public class ModifiableCurve
        {
            public double Y0 { get; set; } = 0.256;
            public double K { get; set; } = 4.86;
            public double W { get; set; } = 0.6;
            public double K2 { get; set; } = 2.88;
            public double P2 { get; set; } = 2.46;
            public double C { get; set; } = 1.0;
            public double A { get; set; } = 1.0;

            public double GetValue(double x)
            {
                if (x <= 0.60) return (Y0 / 0.60) * x;
                if (x <= 0.95) return GetF1(x);
                if (x <= A) return GetF2(x);
                return GetL(x);
            }

            public double GetM()
            {
                double term1 = (1.0 - Y0) / 0.35;
                double term2 = (1.0 - W) + W * (K * Math.Exp(K)) / (Math.Exp(K) - 1.0);
                return term1 * term2;
            }

            public double GetF1(double x)
            {
                double u = (x - 0.6) / 0.35;
                double blend = (1.0 - W) * u + W * (Math.Exp(K * u) - 1.0) / (Math.Exp(K) - 1.0);
                return Y0 + (1.0 - Y0) * blend;
            }

            public double GetF2(double x)
            {
                double m = GetM();
                double u2 = (x - 0.95) / 0.05;
                double expTerm = (Math.Exp(K2 * Math.Pow(u2, P2)) - 1.0) / (Math.Exp(K2) - 1.0);
                return 1.0 + m * (x - 0.95) + (6.424 - 0.05 * m) * expTerm;
            }

            public double GetL(double x)
            {
                double s = GetS();
                double s1 = s * (1.0 - C);
                double f2A = GetF2(A);

                if (Math.Abs(A - 1.0) < 1e-9)
                {
                    return f2A + s1 * (x - A);
                }

                double expTerm = 1.0 - Math.Exp(-(x - A) / (1.0 - A));
                return f2A + s1 * (x - A) + (s - s1) * (1.0 - A) * expTerm;
            }

            public double GetS()
            {
                double m = GetM();
                double uA = (A - 0.95) / 0.05;
                double uAPower = uA > 0 ? Math.Pow(uA, P2) : 0;
                double uAPowerMinus1 = uA > 0 ? Math.Pow(uA, P2 - 1.0) : 0;
                double expTerm = Math.Exp(K2 * uAPower) / (Math.Exp(K2) - 1.0);
                double derivativeFactor = (K2 * P2 / 0.05) * uAPowerMinus1;
                return m + (6.424 - 0.05 * m) * expTerm * derivativeFactor;
            }
        }

        public class CurveRow
        {
            public double PredictedAcc { get; set; }
            public double A { get; set; } = 1.0;
            public double C { get; set; } = 0;
            public double K { get; set; } = 4.86;
            public double W { get; set; } = 0.6;
            public double K2 { get; set; } = 2.88;
            public double P2 { get; set; } = 2.46;

            public bool IsDefault => A >= 0.9995 && K == 4.86 && W == 0.6 && K2 == 2.88 && P2 == 2.46;
        }
        
        public static CurveInputs GetCurveInputs(PPPBeatMapInfo beatMapInfo) =>
            GetCurveInputs(beatMapInfo.BaseStarRating.PredictedAcc, beatMapInfo.BaseStarRating.SliderPercentage, beatMapInfo.BaseStarRating.Notes, beatMapInfo.BaseStarRating.NoteSpan);
        
        public static CurveInputs GetCurveInputs(double? predictedAcc, double? sliderPercentage, int? noteCount, double? noteSpan) =>
            new CurveInputs(predictedAcc ?? 0, sliderPercentage ?? 0, noteCount ?? 0, noteSpan ?? 0);

        public class PpFormula
        {
            public double PassScale { get; set; } = 12.3;
            public double PassExponent { get; set; } = 2.62;
            public double PassOffset { get; set; } = 30;
            public double PassInner { get; set; } = 0.85966;
            public double AccWeight { get; set; } = 21.584;
            public double AccRatingPower { get; set; } = 1.2163;
            public double AccCurvePower { get; set; } = 0.9;
            public double LowStarCurvePower { get; set; } = 1.0;
            public double LowStarCurveStart { get; set; } = 6.7242;
            public double LowStarCurveEnd { get; set; } = 9.606;
            public double VeryLowStarCurvePower { get; set; } = 1.08;
            public double VeryLowStarCurveStart { get; set; } = 2.8818;
            public double VeryLowStarCurveEnd { get; set; } = 4.803;
            public double TechWeight { get; set; } = 0.1612;
            public double TechRatingPower { get; set; } = 1.2477;
            public double TechAccExponent { get; set; } = 3.5;
            public double StarDivisor { get; set; } = 83.6925;
            public double StarScale { get; set; } = 1.1653;
            public double StarExponent { get; set; } = 1.17507;
            public double PpMultiplier { get; set; } = 1.0230;

            public double LowStarBonus { get; set; } = 0.0063371;
            public double LowStarBonusEnd { get; set; } = 8.6454;
            public double LowStarBonusPower { get; set; } = 1.36;

            public static PpFormula Current { get; set; } = new PpFormula();

            public double PassTopKnee { get; set; } = 13;
            public double PassTopSlope { get; set; } = 0.5;

            public double TechTopKnee { get; set; } = 12;
            public double TechTopSlope { get; set; } = 0.33;

            public PpFormula Clone() => (PpFormula)MemberwiseClone();
            
            public double Scale(double pp) => PpMultiplier * pp;
            
            public double StarsFromPp(double ppAt96)
            {
                double stars = Stars(ppAt96);
                if (!(ppAt96 > 0) || LowStarBonus <= 0 || stars >= LowStarBonusEnd) return stars;

                double lo = stars, hi = LowStarBonusEnd;
                while (hi - lo > 1e-6)
                {
                    double mid = (lo + hi) / 2;
                    if (mid < Stars(ppAt96 * LowStarMultiplier((float)mid))) lo = mid; else hi = mid;
                }
                return (float)((lo + hi) / 2);
            }
            
            public double Stars(double ppAt96)
            {
                double basis = ppAt96 / StarDivisor;
                return basis > 0 ? (float)(StarScale * Math.Pow(basis, StarExponent)) : (float)basis;
            }
            
            public double LowStarMultiplier(double stars)
            {
                if (LowStarBonus <= 0 || double.IsNaN(stars) || stars >= LowStarBonusEnd) return 1;
                return 1 + (float)(LowStarBonus * Math.Pow(LowStarBonusEnd - Math.Max(stars, 0), LowStarBonusPower));
            }
            
            public double CurvePowerAt(double stars)
            {
                if (VeryLowStarCurvePower != LowStarCurvePower && stars < VeryLowStarCurveEnd)
                {
                    if (stars <= VeryLowStarCurveStart) return VeryLowStarCurvePower;
                    return VeryLowStarCurvePower + (LowStarCurvePower - VeryLowStarCurvePower) * (stars - VeryLowStarCurveStart) / (VeryLowStarCurveEnd - VeryLowStarCurveStart);
                }
                if (LowStarCurvePower == AccCurvePower || !(stars < LowStarCurveEnd)) return AccCurvePower;
                if (stars <= LowStarCurveStart) return LowStarCurvePower;
                return LowStarCurvePower + (AccCurvePower - LowStarCurvePower) * (stars - LowStarCurveStart) / (LowStarCurveEnd - LowStarCurveStart);
            }
            
            public double AccCurve(double curveValue, double curveAt96, double power)
            {
                if (power == 1 || !(curveValue > 0) || !(curveAt96 > 0)) return curveValue;
                return curveAt96 * Math.Pow(curveValue / curveAt96, (float)power);
            }
            
            public double AccPp(double curveValue, double accRating) =>
                accRating > 0 ? curveValue * (double)AccWeight * Math.Pow(accRating, (float)AccRatingPower) : 0;

            public double TechPp(double accuracy, double techRating)
            {
                if (!(techRating > 0)) return 0;
                double tech = techRating;
                if (tech > TechTopKnee) tech = (float)(TechTopKnee + (tech - TechTopKnee) * TechTopSlope);
                return Math.Exp((float)TechAccExponent * accuracy) * (float)TechWeight * Math.Pow(tech, (float)TechRatingPower);
            }
            
            public double PassPp(double passRating)
            {
                double pass = passRating;
                if (PassTopSlope != 1 && pass > PassTopKnee) pass = (float)(PassTopKnee + (pass - PassTopKnee) * PassTopSlope);
                double passPP = PassScale * Math.Exp(PassInner * Math.Pow(pass, 1 / PassExponent)) - PassOffset;
                if (double.IsInfinity(passPP) || double.IsNaN(passPP) || passPP < 0) passPP = 0;
                return passPP;
            }
        }

        public static double MyClamp(double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public class MapRatings
        {
            public double AccRating;
            public double PassRating;
            public double TechRating;
            public List<(double, double)> Curve { get; } = null;

            public MapRatings(double accRating, double passRating, double techRating, List<(double, double)> curve)
            {
                AccRating = accRating;
                PassRating = passRating;
                TechRating = techRating;
                Curve = curve;
            }
        }

        public class PreparedRatings
        {
            public MapRatings Ratings;
            public double Stars;
            public double StarCurveValue;
            
            public PreparedRatings(MapRatings ratings, double stars, double starCurveValue)
            {
                Ratings = ratings;
                Stars = stars;
                StarCurveValue = starCurveValue;
            }
        }

        public static PreparedRatings Prepare(MapRatings ratings)
        {
            return new PreparedRatings(ratings, ToStars(ratings), CurveValue(StarAccuracy, ratings.Curve));   
        }
        
        public static double ToStars(MapRatings ratings) => ToStars(ratings.AccRating, ratings.PassRating, ratings.TechRating, ratings.Curve);
        
        public static double ToStars(double accRating, double passRating, double techRating, List<(double, double)> curve = null)
        {
            var formula = PpFormula.Current;
            (double passPP, double accPP, double techPP) = GetPp(StarAccuracy, accRating, passRating, techRating, curve);
            return formula.StarsFromPp(passPP + accPP + techPP);
        }
        
        public static double CurveValue(double acc, List<(double, double)> curve)
        {
            if (curve == null || curve.Count < 2)
            {
                return CurveValue(acc);
            }

            int i = 0, hi = curve.Count;
            while (i < hi)
            {
                int mid = (i + hi) >> 1;
                if (curve[mid].Item1 <= acc) hi = mid;
                else i = mid + 1;
            }

            if (i == 0) {
                i = 1;
            }
            if (i >= curve.Count) {
                i = curve.Count - 1;
            }

            double middle_dis = (acc - curve[i - 1].Item1) / (double)(curve[i].Item1 - curve[i - 1].Item1);
            return (float)(curve[i - 1].Item2 + middle_dis * (curve[i].Item2 - curve[i - 1].Item2));
        }
        
        public static (double pass, double acc, double tech) GetPp(double accuracy, double accRating, double passRating, double techRating, List<(double, double)> curve = null, float? stars = null)
        {
            var formula = PpFormula.Current;
            double power = stars is float s ? formula.CurvePowerAt(s) : formula.AccCurvePower;
            return GetPp(accuracy, accRating, passRating, techRating, curve, power, power != 1 ? CurveValue(StarAccuracy, curve) : 0, false);
        }
        
        private static (double pass, double acc, double tech) GetPp(double accuracy, double accRating, double passRating, double techRating, List<(double, double)> curve, double power, double starCurveValue, bool golf)
        {
            var formula = PpFormula.Current;
            double accPP;
            if (golf) {
                accPP = accuracy * accRating * 42D;
            } else {
                double curveValue = CurveValue(accuracy, curve);
                if (power != 1) curveValue = formula.AccCurve(curveValue, starCurveValue, power);
                accPP = formula.AccPp(curveValue, accRating);
            }
            return (formula.PassPp(passRating), accPP, formula.TechPp(accuracy, techRating));
        }
        
        public static double CurveValue(double acc) => BaseValue(acc);
            
            

        public class CurveInputs
        {
            public double PredictedAcc;
            public double SliderPercentage;
            public int NoteCount;
            public double NoteSpan;

            public CurveInputs(double predictedAcc, double sliderPercentage, int noteCount, double noteSpan)
            {
                PredictedAcc = predictedAcc;
                SliderPercentage = sliderPercentage;
                NoteCount = noteCount;
                NoteSpan = noteSpan;
            }
        }
        #endregion

        private (double, double, double) CalculatePP(double accuracy, double accRating, double passRating, double techRating, LeaderboardContext leaderboardContext = LeaderboardContext.None)
        {
            double passPP = 15.2f * Math.Exp(Math.Pow(passRating, 1 / 2.62f)) - 30f;
            if (double.IsInfinity(passPP) || double.IsNaN(passPP) || double.IsNegativeInfinity(passPP) || passPP < 0)
            {
                passPP = 0;
            }
            double accPP = leaderboardContext == LeaderboardContext.BeatLeaderGolf ? accuracy * accRating * 42f : AccCurve(accuracy) * accRating * 34f;
            double techPP = Math.Exp(1.9f * accuracy) * 1.08f * techRating;

            return (passPP, accPP, techPP);
        }

        private double AccCurve(double acc)
        {
            int i = 0;
            for (; i < accPointList.Count; i++)
            {
                if (accPointList[i].Item1 <= acc)
                {
                    break;
                }
            }

            if (i == 0)
            {
                i = 1;
            }

            double middle_dis = (acc - accPointList[i - 1].Item1) / (accPointList[i].Item1 - accPointList[i - 1].Item1);
            return (accPointList[i - 1].Item2 + middle_dis * (accPointList[i].Item2 - accPointList[i - 1].Item2));
        }

        private double Inflate(double pp)
        {
            return (650f * Math.Pow(pp, 1.3f)) / Math.Pow(650f, 1.3f);
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
