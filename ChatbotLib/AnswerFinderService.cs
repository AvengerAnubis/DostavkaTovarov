using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuzzySharp;

namespace ChatbotLib
{
    public class AnswerFinderService : IDisposable
    {
        protected QuestionAnswerHierarchyNode answerHierarchy;
        protected DataSavingService savingService;

        public AnswerFinderService(DataSavingService savingService)
        {
            var loadedHierarchy = savingService.LoadDataAsJson<QuestionAnswerHierarchyNode>("qa_hierarchy").Result;
            if (loadedHierarchy is not null)
                answerHierarchy = loadedHierarchy;
            else
                answerHierarchy = new();
            this.savingService = savingService;
        }

        #region Disposing
        protected bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {

                }
            }
        }
        #endregion
    }
}
