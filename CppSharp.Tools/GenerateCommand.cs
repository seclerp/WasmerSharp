using System.ComponentModel;
using Spectre.Console.Cli;

namespace CppSharp.Tools;

internal sealed class GenerateCommand : Command<GenerateCommand.Settings>
{
  internal sealed class Settings : CommandSettings
  {
    [Description("Output folder.")]
    [CommandArgument(0, "<outputFolder>")]
    public string OutputFolder { get; init; } = null!;
  }

  public override int Execute(CommandContext context, Settings settings)
  {
    var outputFolder = settings.OutputFolder;

    var library = new WasmerLibrary(outputFolder);
    ConsoleDriver.Run(library);
    return 0;
  }
}