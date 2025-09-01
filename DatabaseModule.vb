Imports System.Data.SqlClient
Imports System.Windows.Documents

Module DatabaseModule
    Public ConnectionString As String = "Server=SILINSQL\PatientenAufruf;Database=PAS_Database;User Id=sa;Password=PatientenAufruf4711;"
    Private MasterConnectionString As String = "Server=SILINSQL\PatientenAufruf;Database=master;User Id=sa;Password=PatientenAufruf4711;"
    Public MainForm As FormPAS

    Public Function GetConfigValue(key As String) As String
        Try
            Using conn As New SqlConnection(ConnectionString)
                conn.Open()
                Dim sql As String = "SELECT ConfigValue FROM SystemConfig WHERE ConfigKey = @Key"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@Key", key)
                    Dim result As Object = cmd.ExecuteScalar()
                    If result IsNot Nothing Then
                        Return result.ToString()
                    Else
                        Return ""
                    End If
                End Using
            End Using
        Catch
            Return ""
        End Try
    End Function

    Public Sub InitializeDatabase()
        Try
            ' Schritt 1: Datenbank erstellen falls nicht vorhanden
            Using conn As New SqlConnection(MasterConnectionString)
                conn.Open()

                Dim checkDbSQL As String = "SELECT COUNT(*) FROM sys.databases WHERE name = 'PAS_Database'"
                Using cmd As New SqlCommand(checkDbSQL, conn)
                    Dim dbExists As Integer = Convert.ToInt32(cmd.ExecuteScalar())

                    If dbExists = 0 Then
                        Dim createDbSQL As String = "CREATE DATABASE PAS_Database"
                        Using createCmd As New SqlCommand(createDbSQL, conn)
                            createCmd.ExecuteNonQuery()
                        End Using
                        If MainForm IsNot Nothing Then MainForm.lblStatus.Text = "PAS-Datenbank erstellt"
                    Else
                        If MainForm IsNot Nothing Then MainForm.lblStatus.Text = "PAS-Datenbank gefunden"
                    End If
                End Using
            End Using

            ' Schritt 2: Tabelle erstellen/aktualisieren
            Using conn As New SqlConnection(ConnectionString)
                conn.Open()

                ' Haupttabelle für Warteschlange
                Dim createTableSQL As String = "
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Warteschlange')
BEGIN
    CREATE TABLE Warteschlange (
        ID int IDENTITY(1,1) PRIMARY KEY,
        PatNr nvarchar(20) NULL,                    -- Patientennummer (kann leer sein für Nicht-Patienten)
        Name nvarchar(100) NOT NULL,                -- Name (Patient oder z.B. Pharmareferent)
        Termin nvarchar(20) DEFAULT 'ohne',         -- 'ohne', 'mit Termin', etc.
        Ankunft datetime NULL,                      -- Ankunftszeit
        Wartezeit int DEFAULT 0,                    -- Berechnete Wartezeit in Minuten
        Aufgerufen datetime NULL,                   -- NEU: Wann aufgerufen wurde
        Behandlungsbeginn datetime NULL,            -- Wann Behandlung begonnen (bereits vorhanden!)
        Behandlungsende datetime NULL,              -- NEU: Wann Behandlung beendet
        Gegangen datetime NULL,                     -- Wann Patient gegangen (bereits vorhanden!)
        Status nvarchar(20) DEFAULT 'wartend',      -- 'wartend', 'in Behandlung', 'gegangen'
        Bereich nvarchar(50) NOT NULL,              -- 'Wartezimmer', 'Labor', Arztname, etc.
        Info nvarchar(500) NULL,                    -- Zusatzinfos
        Bemerkung nvarchar(500) NULL,               -- Bemerkungen
        Behandlungsgrund nvarchar(200) NULL,        -- Grund des Besuchs
        Notfall bit DEFAULT 0,                      -- Notfall-Flag
        Mitpersonen int DEFAULT 0,                  -- Anzahl Begleitpersonen
        ErstelltAm datetime DEFAULT GETDATE(),      -- Erstellungszeitpunkt
        ErstelltVon nvarchar(50) NULL,              -- Welcher Benutzer/Terminal
        IstPatient bit DEFAULT 1,                   -- Unterscheidung Patient/Nicht-Patient
        Prioritaet int DEFAULT 0                    -- Für Sortierung (Notfälle zuerst)
    )
    
    -- Indizes für bessere Performance
    CREATE INDEX IX_Warteschlange_Status ON Warteschlange(Status)
    CREATE INDEX IX_Warteschlange_PatNr ON Warteschlange(PatNr)
    CREATE INDEX IX_Warteschlange_Bereich ON Warteschlange(Bereich)
    CREATE INDEX IX_Warteschlange_ErstelltAm ON Warteschlange(ErstelltAm)
    CREATE INDEX IX_Warteschlange_Ankunft ON Warteschlange(Ankunft)  -- NEU: Index für Ankunft
