Imports System.Drawing

Public Class FormQRAnzeige
    Private serverUrl As String = "http://localhost:8080"
    Private updateTimer As Timer
    Private currentPatientID As String = ""

    Private Sub FormQRAnzeige_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Timer für automatische Aktualisierung
        updateTimer = New Timer()
        updateTimer.Interval = 5000 ' 5 Sekunden
        AddHandler updateTimer.Tick, AddressOf UpdateTimer_Tick
        updateTimer.Start()

        ' Praxis-QR-Code anzeigen
        ShowPraxisQR()
    End Sub

    ' Praxis-QR-Code anzeigen (für allgemeine Anzeige)
    Public Sub ShowPraxisQR()
        Try
            lblTitel.Text = "Patientenaufrufe - Praxis"
            lblUntertitel.Text = "Scannen Sie den QR-Code für aktuelle Aufrufe"

            Dim qrImage As Bitmap = QRCodeManager.GeneratePraxisQR(serverUrl)
            picQRCode.Image = qrImage
            picQRCode.SizeMode = PictureBoxSizeMode.Zoom

            lblUrl.Text = serverUrl
            currentPatientID = ""

            ' Zusätzliche Info-Labels ausblenden
            lblPatientInfo.Visible = False
            lblInstructions.Visible = False

        Catch ex As Exception
            lblTitel.Text = "Fehler beim Laden des QR-Codes"
            lblUntertitel.Text = ex.Message
        End Try
    End Sub

    ' Patienten-spezifischen QR-Code anzeigen
    Public Sub ShowPatientQR(patientenID As String)
        Try
            currentPatientID = patientenID
            lblTitel.Text = $"Patienten-QR-Code"
            lblUntertitel.Text = $"Patient {patientenID} - Scannen für Status-Abfrage"

            Dim qrImage As Bitmap = QRCodeManager.GeneratePatientQR(patientenID, serverUrl)
            picQRCode.Image = qrImage
            picQRCode.SizeMode = PictureBoxSizeMode.Zoom

            lblUrl.Text = $"{serverUrl}/mobile/{patientenID}"

            ' Zusätzliche Informationen anzeigen
            lblPatientInfo.Text = $"Patienten-ID: {patientenID}"
            lblPatientInfo.Visible = True
            lblInstructions.Text = "Der Patient kann diesen QR-Code scannen, um:" & vbCrLf &
                                 "• Seine ID zu sehen" & vbCrLf &
                                 "• Den aktuellen Status abzufragen" & vbCrLf &
                                 "• Benachrichtigungen zu erhalten"
            lblInstructions.Visible = True

        Catch ex As Exception
            lblTitel.Text = "Fehler beim Laden des QR-Codes"
            lblUntertitel.Text = ex.Message
        End Try
    End Sub

    ' Server-URL ändern
    Public Sub SetServerUrl(newUrl As String)
        serverUrl = newUrl
        lblServerInfo.Text = $"Server: {serverUrl}"

        ' QR-Code neu generieren
        If String.IsNullOrEmpty(currentPatientID) Then
            ShowPraxisQR()
        Else
            ShowPatientQR(currentPatientID)
        End If
    End Sub

    ' Timer-Update
    Private Sub UpdateTimer_Tick(sender As Object, e As EventArgs)
        ' QR-Code könnte hier aktualisiert werden falls sich die URL ändert
        ' Für jetzt nur Status-Update
        lblZeit.Text = $"Letzte Aktualisierung: {DateTime.Now:HH:mm:ss}"
    End Sub

    ' QR-Code speichern
    Private Sub btnSpeichern_Click(sender As Object, e As EventArgs) Handles btnSpeichern.Click
        If picQRCode.Image IsNot Nothing Then
            Dim saveDialog As New SaveFileDialog()
            saveDialog.Filter = "PNG Dateien (*.png)|*.png|JPEG Dateien (*.jpg)|*.jpg"
            saveDialog.DefaultExt = "png"
            saveDialog.FileName = If(String.IsNullOrEmpty(currentPatientID), "Praxis_QR", $"Patient_{currentPatientID}_QR")

            If saveDialog.ShowDialog() = DialogResult.OK Then
                Try
                    QRCodeManager.SaveQRCodeToFile(picQRCode.Image, saveDialog.FileName)
                    MessageBox.Show("QR-Code erfolgreich gespeichert!", "Gespeichert",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End If
    End Sub

    ' Vollbild-Modus
    Private Sub btnVollbild_Click(sender As Object, e As EventArgs) Handles btnVollbild.Click
        If Me.WindowState = FormWindowState.Maximized Then
            Me.WindowState = FormWindowState.Normal
            Me.FormBorderStyle = FormBorderStyle.Sizable
            btnVollbild.Text = "Vollbild"
        Else
            Me.FormBorderStyle = FormBorderStyle.None
            Me.WindowState = FormWindowState.Maximized
            btnVollbild.Text = "Fenster"
        End If
    End Sub

    ' URL in Zwischenablage kopieren
    Private Sub lblUrl_Click(sender As Object, e As EventArgs) Handles lblUrl.Click
        Try
            Clipboard.SetText(lblUrl.Text)
            MessageBox.Show("URL in Zwischenablage kopiert!", "Kopiert",
                          MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Fehler beim Kopieren: {ex.Message}", "Fehler",
                          MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' QR-Code drucken
    Private Sub btnDrucken_Click(sender As Object, e As EventArgs) Handles btnDrucken.Click
        If picQRCode.Image IsNot Nothing Then
            Dim printDialog As New PrintDialog()
            Dim printDocument As New Printing.PrintDocument()

            AddHandler printDocument.PrintPage, AddressOf PrintDocument_PrintPage
            printDialog.Document = printDocument

            If printDialog.ShowDialog() = DialogResult.OK Then
                printDocument.Print()
            End If
        End If
    End Sub

    ' Druck-Handler
    Private Sub PrintDocument_PrintPage(sender As Object, e As Printing.PrintPageEventArgs)
        Try
            ' QR-Code zentriert drucken
            Dim qrSize As Integer = 300
            Dim x As Integer = (e.PageBounds.Width - qrSize) \ 2
            Dim y As Integer = 100

            e.Graphics.DrawImage(picQRCode.Image, x, y, qrSize, qrSize)

            ' Titel und URL drucken
            Dim font As New Font("Arial", 14, FontStyle.Bold)
            Dim titleSize As SizeF = e.Graphics.MeasureString(lblTitel.Text, font)
            e.Graphics.DrawString(lblTitel.Text, font, Brushes.Black,
                                (e.PageBounds.Width - titleSize.Width) / 2, 50)

            Dim urlFont As New Font("Arial", 10)
            Dim urlSize As SizeF = e.Graphics.MeasureString(lblUrl.Text, urlFont)
            e.Graphics.DrawString(lblUrl.Text, urlFont, Brushes.Black,
                                (e.PageBounds.Width - urlSize.Width) / 2, y + qrSize + 20)

        Catch ex As Exception
            Logger.Debug($"Druckfehler: {ex.Message}")
        End Try
    End Sub

    ' Cleanup
    Private Sub FormQRAnzeige_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If updateTimer IsNot Nothing Then
            updateTimer.Stop()
            updateTimer.Dispose()
        End If
    End Sub
End Class