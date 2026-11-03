using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

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
        private double _sliderPercentage = 0;
        private double _noteSpan = 0;
        private int _notes = 0;
        private double _noteSpanMult = 1;
        private bool? _rankedBeatLeader = null;
        private Dictionary<string, double> _modifiersRating = new Dictionary<string, double>();
        private Dictionary<string, double> _modifierValues = new Dictionary<string, double>();
        
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
        [DefaultValue(0d)] 
        public double SliderPercentage { get => _sliderPercentage; set => _sliderPercentage = value; }
        [DefaultValue(0d)] 
        public double NoteSpan { get => _noteSpan; set => _noteSpan = value; }
        [DefaultValue(0d)] 
        public int Notes { get => _notes; set => _notes = value; }
        [DefaultValue(1d)] 
        public double NoteSpanMult { get => _noteSpanMult; set => _noteSpanMult = value; }
        [DefaultValue(null)]
        public Dictionary<string, double> ModifiersRating { get => _modifiersRating; set => _modifiersRating = value; }
        [DefaultValue(null)]
        public Dictionary<string, double> ModifierValues { get => _modifierValues; set => _modifierValues = value; }
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
            _sliderPercentage = beatLeaderDifficulty.sliderPercentage.GetValueOrDefault();
            _notes = beatLeaderDifficulty.notes;
            _noteSpan = beatLeaderDifficulty.noteSpan.GetValueOrDefault();
            _noteSpanMult = 1;
            _modifiersRating = beatLeaderDifficulty.modifiersRating ?? new Dictionary<string, double>();
            _modifierValues = beatLeaderDifficulty.modifierValues ?? new Dictionary<string, double>();
        }

        internal PPPStarRating(double mulitplier, double accRating, double passRating, double techRating, double predictedAcc, double sliderPercentage, int notes, double noteSpan, double noteSpanMult, bool? rankedBeatLeader)
        {
            _rankedBeatLeader = rankedBeatLeader;
            _multiplier = mulitplier;
            _accRating = accRating;
            _passRating = passRating;
            _techRating = techRating;
            _predictedAcc = predictedAcc;
            _sliderPercentage = sliderPercentage;
            _notes = notes;
            _noteSpan = noteSpan;
            _noteSpanMult = noteSpanMult;
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
            return $"PPPStarRating: Stars {Stars} Multiplier {Multiplier} PredictedAcc {PredictedAcc} PassRating {PassRating} AccRating {AccRating} TechRating {TechRating} SliderPercentage {SliderPercentage} NoteSpan {NoteSpan} Notes {Notes} NoteSpanMult {NoteSpanMult} ModifiersRating {ModifiersRating} ModifierValues {ModifierValues}";
        }
    }
}
