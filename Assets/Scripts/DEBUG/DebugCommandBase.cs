using System;
using System.Globalization;
using UnityEngine;

public abstract class DebugCommandBase
{
    private string _commandId;
    private string _commandDescription;
    private string _commandFormat;

    public string commandId => _commandId;
    public string commandDescription => _commandDescription;
    public string commandFormat => _commandFormat;

    public DebugCommandBase(string id, string description, string format)
    {
        _commandId = id.ToLowerInvariant();
        _commandDescription = description;
        _commandFormat = format;
    }

    public abstract bool Execute(string[] args, out string resultMessage);
}

public class DebugCommand : DebugCommandBase
{
    private Action command;

    public DebugCommand(string id, string description, string format, Action command) : base(id, description, format)
    {
        this.command = command;
    }

    public void Invoke()
    {
        command?.Invoke();
    }

    public override bool Execute(string[] args, out string resultMessage)
    {
        try
        {
            command?.Invoke();
            resultMessage = $"[OK] Wykonano '{commandId}'";
            return true;
        }
        catch (Exception ex)
        {
            resultMessage = $"[BŁĄD] {commandId}: {ex.Message}";
            return false;
        }
    }
}

public class DebugCommandArgs : DebugCommandBase
{
    private Action<string[]> command;

    public DebugCommandArgs(string id, string description, string format, Action<string[]> command) : base(id, description, format)
    {
        this.command = command;
    }

    public override bool Execute(string[] args, out string resultMessage)
    {
        try
        {
            command?.Invoke(args);
            resultMessage = "";
            return true;
        }
        catch (Exception ex)
        {
            resultMessage = $"[BŁĄD] {commandId}: {ex.Message}";
            return false;
        }
    }
}

public class DebugCommand<T> : DebugCommandBase
{
    private Action<T> command;

    public DebugCommand(string id, string description, string format, Action<T> command) : base(id, description, format)
    {
        this.command = command;
    }

    public void Invoke(T value)
    {
        command?.Invoke(value);
    }

    public override bool Execute(string[] args, out string resultMessage)
    {
        if (args == null || args.Length == 0)
        {
            resultMessage = $"[BŁĄD] Wymagany parametr! Użycie: {commandFormat}";
            return false;
        }

        try
        {
            T parsedValue = (T)Convert.ChangeType(args[0], typeof(T), CultureInfo.InvariantCulture);
            command?.Invoke(parsedValue);
            resultMessage = $"[OK] Wykonano '{commandId} {args[0]}'";
            return true;
        }
        catch
        {
            resultMessage = $"[BŁĄD] Niepoprawny parametr '{args[0]}'! Użycie: {commandFormat}";
            return false;
        }
    }
}

public class DebugCommand<T1, T2> : DebugCommandBase
{
    private Action<T1, T2> command;

    public DebugCommand(string id, string description, string format, Action<T1, T2> command) : base(id, description, format)
    {
        this.command = command;
    }

    public void Invoke(T1 value1, T2 value2)
    {
        command?.Invoke(value1, value2);
    }

    public override bool Execute(string[] args, out string resultMessage)
    {
        if (args == null || args.Length < 2)
        {
            resultMessage = $"[BŁĄD] Wymagane 2 parametry! Użycie: {commandFormat}";
            return false;
        }

        try
        {
            T1 val1 = (T1)Convert.ChangeType(args[0], typeof(T1), CultureInfo.InvariantCulture);
            T2 val2 = (T2)Convert.ChangeType(args[1], typeof(T2), CultureInfo.InvariantCulture);
            command?.Invoke(val1, val2);
            resultMessage = $"[OK] Wykonano '{commandId} {args[0]} {args[1]}'";
            return true;
        }
        catch
        {
            resultMessage = $"[BŁĄD] Niepoprawne parametry! Użycie: {commandFormat}";
            return false;
        }
    }
}

public class DebugCommand<T1, T2, T3> : DebugCommandBase
{
    private Action<T1, T2, T3> command;

    public DebugCommand(string id, string description, string format, Action<T1, T2, T3> command) : base(id, description, format)
    {
        this.command = command;
    }

    public void Invoke(T1 value1, T2 value2, T3 value3)
    {
        command?.Invoke(value1, value2, value3);
    }

    public override bool Execute(string[] args, out string resultMessage)
    {
        if (args == null || args.Length < 3)
        {
            resultMessage = $"[BŁĄD] Wymagane 3 parametry! Użycie: {commandFormat}";
            return false;
        }

        try
        {
            T1 val1 = (T1)Convert.ChangeType(args[0], typeof(T1), CultureInfo.InvariantCulture);
            T2 val2 = (T2)Convert.ChangeType(args[1], typeof(T2), CultureInfo.InvariantCulture);
            T3 val3 = (T3)Convert.ChangeType(args[2], typeof(T3), CultureInfo.InvariantCulture);
            command?.Invoke(val1, val2, val3);
            resultMessage = $"[OK] Wykonano '{commandId} {args[0]} {args[1]} {args[2]}'";
            return true;
        }
        catch
        {
            resultMessage = $"[BŁĄD] Niepoprawne parametry! Użycie: {commandFormat}";
            return false;
        }
    }
}