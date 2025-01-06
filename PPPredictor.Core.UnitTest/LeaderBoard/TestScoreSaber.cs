using PPPredictor.Core.DataType;
using PPPredictor.Core.UnitTest.MockService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPPredictor.Core.UnitTest.LeaderBoard
{
    [TestClass]
    public class TestScoreSaber
    {
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstanceCreationErrorWithoutLookup()
        {
            CalculatorInstance ci = await CalculatorInstance.CreateAsyncMock<MockScoreSaberApi, MockBeatLeader, MockHitBloq, MockAccSaber>(
                new Settings(true, false, false, false, "123", DataType.Enums.PPGainCalculationType.Raw, DataType.Enums.MapPoolSorting.Alphabetical, "", 7, DateTime.Now, 12),
                new Dictionary<string, DataType.LeaderBoard.LeaderboardData>(),
                null
                );
        }

        [TestMethod]
        public async Task InstanceCreationWithLookup()
        {
            CalculatorInstance ci = await CalculatorInstance.CreateAsyncMock<MockScoreSaberApi, MockBeatLeader, MockHitBloq, MockAccSaber>(
                new Settings(true, false, false, false, "123", DataType.Enums.PPGainCalculationType.Raw, DataType.Enums.MapPoolSorting.Alphabetical, "", 7, DateTime.Now, 12),
                new Dictionary<string, DataType.LeaderBoard.LeaderboardData>(),
                (PPPBeatMapInfo) => new PPPBeatMapInfo()
                );
        }
    }
}
