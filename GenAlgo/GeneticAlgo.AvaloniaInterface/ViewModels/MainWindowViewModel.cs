using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using GeneticAlgo.Shared;
using GeneticAlgo.Shared.Models;
using GeneticAlgo.UIShared;
using OxyPlot;
using OxyPlot.Series;

namespace GeneticAlgo.AvaloniaInterface.ViewModels;

public class MainWindowViewModel : AvaloniaObject
{
    private readonly IStatisticsConsumer _statisticsConsumer;
    private readonly IExecutionContext _executionContext;
    private readonly ExecutionConfiguration _configuration;

    public MainWindowViewModel(
        IExecutionContext executionContext,
        ExecutionConfiguration configuration)
    {
        _executionContext = executionContext;
        _configuration = configuration;

        IsRunning = AvaloniaProperty
            .RegisterAttached<MainWindowViewModel, bool>(nameof(IsRunning), typeof(MainWindowViewModel));

        var dotSeries = new ScatterSeries
        {
            MarkerSize = 1.5,
            MarkerStroke = OxyColors.ForestGreen,
            MarkerType = MarkerType.Circle,
        };

        var circleSeries = new ScatterSeries
        {
            MarkerFill = OxyColors.Red,
            MarkerType = MarkerType.Circle,
        };
        
        var aimSeries = new ScatterSeries
        {
            MarkerSize = 10,
            MarkerStroke = OxyColors.Yellow,
            MarkerType = MarkerType.Circle,
        };

        ScatterModel = new PlotModel
        {
            Title = "Points",
            Series = { dotSeries, circleSeries, aimSeries },
        };

        var barSeries = new LinearBarSeries
        {
            DataFieldX = nameof(FitnessModel.X),
            DataFieldY = nameof(FitnessModel.Y),
        };

        BarModel = new PlotModel
        {
            Title = "Fitness",
            Series = { barSeries },
        };

        _statisticsConsumer = new PlotStatisticConsumer(aimSeries, circleSeries, dotSeries, barSeries);
    }

    public PlotModel ScatterModel { get; }
    public PlotModel BarModel { get; }

    public AttachedProperty<bool> IsRunning { get; }

    public async Task RunAsync()
    {
        foreach (var series in ScatterModel.Series.OfType<XYAxisSeries>())
        {
            series.XAxis.Maximum = _configuration.MaximumValue;
            series.XAxis.Minimum = _configuration.MinimumValue;
            series.YAxis.Maximum = _configuration.MaximumValue;
            series.YAxis.Minimum = _configuration.MinimumValue;
        }

        SetValue(IsRunning, true);
        _executionContext.Reset();

        IterationResult iterationResult;

        do
        {
            iterationResult = await _executionContext.ExecuteIterationAsync();
            _executionContext.ReportStatistics(_statisticsConsumer);

            ScatterModel.InvalidatePlot(true);
            BarModel.InvalidatePlot(true);

            await Task.Delay(_configuration.InheritDelay);
        }
        while (iterationResult == IterationResult.IterationFinished && GetValue(IsRunning));
    }

    public void Stop()
    {
        SetValue(IsRunning, false);
    }
}