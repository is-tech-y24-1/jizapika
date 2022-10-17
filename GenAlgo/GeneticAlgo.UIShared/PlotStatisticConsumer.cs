using GeneticAlgo.Shared;
using GeneticAlgo.Shared.Models;
using OxyPlot.Series;

namespace GeneticAlgo.UIShared;

public class PlotStatisticConsumer : IStatisticsConsumer
{
    private readonly ScatterSeries _circleSeries;
    private readonly ScatterSeries _scatterSeries;
    private readonly ScatterSeries _aimSeries;
    private readonly LinearBarSeries _linearBarSeries;

    public PlotStatisticConsumer(ScatterSeries aimSeries, ScatterSeries circleSeries, ScatterSeries scatterSeries, LinearBarSeries linearBarSeries)
    {
        _aimSeries = aimSeries;
        _scatterSeries = scatterSeries;
        _linearBarSeries = linearBarSeries;
        _circleSeries = circleSeries;
    }

    public void Consume(IReadOnlyCollection<Statistic> statistics, IReadOnlyCollection<BarrierCircle> barriers)
    {
        _circleSeries.Points.Clear();
        foreach (var (point, radius) in barriers)
        {
            _circleSeries.Points.Add(new ScatterPoint(point.X, point.Y, radius * 90));
        }

        _aimSeries.Points.Clear();
        _aimSeries.Points.Add(new ScatterPoint(1, 1));

        _scatterSeries.Points.Clear();
        foreach (var statistic in statistics)
        {
            var dotPlace = statistic.Dot.DotPlace;
            _scatterSeries.Points.Add(new ScatterPoint(dotPlace.X, dotPlace.Y));
        }

        _linearBarSeries.ItemsSource = statistics
            .Select(s => new FitnessModel(s.Id, s.Fitness))
            .ToArray();
    }
}