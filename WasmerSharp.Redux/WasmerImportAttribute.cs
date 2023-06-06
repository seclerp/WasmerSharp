using System;

namespace WasmerSharp;

/// <summary>
/// This attribute can be applied to a member that takes an IntanceContext as a first parameter
/// and zero or more parameters of type int, long, float or double to register the module name
/// and name.   The resulting method is returned as an Imports that is suitable to be passed
/// to the Instantiate methods.
/// </summary>
[AttributeUsage (AttributeTargets.Method)]
public class WasmerImportAttribute : Attribute {
  /// <summary>The desired module name to apply to this method.</summary>
  public string Module;
  /// <summary>The desired name to surface the method as.</summary>
  public string Name;

  /// <summary>
  /// Sets the import name, and inherits the module name
  /// </summary>
  /// <param name="name">Name to give this import</param>
  public WasmerImportAttribute (string name)
  {
    Module = "";
    Name = name;
  }

  /// <summary>
  /// Sets the import name and module name.
  /// </summary>
  /// <param name="module">Name for the module.</param>
  /// <param name="name">Name to give this import</param>
  public WasmerImportAttribute (string module, string name)
  {
    Module = module;
    Name = name;
  }
}