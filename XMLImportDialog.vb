Imports System.Windows.Forms
Imports System.Drawing

Public Class XMLImportDialog
    Inherits Form

    Private WithEvents btnEintragen As Button
    Private WithEvents btnAbbrechen As Button
    Private cboZimmer As ComboBox
    Private chkCheckIn As CheckBox

    Public Property SelectedZimmer As String = "Wartezimmer"
    Public Property SofortCheckIn As Boolean = True

    Public Sub New(xmlPatient As GDTParser.XMLPatient)
        ' Form-Eigenschaften
        Me.Text = "Patient aus Medical Office gefunden"
        Me.Size = New Size(500, 400)  ' Höhe von 380 auf 400 erhöht
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Color.White
        Me.ShowInTaskbar = False

        InitializeComponents(xmlPatient)
    End Sub

    Private Sub InitializeComponents(xmlPatient As GDTParser.XMLPatient)
        ' Header Panel
        Dim headerPanel As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 60,
            .BackColor = Color.FromArgb(40, 167, 69)
        }

        Dim lblHeader As New Label With {
            .Text = "✓ Patient gefunden",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(20, 15),
            .AutoSize = True
        }
        headerPanel.Controls.Add(lblHeader)

        ' Frage-Label
        Dim lblFrage As New Label With {
            .Text = "Möchten Sie den Patienten eintragen?",
            .Location = New Point(20, 75),
            .Size = New Size(440, 25),
            .Font = New Font("Segoe UI", 11)
        }

        ' Patient-Details Panel
        Dim detailsPanel As New Panel With {
            .Location = New Point(20, 105),
            .Size = New Size(440, 90),
            .BackColor = Color.FromArgb(248, 249, 250),
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblDetails As New Label With {
            .Text = $"PatNr: {xmlPatient.PatientenID}" & vbCrLf &
                    $"Name: {xmlPatient.Nachname}, {xmlPatient.Vorname}" & vbCrLf &
                    $"Geburtsdatum: {xmlPatient.Geburtsdatum}" &
                    If(Not String.IsNullOrEmpty(xmlPatient.ScheinID),
                       vbCrLf & $"Schein-ID: {xmlPatient.ScheinID}", ""),
            .Location = New Point(10, 10),
            .Size = New Size(420, 85),
            .Font = New Font("Segoe UI", 10)
        }
        detailsPanel.Controls.Add(lblDetails)

        ' Zimmer-Auswahl
        Dim lblZimmer As New Label With {
            .Text = "Bereich/Zimmer:",
            .Location = New Point(20, 210),
            .Size = New Size(120, 25),
            .Font = New Font("Segoe UI", 10)
        }

        cboZimmer = New ComboBox With {
            .Location = New Point(145, 208),
            .Size = New Size(315, 25),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 10)
        }

        ' Bereiche laden
        Try
            For Each bereich In ConfigModule.BereicheListe
                If bereich.Aktiv Then
                    cboZimmer.Items.Add(bereich.Bezeichnung)
                End If
            Next
        Catch
            cboZimmer.Items.AddRange({"Wartezimmer", "Anmeldung", "Labor",
                                      "Zimmer 1", "Zimmer 2", "Zimmer 3",
                                      "Zimmer 4", "Zimmer 5", "EKG"})
        End Try

        If cboZimmer.Items.Contains("Anmeldung") Then
            cboZimmer.SelectedItem = "Anmeldung"
        ElseIf cboZimmer.Items.Count > 0 Then
            cboZimmer.SelectedIndex = 0
        End If

        ' Check-In Checkbox
        chkCheckIn = New CheckBox With {
            .Text = "Sofort einchecken (Wartezeit beginnt jetzt)",
            .Location = New Point(20, 245),
            .Size = New Size(440, 25),
            .Font = New Font("Segoe UI", 10),
            .Checked = True
        }

        ' Buttons
        btnEintragen = New Button With {
            .Text = "✓ Eintragen",
            .Location = New Point(150, 300),  ' Y-Position von 290 auf 300 erhöht
            .Size = New Size(130, 40),
            .BackColor = Color.FromArgb(40, 167, 69),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .Cursor = Cursors.Hand
        }
        btnEintragen.FlatAppearance.BorderSize = 0

        btnAbbrechen = New Button With {
            .Text = "Abbrechen",
            .Location = New Point(290, 300),  ' Y-Position von 290 auf 300 erhöht
            .Size = New Size(110, 40),
            .BackColor = Color.FromArgb(108, 117, 125),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 11),
            .Cursor = Cursors.Hand
        }
        btnAbbrechen.FlatAppearance.BorderSize = 0

        ' Controls hinzufügen
        Me.Controls.Add(headerPanel)
        Me.Controls.Add(lblFrage)
        Me.Controls.Add(detailsPanel)
        Me.Controls.Add(lblZimmer)
        Me.Controls.Add(cboZimmer)
        Me.Controls.Add(chkCheckIn)
        Me.Controls.Add(btnEintragen)
        Me.Controls.Add(btnAbbrechen)
    End Sub

    Private Sub btnEintragen_Click(sender As Object, e As EventArgs) Handles btnEintragen.Click
        SelectedZimmer = If(cboZimmer.SelectedItem?.ToString(), "Wartezimmer")
        SofortCheckIn = chkCheckIn.Checked
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnAbbrechen_Click(sender As Object, e As EventArgs) Handles btnAbbrechen.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Me.DialogResult = DialogResult.Cancel
            Me.Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function
End Class