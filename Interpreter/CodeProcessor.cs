using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LisaCore.Interpreter
{
    /// <summary>
    /// An interpreter for executing C# code dynamically.
    /// </summary>
    public sealed class CodeProcessor
    {
        private List<string> _references;
        private List<string> _imports;
        private ScriptState<object>? _scriptState;
        private Action<object?>? _callback;
        private readonly Dictionary<string, object> _globalVariables = new Dictionary<string, object>();

        public CodeProcessor()
        {
            _references = new List<string>();
            _imports = new List<string>();
            _scriptState = null;
            _callback = null;
        }

        /// <summary>
        /// Adds an external assembly as a reference for the C# code.
        /// </summary>
        /// <param name="assembly">The assembly to add as a reference.</param>
        public void AddReference(Assembly assembly)
        {
            _references.Add(assembly.Location);
        }

        /// <summary>
        /// Adds a namespace required by the C# code.
        /// </summary>
        /// <param name="namespaceName">The namespace to add.</param>
        public void AddNamespace(string namespaceName)
        {
            _imports.Add(namespaceName);
        }

        /// <summary>
        /// Sets a global variable for the C# code.
        /// </summary>
        /// <param name="name">The name of the global variable.</param>
        /// <param name="value">The value of the global variable.</param>
        public void SetGlobalVariable(string name, object value)
        {
            lock (_globalVariables)
            {
                _globalVariables[name] = value;
            }
        }

        /// <summary>
        /// Gets the value of a global variable.
        /// </summary>
        /// <param name="name">The name of the global variable.</param>
        /// <returns>The value of the global variable.</returns>
        public object GetGlobalVariable(string name)
        {
            lock (_globalVariables)
            {
                if (_globalVariables.ContainsKey(name))
                {
                    return _globalVariables[name];
                }
                else
                {
                    throw new InvalidOperationException($"Global variable '{name}' not found.");
                }
            }
        }

        private ScriptOptions GetScriptOptions(string code)
        {
            var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var options = ScriptOptions.Default;
            foreach (var currentAssembly in currentAssemblies)
            {
                try
                {
                    options = options.AddReferences(currentAssembly);
                }
                catch { }
            }
            var uniqueReferences = _references
                .Distinct()
                .Where(reference => !currentAssemblies.Any(assembly => assembly.Location == reference));

            if (uniqueReferences.Any())
            {
                options = options.AddReferences(uniqueReferences);
            }

            options = options.AddImports("System",
                "System.IO",
                "System.Linq",
                "System.Text",
                "System.Collections.Generic",
                "System.Tuple",
                "System.DateTime",
                "Microsoft.EntityFrameworkCore"
                );

            var requiredNamespaces = CodeAnalyzer.GetNamespaces(code);
            foreach (var item in _imports.Distinct())
            {
                if (!requiredNamespaces.Contains(item))
                {
                    requiredNamespaces.Add(item);
                }
            }
            var existingNamespaces = options.Imports;

            var missingNamespaces = requiredNamespaces.Except(existingNamespaces).ToList();
            if (missingNamespaces.Any())
            {
                options = options.AddImports(missingNamespaces);
            }
            return options;
        }

        /// <summary>
        /// Compiles and executes C# code asynchronously.
        /// </summary>
        /// <param name="code">The C# code to execute.</param>
        /// <param name="objects">Optional user objects to pass to the code.</param>
        /// <returns>A tuple containing the result of the code execution and any error that occurred.</returns>
        public async Task<(object? Result, Exception? Error)> ExecuteAsync(string code, object? objects = null)
        {
            // Ensure the code is valid
            var valid = IsCodeValid(code, out var diagnostics);
            if (diagnostics.Any(diag => diag.Severity == DiagnosticSeverity.Error))
            {
                return (null, new InvalidOperationException("Invalid code. Please fix the errors before executing."));
            }

            try
            {

                var scriptOptions = GetScriptOptions(code);
                _scriptState = await CSharpScript.RunAsync(code, options: scriptOptions, objects, cancellationToken: CancellationToken.None);
                if (!(objects is CallbackHelper)) _callback = null; //reset call back if script state doesn't have call back
                return (_scriptState.ReturnValue, null);
            }
            catch (Exception ex)
            {
                return (null, ex.InnerException ?? ex);
            }

        }

        /// <summary>
        /// Compiles and executes C# code with callback asynchronously.
        /// </summary>
        /// <param name="code">The C# code to execute.</param>
        /// <param name="callback">An optional callback to invoke when the code is executed.</param>
        /// <param name="objects">Optional user objects to pass to the code.</param>
        /// <returns>A tuple containing the result of the code execution and any error that occurred.</returns>
        public async Task<(object? Result, Exception? Error)> ExecuteAsync(string code, Action<object?> callback, object? objects = null)
        {
            if (callback == null)
            {
                return (null, new InvalidOperationException($"Invalid callback object."));
            }
            try
            {
                DbContext? dbContext = null;
                object? userObjects = null;


                if (objects != null)
                {
                    if (objects is List<object>)
                    {
                        var list = (List<object>)objects;
                        List<object> remove = new();
                        foreach (var item in list)
                        {
                            if (item is DbContext)
                            {
                                dbContext = (DbContext)item;
                                remove.Add(item);
                            }
                        }
                        userObjects = list.Except(remove).ToList();
                    }
                    else if (objects is DbContext)
                    {
                        dbContext = (DbContext)objects;
                    }
                    else
                    {
                        userObjects = objects;
                    }
                }

                var (result, ex) = callback != null || dbContext != null || userObjects != null ? await ExecuteAsync(code, new CallbackHelper(callback, dbContext, userObjects)) : await ExecuteAsync(code, objects);

                if (callback != null)
                {
                    callback(result);
                    _callback = callback;
                }
                return (result, ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (null, ex.InnerException);
            }
        }

        /// <summary>
        /// Continues previous code execution state. Compiles and executes C# code asynchronously.
        /// </summary>
        /// <param name="code">The code to execute.</param>
        /// <returns>A tuple containing the result of the code execution and any error that occurred.</returns>
        public async Task<(object? Result, Exception? Error)> ContinueExecuteAsync(string code)
        {
            // Ensure the code is valid
            var valid = IsCodeValid(code, out var diagnostics);
            if (diagnostics.Any(diag => diag.Severity == DiagnosticSeverity.Error))
            {
                return (null, new InvalidOperationException("Invalid code. Please fix the errors before executing."));
            }

            try
            {
                if (_scriptState != null)
                {
                    var scriptOptions = GetScriptOptions(code);
                    _scriptState = await _scriptState.ContinueWithAsync(code, scriptOptions, cancellationToken: CancellationToken.None);
                    if (_callback != null) { _callback(_scriptState.ReturnValue); }
                    return (_scriptState.ReturnValue, null);
                }

            }
            catch (Exception ex)
            {
                return (null, ex.InnerException);
            }
            return (null, new InvalidOperationException("Script State is blank. Failed to continue previous script state."));
        }



        private class CallbackHelper
        {
            public Action<object>? Callback { get; private set; } = null;
            public DbContext? DbContext { get; private set; } = null;

            public object? Objects { get; private set; } = null;
            public CallbackHelper(Action<object>? callback, DbContext? dbContext, object? objects)
            {
                Callback = callback;
                DbContext = dbContext;
                Objects = objects;
            }
        }

        /// <summary>
        /// Determines if the code is valid and does not contain errors.
        /// </summary>
        /// <param name="code">The C# code to check.</param>
        /// <param name="diagnostics">A list of any diagnostics produced by the code analysis.</param>
        /// <param name="objects">Optional user objects to pass to the code.</param>
        /// <returns>True if the code is valid, false otherwise.</returns>
        public bool IsCodeValid(string code, out IEnumerable<Diagnostic> diagnostics, object? objects = null)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            diagnostics = syntaxTree.GetDiagnostics();

            if (objects != null)
            {
                if (objects is CallbackHelper)
                {
                    var callbackObject = (CallbackHelper)objects;
                    if (callbackObject.DbContext == null)
                    {
                        var dbContextReference = "DbContext"; // The name of the DbContext property
                        if (code.Contains(dbContextReference))
                        {
                            var additionalDiagnostic = Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    id: "CS-DBCONTEXT-NULL",
                                    title: "DbContext is null",
                                    messageFormat: $"The DbContext property is null. Please provide a valid DbContext to the Interpreter.",
                                    category: "Scripting",
                                    defaultSeverity: DiagnosticSeverity.Error,
                                    isEnabledByDefault: true),
                                    Location.None);

                            diagnostics = diagnostics.Concat(new[] { additionalDiagnostic });
                        }
                    }
                }
            }

            return !diagnostics.Any(diag => diag.Severity == DiagnosticSeverity.Error);
        }



    }
}