END
ELSE
BEGIN
                -- Migration bestehender Daten falls Tabelle schon existiert
                -- Prüfen und neue Spalten hinzufügen falls nötig
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                  WHERE TABLE_NAME = 'Warteschlange' AND COLUMN_NAME = 'Aufgerufen')
    BEGIN
        ALTER TABLE Warteschlange ADD Aufgerufen datetime NULL
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                  WHERE TABLE_NAME = 'Warteschlange' AND COLUMN_NAME = 'Behandlungsende')
    BEGIN
        ALTER TABLE Warteschlange ADD Behandlungsende datetime NULL
    END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                              WHERE TABLE_NAME = 'Warteschlange' AND COLUMN_NAME = 'PatNr')
                BEGIN
                    -- Migration von alter Struktur
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                              WHERE TABLE_NAME = 'PatientenAufrufe' AND COLUMN_NAME = 'PatientenID')
                    BEGIN
                        -- Daten von alter Tabelle migrieren
                        EXEC sp_rename 'PatientenAufrufe', 'Warteschlange_OLD'
                        
                        -- Neue Tabelle mit korrekter Struktur erstellen
                        -- (Code von oben wiederholen)
                        
                        -- Daten migrieren
                        INSERT INTO Warteschlange (PatNr, Name, Bereich, Status, Ankunft)
                        SELECT PatientenID, 
                               ISNULL(Vorname + ' ' + Nachname, 'Patient ' + PatientenID),
                               Zimmer,
                               Status,
                               ErstelltAm
                        FROM Warteschlange_OLD
                        WHERE CAST(ErstelltAm AS DATE) = CAST(GETDATE() AS DATE)
                    END
                END
            END"

                Using cmd As New SqlCommand(createTableSQL, conn)
                    cmd.ExecuteNonQuery()
                End Using

                ' Bereich-Tabelle für verfügbare Zimmer/Bereiche
                Dim createBereicheSQL As String = "
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Bereiche')
            BEGIN
                CREATE TABLE Bereiche (
                    ID int IDENTITY(1,1) PRIMARY KEY,
                    Bezeichnung nvarchar(50) NOT NULL UNIQUE,
                    Typ nvarchar(20) DEFAULT 'Zimmer',          -- 'Zimmer', 'Labor', 'Arzt'
                    Aktiv bit DEFAULT 1,
                    Reihenfolge int DEFAULT 0
                )
                
                -- Standard-Bereiche einfügen
                INSERT INTO Bereiche (Bezeichnung, Typ, Reihenfolge) VALUES 
                ('Wartezimmer', 'Zimmer', 1),
                ('Labor', 'Labor', 2),
                ('Zimmer 1 - Allgemeinmedizin', 'Zimmer', 3),
                ('Zimmer 2 - Untersuchung', 'Zimmer', 4),
                ('Zimmer 3 - Behandlung', 'Zimmer', 5),
                ('Zimmer 4 - Ultraschall', 'Zimmer', 6),
                ('Zimmer 5 - EKG', 'Zimmer', 7),
                ('Anmeldung', 'Sonstige', 0)
            END"

                Using cmd As New SqlCommand(createBereicheSQL, conn)
                    cmd.ExecuteNonQuery()
                End Using

                ' System-Konfiguration
                Dim createConfigSQL As String = "
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SystemConfig')
            BEGIN
                CREATE TABLE SystemConfig (
                    ConfigKey nvarchar(50) PRIMARY KEY,
                    ConfigValue nvarchar(500),
                    Beschreibung nvarchar(200)
                )
                
                INSERT INTO SystemConfig VALUES 
                ('ServerMode', 'Auto', 'Auto/Server/Client'),
                ('ServerIP', '192.168.1.100', 'IP-Adresse des PAS-Servers'),
                ('ServerPort', '8080', 'Port für Web-Interface'),
                ('DefaultWartezeit', '10', 'Geschätzte Wartezeit pro Patient in Minuten'),
                ('AutoRefresh', '5000', 'Aktualisierungsintervall in ms')
            END"

                Using cmd As New SqlCommand(createConfigSQL, conn)
                    cmd.ExecuteNonQuery()
                End Using

                ' System-Konfiguration
                Dim createBehandlungsstatistik As String = "
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Behandlungsstatistik')
            BEGIN
                CREATE TABLE Behandlungsstatistik (
                        ID int IDENTITY(1,1) PRIMARY KEY,
                        Bereich nvarchar(50),
                        Prioritaet int,
                        DurchschnittsDauer int DEFAULT 15,  -- Minuten
                        AnzahlBehandlungen int DEFAULT 0,
                        LetzteAktualisierung datetime DEFAULT GETDATE()
                    )

