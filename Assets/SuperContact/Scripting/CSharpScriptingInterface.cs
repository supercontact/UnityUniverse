using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;

public class CSharpScriptingInterface : ScriptingInterface {

    private ScriptState<object> state = null;
    private Boolean isExecuting = false;

    private object environment;

    public void SetEnvironment(object environment) {
        this.environment = environment;
    }

    public override async Task<object> Execute(string code, bool throwException) {
        if (isExecuting) {
            SendLogError("Some code is still running!");
            return null;
        }

        isExecuting = true;
        if (state == null) {
            await Init();
        }
        try {
            state = await state.ContinueWithAsync(code);
        } catch (Exception e) {
            if (throwException) {
                throw e;
            }
            SendException(e);
            return null;
        } finally {
            isExecuting = false;
        }
        return state.ReturnValue;
    }

    public override async Task SubmitCode(string name, string code) {
        if (isExecuting) {
            SendLogError("Some code is still running!");
            return;
        }

        isExecuting = true;
        await Init();
        isExecuting = false;
    }

    private async Task Init(string initialCode = "") {
        try {
            state = await CSharpScript.RunAsync(initialCode, globals: environment);
        } catch (Exception e) {
            SendException(e);
        }
    }
}
