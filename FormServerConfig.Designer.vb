<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormServerConfig
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.GroupBoxServerMode = New System.Windows.Forms.GroupBox()
        Me.rbAuto = New System.Windows.Forms.RadioButton()
        Me.rbClient = New System.Windows.Forms.RadioButton()
        Me.rbServer = New System.Windows.Forms.RadioButton()
        Me.lblHinweis = New System.Windows.Forms.Label()
        Me.GroupBoxServerSettings = New System.Windows.Forms.GroupBox()
        Me.lblServerIP = New System.Windows.Forms.Label()
        Me.txtServerIP = New System.Windows.Forms.TextBox()
        Me.lblServerPort = New System.Windows.Forms.Label()
        Me.txtServerPort = New System.Windows.Forms.TextBox()
        Me.btnTesten = New System.Windows.Forms.Button()
        Me.GroupBoxSettings = New System.Windows.Forms.GroupBox()
        Me.lblWartezeit = New System.Windows.Forms.Label()
        Me.nudWartezeit = New System.Windows.Forms.NumericUpDown()
        Me.lblMinuten = New System.Windows.Forms.Label()
        Me.lblRefresh = New System.Windows.Forms.Label()
        Me.nudRefresh = New System.Windows.Forms.NumericUpDown()
        Me.lblSekunden = New System.Windows.Forms.Label()
        Me.btnSpeichern = New System.Windows.Forms.Button()
        Me.btnAbbrechen = New System.Windows.Forms.Button()
        Me.GroupBoxServerMode.SuspendLayout()
        Me.GroupBoxServerSettings.SuspendLayout()
        Me.GroupBoxSettings.SuspendLayout()
        CType(Me.nudWartezeit, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudRefresh, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupBoxServerMode
        '
        Me.GroupBoxServerMode.Controls.Add(Me.rbAuto)
        Me.GroupBoxServerMode.Controls.Add(Me.rbClient)
        Me.GroupBoxServerMode.Controls.Add(Me.rbServer)
        Me.GroupBoxServerMode.Controls.Add(Me.lblHinweis)
        Me.GroupBoxServerMode.Location = New System.Drawing.Point(12, 12)
        Me.GroupBoxServerMode.Name = "GroupBoxServerMode"
        Me.GroupBoxServerMode.Size = New System.Drawing.Size(400, 120)
        Me.GroupBoxServerMode.TabIndex = 0
        Me.GroupBoxServerMode.TabStop = False
        Me.GroupBoxServerMode.Text = "Server-Modus"
        '
        'rbAuto
        '
        Me.rbAuto.AutoSize = True
        Me.rbAuto.Checked = True
        Me.rbAuto.Location = New System.Drawing.Point(20, 25)
        Me.rbAuto.Name = "rbAuto"
        Me.rbAuto.Size = New System.Drawing.Size(83, 17)
        Me.rbAuto.TabIndex = 0
        Me.rbAuto.TabStop = True
        Me.rbAuto.Text = "Automatisch"
        Me.rbAuto.UseVisualStyleBackColor = True
        '
        'rbClient
        '
        Me.rbClient.AutoSize = True
        Me.rbClient.Location = New System.Drawing.Point(200, 25)
        Me.rbClient.Name = "rbClient"
        Me.rbClient.Size = New System.Drawing.Size(51, 17)
        Me.rbClient.TabIndex = 2
        Me.rbClient.Text = "Client"
        Me.rbClient.UseVisualStyleBackColor = True
        '
        'rbServer
        '
        Me.rbServer.AutoSize = True
        Me.rbServer.Location = New System.Drawing.Point(120, 25)
        Me.rbServer.Name = "rbServer"
        Me.rbServer.Size = New System.Drawing.Size(56, 17)
        Me.rbServer.TabIndex = 1
        Me.rbServer.Text = "Server"
        Me.rbServer.UseVisualStyleBackColor = True
        '
        'lblHinweis
        '
        Me.lblHinweis.Location = New System.Drawing.Point(17, 55)
        Me.lblHinweis.Name = "lblHinweis"
        Me.lblHinweis.Size = New System.Drawing.Size(360, 50)
        Me.lblHinweis.TabIndex = 3
        Me.lblHinweis.Text = "Automatische Erkennung beim Programmstart."
        '
        'GroupBoxServerSettings
        '
        Me.GroupBoxServerSettings.Controls.Add(Me.lblServerIP)
        Me.GroupBoxServerSettings.Controls.Add(Me.txtServerIP)
        Me.GroupBoxServerSettings.Controls.Add(Me.lblServerPort)
        Me.GroupBoxServerSettings.Controls.Add(Me.txtServerPort)
        Me.GroupBoxServerSettings.Controls.Add(Me.btnTesten)
        Me.GroupBoxServerSettings.Location = New System.Drawing.Point(12, 138)
        Me.GroupBoxServerSettings.Name = "GroupBoxServerSettings"
        Me.GroupBoxServerSettings.Size = New System.Drawing.Size(400, 100)
        Me.GroupBoxServerSettings.TabIndex = 1
        Me.GroupBoxServerSettings.TabStop = False
        Me.GroupBoxServerSettings.Text = "WebService-Server"
        '
        'lblServerIP
        '
        Me.lblServerIP.AutoSize = True
        Me.lblServerIP.Enabled = False
        Me.lblServerIP.Location = New System.Drawing.Point(17, 28)
        Me.lblServerIP.Name = "lblServerIP"
        Me.lblServerIP.Size = New System.Drawing.Size(54, 13)
        Me.lblServerIP.TabIndex = 0
        Me.lblServerIP.Text = "Server-IP:"
        '
        'txtServerIP
        '
        Me.txtServerIP.Enabled = False
        Me.txtServerIP.Location = New System.Drawing.Point(77, 25)
        Me.txtServerIP.Name = "txtServerIP"
        Me.txtServerIP.Size = New System.Drawing.Size(150, 20)
        Me.txtServerIP.TabIndex = 1
        Me.txtServerIP.Text = "192.168.1.100"
        '
        'lblServerPort
        '
        Me.lblServerPort.AutoSize = True
        Me.lblServerPort.Enabled = False
        Me.lblServerPort.Location = New System.Drawing.Point(245, 28)
        Me.lblServerPort.Name = "lblServerPort"
        Me.lblServerPort.Size = New System.Drawing.Size(29, 13)
        Me.lblServerPort.TabIndex = 2
        Me.lblServerPort.Text = "Port:"
        '
        'txtServerPort
        '
        Me.txtServerPort.Enabled = False
        Me.txtServerPort.Location = New System.Drawing.Point(280, 25)
        Me.txtServerPort.Name = "txtServerPort"
        Me.txtServerPort.Size = New System.Drawing.Size(60, 20)
        Me.txtServerPort.TabIndex = 3
        Me.txtServerPort.Text = "8080"
        '
        'btnTesten
        '
        Me.btnTesten.Enabled = False
        Me.btnTesten.Location = New System.Drawing.Point(150, 60)
        Me.btnTesten.Name = "btnTesten"
        Me.btnTesten.Size = New System.Drawing.Size(100, 25)
        Me.btnTesten.TabIndex = 4
        Me.btnTesten.Text = "Verbindung testen"
        Me.btnTesten.UseVisualStyleBackColor = True
        '
        'GroupBoxSettings
        '
        Me.GroupBoxSettings.Controls.Add(Me.lblWartezeit)
        Me.GroupBoxSettings.Controls.Add(Me.nudWartezeit)
        Me.GroupBoxSettings.Controls.Add(Me.lblMinuten)
        Me.GroupBoxSettings.Controls.Add(Me.lblRefresh)
        Me.GroupBoxSettings.Controls.Add(Me.nudRefresh)
        Me.GroupBoxSettings.Controls.Add(Me.lblSekunden)
        Me.GroupBoxSettings.Location = New System.Drawing.Point(12, 244)
        Me.GroupBoxSettings.Name = "GroupBoxSettings"
        Me.GroupBoxSettings.Size = New System.Drawing.Size(400, 90)
        Me.GroupBoxSettings.TabIndex = 2
        Me.GroupBoxSettings.TabStop = False
        Me.GroupBoxSettings.Text = "Allgemeine Einstellungen"
        '
        'lblWartezeit
        '
        Me.lblWartezeit.AutoSize = True
        Me.lblWartezeit.Location = New System.Drawing.Point(17, 28)
        Me.lblWartezeit.Name = "lblWartezeit"
        Me.lblWartezeit.Size = New System.Drawing.Size(150, 13)
        Me.lblWartezeit.TabIndex = 0
        Me.lblWartezeit.Text = "Geschätzte Wartezeit/Patient:"
        '
        'nudWartezeit
        '
        Me.nudWartezeit.Location = New System.Drawing.Point(167, 26)
        Me.nudWartezeit.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.nudWartezeit.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudWartezeit.Name = "nudWartezeit"
        Me.nudWartezeit.Size = New System.Drawing.Size(60, 20)
        Me.nudWartezeit.TabIndex = 1
        Me.nudWartezeit.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'lblMinuten
        '
        Me.lblMinuten.AutoSize = True
        Me.lblMinuten.Location = New System.Drawing.Point(233, 28)
        Me.lblMinuten.Name = "lblMinuten"
        Me.lblMinuten.Size = New System.Drawing.Size(45, 13)
        Me.lblMinuten.TabIndex = 2
        Me.lblMinuten.Text = "Minuten"
        '
        'lblRefresh
        '
        Me.lblRefresh.AutoSize = True
        Me.lblRefresh.Location = New System.Drawing.Point(17, 54)
        Me.lblRefresh.Name = "lblRefresh"
        Me.lblRefresh.Size = New System.Drawing.Size(117, 13)
        Me.lblRefresh.TabIndex = 3
        Me.lblRefresh.Text = "Aktualisierungsintervall:"
        '
        'nudRefresh
        '
        Me.nudRefresh.Location = New System.Drawing.Point(167, 52)
        Me.nudRefresh.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.nudRefresh.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudRefresh.Name = "nudRefresh"
        Me.nudRefresh.Size = New System.Drawing.Size(60, 20)
        Me.nudRefresh.TabIndex = 4
        Me.nudRefresh.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'lblSekunden
        '
        Me.lblSekunden.AutoSize = True
        Me.lblSekunden.Location = New System.Drawing.Point(233, 54)
        Me.lblSekunden.Name = "lblSekunden"
        Me.lblSekunden.Size = New System.Drawing.Size(56, 13)
        Me.lblSekunden.TabIndex = 5
        Me.lblSekunden.Text = "Sekunden"
        '
        'btnSpeichern
        '
        Me.btnSpeichern.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(123, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.btnSpeichern.ForeColor = System.Drawing.Color.White
        Me.btnSpeichern.Location = New System.Drawing.Point(120, 350)
        Me.btnSpeichern.Name = "btnSpeichern"
        Me.btnSpeichern.Size = New System.Drawing.Size(90, 30)
        Me.btnSpeichern.TabIndex = 3
        Me.btnSpeichern.Text = "Speichern"
        Me.btnSpeichern.UseVisualStyleBackColor = False
        '
        'btnAbbrechen
        '
        Me.btnAbbrechen.Location = New System.Drawing.Point(220, 350)
        Me.btnAbbrechen.Name = "btnAbbrechen"
        Me.btnAbbrechen.Size = New System.Drawing.Size(90, 30)
        Me.btnAbbrechen.TabIndex = 4
        Me.btnAbbrechen.Text = "Abbrechen"
        Me.btnAbbrechen.UseVisualStyleBackColor = True
        '
        'FormServerConfig
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(424, 401)
        Me.Controls.Add(Me.btnAbbrechen)
        Me.Controls.Add(Me.btnSpeichern)
        Me.Controls.Add(Me.GroupBoxSettings)
        Me.Controls.Add(Me.GroupBoxServerSettings)
        Me.Controls.Add(Me.GroupBoxServerMode)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormServerConfig"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Server-Konfiguration - PAS"
        Me.GroupBoxServerMode.ResumeLayout(False)
        Me.GroupBoxServerMode.PerformLayout()
        Me.GroupBoxServerSettings.ResumeLayout(False)
        Me.GroupBoxServerSettings.PerformLayout()
        Me.GroupBoxSettings.ResumeLayout(False)
        Me.GroupBoxSettings.PerformLayout()
        CType(Me.nudWartezeit, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudRefresh, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents GroupBoxServerMode As GroupBox
    Friend WithEvents rbAuto As RadioButton
    Friend WithEvents rbClient As RadioButton
    Friend WithEvents rbServer As RadioButton
    Friend WithEvents lblHinweis As Label
    Friend WithEvents GroupBoxServerSettings As GroupBox
    Friend WithEvents lblServerIP As Label
    Friend WithEvents txtServerIP As TextBox
    Friend WithEvents lblServerPort As Label
    Friend WithEvents txtServerPort As TextBox
    Friend WithEvents btnTesten As Button
    Friend WithEvents GroupBoxSettings As GroupBox
    Friend WithEvents lblWartezeit As Label
    Friend WithEvents nudWartezeit As NumericUpDown
    Friend WithEvents lblMinuten As Label
    Friend WithEvents lblRefresh As Label
    Friend WithEvents nudRefresh As NumericUpDown
    Friend WithEvents lblSekunden As Label
    Friend WithEvents btnSpeichern As Button
    Friend WithEvents btnAbbrechen As Button
End Class