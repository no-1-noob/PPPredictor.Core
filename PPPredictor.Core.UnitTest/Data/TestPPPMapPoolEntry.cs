using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.MapPool;
using static PPPredictor.Core.DataType.LeaderBoard.BeatLeaderDataTypes;

namespace UnitTests.Data
{
    [TestClass]
    public class TestPPPMapPoolEntry
    {
        private readonly string testHash = "#Test";

        [TestMethod]
        public void DefaultConstructor()
        {
            PPPMapPoolEntry mapPoolEntry = new PPPMapPoolEntry();
            Assert.IsNotNull(mapPoolEntry.Searchstring);
            Assert.IsTrue(mapPoolEntry.Searchstring == string.Empty, "Searchstring is string.Empty");
            mapPoolEntry.Searchstring = testHash;
            Assert.IsTrue(mapPoolEntry.Searchstring == testHash, "Searchstring is testHash");
        }

        [TestMethod]
        public void SingleConstructor()
        {
            PPPMapPoolEntry mapPoolEntry = new PPPMapPoolEntry(testHash);
            Assert.IsNotNull(mapPoolEntry.Searchstring);
            Assert.IsTrue(mapPoolEntry.Searchstring == testHash, "Searchstring is testHash");
        }

        [TestMethod]
        public void TupelConstructor()
        {
            BeatLeaderSong song = new BeatLeaderSong() { hash = testHash };
            BeatLeaderPlayListDifficulties diff = new BeatLeaderPlayListDifficulties() { name = Enums.BeatMapDifficulty.ExpertPlus};
            PPPMapPoolEntry mapPoolEntry = new PPPMapPoolEntry(song, diff);
            Assert.IsNotNull(mapPoolEntry.Searchstring);
            Assert.IsTrue(mapPoolEntry.Searchstring == $"{testHash}_{(int)Enums.BeatMapDifficulty.ExpertPlus}", "Searchstring is correct");
        }
    }
}
