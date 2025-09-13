namespace PPPredictor.Core.DataType.MapPool
{
    public class PPPMapPoolEntry
    {
        private string _searchstring;
        public string Searchstring { get => _searchstring; set => _searchstring = value; }

        public PPPMapPoolEntry()
        {
            Searchstring = string.Empty;
        }
        public PPPMapPoolEntry(string searchstring)
        {
            Searchstring = searchstring;
        }
    }
}
