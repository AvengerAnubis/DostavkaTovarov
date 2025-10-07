using ChatbotLib.Interfaces;
using System.Text;
using System.Text.Json;

namespace ChatbotLib.Services
{
    public class DataSavingService : IDisposable, IDataSavingService
    {
        public static string SaveFilesPath => Directory.GetCurrentDirectory();
        protected Dictionary<string, SemaphoreSlim> fileLocks = [];

        protected CancellationTokenSource sharedCts = new();
        protected JsonSerializerOptions options = new()
        {
            WriteIndented = false,
            IncludeFields = false,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        #region Сохранение
        public async Task SaveData(byte[] data, string filename, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var registerToken = token.Register(() => sharedCts.Cancel());

            SemaphoreSlim fileLock;
            if (fileLocks.TryGetValue(filename, out SemaphoreSlim? value))
                fileLock = value;
            else
            {
                fileLock = new SemaphoreSlim(1, 1);
                fileLocks.Add(filename, fileLock);
            }

            await fileLock.WaitAsync(sharedCts.Token);
            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);
                using FileStream file = File.OpenWrite(Path.Combine(SaveFilesPath, filename));

                await file.WriteAsync(data, sharedCts.Token);
            }
            finally
            {
                registerToken.Unregister();
                fileLock.Release();
            }
        }
        public async Task SaveDataAsJson(object obj, string filename, CancellationToken token = default)
        {
            string json = JsonSerializer.Serialize(obj, options);
            byte[] data = Encoding.UTF8.GetBytes(json);
            await SaveData(data, filename, sharedCts.Token);
        }
        #endregion
        #region Загрузка
        public async Task<byte[]> LoadData(string filename, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var registerToken = token.Register(() => sharedCts.Cancel());

            SemaphoreSlim fileLock;
            if (fileLocks.TryGetValue(filename, out SemaphoreSlim? value))
                fileLock = value;
            else
            {
                fileLock = new SemaphoreSlim(1, 1);
                fileLocks.Add(filename, fileLock);
            }

            await fileLock.WaitAsync(sharedCts.Token);
            try
            {
                if (!File.Exists(Path.Combine(SaveFilesPath, filename)))
                {
                    await File.WriteAllBytesAsync(Path.Combine(SaveFilesPath, filename), [], sharedCts.Token);
                    return [];
                }
                using FileStream file = File.OpenRead(Path.Combine(SaveFilesPath, filename));

                byte[] data = new byte[file.Length];
                await file.ReadExactlyAsync(data, sharedCts.Token);

                return data;
            }
            finally
            {
                registerToken.Unregister();
                fileLock.Release();
            }
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
                    fileLocks.Clear();
                    sharedCts.Cancel();
                    sharedCts.Dispose();
                }
            }
        }
        #endregion
    }
}
