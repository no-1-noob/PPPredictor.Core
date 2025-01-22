using PPPredictor.Core.DataType;
using PPPredictor.Core.DataType.Curve;
using PPPredictor.Core.DataType.MapPool;
using System.Security.Cryptography;
using static PPPredictor.Core.DataType.Enums;

namespace UnitTests.Data
{
    [TestClass]
    public class TestPPPMapPool
    {
        [TestMethod]
        public void DefaultConstructor()
        {
            PPPMapPool mapPool = new PPPMapPool();
            Assert.IsNotNull(mapPool.CurrentPlayer);
            Assert.IsNotNull(mapPool.LsScores);
            Assert.IsNotNull(mapPool.LsLeaderboadInfo);
            Assert.IsNotNull(mapPool.LsMapPoolEntries);
            Assert.IsNotNull(mapPool.LsPlayerRankings);
            Assert.AreEqual(mapPool.DtUtcLastRefresh, new DateTime(2000, 1, 1), "DtUtcLastRefresh should match");
            Assert.IsNotNull(mapPool.Curve);
            Assert.IsNull(mapPool.CurveInfo);
            Assert.AreEqual(mapPool.Id, "-1", "Id should be -1");
            Assert.AreEqual(mapPool.PlayListId, "-1", "PlayListId should be -1");
            Assert.AreEqual(mapPool.MapPoolType, MapPoolType.Custom, "MapPoolType should be Custom");
            Assert.AreEqual(mapPool.AccumulationConstant, 0, "AccumulationConstant should be 0");
            Assert.AreEqual(mapPool.SortIndex, -1, "SortIndex should be -1");
            Assert.AreEqual(mapPool.DtLastScoreSet, new DateTime(2000, 1, 1), "DtLastScoreSet should match");
            Assert.AreEqual(mapPool.Popularity, 0, "Popularity should be 0");
            Assert.AreEqual(mapPool.SyncUrl, string.Empty, "SyncUrl should be string.Empty");
            Assert.AreEqual(mapPool.MapPoolName, string.Empty, "MapPoolName should be string.Empty");
            Assert.IsNull(mapPool.IconData);
            Assert.IsNull(mapPool.SessionPlayer);
            Assert.IsNull(mapPool.IconUrl);
            Assert.AreEqual(mapPool.ToString(), string.Empty);

            mapPool.MapPoolName = "Test";
            mapPool.AccumulationConstant = 1;
            mapPool.SortIndex = 1;
            mapPool.LsScores = null;
            mapPool.LsLeaderboadInfo = null;
            mapPool.LsMapPoolEntries = null;
            mapPool.MapPoolType = MapPoolType.Default;
            mapPool.Curve = new BeatLeaderPPPCurve();
            mapPool.SessionPlayer = new PPPPlayer();
            mapPool.CurrentPlayer = null;
            mapPool.Id = "1";
            mapPool.PlayListId = "1";
            mapPool.LsPlayerRankings = null;
            mapPool.DtUtcLastRefresh = DateTime.Now;
            mapPool.DtLastScoreSet = DateTime.Now;
            mapPool.IconUrl = "URL";
            mapPool.IconData = new byte[] { };
            mapPool.Popularity = 1;
            mapPool.SyncUrl = "URL";

            Assert.IsNull(mapPool.CurrentPlayer);
            Assert.IsNull(mapPool.LsScores);
            Assert.IsNull(mapPool.LsLeaderboadInfo);
            Assert.IsNull(mapPool.LsMapPoolEntries);
            Assert.IsNull(mapPool.LsPlayerRankings);
            Assert.AreNotEqual(mapPool.DtUtcLastRefresh, new DateTime(2000, 1, 1), "DtUtcLastRefresh should not match");
            Assert.IsNotNull(mapPool.Curve);
            Assert.IsNotNull(mapPool.CurveInfo);
            Assert.AreNotEqual(mapPool.Id, "-1", "Id should not be -1");
            Assert.AreNotEqual(mapPool.PlayListId, "-1", "PlayListId should not be -1");
            Assert.AreNotEqual(mapPool.MapPoolType, MapPoolType.Custom, "MapPoolType should not be Custom");
            Assert.AreNotEqual(mapPool.AccumulationConstant, 0, "AccumulationConstant should not be 0");
            Assert.AreNotEqual(mapPool.SortIndex, -1, "SortIndex should not be -1");
            Assert.AreNotEqual(mapPool.DtLastScoreSet, new DateTime(2000, 1, 1), "DtLastScoreSet should not match");
            Assert.AreNotEqual(mapPool.Popularity, 0, "Popularity should not be 0");
            Assert.AreNotEqual(mapPool.SyncUrl, string.Empty, "SyncUrl should not be string.Empty");
            Assert.AreNotEqual(mapPool.MapPoolName, string.Empty, "MapPoolName should not be string.Empty");
            Assert.IsNotNull(mapPool.IconData);
            Assert.IsNotNull(mapPool.SessionPlayer);
            Assert.IsNotNull(mapPool.IconUrl);
            Assert.AreNotEqual(mapPool.ToString(), string.Empty);

            mapPool.LsScores = new List<PPPredictor.Core.DataType.Score.ShortScore>()
            {
                new PPPredictor.Core.DataType.Score.ShortScore("Test", 1)
            };
            Assert.IsNotNull(mapPool.LsScores);

            mapPool.Curve = null;
            Assert.ThrowsException<NullReferenceException>(() => mapPool.CurveInfo, "Should throw a NullReferenceException");

            mapPool.CurveInfo = new CurveInfo() { CurveType = CurveType.ScoreSaber };
            Assert.IsNotNull(mapPool.Curve);
            Assert.IsNotNull(mapPool.CurveInfo);
        }

