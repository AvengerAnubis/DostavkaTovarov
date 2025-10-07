using System.Text;
using System.Text.Json;

namespace ChatbotLib
{
    public class DataSavingService : IDisposable
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
                using FileStream file = File.OpenWrite($@"{SaveFilesPath}\{filename}");

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
                using FileStream file = File.OpenRead($@"{SaveFilesPath}\{filename}");

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
            string json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<T>(json, options);
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
