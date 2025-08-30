Imports System.IO
Imports System.Threading

Module GDTModule
    Private gdtWatchFolder As String = "C:\GDT\Import"
    Private gdtCheckTimer As System.Windows.Forms.Timer
    Private processedFiles As New Dictionary(Of String, DateTime)

    Public Sub StartGDTMonitoring()
        ' Timer für regelmäßige Überprüfung
        gdtCheckTimer = New System.Windows.Forms.Timer()
        gdtCheckTimer.Interval = 2000
        AddHandler gdtCheckTimer.Tick, AddressOf CheckForGDTFiles
        gdtCheckTimer.Start()
    End Sub

    Public Sub StopGDTMonitoring()
        If gdtCheckTimer IsNot Nothing Then
            gdtCheckTimer.Stop()
            gdtCheckTimer.Dispose()
        End If
    End Sub

    Public Sub ProcessExistingGDTFiles()
        Try
            If Not Directory.Exists(gdtWatchFolder) Then
                Directory.CreateDirectory(gdtWatchFolder)
                Return
            End If

            Dim backupFolder As String = Path.Combine(gdtWatchFolder, "Backup")
            If Not Directory.Exists(backupFolder) Then
                Directory.CreateDirectory(backupFolder)
            End If

            Dim gdtFiles() As String = Directory.GetFiles(gdtWatchFolder, "*.gdt", SearchOption.TopDirectoryOnly)

            For Each gdtFile As String In gdtFiles
                ProcessSingleGDTFile(gdtFile)
            Next
        Catch ex As Exception
            ' Log error
        End Try
    End Sub

    Private Sub CheckForGDTFiles()
        Try
            If Directory.Exists(gdtWatchFolder) Then
                Dim gdtFiles() As String = Directory.GetFiles(gdtWatchFolder, "*.gdt", SearchOption.TopDirectoryOnly)

                For Each gdtFile As String In gdtFiles
                    ProcessSingleGDTFile(gdtFile)
                Next
            End If
        Catch
            ' Fehler ignorieren, beim nächsten Durchlauf erneut versuchen
        End Try
    End Sub

    Private Sub ProcessSingleGDTFile(gdtFile As String)
        Try
            Dim backupFolder As String = Path.Combine(gdtWatchFolder, "Backup")
            Dim backupFile As String = Path.Combine(backupFolder,
                $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(gdtFile)}")

            File.Move(gdtFile, backupFile)

            ' Debug-Ausgabe
            Dim patient As GDTParser.GDTPatient = GDTParser.ParseGDTFile(backupFile)

            If patient IsNot Nothing Then
                Logger.Debug($"Patient gefunden: {patient.Nachname}, {patient.Vorname}")

                If DatabaseModule.MainForm IsNot Nothing Then
                    DatabaseModule.MainForm.Invoke(Sub()
                                                       DatabaseModule.MainForm.NeuerPatientVonGDT(patient)
                                                   End Sub)
                Else
                    Logger.Debug("MainForm ist nicht gesetzt!")
                End If
            Else
                Logger.Debug("GDTParser lieferte Nothing zurück!")
            End If
        Catch ex As Exception
            Logger.Debug($"Fehler: {ex.Message}")
        End Try
    End Sub


End Module
