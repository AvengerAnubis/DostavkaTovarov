using System.Reflection;
using System.Text;
using ChatbotLib;

namespace ChatbotLib.Tests
{
    public class DataSavingServiceTests
    {
        [Fact]
        public async Task DataSavingService_SaveDataToFile_FileCreated()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello, Test!");
            using DataSavingService dataSavingService = new();
            await dataSavingService.SaveData(data, "test.bin");
            string expectedFilePath = $@"{Directory.GetCurrentDirectory()}\test.bin";
            if (File.Exists(expectedFilePath))
            {
                using (FileStream file = File.OpenRead(expectedFilePath))
                {
                    byte[] fileData = new byte[file.Length];
                    await file.ReadExactlyAsync(fileData);
                    Assert.True(data.SequenceEqual(fileData), "File data is wrong");
                }
                
                File.Delete(expectedFilePath);
            }
            else
            {
                Assert.Fail("File doesn't exist");
            }
        }
    }
}
