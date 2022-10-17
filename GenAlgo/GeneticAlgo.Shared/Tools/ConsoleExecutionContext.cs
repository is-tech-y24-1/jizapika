using System.Buffers;
using GeneticAlgo.Shared.Entities;
using GeneticAlgo.Shared.Models;

namespace GeneticAlgo.Shared.Tools;

public class ConsoleExecutionContext
{
    private readonly int _populationSize;
    private readonly int _chromosomeSize;
    private readonly int _circleCount;
    private readonly int _plotMaximum;
    private readonly int _plotMinimum;
    private readonly Random _random;
    private DotPopulation _dotPopulation;
    private readonly List<BarrierCircle> _circles;
    private int _currentId;
    private readonly double _aimAura = 0.1;
    private ArrayPool<Gen> _genPool;
    public ConsoleExecutionContext(int populationSize, int chromosomeSize, int circleCount, int plotMaximum, int plotMinimum)
    {
        _genPool = ArrayPool<Gen>.Create(chromosomeSize, populationSize);
        _populationSize = populationSize;
        _chromosomeSize = chromosomeSize;
        _random = Random.Shared;
        _dotPopulation = new DotPopulation(_genPool, _populationSize, _chromosomeSize);
        _circleCount = circleCount;
        _circles = new List<BarrierCircle>();
        _currentId = 0;
        for (int i = 0; i < _circleCount; i++)
        {
            _circles.Add(GeneratedBarrierCircle(_circles));
        }
        _plotMaximum = plotMaximum;
        _plotMinimum = plotMinimum;
    }

    private double Next => _random.NextDouble();

    public Task<IterationResult> ExecuteIterationAsync()
    {
        return Task.FromResult(IterationResult.IterationFinished);
    }
    
    public void MakeIteration()
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