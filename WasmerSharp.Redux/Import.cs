using System;
using System.Collections.Generic;
using System.Reflection;

namespace WasmerSharp;

/// <summary>
/// Use this class to create the various Import objects (Globals, Memory, Function and Tables)
/// </summary>
public class Import {
  /// <summary>
  /// The module name for this import
  /// </summary>
  public string ModuleName { get; private set; }
  /// <summary>
  ///  The name for this import
  /// </summary>
  public string ImportName { get; private set; }
  /// <summary>
  /// The kind of import
  /// </summary>
  public ImportExportKind Kind { get; private set; }
  internal WasmerNativeHandle payload;

  /// <summary>
  /// Returns an array of Import elements based on the suitable functions in the type T as Imports.
  /// </summary>
  /// <typeparam name="T">The type that will be scanned for methods.</typeparam>
  /// <param name="moduleName">The module name used by default.</param>
  /// <remarks>
  /// You can use this method to easily expose a number of methods in a type to the 
  /// </remarks>
  /// <returns>Array of imports with ImportFunctions as defined on the type</returns>
  public static Import [] FunctionsFromType<T> (string moduleName)
  {
    var result = new List<Import> ();
    foreach (var mi in typeof (T).GetMethods (BindingFlags.Static | BindingFlags.Public)) {
      var pi = mi.GetParameters ();
      if (pi.Length == 0)
        continue;
      if (pi [0].GetType () == typeof (InstanceContext)) {
        for (int i = 1; i < pi.Length; i++) {
          var pit = pi [i].GetType ();
          if (pit == typeof (int) || pit == typeof (long) || pit == typeof (double) || pit == typeof (float)) {
            var module = moduleName;
            var name = mi.Name;

            var cattr = mi.GetCustomAttribute<WasmerImportAttribute> ();
            if (cattr != null) {
              if (cattr.Module != null)
                module = cattr.Module;
              if (cattr.Name != null)
                name = cattr.Name;
            }
            var func = new ImportFunction (Delegate.CreateDelegate (typeof(T), mi));
            result.Add (new Import (moduleName, name, func));
          }
        }
      }
    }
    return result.ToArray ();
  }

  /// <summary>
  /// Creates a Memory import.
  /// </summary>
  /// <param name="moduleName">The module name for this import</param>
  /// <param name="importName">The name for this import, if not specified, it will default to "memory"</param>
  /// <param name="memory">The memory object to import</param>
  public Import (string moduleName, string importName, Memory memory)
  {
    if (moduleName == null)
      throw new ArgumentNullException (nameof (moduleName));
    if (memory == null)
      throw new ArgumentNullException (nameof (memory));
    ModuleName = moduleName;
    ImportName = importName ?? "memory";
    Kind = ImportExportKind.Memory;
    payload = memory;
  }

  /// <summary>
  /// Creates a Global import.
  /// </summary>
  /// <param name="moduleName">The module name for this import</param>
  /// <param name="importName">The name for this import.</param>
  /// <param name="global">The global object to import</param>
  public Import (string moduleName, string importName, Global global)
  {
    if (moduleName == null)
      throw new ArgumentNullException (nameof (moduleName));
    if (global == null)
      throw new ArgumentNullException (nameof (global));
    ModuleName = moduleName;
    ImportName = importName ?? "memory";
    Kind = ImportExportKind.Global;
    payload = global;
  }

  /// <summary>
  /// Creates a Function import.
  /// </summary>
  /// <param name="moduleName">The module name for this import</param>
  /// <param name="importName">The name for this import</param>
  /// <param name="function">The function to import</param>
  public Import (string moduleName, string importName, ImportFunction function)
  {
    if (moduleName == null)
      throw new ArgumentNullException (nameof (moduleName));
    if (importName == null)
      throw new ArgumentNullException (nameof (importName));
    if (function == null)
      throw new ArgumentNullException (nameof (function));
    ModuleName = moduleName;
    ImportName = importName;
    Kind = ImportExportKind.Function;
    payload = function;
  }

  /// <summary>
  /// Creates a Table import.
  /// </summary>
  /// <param name="moduleName">The module name for this import</param>
  /// <param name="importName">The name for this import</param>
  /// <param name="table">The table to import</param>
  public Import (string moduleName, string importName, Table table)
  {
    if (moduleName == null)
      throw new ArgumentNullException (nameof (moduleName));
    if (importName == null)
      throw new ArgumentNullException (nameof (importName));
    if (table == null)
      throw new ArgumentNullException (nameof (table));
    ModuleName = moduleName;
    ImportName = importName;
    Kind = ImportExportKind.Table;
    payload = table;
  }
}