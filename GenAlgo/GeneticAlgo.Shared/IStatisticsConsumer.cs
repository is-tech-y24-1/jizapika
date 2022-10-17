using GeneticAlgo.Shared.Models;

namespace GeneticAlgo.Shared;

public interface IStatisticsConsumer
{
    void Consume(IReadOnlyCollection<Statistic> statistics, IReadOnlyCollection<BarrierCircle> barriers);
}