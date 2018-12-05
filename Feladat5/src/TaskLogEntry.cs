using System;
using System.Threading;
using Feladat5;

public class TaskLogEntry {

    public Task Task { get; private set; }
    public int ID => Task.ID;
    public int Color => Task.Color;
    public Thread Thread { get; private set; }
    public TaskLogEntryType Type { get; private set; }
    public Exception Exception { get; private set; }

    public TaskLogEntry(Task task, TaskLogEntryType type, Thread thread, Exception exception) {
        Task = task;
        Type = type;
        Thread = thread;
        Exception = exception;
    }

    public override string ToString() => $"TaskLogEntry{{{Task},{Thread.Name},{Type}}}";
}