        [TestMethod]
        public void LongConstructor()
        {
            string id = "5";
            string playListId = "5";
            string mapPoolName = "TestName";
            float accConstant = 1.23f;
            int sortIndex = 123;
            string iconUrl = "URLTest";
            int popularity = 4;
            string syncUrl = "SyncURL";
            IPPPCurve curve = new BeatLeaderPPPCurve();
            PPPMapPool mapPool = new PPPMapPool(id, playListId, MapPoolType.Default, mapPoolName, accConstant, sortIndex, curve, iconUrl);
            Assert.AreEqual(mapPool.MapPoolType, MapPoolType.Default, "MapPoolType should be Default");
            Assert.AreEqual(mapPool.MapPoolName, mapPoolName, "MapPoolName should be mapPoolName");
            Assert.AreEqual(mapPool.AccumulationConstant, accConstant, "AccumulationConstant should be accConstant");
            Assert.AreEqual(mapPool.SortIndex, sortIndex, "SortIndex should be sortIndex");
            Assert.AreEqual(mapPool.Curve, curve, "Curve should match");
            Assert.AreEqual(mapPool.IconUrl, iconUrl, "IconUrl should match");
            Assert.AreEqual(mapPool.Popularity, 0, "Popularity should be 0");
            Assert.AreEqual(mapPool.SyncUrl, string.Empty, "SyncUrl should be empty");

            mapPool = new PPPMapPool(id, playListId, MapPoolType.Default, mapPoolName, accConstant, sortIndex, curve, iconUrl, popularity, syncUrl, LeaderboardContext.BeatLeaderNoModifiers);
            Assert.AreEqual(mapPool.MapPoolType, MapPoolType.Default, "MapPoolType should be Default");
            Assert.AreEqual(mapPool.MapPoolName, mapPoolName, "MapPoolName should be mapPoolName");
            Assert.AreEqual(mapPool.AccumulationConstant, accConstant, "AccumulationConstant should be accConstant");
            Assert.AreEqual(mapPool.SortIndex, sortIndex, "SortIndex should be sortIndex");
            Assert.AreEqual(mapPool.Curve, curve, "Curve should match");
            Assert.AreEqual(mapPool.IconUrl, iconUrl, "IconUrl should match");
            Assert.AreEqual(mapPool.Popularity, popularity, "Popularity should be popularity");
            Assert.AreEqual(mapPool.SyncUrl, syncUrl, "SyncUrl should be syncUrl");

            Assert.IsTrue(mapPool.IsPlayerFound, "IsPlayerFound should be true");
            mapPool.IsPlayerFound = false;
            Assert.IsFalse(mapPool.IsPlayerFound, "IsPlayerFound should be false");
            Assert.AreEqual(LeaderboardContext.BeatLeaderNoModifiers, mapPool.LeaderboardContext);

            Assert.AreEqual(string.Empty, mapPool.CustomLeaderboardUserId);
            mapPool.CustomLeaderboardUserId = "-1";
            Assert.AreEqual("-1", mapPool.CustomLeaderboardUserId);

            Assert.IsNotNull(mapPool.DctWeightLookup);
        }

