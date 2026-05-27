using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace PPPredictor.Core.DataType.Score
{
    public class ShortScore
    {
        private readonly string _searchstring;
        private readonly string _category = "";
        private double _pp;
        private double _weightedSum;
        private double _ppWeighted;
        private PPPStarRating _starRating;
        private DateTime _fetchTime;
        private bool _isPlaceHolder = false;

        public string Searchstring { get => _searchstring; }
        public double Pp { get => _pp; set => _pp = value; }
        public PPPStarRating StarRating { get => _starRating; set => _starRating = value; }
        public DateTime FetchTime { get => _fetchTime; set => _fetchTime = value; }
        [DefaultValue("")]
        public string Category => _category;
        // [JsonIgnore]
        public double WeightedSum { get => _weightedSum; set => _weightedSum = value; }
        // [JsonIgnore]
        public double PPWeighted { get => _ppWeighted; set => _ppWeighted = value; }
        [DefaultValue(false)]
        public bool IsPlaceHolder { get => _isPlaceHolder; set => _isPlaceHolder = value; }

        public ShortScore(string searchstring, double pp)
        {
            _searchstring = searchstring.ToUpper();
            _pp = pp;
            _starRating = new PPPStarRating();
        }
        
        public ShortScore(string searchString, bool isPlaceHolder)
        {
            _searchstring = searchString.ToUpper();
            _isPlaceHolder = isPlaceHolder;
            _starRating = new PPPStarRating();
        }

        public ShortScore(string searchstring, PPPStarRating starRating, DateTime fetchTime)
        {
            _searchstring = searchstring.ToUpper();
            _starRating = starRating;
            _fetchTime = fetchTime;
        }

        public ShortScore(string searchstring, PPPStarRating starRating, DateTime fetchTime, string category)
        {
            _searchstring = searchstring.ToUpper();
            _starRating = starRating;
            _fetchTime = fetchTime;
            _category = category;
        }

        [JsonConstructor]
        public ShortScore(string searchstring, double pp, PPPStarRating starRating, DateTime fetchTime, string category)
        {
            _searchstring = searchstring.ToUpper();
            _pp = pp;
            _fetchTime = fetchTime;
            _starRating = starRating ?? new PPPStarRating();
            _category = category;
        }

        public bool ShouldSerializeStarRating()
        {
            return _starRating.IsRanked();
        }
    }
}
