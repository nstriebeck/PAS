Imports System.IO

Public Module Logger
    Private ReadOnly logFile As String = Path.Combine(Application.StartupPath, $"PAS_Log_{DateTime.Now:yyyy-MM-dd}.txt")
    Private ReadOnly lockObj As New Object()

    Public Enum LogLevel
        Debug = 0
        Info = 1
        Warning = 2
        [Error] = 3
    End Enum

    Public Property MinLogLevel As LogLevel = LogLevel.Debug

    Public Sub Log(level As LogLevel, message As String, Optional ex As Exception = Nothing)
        If level < MinLogLevel Then Return

        Dim timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        Dim levelStr = level.ToString().PadRight(7)
        Dim logEntry = $"{timestamp} [{levelStr}] {message}"

        If ex IsNot Nothing Then
            logEntry &= Environment.NewLine & $"  Exception: {ex.Message}" & Environment.NewLine & $"  StackTrace: {ex.StackTrace}"
        End If

        Try
            ' 1. Debug-Ausgabe
            System.Diagnostics.Debug.WriteLine(logEntry)

            ' 2. Datei
            SyncLock lockObj
                File.AppendAllText(logFile, logEntry & Environment.NewLine)
            End SyncLock

        Catch
            ' Logging darf nie crashen
        End Try
    End Sub

    Public Sub Debug(message As String)
        Log(LogLevel.Debug, message)
    End Sub

    Public Sub Info(message As String)
        Log(LogLevel.Info, message)
    End Sub

    Public Sub Warning(message As String)
        Log(LogLevel.Warning, message)
    End Sub

    Public Sub [Error](message As String, Optional ex As Exception = Nothing)
        Log(LogLevel.[Error], message, ex)
    End Sub
End Module
