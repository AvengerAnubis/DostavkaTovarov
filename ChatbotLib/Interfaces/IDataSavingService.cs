using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotLib.Interfaces
{
    public interface IDataSavingService
    {
        Task SaveData(byte[] data, string filename, CancellationToken token = default);
        Task SaveDataAsJson(object obj, string filename, CancellationToken token = default);
        Task<byte[]> LoadData(string filename, CancellationToken token = default);
        Task<T?> LoadDataAsJson<T>(string filename, CancellationToken token = default);
    }
}
