using ChatbotLib.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace ChatbotLib.Services
{
    public class DataSavingService : IDisposable, IDataSavingService
    {
        public static string SaveFilesPath => Directory.GetCurrentDirectory();
        protected ConcurrentDictionary<string, SemaphoreSlim> fileLocks = [];

        protected static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = false,
            IncludeFields = false,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        public static JsonSerializerOptions SerializerOptions => options;

        protected async Task<T> LockFileAndExecute<T>(
            string filename, Func<Task<T>> action, CancellationToken token = default)
        {
            var fileLock = fileLocks.GetOrAdd(filename, _ => new SemaphoreSlim(1, 1));
            await fileLock.WaitAsync(token);

            try
            {
                return await action();
            }
            finally
            {
                fileLock.Release();
            }
        }

        #region Сохранение
        public async Task SaveData(byte[] data, string filename, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            await LockFileAndExecute(filename, async () =>
            {
                string fullFilepath = Path.Combine(SaveFilesPath, filename);
                using FileStream file = new(fullFilepath, FileMode.Create, FileAccess.Write, FileShare.None);

                await file.WriteAsync(data, token);
                return (object?)null;
            }, token);
        }
        public async Task SaveDataAsJson(object obj, string filename, CancellationToken token = default)
        {
            string json = JsonSerializer.Serialize(obj, options);
            byte[] data = Encoding.UTF8.GetBytes(json);
            await SaveData(data, filename, token);
        }
        #endregion
        #region Загрузка
        public async Task<byte[]> LoadData(string filename, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            return await LockFileAndExecute(filename, async () =>
            {
                string fullFilepath = Path.Combine(SaveFilesPath, filename);
                using FileStream file = new(fullFilepath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);

                byte[] data = new byte[file.Length];
                await file.ReadExactlyAsync(data, token);

                return data;
            }, token);
        }
        public async Task<T?> LoadDataAsJson<T>(string filename, CancellationToken token = default)
        {
            byte[] data = await LoadData(filename, token);
            if (data.Length == 0)
                return default;
            string json = Encoding.UTF8.GetString(data);
            try
            {
                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (JsonException)
            {
                return default;
            }
        }
        #endregion

        #region Disposing
        protected bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                if (disposing)
                {
                    foreach (var sem in fileLocks.Values)
                        sem.Dispose();

                    fileLocks.Clear();
                }
            }
        }
        #endregion
    }
}
