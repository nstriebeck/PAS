<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormZimmerAuswahl
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
        Me.lstZimmer = New System.Windows.Forms.ListBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnAbbrechen = New System.Windows.Forms.Button()
        Me.lblTitel = New System.Windows.Forms.Label()
        Me.lblHinweis = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lstZimmer
        '
        Me.lstZimmer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstZimmer.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lstZimmer.FormattingEnabled = True
        Me.lstZimmer.ItemHeight = 16
        Me.lstZimmer.Location = New System.Drawing.Point(12, 54)
        Me.lstZimmer.Name = "lstZimmer"
        Me.lstZimmer.Size = New System.Drawing.Size(280, 196)
        Me.lstZimmer.TabIndex = 0
        '
        'btnOK
        '
        Me.btnOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOK.BackColor = System.Drawing.Color.LightGreen
        Me.btnOK.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnOK.Location = New System.Drawing.Point(136, 270)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(75, 30)
        Me.btnOK.TabIndex = 1
        Me.btnOK.Text = "OK"
        Me.btnOK.UseVisualStyleBackColor = False
        '
        'btnAbbrechen
        '
        Me.btnAbbrechen.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnAbbrechen.BackColor = System.Drawing.Color.LightCoral
        Me.btnAbbrechen.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAbbrechen.Location = New System.Drawing.Point(217, 270)
        Me.btnAbbrechen.Name = "btnAbbrechen"
        Me.btnAbbrechen.Size = New System.Drawing.Size(75, 30)
        Me.btnAbbrechen.TabIndex = 2
        Me.btnAbbrechen.Text = "Abbrechen"
        Me.btnAbbrechen.UseVisualStyleBackColor = False
        '
        'lblTitel
        '
        Me.lblTitel.AutoSize = True
        Me.lblTitel.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitel.Location = New System.Drawing.Point(12, 9)
        Me.lblTitel.Name = "lblTitel"
        Me.lblTitel.Size = New System.Drawing.Size(144, 20)
        Me.lblTitel.TabIndex = 3
        Me.lblTitel.Text = "Zimmer auswählen"
        '
        'lblHinweis
        '
        Me.lblHinweis.AutoSize = True
        Me.lblHinweis.ForeColor = System.Drawing.Color.Gray
        Me.lblHinweis.Location = New System.Drawing.Point(13, 32)
        Me.lblHinweis.Name = "lblHinweis"
        Me.lblHinweis.Size = New System.Drawing.Size(219, 13)
        Me.lblHinweis.TabIndex = 4
        Me.lblHinweis.Text = "Doppelklick oder Enter für schnelle Auswahl"
        '
        'FormZimmerAuswahl
        '
        Me.AcceptButton = Me.btnOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnAbbrechen
        Me.ClientSize = New System.Drawing.Size(304, 312)
        Me.Controls.Add(Me.lblHinweis)
        Me.Controls.Add(Me.lblTitel)
        Me.Controls.Add(Me.btnAbbrechen)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.lstZimmer)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormZimmerAuswahl"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Zimmer auswählen"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lstZimmer As ListBox
    Friend WithEvents btnOK As Button
    Friend WithEvents btnAbbrechen As Button
    Friend WithEvents lblTitel As Label
    Friend WithEvents lblHinweis As Label
End Class