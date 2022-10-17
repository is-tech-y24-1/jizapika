using System.Buffers;

namespace GeneticAlgo.Shared.Entities;

public class Chromosome
{
    public Gen[] Gens;
    public int _chromosomeSize;

    public Chromosome(Gen[] gens, int chromosomeSize)
    {
        Gens = gens;
        _chromosomeSize = chromosomeSize;
    }

    public Chromosome(ArrayPool<Gen> pool, int chromosomeSize)
    {
        _chromosomeSize = chromosomeSize;
        Gens = pool.Rent(chromosomeSize);
        for (var i = 0; i < chromosomeSize; i++) Gens[i] = new Gen();
    }

    public Chromosome GetMutation(ArrayPool<Gen> pool, double genMutationProbability)
    {
        for (int i = 0; i < _chromosomeSize; i++)
        {
            if (Random.Shared.NextDouble() < genMutationProbability) Gens[i] = new Gen();
        }

        return new Chromosome(Gens, _chromosomeSize);
    }

    public void Delete(ArrayPool<Gen> pool)
    {
        pool.Return(Gens);
    }
}