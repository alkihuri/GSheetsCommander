using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace GSheetsCommander
{
    /// <summary>Asynchronous HTTP client for a Google Apps Script Google Sheets API.</summary>
    public sealed class GoogleSheetsClient
    {
        private const string LogPrefix = "[GSheetsCommander]";
        private readonly GoogleSheetsConfig config;

        /// <summary>Creates a client using the supplied configuration.</summary>
        public GoogleSheetsClient(GoogleSheetsConfig configuration)
        {
            config = configuration ?? throw new GoogleSheetsException("CONFIGURATION_ERROR", "GoogleSheetsConfig is required.");
            config.Validate();
        }

        /// <summary>Checks that the backend is available.</summary>
        public Task<HealthData> HealthAsync(CancellationToken cancellationToken = default) => SendAsync<HealthData>("health", new { }, cancellationToken);
        /// <summary>Lists sheet tabs.</summary>
        public Task<List<SheetInfo>> ListSheetsAsync(CancellationToken cancellationToken = default) => SendAsync<List<SheetInfo>>("listSheets", new { }, cancellationToken);
        /// <summary>Gets one sheet's metadata.</summary>
        public Task<SheetInfo> GetSheetAsync(string sheet, CancellationToken cancellationToken = default) => SendAsync<SheetInfo>("getSheet", new { sheet = Require(sheet, nameof(sheet)) }, cancellationToken);
        /// <summary>Creates a sheet tab.</summary>
        public Task<SheetInfo> CreateSheetAsync(string name, CancellationToken cancellationToken = default) => SendAsync<SheetInfo>("createSheet", new { name = Require(name, nameof(name)) }, cancellationToken);
        /// <summary>Renames a sheet tab.</summary>
        public Task<RenameSheetResult> RenameSheetAsync(string sheet, string newName, CancellationToken cancellationToken = default) => SendAsync<RenameSheetResult>("renameSheet", new { sheet = Require(sheet, nameof(sheet)), newName = Require(newName, nameof(newName)) }, cancellationToken);
        /// <summary>Deletes a sheet tab.</summary>
        public Task<DeleteSheetResult> DeleteSheetAsync(string sheet, CancellationToken cancellationToken = default) => SendAsync<DeleteSheetResult>("deleteSheet", new { sheet = Require(sheet, nameof(sheet)) }, cancellationToken);
        /// <summary>Gets one cell.</summary>
        public Task<CellData> GetCellAsync(string sheet, string cell, CancellationToken cancellationToken = default) => SendAsync<CellData>("getCell", new { sheet = Require(sheet, nameof(sheet)), cell = Require(cell, nameof(cell)) }, cancellationToken);
        /// <summary>Sets one cell.</summary>
        public Task<CellData> SetCellAsync(string sheet, string cell, object value, CancellationToken cancellationToken = default) => SendAsync<CellData>("setCell", new { sheet = Require(sheet, nameof(sheet)), cell = Require(cell, nameof(cell)), value }, cancellationToken);
        /// <summary>Gets a range of cells.</summary>
        public Task<RangeData> GetRangeAsync(string sheet, string range, CancellationToken cancellationToken = default) => SendAsync<RangeData>("getRange", new { sheet = Require(sheet, nameof(sheet)), range = Require(range, nameof(range)) }, cancellationToken);
        /// <summary>Sets a range of cells.</summary>
        public Task<RangeData> SetRangeAsync(string sheet, string range, object[][] values, CancellationToken cancellationToken = default) => SendAsync<RangeData>("setRange", new { sheet = Require(sheet, nameof(sheet)), range = Require(range, nameof(range)), values = values ?? throw new ArgumentNullException(nameof(values)) }, cancellationToken);
        /// <summary>Appends a row.</summary>
        public Task<RowData> AppendRowAsync(string sheet, object[] values, CancellationToken cancellationToken = default) => SendAsync<RowData>("appendRow", new { sheet = Require(sheet, nameof(sheet)), values = values ?? throw new ArgumentNullException(nameof(values)) }, cancellationToken);
        /// <summary>Updates an existing row.</summary>
        public Task<RowData> UpdateRowAsync(string sheet, int row, object[] values, CancellationToken cancellationToken = default)
        {
            if (row < 1) throw new ArgumentOutOfRangeException(nameof(row), "Row numbers start at 1.");
            return SendAsync<RowData>("updateRow", new { sheet = Require(sheet, nameof(sheet)), row, values = values ?? throw new ArgumentNullException(nameof(values)) }, cancellationToken);
        }

        /// <summary>Sends a custom request using the standard API envelope.</summary>
        public async Task<T> SendAsync<T>(string action, object payload, CancellationToken cancellationToken = default)
        {
            action = Require(action, nameof(action));
            var request = new GoogleSheetsRequest
            {
                action = action,
                requestId = Guid.NewGuid().ToString("D"),
                apiKey = config.ApiKey,
                payload = payload ?? new { }
            };

            string body;
            try { body = JsonConvert.SerializeObject(request); }
            catch (JsonException ex) { throw new GoogleSheetsException("INVALID_REQUEST", "The request could not be serialized.", null, ex); }

            LogRequest(request);
            using (var webRequest = new UnityWebRequest(config.GoogleAppsScriptUrl, UnityWebRequest.kHttpVerbPOST))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.timeout = config.TimeoutSeconds;
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");

                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                var responseText = webRequest.downloadHandler?.text;
                LogResponse(webRequest.responseCode, responseText);
                GoogleSheetsResponse<T> response;
                try { response = string.IsNullOrWhiteSpace(responseText) ? null : JsonConvert.DeserializeObject<GoogleSheetsResponse<T>>(responseText); }
                catch (JsonException ex)
                {
                    if (webRequest.result != UnityWebRequest.Result.Success)
                        throw new GoogleSheetsException("NETWORK_ERROR", string.IsNullOrWhiteSpace(webRequest.error) ? "The request failed." : webRequest.error, null, ex);
                    throw new GoogleSheetsException("INVALID_RESPONSE", "The backend returned invalid JSON.", null, ex);
                }

                if (response == null)
                {
                    if (webRequest.result != UnityWebRequest.Result.Success)
                        throw new GoogleSheetsException("NETWORK_ERROR", string.IsNullOrWhiteSpace(webRequest.error) ? "The request failed." : webRequest.error);
                    throw new GoogleSheetsException("INVALID_RESPONSE", "The backend returned an empty response.");
                }
                if (response.requestId != request.requestId)
                    throw new GoogleSheetsException("INVALID_RESPONSE", "The response requestId does not match the request.");
                if (!response.success)
                    throw new GoogleSheetsException(response.error?.code ?? "INTERNAL_ERROR", response.error?.message ?? "The backend reported an error.", response.error?.details);
                if (webRequest.result != UnityWebRequest.Result.Success)
                    throw new GoogleSheetsException("NETWORK_ERROR", string.IsNullOrWhiteSpace(webRequest.error) ? "The request failed." : webRequest.error);
                return response.data;
            }
        }

        private static string Require(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A value is required.", parameterName);
            return value;
        }

        private void LogRequest(GoogleSheetsRequest request)
        {
            if (config.EnableLogging)
                Debug.Log($"{LogPrefix} Request action={request.action}, requestId={request.requestId}");
        }

        private void LogResponse(long statusCode, string response)
        {
            if (config.EnableLogging)
                Debug.Log($"{LogPrefix} Response status={statusCode}, body={response}");
        }
    }
}
