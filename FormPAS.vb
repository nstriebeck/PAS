Imports System.Data.SqlClient
Imports System.Net.Http
Imports Newtonsoft.Json
Imports System.Text
Imports System.IO
Imports System.Configuration

Public Class FormPAS

    Private tagesMitDaten As New Dictionary(Of Date, Integer) ' Datum -> Anzahl Patienten
    Private isUpdating As Boolean = False

    Private blinkState As Boolean = False
    Private WithEvents timerNotfallBlink As New Timer()

    Private lastNotfallSound As DateTime = DateTime.MinValue
    Private notfallSoundEnabled As Boolean = True

    ' Bereiche-Liste als Klassenvariable deklarieren

    ' Struktur für Patientendaten
    Public Class PatientInfo
        Public Property PatientenID As String
        Public Property Name As String
        Public Property Ankunftszeit As DateTime
        Public Property Status As String ' Wartend, Aufgerufen, InBehandlung, Fertig
        Public Property Zimmer As String
        Public Property Wartezeit As Integer ' in Minuten
        Public Property Prioritaet As Integer ' 1=Normal, 2=Dringend, 3=Notfall
        Public Property Bemerkung As String
    End Class

    ' Klasse für Historie-Einträge
    Public Class HistorieEintrag
        Public Property PatientenID As String
        Public Property Name As String
        Public Property Ankunftszeit As DateTime
        Public Property AufrufZeit As DateTime?
        Public Property BehandlungStart As DateTime?
        Public Property BehandlungEnde As DateTime?
        Public Property Wartezeit As Integer
        Public Property Zimmer As String
        Public Property Bemerkung As String
        Public Property Mitarbeiter As String
    End Class

    Private Async Sub FormPAS_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'Alarmsound

        Dim mnuSound As New ToolStripMenuItem("🔊 Alarm-Sound")
        mnuSound.Name = "mnuSound"
        AddHandler mnuSound.Click, AddressOf ToggleNotfallSound
        ToolStrip1.Items.Add(mnuSound)

        ' Bereiche vom Server laden
        Await ConfigModule.LadeBereicheAsync()

        ' TreeView aufbauen
        BuildBereicheTreeView() ' Mit ConfigModule.BereicheListe


        InitializeGrid()

        DatabaseModule.MainForm = Me

        ' Timer für automatische Aktualisierung
        timerRefresh.Interval = 5000 ' 5 Sekunden
        timerRefresh.Start()

        ' TreeView mit Filteroptionen befüllen
        'InitializeTreeView()

        ' Erste Ladung
        IntelligentesUpdate()

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

    'Timer-Event für Blink-Effekt
    Private Sub timerNotfallBlink_Tick(sender As Object, e As EventArgs) Handles timerNotfallBlink.Tick
        blinkState = Not blinkState
        Dim hatNotfall As Boolean = False

        For Each row As DataGridViewRow In dgvPatienten.Rows
            If row.IsNewRow Then Continue For

            Dim prioritaet = CInt(If(row.Cells("PrioritaetWert").Value, 0))
            Dim status = row.Cells("Status").Value?.ToString()

            If prioritaet = 2 AndAlso (status = "Wartend" OrElse status = "Aufgerufen") Then
                hatNotfall = True
                ' Nur Hintergrund blinkt, Text bleibt konstant
                If blinkState Then
                    row.DefaultCellStyle.BackColor = Color.Red
                Else
                    row.DefaultCellStyle.BackColor = Color.Yellow
                End If
                ' Text-Formatierung bleibt konstant
                row.DefaultCellStyle.ForeColor = Color.Black
                row.DefaultCellStyle.Font = New Font(dgvPatienten.Font, FontStyle.Bold)

                ' Prioritäts-Spalte extra hervorheben
                row.Cells("Prioritaet").Style.ForeColor = Color.White
                row.Cells("Prioritaet").Style.BackColor = Color.DarkRed
            End If
        Next

        ' Sound alle 30 Sekunden wenn Notfall vorhanden
        If hatNotfall AndAlso notfallSoundEnabled Then
            If DateTime.Now.Subtract(lastNotfallSound).TotalSeconds > 30 Then
                Try
                    My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)
                    lastNotfallSound = DateTime.Now
                Catch ex As Exception
                    Logger.Debug($"Sound-Fehler: {ex.Message}")
                End Try
            End If
        End If
    End Sub

    ' Toggle-Button im Menü
    'Private Sub btnToggleNotfallSound_Click(sender As Object, e As EventArgs) Handles btnToggleNotfallSound.Click
    '    notfallSoundEnabled = Not notfallSoundEnabled

    '    ' In DB speichern
    '    Using conn As New SqlConnection(ConfigModule.SqlConnectionString)
    '        conn.Open()
    '        Dim cmd As New SqlCommand("UPDATE SystemConfig SET ConfigValue = @val WHERE ConfigKey = 'NotfallSound'", conn)
    '        cmd.Parameters.AddWithValue("@val", If(notfallSoundEnabled, "1", "0"))
    '        cmd.ExecuteNonQuery()
    '    End Using

    '    btnToggleNotfallSound.Text = If(notfallSoundEnabled, "🔊 Sound aus", "🔇 Sound ein")
    'End Sub
    '' Event-Handler ohne Handles-Klausel
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

    Private Sub BuildBereicheTreeView()
        Logger.Debug($"BuildBereicheTreeView aufgerufen, Anzahl Bereiche: {ConfigModule.BereicheListe.Count}")
        TreeView1.Nodes.Clear()
        TreeView1.CheckBoxes = True

        ' Gruppiere nach eindeutigen Typen
        Dim typenGruppen = ConfigModule.BereicheListe.
        Where(Function(b) b.Aktiv).
        GroupBy(Function(b) b.Typ).
        OrderBy(Function(g) If(g.Key = "Sonstige", 0, If(g.Key = "Diagnostik", 1, 2)))

        For Each gruppe In typenGruppen
            ' Parent-Node für jeden Typ erstellen
            Dim parentNode = TreeView1.Nodes.Add(gruppe.Key)

            ' Alle Bereiche dieses Typs als Child-Nodes
            For Each bereich In gruppe.OrderBy(Function(b) b.Reihenfolge)
                Dim childNode = parentNode.Nodes.Add(bereich.Bezeichnung)
                childNode.Checked = True
                Logger.Debug($"Typ: {gruppe.Key}, Bereich: {bereich.Bezeichnung}")
            Next

            parentNode.Checked = True
            parentNode.Expand()
        Next

        TreeView1.ExpandAll()
    End Sub

    Private Sub InitializeTreeView_OLD()
        TreeView1.Nodes.Clear()
        TreeView1.CheckBoxes = True

        ' Hauptknoten für Filteroptionen
        Dim nodeAnmeldung = TreeView1.Nodes.Add("Anmeldung")
        nodeAnmeldung.Checked = True

        Dim nodeLabor = TreeView1.Nodes.Add("Labor")
        nodeLabor.Checked = True

        Dim nodeRoentgen = TreeView1.Nodes.Add("Röntgen")
        nodeRoentgen.Checked = True

        Dim nodeZimmer = TreeView1.Nodes.Add("Behandlungszimmer")
        nodeZimmer.Nodes.Add("Zimmer 1").Checked = True
        nodeZimmer.Nodes.Add("Zimmer 2").Checked = True
        nodeZimmer.Nodes.Add("Zimmer 3").Checked = True
        nodeZimmer.ExpandAll()
    End Sub

    Private Sub InitializeGrid()
        dgvPatienten.Columns.Clear()
        dgvPatienten.AutoGenerateColumns = False
        dgvPatienten.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvPatienten.MultiSelect = False
        dgvPatienten.AllowUserToAddRows = False

        ' Spalten definieren
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
        .Name = "PatientenID",
        .HeaderText = "Pat-Nr",
        .Width = 70,
        .ReadOnly = True
    })
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
        .Name = "Name",
        .HeaderText = "Name",
        .Width = 150,
        .ReadOnly = True
    })
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
        .Name = "Ankunftszeit",
        .HeaderText = "Ankunft",
        .Width = 70,
        .ReadOnly = True,
        .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "HH:mm"}
    })
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
        .Name = "Wartezeit",
        .HeaderText = "Wartet",
        .Width = 60,
        .ReadOnly = True
    })
        ' Zimmer-Spalte als normale TextBox statt ComboBox
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
    .Name = "Zimmer",
    .HeaderText = "Zimmer",
    .Width = 150,  ' Breiter für mehrere Einträge
    .ReadOnly = True  ' Nur über Dialog bearbeitbar
})

        '    ' ComboBox für Zimmer - mit FlatStyle für bessere Farbdarstellung
        '    Dim zimmerColumn As New DataGridViewComboBoxColumn With {
        '    .Name = "Zimmer",
        '    .HeaderText = "Zimmer",
        '    .Width = 100,
        '    .DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing, ' Zeigt Dropdown nur beim Klicken
        '    .FlatStyle = FlatStyle.Flat, ' Wichtig für die Farbübernahme
        '    .DataPropertyName = "Zimmer"
        '}
        '    zimmerColumn.Items.AddRange({"", "Anmeldung", "Wartezimmer", "Zimmer 1", "Zimmer 2", "Zimmer 3", "Labor", "Röntgen"})
        '    dgvPatienten.Columns.Add(zimmerColumn)

        ' Status
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
        .Name = "Status",
        .HeaderText = "Status",
        .Width = 100,
        .ReadOnly = True
    })

        ' Priorität mit Text statt Zahl
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
        .Name = "Prioritaet",
        .HeaderText = "Priorität",
        .Width = 80,
        .ReadOnly = True
    })

        ' Versteckte Spalte für Prioritäts-Wert (für Sortierung)
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
        .Name = "PrioritaetWert",
        .HeaderText = "PrioWert",
        .Width = 0,
        .Visible = False
    })

        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
        .Name = "Bemerkung",
        .HeaderText = "Bemerkung",
        .Width = 150,
        .ReadOnly = False
    })

        ' DataError Event Handler hinzufügen um ComboBox-Fehler abzufangen
        AddHandler dgvPatienten.DataError, AddressOf dgvPatienten_DataError
    End Sub

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

    ' Hilfsfunktion: Triage-basierte Sortierung
    Private Sub SortierePatienten()
        Try
            dgvPatienten.Sort(New PatientenComparer())
        Catch ex As Exception
            ' Fallback bei Fehler
        End Try
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
            btnAufruf.Enabled = True
            btnInBehandlung.Enabled = True
            btnFertig.Enabled = True
            btnExport.Text = "Export"

            ' Live-Daten laden
            'IntelligentesUpdate()

            ' Sofort Daten laden und anzeigen (ERSETZT IntelligentesUpdate())
            Task.Run(Async Function()
                         Try
                             Dim neueDaten = Await HoleDatenVomServer()

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

                                           FaerbeZeilenNachStatus()
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
            Dim response = Await httpClient.GetAsync($"{serviceUrl}/api/warteschlange?datum={datum:yyyy-MM-dd}")

            If response.IsSuccessStatusCode Then
                Dim json = Await response.Content.ReadAsStringAsync()
                Dim termine = JsonConvert.DeserializeObject(Of List(Of PatientInfo))(json)

                ' Statt nur nach Ankunftszeit sortieren:
                ' termine = termine.OrderBy(Function(t) t.Ankunftszeit).ToList()

                ' Nach Priorität DANN Ankunftszeit sortieren:
                termine = termine.OrderByDescending(Function(t) t.Prioritaet).ThenBy(Function(t) t.Ankunftszeit).ToList()

                ' Grid befüllen
                ' In LadeTermineFuerTag - vereinfacht ohne Färbung
                For Each termin In termine
                    Dim index = dgvPatienten.Rows.Add()
                    Dim row = dgvPatienten.Rows(index)

                    row.Cells("PatientenID").Value = termin.PatientenID
                    row.Cells("Name").Value = termin.Name
                    row.Cells("Ankunftszeit").Value = termin.Ankunftszeit
                    row.Cells("Wartezeit").Value = "-"

                    ' Zimmer-Wert prüfen und ggf. anpassen
                    If Not String.IsNullOrEmpty(termin.Zimmer) Then
                        Dim zimmerColumn = CType(dgvPatienten.Columns("Zimmer"), DataGridViewComboBoxColumn)
                        If zimmerColumn.Items.Contains(termin.Zimmer) Then
                            row.Cells("Zimmer").Value = termin.Zimmer
                        Else
                            row.Cells("Zimmer").Value = ""
                        End If
                    Else
                        row.Cells("Zimmer").Value = ""
                    End If

                    row.Cells("Status").Value = "Geplant"
                    row.Cells("PrioritaetWert").Value = termin.Prioritaet
                    row.Cells("Prioritaet").Value = GetPrioritaetText(termin.Prioritaet)
                    row.Cells("Bemerkung").Value = termin.Bemerkung
                Next

                ' Einmal zentral färben
                FaerbeZeilenNachStatus()

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

    ' Intelligente Update-Funktion
    Private Async Sub IntelligentesUpdate()
        Try
            ' Neue Prüfung: Kommen wir von Planung/Historie?
            Dim vonPlanungGekommen = False
            If dgvPatienten.Rows.Count > 0 Then
                For Each row As DataGridViewRow In dgvPatienten.Rows
                    If Not row.IsNewRow AndAlso row.Cells("Status").Value?.ToString() = "Geplant" Then
                        vonPlanungGekommen = True
                        Exit For
                    End If
                Next
            End If

            If vonPlanungGekommen Then
                dgvPatienten.Rows.Clear()  ' Alles löschen wenn von Planung kommend
            End If
            If dgvPatienten.IsCurrentCellInEditMode OrElse MouseButtons <> MouseButtons.None Then
                Return
            End If

            Dim neueDaten = Await HoleDatenVomServer()
            Dim aktuellePatientenIDs As New HashSet(Of String)

            Dim existierendeZeilen As New Dictionary(Of String, DataGridViewRow)
            For Each row As DataGridViewRow In dgvPatienten.Rows
                If Not row.IsNewRow Then
                    Dim patID = row.Cells("PatientenID").Value?.ToString()
                    If Not String.IsNullOrEmpty(patID) Then
                        existierendeZeilen(patID) = row
                    End If
                End If
            Next

            For Each patient In neueDaten
                aktuellePatientenIDs.Add(patient.PatientenID)

                If existierendeZeilen.ContainsKey(patient.PatientenID) Then
                    Dim row = existierendeZeilen(patient.PatientenID)

                    If row.Cells("Status").Value?.ToString() <> patient.Status Then
                        row.Cells("Status").Value = patient.Status

                        'Select Case patient.Status
                        '    Case "Wartend"
                        '        row.Cells("Status").Style.BackColor = Color.LightYellow
                        '    Case "Aufgerufen"
                        '        row.Cells("Status").Style.BackColor = Color.LightBlue
                        '    Case "InBehandlung"
                        '        row.Cells("Status").Style.BackColor = Color.LightGreen
                        '    Case "Fertig"
                        '        row.Cells("Status").Style.BackColor = Color.LightGray
                        'End Select
                    End If


                    If Not row.Cells("Zimmer").IsInEditMode Then
                        If row.Cells("Zimmer").Value?.ToString() <> patient.Zimmer Then
                            row.Cells("Zimmer").Value = patient.Zimmer
                        End If
                    End If

                    ' Wartezeit aktualisieren
                    'Dim wartezeit = CInt(Math.Floor((DateTime.Now - patient.Ankunftszeit).TotalMinutes))
                    'row.Cells("Wartezeit").Value = $"{wartezeit} min"

                    ' Tatsächliche Wartezeit (seit Ankunft)
                    Dim tatsaechlicheWartezeit = CInt(Math.Floor((DateTime.Now - patient.Ankunftszeit).TotalMinutes))

                    ' Geschätzte Restwartezeit berechnen
                    Dim geschaetzteRestzeit = BerechneWartezeit(row)

                    ' Anzeige formatieren
                    If patient.Status = "Wartend" Then
                        row.Cells("Wartezeit").Value = $"{tatsaechlicheWartezeit} min (noch ca. {geschaetzteRestzeit} min)"
                    Else
                        row.Cells("Wartezeit").Value = $"{tatsaechlicheWartezeit} min"
                    End If

                    Logger.Debug($"Patient {patient.PatientenID}: Server-Priorität={patient.Prioritaet}, Grid-Priorität={row.Cells("PrioritaetWert").Value}")

                    ' Priorität aktualisieren
                    If row.Cells("PrioritaetWert").Value?.ToString() <> patient.Prioritaet.ToString() Then
                        Logger.Debug($"Ändere Priorität von {row.Cells("PrioritaetWert").Value} auf {patient.Prioritaet}")
                        row.Cells("PrioritaetWert").Value = patient.Prioritaet
                        row.Cells("Prioritaet").Value = GetPrioritaetText(patient.Prioritaet)

                        Select Case patient.Prioritaet
                            Case 2 ' Notfall
                                row.DefaultCellStyle.ForeColor = Color.Red
                                row.DefaultCellStyle.Font = New Font(dgvPatienten.Font, FontStyle.Bold)
        ' row.Cells("Prioritaet").Style.BackColor = Color.LightCoral  ' AUSKOMMENTIERT
                            Case 1 ' Dringend
                                row.DefaultCellStyle.ForeColor = Color.OrangeRed
                                ' row.Cells("Prioritaet").Style.BackColor = Color.LightYellow  ' AUSKOMMENTIERT

                            Case Else ' Normal
                                row.DefaultCellStyle.ForeColor = Color.Black
                                ' row.Cells("Prioritaet").Style.BackColor = Color.White  ' AUSKOMMENTIERT
                        End Select
                    End If

                Else
                    ' Neue Zeile hinzufügen
                    Dim index = dgvPatienten.Rows.Add()
                    Dim newRow = dgvPatienten.Rows(index)

                    newRow.Cells("PatientenID").Value = patient.PatientenID
                    newRow.Cells("Name").Value = patient.Name
                    newRow.Cells("Ankunftszeit").Value = patient.Ankunftszeit
                    newRow.Cells("Wartezeit").Value = $"{CInt(Math.Floor((DateTime.Now - patient.Ankunftszeit).TotalMinutes))} min"
                    newRow.Cells("Zimmer").Value = patient.Zimmer
                    newRow.Cells("Status").Value = patient.Status
                    newRow.Cells("PrioritaetWert").Value = patient.Prioritaet
                    newRow.Cells("Prioritaet").Value = GetPrioritaetText(patient.Prioritaet)
                    newRow.Cells("Bemerkung").Value = patient.Bemerkung

                    ' Status-Farbe
                    'Select Case patient.Status
                    '    Case "Wartend"
                    '        newRow.Cells("Status").Style.BackColor = Color.LightYellow
                    '    Case "Aufgerufen"
                    '        newRow.Cells("Status").Style.BackColor = Color.LightBlue
                    '    Case "InBehandlung"
                    '        newRow.Cells("Status").Style.BackColor = Color.LightGreen
                    'End Select

                    ' Prioritäts-Formatierung
                    Select Case patient.Prioritaet
                        Case 2 ' Notfall
                            newRow.DefaultCellStyle.ForeColor = Color.Red
                            newRow.DefaultCellStyle.Font = New Font(dgvPatienten.Font, FontStyle.Bold)
                            'newRow.Cells("Prioritaet").Style.BackColor = Color.LightCoral
                        Case 1 ' Dringend
                            newRow.DefaultCellStyle.ForeColor = Color.OrangeRed
                            'newRow.Cells("Prioritaet").Style.BackColor = Color.LightYellow
                        Case Else ' Normal
                            newRow.DefaultCellStyle.ForeColor = Color.Black
                            'newRow.Cells("Prioritaet").Style.BackColor = Color.White
                    End Select

                    ' Kurzes Highlight für neue Zeile (Fire-and-Forget ist hier beabsichtigt)
                    newRow.DefaultCellStyle.BackColor = Color.LightGreen
                    Dim unused1 = Task.Delay(2000).ContinueWith(Sub(t)
                                                                    Me.Invoke(Sub()
                                                                                  If newRow.Index >= 0 AndAlso newRow.Index < dgvPatienten.Rows.Count Then
                                                                                      newRow.DefaultCellStyle.BackColor = Color.White
                                                                                  End If
                                                                              End Sub)
                                                                End Sub)
                End If
            Next

            ' Entfernte Patienten löschen
            For i As Integer = dgvPatienten.Rows.Count - 1 To 0 Step -1
                If Not dgvPatienten.Rows(i).IsNewRow Then
                    Dim patID = dgvPatienten.Rows(i).Cells("PatientenID").Value?.ToString()

                    ' Prüfen ob Patient gerade erst hinzugefügt wurde (z.B. innerhalb der letzten 10 Sekunden)
                    Dim ankunftszeit = dgvPatienten.Rows(i).Cells("Ankunftszeit").Value
                    If ankunftszeit IsNot Nothing Then
                        Dim zeitDifferenz = DateTime.Now.Subtract(CDate(ankunftszeit)).TotalSeconds
                        If zeitDifferenz < 10 Then
                            Continue For ' Diesen Patienten noch nicht löschen
                        End If
                    End If

                    If Not String.IsNullOrEmpty(patID) AndAlso Not aktuellePatientenIDs.Contains(patID) Then
                        dgvPatienten.Rows.RemoveAt(i)
                    End If
                End If
            Next

            ' Nach Triage sortieren (Notfall > Dringend > Normal nach Zeit)
            SortierePatienten()
            'FaerbeZeilenNachStatus()
            UpdateStatusBar()

        Catch ex As Exception
            Logger.Debug($"Update-Fehler: {ex.Message}")
        End Try
    End Sub

    Private Async Function HoleDatenVomServer(Optional datum As Date? = Nothing) As Task(Of List(Of PatientInfo))
        Try
            ' Wenn kein Datum übergeben, verwende heute
            Dim abfrageDatum = If(datum.HasValue, datum.Value, DateTime.Today)

            Dim response = Await httpClient.GetAsync($"{serviceUrl}/api/warteschlange?datum={abfrageDatum:yyyy-MM-dd}")

            If response.IsSuccessStatusCode Then
                Dim json = Await response.Content.ReadAsStringAsync()
                lblVerbindung.Text = "Verbindung: OK"
                Return JsonConvert.DeserializeObject(Of List(Of PatientInfo))(json)
            Else
                lblVerbindung.Text = "Verbindung: Fehler"
            End If
        Catch ex As Exception
            lblVerbindung.Text = "Verbindung: Fehler"
        End Try
        Return New List(Of PatientInfo)()
    End Function

    Private Sub UpdateStatusBar()
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
        IntelligentesUpdate()
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
        FaerbeZeilenNachStatus()
    End Sub

    Private Sub btnInBehandlung_Click(sender As Object, e As EventArgs) Handles btnInBehandlung.Click
        If dgvPatienten.CurrentRow IsNot Nothing Then
            dgvPatienten.CurrentRow.Cells("Status").Value = "InBehandlung"
            dgvPatienten.CurrentRow.Cells("Status").Style.BackColor = Color.LightGreen
            FaerbeZeilenNachStatus()
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

                         Await httpClient.PostAsync($"{serviceUrl}/api/statusupdate", New FormUrlEncodedContent(statusValues))

                         ' Statistik speichern (optional - nur wenn die Route existiert)
                         'Dim statistikValues As New Dictionary(Of String, String) From {
                         '    {"bereich", zimmer},
                         '    {"prioritaet", prioritaet.ToString()},
                         '    {"dauer", behandlungsDauer.ToString()}
                         '}
                         'Await httpClient.PostAsync($"{serviceUrl}/api/statistik", New FormUrlEncodedContent(statistikValues))

                     Catch ex As Exception
                         Logger.Debug($"Fehler: {ex.Message}")
                     End Try
                     Return True
                 End Function)
        FaerbeZeilenNachStatus()
    End Sub    ' Neu-Button in Toolbar - öffnet Eingabeformular
    Private Sub btnNeu_Click(sender As Object, e As EventArgs) Handles btnNeu.Click
        Using frm As New FormPatientEingabe()
            frm.IstBesucher = False
            frm.GewuenschtesDatum = MonthCalendar1.SelectionStart
            If frm.ShowDialog(Me) = DialogResult.OK Then
                PatientManuellHinzufuegen(frm.PatientenID, frm.Vorname, frm.Nachname,
                                 frm.Prioritaet, frm.Bemerkung, frm.IstBesucher,
                                 frm.TerminZeit, frm.Zimmer)  ' <-- Zimmer hinzugefügt
            End If
        End Using
    End Sub

    Private Sub PatientManuellHinzufuegen(patientenID As String, vorname As String, nachname As String,
                                  prioritaet As Integer, bemerkung As String, istBesucher As Boolean,
                                  terminZeit As DateTime, zimmer As String)  ' <-- zimmer hinzugefügt
        Try
            Dim patientName = If(String.IsNullOrWhiteSpace(vorname), nachname, $"{nachname}, {vorname}")

            ' NEUE ZEILEN: Ankunftszeit basierend auf gewähltem Datum
            Dim ankunftszeit As DateTime
            If MonthCalendar1.SelectionStart.Date = DateTime.Today Then
                ankunftszeit = DateTime.Now
            Else
                ' Für zukünftige/vergangene Termine: gewähltes Datum + aktuelle Uhrzeit
                ankunftszeit = MonthCalendar1.SelectionStart.Date.Add(DateTime.Now.TimeOfDay)
            End If

            ' Neuen Patienten/Besucher zum Grid hinzufügen
            Dim index = dgvPatienten.Rows.Add()
            Dim newRow = dgvPatienten.Rows(index)

            newRow.Cells("PatientenID").Value = patientenID
            newRow.Cells("Name").Value = patientName
            newRow.Cells("Ankunftszeit").Value = terminZeit  ' GEÄNDERT: terminZeit statt ankunftszeit 
            newRow.Cells("Wartezeit").Value = "0 min"
            newRow.Cells("Zimmer").Value = If(String.IsNullOrEmpty(zimmer),
                                  If(istBesucher, "Anmeldung", "Wartezimmer"),
                                  zimmer)
            newRow.Cells("Status").Value = "Wartend"
            newRow.Cells("PrioritaetWert").Value = prioritaet
            newRow.Cells("Prioritaet").Value = GetPrioritaetText(prioritaet)
            newRow.Cells("Bemerkung").Value = bemerkung

            ' Status-Farbe
            newRow.Cells("Status").Style.BackColor = Color.LightYellow

            ' Prioritäts-Formatierung und Farben
            ' Status-Farbe
            newRow.Cells("Status").Style.BackColor = Color.LightYellow

            ' Prioritäts-Formatierung und Farben - NEUE WERTE
            Select Case prioritaet
                Case 2 ' Notfall (war 3)
                    newRow.DefaultCellStyle.ForeColor = Color.Red
                    newRow.DefaultCellStyle.Font = New Font(dgvPatienten.Font, FontStyle.Bold)
                    newRow.Cells("Prioritaet").Style.BackColor = Color.LightCoral
                Case 1 ' Dringend (war 2)
                    newRow.DefaultCellStyle.ForeColor = Color.OrangeRed
                    newRow.Cells("Prioritaet").Style.BackColor = Color.LightYellow
                Case Else ' Normal (0 oder andere)
                    newRow.DefaultCellStyle.ForeColor = Color.Black
                    newRow.Cells("Prioritaet").Style.BackColor = Color.White
            End Select

            ' Besucher kennzeichnen
            If istBesucher Then
                newRow.DefaultCellStyle.BackColor = Color.Lavender
            End If

            ' Nach Triage sortieren
            SortierePatienten()

            ' Zeile wieder finden und markieren
            For Each row As DataGridViewRow In dgvPatienten.Rows
                If row.Cells("PatientenID").Value?.ToString() = patientenID Then
                    row.Selected = True
                    dgvPatienten.CurrentCell = row.Cells(0)
                    dgvPatienten.FirstDisplayedScrollingRowIndex = row.Index
                    Exit For
                End If
            Next

            ' Kurzes Highlight
            If Not istBesucher Then
                newRow.DefaultCellStyle.BackColor = Color.LightGreen
            End If
            Dim unused = Task.Delay(3000).ContinueWith(Sub(t)
                                                           Me.Invoke(Sub()
                                                                         If newRow.Index >= 0 AndAlso newRow.Index < dgvPatienten.Rows.Count Then
                                                                             newRow.DefaultCellStyle.BackColor = If(istBesucher, Color.Lavender, Color.White)
                                                                         End If
                                                                     End Sub)
                                                       End Sub)
            Logger.Debug($"=== Neuer Patient ===")
            Logger.Debug($"Original Name: {patientName}")
            Logger.Debug($"Name Bytes: {String.Join(",", Encoding.UTF8.GetBytes(patientName))}")

            ' WICHTIG: Patient auch an Server senden, damit er persistiert wird
            Dim unusedTask = Task.Run(Async Function()
                                          Try
                                              Logger.Debug($"Sende neuen Patient an Server: ID={patientenID}, Priorität={prioritaet}")

                                              Dim values As New Dictionary(Of String, String) From {
                                                    {"patientenID", patientenID},
                                                    {"name", patientName},
                                                    {"vorname", vorname},
                                                    {"nachname", nachname},
                                                    {"status", "Wartend"},
                                                    {"zimmer", If(String.IsNullOrEmpty(zimmer),
                                                                 If(istBesucher, "Anmeldung", "Wartezimmer"),
                                                                 zimmer)},  ' <-- angepasst
                                                    {"prioritaet", prioritaet.ToString()},
                                                    {"bemerkung", bemerkung},
                                                    {"ankunftszeit", terminZeit.ToString("yyyy-MM-dd HH:mm:ss")},
                                                    {"istBesucher", istBesucher.ToString()}
                                                }

                                              Dim content = New FormUrlEncodedContent(values)
                                              Dim response = Await HttpClient.PostAsync($"{ServiceUrl}/api/neuerpatient", content)

                                              Logger.Debug($"URL Encoded: {response}")

                                              If Not response.IsSuccessStatusCode Then
                                                  Me.Invoke(Sub()
                                                                MessageBox.Show($"Warnung: Patient wurde nur lokal hinzugefügt. Server-Fehler: {response.StatusCode}",
                                                                          "Server-Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                                            End Sub)
                                              End If
                                          Catch ex As Exception
                                              Logger.Debug($"Fehler beim Senden an Server: {ex.Message}")
                                              Me.Invoke(Sub()
                                                            MessageBox.Show($"Warnung: Patient wurde nur lokal hinzugefügt. Server nicht erreichbar.",
                                                                      "Server-Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                                        End Sub)
                                          End Try
                                          Return True
                                      End Function)

            ' Sound
            Try
                If prioritaet = 3 Then
                    My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)
                Else
                    My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Asterisk)
                End If
            Catch
            End Try

            ' Statusmeldung
            Dim typ = If(istBesucher, "Besucher", "Patient")
            lblStatus.Text = $"{typ} {nachname} wurde zur Warteschlange hinzugefügt"

            UpdateStatusBar()

        Catch ex As Exception

            Logger.Debug($"Fehler beim Hinzufügen: {ex.Message}")
        End Try
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
                .Prioritaet = 1,
                .Bemerkung = "Über GDT hinzugefügt"
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
        ' Rekursion verhindern
        If isUpdating Then Return

        isUpdating = True

        Try
            ' Bei Parent-Node alle Children mit-checken/unchecken
            If e.Node.Nodes.Count > 0 Then
                For Each childNode As TreeNode In e.Node.Nodes
                    childNode.Checked = e.Node.Checked
                Next
            End If

            ' Bei Child-Node: Parent anpassen
            If e.Node.Parent IsNot Nothing Then
                Dim parentNode = e.Node.Parent
                Dim anyChecked = False
                For Each childNode As TreeNode In parentNode.Nodes
                    If childNode.Checked Then
                        anyChecked = True
                        Exit For
                    End If
                Next
                parentNode.Checked = anyChecked
            End If

        Finally
            isUpdating = False
        End Try

        ' Filter anwenden
        ApplyZimmerFilter()
    End Sub

    Private Sub ApplyZimmerFilter()
        ' Sammle aktive Filter
        Dim aktiveFilter As New List(Of String)

        ' Durchlaufe alle Nodes
        For Each parentNode As TreeNode In TreeView1.Nodes
            If parentNode.Text = "Behandlungszimmer" Then
                ' Nur Child-Nodes prüfen
                For Each childNode As TreeNode In parentNode.Nodes
                    If childNode.Checked Then
                        aktiveFilter.Add(childNode.Text)
                    End If
                Next
            ElseIf parentNode.Checked Then
                ' Andere Bereiche
                Select Case parentNode.Text
                    Case "Anmeldung"
                        aktiveFilter.Add("Anmeldung")
                        aktiveFilter.Add("Wartezimmer")
                    Case "Labor"
                        aktiveFilter.Add("Labor")
                    Case "Röntgen"
                        aktiveFilter.Add("Röntgen")
                    Case "EKG"  ' Falls EKG auch als Parent existiert
                        aktiveFilter.Add("EKG")
                End Select
            End If
        Next

        ' Filter anwenden
        For Each row As DataGridViewRow In dgvPatienten.Rows
            If row.IsNewRow Then Continue For

            Dim zimmerWert = row.Cells("Zimmer").Value?.ToString()

            If aktiveFilter.Count = 0 Then
                ' Keine Filter aktiv = alle anzeigen
                row.Visible = True
            Else
                ' Prüfen ob MINDESTENS EINES der gefilterten Zimmer im Zimmer-String vorkommt
                Dim zeigeZeile = False
                If Not String.IsNullOrEmpty(zimmerWert) Then
                    ' Zimmer-String in einzelne Zimmer aufteilen
                    Dim patientZimmer = zimmerWert.Split(","c).Select(Function(z) z.Trim()).ToList()

                    ' Prüfen ob mindestens ein gefiltertes Zimmer dabei ist
                    For Each gefiltertesZimmer As String In aktiveFilter
                        If patientZimmer.Contains(gefiltertesZimmer) Then
                            zeigeZeile = True
                            Exit For
                        End If
                    Next
                End If
                row.Visible = zeigeZeile
            End If
        Next

        ' Status aktualisieren
        Dim sichtbar = dgvPatienten.Rows.Cast(Of DataGridViewRow).
                Where(Function(r) Not r.IsNewRow AndAlso r.Visible).Count()
        Dim total = dgvPatienten.Rows.Count - 1

        If aktiveFilter.Count = 0 Then
            lblStatus.Text = $"{total} Patienten (kein Filter aktiv)"
        Else
            lblStatus.Text = $"{sichtbar} von {total} Patienten (Filter: {String.Join(", ", aktiveFilter)})"
        End If
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
    Private Sub SetzeZeilenFarbe(row As DataGridViewRow, status As String, prioritaet As Integer)
        ' Basis-Farben nach Status
        Dim backColor As Color
        Select Case status
            Case "Wartend"
                backColor = Color.FromArgb(255, 230, 230) ' Hellrot
            Case "Aufgerufen"
                backColor = Color.FromArgb(255, 255, 200) ' Hellgelb
            Case "InBehandlung"
                backColor = Color.FromArgb(230, 255, 230) ' Hellgrün
            Case "Fertig"
                backColor = Color.FromArgb(230, 230, 255) ' Hellblau
            Case "Geplant"
                backColor = Color.LightCyan
            Case Else
                backColor = Color.White
        End Select

        ' Priorität-Kennzeichnung IMMER anzeigen (auch bei Fertig)
        If prioritaet = 2 Then ' Notfall
            ' Dunkler Rand oder Markierung für Notfall-Historie
            If status = "Fertig" Then
                row.Cells("Prioritaet").Style.BackColor = Color.LightCoral
                row.Cells("Prioritaet").Value = "NOTFALL"
            Else
                backColor = Color.FromArgb(255, 150, 150) ' Kräftiges Rot
            End If
            row.DefaultCellStyle.ForeColor = Color.DarkRed
            row.DefaultCellStyle.Font = New Font(dgvPatienten.Font, FontStyle.Bold)
        ElseIf prioritaet = 1 Then ' Dringend
            row.DefaultCellStyle.ForeColor = Color.DarkOrange
        Else
            row.DefaultCellStyle.ForeColor = Color.Black
        End If

        row.DefaultCellStyle.BackColor = backColor
    End Sub

    Private Sub FaerbeZeilenNachStatus()
        For Each row As DataGridViewRow In dgvPatienten.Rows
            If row.IsNewRow Then Continue For

            Dim status = row.Cells("Status").Value?.ToString()
            Dim prioritaet = CInt(If(row.Cells("PrioritaetWert").Value, 0))

            SetzeZeilenFarbe(row, status, prioritaet)
        Next
        dgvPatienten.Refresh()
    End Sub


    Private Class PatientenComparer
        Implements IComparer

        Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
            Dim row1 = CType(x, DataGridViewRow)
            Dim row2 = CType(y, DataGridViewRow)

            ' Fertige Patienten nach OBEN
            Dim status1 = row1.Cells("Status").Value?.ToString()
            Dim status2 = row2.Cells("Status").Value?.ToString()

            ' Fertig = ganz oben (niedrigste Sortiernummer)
            If status1 = "Fertig" AndAlso status2 <> "Fertig" Then Return -1
            If status2 = "Fertig" AndAlso status1 <> "Fertig" Then Return 1

            ' Innerhalb der fertigen Patienten: Nach Ankunftszeit
            If status1 = "Fertig" AndAlso status2 = "Fertig" Then
                Dim zeitFertig1 = CDate(If(row1.Cells("Ankunftszeit").Value, DateTime.MaxValue))
                Dim zeitFertig2 = CDate(If(row2.Cells("Ankunftszeit").Value, DateTime.MaxValue))
                Return zeitFertig1.CompareTo(zeitFertig2)
            End If

            ' Für wartende/in Behandlung: Nach Priorität
            Dim prio1 = CInt(If(row1.Cells("PrioritaetWert").Value, 0))
            Dim prio2 = CInt(If(row2.Cells("PrioritaetWert").Value, 0))

            If prio1 <> prio2 Then
                Return prio2.CompareTo(prio1)
            End If

            ' Bei gleicher Priorität: Nach Ankunftszeit
            Dim zeitWartend1 = CDate(If(row1.Cells("Ankunftszeit").Value, DateTime.MaxValue))
            Dim zeitWartend2 = CDate(If(row2.Cells("Ankunftszeit").Value, DateTime.MaxValue))

            Return zeitWartend1.CompareTo(zeitWartend2)
        End Function
    End Class

    Private Sub dgvPatienten_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPatienten.CellContentClick

    End Sub
End Class

Public Class Bereich
    Public Property ID As Integer
    Public Property Bezeichnung As String
    Public Property Typ As String
    Public Property Aktiv As Boolean
    Public Property Reihenfolge As Integer
End Class