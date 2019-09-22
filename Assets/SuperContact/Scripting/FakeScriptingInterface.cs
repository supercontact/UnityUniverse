using System.Threading.Tasks;

public class FakeScriptingInterface : ScriptingInterface {

    public override Task<object> Execute(string code, bool throwException) {
        return Task.FromResult<object>(code + "\n ...looks good!");
    }

    public override Task SubmitCode(string name, string code) {
        return Task.CompletedTask;
    }
}
