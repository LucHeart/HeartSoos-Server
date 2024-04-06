﻿using System.Runtime.CompilerServices;

namespace LucHeart.HeartSoos.Utils;

public static class LucTask
{
    private static readonly ILogger Logger = ApplicationLogging.CreateLogger(typeof(LucTask));
    
    public static Task Run(Func<Task?> function, [CallerFilePath] string file = "",
        [CallerMemberName] string member = "", [CallerLineNumber] int line = -1) => Task.Run(function).ContinueWith(
        t =>
        {
            if (!t.IsFaulted) return;
            var index = file.LastIndexOf('\\');
            if (index == -1) index = file.LastIndexOf('/');
            Logger.LogError(t.Exception,
                "Error during task execution. {File}::{Member}:{Line} - Stack: {Stack}",
                file.Substring(index + 1, file.Length - index - 1), member, line, t.Exception?.StackTrace);
            
        }, TaskContinuationOptions.OnlyOnFaulted);

    public static Task Run(Task? function, [CallerFilePath] string file = "",
        [CallerMemberName] string member = "", [CallerLineNumber] int line = -1) => Task.Run(() => function).ContinueWith(
        t =>
        {
            if (!t.IsFaulted) return;
            var index = file.LastIndexOf('\\');
            if (index == -1) index = file.LastIndexOf('/');
            Logger.LogError(t.Exception,
                "Error during task execution. {File}::{Member}:{Line} - Stack: {Stack}",
                file.Substring(index + 1, file.Length - index - 1), member, line, t.Exception?.StackTrace);
            
        }, TaskContinuationOptions.OnlyOnFaulted);
}