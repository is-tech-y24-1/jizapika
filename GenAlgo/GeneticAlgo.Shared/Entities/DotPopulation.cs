using System.Buffers;
using GeneticAlgo.Shared.Models;

namespace GeneticAlgo.Shared.Entities;

public class DotPopulation
{
    public Dot[] Dots { get; }
    private int _populationSize;

    public DotPopulation(ArrayPool<Gen> genPool, int populationSize, int chromosomeSize)
    {
        _populationSize = populationSize;
        Dots = new Dot[populationSize];
        for (var i = 0; i < populationSize; i++) Dots[i] = new Dot(genPool, chromosomeSize);
    }

    public DotPopulation(ArrayPool<Gen> genPool, DotPopulation prevPopulation, double genMutationProbability, Aim aim)
    {
        _populationSize = prevPopulation._populationSize;
        Dots = prevPopulation.Dots;
        for (var i = 0; i < _populationSize; i++)
        {
            Dots[i] = new Dot(prevPopulation.GetTheAncestor(aim).InheritChromosome(genMutationProbability, genPool));
        }
    }

    public void MakeIteration(List<BarrierCircle> circles, Aim aim, int plotMaximum, int plotMinimum)
    {
        for (var i = 0; i < _populationSize; i++)
        {
            Dots[i].MakeIteration(circles, aim, plotMaximum, plotMinimum);
        }
    }

    private Dot GetTheAncestor(Aim aim)
    {
        var prefixSumFitnessList = new double[_populationSize];
        double prefixSum = 0;
        for (int i = 0; i < _populationSize; i++)
        {
            prefixSum += Dots[i].FitnessFunction(aim);
            prefixSumFitnessList[i] = prefixSum;
        }

        double choosingNumber = Random.Shared.NextDouble() * prefixSum;
        int ancestorId = Array.BinarySearch(prefixSumFitnessList.ToArray(), choosingNumber);
        return Dots[ancestorId < 0 ? ~ancestorId : ancestorId];
    }
}