        [TestMethod]
        public void MediumConstructor()
        {
            string mapPoolName = "TestName";
            float accConstant = 1.23f;
            int sortIndex = 123;
            IPPPCurve curve = new BeatLeaderPPPCurve();
            PPPMapPool mapPool = new PPPMapPool(MapPoolType.Default, mapPoolName, accConstant, sortIndex, curve);
            Assert.AreEqual(mapPool.MapPoolType, MapPoolType.Default, "MapPoolType should be Default");
            Assert.AreEqual(mapPool.MapPoolName, mapPoolName, "MapPoolName should be mapPoolName");
            Assert.AreEqual(mapPool.AccumulationConstant, accConstant, "AccumulationConstant should be accConstant");
            Assert.AreEqual(mapPool.SortIndex, sortIndex, "SortIndex should be sortIndex");
            Assert.AreEqual(mapPool.Curve, curve, "Curve should match");
        }

        [TestMethod]
        public void IdConstructor()
        {
            string mapPoolName = "TestName";
            float accConstant = 1.23f;
            int sortIndex = 123;
            IPPPCurve curve = new BeatLeaderPPPCurve();
            PPPMapPool mapPool = new PPPMapPool("-1", MapPoolType.Default, mapPoolName, accConstant, sortIndex, curve);
            Assert.AreEqual(mapPool.MapPoolType, MapPoolType.Default, "MapPoolType should be Default");
            Assert.AreEqual(mapPool.MapPoolName, mapPoolName, "MapPoolName should be mapPoolName");
            Assert.AreEqual(mapPool.AccumulationConstant, accConstant, "AccumulationConstant should be accConstant");
            Assert.AreEqual(mapPool.SortIndex, sortIndex, "SortIndex should be sortIndex");
            Assert.AreEqual(mapPool.Curve, curve, "Curve should match");
            Assert.AreEqual(mapPool.Id, "-1", "Id should match");
        }

        [TestMethod]
        public void TestToString()
        {
            PPPMapPool mapPool = new PPPMapPool();
            Assert.AreEqual(mapPool.ToString(), string.Empty);
            mapPool.MapPoolName = "MapPoolName";
            Assert.AreNotEqual(mapPool.ToString(), string.Empty);
            mapPool.MapPoolName = null;
            Assert.AreEqual(mapPool.ToString(), string.Empty);
        }

        [TestMethod]
        public void TestImplicitConversion()
        {
            string id = "5";
            string playListId = "5";
            string mapPoolName = "TestName";
            float accConstant = 1.23f;
            int sortIndex = 123;
            string iconUrl = "URLTest";
            int popularity = 4;
            string syncUrl = "SyncURL";
            IPPPCurve curve = new BeatLeaderPPPCurve();
            PPPMapPoolShort mapPool = new PPPMapPool(id, playListId, MapPoolType.Default, mapPoolName, accConstant, sortIndex, curve, iconUrl, popularity, syncUrl);
            var iconData = new byte[] { };
            mapPool.IconData = iconData;
            Assert.AreEqual(mapPool.MapPoolName, mapPoolName, "MapPoolName should be mapPoolName");
            Assert.AreEqual(mapPool.SortIndex, sortIndex, "SortIndex should be sortIndex");
            Assert.AreEqual(mapPool.IconUrl, iconUrl, "IconUrl should match");
            Assert.AreEqual(mapPool.IconData, iconData, "IconData should match");
            Assert.AreEqual(mapPool.Id, id, "id should match");

            Assert.AreEqual(mapPool.ToString(), mapPoolName, "ToString should match mapPoolName");
            mapPool = new PPPMapPool(id, playListId, MapPoolType.Default, null, accConstant, sortIndex, curve, iconUrl, popularity, syncUrl);
            Assert.AreEqual(mapPool.ToString(), "", "ToString should be empty");

            Assert.IsFalse(mapPool.SelectedByLoading, "Selected by loading should be false");
            mapPool.SelectedByLoading = true;
            Assert.IsTrue(mapPool.SelectedByLoading, "Selected by loading should be false");

            PPPMapPool? nullMapPool = null;
            mapPool = nullMapPool;

            Assert.IsNull(mapPool, "MapPool should be null");

        }

        [TestMethod]
        public void ShortPPMapPool()
        {
            PPPMapPoolShort mapPool = new PPPMapPoolShort();
            var iconData = new byte[] { };
            Assert.AreEqual(mapPool.MapPoolName, string.Empty, "MapPoolName should be mapPoolName");
            Assert.AreEqual(mapPool.SortIndex, 0, "SortIndex should be sortIndex");
            Assert.AreEqual(mapPool.IconUrl, string.Empty, "IconUrl should match");
            Assert.AreEqual(mapPool.IconData.Length, 0, "IconData should match");
            Assert.AreEqual(mapPool.Id, string.Empty, "id should match");
        }
    }
}
