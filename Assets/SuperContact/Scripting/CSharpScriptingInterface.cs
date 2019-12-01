using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

public class CSharpScriptingInterface : ScriptingInterface {

    private ScriptState<object> state = null;
    private Boolean isExecuting = false;

    private object environment;
    private ScriptOptions options = ScriptOptions.Default;

    public void SetEnvironment(object environment) {
        this.environment = environment;
    }

    public void SetAssemblies(params Assembly[] assemblies) {
        options = options.WithReferences(assemblies);
    }

    public void SetImports(params string[] imports) {
        options = options.WithImports(imports);
    }

    public override async Task<object> Execute(string code, bool throwException) {
        return await Run(code, throwException, isAsync);
    }

    public override async Task SubmitCode(string name, string code) {
        state = null;
        await Run(code, false /* throwException */, isAsync);
    }

    private async Task<object> Run(string code, bool throwException, bool isAsync) {
        if (isExecuting) {
            SendLogError("Some code is still running!");
            return null;
        }

        isExecuting = true;
        Task<ScriptState<object>> task;

        try {
            if (isAsync) {
                task = Task.Run(async () => await (state == null ? CSharpScript.RunAsync(code, options, environment) : state.ContinueWithAsync(code)));
            } else {
                task = state == null ? CSharpScript.RunAsync(code, options, environment) : state.ContinueWithAsync(code);
            }
            await task;
        } catch (Exception e) {
            if (throwException) {
                isExecuting = false;
                throw e;
            } else {
                SendException(e);
                return null;
            }
        } finally {
            isExecuting = false;
        }

        state = task.Result;
        return state.ReturnValue;
    }
}
