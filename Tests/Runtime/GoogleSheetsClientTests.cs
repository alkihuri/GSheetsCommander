using System;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace GSheetsCommander.Tests
{
    public sealed class GoogleSheetsClientTests
    {
        private GoogleSheetsConfig config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GoogleSheetsConfig>();
            config.Configure("https://script.google.com/macros/s/example/exec", "test-key");
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(config);

        [Test]
        public void Client_CanBeCreated_WithValidConfiguration() => Assert.DoesNotThrow(() => new GoogleSheetsClient(config));

        [Test]
        public void Configuration_RejectsMissingApiKey()
        {
            config.Configure("https://script.google.com/macros/s/example/exec", "");
            var exception = Assert.Throws<GoogleSheetsException>(() => config.Validate());
            Assert.That(exception.Code, Is.EqualTo("CONFIGURATION_ERROR"));
        }

        [Test]
        public void Request_SerializesExpectedEnvelope()
        {
            var request = new GoogleSheetsRequest { action = "getCell", requestId = "id", apiKey = "key", payload = new { sheet = "Users", cell = "A1" } };
            var json = JsonConvert.SerializeObject(request);
            StringAssert.Contains("\"action\":\"getCell\"", json);
            StringAssert.Contains("\"sheet\":\"Users\"", json);
        }

        [Test]
        public void Response_DeserializesTypedData()
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse<CellData>>("{\"success\":true,\"requestId\":\"id\",\"data\":{\"sheet\":\"Users\",\"cell\":\"A1\",\"value\":42},\"error\":null}");
            Assert.That(response.success, Is.True);
            Assert.That(response.data.cell, Is.EqualTo("A1"));
        }

        [Test]
        public void Error_DeserializesAndCanBecomeException()
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse<object>>("{\"success\":false,\"requestId\":\"id\",\"data\":null,\"error\":{\"code\":\"SHEET_NOT_FOUND\",\"message\":\"Sheet was not found\",\"details\":null}}");
            var exception = new GoogleSheetsException(response.error.code, response.error.message, response.error.details);
            Assert.That(exception.Code, Is.EqualTo("SHEET_NOT_FOUND"));
            Assert.That(exception.Message, Is.EqualTo("Sheet was not found"));
        }
    }

    /// <summary>Opt-in integration flow. Supply configuration through environment variables in CI.</summary>
    public sealed class GoogleSheetsIntegrationTests
    {
        [Test, Explicit("Requires GSHEETS_COMMANDER_URL and GSHEETS_COMMANDER_API_KEY environment variables.")]
        public async System.Threading.Tasks.Task FullBackendFlow()
        {
            var url = Environment.GetEnvironmentVariable("GSHEETS_COMMANDER_URL");
            var key = Environment.GetEnvironmentVariable("GSHEETS_COMMANDER_API_KEY");
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key)) Assert.Ignore("Integration endpoint is not configured.");

            var integrationConfig = ScriptableObject.CreateInstance<GoogleSheetsConfig>();
            integrationConfig.Configure(url, key);
            var client = new GoogleSheetsClient(integrationConfig);
            var name = "GSheetsCommander_" + Guid.NewGuid().ToString("N");
            try
            {
                await client.HealthAsync();
                await client.CreateSheetAsync(name);
                await client.SetCellAsync(name, "A1", "value");
                await client.GetCellAsync(name, "A1");
                await client.SetRangeAsync(name, "A2:B2", new object[][] { new object[] { "one", "two" } });
                await client.GetRangeAsync(name, "A2:B2");
                await client.AppendRowAsync(name, new object[] { "append" });
                await client.UpdateRowAsync(name, 3, new object[] { "update" });
                await client.ListSheetsAsync();
                await client.RenameSheetAsync(name, name + "_Renamed");
                await client.DeleteSheetAsync(name + "_Renamed");
            }
            finally { UnityEngine.Object.DestroyImmediate(integrationConfig); }
        }
    }
}
