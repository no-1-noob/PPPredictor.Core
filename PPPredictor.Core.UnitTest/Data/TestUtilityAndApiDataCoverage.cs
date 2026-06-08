using System.Net;
using System.Reflection;
using PPPredictor.Core;
using PPPredictor.Core.DataType.LeaderBoard;
using static PPPredictor.Core.DataType.Enums;

namespace UnitTests.Data
{
    [TestClass]
    public class TestUtilityAndApiDataCoverage
    {
        [TestMethod]
        public void AccSaberReloadedApiDataTypesPropertiesRoundTrip()
        {
            RoundTripNestedTypeProperties(typeof(AccSaberReloadedDataTypes));

            AccSaberReloadedDataTypes.AccSaberReloadedCurve curve = new AccSaberReloadedDataTypes.AccSaberReloadedCurve
            {
                points = new List<AccSaberReloadedDataTypes.AccSaberReloadedCurvePoint>
                {
                    new AccSaberReloadedDataTypes.AccSaberReloadedCurvePoint { x = 1.25, y = 2.5 },
                    new AccSaberReloadedDataTypes.AccSaberReloadedCurvePoint { x = 3.75, y = 4.5 }
                }
            };

            List<(double, double)> points = curve.GetPointsAsTuples();
            Assert.AreEqual(2, points.Count);
            Assert.AreEqual((1.25, 2.5), points[0]);
            Assert.AreEqual((3.75, 4.5), points[1]);
        }

        [TestMethod]
        public void OtherApiDataTypesPropertiesRoundTrip()
        {
            RoundTripNestedTypeProperties(typeof(BeatLeaderDataTypes));
            RoundTripNestedTypeProperties(typeof(HitBloqDataTypes));
            RoundTripNestedTypeProperties(typeof(ScoreSaberDataTypes));

            BeatLeaderDataTypes.BeatLeaderEvent beatLeaderEvent = new BeatLeaderDataTypes.BeatLeaderEvent
            {
                endDate = new DateTimeOffset(new DateTime(2024, 5, 1), TimeSpan.Zero).ToUnixTimeSeconds()
            };
            Assert.AreEqual(new DateTime(2024, 5, 1), beatLeaderEvent.dtEndDate);
        }

        [TestMethod]
        public async Task NetworkUtilReturnsDeserializedDataAndFallbackData()
        {
            HttpClient successClient = new HttpClient(new StubHttpHandler(HttpStatusCode.OK, "{\"id\":\"curve-id\",\"scale\":2.5}"));
            AccSaberReloadedDataTypes.AccSaberReloadedCurve curve = await NetworkUtil.GetDataAsync<AccSaberReloadedDataTypes.AccSaberReloadedCurve>(
                successClient,
                Leaderboard.AccSaberReloaded,
                nameof(NetworkUtilReturnsDeserializedDataAndFallbackData),
                "https://mock/curve");

            Assert.AreEqual("curve-id", curve.id);
            Assert.AreEqual(2.5, curve.scale);

            HttpClient failedClient = new HttpClient(new StubHttpHandler(HttpStatusCode.InternalServerError, "{}"));
            AccSaberReloadedDataTypes.AccSaberReloadedCurve fallback = await NetworkUtil.GetDataAsync<AccSaberReloadedDataTypes.AccSaberReloadedCurve>(
                failedClient,
                Leaderboard.AccSaberReloaded,
                nameof(NetworkUtilReturnsDeserializedDataAndFallbackData),
                "https://mock/fail");

            Assert.IsNotNull(fallback);
            Assert.IsNull(fallback.id);
        }

        [TestMethod]
        public async Task NetworkUtilReturnsFallbackOnException()
        {
            HttpClient throwingClient = new HttpClient(new ThrowingHttpHandler());
            AccSaberReloadedDataTypes.AccSaberReloadedCurve fallback = await NetworkUtil.GetDataAsync<AccSaberReloadedDataTypes.AccSaberReloadedCurve>(
                throwingClient,
                Leaderboard.AccSaberReloaded,
                nameof(NetworkUtilReturnsFallbackOnException),
                "https://mock/throw");

            Assert.IsNotNull(fallback);
            Assert.IsNull(fallback.id);
        }

