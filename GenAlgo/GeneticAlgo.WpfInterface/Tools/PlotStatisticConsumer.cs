using System.Collections.Generic;
using System.Linq;
using GeneticAlgo.Shared;
using GeneticAlgo.Shared.Models;
using OxyPlot.Series;

namespace GeneticAlgo.WpfInterface.Tools;

public class PlotStatisticConsumer : IStatisticsConsumer
{
    private readonly ScatterSeries _circleSeries;
    private readonly ScatterSeries _scatterSeries;
    private readonly LinearBarSeries _linearBarSeries;

    public PlotStatisticConsumer(ScatterSeries circleSeries, ScatterSeries scatterSeries, LinearBarSeries linearBarSeries)
    {
        _circleSeries = circleSeries;
        _scatterSeries = scatterSeries;
        _linearBarSeries = linearBarSeries;
    }

    public void Consume(IReadOnlyCollection<Statistic> statistics, IReadOnlyCollection<BarrierCircle> barriers)
    {
        _scatterSeries.Points.Clear();

        foreach (var statistic in statistics)
        {
            var point = statistic.Point;
            _scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
        }

        _circleSeries.Points.Clear();

        foreach (var (point, radius) in barriers)
        {
            _circleSeries.Points.Add(new ScatterPoint(point.X, point.Y, radius));
        }

        _linearBarSeries.ItemsSource = statistics
            .Select(s => new FitnessModel(s.Id, s.Fitness))
            .ToArray();
    }
}