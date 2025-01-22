using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.BeatSaberEncapsulation;

namespace UnitTests.Data
{
    [TestClass]
    public class TestPPPBeatMapInfo
    {
        [TestMethod]
        public void DefaultConstructor()
        {
            PPPBeatMapInfo pPPBeatMapInfo = new PPPBeatMapInfo();
            Assert.IsNotNull(pPPBeatMapInfo.BaseStarRating, "BaseStarRating should be set");
            Assert.IsNotNull(pPPBeatMapInfo.ModifiedStarRating, "ModifiedStarRating should be set");
            Assert.IsNull(pPPBeatMapInfo.CustomLevelHash, "SelectedCustomBeatmapLevel should not be set");
            Assert.IsNull(pPPBeatMapInfo.BeatmapKey, "BeatmapKey should be set");
            Assert.IsNull(pPPBeatMapInfo.SelectedMapSearchString, "SelectedMapSearchString should not be set");

            Assert.IsTrue(pPPBeatMapInfo.MaxPP == -1, "MaxPPS should be -1");
            Assert.IsTrue(pPPBeatMapInfo.BaseStarRating.Stars == 0, "MaxPPS should be 0");
            Assert.IsTrue(pPPBeatMapInfo.ModifiedStarRating.Stars == 0, "MaxPPS should be 0");
            Assert.IsFalse(pPPBeatMapInfo.OldDotsEnabled, "OldDots should be false");

            pPPBeatMapInfo.BaseStarRating = new PPPStarRating(1);
            pPPBeatMapInfo.ModifiedStarRating = new PPPStarRating(2);
            pPPBeatMapInfo.CustomLevelHash = "HASH";
            pPPBeatMapInfo.BeatmapKey = new BeatmapKey();
            pPPBeatMapInfo.MaxPP = 123;
            pPPBeatMapInfo.SelectedMapSearchString = "Test";
            pPPBeatMapInfo.OldDotsEnabled = true;

            Assert.IsTrue(pPPBeatMapInfo.BaseStarRating.Stars == 1, "MaxPPS should be 1");
            Assert.IsTrue(pPPBeatMapInfo.ModifiedStarRating.Stars == 2, "MaxPPS should be 2");
            Assert.IsNotNull(pPPBeatMapInfo.CustomLevelHash, "SelectedCustomBeatmapLevel should not be null");
            Assert.IsTrue(pPPBeatMapInfo.CustomLevelHash == "HASH", "SelectedCustomBeatmapLevel should be set");
            Assert.IsNotNull(pPPBeatMapInfo.BeatmapKey, "BeatmapKey should not be null");
            Assert.IsTrue(pPPBeatMapInfo.MaxPP == 123, "MaxPPS should be 123");
            Assert.IsTrue(pPPBeatMapInfo.SelectedMapSearchString == "Test", "SelectedMapSearchString should be 'Test'");
            Assert.IsTrue(pPPBeatMapInfo.OldDotsEnabled, "OldDots should be true");
        }

        [TestMethod]
        public void BaseStarsConstructor()
        {
            int startRating = 5;
            PPPBeatMapInfo pPPBeatMapInfo = new PPPBeatMapInfo(new PPPBeatMapInfo("HASH", new BeatmapKey()), new PPPStarRating(startRating));
            Assert.IsNotNull(pPPBeatMapInfo.BaseStarRating, "BaseStarRating should be set");
            Assert.IsNotNull(pPPBeatMapInfo.ModifiedStarRating, "ModifiedStarRating should be set");
            Assert.IsNotNull(pPPBeatMapInfo.CustomLevelHash, "SelectedCustomBeatmapLevel should be set");
            Assert.IsTrue(pPPBeatMapInfo.CustomLevelHash == "HASH", "SelectedCustomBeatmapLevel should be set");
            Assert.IsNotNull(pPPBeatMapInfo.BeatmapKey, "BeatmapKey should not be set");
            Assert.IsNull(pPPBeatMapInfo.SelectedMapSearchString, "SelectedMapSearchString should not be set");

            Assert.IsTrue(pPPBeatMapInfo.MaxPP == -1, "MaxPPS should be -1");
            Assert.IsTrue(pPPBeatMapInfo.BaseStarRating.Stars == startRating, "MaxPPS should be startRating");
            Assert.IsTrue(pPPBeatMapInfo.ModifiedStarRating.Stars == startRating, "MaxPPS should be startRating");
        }

        [TestMethod]
        public void BeatmapConstructor()
        {
            PPPBeatMapInfo pPPBeatMapInfo = new PPPBeatMapInfo("HASH", new BeatmapKey());
            Assert.IsNotNull(pPPBeatMapInfo.BaseStarRating, "BaseStarRating should be set");
            Assert.IsNotNull(pPPBeatMapInfo.ModifiedStarRating, "ModifiedStarRating should be set");
            Assert.IsNotNull(pPPBeatMapInfo.CustomLevelHash, "SelectedCustomBeatmapLevel should be set");
            Assert.IsTrue(pPPBeatMapInfo.CustomLevelHash == "HASH", "SelectedCustomBeatmapLevel should be set");
            Assert.IsNotNull(pPPBeatMapInfo.BeatmapKey, "BeatmapKey should be set");
            Assert.IsNull(pPPBeatMapInfo.SelectedMapSearchString, "SelectedMapSearchString should not be set");

            Assert.IsTrue(pPPBeatMapInfo.MaxPP == -1, "MaxPPS should be -1");
            Assert.IsTrue(pPPBeatMapInfo.BaseStarRating.Stars == 0, "MaxPPS should be startRating");
            Assert.IsTrue(pPPBeatMapInfo.ModifiedStarRating.Stars == 0, "MaxPPS should be startRating");
        }
    }
}