        [TestMethod]
        public void ParsingUtilCoversKnownAndUnknownDifficulties()
        {
            Assert.AreEqual(9, ParsingUtil.ParseDifficultyNameToInt("ExpertPlus"));
            Assert.AreEqual(9, ParsingUtil.ParseDifficultyNameToInt("EXPERT_PLUS"));
            Assert.AreEqual(7, ParsingUtil.ParseDifficultyNameToInt("expert"));
            Assert.AreEqual(5, ParsingUtil.ParseDifficultyNameToInt("Hard"));
            Assert.AreEqual(3, ParsingUtil.ParseDifficultyNameToInt("Normal"));
            Assert.AreEqual(1, ParsingUtil.ParseDifficultyNameToInt("Easy"));
            Assert.AreEqual(-1, ParsingUtil.ParseDifficultyNameToInt("Unknown"));
        }

        [TestMethod]
        public void LoggingMessagesAreRaisedForAllMessageTypes()
        {
            List<LoggingMessage> messages = new List<LoggingMessage>();
            CalculatorInstance ci = new CalculatorInstance(new Settings(false, false, false, false, false, "123", PPGainCalculationType.Raw, MapPoolSorting.Alphabetical, "", 7, 12));
            ci.OnMessage += (_, message) => messages.Add(message);

            Logging.ErrorPrint("error");
            Logging.LoadingStatusPrint(Leaderboard.BeatLeader, "loading");
            Logging.DebugNetworkPrint("network", Leaderboard.HitBloq);

            Assert.IsTrue(messages.Any(x => x.loggingType == LoggingMessage.LoggingType.Error && x.message == "error" && x.leaderboard == Leaderboard.NoLeaderboard));
            Assert.IsTrue(messages.Any(x => x.loggingType == LoggingMessage.LoggingType.LoadingStatus && x.message == "loading" && x.leaderboard == Leaderboard.BeatLeader));
            Assert.IsTrue(messages.Any(x => x.loggingType == LoggingMessage.LoggingType.DebugNetworkPrint && x.message == "network" && x.leaderboard == Leaderboard.HitBloq));
        }

        private static void RoundTripNestedTypeProperties(Type ownerType)
        {
            foreach (Type nestedType in ownerType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                object? instance = Activator.CreateInstance(nestedType);
                Assert.IsNotNull(instance, nestedType.FullName);

                foreach (PropertyInfo property in nestedType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!property.CanRead)
                    {
                        continue;
                    }

                    object? value = CreateValue(property.PropertyType);
                    if (property.CanWrite)
                    {
                        property.SetValue(instance, value);
                    }

                    object? actual = property.GetValue(instance);
                    if (property.CanWrite)
                    {
                        AssertPropertyValue(value, actual, property);
                    }
                }
            }
        }

        private static object? CreateValue(Type type)
        {
            if (type == typeof(string)) return "value";
            if (type == typeof(int)) return 42;
            if (type == typeof(long)) return 4242L;
            if (type == typeof(float)) return 4.2f;
            if (type == typeof(double)) return 4.2d;
            if (type == typeof(bool)) return true;
            if (type == typeof(DateTimeOffset)) return new DateTimeOffset(new DateTime(2024, 6, 1));
            if (type == typeof(DateTime)) return new DateTime(2024, 6, 1);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return Activator.CreateInstance(type);
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return Activator.CreateInstance(type);
            }
            if (type.IsClass)
            {
                return Activator.CreateInstance(type);
            }
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static void AssertPropertyValue(object? expected, object? actual, PropertyInfo property)
        {
            if (expected is System.Collections.ICollection expectedCollection && actual is System.Collections.ICollection actualCollection)
            {
                Assert.AreEqual(expectedCollection.Count, actualCollection.Count, property.Name);
                return;
            }

            Assert.AreEqual(expected, actual, property.Name);
        }

        private class StubHttpHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode statusCode;
            private readonly string content;

            public StubHttpHandler(HttpStatusCode statusCode, string content)
            {
                this.statusCode = statusCode;
                this.content = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(statusCode)
                {
                    RequestMessage = request,
                    Content = new StringContent(content)
                });
            }
        }

        private class ThrowingHttpHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("mock failure");
            }
        }
    }
}
