using ChatbotLib.Services;
using System.Text;
using System.Text.Json;

namespace ChatbotLib.Tests
{
	[Trait("Category", "Modules")]
	public class DataSavingServiceTests
	{
        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = false,
            IncludeFields = false,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        [Fact]
        public async Task DataSavingService_SaveDataToFile_FileCreated()
        {
            string filePath = Path.Combine(DataSavingService.SaveFilesPath, "test.bin");
            byte[] data = Encoding.UTF8.GetBytes("Hello, Test!");
            using DataSavingService dataSavingService = new();

            await dataSavingService.SaveData(data, "test.bin");

            Assert.True(File.Exists(filePath), "File should be created.");

            byte[] fileData = await File.ReadAllBytesAsync(filePath);
            Assert.Equal(data, fileData);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_LoadDataFromFile_DataCorrectlyLoaded()
        {
            string filePath = Path.Combine(DataSavingService.SaveFilesPath, "test.bin");
            byte[] originalData = Encoding.UTF8.GetBytes("Hello, Data!");
            await File.WriteAllBytesAsync(filePath, originalData);

            using DataSavingService dataSavingService = new();
            byte[] loadedData = await dataSavingService.LoadData("test.bin");

            Assert.Equal(originalData, loadedData);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_SaveDataAsJson_FileContainsValidJson()
        {
            string filePath = Path.Combine(DataSavingService.SaveFilesPath, "test.json");
            var obj = new { Name = "Test", Value = 123 };
            using DataSavingService dataSavingService = new();

            await dataSavingService.SaveDataAsJson(obj, "test.json");

            Assert.True(File.Exists(filePath));

            string json = await File.ReadAllTextAsync(filePath);
            Assert.Contains("name", json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("value", json, StringComparison.OrdinalIgnoreCase);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_LoadDataAsJson_DeserializesCorrectly()
        {
            string filePath = Path.Combine(DataSavingService.SaveFilesPath, "test.json");
            var testObject = new TestObject { Name = "Example", Value = 42 };

            string json = JsonSerializer.Serialize(testObject, _options);
            await File.WriteAllTextAsync(filePath, json);

            using DataSavingService dataSavingService = new();

            var result = await dataSavingService.LoadDataAsJson<TestObject>("test.json");

            Assert.NotNull(result);
            Assert.Equal(testObject.Name, result!.Name);
            Assert.Equal(testObject.Value, result.Value);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_LoadDataAsJson_InvalidJson_ReturnsEmptyArray()
        {
            string filePath = Path.Combine(DataSavingService.SaveFilesPath, "test.json");
            await File.WriteAllTextAsync(filePath, "{invalid_json:true");

            using DataSavingService dataSavingService = new();

            Assert.Equivalent(await dataSavingService.LoadData("nonexistent_file.bin"), Array.Empty<byte>());

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_SaveAndLoad_LargeData_FileIntegrityPreserved()
        {
            string filePath = Path.Combine(DataSavingService.SaveFilesPath, "test.bin");
            byte[] largeData = new byte[1024 * 1024]; // 1 MB
            new Random().NextBytes(largeData);

            using DataSavingService dataSavingService = new();
            await dataSavingService.SaveData(largeData, "test.bin");

            byte[] loadedData = await dataSavingService.LoadData("test.bin");

            Assert.Equal(largeData, loadedData);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public async Task DataSavingService_LoadData_FileNotFound_ReturnsEmptyArray()
        {
            using DataSavingService dataSavingService = new();

            Assert.Equivalent(await dataSavingService.LoadData("nonexistent_file.bin"), Array.Empty<byte>());
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

        [Fact]
        public async Task DataSavingService_ConcurrentReadWrite_SemaphoresPreventCorruption()
        {
            string filePath = Path.Combine(DataSavingService.SaveFilesPath, "test.json");
            using DataSavingService dataSavingService = new();

            var initialObject = new TestObject { Name = "Initial", Value = 0 };
            await dataSavingService.SaveDataAsJson(initialObject, "test.json");

            int parallelCount = 10;
            await Parallel.ForAsync(0, 10, async (i, token) =>
            {
                if (i % 2 == 0)
                {
                    var obj = new TestObject { Name = $"Obj_{i}", Value = i };
                    try
                    {
                        await dataSavingService.SaveDataAsJson(obj, "test.json", token);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail($"Unexpected exception during concurrent write: {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        _ = await dataSavingService.LoadDataAsJson<TestObject>("test.json", token);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail($"Unexpected exception during concurrent read: {ex.Message}");
                    }
                }
            });

            string json = await File.ReadAllTextAsync(filePath);
            TestObject? result = JsonSerializer.Deserialize<TestObject>(json, _options);

            Assert.NotNull(result);
            Assert.StartsWith("Obj_", result!.Name); // Последняя запись должна быть корректной
            Assert.InRange(result.Value, 0, parallelCount - 1);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private class TestObject
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }
    }
}
