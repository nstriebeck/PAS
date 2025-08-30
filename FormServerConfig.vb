Imports System.Data.SqlClient

Public Class FormServerConfig
    Private connectionString As String = "Server=SILINSQL\PatientenAufruf;Database=PAS_Database;User Id=sa;Password=PatientenAufruf4711;"
    
    Private Sub FormServerConfig_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.StartPosition = FormStartPosition.CenterParent
        LoadCurrentSettings()
    End Sub
    
    Private Sub LoadCurrentSettings()
        Try
            Using conn As New SqlConnection(connectionString)
                conn.Open()
                
                ' Server-Modus
                Dim sqlMode As String = "SELECT ConfigValue FROM SystemConfig WHERE ConfigKey = 'ServerMode'"
                Using cmd As New SqlCommand(sqlMode, conn)
                    Dim mode As String = cmd.ExecuteScalar()?.ToString()
                    Select Case mode
                        Case "Server"
                            rbServer.Checked = True
                        Case "Client"
                            rbClient.Checked = True
                        Case Else
                            rbAuto.Checked = True
                    End Select
                End Using
                
                ' Server-IP
                Dim sqlIP As String = "SELECT ConfigValue FROM SystemConfig WHERE ConfigKey = 'ServerIP'"
                Using cmd As New SqlCommand(sqlIP, conn)
                    txtServerIP.Text = If(cmd.ExecuteScalar()?.ToString(), "192.168.1.100")
                End Using
                
                ' Server-Port
                Dim sqlPort As String = "SELECT ConfigValue FROM SystemConfig WHERE ConfigKey = 'ServerPort'"
                Using cmd As New SqlCommand(sqlPort, conn)
                    txtServerPort.Text = If(cmd.ExecuteScalar()?.ToString(), "8080")
                End Using
                
                ' Wartezeit pro Patient
                Dim sqlWartezeit As String = "SELECT ConfigValue FROM SystemConfig WHERE ConfigKey = 'DefaultWartezeit'"
                Using cmd As New SqlCommand(sqlWartezeit, conn)
                    nudWartezeit.Value = Convert.ToInt32(If(cmd.ExecuteScalar()?.ToString(), "10"))
                End Using
                
                ' Auto-Refresh Intervall
                Dim sqlRefresh As String = "SELECT ConfigValue FROM SystemConfig WHERE ConfigKey = 'AutoRefresh'"
                Using cmd As New SqlCommand(sqlRefresh, conn)
                    Dim refreshMs As Integer = Convert.ToInt32(If(cmd.ExecuteScalar()?.ToString(), "5000"))
                    nudRefresh.Value = refreshMs / 1000 ' In Sekunden anzeigen
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Fehler beim Laden der Einstellungen: " & ex.Message)
        End Try
    End Sub
    
    Private Sub btnSpeichern_Click(sender As Object, e As EventArgs) Handles btnSpeichern.Click
        ' Validierung
        If Not ValidateIPAddress(txtServerIP.Text) Then
            MessageBox.Show("Bitte geben Sie eine gültige IP-Adresse ein.", "Ungültige IP", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtServerIP.Focus()
            Return
        End If
        
        Dim port As Integer
        If Not Integer.TryParse(txtServerPort.Text, port) OrElse port < 1 OrElse port > 65535 Then
            MessageBox.Show("Bitte geben Sie einen gültigen Port (1-65535) ein.", "Ungültiger Port", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtServerPort.Focus()
            Return
        End If
        
        Try
            Using conn As New SqlConnection(connectionString)
                conn.Open()
                
                ' Server-Modus speichern
                Dim serverMode As String = "Auto"
                If rbServer.Checked Then serverMode = "Server"
                If rbClient.Checked Then serverMode = "Client"
                
                UpdateConfig(conn, "ServerMode", serverMode)
                UpdateConfig(conn, "ServerIP", txtServerIP.Text)
                UpdateConfig(conn, "ServerPort", txtServerPort.Text)
                UpdateConfig(conn, "DefaultWartezeit", nudWartezeit.Value.ToString())
                UpdateConfig(conn, "AutoRefresh", (nudRefresh.Value * 1000).ToString()) ' In ms speichern
            End Using
            
            MessageBox.Show("Einstellungen wurden gespeichert.", "Gespeichert", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information)
            
            Me.DialogResult = DialogResult.OK
            Me.Close()
            
        Catch ex As Exception
            Logger.Debug("Fehler beim Speichern: " & ex.Message)
        End Try
    End Sub
    
    Private Sub UpdateConfig(conn As SqlConnection, key As String, value As String)
        Dim sql As String = "
            IF EXISTS (SELECT 1 FROM SystemConfig WHERE ConfigKey = @Key)
                UPDATE SystemConfig SET ConfigValue = @Value WHERE ConfigKey = @Key
            ELSE
                INSERT INTO SystemConfig (ConfigKey, ConfigValue) VALUES (@Key, @Value)"
        
        Using cmd As New SqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@Key", key)
            cmd.Parameters.AddWithValue("@Value", value)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    
    Private Function ValidateIPAddress(ip As String) As Boolean
        ' Einfache IP-Validierung
        If ip = "localhost" Then Return True
        
        Dim parts() As String = ip.Split("."c)
        If parts.Length <> 4 Then Return False
        
        For Each part In parts
            Dim num As Integer
            If Not Integer.TryParse(part, num) OrElse num < 0 OrElse num > 255 Then
                Return False
            End If
        Next
        
        Return True
    End Function
    
    Private Sub btnTesten_Click(sender As Object, e As EventArgs) Handles btnTesten.Click
        ' Verbindung zum WebService testen
        Try
            Dim testUrl As String = $"http://{txtServerIP.Text}:{txtServerPort.Text}/api/status"
            
            Using client As New Net.WebClient()
                client.Encoding = System.Text.Encoding.UTF8
                Dim response As String = client.DownloadString(testUrl)
                
                If response.Contains("online") Then
                    MessageBox.Show("Verbindung zum WebService erfolgreich!", "Test erfolgreich", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    MessageBox.Show("WebService antwortet, aber Status unklar.", "Test", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            End Using
            
        Catch ex As Exception
            MessageBox.Show($"Verbindung fehlgeschlagen:{vbCrLf}{ex.Message}", "Verbindungsfehler", 
                          MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub btnAbbrechen_Click(sender As Object, e As EventArgs) Handles btnAbbrechen.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub
    
    Private Sub rbServerMode_CheckedChanged(sender As Object, e As EventArgs) Handles rbServer.CheckedChanged, rbClient.CheckedChanged, rbAuto.CheckedChanged
        ' Bei Client-Modus IP-Eingabe aktivieren
        Dim isClient As Boolean = rbClient.Checked
        lblServerIP.Enabled = isClient
        txtServerIP.Enabled = isClient
        lblServerPort.Enabled = isClient
        txtServerPort.Enabled = isClient
        btnTesten.Enabled = isClient
        
        If rbServer.Checked Then
            lblHinweis.Text = "Dieser PC wird als WebService-Server fungieren."
        ElseIf rbClient.Checked Then
            lblHinweis.Text = "Dieser PC verbindet sich mit einem externen WebService."
        Else
            lblHinweis.Text = "Automatische Erkennung beim Programmstart."
        End If
    End Sub
End Class