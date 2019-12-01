using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MineSweeper {
    public class MineSweeperConsole : MonoBehaviour {

        public CodeEditor codeEditor;
        public ConsoleEditor consoleEditor;
        public LogEditor logEditor;

        private CSharpScriptingInterface scriptingInterface;

        private void Start() {
            Assembly mineSweeperAssembly = typeof(MineSweeperGame).Assembly;
            Assembly geometryAssembly = typeof(Geometry).Assembly;
            Assembly hyperPrimitiveAssembly = typeof(HyperPrimitive).Assembly;
            Assembly commonAssembly = typeof(NumberUtil).Assembly;
            Assembly toolsAssembly = typeof(FocusManager).Assembly;
            Assembly unityCoreAssembly = typeof(GameObject).Assembly;
            Assembly netStandardAssembly = Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51");
            scriptingInterface = new CSharpScriptingInterface();
            scriptingInterface.SetAssemblies(mineSweeperAssembly, geometryAssembly, hyperPrimitiveAssembly, commonAssembly, toolsAssembly, unityCoreAssembly, netStandardAssembly);
            scriptingInterface.SetImports("MineSweeper");
            codeEditor.scriptingInterface = scriptingInterface;
            consoleEditor.scriptingInterface = scriptingInterface;
            logEditor.scriptingInterface = scriptingInterface;
        }
    }
}