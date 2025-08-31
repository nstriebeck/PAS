Imports System.Data.SqlClient
Imports System.Net.Http
Imports Newtonsoft.Json

Public Class FormPatientEingabe

    Public Property PatientenID As String
    Public Property PatientenName As String
    Public Property Vorname As String
    Public Property Nachname As String
    Public Property Prioritaet As Integer
    Public Property Bemerkung As String
    Public Property IstBesucher As Boolean
    Public Property GewuenschtesDatum As Date
    ' Property für Termin-Zeit
    Public Property TerminZeit As DateTime
    'Public Property Zimmer As String = ""
    Public Property ZimmerTemp As String = ""  ' Temporäre Property

    Private _zimmer As String = ""

    Public Property Zimmer As String
        Get
            Dim ausgewaehlteZimmer As New List(Of String)
            For Each item In clbZimmer.CheckedItems
                ausgewaehlteZimmer.Add(item.ToString())
            Next
            Return String.Join(", ", ausgewaehlteZimmer)
        End Get
        Set(value As String)
            Logger.Debug($"Zimmer-Setter aufgerufen mit: '{value}'")

            ' Alle abwählen
            For i = 0 To clbZimmer.Items.Count - 1
                clbZimmer.SetItemChecked(i, False)
            Next

            ' Übergebene Zimmer markieren
            If Not String.IsNullOrEmpty(value) Then
                Dim zimmerListe = value.Split(","c).Select(Function(z) z.Trim()).ToList()
                Logger.Debug($"Zimmer-Liste nach Split: {String.Join(" | ", zimmerListe)}")

                For i = 0 To clbZimmer.Items.Count - 1
                    Dim itemText = clbZimmer.Items(i).ToString()
                    If zimmerListe.Contains(itemText) Then
                        Logger.Debug($"Markiere: '{itemText}'")
                        clbZimmer.SetItemChecked(i, True)
                    Else
                        Logger.Debug($"Nicht gefunden: '{itemText}'")
                    End If
                Next
            End If
        End Set
    End Property

    Private Sub FormPatientEingabe_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Bereiche direkt vom Server laden
        ' Bereiche aus ConfigModule verwenden (wurden beim Start geladen)
        clbZimmer.Items.Clear()

        If ConfigModule.BereicheListe.Count > 0 Then
            For Each bereich In ConfigModule.BereicheListe.Where(Function(b) b.Aktiv).OrderBy(Function(b) b.Reihenfolge)
                clbZimmer.Items.Add(bereich.Bezeichnung)
            Next
        Else
            ' Fallback falls keine Bereiche geladen wurden
            clbZimmer.Items.AddRange({"Anmeldung", "Wartezimmer", "Zimmer 1", "Zimmer 2", "Zimmer 3", "Labor", "Röntgen"})
        End If

        clbZimmer.CheckOnClick = True

        ' Zimmer setzen wenn ZimmerTemp vorhanden (beim Bearbeiten)
        If Not String.IsNullOrEmpty(ZimmerTemp) Then
            Zimmer = ZimmerTemp
        End If

        ' ComboBox für Priorität befüllen
        cmbPrioritaet.Items.Clear()
        cmbPrioritaet.Items.AddRange({"Normal", "Dringend", "Notfall"})
        cmbPrioritaet.SelectedIndex = 0 ' Normal als Standard

        ' Farben setzen
        UpdatePrioritaetFarbe()

        ' Wenn Besucher-Modus
        If IstBesucher Then
            Me.Text = "Besucher hinzufügen"
            lblTitel.Text = "Neuer Besucher"
            chkBesucher.Checked = True
            txtPatientenID.Enabled = False
            txtPatientenID.Text = IDGenerator.GetNeueID(True)  ' z.B. "B-260825-1"
        Else
            Me.Text = "Patient hinzufügen"
            lblTitel.Text = "Neuer Patient"
            ' Normale Patienten kommen meist aus Medical Office mit ihrer ID
            If String.IsNullOrEmpty(PatientenID) Then
                txtPatientenID.Text = IDGenerator.GetNeueID(False)  ' z.B. "P-260825-1"
            Else
                txtPatientenID.Text = PatientenID
            End If
        End If


        ' Focus auf Nachname
        txtNachname.Focus()

        ' DateTimePicker erstellen
        dtpTermin = New DateTimePicker()
        dtpTermin.Location = New Point(100, 230)  ' Position anpassen
        dtpTermin.Size = New Size(200, 20)
        dtpTermin.CustomFormat = "dd.MM.yyyy HH:mm"
        dtpTermin.Format = DateTimePickerFormat.Custom
        Me.Controls.Add(dtpTermin)

        ' Label dazu
        Dim lblTermin As New Label()
        lblTermin.Text = "Termin:"
        lblTermin.Location = New Point(12, 233)
        Me.Controls.Add(lblTermin)

        ' Gewünschtes Datum aus dem Kalender verwenden
        If GewuenschtesDatum <> Nothing AndAlso GewuenschtesDatum <> DateTime.MinValue Then
            ' Gewähltes Datum + aktuelle Uhrzeit
            dtpTermin.Value = GewuenschtesDatum.Date.Add(DateTime.Now.TimeOfDay)
        Else
            ' Fallback: aktuelles Datum und Zeit
            dtpTermin.Value = DateTime.Now
        End If

        ' Wenn Bearbeitungsmodus (Werte vorhanden), dann Controls befüllen
        If Not String.IsNullOrEmpty(PatientenID) Then
            txtPatientenID.Text = PatientenID
            txtPatientenID.ReadOnly = True  ' ID nicht änderbar beim Bearbeiten
        End If

        If Not String.IsNullOrEmpty(Nachname) Then
            txtNachname.Text = Nachname
        End If

        If Not String.IsNullOrEmpty(Vorname) Then
            txtVorname.Text = Vorname
        End If

        If Not String.IsNullOrEmpty(Bemerkung) Then
            txtBemerkung.Text = Bemerkung
        End If

        ' Priorität setzen
        cmbPrioritaet.SelectedIndex = Prioritaet

        ' Termin setzen wenn vorhanden
        If TerminZeit <> DateTime.MinValue Then
            dtpTermin.Value = TerminZeit
        End If
    End Sub

    Private Sub cmbPrioritaet_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPrioritaet.SelectedIndexChanged
        UpdatePrioritaetFarbe()
    End Sub

    Private Sub UpdatePrioritaetFarbe()
        Select Case cmbPrioritaet.SelectedIndex
            Case 0 ' Normal
                cmbPrioritaet.BackColor = Color.White
                cmbPrioritaet.ForeColor = Color.Black
            Case 1 ' Dringend
                cmbPrioritaet.BackColor = Color.LightYellow
                cmbPrioritaet.ForeColor = Color.DarkOrange
            Case 2 ' Notfall
                cmbPrioritaet.BackColor = Color.LightCoral
                cmbPrioritaet.ForeColor = Color.DarkRed
                ' Warnsymbol anzeigen
                lblWarnung.Text = "⚠ NOTFALL - Patient wird priorisiert!"
                lblWarnung.Visible = True
            Case 3 ' Vertäglich
                cmbPrioritaet.BackColor = Color.LightGreen
                cmbPrioritaet.ForeColor = Color.DarkGreen
            Case 4 ' VIP
                cmbPrioritaet.BackColor = Color.LightBlue
                cmbPrioritaet.ForeColor = Color.DarkBlue
        End Select

        If cmbPrioritaet.SelectedIndex <> 2 Then
            lblWarnung.Visible = False
        End If
    End Sub

    Private Sub chkBesucher_CheckedChanged(sender As Object, e As EventArgs) Handles chkBesucher.CheckedChanged
        If chkBesucher.Checked Then
            txtPatientenID.Enabled = False
            txtPatientenID.Text = IDGenerator.GetNeueID(True)  ' z.B. "B-260825-1"
            lblTitel.Text = "Neuer Besucher"
            grpBesucherDetails.Visible = True
        Else
            txtPatientenID.Enabled = True
            If txtPatientenID.Text.StartsWith("B-") Then
                txtPatientenID.Text = IDGenerator.GetNeueID(False)  ' z.B. "P-260825-1"
            End If
            lblTitel.Text = "Neuer Patient"
            grpBesucherDetails.Visible = False
        End If
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        ' Validierung
        If String.IsNullOrWhiteSpace(txtNachname.Text) Then
            MessageBox.Show("Bitte geben Sie einen Nachnamen ein.", "Eingabe erforderlich",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtNachname.Focus()
            Return
        End If

        ' Werte übernehmen
        PatientenID = txtPatientenID.Text
        Nachname = txtNachname.Text.Trim()
        Vorname = txtVorname.Text.Trim()
        PatientenName = If(String.IsNullOrWhiteSpace(Vorname), Nachname, $"{Nachname}, {Vorname}")

        TerminZeit = dtpTermin.Value

        Logger.Debug($"DateTimePicker Wert: {dtpTermin.Value}" & " " & $"TerminZeit Property: {TerminZeit}")

        Prioritaet = cmbPrioritaet.SelectedIndex

        ' Bemerkung zusammensetzen
        Bemerkung = txtBemerkung.Text

        If chkBesucher.Checked Then
            IstBesucher = True
            If Not String.IsNullOrWhiteSpace(txtFirma.Text) Then
                Bemerkung = $"Firma: {txtFirma.Text} | {Bemerkung}"
            End If
            If Not String.IsNullOrWhiteSpace(txtGrund.Text) Then
                Bemerkung = $"Grund: {txtGrund.Text} | {Bemerkung}"
            End If
        End If

        ' Bei Notfall Warnung anzeigen
        If Prioritaet = 3 Then
            Dim result = MessageBox.Show(
                $"NOTFALL-Patient: {PatientenName}" & vbCrLf & vbCrLf &
                "Der Patient wird sofort an die erste Stelle der Warteschlange gesetzt." & vbCrLf &
                "Fortfahren?",
                "Notfall-Bestätigung",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)

            If result = DialogResult.No Then
                Return
            End If
        End If

        ' Zimmer sammeln
        Dim ausgewaehlteZimmer As New List(Of String)
        For Each item In clbZimmer.CheckedItems
            ausgewaehlteZimmer.Add(item.ToString())
        Next
        Zimmer = String.Join(", ", ausgewaehlteZimmer)  ' Property setzen

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnAbbrechen_Click(sender As Object, e As EventArgs) Handles btnAbbrechen.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    ' Enter-Taste für schnelle Navigation
    Private Sub txtVorname_KeyDown(sender As Object, e As KeyEventArgs) Handles txtVorname.KeyDown
        If e.KeyCode = Keys.Enter Then
            txtNachname.Focus()
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub txtNachname_KeyDown(sender As Object, e As KeyEventArgs) Handles txtNachname.KeyDown
        If e.KeyCode = Keys.Enter Then
            cmbPrioritaet.Focus()
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub cmbPrioritaet_KeyDown(sender As Object, e As KeyEventArgs) Handles cmbPrioritaet.KeyDown
        If e.KeyCode = Keys.Enter Then
            txtBemerkung.Focus()
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub txtBemerkung_KeyDown(sender As Object, e As KeyEventArgs) Handles txtBemerkung.KeyDown
        If e.KeyCode = Keys.Enter AndAlso Not e.Shift Then
            btnOK_Click(sender, e)
            e.SuppressKeyPress = True
        End If
    End Sub


End Class

' In FormPatientEingabe.vb oder als separates Modul
Public Module IDGenerator
    Public Function GetNeueID(istBesucher As Boolean, Optional zielDatum As Date = Nothing) As String
        If zielDatum = Nothing Then
            zielDatum = Date.Today
        End If

        Dim dateKey = zielDatum.ToString("ddMMyy")
        Dim prefix = If(istBesucher, "B", "P")

        ' Höchste Nummer aus Datenbank holen
        Dim laufendeNummer As Integer = 1

        Try
            Using conn As New SqlConnection(ConfigModule.SqlConnectionString)
                conn.Open()

                ' Pattern für LIKE: P-290825-%
                Dim pattern = $"{prefix}-{dateKey}-%"

                ' Höchste Nummer für dieses Muster finden
                Dim query = "SELECT PatNr FROM dbo.Warteschlange WHERE PatNr LIKE @pattern"

                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@pattern", pattern)
                    Dim reader = cmd.ExecuteReader()

                    Dim maxNummer = 0
                    While reader.Read()
                        Dim patNr = reader("PatNr").ToString()
                        ' Nummer extrahieren (nach dem letzten -)
                        Dim parts = patNr.Split("-"c)
                        If parts.Length >= 3 Then
                            Dim nummer As Integer
                            If Integer.TryParse(parts(2), nummer) Then
                                If nummer > maxNummer Then
                                    maxNummer = nummer
                                End If
                            End If
                        End If
                    End While

                    laufendeNummer = maxNummer + 1
                End Using
            End Using
        Catch ex As Exception
            Logger.Debug($"Fehler beim Ermitteln der ID: {ex.Message}")
        End Try

        Return $"{prefix}-{dateKey}-{laufendeNummer}"
    End Function
End Module