using CppSharp.Tools;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
  config.AddCommand<GenerateCommand>("generate");
});
app.Run(args);