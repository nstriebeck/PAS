<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormKonfiguration
    Inherits System.Windows.Forms.Form

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private Sub InitializeComponent()
        Me.lblTitel = New System.Windows.Forms.Label()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.ButtonOK = New System.Windows.Forms.Button()
        Me.ButtonAbbrechen = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblTitel
        '
        Me.lblTitel.AutoSize = True
        Me.lblTitel.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitel.Location = New System.Drawing.Point(12, 9)
        Me.lblTitel.Name = "lblTitel"
        Me.lblTitel.Size = New System.Drawing.Size(179, 20)
        Me.lblTitel.TabIndex = 0
        Me.lblTitel.Text = "System-Konfiguration"
        '
        'lblInfo
        '
        Me.lblInfo.Location = New System.Drawing.Point(12, 40)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(360, 180)
        Me.lblInfo.TabIndex = 1
        Me.lblInfo.Text = "Lade Konfiguration..."
        '
        'ButtonOK
        '
        Me.ButtonOK.BackColor = System.Drawing.Color.LightGreen
        Me.ButtonOK.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold)
        Me.ButtonOK.Location = New System.Drawing.Point(216, 240)
        Me.ButtonOK.Name = "ButtonOK"
        Me.ButtonOK.Size = New System.Drawing.Size(75, 30)
        Me.ButtonOK.TabIndex = 2
        Me.ButtonOK.Text = "OK"
        Me.ButtonOK.UseVisualStyleBackColor = False
        '
        'ButtonAbbrechen
        '
        Me.ButtonAbbrechen.Location = New System.Drawing.Point(297, 240)
        Me.ButtonAbbrechen.Name = "ButtonAbbrechen"
        Me.ButtonAbbrechen.Size = New System.Drawing.Size(75, 30)
        Me.ButtonAbbrechen.TabIndex = 3
        Me.ButtonAbbrechen.Text = "Abbrechen"
        Me.ButtonAbbrechen.UseVisualStyleBackColor = True
        '
        'FormKonfiguration
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(384, 282)
        Me.Controls.Add(Me.ButtonAbbrechen)
        Me.Controls.Add(Me.ButtonOK)
        Me.Controls.Add(Me.lblInfo)
        Me.Controls.Add(Me.lblTitel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormKonfiguration"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Konfiguration - Patienten-Aufrufsystem"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblTitel As Label
    Friend WithEvents lblInfo As Label
    Friend WithEvents ButtonOK As Button
    Friend WithEvents ButtonAbbrechen As Button
End Class