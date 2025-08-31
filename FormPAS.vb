Imports System.Data.SqlClient
Imports System.Net.Http
Imports Newtonsoft.Json
Imports System.Text
Imports System.IO
Imports System.Configuration

Public Class FormPAS
    Public Property GridManager As PatientenGridManager
    Public Property BereicheManager As BereicheManager
    Public Property ServerComm As ServerKommunikation
    Public Property UpdateManager As UpdateManager

    Private tagesMitDaten As New Dictionary(Of Date, Integer) ' Datum -> Anzahl Patienten
    Private isUpdating As Boolean = False

    Public Property IsBlinking As Boolean = False  ' Für Blink-Effekt

    Private blinkState As Boolean = False
    Private WithEvents timerNotfallBlink As New Timer()
    ' In FormPAS.vb ein ContextMenuStrip hinzufügen
    Private WithEvents contextMenuPatient As New ContextMenuStrip()

    Private lastNotfallSound As DateTime = DateTime.MinValue
    Private notfallSoundEnabled As Boolean = True

    ' Controls die andere Klassen brauchen
    Public ReadOnly Property PatientenGrid As DataGridView
        Get
            Return dgvPatienten
        End Get
    End Property

    Public ReadOnly Property StatusLabel As ToolStripStatusLabel
        Get
            Return lblStatus
        End Get
    End Property

    Private Async Sub FormPAS_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        GridManager = New PatientenGridManager(dgvPatienten, Me)
        BereicheManager = New BereicheManager(TreeView1, Me)
        ServerComm = New ServerKommunikation(Me)
        UpdateManager = New UpdateManager(Me, GridManager, ServerComm)

        ' Event-Handler registrieren
        AddHandler GDTModule.NeuerGDTPatient, AddressOf NeuerPatientVonGDT
        AddHandler GDTModule.NeuerXMLPatient, AddressOf NeuerPatientVonXML

        'Alarmsound
        Dim mnuSound As New ToolStripMenuItem("🔊 Alarm-Sound")
        mnuSound.Name = "mnuSound"
        AddHandler mnuSound.Click, AddressOf ToggleNotfallSound
        ToolStrip1.Items.Add(mnuSound)

        ' Bereiche vom Server laden
        Await ConfigModule.LadeBereicheAsync()

        ' TreeView aufbauen
        BereicheManager.BuildBereicheTreeView() ' Mit ConfigModule.BereicheListe


        GridManager.InitializeGrid()

        ' Selection-Farben transparent machen (HIER EINFÜGEN)
        'dgvPatienten.DefaultCellStyle.SelectionBackColor = Color.Transparent
        'dgvPatienten.DefaultCellStyle.SelectionForeColor = Color.Black
        'dgvPatienten.RowHeadersDefaultCellStyle.SelectionBackColor = Color.Transparent


        DatabaseModule.MainForm = Me

        ' Timer für automatische Aktualisierung
        timerRefresh.Interval = 5000 ' 5 Sekunden
        timerRefresh.Start()

        ' TreeView mit Filteroptionen befüllen
        'InitializeTreeView()

        ' Erste Ladung
        UpdateManager.IntelligentesUpdate()

        'Kontextmenü initialisieren
        InitializeContextMenu()

        ' Zeit-Anzeige starten
        Dim timerZeit As New Timer()
        timerZeit.Interval = 1000
        AddHandler timerZeit.Tick, Sub() lblZeit.Text = DateTime.Now.ToString("HH:mm:ss")
        timerZeit.Start()

        ' TreeView Event Handler
        AddHandler TreeView1.AfterCheck, AddressOf TreeView1_AfterCheck

        ' Monatsübersicht laden
        LadeMonatsuebersicht(DateTime.Today)

        ' GDT-Überwachung starten
        GDTModule.StartGDTMonitoring()
        GDTModule.ProcessExistingGDTFiles() ' Vorhandene Dateien verarbeiten

        ' Notfall-Blink Timer konfigurieren
        timerNotfallBlink.Interval = 500 ' Alle 500ms blinken
        timerNotfallBlink.Start()

        'Sound bei Notfall konfigurieren
        Try
            Using conn As New SqlConnection(ConfigModule.SqlConnectionString)
                conn.Open()
                Dim cmd As New SqlCommand("SELECT ConfigValue FROM SystemConfig WHERE ConfigKey = 'NotfallSound'", conn)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then
                    notfallSoundEnabled = (result.ToString() = "1")
                End If
            End Using
        Catch ex As Exception
            ' Standard: Sound aktiviert
            notfallSoundEnabled = True
        End Try

    End Sub

    Private Sub InitializeContextMenu()
        ' Neues Kontextmenü erstellen
        contextMenuPatient = New ContextMenuStrip()

        ' Menüeinträge erstellen
        Dim menuCheckIn = New ToolStripMenuItem("Check-In (Geplant → Wartend)")
        Dim menuAufruf = New ToolStripMenuItem("Aufruf")
        Dim menuInBehandlung = New ToolStripMenuItem("In Behandlung")
        Dim menuFertig = New ToolStripMenuItem("Fertig")
        Dim menuBearbeiten = New ToolStripMenuItem("Bearbeiten")
        Dim menuLoeschen = New ToolStripMenuItem("Löschen")

        ' Event-Handler zuweisen
        AddHandler menuCheckIn.Click, AddressOf ContextMenu_CheckIn
        AddHandler menuAufruf.Click, AddressOf ContextMenu_Aufruf
        AddHandler menuInBehandlung.Click, AddressOf ContextMenu_InBehandlung
        AddHandler menuFertig.Click, AddressOf ContextMenu_Fertig
        AddHandler menuBearbeiten.Click, AddressOf ContextMenu_Bearbeiten
        AddHandler menuLoeschen.Click, AddressOf ContextMenu_Loeschen

        ' Zum Kontextmenü hinzufügen
        contextMenuPatient.Items.Add(menuCheckIn)
        contextMenuPatient.Items.Add(New ToolStripSeparator())
        contextMenuPatient.Items.Add(menuAufruf)
        contextMenuPatient.Items.Add(menuInBehandlung)
        contextMenuPatient.Items.Add(menuFertig)
        contextMenuPatient.Items.Add(New ToolStripSeparator())
        contextMenuPatient.Items.Add(menuBearbeiten)
        contextMenuPatient.Items.Add(menuLoeschen)

        ' Dem Grid zuweisen
        dgvPatienten.ContextMenuStrip = contextMenuPatient
    End Sub



    ' Event-Handler für Kontextmenü
    Private Sub ContextMenu_CheckIn(sender As Object, e As EventArgs)
        If dgvPatienten.CurrentRow Is Nothing Then Return

        Dim row = dgvPatienten.CurrentRow
        Dim status = row.Cells("Status").Value?.ToString()

        If status = "Geplant" Then
            row.Cells("Status").Value = "Wartend"
            row.Cells("Ankunftszeit").Value = DateTime.Now

            Dim patientenID = row.Cells("PatientenID").Value?.ToString()
            Task.Run(Async Function()
                         Await ServerComm.StatusUpdate(patientenID, "Wartend", DateTime.Now)
                         Return True
                     End Function)

            GridManager.FaerbeZeilenNachStatus()
            GridManager.SortierePatienten()
        End If
    End Sub

    Private Sub ContextMenu_Aufruf(sender As Object, e As EventArgs)
        If dgvPatienten.CurrentRow IsNot Nothing Then
            btnAufruf.PerformClick()
        End If
    End Sub

    Private Sub ContextMenu_InBehandlung(sender As Object, e As EventArgs)
        If dgvPatienten.CurrentRow IsNot Nothing Then
            btnInBehandlung.PerformClick()
        End If
    End Sub

    Private Sub ContextMenu_Fertig(sender As Object, e As EventArgs)
        If dgvPatienten.CurrentRow IsNot Nothing Then
            btnFertig.PerformClick()
        End If
    End Sub

    Private Sub ContextMenu_Bearbeiten(sender As Object, e As EventArgs)
        If dgvPatienten.CurrentRow IsNot Nothing Then
            BearbeitePatient(dgvPatienten.CurrentRow)
        End If
    End Sub

    Private Sub ContextMenu_Loeschen(sender As Object, e As EventArgs)
        If dgvPatienten.CurrentRow IsNot Nothing Then
            btnLoeschen.PerformClick()
        End If
    End Sub

    ' Kontextmenü dynamisch anpassen basierend auf Status
    Private Sub dgvPatienten_CellMouseDown(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvPatienten.CellMouseDown
        If e.Button = MouseButtons.Right AndAlso e.RowIndex >= 0 Then
            dgvPatienten.CurrentCell = dgvPatienten.Rows(e.RowIndex).Cells(e.ColumnIndex)

            Dim status = dgvPatienten.CurrentRow.Cells("Status").Value?.ToString()

            ' Menüeinträge basierend auf Status aktivieren/deaktivieren
            For Each item As ToolStripItem In contextMenuPatient.Items
                If TypeOf item Is ToolStripMenuItem Then
                    Dim menuItem = CType(item, ToolStripMenuItem)
                    Select Case menuItem.Text
                        Case "Check-In (Geplant → Wartend)"
                            menuItem.Visible = (status = "Geplant")
                        Case "Aufruf"
                            menuItem.Enabled = (status = "Wartend")
                        Case "In Behandlung"
                            menuItem.Enabled = (status = "Aufgerufen")
                        Case "Fertig"
                            menuItem.Enabled = (status = "InBehandlung")
                    End Select
                End If
            Next
        End If
    End Sub

    'Timer-Event für Blink-Effekt
    Private Sub timerNotfallBlink_Tick(sender As Object, e As EventArgs) Handles timerNotfallBlink.Tick
        blinkState = Not blinkState

        Dim hatNotfall As Boolean = False  ' <-- HIER DEKLARIEREN

        For Each row As DataGridViewRow In dgvPatienten.Rows
            If row.IsNewRow Then Continue For

            Dim prioritaet = CInt(If(row.Cells("PrioritaetWert").Value, 0))
            Dim status = row.Cells("Status").Value?.ToString()

            If prioritaet = 2 AndAlso (status = "Wartend" OrElse status = "Aufgerufen") Then
                hatNotfall = True  ' <-- HIER SETZEN

                If blinkState Then
                    row.DefaultCellStyle.BackColor = Color.Red
                    row.DefaultCellStyle.ForeColor = Color.White
                Else
                    row.DefaultCellStyle.BackColor = Color.Yellow
                    row.DefaultCellStyle.ForeColor = Color.Black
                End If
            End If
        Next

        ' Sound-Block debuggen
        If hatNotfall AndAlso notfallSoundEnabled Then
            If DateTime.Now.Subtract(lastNotfallSound).TotalSeconds > 10 Then
                Try
                    ' Verschiedene Sound-Methoden testen
                    Console.Beep(1000, 500)  ' Einfacher Beep als Test
                    ' Oder: My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)

                    lastNotfallSound = DateTime.Now
                    Logger.Debug("Sound ausgelöst")
                Catch ex As Exception
                    Logger.Debug($"Sound-Fehler: {ex.Message}")
                End Try
            End If
        End If

        ' Sound alle 30 Sekunden wenn Notfall vorhanden
        'If hatNotfall AndAlso notfallSoundEnabled Then
        '    If DateTime.Now.Subtract(lastNotfallSound).TotalSeconds > 10 Then
        '        Try
        '            My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)
        '            lastNotfallSound = DateTime.Now
        '        Catch ex As Exception
        '            Logger.Debug($"Sound-Fehler: {ex.Message}")
        '        End Try
        '    End If
        'End If
    End Sub


    ' Event-Handler ohne Handles-Klausel
    Private Sub ToggleNotfallSound(sender As Object, e As EventArgs)
        notfallSoundEnabled = Not notfallSoundEnabled

        Using conn As New SqlConnection(ConfigModule.SqlConnectionString)
            conn.Open()
            ' Erst prüfen ob Eintrag existiert
            Dim checkCmd As New SqlCommand("SELECT COUNT(*) FROM SystemConfig WHERE ConfigKey = 'NotfallSound'", conn)
            Dim exists = CInt(checkCmd.ExecuteScalar()) > 0

            If exists Then
                Dim cmd As New SqlCommand("UPDATE SystemConfig SET ConfigValue = @val WHERE ConfigKey = 'NotfallSound'", conn)
                cmd.Parameters.AddWithValue("@val", If(notfallSoundEnabled, "1", "0"))
                cmd.ExecuteNonQuery()
            Else
                Dim cmd As New SqlCommand("INSERT INTO SystemConfig (ConfigKey, ConfigValue) VALUES ('NotfallSound', @val)", conn)
                cmd.Parameters.AddWithValue("@val", If(notfallSoundEnabled, "1", "0"))
                cmd.ExecuteNonQuery()
            End If
        End Using

        Dim menuItem = CType(sender, ToolStripMenuItem)
        menuItem.Text = If(notfallSoundEnabled, "🔊 Alarm-Sound", "🔇 Alarm-Sound")
    End Sub
    Private Async Function LadeBereicheVomServer() As Task
        Try
            Logger.Debug("Lade Bereiche vom Server...")
            Dim response = Await httpClient.GetAsync($"{serviceUrl}/api/bereiche")
            If response.IsSuccessStatusCode Then
                Dim json = Await response.Content.ReadAsStringAsync()
                Logger.Debug($"Bereiche JSON: {json}")
                BereicheListe = JsonConvert.DeserializeObject(Of List(Of Bereich))(json)
                Logger.Debug($"Anzahl Bereiche geladen: {BereicheListe.Count}")
            Else
                Logger.Debug($"Fehler beim Laden: {response.StatusCode}")
            End If
        Catch ex As Exception
            ' Fallback auf Standard-Bereiche
            Logger.Debug($"Fehler beim Laden der Bereiche: {ex.Message}")
        End Try
    End Function

    ' Hilfsfunktion: Priorität-Zahl in Text umwandeln
    Private Function GetPrioritaetText(prioritaet As Integer) As String
        Select Case prioritaet
            Case 2
                Return "NOTFALL"
            Case 1
                Return "Dringend"
            Case Else
                Return "Normal"
        End Select
    End Function

    ' Handler für DataError Events (z.B. ungültige ComboBox-Werte)
    Private Sub dgvPatienten_DataError(sender As Object, e As DataGridViewDataErrorEventArgs)
        ' Fehler bei ComboBox-Spalte ignorieren und Standardwert setzen
        If e.ColumnIndex = dgvPatienten.Columns("Zimmer").Index Then
            dgvPatienten.Rows(e.RowIndex).Cells(e.ColumnIndex).Value = ""
            e.ThrowException = False
        End If
    End Sub
    Private Sub dgvPatienten_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPatienten.CellDoubleClick
        If e.RowIndex < 0 Then Return

        Dim row = dgvPatienten.Rows(e.RowIndex)
        BearbeitePatient(row)
    End Sub

    Private Sub BearbeitePatient(row As DataGridViewRow)
        Dim patientenID = row.Cells("PatientenID").Value?.ToString()
        If String.IsNullOrEmpty(patientenID) Then Return

        Using frm As New FormPatientEingabe()
            frm.Text = "Patient bearbeiten"

            ' Erst ShowDialog aufrufen (lädt das Form und füllt clbZimmer)
            ' aber mit einer Hilfsvariable arbeiten
            Dim zimmerTemp = row.Cells("Zimmer").Value?.ToString()

            ' Properties setzen die nicht von Controls abhängen
            frm.PatientenID = patientenID
            Dim nameParts = row.Cells("Name").Value?.ToString().Split(","c)
            If nameParts?.Length > 0 Then
                frm.Nachname = nameParts(0).Trim()
                If nameParts.Length > 1 Then
                    frm.Vorname = nameParts(1).Trim()
                End If
            End If
            frm.Prioritaet = CInt(If(row.Cells("PrioritaetWert").Value, 0))
            frm.Bemerkung = row.Cells("Bemerkung").Value?.ToString()
            frm.TerminZeit = CDate(row.Cells("Ankunftszeit").Value)
            frm.GewuenschtesDatum = CDate(row.Cells("Ankunftszeit").Value)

            ' Zimmer in einer Variable speichern
            frm.ZimmerTemp = zimmerTemp ' Neue Property für temporäre Speicherung

            If frm.ShowDialog(Me) = DialogResult.OK Then
                ' Zeile aktualisieren
                row.Cells("Name").Value = frm.PatientenName
                row.Cells("Ankunftszeit").Value = frm.TerminZeit
                row.Cells("PrioritaetWert").Value = frm.Prioritaet
                row.Cells("Prioritaet").Value = GetPrioritaetText(frm.Prioritaet)
                row.Cells("Bemerkung").Value = frm.Bemerkung
                row.Cells("Zimmer").Value = frm.Zimmer

                ' Updates an Server senden
                Dim unusedTask = Task.Run(Async Function()
                                              Try
                                                  Dim values As New Dictionary(Of String, String) From {
                                {"patientenID", frm.PatientenID},
                                {"name", frm.PatientenName},
                                {"prioritaet", frm.Prioritaet.ToString()},
                                {"zimmer", frm.Zimmer},
                                {"bemerkung", frm.Bemerkung},
                                {"status", row.Cells("Status").Value?.ToString()}
                            }

                                                  Dim content = New FormUrlEncodedContent(values)
                                                  Dim response = Await HttpClient.PostAsync($"{ServiceUrl}/api/updatepatient", content)

                                                  If Not response.IsSuccessStatusCode Then
                                                      Logger.Debug($"Fehler beim Update: {response.StatusCode}")
                                                  End If
                                              Catch ex As Exception
                                                  Logger.Debug($"Fehler beim Server-Update: {ex.Message}")
                                              End Try
                                              Return True
                                          End Function)
            End If
        End Using
    End Sub

    Private Sub MonthCalendar1_DateChanged(sender As Object, e As DateRangeEventArgs) Handles MonthCalendar1.DateChanged
        Dim ausgewaehltesDatum As Date = e.Start.Date

        ' Timer sofort stoppen
        timerRefresh.Stop()

        ' Grid leeren für sofortiges Feedback
        dgvPatienten.Rows.Clear()
        lblStatus.Text = "Lade Daten..."
        Application.DoEvents() ' UI sofort aktualisieren

        If ausgewaehltesDatum = DateTime.Today Then
            ' LIVE-Modus
            lblTagesansicht.Text = "LIVE - Aktuelle Warteschlange"
            lblTagesansicht.ForeColor = Color.Green
            lblTagesansicht.Font = New Font(lblTagesansicht.Font, FontStyle.Bold)

            ' History-Spalten entfernen wenn nötig
            If dgvPatienten.Columns.Contains("Aufgerufen") Then
                dgvPatienten.Columns.Remove("Aufgerufen")
                dgvPatienten.Columns.Remove("Behandlungsbeginn")
                dgvPatienten.Columns.Remove("Behandlungsende")
            End If

            ' Buttons aktivieren
            btnCheckIn.Enabled = True
            btnAufruf.Enabled = True
            btnInBehandlung.Enabled = True
            btnFertig.Enabled = True
            btnExport.Text = "Export"

            ' Sofort Daten laden und anzeigen (ERSETZT IntelligentesUpdate())
            Task.Run(Async Function()
                         Try
                             Dim neueDaten = Await ServerComm.HoleDatenVomServer()

                             Me.Invoke(Sub()
                                           dgvPatienten.Rows.Clear()
                                           For Each patient In neueDaten
                                               Dim index = dgvPatienten.Rows.Add()
                                               Dim row = dgvPatienten.Rows(index)

                                               row.Cells("PatientenID").Value = patient.PatientenID
                                               row.Cells("Name").Value = patient.Name
                                               row.Cells("Ankunftszeit").Value = patient.Ankunftszeit
                                               row.Cells("Wartezeit").Value = $"{CInt(Math.Floor((DateTime.Now - patient.Ankunftszeit).TotalMinutes))} min"
                                               row.Cells("Zimmer").Value = patient.Zimmer
                                               row.Cells("Status").Value = patient.Status
                                               row.Cells("PrioritaetWert").Value = patient.Prioritaet
                                               row.Cells("Prioritaet").Value = GetPrioritaetText(patient.Prioritaet)
                                               row.Cells("Bemerkung").Value = patient.Bemerkung
                                           Next

                                           GridManager.FaerbeZeilenNachStatus()
                                           UpdateStatusBar()
                                       End Sub)
                         Catch ex As Exception
                             Logger.Debug($"Fehler beim Laden der Live-Daten: {ex.Message}")
                         End Try
                         Return True
                     End Function)

            ' Timer verzögert starten (behält die alte Zeile bei)
            Task.Delay(1000).ContinueWith(Sub(t)
                                              Me.Invoke(Sub() timerRefresh.Start())
                                          End Sub)

            ' Timer verzögert starten
            Task.Delay(1000).ContinueWith(Sub(t)
                                              Me.Invoke(Sub() timerRefresh.Start())
                                          End Sub)

        Else
            ' Historie/Planung
            If ausgewaehltesDatum > DateTime.Today Then
                lblTagesansicht.Text = $"PLANUNG - {ausgewaehltesDatum:dddd, dd.MM.yyyy}"
                lblTagesansicht.ForeColor = Color.DarkOrange
                lblTagesansicht.Font = New Font(lblTagesansicht.Font, FontStyle.Regular)
                SetzeHistorieModus(True)
                LadeTermineFuerTag(ausgewaehltesDatum)
            Else
                lblTagesansicht.Text = $"HISTORIE - {ausgewaehltesDatum:dddd, dd.MM.yyyy}"
                lblTagesansicht.ForeColor = Color.Blue
                lblTagesansicht.Font = New Font(lblTagesansicht.Font, FontStyle.Regular)
                SetzeHistorieModus(True)
                LadeHistorieFuerTag(ausgewaehltesDatum)
            End If
        End If
    End Sub

    ' Monatsübersicht laden für Kalender-Markierungen
    Private Async Sub LadeMonatsuebersicht(datum As Date)
        Try
            Dim ersterTag = New Date(datum.Year, datum.Month, 1)
            Dim letzterTag = ersterTag.AddMonths(1).AddDays(-1)

            ' Statistiken vom Server holen
            Dim response = Await httpClient.GetAsync(
                $"{serviceUrl}/api/monatsstatistik?von={ersterTag:yyyy-MM-dd}&bis={letzterTag:yyyy-MM-dd}")

            If response.IsSuccessStatusCode Then
                Dim json = Await response.Content.ReadAsStringAsync()
                tagesMitDaten = JsonConvert.DeserializeObject(Of Dictionary(Of Date, Integer))(json)

                ' Kalender neu zeichnen mit Markierungen
                MonthCalendar1.RemoveAllBoldedDates()

                For Each kvp As KeyValuePair(Of Date, Integer) In tagesMitDaten
                    If kvp.Value > 0 Then
                        ' Tage mit Daten fett markieren
                        MonthCalendar1.AddBoldedDate(kvp.Key)
                    End If
                Next

                MonthCalendar1.UpdateBoldedDates()
            End If

        Catch ex As Exception
            Logger.Debug($"Fehler beim Laden der Monatsübersicht: {ex.Message}")
        End Try
    End Sub

    Private Async Sub LadeHistorieFuerTag(datum As Date)
        Try
            Me.Cursor = Cursors.WaitCursor
            dgvPatienten.Rows.Clear()

            ' Verwende dieselbe API wie für Termine - holt ALLE Einträge des Tages
            Dim response = Await httpClient.GetAsync($"{serviceUrl}/api/warteschlange?datum={datum:yyyy-MM-dd}")

            If response.IsSuccessStatusCode Then
                Dim json = Await response.Content.ReadAsStringAsync()
                Dim eintraege = JsonConvert.DeserializeObject(Of List(Of PatientInfo))(json)

                ' Nach Ankunftszeit sortieren
                eintraege = eintraege.OrderBy(Function(t) t.Ankunftszeit).ToList()

                ' Grid befüllen
                For Each eintrag In eintraege
                    Dim index = dgvPatienten.Rows.Add()
                    Dim row = dgvPatienten.Rows(index)

                    row.Cells("PatientenID").Value = eintrag.PatientenID
                    row.Cells("Name").Value = eintrag.Name
                    row.Cells("Ankunftszeit").Value = eintrag.Ankunftszeit

                    ' Wartezeit berechnen (für Historie: Zeit zwischen Ankunft und Ende des Tages oder Behandlungsende)
                    row.Cells("Wartezeit").Value = If(eintrag.Wartezeit > 0, $"{eintrag.Wartezeit} min", "-")

                    ' Zimmer und Status aus DB
                    row.Cells("Zimmer").Value = eintrag.Zimmer
                    row.Cells("Status").Value = eintrag.Status
                    row.Cells("PrioritaetWert").Value = eintrag.Prioritaet
                    row.Cells("Prioritaet").Value = GetPrioritaetText(eintrag.Prioritaet)
                    row.Cells("Bemerkung").Value = eintrag.Bemerkung

                    ' Status-Farben für Historie
                    Select Case eintrag.Status
                        Case "Wartend"
                            row.Cells("Status").Style.BackColor = Color.LightYellow
                            row.DefaultCellStyle.BackColor = Color.LightYellow ' Noch wartend = Problem!
                        Case "Aufgerufen"
                            row.Cells("Status").Style.BackColor = Color.LightBlue
                        Case "InBehandlung"
                            row.Cells("Status").Style.BackColor = Color.LightGreen
                        Case "Fertig"
                            row.Cells("Status").Style.BackColor = Color.LightGray
                            row.DefaultCellStyle.BackColor = Color.WhiteSmoke
                        Case Else
                            row.Cells("Status").Style.BackColor = Color.White
                    End Select

                    ' Prioritäts-Formatierung bleibt gleich
                    Select Case eintrag.Prioritaet
                        Case 2 ' Notfall
                            row.DefaultCellStyle.ForeColor = Color.Red
                            row.Cells("Prioritaet").Style.BackColor = Color.LightCoral
                        Case 1 ' Dringend
                            row.DefaultCellStyle.ForeColor = Color.OrangeRed
                            row.Cells("Prioritaet").Style.BackColor = Color.LightYellow
                        Case Else ' Normal
                            row.DefaultCellStyle.ForeColor = Color.DarkGreen
                            row.Cells("Prioritaet").Style.BackColor = Color.LightGreen
                    End Select
                Next

                ' Statistik anzeigen
                If eintraege.Count > 0 Then
                    Dim wartend = eintraege.Where(Function(e) e.Status = "Wartend").Count()
                    Dim fertig = eintraege.Where(Function(e) e.Status = "Fertig").Count()
                    lblStatus.Text = $"Historie {datum:dd.MM.yyyy}: {eintraege.Count} Patienten | " &
                               $"Behandelt: {fertig} | Noch wartend: {wartend}"
                Else
                    lblStatus.Text = $"Keine Einträge für {datum:dd.MM.yyyy}"
                End If

            Else
                lblStatus.Text = $"Keine Daten für {datum:dd.MM.yyyy} verfügbar"
            End If

        Catch ex As Exception
            lblStatus.Text = $"Fehler beim Laden der Historie"
            Logger.Debug($"Fehler: {ex.Message}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Async Sub LadeTermineFuerTag(datum As Date)
        Try
            Me.Cursor = Cursors.WaitCursor
            dgvPatienten.Rows.Clear()

            ' Termine vom Server abrufen
            Dim response = Await HttpClient.GetAsync($"{ServiceUrl}/api/warteschlange?datum={datum:yyyy-MM-dd}")

            If response.IsSuccessStatusCode Then
                Dim json = Await response.Content.ReadAsStringAsync()
                Logger.Debug($"JSON empfangen: {json}")

                Dim termine = JsonConvert.DeserializeObject(Of List(Of PatientInfo))(json)
                Logger.Debug($"Deserialisiert: {termine.Count} Termine")


                ' Statt nur nach Ankunftszeit sortieren:
                ' termine = termine.OrderBy(Function(t) t.Ankunftszeit).ToList()

                ' Nach Priorität DANN Ankunftszeit sortieren:
                termine = termine.OrderByDescending(Function(t) t.Prioritaet).ThenBy(Function(t) t.Ankunftszeit).ToList()

                Logger.Debug($"Füge {termine.Count} Termine ins Grid ein")

                ' Grid befüllen
                ' In LadeTermineFuerTag - vereinfacht ohne Färbung
                For Each termin In termine
                    Try
                        Dim index = dgvPatienten.Rows.Add()
                        Dim row = dgvPatienten.Rows(index)

                        row.Cells("PatientenID").Value = termin.PatientenID
                        row.Cells("Name").Value = termin.Name
                        row.Cells("Ankunftszeit").Value = termin.Ankunftszeit
                        row.Cells("Wartezeit").Value = "-"
                        row.Cells("Zimmer").Value = If(String.IsNullOrEmpty(termin.Zimmer), "Wartezimmer", termin.Zimmer)
                        row.Cells("Status").Value = "Geplant"
                        row.Cells("PrioritaetWert").Value = termin.Prioritaet
                        row.Cells("Prioritaet").Value = GetPrioritaetText(termin.Prioritaet)
                        row.Cells("Bemerkung").Value = termin.Bemerkung

                    Catch ex As Exception
                        Logger.Debug($"Fehler beim Hinzufügen von Termin {termin.PatientenID}: {ex.Message}")
                    End Try
                Next

                'Patienten sortieren
                GridManager.SortierePatienten()

                ' Einmal zentral färben
                GridManager.FaerbeZeilenNachStatus()

                UpdateStatusBar()

                ' Statistik für geplante Termine
                If termine.Count > 0 Then
                    lblStatus.Text = $"Geplante Termine: {termine.Count} | " &
                                $"Erster: {termine.First().Ankunftszeit:HH:mm} | " &
                                $"Letzter: {termine.Last().Ankunftszeit:HH:mm}"
                Else
                    lblStatus.Text = $"Keine Termine für {datum:dd.MM.yyyy} vorhanden"
                End If

            Else
                ' Keine Termine gefunden oder Server nicht erreichbar
                lblStatus.Text = $"Keine Termine für {datum:dd.MM.yyyy} vorhanden"

                ' Info-Zeile hinzufügen
                Dim index = dgvPatienten.Rows.Add()
                Dim row = dgvPatienten.Rows(index)
                row.Cells("Name").Value = "-- Keine Termine geplant --"
                row.DefaultCellStyle.ForeColor = Color.Gray
                row.DefaultCellStyle.Font = New Font(dgvPatienten.Font, FontStyle.Italic)
            End If

        Catch ex As Exception
            lblStatus.Text = $"Fehler beim Laden der Termine"
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub


    Private Sub ZeigeTagesstatistik(daten As List(Of HistorieEintrag), datum As Date)
        If daten.Count = 0 Then
            If datum.DayOfWeek = DayOfWeek.Saturday OrElse datum.DayOfWeek = DayOfWeek.Sunday Then
                lblStatus.Text = "Wochenende - Praxis geschlossen"
            Else
                lblStatus.Text = "Keine Patienten an diesem Tag"
            End If
            Return
        End If

        Dim anzahlPatienten = daten.Count
        Dim durchschnittWartezeit = daten.Average(Function(h) h.Wartezeit)
        Dim maxWartezeit = daten.Max(Function(h) h.Wartezeit)
        Dim minWartezeit = daten.Min(Function(h) h.Wartezeit)

        lblStatus.Text = $"Patienten: {anzahlPatienten} | " &
                        $"Ø Wartezeit: {durchschnittWartezeit:F0} min | " &
                        $"Min: {minWartezeit} min | Max: {maxWartezeit} min"
    End Sub

    Private Sub SetzeHistorieModus(historieAktiv As Boolean)
        If historieAktiv Then
            If Not dgvPatienten.Columns.Contains("Aufgerufen") Then
                dgvPatienten.Columns.Insert(4, New DataGridViewTextBoxColumn With {
                    .Name = "Aufgerufen",
                    .HeaderText = "Aufgerufen",
                    .Width = 70
                })
                dgvPatienten.Columns.Insert(7, New DataGridViewTextBoxColumn With {
                    .Name = "Behandlungsbeginn",
                    .HeaderText = "Beh.Start",
                    .Width = 70
                })
                dgvPatienten.Columns.Insert(8, New DataGridViewTextBoxColumn With {
                    .Name = "Behandlungsende",
                    .HeaderText = "Beh.Ende",
                    .Width = 70
                })
            End If
            btnCheckIn.Enabled = False
            btnAufruf.Enabled = False
            btnInBehandlung.Enabled = False
            btnFertig.Enabled = False
            btnExport.Text = "📊 Export"

        Else
            If dgvPatienten.Columns.Contains("Aufgerufen") Then
                dgvPatienten.Columns.Remove("Aufgerufen")
                dgvPatienten.Columns.Remove("Behandlungsbeginn")
                dgvPatienten.Columns.Remove("Behandlungsende")
            End If

            btnCheckIn.Enabled = False
            btnAufruf.Enabled = True
            btnInBehandlung.Enabled = True
            btnFertig.Enabled = True
            btnExport.Text = "Export"
        End If
    End Sub
    Private Function BerechneWartezeit(patientenRow As DataGridViewRow) As Integer
        Dim position As Integer = 0  ' Außerhalb des Try-Blocks deklarieren

        Try
            Dim geschaetzteWartezeit As Integer = 0
            Dim patientPrio = CInt(patientenRow.Cells("PrioritaetWert").Value)
            Dim patientAnkunft = CDate(patientenRow.Cells("Ankunftszeit").Value)

            ' Position in Warteschlange bestimmen
            For Each row As DataGridViewRow In dgvPatienten.Rows
                If row.Cells("Status").Value?.ToString() = "Wartend" Then
                    Dim vergleichsPrio = CInt(row.Cells("PrioritaetWert").Value)
                    Dim vergleichsAnkunft = CDate(row.Cells("Ankunftszeit").Value)

                    ' Höhere Priorität oder gleiche Prio aber früher da
                    If vergleichsPrio > patientPrio OrElse
                   (vergleichsPrio = patientPrio AndAlso vergleichsAnkunft < patientAnkunft) Then
                        position += 1

                        ' Zeit addieren basierend auf Priorität
                        Select Case vergleichsPrio
                            Case 2  ' Notfall
                                geschaetzteWartezeit += 20
                            Case 1  ' Dringend
                                geschaetzteWartezeit += 10
                            Case Else  ' Normal
                                geschaetzteWartezeit += 15
                        End Select
                    End If
                End If
            Next

            ' Patienten in Behandlung berücksichtigen
            Dim inBehandlung = dgvPatienten.Rows.Cast(Of DataGridViewRow).
            Count(Function(r) r.Cells("Status").Value?.ToString() = "InBehandlung")

            ' Wenn Zimmer belegt, zusätzliche Zeit
            If inBehandlung > 0 Then
                geschaetzteWartezeit += 10  ' Restzeit für aktuelle Behandlung
            End If

            Return geschaetzteWartezeit

        Catch ex As Exception
            Return position * 15  ' Fallback: 15 Min pro Patient
        End Try
    End Function

    Public Sub UpdateStatusBar()
        Dim wartend = 0
        Dim inBehandlung = 0

        For Each row As DataGridViewRow In dgvPatienten.Rows
            If Not row.IsNewRow Then
                Select Case row.Cells("Status").Value?.ToString()
                    Case "Wartend", "Aufgerufen"
                        wartend += 1
                    Case "InBehandlung"
                        inBehandlung += 1
                End Select
            End If
        Next

        lblStatus.Text = $"Wartend: {wartend} | In Behandlung: {inBehandlung} | Gesamt: {dgvPatienten.RowCount}"
    End Sub

    Private Sub timerRefresh_Tick(sender As Object, e As EventArgs) Handles timerRefresh.Tick
        UpdateManager.IntelligentesUpdate()
    End Sub

    ' Timer pausieren bei Benutzerinteraktion
    Private Sub dgvPatienten_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles dgvPatienten.CellBeginEdit
        timerRefresh.Stop()
    End Sub

    Private Sub dgvPatienten_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPatienten.CellEndEdit
        timerRefresh.Start()
    End Sub

    Private Sub dgvPatienten_MouseDown(sender As Object, e As MouseEventArgs) Handles dgvPatienten.MouseDown
        timerRefresh.Stop()
        timerRefresh.Interval = 7000
        timerRefresh.Start()
    End Sub

    Private Sub dgvPatienten_MouseUp(sender As Object, e As MouseEventArgs) Handles dgvPatienten.MouseUp
        Dim unused = Task.Delay(2000).ContinueWith(Sub(t)
                                                       Me.Invoke(Sub()
                                                                     timerRefresh.Interval = 5000
                                                                 End Sub)
                                                   End Sub)
    End Sub

    ' Button Event Handler
    Private Async Sub btnAufruf_Click(sender As Object, e As EventArgs) Handles btnAufruf.Click
        If dgvPatienten.CurrentRow Is Nothing Then
            MessageBox.Show("Bitte wählen Sie einen Patienten aus.", "Hinweis")
            Return
        End If

        Dim patID = dgvPatienten.CurrentRow.Cells("PatientenID").Value?.ToString()
        If String.IsNullOrEmpty(patID) Then Return

        ' Status lokal aktualisieren
        dgvPatienten.CurrentRow.Cells("Status").Value = "Aufgerufen"
        dgvPatienten.CurrentRow.Cells("Status").Style.BackColor = Color.LightBlue

        ' An Server senden
        Try

            Dim values As New Dictionary(Of String, String) From {
            {"patientenID", patID},
            {"status", "Aufgerufen"},
            {"zeitpunkt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
        }

            Dim content = New FormUrlEncodedContent(values)
            Dim response = Await HttpClient.PostAsync($"{ServiceUrl}/api/statusupdate", content)

            If Not response.IsSuccessStatusCode Then
                MessageBox.Show("Status konnte nicht an Server übertragen werden", "Warnung")
                ' Status zurücksetzen bei Fehler
                dgvPatienten.CurrentRow.Cells("Status").Value = "Wartend"
                dgvPatienten.CurrentRow.Cells("Status").Style.BackColor = Color.LightYellow
            End If
        Catch ex As Exception
            Logger.Debug($"Fehler beim Senden: {ex.Message}")
        End Try
        GridManager.FaerbeZeilenNachStatus()
    End Sub

    Private Sub btnInBehandlung_Click(sender As Object, e As EventArgs) Handles btnInBehandlung.Click
        If dgvPatienten.CurrentRow IsNot Nothing Then
            Dim row = dgvPatienten.CurrentRow
            Dim patientenID = row.Cells("PatientenID").Value?.ToString()

            ' Status lokal setzen
            row.Cells("Status").Value = "InBehandlung"
            row.Cells("Status").Style.BackColor = Color.LightGreen

            ' Status an Server senden
            Task.Run(Async Function()
                         Await ServerComm.StatusUpdate(patientenID, "InBehandlung", DateTime.Now)
                         Return True
                     End Function)

            GridManager.FaerbeZeilenNachStatus()
        End If
    End Sub

    Private Sub btnFertig_Click(sender As Object, e As EventArgs) Handles btnFertig.Click
        If dgvPatienten.CurrentRow Is Nothing Then Return

        Dim row = dgvPatienten.CurrentRow
        Dim patientenID = row.Cells("PatientenID").Value?.ToString()

        ' Status auf Fertig setzen
        row.Cells("Status").Value = "Fertig"
        row.Cells("Status").Style.BackColor = Color.LightGray

        ' Werte aus der aktuellen Zeile holen
        Dim zimmer = row.Cells("Zimmer").Value?.ToString()
        Dim prioritaet = CInt(If(row.Cells("PrioritaetWert").Value, 1))

        ' Behandlungsdauer berechnen (von Ankunft bis jetzt)
        Dim ankunftszeit = CDate(row.Cells("Ankunftszeit").Value)
        Dim behandlungsDauer = Math.Floor((DateTime.Now - ankunftszeit).TotalMinutes)

        ' Status an Server senden
        Task.Run(Async Function()
                     Try
                         ' Status-Update
                         Dim statusValues As New Dictionary(Of String, String) From {
                {"patientenID", patientenID},
                {"status", "Fertig"},
                {"zeitpunkt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
            }

                         Await ServerComm.StatusUpdate(patientenID, "Fertig", DateTime.Now)

                     Catch ex As Exception
                         Logger.Debug($"Fehler: {ex.Message}")
                     End Try
                     Return True
                 End Function)
        GridManager.FaerbeZeilenNachStatus()
    End Sub

    ' Neu-Button in Toolbar - öffnet Eingabeformular
    Private Sub btnNeu_Click(sender As Object, e As EventArgs) Handles btnNeu.Click
        Using frm As New FormPatientEingabe()
            frm.IstBesucher = False
            frm.GewuenschtesDatum = MonthCalendar1.SelectionStart

            ' Status basierend auf Datum setzen
            Dim status As String = If(MonthCalendar1.SelectionStart.Date > Date.Today, "Geplant", "Wartend")


            If frm.ShowDialog(Me) = DialogResult.OK Then
                ServerComm.PatientManuellHinzufuegen(
                frm.PatientenID, frm.Vorname, frm.Nachname,
                frm.Prioritaet, frm.Bemerkung, frm.IstBesucher,
                frm.TerminZeit, frm.Zimmer, status)

                ' Grid manuell aktualisieren nach Hinzufügen
                Task.Delay(500).ContinueWith(Sub(t)
                                                 Me.Invoke(Sub()
                                                               UpdateManager.IntelligentesUpdate()
                                                           End Sub)
                                             End Sub)
            End If

        End Using
    End Sub



    ' GDT-Integration: Neuer Patient von Medical Office
    Public Sub NeuerPatientVonGDT(gdtPatient As GDTParser.GDTPatient)
        Try
            ' GDTPatient in PatientInfo konvertieren
            Dim patient As New PatientInfo With {
                .PatientenID = gdtPatient.PatientenID,
                .Name = $"{gdtPatient.Nachname}, {gdtPatient.Vorname}",
                .Ankunftszeit = DateTime.Now,
                .Status = "Wartend",
                .Zimmer = "Wartezimmer",
                .Wartezeit = 0,
                .Prioritaet = 0,
                .Bemerkung = " "
            }

            ' Prüfen ob Patient bereits in der Liste
            For Each row As DataGridViewRow In dgvPatienten.Rows
                If Not row.IsNewRow AndAlso row.Cells("PatientenID").Value?.ToString() = patient.PatientenID Then
                    ' Patient bereits vorhanden - nur Status aktualisieren
                    row.Cells("Status").Value = "Wartend"
                    row.Cells("Status").Style.BackColor = Color.LightYellow
                    row.Cells("Ankunftszeit").Value = DateTime.Now
                    row.Selected = True
                    dgvPatienten.CurrentCell = row.Cells(0)

                    ' Nachricht anzeigen
                    lblStatus.Text = $"Patient {patient.Name} wurde aktualisiert"
                    Return
                End If
            Next

            ' Neuen Patienten hinzufügen
            Dim index = dgvPatienten.Rows.Add()
            Dim newRow = dgvPatienten.Rows(index)

            newRow.Cells("PatientenID").Value = patient.PatientenID
            newRow.Cells("Name").Value = patient.Name
            newRow.Cells("Ankunftszeit").Value = patient.Ankunftszeit
            newRow.Cells("Wartezeit").Value = "0 min"
            newRow.Cells("Zimmer").Value = patient.Zimmer
            newRow.Cells("Status").Value = patient.Status
            newRow.Cells("PrioritaetWert").Value = patient.Prioritaet
            newRow.Cells("Prioritaet").Value = GetPrioritaetText(patient.Prioritaet)
            newRow.Cells("Bemerkung").Value = patient.Bemerkung

            ' Zusätzliche GDT-Infos in Bemerkung
            If Not String.IsNullOrEmpty(gdtPatient.Geburtsdatum) Then
                newRow.Cells("Bemerkung").Value &= $" | Geb: {gdtPatient.Geburtsdatum}"
            End If

            ' Status-Farbe
            newRow.Cells("Status").Style.BackColor = Color.LightYellow

            ' Neue Zeile markieren und sichtbar machen
            newRow.Selected = True
            dgvPatienten.CurrentCell = newRow.Cells(0)
            dgvPatienten.FirstDisplayedScrollingRowIndex = newRow.Index

            ' Kurzes Highlight für neue Zeile (Fire-and-Forget ist hier beabsichtigt)
            newRow.DefaultCellStyle.BackColor = Color.LightGreen
            Dim unused = Task.Delay(3000).ContinueWith(Sub(t)
                                                           Me.Invoke(Sub()
                                                                         If newRow.Index >= 0 AndAlso newRow.Index < dgvPatienten.Rows.Count Then
                                                                             newRow.DefaultCellStyle.BackColor = Color.White
                                                                         End If
                                                                     End Sub)
                                                       End Sub)

            ' Patient auch an Server senden (falls WebService läuft)
            Dim unusedTask1 = Task.Run(Async Function()
                                           Try
                                               Dim content = New FormUrlEncodedContent(New Dictionary(Of String, String) From {
            {"patientenID", patient.PatientenID},
            {"name", patient.Name},
            {"status", patient.Status},
            {"zimmer", patient.Zimmer},
            {"prioritaet", patient.Prioritaet.ToString()},
            {"ankunftszeit", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
            {"bemerkung", patient.Bemerkung}
        })

                                               Dim response = Await httpClient.PostAsync($"{serviceUrl}/api/neuerpatient", content)

                                               Me.Invoke(Sub()
                                                             Logger.Debug($"Server-Antwort: {response.StatusCode} - {response.ReasonPhrase}")
                                                         End Sub)


                                               If Not response.IsSuccessStatusCode Then
                                                   Me.Invoke(Sub()
                                                                 Logger.Debug($"Server-Fehler: {response.StatusCode} - {response.ReasonPhrase}")
                                                             End Sub)
                                               End If
                                           Catch ex As Exception
                                               Logger.Debug($"Fehler beim Senden an Server: {ex.Message}")
                                           End Try
                                       End Function)

            ' Statusmeldung
            lblStatus.Text = $"Patient {patient.Name} wurde hinzugefügt"

            ' Sound abspielen (optional)
            Try
                My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Asterisk)
            Catch
                ' Fehler beim Sound ignorieren
            End Try

            UpdateStatusBar()

        Catch ex As Exception
            Logger.Debug($"Fehler beim Hinzufügen des Patienten: {ex.Message}")
        End Try
    End Sub

    ' Direkte Integration von Medical Office (ohne GDT-Datei)
    Public Sub NeuerPatient(patientenID As String, vorname As String, nachname As String)
        Try
            ' PatientInfo erstellen
            Dim patient As New PatientInfo With {
                .PatientenID = patientenID,
                .Name = If(String.IsNullOrWhiteSpace(vorname), nachname, $"{nachname}, {vorname}"),
                .Ankunftszeit = DateTime.Now,
                .Status = "Wartend",
                .Zimmer = "Wartezimmer",
                .Wartezeit = 0,
                .Prioritaet = 1,
                .Bemerkung = "Direkt aus Medical Office"
            }

            ' Prüfen ob Patient bereits in der Liste
            For Each row As DataGridViewRow In dgvPatienten.Rows
                If Not row.IsNewRow AndAlso row.Cells("PatientenID").Value?.ToString() = patientenID Then
                    ' Patient bereits vorhanden - nur Status aktualisieren
                    row.Cells("Status").Value = "Wartend"
                    row.Cells("Status").Style.BackColor = Color.LightYellow
                    row.Cells("Ankunftszeit").Value = DateTime.Now
                    row.Cells("Wartezeit").Value = "0 min"
                    row.Selected = True
                    dgvPatienten.CurrentCell = row.Cells(0)

                    ' Nachricht anzeigen
                    lblStatus.Text = $"Patient {patient.Name} wurde aktualisiert (erneuter Aufruf)"

                    ' Sound für erneuten Aufruf
                    Try
                        My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)
                    Catch
                    End Try

                    Return
                End If
            Next

            ' Neuen Patienten hinzufügen
            Dim index = dgvPatienten.Rows.Add()
            Dim newRow = dgvPatienten.Rows(index)

            newRow.Cells("PatientenID").Value = patient.PatientenID
            newRow.Cells("Name").Value = patient.Name
            newRow.Cells("Ankunftszeit").Value = patient.Ankunftszeit
            newRow.Cells("Wartezeit").Value = "0 min"
            newRow.Cells("Zimmer").Value = patient.Zimmer
            newRow.Cells("Status").Value = patient.Status
            newRow.Cells("Prioritaet").Value = patient.Prioritaet
            newRow.Cells("Bemerkung").Value = patient.Bemerkung

            ' Status-Farbe
            newRow.Cells("Status").Style.BackColor = Color.LightYellow

            ' Neue Zeile markieren und sichtbar machen
            newRow.Selected = True
            dgvPatienten.CurrentCell = newRow.Cells(0)

            ' Falls Grid gescrollt ist, zur neuen Zeile scrollen
            If dgvPatienten.RowCount > 10 Then
                dgvPatienten.FirstDisplayedScrollingRowIndex = newRow.Index
            End If

            ' Kurzes Highlight für neue Zeile
            newRow.DefaultCellStyle.BackColor = Color.LightGreen
            Task.Delay(3000).ContinueWith(Sub()
                                              Me.Invoke(Sub()
                                                            If newRow.Index >= 0 AndAlso newRow.Index < dgvPatienten.Rows.Count Then
                                                                newRow.DefaultCellStyle.BackColor = Color.White
                                                            End If
                                                        End Sub)
                                          End Sub)

            ' Patient auch an Server senden (falls WebService läuft)
            Dim unusedTask2 = Task.Run(Async Function()
                                           Try
                                               Dim content = New FormUrlEncodedContent(New Dictionary(Of String, String) From {
                                                   {"patientenID", patient.PatientenID},
                                                   {"name", patient.Name},
                                                   {"status", patient.Status},
                                                   {"zimmer", patient.Zimmer}
                                               })

                                               Await httpClient.PostAsync($"{serviceUrl}/api/neuerpatient", content)
                                           Catch ex As Exception
                                               Logger.Debug($"Fehler beim Senden an Server: {ex.Message}")
                                           End Try
                                       End Function)

            ' Statusmeldung
            lblStatus.Text = $"Patient {patient.Name} wurde zur Warteschlange hinzugefügt"

            ' Sound abspielen
            Try
                My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Asterisk)
            Catch
                ' Fehler beim Sound ignorieren
            End Try

            UpdateStatusBar()

        Catch ex As Exception
            Logger.Debug($"Fehler beim Hinzufügen des Patienten: {ex.Message}")
        End Try
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If dgvPatienten.Rows.Count = 0 Then
            MessageBox.Show("Keine Daten zum Exportieren", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim saveDialog As New SaveFileDialog() With {
            .Filter = "CSV Dateien (*.csv)|*.csv",
            .FileName = $"Patientenprotokoll_{MonthCalendar1.SelectionStart:yyyy-MM-dd}.csv"
        }

        If saveDialog.ShowDialog() = DialogResult.OK Then
            Using writer As New StreamWriter(saveDialog.FileName, False, Encoding.UTF8)
                writer.WriteLine("Pat-Nr;Name;Ankunft;Wartezeit;Zimmer;Status;Bemerkung")

                For Each row As DataGridViewRow In dgvPatienten.Rows
                    If Not row.IsNewRow Then
                        Dim zeile = String.Join(";",
                            row.Cells("PatientenID").Value,
                            row.Cells("Name").Value,
                            row.Cells("Ankunftszeit").Value,
                            row.Cells("Wartezeit").Value,
                            row.Cells("Zimmer").Value,
                            row.Cells("Status").Value,
                            row.Cells("Bemerkung").Value)
                        writer.WriteLine(zeile)
                    End If
                Next
            End Using

            MessageBox.Show($"Export erfolgreich: {saveDialog.FileName}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub TreeView1_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterCheck
        BereicheManager.TreeView_AfterCheck(e)
    End Sub

    Private Sub dgvPatienten_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPatienten.CellValueChanged
        ' Nur reagieren wenn es die Zimmer-Spalte ist
        If e.RowIndex < 0 OrElse e.ColumnIndex <> dgvPatienten.Columns("Zimmer").Index Then Return

        Dim row = dgvPatienten.Rows(e.RowIndex)
        Dim patientenID = row.Cells("PatientenID").Value?.ToString()
        Dim neuesZimmer = row.Cells("Zimmer").Value?.ToString()

        If String.IsNullOrEmpty(patientenID) OrElse String.IsNullOrEmpty(neuesZimmer) Then Return

        ' An Server senden
        Task.Run(Async Function()
                     Try
                         Dim values As New Dictionary(Of String, String) From {
                {"patientenID", patientenID},
                {"zimmer", neuesZimmer}
            }

                         Dim content = New FormUrlEncodedContent(values)
                         Dim response = Await httpClient.PostAsync($"{serviceUrl}/api/zimmerwechsel", content)

                         If Not response.IsSuccessStatusCode Then
                             Me.Invoke(Sub()
                                           MessageBox.Show("Zimmerwechsel konnte nicht gespeichert werden", "Fehler")
                                       End Sub)
                         End If
                     Catch ex As Exception
                         Logger.Debug($"Fehler beim Zimmerwechsel: {ex.Message}")
                     End Try
                     Return True
                 End Function)
    End Sub

    Private Sub dgvPatienten_SelectionChanged(sender As Object, e As EventArgs) Handles dgvPatienten.SelectionChanged
        If dgvPatienten.SelectedRows.Count > 0 Then
            Dim stackTrace = New System.Diagnostics.StackTrace()
            Logger.Debug($"Selection geändert zu Zeile: {dgvPatienten.SelectedRows(0).Index}")
            Logger.Debug($"Aufgerufen von: {stackTrace.GetFrame(1).GetMethod().Name}")
        End If
    End Sub

    Public Sub NeuerPatientVonXML(xmlPatient As GDTParser.XMLPatient)
        Try
            If dgvPatienten.Columns.Count = 0 Then
                Logger.Debug("Grid noch nicht initialisiert - XML-Patient wird verworfen")
                Return
            End If

            ' PatientInfo aus XML-Daten erstellen
            Dim patient As New PatientInfo With {
                .PatientenID = xmlPatient.PatientenID,
                .Name = $"{xmlPatient.Nachname}, {xmlPatient.Vorname}",
                .Vorname = xmlPatient.Vorname,
                .Nachname = xmlPatient.Nachname,
                .Ankunftszeit = DateTime.Now,
                .Status = "Wartend",
                .Zimmer = "Wartezimmer",
                .Wartezeit = 0,
                .Prioritaet = 0,
                .Bemerkung = ""
            }

            ' Schein-ID separat speichern für späteren Aufruf
            ' (in Dictionary oder Datenbank)
            If Not String.IsNullOrEmpty(xmlPatient.ScheinID) Then
                ' Speichere Mapping PatientenID -> ScheinID
                StoreScheinIDMapping(xmlPatient.PatientenID, xmlPatient.ScheinID)
            End If

            ' Prüfen ob Patient schon existiert
            For Each row As DataGridViewRow In dgvPatienten.Rows
                If Not row.IsNewRow AndAlso row.Cells("PatientenID").Value?.ToString() = patient.PatientenID Then
                    Logger.Debug($"Patient {patient.PatientenID} bereits vorhanden")
                    Return
                End If
            Next

            ' Patient hinzufügen
            Dim index = dgvPatienten.Rows.Add()
            Dim newRow = dgvPatienten.Rows(index)

            newRow.Cells("PatientenID").Value = patient.PatientenID
            newRow.Cells("Name").Value = patient.Name
            newRow.Cells("Ankunftszeit").Value = patient.Ankunftszeit
            newRow.Cells("Wartezeit").Value = "0 min"
            newRow.Cells("Zimmer").Value = patient.Zimmer
            newRow.Cells("Status").Value = patient.Status
            newRow.Cells("PrioritaetWert").Value = patient.Prioritaet
            newRow.Cells("Prioritaet").Value = GridManager.GetPrioritaetText(patient.Prioritaet)
            newRow.Cells("Bemerkung").Value = patient.Bemerkung

            ' An Server senden
            ServerComm.PatientManuellHinzufuegen(
                patient.PatientenID,
                patient.Vorname,
                patient.Nachname,
                patient.Prioritaet,
                patient.Bemerkung,
                False,
                patient.Ankunftszeit,
                patient.Zimmer
            )

            ' Grid aktualisieren
            GridManager.SortierePatienten()
            GridManager.FaerbeZeilenNachStatus()

            Logger.Debug($"XML-Patient {patient.PatientenID} hinzugefügt mit ScheinID {xmlPatient.ScheinID}")

        Catch ex As Exception
            Logger.Debug($"Fehler in NeuerPatientVonXML: {ex.Message}")
        End Try
    End Sub

    ' Dictionary für ScheinID-Mapping
    Private scheinIDMapping As New Dictionary(Of String, String)

    Private Sub StoreScheinIDMapping(patientenID As String, scheinID As String)
        scheinIDMapping(patientenID) = scheinID
    End Sub

    Private Function GetScheinID(patientenID As String) As String
        If scheinIDMapping.ContainsKey(patientenID) Then
            Return scheinIDMapping(patientenID)
        End If
        Return ""
    End Function

    Private Sub btnCheckIn_Click(sender As Object, e As EventArgs) Handles btnCheckIn.Click
        If dgvPatienten.CurrentRow Is Nothing Then Return

        Dim row = dgvPatienten.CurrentRow
        Dim status = row.Cells("Status").Value?.ToString()
        Dim patientenID = row.Cells("PatientenID").Value?.ToString()

        If status = "Geplant" Then
            ' Status auf Wartend setzen
            row.Cells("Status").Value = "Wartend"
            row.Cells("Ankunftszeit").Value = DateTime.Now
            row.Cells("Wartezeit").Value = "0 min"

            ' Server-Update
            Task.Run(Async Function()
                         Await ServerComm.StatusUpdate(patientenID, "Wartend", DateTime.Now)
                         Return True
                     End Function)

            ' Grid aktualisieren
            GridManager.FaerbeZeilenNachStatus()
            GridManager.SortierePatienten()

            Logger.Debug($"Patient {patientenID} eingecheckt")
        Else
            MessageBox.Show("Nur geplante Patienten können eingecheckt werden.",
                       "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub



    'Private Sub ProcessSingleXMLFile(xmlFile As String)
    '    Try
    '        Dim xmlPatient = GDTParser.ParseMedicalOfficeXML(xmlFile)
    '        If xmlPatient IsNot Nothing Then
    '            ' Prüfen ob Grid bereit ist
    '            If dgvPatienten.Columns.Count > 0 Then
    '                NeuerPatientVonXML(xmlPatient)
    '                File.Delete(xmlFile)
    '                Logger.Debug($"XML-Datei verarbeitet und gelöscht: {xmlFile}")
    '            Else
    '                Logger.Debug("Grid noch nicht bereit - XML-Datei wird beim nächsten Durchlauf verarbeitet")
    '            End If
    '        End If
    '    Catch ex As Exception
    '        Logger.Debug($"XML-Verarbeitung fehlgeschlagen: {ex.Message}")
    '    End Try
    'End Sub
End Class