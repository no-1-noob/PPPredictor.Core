using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PPPredictor.Core.DataType.Curve;

namespace PPPredictor.Core.DataType
{
    public class PPPStarRating
    {
        private double _stars = 0;
        private double _multiplier = 1;
        private double _predictedAcc = 0;
        private double _passRating = 0;
        private double _accRating = 0;
        private double _techRating = 0;
        private bool? _rankedBeatLeader = null;
        private int _mode;
        private int _value;
        private string _songId;
        private Dictionary<string, double> _modifiersRating = new Dictionary<string, double>();
        private Dictionary<string, double> _modifierValues = new Dictionary<string, double>();
        private Dictionary<string, CurveInfo> _dctCurves = new Dictionary<string, CurveInfo>();
        
        [DefaultValue(null)]
        public bool? RankedBeatLeader{ get => _rankedBeatLeader; set => _rankedBeatLeader = value; }
        [DefaultValue(0d)]
        public double Stars { get => _stars; set => _stars = value; }
        [DefaultValue(0d)]
        public double PredictedAcc { get => _predictedAcc; set => _predictedAcc = value; }
        [DefaultValue(0d)]
        public double PassRating { get => _passRating; set => _passRating = value; }
        [DefaultValue(0d)]
        public double AccRating { get => _accRating; set => _accRating = value; }
        [DefaultValue(0d)]
        public double TechRating { get => _techRating; set => _techRating = value; }
        [DefaultValue(0)]
        public int Mode { get => _mode; set => _mode = value; }
        [DefaultValue(0)]
        public int Value { get => _value; set => _value = value; }
        [DefaultValue(0)]
        public string SongId { get => _songId; set => _songId = value; }
        [DefaultValue(null)]
        public Dictionary<string, double> ModifiersRating { get => _modifiersRating; set => _modifiersRating = value; } //Needed?
        [DefaultValue(null)]
        public Dictionary<string, double> ModifierValues { get => _modifierValues; set => _modifierValues = value; }
        public bool ShouldSerializeDctCurves()
        {
            return DctCurves != null && DctCurves.Count > 0;
        }
        [DefaultValue(null)]
        public Dictionary<string, CurveInfo> DctCurves { get => _dctCurves; set => _dctCurves = value; }
        [JsonIgnoreAttribute]
        [DefaultValue(1)]
        public double Multiplier { get => _multiplier; set => _multiplier = value; }
        

        internal PPPStarRating()
        {
        }

        public PPPStarRating(double starRating)
        {
            _stars = _predictedAcc = _passRating = _accRating = _techRating = starRating;
        }

        internal PPPStarRating(LeaderBoard.BeatLeaderDataTypes.BeatLeaderDifficulty beatLeaderDifficulty, bool? isEventRanked = null)
        {
            if (isEventRanked.HasValue)
            {
                _rankedBeatLeader = isEventRanked.Value;
            }
            else
            {
                _rankedBeatLeader = beatLeaderDifficulty.status == (int)Enums.BeatLeaderDifficultyStatus.ranked;
            }
            _predictedAcc = beatLeaderDifficulty.predictedAcc.GetValueOrDefault();
            _passRating = beatLeaderDifficulty.passRating.GetValueOrDefault();
            _accRating = beatLeaderDifficulty.accRating.GetValueOrDefault();
            _techRating = beatLeaderDifficulty.techRating.GetValueOrDefault();
            _mode = beatLeaderDifficulty.mode;
            _value = beatLeaderDifficulty.value;
            _songId = beatLeaderDifficulty.songId;
            _modifiersRating = beatLeaderDifficulty.modifiersRating ?? new Dictionary<string, double>();
            _modifierValues = beatLeaderDifficulty.modifierValues ?? new Dictionary<string, double>();
        }

        internal PPPStarRating(double mulitplier, double accRating, double passRating, double techRating, double predictedAcc,
            int mode, int value, string songId, Dictionary<string, CurveInfo> dctCurves, bool? rankedBeatLeader)
        {
            _rankedBeatLeader = rankedBeatLeader;
            _multiplier = mulitplier;
            _accRating = accRating;
            _passRating = passRating;
            _techRating = techRating;
            _predictedAcc = predictedAcc;
            _value = value;
            _mode = mode;
            _songId = songId;
            _dctCurves = dctCurves;
        }

        public bool IsRanked()
        {
            if(_rankedBeatLeader == false){
                return false;
            }

            return _predictedAcc > 0 || _passRating > 0 || _accRating > 0 || _techRating > 0 || _stars > 0;      
        }

        public override string ToString()
        {
            return $"PPPStarRating: Stars {Stars} Multiplier {Multiplier} PredictedAcc {_predictedAcc} PassRating {_passRating} AccRating {_accRating} TechRating {_techRating} ModifiersRating {ModifiersRating} ModifierValues {ModifierValues}";
        }
    }
}
