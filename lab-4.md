## Задание
Есть материальная точка, которая находится в точке (0, 0) на плоскости. Точка поддаётся законам механики (у точки есть скорость, а прикладывание силы создаёт ускорение); к ней каждые dt единиц времени прикладывается вектор силы, чтобы повлиять на траекторию движения.

Есть материальная точка, которая находится в точке (0, 0) на плоскости. В каждый момент времени точку можно сдвигать на определённый вектор. Требуется за минимальное количество движений переместиться в позицию (1, 1). При этом нельзя выходить за пределы квадрата.
## Решение
Основопологающим для поиска решения является ген. Ген обозначает направление и силу ускорения, направленные на точку в конкретный момент времени. Этот момент зависит от местоположения в хромосоме. Ген генерируется случайно и может передаваться из одной хромосомы в другую. Хромосома это набор генов, который нужен для сохранения решения задачи точкой.

### Gen.cs
```cs
public class Gen
{
    private const double MaxPower = 0.001;
    public readonly double DirectionAngle = Random.Shared.NextDouble() * 2 * Math.PI;
    public readonly double Power = Random.Shared.NextDouble() * MaxPower;
}
```

### Chromosome.cs
```cs
public class Chromosome
{
    private List<Gen> _gens;

    public Chromosome(List<Gen> gens)
    {
        _gens = gens;
    }

    public Chromosome(int chromosomeSize)
    {
        _gens = Enumerable.Range(0, chromosomeSize)
            .Select(_ => new Gen()).ToList();
    }

    public ImmutableList<Gen> Gens  => _gens.ToImmutableList();

    public Chromosome GetMutation(double genMutationProbability) =>
        new(
            _gens.Select(gen => Random.Shared.NextDouble() < genMutationProbability
                    ? new Gen()
                    : gen).ToList()
        );
}
```

Мы можем увидеть у хромосом метод GetMutation. Что это такое? Хромосома может изменяться под влиянием случайных факторов при передаче от одной точки, к другой (наследнику). Это нужно для изменчивости организмов (точек), потому что иначе новых способов решения задачи, а, соответственно, и улучшения этих решений не найти. Точки сохраняют в себе хромосомы и путешествуют, меняя только своё ускорение.

Когда становится задача улучшения решения, встаёт вопрос не только о мутации и переборе различных способов решения, но и сохранения лучших решений. Ведь гораздо оптимальнее мутировать лучшие решения, чем случайные. Для этого нам понадобится DotPopulation и FitnessFunction.

Фитнесс-функция нужна для определения реально лучшего значения. У нас есть два конечных результата точек. И теперь нужно выбрать ту, которая лучше справилась с решением. Фитнесс-функция назначает число, характеризующее успех конкретной точки.
```cs
public double FitnessFunction(Aim aim)
{
    const double fitnessMax = 1;
    var distance = Math.Max(1, Distance(DotPlace, aim.Center) / aim.Radius);
    return fitnessMax / Math.Pow(distance, 2) / (_currentIteration + 1);
}
```
В приоритете у неё стоят точки, которые достигли цель. Расстояние получило обратную квадратную зависимость, а количество итераций просто обратную, потому что нам важнее приближение точки к цели (а лучше даже попадание), чем количество итераций, которые она на это потратила. Но важно учесть обе этих составляющие, чтобы достичь точного и оптимального решения.

А популяция точек нам нужна для большого поля для разнообразия. Чем больше у нас точек скопировало чью-то хорошую хромосому, тем менее страшно терять её под влиянием мутации. Популяция является важнейшим фактором соревнования между точками и попытках заполучить максимальное значение фитнесс-функции, ведь те, кто получают маленькое значение, в итоге не размножаются.
### DotPopulation.cs
```cs
using GeneticAlgo.Shared.Models;

namespace GeneticAlgo.Shared.Entities;

public class DotPopulation
{
    public List<Dot> Dots { get; }
    private int _populationSize;

    public DotPopulation(int populationSize, int chromosomeSize)
    {
        _populationSize = populationSize;
        Dots = new List<Dot>();
        for (int i = 0; i < populationSize; i++)
        {
            Dots.Add(new Dot(chromosomeSize));
        }
    }

    public DotPopulation(DotPopulation prevPopulation, double genMutationProbability, Aim aim)
    {
        _populationSize = prevPopulation._populationSize;
        Dots = new List<Dot>();
        for (var i = 0; i < _populationSize; i++)
        {
            Dots.Add(new Dot(prevPopulation.GetTheAncestor(aim).InheritChromosome(genMutationProbability)));
        }
    }

    public void MakeIteration(List<BarrierCircle> circles, Aim aim, int plotMaximum, int plotMinimum)
    {
        foreach (var dot in Dots)
        {
            dot.MakeIteration(circles, aim, plotMaximum, plotMinimum);
        }
    }

    private Dot GetTheAncestor(Aim aim)
    {
        var prefixSumFitnessList = new List<double>();
        double prefixSum = 0;
        foreach (var dot in Dots)
        {
            prefixSum += dot.FitnessFunction(aim);
            prefixSumFitnessList.Add(prefixSum);
        }

        double choosingNumber = Random.Shared.NextDouble() * prefixSum;
        int ancestorId = Array.BinarySearch(prefixSumFitnessList.ToArray(), choosingNumber);
        return Dots[ancestorId < 0 ? ~ancestorId : ancestorId];
    }
}
```
Метод GetTheAncestor помогает выбрать наследника случайным способом. При наследовании шанс стать родителем очередного потомка пропорционален фитнесс-функции точки. Соответственно уходят в следующее поколение в основном только лучшие стратегии.

### Препятствия
Для усложнения задачи, на пути точек в разных местах добавляются круглые препятствия разных размеров. И даже в таком случае точкам удаётся доходить до координаты (1;1).

Сам полигон выглядит примерно так:
[Application](https://github.com/is-tech-y24-1/jizapika/blob/lab-4/11.bmp)
![](https://github.com/is-tech-y24-1/jizapika/blob/lab-4/11.bmp)
