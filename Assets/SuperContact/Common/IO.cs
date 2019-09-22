using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class IO {

    public const string TestLogPath = "TestLog.txt"; 

    public static StreamWriter writer;
    public static StreamReader reader;
    public static string rootPath = Path.GetFullPath(".");

    public static void OpenForWrite(string path) {
        writer = new StreamWriter(rootPath + Path.DirectorySeparatorChar + path);
    }

    public static void OpenForRead(string path) {
        reader = new StreamReader(rootPath + path);
    }

    public static void CloseWrite() {
        writer.Close();
    }

    public static void CloseRead() {
        reader.Close();
    }

    public static void Write(string str) {
        writer.Write(str);
    }

    public static void WriteLine(string str) {
        writer.WriteLine(str);
    }

    public static string ReadLine() {
        return reader.ReadLine();
    }
}
