using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GSheetsCommander.Samples
{
    /// <summary>A minimal example of common client calls.</summary>
    public sealed class BasicExample : MonoBehaviour
    {
        [SerializeField] private GoogleSheetsConfig config;

        private async void Start()
        {
            try
            {
                var client = new GoogleSheetsClient(config);
                var cell = await client.GetCellAsync("Test", "A1");
                Debug.Log(cell.value);

                await client.SetCellAsync("Test", "A1", "Updated from Unity");
                var range = await client.GetRangeAsync("Test", "A1:B2");
                Debug.Log($"Read {range.values?.Length ?? 0} rows.");
                await client.AppendRowAsync("Test", new object[] { "Example", DateTime.UtcNow.ToString("O") });
            }
            catch (GoogleSheetsException exception)
            {
                Debug.LogError($"{exception.Code}: {exception.Message}");
            }
        }
    }
}
