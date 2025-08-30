<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormPatientEingabe
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lblTitel = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtPatientenID = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtVorname = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtNachname = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbPrioritaet = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtBemerkung = New System.Windows.Forms.TextBox()
        Me.chkBesucher = New System.Windows.Forms.CheckBox()
        Me.grpBesucherDetails = New System.Windows.Forms.GroupBox()
        Me.txtGrund = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.txtFirma = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.lblWarnung = New System.Windows.Forms.Label()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnAbbrechen = New System.Windows.Forms.Button()
        Me.clbZimmer = New System.Windows.Forms.CheckedListBox()
        Me.LabelZimmer = New System.Windows.Forms.Label()
        Me.grpBesucherDetails.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblTitel
        '
        Me.lblTitel.BackColor = System.Drawing.SystemColors.Control
        Me.lblTitel.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblTitel.Location = New System.Drawing.Point(12, 9)
        Me.lblTitel.Name = "lblTitel"
        Me.lblTitel.Size = New System.Drawing.Size(410, 30)
        Me.lblTitel.TabIndex = 0
        Me.lblTitel.Text = "Neuer Patient"
        Me.lblTitel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 56)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(69, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Patienten-ID:"
        '
        'txtPatientenID
        '
        Me.txtPatientenID.Location = New System.Drawing.Point(100, 53)
        Me.txtPatientenID.Name = "txtPatientenID"
        Me.txtPatientenID.Size = New System.Drawing.Size(200, 20)
        Me.txtPatientenID.TabIndex = 2
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 82)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(52, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Vorname:"
        '
        'txtVorname
        '
        Me.txtVorname.Location = New System.Drawing.Point(100, 79)
        Me.txtVorname.Name = "txtVorname"
        Me.txtVorname.Size = New System.Drawing.Size(200, 20)
        Me.txtVorname.TabIndex = 4
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 108)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(62, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Nachname:"
        '
        'txtNachname
        '
        Me.txtNachname.Location = New System.Drawing.Point(100, 105)
        Me.txtNachname.Name = "txtNachname"
        Me.txtNachname.Size = New System.Drawing.Size(200, 20)
        Me.txtNachname.TabIndex = 6
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 134)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(45, 13)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Priorität:"
        '
        'cmbPrioritaet
        '
        Me.cmbPrioritaet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPrioritaet.FormattingEnabled = True
        Me.cmbPrioritaet.Location = New System.Drawing.Point(100, 131)
        Me.cmbPrioritaet.Name = "cmbPrioritaet"
        Me.cmbPrioritaet.Size = New System.Drawing.Size(200, 21)
        Me.cmbPrioritaet.TabIndex = 8
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 161)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(64, 13)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Bemerkung:"
        '
        'txtBemerkung
        '
        Me.txtBemerkung.Location = New System.Drawing.Point(100, 158)
        Me.txtBemerkung.Multiline = True
        Me.txtBemerkung.Name = "txtBemerkung"
        Me.txtBemerkung.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtBemerkung.Size = New System.Drawing.Size(322, 60)
        Me.txtBemerkung.TabIndex = 10
        '
        'chkBesucher
        '
        Me.chkBesucher.AutoSize = True
        Me.chkBesucher.Location = New System.Drawing.Point(320, 55)
        Me.chkBesucher.Name = "chkBesucher"
        Me.chkBesucher.Size = New System.Drawing.Size(102, 17)
        Me.chkBesucher.TabIndex = 11
        Me.chkBesucher.Text = "Ist ein Besucher"
        Me.chkBesucher.UseVisualStyleBackColor = True
        '
        'grpBesucherDetails
        '
        Me.grpBesucherDetails.Controls.Add(Me.txtGrund)
        Me.grpBesucherDetails.Controls.Add(Me.Label7)
        Me.grpBesucherDetails.Controls.Add(Me.txtFirma)
        Me.grpBesucherDetails.Controls.Add(Me.Label6)
        Me.grpBesucherDetails.Location = New System.Drawing.Point(12, 326)
        Me.grpBesucherDetails.Name = "grpBesucherDetails"
        Me.grpBesucherDetails.Size = New System.Drawing.Size(410, 80)
        Me.grpBesucherDetails.TabIndex = 12
        Me.grpBesucherDetails.TabStop = False
        Me.grpBesucherDetails.Text = "Besucher-Details"
        Me.grpBesucherDetails.Visible = False
        '
        'txtGrund
        '
        Me.txtGrund.Location = New System.Drawing.Point(88, 45)
        Me.txtGrund.Name = "txtGrund"
        Me.txtGrund.Size = New System.Drawing.Size(316, 20)
        Me.txtGrund.TabIndex = 3
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(6, 48)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(78, 13)
        Me.Label7.TabIndex = 2
        Me.Label7.Text = "Besuchsgrund:"
        '
        'txtFirma
        '
        Me.txtFirma.Location = New System.Drawing.Point(88, 19)
        Me.txtFirma.Name = "txtFirma"
        Me.txtFirma.Size = New System.Drawing.Size(316, 20)
        Me.txtFirma.TabIndex = 1
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(6, 22)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(35, 13)
        Me.Label6.TabIndex = 0
        Me.Label6.Text = "Firma:"
        '
        'lblWarnung
        '
        Me.lblWarnung.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblWarnung.ForeColor = System.Drawing.Color.Red
        Me.lblWarnung.Location = New System.Drawing.Point(12, 409)
        Me.lblWarnung.Name = "lblWarnung"
        Me.lblWarnung.Size = New System.Drawing.Size(410, 23)
        Me.lblWarnung.TabIndex = 13
        Me.lblWarnung.Text = "⚠ NOTFALL - Patient wird priorisiert!"
        Me.lblWarnung.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblWarnung.Visible = False
        '
        'btnOK
        '
        Me.btnOK.Location = New System.Drawing.Point(266, 439)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(75, 30)
        Me.btnOK.TabIndex = 14
        Me.btnOK.Text = "OK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'btnAbbrechen
        '
        Me.btnAbbrechen.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAbbrechen.Location = New System.Drawing.Point(347, 439)
        Me.btnAbbrechen.Name = "btnAbbrechen"
        Me.btnAbbrechen.Size = New System.Drawing.Size(75, 30)
        Me.btnAbbrechen.TabIndex = 15
        Me.btnAbbrechen.Text = "Abbrechen"
        Me.btnAbbrechen.UseVisualStyleBackColor = True
        '
        'clbZimmer
        '
        Me.clbZimmer.FormattingEnabled = True
        Me.clbZimmer.Location = New System.Drawing.Point(100, 224)
        Me.clbZimmer.Name = "clbZimmer"
        Me.clbZimmer.Size = New System.Drawing.Size(322, 94)
        Me.clbZimmer.TabIndex = 16
        '
        'LabelZimmer
        '
        Me.LabelZimmer.AutoSize = True
        Me.LabelZimmer.Location = New System.Drawing.Point(13, 224)
        Me.LabelZimmer.Name = "LabelZimmer"
        Me.LabelZimmer.Size = New System.Drawing.Size(44, 13)
        Me.LabelZimmer.TabIndex = 17
        Me.LabelZimmer.Text = "Zimmer:"
        '
        'FormPatientEingabe
        '
        Me.AcceptButton = Me.btnOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnAbbrechen
        Me.ClientSize = New System.Drawing.Size(437, 481)
        Me.Controls.Add(Me.LabelZimmer)
        Me.Controls.Add(Me.clbZimmer)
        Me.Controls.Add(Me.btnAbbrechen)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.lblWarnung)
        Me.Controls.Add(Me.grpBesucherDetails)
        Me.Controls.Add(Me.chkBesucher)
        Me.Controls.Add(Me.txtBemerkung)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.cmbPrioritaet)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtNachname)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtVorname)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtPatientenID)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblTitel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormPatientEingabe"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Patient hinzufügen"
        Me.grpBesucherDetails.ResumeLayout(False)
        Me.grpBesucherDetails.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblTitel As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents txtPatientenID As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txtVorname As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents txtNachname As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents cmbPrioritaet As ComboBox
    Friend WithEvents Label5 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents dtpTermin As DateTimePicker
    Friend WithEvents Label9 As Label
	Friend WithEvents dtpTerminZeit As DateTimePicker
    Friend WithEvents txtBemerkung As TextBox
    Friend WithEvents chkBesucher As CheckBox
    Friend WithEvents grpBesucherDetails As GroupBox
    Friend WithEvents txtGrund As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents txtFirma As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents lblWarnung As Label
    Friend WithEvents btnOK As Button
    Friend WithEvents btnAbbrechen As Button
    Friend WithEvents clbZimmer As CheckedListBox
    Friend WithEvents LabelZimmer As Label
End Class