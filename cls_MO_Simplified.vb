Imports System.Runtime.InteropServices

Public Class cls_MO_Simplified
    ' DLL-Imports für Medical Office Submgr.dll - KORREKTE SIGNATUREN
    <DllImport("submgr.dll", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function OpenMO(ByVal username As String) As Integer
    End Function

    <DllImport("submgr.dll", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Sub CloseMO()
    End Sub

    <DllImport("submgr.dll", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function OpenPatient(ByVal PatNr As Long, ByVal ScheinNr As Long) As Long
    End Function

    ' Debug-Output für Diagnose
    <DllImport("kernel32.dll", CharSet:=CharSet.Ansi)>
    Private Shared Sub OutputDebugString(ByVal lpOutputString As String)
    End Sub

    ' Eigenschaften
    Private _isConnected As Boolean = False
    Private _lastError As String = ""
    Private _username As String = "admin"

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _isConnected
        End Get
    End Property

    Public ReadOnly Property LastError As String
        Get
            Return _lastError
        End Get
    End Property

    ' Konstruktor
    Public Sub New(Optional username As String = "admin")
        _username = username
    End Sub

    ' Verbindung zu Medical Office öffnen
    Public Function Connect() As Boolean
        Try
            DebugLog($"Versuche Verbindung zu Medical Office mit Benutzer '{_username}'")

            Dim result As Integer = OpenMO(_username)

            If result = 0 Then
                _lastError = "OpenMO hat 0 zurückgegeben - Verbindung fehlgeschlagen"
                _isConnected = False
                DebugLog(_lastError)
                Return False
            Else
                _isConnected = True
                DebugLog($"Medical Office erfolgreich geöffnet (Return: {result})")
                Return True
            End If

        Catch ex As Exception
            _lastError = "Fehler beim Verbinden: " & ex.Message
            _isConnected = False
            DebugLog(_lastError)

            ' Prüfen ob submgr.dll vorhanden ist
            If ex.Message.Contains("submgr.dll") Then
                _lastError &= vbCrLf & "Hinweis: submgr.dll nicht gefunden. Medical Office muss installiert sein."
            End If

            Return False
        End Try
    End Function

    ' Verbindung trennen
    Public Sub Disconnect()
        Try
            If _isConnected Then
                CloseMO()
                _isConnected = False
                DebugLog("Medical Office Verbindung getrennt")
            End If
        Catch ex As Exception
            _lastError = "Fehler beim Trennen: " & ex.Message
            DebugLog(_lastError)
        End Try
    End Sub

    ' Patient in Medical Office öffnen
    Public Function OpenPatientInMO(patientID As String, Optional scheinNr As Long = 0) As Boolean
        Try
            If Not _isConnected Then
                ' Versuche zu verbinden
                If Not Connect() Then
                    _lastError = "Nicht mit Medical Office verbunden"
                    Return False
                End If
            End If

            If String.IsNullOrEmpty(patientID) Then
                _lastError = "Keine Patienten-ID angegeben"
                Return False
            End If

            ' PatientID in Long konvertieren
            Dim patNr As Long
            If Not Long.TryParse(patientID, patNr) Then
                _lastError = $"Ungültige Patienten-ID: {patientID}"
                Return False
            End If

            DebugLog($"Öffne Patient {patNr} mit ScheinNr {scheinNr}")

            ' Zuerst mit ScheinNr = 0 versuchen
            Dim result As Long = OpenPatient(patNr, scheinNr)

            DebugLog($"OpenPatient Rückgabewert: {result}")

            If result > 0 Then
                Return True
            Else
                ' Wenn 0 fehlschlägt, könnte Patient nicht existieren
                _lastError = $"Patient {patientID} konnte nicht geöffnet werden (Return: {result})"

                ' Optional: Hier könnte man versuchen die ScheinNr zu ermitteln
                ' Das würde aber Zugriff auf die MO-Datenbank erfordern

                Return False
            End If

        Catch ex As Exception
            _lastError = "Fehler beim Öffnen des Patienten: " & ex.Message
            DebugLog(_lastError)
            Return False
        End Try
    End Function

    ' Hilfsmethode: Medical Office starten falls nicht läuft
    Public Function EnsureMedicalOfficeRunning() As Boolean
        Try
            ' Prüfen ob Medical Office läuft
            Dim moProcesses() As Process = Process.GetProcessesByName("MedicalOffice")

            If moProcesses.Length > 0 Then
                DebugLog("Medical Office läuft bereits")
                Return True
            End If

            ' Medical Office Pfad suchen
            Dim moPath As String = FindMedicalOfficePath()

            If String.IsNullOrEmpty(moPath) Then
                _lastError = "Medical Office Installation nicht gefunden"
                Return False
            End If

            DebugLog($"Starte Medical Office von: {moPath}")
            Process.Start(moPath)

            ' Warten bis gestartet
            System.Threading.Thread.Sleep(5000)

            ' Prüfen ob erfolgreich gestartet
            moProcesses = Process.GetProcessesByName("MedicalOffice")
            Return moProcesses.Length > 0

        Catch ex As Exception
            _lastError = "Fehler beim Starten von Medical Office: " & ex.Message
            DebugLog(_lastError)
            Return False
        End Try
    End Function

    ' Medical Office Pfad finden
    Private Function FindMedicalOfficePath() As String
        Try
            ' Registry prüfen (32-bit)
            Dim regKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\WOW6432Node\Indamed\Medical Office")

            If regKey Is Nothing Then
                ' Registry prüfen (64-bit)
                regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\Indamed\Medical Office")
            End If

            If regKey IsNot Nothing Then
                Dim installPath As String = regKey.GetValue("InstallPath", "").ToString()
                regKey.Close()

                If Not String.IsNullOrEmpty(installPath) Then
                    Dim exePath As String = IO.Path.Combine(installPath, "MedicalOffice.exe")
                    If IO.File.Exists(exePath) Then
                        Return exePath
                    End If
                End If
            End If

            ' Standard-Pfade prüfen
            Dim standardPaths() As String = {
                "C:\Programme\Medical Office\MedicalOffice.exe",
                "C:\Program Files\Medical Office\MedicalOffice.exe",
                "C:\Program Files (x86)\Medical Office\MedicalOffice.exe",
                "C:\MedicalOffice\MedicalOffice.exe"
            }

            For Each path In standardPaths
                If IO.File.Exists(path) Then
                    Return path
                End If
            Next

        Catch ex As Exception
            DebugLog("Fehler bei Pfadsuche: " & ex.Message)
        End Try

        Return ""
    End Function

    ' Debug-Ausgabe
    Public Sub DebugLog(message As String)
        Try
            ' An Windows Debug-Output senden
            OutputDebugString("PatientenAufruf: " & message)

            ' Optional: In Datei schreiben
            Dim logFile As String = IO.Path.Combine(Application.StartupPath, "MO_Integration.log")
            IO.File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{vbCrLf}")

        Catch
            ' Fehler beim Logging ignorieren
        End Try
    End Sub

    ' Destruktor
    Protected Overrides Sub Finalize()
        Disconnect()
        MyBase.Finalize()
    End Sub
End Class