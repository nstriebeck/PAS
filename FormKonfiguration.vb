Public Class FormKonfiguration
    Private mainForm As FormPatientenAufruf

    Public Sub New(parentForm As FormPatientenAufruf)
        InitializeComponent()
        mainForm = parentForm
    End Sub

    Private Sub FormKonfiguration_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblInfo.Text = "Aktuelle Konfiguration:" & vbCrLf & vbCrLf &
                      "Interne URL: http://192.168.1.100:8080" & vbCrLf &
                      "Externe URL: https://www.ihre-praxis.de/patient" & vbCrLf &
                      "Auto-Refresh: 5 Sekunden" & vbCrLf & vbCrLf &
                      "Erweiterte Einstellungen werden in einer" & vbCrLf &
                      "zukünftigen Version verfügbar sein."
    End Sub

    Private Sub ButtonOK_Click(sender As Object, e As EventArgs) Handles ButtonOK.Click
        MessageBox.Show("Konfiguration zur Kenntnis genommen!", "Information")
        Me.Close()
    End Sub

    Private Sub ButtonAbbrechen_Click(sender As Object, e As EventArgs) Handles ButtonAbbrechen.Click
        Me.Close()
    End Sub
End Class