-- Standardwerte einfügen
INSERT INTO Behandlungsstatistik (Bereich, Prioritaet, DurchschnittsDauer) VALUES 
('Zimmer 1', 1, 15),  -- Normal
('Zimmer 1', 2, 10),  -- Dringend  
('Zimmer 1', 3, 20),  -- Notfall
('Zimmer 2', 1, 12),
('Zimmer 3', 1, 18),
('Labor', 1, 8),
('Anmeldung', 1, 5)
END"

                Using cmd As New SqlCommand(createBehandlungsstatistik, conn)
                    cmd.ExecuteNonQuery()
                End Using




                ' System-Konfiguration
                Dim createFarbtabelleSQL As String = "
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Farbkonfiguration')
BEGIN
    CREATE TABLE Farbkonfiguration (
        StatusName NVARCHAR(50) PRIMARY KEY,
        FarbeHex NVARCHAR(7) NOT NULL,
        Beschreibung NVARCHAR(100)
    );
    
    -- Kräftigere Standard-Farben einfügen
    INSERT INTO Farbkonfiguration (StatusName, FarbeHex, Beschreibung) VALUES
    ('wartend', '#FF9800', 'Kräftiges Orange'),
    ('aufgerufen', '#4CAF50', 'Kräftiges Grün'),
    ('in Behandlung', '#2196F3', 'Kräftiges Blau'),
    ('gegangen', '#9E9E9E', 'Mittelgrau'),
    ('Notfall', '#F44336', 'Kräftiges Rot'),
    ('NotfallBlink', '#FFCDD2', 'Hellrot für Blink');
    
    PRINT 'Farbkonfiguration-Tabelle wurde erfolgreich erstellt';
END
ELSE
BEGIN
    PRINT 'Farbkonfiguration-Tabelle existiert bereits';
END
GO

-- Optional: Farben aktualisieren wenn Tabelle schon existiert
-- UPDATE Farbkonfiguration SET FarbeHex = '#FF9800' WHERE StatusName = 'wartend';
-- UPDATE Farbkonfiguration SET FarbeHex = '#4CAF50' WHERE StatusName = 'aufgerufen';
-- UPDATE Farbkonfiguration SET FarbeHex = '#2196F3' WHERE StatusName = 'in Behandlung';
-- UPDATE Farbkonfiguration SET FarbeHex = '#9E9E9E' WHERE StatusName = 'gegangen';
-- UPDATE Farbkonfiguration SET FarbeHex = '#F44336' WHERE StatusName = 'Notfall';
-- UPDATE Farbkonfiguration SET FarbeHex = '#FFCDD2' WHERE StatusName = 'NotfallBlink';"

                Using cmd As New SqlCommand(createFarbtabelleSQL, conn)
                    cmd.ExecuteNonQuery()
                End Using



                If MainForm IsNot Nothing Then MainForm.lblStatus.Text = "PAS-Datenbank initialisiert"
                If MainForm IsNot Nothing Then MainForm.lblStatus.ForeColor = Color.Green

                ' WICHTIG: KEINE Test-Daten mehr einfügen!
                ' Die Tabelle bleibt leer bis echte Daten eingegeben werden

            End Using

        Catch ex As SqlException
            Select Case ex.Number
                Case 2
                    Logger.Debug("SQL Server 'SILINSQL\PatientenAufruf' nicht erreichbar.")
                Case 18456
                    Logger.Debug("Anmeldung an SQL Server fehlgeschlagen.")
                Case Else
                    Logger.Debug($"SQL-Fehler ({ex.Number}): {ex.Message}")
            End Select

            If MainForm IsNot Nothing Then MainForm.lblStatus.Text = $"SQL-Fehler: {ex.Number}"
            If MainForm IsNot Nothing Then MainForm.lblStatus.ForeColor = Color.Red

        Catch ex As Exception
            Logger.Debug("Fehler bei Datenbankinitialisierung: " & ex.Message)
            If MainForm IsNot Nothing Then MainForm.lblStatus.Text = "Datenbankfehler"
            If MainForm IsNot Nothing Then MainForm.lblStatus.ForeColor = Color.Red
        End Try
    End Sub


    ' Weitere Datenbank-Hilfsfunktionen


    Public Sub SetConfigValue(key As String, value As String)
        ' Config-Werte in SystemConfig Tabelle speichern
    End Sub
End Module
