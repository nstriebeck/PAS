Imports QRCoder
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

Public Class QRCodeManager
    Private Shared qrGenerator As QRCodeGenerator = New QRCodeGenerator()

    ' QR-Code für Patient generieren
    Public Shared Function GeneratePatientQR(patientenID As String, serverUrl As String) As Bitmap
        Try
            ' URL für mobile Ansicht erstellen
            Dim mobileUrl As String = $"{serverUrl}/mobile/{patientenID}"
            
            ' QR-Code-Daten erstellen
            Dim qrCodeData As QRCodeData = qrGenerator.CreateQrCode(mobileUrl, QRCodeGenerator.ECCLevel.Q)
            
            ' QR-Code als Bitmap erstellen
            Dim qrCode As New QRCode(qrCodeData)
            Dim qrCodeImage As Bitmap = qrCode.GetGraphic(20, Color.Black, Color.White, True)
            
            Return qrCodeImage
            
        Catch ex As Exception
            Throw New Exception($"Fehler beim Generieren des QR-Codes: {ex.Message}")
        End Try
    End Function

    ' QR-Code mit Logo erstellen (optional)
    Public Shared Function GeneratePatientQRWithLogo(patientenID As String, serverUrl As String, logoPath As String) As Bitmap
        Try
            Dim mobileUrl As String = $"{serverUrl}/mobile/{patientenID}"
            Dim qrCodeData As QRCodeData = qrGenerator.CreateQrCode(mobileUrl, QRCodeGenerator.ECCLevel.Q)
            Dim qrCode As New QRCode(qrCodeData)
            
            ' Logo laden falls vorhanden
            Dim logo As Bitmap = Nothing
            If File.Exists(logoPath) Then
                logo = New Bitmap(logoPath)
            End If
            
            Dim qrCodeImage As Bitmap = qrCode.GetGraphic(20, Color.Black, Color.White, logo)
            
            Return qrCodeImage
            
        Catch ex As Exception
            Throw New Exception($"Fehler beim Generieren des QR-Codes mit Logo: {ex.Message}")
        End Try
    End Function

    ' QR-Code als Base64-String für Web-Anzeige
    Public Shared Function GeneratePatientQRBase64(patientenID As String, serverUrl As String) As String
        Try
            Dim qrImage As Bitmap = GeneratePatientQR(patientenID, serverUrl)
            
            Using ms As New MemoryStream()
                qrImage.Save(ms, ImageFormat.PNG)
                Dim imageBytes As Byte() = ms.ToArray()
                Return Convert.ToBase64String(imageBytes)
            End Using
            
        Catch ex As Exception
            Throw New Exception($"Fehler beim Generieren des Base64 QR-Codes: {ex.Message}")
        End Try
    End Function

    ' Allgemeinen QR-Code für Praxis-Anzeige generieren
    Public Shared Function GeneratePraxisQR(serverUrl As String) As Bitmap
        Try
            ' URL zur Hauptanzeige
            Dim displayUrl As String = $"{serverUrl}/"
            
            Dim qrCodeData As QRCodeData = qrGenerator.CreateQrCode(displayUrl, QRCodeGenerator.ECCLevel.Q)
            Dim qrCode As New QRCode(qrCodeData)
            Dim qrCodeImage As Bitmap = qrCode.GetGraphic(15, Color.Black, Color.White, True)
            
            Return qrCodeImage
            
        Catch ex As Exception
            Throw New Exception($"Fehler beim Generieren des Praxis-QR-Codes: {ex.Message}")
        End Try
    End Function

    ' QR-Code in Datei speichern
    Public Shared Sub SaveQRCodeToFile(qrImage As Bitmap, filePath As String)
        Try
            qrImage.Save(filePath, ImageFormat.PNG)
        Catch ex As Exception
            Throw New Exception($"Fehler beim Speichern des QR-Codes: {ex.Message}")
        End Try
    End Sub
End Class