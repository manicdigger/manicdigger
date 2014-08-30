// Copyright (c) 2011 by Henon <meinrad.recheis@gmail.com>
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Jint;
using Jint.Native;


public interface IScriptInterpreter
{
    TimeSpan ExecutionTimeout { get; set; }
    bool Execute(string script);
    bool Execute(string script, out object result);
    void SetVariables(Dictionary<string, object> variables);
    void SetVariable(string name, object value);
    void SetFunction(string name, Delegate function);
}

public class JavaScriptInterpreter : IScriptInterpreter
{
    private JintEngine m_engine;

    public JavaScriptInterpreter()
    {
        Console.Write("Loading JavaScript interpreter: ");
        try
        {
            m_engine = new JintEngine();
            m_engine.AllowClr = false;
            m_engine.DisableSecurity();
            Console.WriteLine("done.");
        }
        catch (Exception e)
        {
            Console.WriteLine("FAIL.");
            Console.WriteLine("Unable to load JavaScript engine\n*********************************************\n\t" + e.Message + "\n*********************************************");
        }
    }

    public TimeSpan ExecutionTimeout { get; set; }

    public bool Execute(string script)
    {
        object result; // <-- discard
        return Execute(script, out result);
    }

    public bool Execute(string script, out object result)
    {
        try
        {
            result = m_engine.Run(script);
        }
        catch (Exception e)
        {
            result = null;
            Console.WriteLine("Script failed with error:\n*********************************************\n\t" + e.InnerException.Message + "\n*********************************************" + e.StackTrace);
            return false;
        }
        return true;
    }

    public void SetVariables(Dictionary<string, object> variables)
    {
        foreach (var pair in variables)
            SetVariable(pair.Key, pair.Value);
    }

    public void SetVariable(string name, object value)
    {
        m_engine.SetParameter(name, value);
    }

    public void SetFunction(string name, Delegate function)
    {
        m_engine.SetFunction(name, function);
    }
}

