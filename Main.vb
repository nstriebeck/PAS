Imports System.Windows.Forms
Imports System.Threading
Imports System.Runtime.InteropServices
Imports System.IO

Module MainProgram
    Private Const WM_COPYDATA As Integer = &H4A
    Private _mutex As Mutex
    Private _mainForm As FormPAS

    <STAThread()>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' Single Instance Check mit Mutex
        Dim mutexName As String = "PatientenAufrufSystem_SingleInstance"
        Dim isNewInstance As Boolean
        _mutex = New Mutex(True, mutexName, isNewInstance)

        If Not isNewInstance Then
            ' Bereits eine Instanz läuft - Command Line Args an bestehende Instanz senden
            SendArgsToExistingInstance()
            Return
        End If

        Try
            ' Erste Instanz - normale Ausführung
            _mainForm = New FormPAS()

            ' Command Line Args verarbeiten nach kurzer Verzögerung
            ProcessInitialArgs()

            ' Anwendung starten
            Application.Run(_mainForm)
        Finally
            _mutex?.ReleaseMutex()
            _mutex?.Dispose()
        End Try
    End Sub

    Private Sub ProcessInitialArgs()
        Dim args As String() = Environment.GetCommandLineArgs()
        If args.Length > 1 Then
            ' Timer verwenden um zu warten bis Form geladen ist
            Dim timer As New Windows.Forms.Timer()
            timer.Interval = 2000 ' 2 Sekunden warten
            AddHandler timer.Tick, Sub(sender, e)
                                       timer.Stop()
                                       timer.Dispose()
                                       ProcessCommandLineArgs(_mainForm, args)
                                   End Sub
            timer.Start()
        End If
    End Sub

    Private Sub SendArgsToExistingInstance()
        Try
            ' Args in Temp-Datei schreiben für bestehende Instanz
            Dim tempFile As String = Path.Combine(Path.GetTempPath(), "PatientenAufruf_NewArgs.tmp")
            Dim args As String() = Environment.GetCommandLineArgs()
            File.WriteAllText(tempFile, String.Join("|", args))

            ' Versuche das Hauptfenster zu aktivieren
            BringExistingInstanceToFront()
        Catch ex As Exception
            ' Ignoriere Fehler beim Senden
        End Try
    End Sub

    Private Sub BringExistingInstanceToFront()
        Try
            ' Finde den Hauptprocess
            Dim currentProcess = Process.GetCurrentProcess()
            Dim processes = Process.GetProcessesByName(currentProcess.ProcessName)

            For Each proc In processes
                If proc.Id <> currentProcess.Id AndAlso proc.MainWindowHandle <> IntPtr.Zero Then
                    ShowWindow(proc.MainWindowHandle, SW_RESTORE)
                    SetForegroundWindow(proc.MainWindowHandle)
                    Exit For
                End If
            Next
        Catch ex As Exception
            ' Ignoriere Fehler
        End Try
    End Sub

    Private Sub ProcessCommandLineArgs(mainForm As FormPAS, args As String())
        Try
            ' Parameter verarbeiten
            Dim patientenID As String = ""
            Dim vorname As String = ""
            Dim nachname As String = ""

            For i As Integer = 1 To args.Length - 1 ' Start bei 1, da 0 = Programmname
                Select Case args(i).ToLower()
                    Case "-patient"
                        If i + 1 < args.Length Then patientenID = args(i + 1)
                    Case "-vorname"
                        If i + 1 < args.Length Then vorname = args(i + 1)
                    Case "-nachname"
                        If i + 1 < args.Length Then nachname = args(i + 1)
                End Select
            Next

            ' Patient hinzufügen wenn ID vorhanden
            If Not String.IsNullOrEmpty(patientenID) Then
                If mainForm.InvokeRequired Then
                    mainForm.Invoke(Sub()
                                        mainForm.NeuerPatient(patientenID, vorname, nachname)
                                        BringFormToFront(mainForm)
                                    End Sub)
                Else
                    mainForm.NeuerPatient(patientenID, vorname, nachname)
                    BringFormToFront(mainForm)
                End If
            End If
        Catch ex As Exception
            ' Ignoriere Command Line Fehler
        End Try
    End Sub

    Private Sub BringFormToFront(form As Form)
        Try
            If form.WindowState = FormWindowState.Minimized Then
                form.WindowState = FormWindowState.Normal
            End If
            form.BringToFront()
            form.Activate()
        Catch ex As Exception
            ' Ignoriere Fehler
        End Try
    End Sub

    ' Windows API Imports für Fenster-Aktivierung
    <DllImport("user32.dll")>
    Private Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    Private Const SW_RESTORE As Integer = 9
End Module