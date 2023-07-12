using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

//Keep this as LisaCore
namespace LisaCore
{
    /// <summary>
    /// Part of Lisa class
    /// </summary>
    public partial class Lisa
    {

        private void LoadDefaultNameSpaces()
        {
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.Efficiency.AirFlow");
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.Efficiency.Chiller");
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.Efficiency.Pump");
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.Efficiency.Temperature");
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.DataModels");
        }

        public void AddReference(Assembly assembly)
        {
            _codeProcessor.AddReference(assembly);
        }

        /// <summary>
        /// Adds a namespace required by the C# code.
        /// </summary>
        /// <param name="namespaceName">The namespace to add.</param>
        public void AddNamespace(string namespaceName)
        {
            _codeProcessor.AddNamespace(namespaceName);
        }

        // <summary>
        /// Sets a global variable for the C# code.
        /// </summary>
        /// <param name="name">The name of the global variable.</param>
        /// <param name="value">The value of the global variable.</param>
        public void SetGlobalVariable(string name, object value)
        {
            _codeProcessor.SetGlobalVariable(name, value);
        }

        /// <summary>
        /// Compiles and executes C# code asynchronously.
        /// </summary>
        /// <param name="code">The C# code to execute.</param>
        /// <param name="objects">Optional user objects to pass to the code.</param>
        /// <returns>A tuple containing the result of the code execution and any error that occurred.</returns>
        public async Task<(object? Result, Exception? Error)> ExecuteAsync(string code, object? objects = null)
        {
            return await _codeProcessor.ExecuteAsync(code, objects);
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
            return await _codeProcessor.ExecuteAsync(code, callback, objects);
        }


        /// <summary>
        /// Continues previous code execution state. Compiles and executes C# code asynchronously.
        /// </summary>
        /// <param name="code">The code to execute.</param>
        /// <returns>A tuple containing the result of the code execution and any error that occurred.</returns>
        public async Task<(object? Result, Exception? Error)> ContinueExecuteAsync(string code)
        {
            return await _codeProcessor.ContinueExecuteAsync(code);
        }


    }
}
