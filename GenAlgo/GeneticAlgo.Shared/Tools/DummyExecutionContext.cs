using System.Buffers;
using GeneticAlgo.Shared.Entities;
using GeneticAlgo.Shared.Models;

namespace GeneticAlgo.Shared.Tools;

public class DummyExecutionContext : IExecutionContext
{
    private readonly int _circleCount;
    private readonly int _populationSize;
    private readonly int _chromosomeSize;
    private readonly Random _random;
    private const double _aimAura = 0.1;
    private DotPopulation _dotPopulation;
    private int _currentId = 0;
    private List<BarrierCircle> _circles;
    private int _plotMaximum;
    private int _plotMinimum;
    private ArrayPool<Gen> _genPool;

    public DummyExecutionContext(
        int populationSize, int chromosomeSize, int circleCount, int plotMaximum, int plotMinimum)
    {
        _genPool = ArrayPool<Gen>.Create(chromosomeSize, populationSize);
        _populationSize = populationSize;
        _chromosomeSize = chromosomeSize;
        _circleCount = circleCount;
        _random = Random.Shared;
        _dotPopulation = new DotPopulation(_genPool, _populationSize, _chromosomeSize);
        _circles = new List<BarrierCircle>();
        _plotMaximum = plotMaximum;
        _plotMinimum = plotMinimum;
        for (int i = 0; i < _circleCount; i++)
        {
            _circles.Add(GeneratedBarrierCircle(_circles));
        }
    }

    private double Next => _random.NextDouble();

    public void Reset() { }

    public Task<IterationResult> ExecuteIterationAsync()
    {
        return Task.FromResult(IterationResult.IterationFinished);
    }

    public void ReportStatistics(IStatisticsConsumer statisticsConsumer)
    {
        var aim = new Aim(new Coordinates2D(1, 1), _aimAura);
        if (_currentId >= _populationSize)
        {
            _currentId = 0;
            _dotPopulation = new DotPopulation(_genPool, _dotPopulation, 0.04, aim);
        }
        else
        {
            _dotPopulation.MakeIteration(_circles, aim, _plotMaximum, _plotMinimum);
        }
        
        Statistic[] statistics = Enumerable.Range(0, _populationSize)
            .Select(id => new Statistic(id, _dotPopulation.Dots[id], _dotPopulation.Dots[id].FitnessFunction(aim)))
            .ToArray();

        statisticsConsumer.Consume(statistics, _circles);
        _currentId++;
    }

    private BarrierCircle GeneratedBarrierCircle(List<BarrierCircle> circles)
    {
        var aim = new Coordinates2D(1, 1);
        var start = new Coordinates2D(0, 0);
        BarrierCircle newCircle;
        do
        {
            newCircle = new BarrierCircle(new Coordinates2D(Next, Next), Next / 5);
        } while (circles.Any(bc => AreCrossCircles(newCircle, bc))
                 || IsCrossPoint(newCircle, aim)
                 || IsCrossPoint(newCircle, start));

        return newCircle;
    }

    private static bool AreCrossCircles(BarrierCircle circle1, BarrierCircle circle2) =>
        Math.Sqrt(Math.Pow(circle1.Center.X - circle2.Center.X, 2) + Math.Pow(circle1.Center.Y - circle2.Center.Y, 2))
        < circle1.Radius + circle2.Radius;
    
    private static bool IsCrossPoint(BarrierCircle circle, Coordinates2D point) =>
        Math.Sqrt(Math.Pow(circle.Center.X - point.X, 2) + Math.Pow(circle.Center.Y - point.Y, 2))
        < circle.Radius;
}