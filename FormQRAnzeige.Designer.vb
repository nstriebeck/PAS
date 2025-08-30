<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormQRAnzeige
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.picQRCode = New System.Windows.Forms.PictureBox()
        Me.lblTitel = New System.Windows.Forms.Label()
        Me.lblUntertitel = New System.Windows.Forms.Label()
        Me.lblUrl = New System.Windows.Forms.Label()
        Me.btnSpeichern = New System.Windows.Forms.Button()
        Me.btnDrucken = New System.Windows.Forms.Button()
        Me.btnVollbild = New System.Windows.Forms.Button()
        Me.lblServerInfo = New System.Windows.Forms.Label()
        Me.lblZeit = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.lblPatientInfo = New System.Windows.Forms.Label()
        Me.lblInstructions = New System.Windows.Forms.Label()
        CType(Me.picQRCode, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'picQRCode
        '
        Me.picQRCode.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.picQRCode.BackColor = System.Drawing.Color.White
        Me.picQRCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picQRCode.Location = New System.Drawing.Point(100, 101)
        Me.picQRCode.Name = "picQRCode"
        Me.picQRCode.Size = New System.Drawing.Size(300, 300)
        Me.picQRCode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.picQRCode.TabIndex = 0
        Me.picQRCode.TabStop = False
        '
        'lblTitel
        '
        Me.lblTitel.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblTitel.Font = New System.Drawing.Font("Microsoft Sans Serif", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitel.ForeColor = System.Drawing.Color.DarkBlue
        Me.lblTitel.Location = New System.Drawing.Point(0, 55)
        Me.lblTitel.Name = "lblTitel"
        Me.lblTitel.Size = New System.Drawing.Size(500, 40)
        Me.lblTitel.TabIndex = 1
        Me.lblTitel.Text = "Patientenaufrufe"
        Me.lblTitel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblUntertitel
        '
        Me.lblUntertitel.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblUntertitel.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblUntertitel.ForeColor = System.Drawing.Color.Gray
        Me.lblUntertitel.Location = New System.Drawing.Point(0, 25)
        Me.lblUntertitel.Name = "lblUntertitel"
        Me.lblUntertitel.Size = New System.Drawing.Size(500, 30)
        Me.lblUntertitel.TabIndex = 2
        Me.lblUntertitel.Text = "Scannen Sie den QR-Code"
        Me.lblUntertitel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblUrl
        '
        Me.lblUrl.Cursor = System.Windows.Forms.Cursors.Hand
        Me.lblUrl.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblUrl.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblUrl.ForeColor = System.Drawing.Color.Blue
        Me.lblUrl.Location = New System.Drawing.Point(0, 0)
        Me.lblUrl.Name = "lblUrl"
        Me.lblUrl.Size = New System.Drawing.Size(500, 25)
        Me.lblUrl.TabIndex = 3
        Me.lblUrl.Text = "http://localhost:8080"
        Me.lblUrl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnSpeichern
        '
        Me.btnSpeichern.Location = New System.Drawing.Point(10, 10)
        Me.btnSpeichern.Name = "btnSpeichern"
        Me.btnSpeichern.Size = New System.Drawing.Size(80, 30)
        Me.btnSpeichern.TabIndex = 4
        Me.btnSpeichern.Text = "Speichern"
        Me.btnSpeichern.UseVisualStyleBackColor = True
        '
        'btnDrucken
        '
        Me.btnDrucken.Location = New System.Drawing.Point(100, 10)
        Me.btnDrucken.Name = "btnDrucken"
        Me.btnDrucken.Size = New System.Drawing.Size(80, 30)
        Me.btnDrucken.TabIndex = 5
        Me.btnDrucken.Text = "Drucken"
        Me.btnDrucken.UseVisualStyleBackColor = True
        '
        'btnVollbild
        '
        Me.btnVollbild.Location = New System.Drawing.Point(190, 10)
        Me.btnVollbild.Name = "btnVollbild"
        Me.btnVollbild.Size = New System.Drawing.Size(80, 30)
        Me.btnVollbild.TabIndex = 6
        Me.btnVollbild.Text = "Vollbild"
        Me.btnVollbild.UseVisualStyleBackColor = True
        '
        'lblServerInfo
        '
        Me.lblServerInfo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblServerInfo.AutoSize = True
        Me.lblServerInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblServerInfo.ForeColor = System.Drawing.Color.Gray
        Me.lblServerInfo.Location = New System.Drawing.Point(10, 530)
        Me.lblServerInfo.Name = "lblServerInfo"
        Me.lblServerInfo.Size = New System.Drawing.Size(113, 13)
        Me.lblServerInfo.TabIndex = 7
        Me.lblServerInfo.Text = "Server: localhost:8080"
        '
        'lblZeit
        '
        Me.lblZeit.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblZeit.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblZeit.ForeColor = System.Drawing.Color.Gray
        Me.lblZeit.Location = New System.Drawing.Point(300, 530)
        Me.lblZeit.Name = "lblZeit"
        Me.lblZeit.Size = New System.Drawing.Size(190, 13)
        Me.lblZeit.TabIndex = 8
        Me.lblZeit.Text = "Letzte Aktualisierung: --:--:--"
        Me.lblZeit.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.lblTitel)
        Me.Panel1.Controls.Add(Me.lblUntertitel)
        Me.Panel1.Controls.Add(Me.lblUrl)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(500, 95)
        Me.Panel1.TabIndex = 9
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.btnSpeichern)
        Me.Panel2.Controls.Add(Me.btnDrucken)
        Me.Panel2.Controls.Add(Me.btnVollbild)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel2.Location = New System.Drawing.Point(0, 530)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(500, 50)
        Me.Panel2.TabIndex = 10
        '
        'lblPatientInfo
        '
        Me.lblPatientInfo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblPatientInfo.AutoSize = True
        Me.lblPatientInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPatientInfo.ForeColor = System.Drawing.Color.DarkBlue
        Me.lblPatientInfo.Location = New System.Drawing.Point(12, 423)
        Me.lblPatientInfo.Name = "lblPatientInfo"
        Me.lblPatientInfo.Size = New System.Drawing.Size(103, 17)
        Me.lblPatientInfo.TabIndex = 9
        Me.lblPatientInfo.Text = "Patienten-ID:"
        '
        'lblInstructions
        '
        Me.lblInstructions.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblInstructions.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblInstructions.ForeColor = System.Drawing.Color.Gray
        Me.lblInstructions.Location = New System.Drawing.Point(12, 448)
        Me.lblInstructions.Name = "lblInstructions"
        Me.lblInstructions.Size = New System.Drawing.Size(476, 60)
        Me.lblInstructions.TabIndex = 10
        Me.lblInstructions.Text = "Anweisungen für den Patienten"
        '
        'FormQRAnzeige
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(500, 580)
        Me.Controls.Add(Me.lblInstructions)
        Me.Controls.Add(Me.lblPatientInfo)
        Me.Controls.Add(Me.lblZeit)
        Me.Controls.Add(Me.lblServerInfo)
        Me.Controls.Add(Me.picQRCode)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.MinimumSize = New System.Drawing.Size(400, 600)
        Me.Name = "FormQRAnzeige"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "QR-Code Anzeige - Patientenaufrufe"
        CType(Me.picQRCode, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents picQRCode As PictureBox
    Friend WithEvents lblTitel As Label
    Friend WithEvents lblUntertitel As Label
    Friend WithEvents lblUrl As Label
    Friend WithEvents btnSpeichern As Button
    Friend WithEvents btnDrucken As Button
    Friend WithEvents btnVollbild As Button
    Friend WithEvents lblServerInfo As Label
    Friend WithEvents lblZeit As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents lblPatientInfo As Label
    Friend WithEvents lblInstructions As Label
End Class