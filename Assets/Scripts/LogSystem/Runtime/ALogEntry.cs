using System;
using System.Collections.Generic;

public enum ALogLevel
{
    Log = 0,
    Warning = 1,
    Error = 2,
}

/// <summary>一条日志里的单个调用栈帧,FilePath为空表示是C层/无源码的帧</summary>
public class ALogFrame
{
    public string Signature;
    public string FilePath;
    public int Line;

    public bool CanJump => !string.IsNullOrEmpty(FilePath) && Line > 0;

    public string Location => CanJump ? $"{FilePath}:{Line}" : "<no source>";
}

public class ALogEntry
{
    public int Id;
    public ALogLevel Level;
    public string Category;
    public string Message;
    public string RawStack;
    public List<ALogFrame> Frames;
    public DateTime Time;

    public string TimeText => Time.ToString("HH:mm:ss");
}
