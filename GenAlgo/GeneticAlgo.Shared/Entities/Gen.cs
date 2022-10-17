namespace GeneticAlgo.Shared.Entities;

public class Gen
{
    private const double MaxPower = 0.001;
    public readonly double DirectionAngle = Random.Shared.NextDouble() * 2 * Math.PI;
    public readonly double Power = Random.Shared.NextDouble() * MaxPower;
}