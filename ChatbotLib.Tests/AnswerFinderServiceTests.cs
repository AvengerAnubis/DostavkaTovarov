using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using ChatbotLib.Services;
using Moq;

namespace ChatbotLib.Tests
{
    [Trait("Category", "Modules")]
    public class AnswerFinderServiceTests
    {
        private static QuestionAnswerNode BuildHierarchy()
        {
            var root = new QuestionAnswerNode
            {
                Question = "Как контрол срок?",
                QuestionContexted = "Как контрол срок их",
                Answer = "Сроки контролируются автоматически системой учёта.",
                ContextChildren =
                [
                    new QuestionAnswerNode
                    {
                        Question = "Почему контрол срок важн",
                        QuestionContexted = "Почему он это важн",
                        Answer = "Потому что несоблюдение сроков ведёт к срыву поставки."
                    },
                    new QuestionAnswerNode
                    {
                        Question = "Какая систем контрол срок",
                        QuestionContexted = "Какая систем контрол их его",
                        Answer = "Используется внутренняя система мониторинга сроков."
                    }
                ]
            };
            return root;
        }

        private static Mock<IDataSavingService> CreateMockSavingService(QuestionAnswerNode? hierarchy = null)
        {
            var mock = new Mock<IDataSavingService>();

            // Метод для загрузки данных
            mock.Setup(s => s.LoadDataAsJson<List<QuestionAnswerNode>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((hierarchy is not null) ? [hierarchy] : []);

            return mock;
        }

        [Fact]
        public void AnswerFinderService_Constructor_FileExists_LoadsHierarchy()
        {
            var hierarchy = BuildHierarchy();

            IDataSavingService savingService = CreateMockSavingService(hierarchy).Object;
            var finder = new AnswerFinderService(savingService);

            Assert.NotNull(finder);
        }

        [Fact]
        public void AnswerFinderService_Constructor_FileMissing_CreatesEmptyHierarchy()
        {
            IDataSavingService savingService = CreateMockSavingService().Object;
            var finder = new AnswerFinderService(savingService);
            Assert.NotNull(finder);
        }

        // --- Контекст и его применение ---

        [Fact]
        public async Task AnswerFinderService_ApplyContext_ValidNode_SetsContextSuccessfully()
        {
            var hierarchy = BuildHierarchy();
            IDataSavingService savingService = CreateMockSavingService(hierarchy).Object;
            var finder = new AnswerFinderService(savingService);

            finder.ApplyContext(hierarchy);

            // Проверяем, что теперь поиск в контексте отдаёт этот узел
            List<AnswerFinderResult> result = [.. await finder.FindAnswerNode("Почему это важно?", searchInContext: true)];
            Assert.NotNull(result[0].FoundNode);
            Assert.Contains("срок", result[0].FoundNode.Answer, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AnswerFinderService_ApplyContext_NodeNotInHierarchy_DoesNotChangeContext()
        {
            var hierarchy = BuildHierarchy();
            IDataSavingService savingService = CreateMockSavingService(hierarchy).Object;
            var finder = new AnswerFinderService(savingService);

            var invalidNode = new QuestionAnswerNode
            {
                Question = "Несуществующий",
                Answer = "Такого нет"
            };

            finder.ApplyContext(invalidNode); // не должен сработать

            // Поиск всё ещё идёт по исходной иерархии
            List<AnswerFinderResult> result = [..await finder.FindAnswerNode("Какая система контролирует сроки?", searchInContext: true)];
            Assert.NotNull(result[0].FoundNode);
            Assert.Contains("систем", result[0].FoundNode.Answer, StringComparison.OrdinalIgnoreCase);
        }

        // --- Контекстный поиск (обновлённые тесты) ---

        [Fact]
        public async Task AnswerFinderService_FindAnswerNode_ContextMatch_PrefersContextResult()
        {
            var hierarchy = BuildHierarchy();
            IDataSavingService savingService = CreateMockSavingService(hierarchy).Object;
            var finder = new AnswerFinderService(savingService);

            // Устанавливаем контекст на корневой вопрос
            finder.ApplyContext(hierarchy);

            List<AnswerFinderResult> result = [..await finder.FindAnswerNode("какая система контролирует сроки?", searchInContext: true, minScoreForContext: 80)];

            Assert.NotNull(result[0].FoundNode);
            Assert.Contains("систем", result[0].FoundNode.Answer, StringComparison.OrdinalIgnoreCase);
            Assert.InRange(result[0].Score, 80, 100);
        }

        [Fact]
        public async Task AnswerFinderService_FindAnswerNode_NonContextMatch_FallsBackToGlobalSearch()
        {
            var hierarchy = BuildHierarchy();
            IDataSavingService savingService = CreateMockSavingService(hierarchy).Object;
            var finder = new AnswerFinderService(savingService);

            finder.ApplyContext(hierarchy); // контекст: "Почему это важно?"

            // Этот вопрос не в контексте, значит, поиск должен идти по всем узлам
            List<AnswerFinderResult> result = [.. await finder.FindAnswerNode("Как контролируются сроки?", searchInContext: true, minScoreForContext: 80)];

            Assert.NotNull(result[0].FoundNode);
            Assert.Contains("сроки", result[0].FoundNode.Answer, StringComparison.OrdinalIgnoreCase);
            Assert.InRange(result[0].Score, 80, 100);
        }

        [Fact]
        public async Task AnswerFinderService_FindAnswerNode_RespectsCancellationToken()
        {
            var hierarchy = BuildHierarchy();
            IDataSavingService savingService = CreateMockSavingService(hierarchy).Object;
            var finder = new AnswerFinderService(savingService);
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await finder.FindAnswerNode("Как контролируются сроки?", token: cts.Token);
            });
        }
    }
}
