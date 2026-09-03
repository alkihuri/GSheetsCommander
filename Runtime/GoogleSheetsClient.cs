using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        public Task<HealthData> HealthAsync(CancellationToken cancellationToken = default) => HealthAsync(config.UseGetRequests, cancellationToken);
        public Task<HealthData> HealthAsync(bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<HealthData>("health", new { }, useGetRequests, cancellationToken);
        /// <summary>Lists sheet tabs.</summary>
        public Task<List<SheetInfo>> ListSheetsAsync(CancellationToken cancellationToken = default) => ListSheetsAsync(config.UseGetRequests, cancellationToken);
        public Task<List<SheetInfo>> ListSheetsAsync(bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<List<SheetInfo>>("listSheets", new { }, useGetRequests, cancellationToken);
        /// <summary>Gets one sheet's metadata.</summary>
        public Task<SheetInfo> GetSheetAsync(string sheet, CancellationToken cancellationToken = default) => GetSheetAsync(sheet, config.UseGetRequests, cancellationToken);
        public Task<SheetInfo> GetSheetAsync(string sheet, bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<SheetInfo>("getSheet", new { sheet = Require(sheet, nameof(sheet)) }, useGetRequests, cancellationToken);
        /// <summary>Gets one sheet's metadata, or <see langword="null"/> when the sheet does not exist.</summary>
        public async Task<SheetInfo> TryGetSheetAsync(string sheet, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetSheetAsync(sheet, cancellationToken);
            }
            catch (GoogleSheetsException exception) when (exception.Code == "SHEET_NOT_FOUND")
            {
                return null;
            }
        }
        /// <summary>Checks whether a sheet tab exists without throwing when it is absent.</summary>
        public async Task<bool> SheetExistsAsync(string sheet, CancellationToken cancellationToken = default) =>
            await TryGetSheetAsync(sheet, cancellationToken) != null;
        /// <summary>Creates a sheet tab.</summary>
        public Task<SheetInfo> CreateSheetAsync(string name, CancellationToken cancellationToken = default) => CreateSheetAsync(name, config.UseGetRequests, cancellationToken);
        public Task<SheetInfo> CreateSheetAsync(string name, bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<SheetInfo>("createSheet", new { name = Require(name, nameof(name)) }, useGetRequests, cancellationToken);
        /// <summary>Renames a sheet tab.</summary>
        public Task<RenameSheetResult> RenameSheetAsync(string sheet, string newName, CancellationToken cancellationToken = default) => RenameSheetAsync(sheet, newName, config.UseGetRequests, cancellationToken);
        public Task<RenameSheetResult> RenameSheetAsync(string sheet, string newName, bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<RenameSheetResult>("renameSheet", new { sheet = Require(sheet, nameof(sheet)), newName = Require(newName, nameof(newName)) }, useGetRequests, cancellationToken);
        /// <summary>Deletes a sheet tab.</summary>
        public Task<DeleteSheetResult> DeleteSheetAsync(string sheet, CancellationToken cancellationToken = default) => DeleteSheetAsync(sheet, config.UseGetRequests, cancellationToken);
        public Task<DeleteSheetResult> DeleteSheetAsync(string sheet, bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<DeleteSheetResult>("deleteSheet", new { sheet = Require(sheet, nameof(sheet)) }, useGetRequests, cancellationToken);
        /// <summary>Gets one cell.</summary>
        public Task<CellData> GetCellAsync(string sheet, string cell, CancellationToken cancellationToken = default) => GetCellAsync(sheet, cell, config.UseGetRequests, cancellationToken);
        public Task<CellData> GetCellAsync(string sheet, string cell, bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<CellData>("getCell", new { sheet = Require(sheet, nameof(sheet)), cell = Require(cell, nameof(cell)) }, useGetRequests, cancellationToken);
        /// <summary>Sets one cell.</summary>
        public Task<CellData> SetCellAsync(string sheet, string cell, object value, CancellationToken cancellationToken = default) => SetCellAsync(sheet, cell, value, config.UseGetRequests, cancellationToken);
        public Task<CellData> SetCellAsync(string sheet, string cell, object value, bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<CellData>("setCell", new { sheet = Require(sheet, nameof(sheet)), cell = Require(cell, nameof(cell)), value }, useGetRequests, cancellationToken);
        /// <summary>Gets a range of cells.</summary>
        public Task<RangeData> GetRangeAsync(string sheet, string range, CancellationToken cancellationToken = default) => GetRangeAsync(sheet, range, config.UseGetRequests, cancellationToken);
        public Task<RangeData> GetRangeAsync(string sheet, string range, bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<RangeData>("getRange", new { sheet = Require(sheet, nameof(sheet)), range = Require(range, nameof(range)) }, useGetRequests, cancellationToken);
        /// <summary>Sets a range of cells.</summary>
        public Task<RangeData> SetRangeAsync(string sheet, string range, object[][] values, CancellationToken cancellationToken = default) => SetRangeAsync(sheet, range, values, config.UseGetRequests, cancellationToken);
        public Task<RangeData> SetRangeAsync(string sheet, string range, object[][] values, bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<RangeData>("setRange", new { sheet = Require(sheet, nameof(sheet)), range = Require(range, nameof(range)), values = values ?? throw new ArgumentNullException(nameof(values)) }, useGetRequests, cancellationToken);
        /// <summary>Appends a row.</summary>
        public Task<RowData> AppendRowAsync(string sheet, object[] values, CancellationToken cancellationToken = default) => AppendRowAsync(sheet, values, config.UseGetRequests, cancellationToken);
        public Task<RowData> AppendRowAsync(string sheet, object[] values, bool useGetRequests, CancellationToken cancellationToken = default) => SendAsync<RowData>("appendRow", new { sheet = Require(sheet, nameof(sheet)), values = values ?? throw new ArgumentNullException(nameof(values)) }, useGetRequests, cancellationToken);
        /// <summary>Updates an existing row.</summary>
        public Task<RowData> UpdateRowAsync(string sheet, int row, object[] values, CancellationToken cancellationToken = default) => UpdateRowAsync(sheet, row, values, config.UseGetRequests, cancellationToken);
        public Task<RowData> UpdateRowAsync(string sheet, int row, object[] values, bool useGetRequests, CancellationToken cancellationToken = default)
        {
            if (row < 1) throw new ArgumentOutOfRangeException(nameof(row), "Row numbers start at 1.");
            return SendAsync<RowData>("updateRow", new { sheet = Require(sheet, nameof(sheet)), row, values = values ?? throw new ArgumentNullException(nameof(values)) }, useGetRequests, cancellationToken);
        }

        /// <summary>Sends a custom request using the standard API envelope.</summary>
        public Task<T> SendAsync<T>(string action, object payload, CancellationToken cancellationToken = default) => SendAsync<T>(action, payload, config.UseGetRequests, cancellationToken);
        public async Task<T> SendAsync<T>(string action, object payload, bool useGetRequests, CancellationToken cancellationToken = default)
        {
            action = Require(action, nameof(action));
            var request = new GoogleSheetsRequest
            {
                action = action,
                requestId = Guid.NewGuid().ToString("D"),
                apiKey = config.ApiKey,
                payload = payload ?? new { }
            };

            LogRequest(request);

            string requestUrl = config.GoogleAppsScriptUrl;
            string body = null;
            if (useGetRequests)
            {
                requestUrl = AppendQueryString(requestUrl, BuildQueryString(request));
            }
            else
            {
                try { body = JsonConvert.SerializeObject(request); }
                catch (JsonException ex) { throw new GoogleSheetsException("INVALID_REQUEST", "The request could not be serialized.", null, ex); }
            }

            using (var webRequest = useGetRequests ? UnityWebRequest.Get(requestUrl) : new UnityWebRequest(config.GoogleAppsScriptUrl, UnityWebRequest.kHttpVerbPOST))
            {
                if (!useGetRequests)
                {
                    webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                }
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.timeout = config.TimeoutSeconds;
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

        private static string BuildQueryString(GoogleSheetsRequest request)
        {
            var parameters = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["action"] = request.action,
                ["requestId"] = request.requestId,
                ["apiKey"] = request.apiKey
            };

            foreach (var entry in FlattenPayload(request.payload))
            {
                parameters[entry.Key] = entry.Value;
            }

            return string.Join("&", parameters
                .Where(pair => !string.IsNullOrEmpty(pair.Value))
                .Select(pair => string.Concat(Uri.EscapeDataString(pair.Key), "=", Uri.EscapeDataString(pair.Value))));
        }

        private static string AppendQueryString(string baseUrl, string query)
        {
            if (string.IsNullOrEmpty(query))
                return baseUrl;

            return baseUrl.Contains("?") ? string.Concat(baseUrl, "&", query) : string.Concat(baseUrl, "?", query);
        }

        private static IEnumerable<KeyValuePair<string, string>> FlattenPayload(object payload)
        {
            if (payload == null)
                yield break;

            if (payload is IDictionary<string, object> objectDictionary)
            {
                foreach (var entry in objectDictionary)
                {
                    yield return new KeyValuePair<string, string>(entry.Key, SerializeQueryValue(entry.Value));
                }
                yield break;
            }

            if (payload is IDictionary<string, string> stringDictionary)
            {
                foreach (var entry in stringDictionary)
                {
                    yield return new KeyValuePair<string, string>(entry.Key, SerializeQueryValue(entry.Value));
                }
                yield break;
            }

            var properties = payload.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.CanRead && property.GetIndexParameters().Length == 0);

            foreach (var property in properties)
            {
                var value = property.GetValue(payload, null);
                yield return new KeyValuePair<string, string>(property.Name, SerializeQueryValue(value));
            }
        }

        private static string SerializeQueryValue(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is string stringValue)
                return stringValue;

            if (value is IEnumerable enumerable && !(value is string))
                return JsonConvert.SerializeObject(value);

            if (value is bool boolValue)
                return boolValue ? bool.TrueString : bool.FalseString;

            if (value is IFormattable formattable)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            return JsonConvert.SerializeObject(value);
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
