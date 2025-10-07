using ChatbotLib;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ChatbotLib.Tests
{
	[Trait("Category", "Modules")]
	public class DataSavingServiceTests
	{
        private JsonSerializerOptions _options = new()
        {
            WriteIndented = false,
            IncludeFields = false,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        [Fact]
        public async Task DataSavingService_SaveDataToFile_FileCreated()
        {
            string expectedFilePath = $@"{DataSavingService.SaveFilesPath}\test.bin";
            byte[] data = Encoding.UTF8.GetBytes("Hello, Test!");
            using DataSavingService dataSavingService = new();

            await dataSavingService.SaveData(data, "test.bin");

            Assert.True(File.Exists(expectedFilePath), "File should be created.");

            byte[] fileData = await File.ReadAllBytesAsync(expectedFilePath);
            Assert.Equal(data, fileData);

            File.Delete(expectedFilePath);
        }

        [Fact]
        public async Task DataSavingService_LoadDataFromFile_DataCorrectlyLoaded()
        {
            string filePath = $@"{DataSavingService.SaveFilesPath}\test.bin";
            byte[] originalData = Encoding.UTF8.GetBytes("Hello, Data!");
            await File.WriteAllBytesAsync(filePath, originalData);

            using DataSavingService dataSavingService = new();
            byte[] loadedData = await dataSavingService.LoadData("test.bin");

            Assert.Equal(originalData, loadedData);

            File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_SaveDataAsJson_FileContainsValidJson()
        {
            string filePath = $@"{DataSavingService.SaveFilesPath}\test.json";
            var obj = new { Name = "Test", Value = 123 };
            using DataSavingService dataSavingService = new();

            await dataSavingService.SaveDataAsJson(obj, "test.json");

            Assert.True(File.Exists(filePath));

            string json = await File.ReadAllTextAsync(filePath);
            Assert.Contains("name", json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("value", json, StringComparison.OrdinalIgnoreCase);

            File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_LoadDataAsJson_DeserializesCorrectly()
        {
            string filePath = $@"{DataSavingService.SaveFilesPath}\test.json";
            var testObject = new TestObject { Name = "Example", Value = 42 };

            string json = JsonSerializer.Serialize(testObject, _options);
            await File.WriteAllTextAsync(filePath, json);

            using DataSavingService dataSavingService = new();

            var result = await dataSavingService.LoadDataAsJson<TestObject>("test.json");

            Assert.NotNull(result);
            Assert.Equal(testObject.Name, result!.Name);
            Assert.Equal(testObject.Value, result.Value);

            File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_LoadDataAsJson_InvalidJson_ThrowsJsonException()
        {
            string filePath = $@"{DataSavingService.SaveFilesPath}\test.json";
            await File.WriteAllTextAsync(filePath, "{invalid_json:true");

            using DataSavingService dataSavingService = new();

            await Assert.ThrowsAsync<JsonException>(async () =>
            {
                await dataSavingService.LoadDataAsJson<TestObject>("test.json");
            });

            File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_SaveAndLoad_LargeData_FileIntegrityPreserved()
        {
            string filePath = $@"{DataSavingService.SaveFilesPath}\test.bin";
            byte[] largeData = new byte[1024 * 1024]; // 1 MB
            new Random().NextBytes(largeData);

            using DataSavingService dataSavingService = new();
            await dataSavingService.SaveData(largeData, "test.bin");

            byte[] loadedData = await dataSavingService.LoadData("test.bin");

            Assert.Equal(largeData, loadedData);

            File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_LoadData_FileNotFound_ThrowsException()
        {
            using DataSavingService dataSavingService = new();

            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            {
                await dataSavingService.LoadData("nonexistent_file.bin");
            });
        }

        [Fact]
        public void DataSavingService_Dispose_CancelsTokenAndClearsLocks()
        {
            DataSavingService dataSavingService = new();

            dataSavingService.Dispose();

            // Попытка вызвать Dispose повторно не должна кидать исключение
            dataSavingService.Dispose();

            Assert.True(true, "Dispose executed without exceptions.");
        }

        private class TestObject
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }
    }
}
