Public Class FormZimmerAuswahl
    Private _gewaehlterRaum As String = ""
    
    Public ReadOnly Property GewaehlterRaum As String
        Get
            Return _gewaehlterRaum
        End Get
    End Property

    Private Sub FormZimmerAuswahl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Zimmer-Liste initialisieren
        InitializeZimmerListe()

        Me.StartPosition = FormStartPosition.CenterScreen

        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        ' Erstes Element vorauswählen
        If lstZimmer.Items.Count > 0 Then
            lstZimmer.SelectedIndex = 0
        End If
        
        ' Focus auf OK-Button
        btnOK.Focus()
    End Sub

    Private Sub InitializeZimmerListe()
        ' Standard-Zimmer hinzufügen
        ' Diese können später in einer Konfigurationsdatei oder Datenbank gespeichert werden
        lstZimmer.Items.Clear()
        
        With lstZimmer.Items
            .Add("Zimmer 1 - Allgemeinmedizin")
            .Add("Zimmer 2 - Untersuchung")
            .Add("Zimmer 3 - Labor")
            .Add("Zimmer 4 - Behandlung")
            .Add("Zimmer 5 - EKG")
            .Add("Empfang - Anmeldung")
            .Add("Wartezimmer")
            .Add("Labor - Blutentnahme")
            .Add("Röntgen")
            .Add("Ultraschall")
        End With
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If lstZimmer.SelectedIndex >= 0 Then
            _gewaehlterRaum = lstZimmer.SelectedItem.ToString()
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Else
            MessageBox.Show("Bitte wählen Sie ein Zimmer aus.", "Zimmer auswählen",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub btnAbbrechen_Click(sender As Object, e As EventArgs) Handles btnAbbrechen.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub lstZimmer_DoubleClick(sender As Object, e As EventArgs) Handles lstZimmer.DoubleClick
        ' Doppelklick = OK
        btnOK_Click(sender, e)
    End Sub

    Private Sub lstZimmer_KeyDown(sender As Object, e As KeyEventArgs) Handles lstZimmer.KeyDown
        ' Enter = OK
        If e.KeyCode = Keys.Enter Then
            btnOK_Click(sender, e)
        ElseIf e.KeyCode = Keys.Escape Then
            btnAbbrechen_Click(sender, e)
        End If
    End Sub

    ' Zimmer zur Laufzeit hinzufügen
    Public Sub ZimmerHinzufuegen(zimmerName As String)
        If Not lstZimmer.Items.Contains(zimmerName) Then
            lstZimmer.Items.Add(zimmerName)
        End If
    End Sub

    ' Zimmer entfernen
    Public Sub ZimmerEntfernen(zimmerName As String)
        If lstZimmer.Items.Contains(zimmerName) Then
            lstZimmer.Items.Remove(zimmerName)
        End If
    End Sub

    ' Alle Zimmer aus Konfiguration laden (später implementieren)
    Public Sub ZimmerAusKonfigurationLaden()
        ' TODO: Zimmer aus App.config oder Datenbank laden
        ' Für jetzt verwenden wir die Standard-Liste
        InitializeZimmerListe()
    End Sub
End Class