using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DebugConsole : MonoBehaviour {

    public CodeEditor codeEditor;
    public ConsoleEditor consoleEditor;
    public LogEditor logEditor;

    private CSharpScriptingInterface scriptingInterface;

    private void Start() {
        Assembly gameAssembly = typeof(DebugConsole).Assembly;
        Assembly geometryAssembly = typeof(Geometry).Assembly;
        Assembly hyperPrimitiveAssembly = typeof(HyperPrimitive).Assembly;
        Assembly commonAssembly = typeof(NumberUtil).Assembly;
        Assembly toolsAssembly = typeof(FocusManager).Assembly;
        Assembly networkAssembly = typeof(Server).Assembly;
        Assembly unityCoreAssembly = typeof(GameObject).Assembly;
        Assembly netStandardAssembly = Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51");
        scriptingInterface = new CSharpScriptingInterface();
        scriptingInterface.SetAssemblies(gameAssembly, geometryAssembly, hyperPrimitiveAssembly, commonAssembly, toolsAssembly, networkAssembly, unityCoreAssembly, netStandardAssembly);
        scriptingInterface.SetImports("UnityEngine", "System.Collections.Generic", "System.Linq");
        codeEditor.scriptingInterface = scriptingInterface;
        consoleEditor.scriptingInterface = scriptingInterface;
        logEditor.scriptingInterface = scriptingInterface;
    }
}