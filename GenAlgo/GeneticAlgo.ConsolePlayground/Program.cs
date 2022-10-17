// See https://aka.ms/new-console-template for more information

using GeneticAlgo.Shared;
using GeneticAlgo.Shared.Tools;
using Serilog;

Logger.Init();
Log.Information("Start console polygon");
var executionContext = new ConsoleExecutionContext(100, 100, 3, 0, 0);
while (true)
{
    executionContext.MakeIteration();
}