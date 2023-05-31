using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Passes;

ConsoleDriver.Run(new WasmerLibrary(Path.Combine(Directory.GetCurrentDirectory(), "include")));

public class WasmerLibrary : ILibrary
{
  private readonly string _headersDirectoryPath;

  public WasmerLibrary(string headersDirectoryPath)
  {
    _headersDirectoryPath = headersDirectoryPath;
  }

  public void Setup(Driver driver)
  {
    var options = driver.Options;
    options.GeneratorKind = GeneratorKind.CSharp;
    options.OutputDir = "bindings";
    var module = options.AddModule("Wasmer");
    module.IncludeDirs.Add(_headersDirectoryPath);
    module.Headers.Add("wasmer.h");
    module.Headers.Add("wasm.h");
    // module.LibraryDirs.Add(_binariesDirectoryPath);
    // module.Libraries.Add("Sample.lib");
  }

  public void Preprocess(Driver driver, ASTContext ctx)
  {
  }

  public void Postprocess(Driver driver, ASTContext ctx)
  {
  }

  public void SetupPasses(Driver driver)
  {
    driver.Context.TranslationUnitPasses.RenameDeclsUpperCase(RenameTargets.Any);
    driver.Context.TranslationUnitPasses.AddPass(new FunctionToInstanceMethodPass());
  }
}