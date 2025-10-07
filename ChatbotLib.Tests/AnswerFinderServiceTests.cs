using ChatbotLib.DataObjects;
using ChatbotLib.Services;
using System.Text.Json;

namespace ChatbotLib.Tests
{
    [Trait("Category", "Modules")]
    public class AnswerFinderServiceTests
    {
        private static string FilePath => Path.Combine(Directory.GetCurrentDirectory(), "qa_hierarchy.json");

        private static QuestionAnswerNode BuildHierarchy()
        {
            var root = new QuestionAnswerNode
            {
                Question = "Как контролируются сроки?",
                QuestionContexted = "Контроль сроков",
                Answer = "Сроки контролируются автоматически системой учёта.",
                ContextChildren =
                [
                    new QuestionAnswerNode
                    {
                        Question = "Почему это важно?",
                        QuestionContexted = "Почему контроль сроков важен?",
                        Answer = "Потому что несоблюдение сроков ведёт к срыву поставки."
                    },
                    new QuestionAnswerNode
                    {
                        Question = "Какая система контролирует сроки?",
                        QuestionContexted = "Система контроля сроков",
                        Answer = "Используется внутренняя система мониторинга сроков."
                    }
                ]
            };
            return root;
        }

        private static async Task WriteHierarchyFileAsync(QuestionAnswerNode node)
        {
            string json = JsonSerializer.Serialize(node, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            await File.WriteAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "qa_hierarchy.json"), json);
        }

        private static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        [Fact]
        public async Task AnswerFinderService_Constructor_FileExists_LoadsHierarchy()
        {
            var hierarchy = BuildHierarchy();
            await WriteHierarchyFileAsync(hierarchy);

            using var savingService = new DataSavingService();
            using var finder = new AnswerFinderService(savingService);

            Assert.NotNull(finder);
            DeleteFileIfExists(FilePath);
        }

        [Fact]
        public void AnswerFinderService_Constructor_FileMissing_CreatesEmptyHierarchy()
        {
            DeleteFileIfExists(FilePath);
            using var savingService = new DataSavingService();
            using var finder = new AnswerFinderService(savingService);
            Assert.NotNull(finder);
        }

        // --- Контекст и его применение ---

        [Fact]
        public async Task AnswerFinderService_ApplyContext_ValidNode_SetsContextSuccessfully()
        {
            var hierarchy = BuildHierarchy();
            await WriteHierarchyFileAsync(hierarchy);
            using var savingService = new DataSavingService();
            using var finder = new AnswerFinderService(savingService);

            // Берём дочерний узел
            var childNode = hierarchy.ContextChildren.First();
            finder.ApplyContext(childNode);

            // Проверяем, что теперь поиск в контексте отдаёт этот узел
            var result = await finder.FindAnswerNode("Почему это важно?", searchInContext: true);
            Assert.NotNull(result.FoundNode);
            Assert.Equal(childNode.Answer, result.FoundNode.Answer);

            DeleteFileIfExists(FilePath);
        }

        [Fact]
        public async Task AnswerFinderService_ApplyContext_NodeNotInHierarchy_DoesNotChangeContext()
        {
            var hierarchy = BuildHierarchy();
            await WriteHierarchyFileAsync(hierarchy);
            using var savingService = new DataSavingService();
            using var finder = new AnswerFinderService(savingService);

            var invalidNode = new QuestionAnswerNode
            {
                Question = "Несуществующий",
                Answer = "Такого нет"
            };

            finder.ApplyContext(invalidNode); // не должен сработать

            // Поиск всё ещё идёт по исходной иерархии
            var result = await finder.FindAnswerNode("Какая система контролирует сроки?", searchInContext: true);
            Assert.NotNull(result.FoundNode);
            Assert.Contains("система", result.FoundNode.Answer, StringComparison.OrdinalIgnoreCase);

            DeleteFileIfExists(FilePath);
        }

        // --- Контекстный поиск (обновлённые тесты) ---

        [Fact]
        public async Task AnswerFinderService_FindAnswerNode_ContextMatch_PrefersContextResult()
        {
            var hierarchy = BuildHierarchy();
            await WriteHierarchyFileAsync(hierarchy);
            using var savingService = new DataSavingService();
            using var finder = new AnswerFinderService(savingService);

            // Устанавливаем контекст на корневой вопрос
            finder.ApplyContext(hierarchy);

            var result = await finder.FindAnswerNode("какая система контролирует сроки?", searchInContext: true, minScoreForContext: 60);

            Assert.NotNull(result.FoundNode);
            Assert.Contains("система", result.FoundNode.Question, StringComparison.OrdinalIgnoreCase);
            Assert.InRange(result.Score, 80, 100);

            DeleteFileIfExists(FilePath);
        }

        [Fact]
        public async Task AnswerFinderService_FindAnswerNode_NonContextMatch_FallsBackToGlobalSearch()
        {
            var hierarchy = BuildHierarchy();
            await WriteHierarchyFileAsync(hierarchy);
            using var savingService = new DataSavingService();
            using var finder = new AnswerFinderService(savingService);

            finder.ApplyContext(hierarchy.ContextChildren.First()); // контекст: "Почему это важно?"

            // Этот вопрос не в контексте, значит, поиск должен идти по всем узлам
            var result = await finder.FindAnswerNode("Как контролируются сроки?", searchInContext: true, minScoreForContext: 90);

            Assert.NotNull(result.FoundNode);
            Assert.Contains("сроки", result.FoundNode.Answer, StringComparison.OrdinalIgnoreCase);
            Assert.InRange(result.Score, 80, 100);

            DeleteFileIfExists(FilePath);
        }

        // --- Остальные (без изменений) ---

        [Fact]
        public async Task AnswerFinderService_FindAnswerNode_InvalidJson_UsesEmptyHierarchy()
        {
            await File.WriteAllTextAsync(FilePath, "{invalid_json:true");
            using var savingService = new DataSavingService();
            using var finder = new AnswerFinderService(savingService);
            var result = await finder.FindAnswerNode("Любой вопрос");
            Assert.NotNull(result.FoundNode);
            DeleteFileIfExists(FilePath);
        }

        [Fact]
        public async Task AnswerFinderService_FindAnswerNode_RespectsCancellationToken()
        {
            var hierarchy = BuildHierarchy();
            await WriteHierarchyFileAsync(hierarchy);
            using var savingService = new DataSavingService();
            using var finder = new AnswerFinderService(savingService);
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await finder.FindAnswerNode("Как контролируются сроки?", token: cts.Token);
            });
            DeleteFileIfExists(FilePath);
        }

        [Fact]
        public void AnswerFinderService_Dispose_CancelsAndReleases()
        {
            using var savingService = new DataSavingService();
            var finder = new AnswerFinderService(savingService);
            finder.Dispose();
            finder.Dispose(); // повторный вызов не должен кидать исключение
            Assert.True(true);
        }
    }
}
