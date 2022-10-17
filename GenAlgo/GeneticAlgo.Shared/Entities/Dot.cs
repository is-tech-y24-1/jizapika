using System.Buffers;
using GeneticAlgo.Shared.Models;

namespace GeneticAlgo.Shared.Entities;

public class Dot
{
    private Coordinates2D _dotVelocity;
    private Coordinates2D _dotAcceleration;
    private int _currentIteration;

    public Dot(Chromosome chromosome)
    {
        Chromosome = chromosome;
        DotPlace = new Coordinates2D(0, 0);
        _dotVelocity = new Coordinates2D(0, 0);
        _dotAcceleration = new Coordinates2D(0, 0);
        _currentIteration = 0;
    }

    public Dot(ArrayPool<Gen> genPool, int chromosomeSize)
    {
        Chromosome = new Chromosome(genPool, chromosomeSize);
        DotPlace = new Coordinates2D(0, 0);
        _dotVelocity = new Coordinates2D(0, 0);
        _dotAcceleration = new Coordinates2D(0, 0);
        _currentIteration = 0;
    }

    private Chromosome Chromosome { get; }
    public Coordinates2D DotPlace { get; private set; }

    public void MakeIteration(List<BarrierCircle> circles, Aim aim, int plotMaximum, int plotMinimum)
    {
        if (circles.Any(circle => IsIn(DotPlace, circle))
            || DotPlace.X > plotMaximum || DotPlace.X < plotMinimum
            || DotPlace.Y > plotMaximum || DotPlace.Y < plotMinimum)
        {
            _currentIteration++;
            return;
        }

        if (Distance(DotPlace, aim.Center) < aim.Radius) return;
        if (_currentIteration >= Chromosome._chromosomeSize) return;

        var currentGen = Chromosome.Gens[_currentIteration];
        var accelerationIncrease = new Coordinates2D(
            currentGen.Power * Math.Cos(currentGen.DirectionAngle),
            currentGen.Power * Math.Sin(currentGen.DirectionAngle));
        _dotAcceleration = SumOfCoordinates2D(_dotAcceleration, accelerationIncrease);
        _dotVelocity = SumOfCoordinates2D(_dotVelocity, _dotAcceleration);
        DotPlace = SumOfCoordinates2D(DotPlace, _dotVelocity);
        _currentIteration++;
    }

    private static Coordinates2D SumOfCoordinates2D(Coordinates2D first, Coordinates2D second) =>
        new (first.X + second.X, first.Y + second.Y);

    private static double Distance(Coordinates2D firstPoint, Coordinates2D secondPoint) =>
        Math.Sqrt(Math.Pow(firstPoint.X - secondPoint.X, 2) + Math.Pow(firstPoint.Y - secondPoint.Y, 2));
    public Chromosome InheritChromosome(double genMutationProbability, ArrayPool<Gen> pool) 
        => Chromosome.GetMutation(pool, genMutationProbability);

    public double FitnessFunction(Aim aim)
    {
        const double fitnessMax = 1;
        var distance = Math.Max(1, Distance(DotPlace, aim.Center) / aim.Radius);
        return fitnessMax / Math.Pow(distance, 2) / (_currentIteration + 1);
    }

    private static bool IsIn(Coordinates2D dot, BarrierCircle circle)
        => Distance(dot, circle.Center) < circle.Radius;
}