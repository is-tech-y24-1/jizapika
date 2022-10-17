using GeneticAlgo.Shared.Models;

namespace GeneticAlgo.Shared
{
    public interface IExecutionContext
    {
        void Reset();
        Task<IterationResult> ExecuteIterationAsync();
        void ReportStatistics(IStatisticsConsumer statisticsConsumer);
    }
}