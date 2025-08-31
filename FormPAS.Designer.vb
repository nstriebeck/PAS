<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormPAS
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.btnNeu = New System.Windows.Forms.ToolStripButton()
        Me.btnBearbeiten = New System.Windows.Forms.ToolStripButton()
        Me.btnLoeschen = New System.Windows.Forms.ToolStripButton()
        Me.btnDrucken = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.btnOhneTermin = New System.Windows.Forms.ToolStripButton()
        Me.btnMitTermin = New System.Windows.Forms.ToolStripButton()
        Me.btnEingetroffen = New System.Windows.Forms.ToolStripButton()
        Me.btnOffeneTermine = New System.Windows.Forms.ToolStripButton()
        Me.btnNichtErschienen = New System.Windows.Forms.ToolStripButton()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.TreeView1 = New System.Windows.Forms.TreeView()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.MonthCalendar1 = New System.Windows.Forms.MonthCalendar()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.dgvPatienten = New System.Windows.Forms.DataGridView()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.AufrufenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.InBehandlungToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FertigToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.PrioritätToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NormalToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DringendToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NotfallToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.lblTagesansicht = New System.Windows.Forms.Label()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.btnCheckIn = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnFertig = New System.Windows.Forms.Button()
        Me.btnInBehandlung = New System.Windows.Forms.Button()
        Me.btnAufruf = New System.Windows.Forms.Button()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.lblStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblVerbindung = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblZeit = New System.Windows.Forms.ToolStripStatusLabel()
        Me.timerRefresh = New System.Windows.Forms.Timer(Me.components)
        Me.ToolStrip1.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        CType(Me.dgvPatienten, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnNeu, Me.btnBearbeiten, Me.btnLoeschen, Me.btnDrucken, Me.ToolStripSeparator1, Me.btnOhneTermin, Me.btnMitTermin, Me.btnEingetroffen, Me.btnOffeneTermine, Me.btnNichtErschienen})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(1284, 25)
        Me.ToolStrip1.TabIndex = 0
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'btnNeu
        '
        Me.btnNeu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnNeu.Name = "btnNeu"
        Me.btnNeu.Size = New System.Drawing.Size(33, 22)
        Me.btnNeu.Text = "Neu"
        '
        'btnBearbeiten
        '
        Me.btnBearbeiten.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnBearbeiten.Name = "btnBearbeiten"
        Me.btnBearbeiten.Size = New System.Drawing.Size(67, 22)
        Me.btnBearbeiten.Text = "Bearbeiten"
        '
        'btnLoeschen
        '
        Me.btnLoeschen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnLoeschen.Name = "btnLoeschen"
        Me.btnLoeschen.Size = New System.Drawing.Size(55, 22)
        Me.btnLoeschen.Text = "Löschen"
        '
        'btnDrucken
        '
        Me.btnDrucken.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnDrucken.Name = "btnDrucken"
        Me.btnDrucken.Size = New System.Drawing.Size(55, 22)
        Me.btnDrucken.Text = "Drucken"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'btnOhneTermin
        '
        Me.btnOhneTermin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnOhneTermin.Name = "btnOhneTermin"
        Me.btnOhneTermin.Size = New System.Drawing.Size(79, 22)
        Me.btnOhneTermin.Text = "Ohne Termin"
        '
        'btnMitTermin
        '
        Me.btnMitTermin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnMitTermin.Name = "btnMitTermin"
        Me.btnMitTermin.Size = New System.Drawing.Size(68, 22)
        Me.btnMitTermin.Text = "Mit Termin"
        '
        'btnEingetroffen
        '
        Me.btnEingetroffen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnEingetroffen.Name = "btnEingetroffen"
        Me.btnEingetroffen.Size = New System.Drawing.Size(76, 22)
        Me.btnEingetroffen.Text = "Eingetroffen"
        '
        'btnOffeneTermine
        '
        Me.btnOffeneTermine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnOffeneTermine.Name = "btnOffeneTermine"
        Me.btnOffeneTermine.Size = New System.Drawing.Size(92, 22)
        Me.btnOffeneTermine.Text = "Offene Termine"
        '
        'btnNichtErschienen
        '
        Me.btnNichtErschienen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnNichtErschienen.Name = "btnNichtErschienen"
        Me.btnNichtErschienen.Size = New System.Drawing.Size(100, 22)
        Me.btnNichtErschienen.Text = "Nicht erschienen"
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 25)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.Panel1)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.Panel2)
        Me.SplitContainer1.Size = New System.Drawing.Size(1284, 661)
        Me.SplitContainer1.SplitterDistance = 250
        Me.SplitContainer1.TabIndex = 1
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.TreeView1)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.MonthCalendar1)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(250, 661)
        Me.Panel1.TabIndex = 0
        '
        'TreeView1
        '
        Me.TreeView1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TreeView1.Location = New System.Drawing.Point(12, 210)
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.Size = New System.Drawing.Size(226, 439)
        Me.TreeView1.TabIndex = 2
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(12, 191)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(93, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Bereiche filtern"
        '
        'MonthCalendar1
        '
        Me.MonthCalendar1.Location = New System.Drawing.Point(12, 10)
        Me.MonthCalendar1.MaxSelectionCount = 1
        Me.MonthCalendar1.Name = "MonthCalendar1"
        Me.MonthCalendar1.TabIndex = 0
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.dgvPatienten)
        Me.Panel2.Controls.Add(Me.lblTagesansicht)
        Me.Panel2.Controls.Add(Me.Panel3)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel2.Location = New System.Drawing.Point(0, 0)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(1030, 661)
        Me.Panel2.TabIndex = 0
        '
        'dgvPatienten
        '
        Me.dgvPatienten.AllowUserToAddRows = False
        Me.dgvPatienten.AllowUserToDeleteRows = False
        Me.dgvPatienten.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvPatienten.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvPatienten.ContextMenuStrip = Me.ContextMenuStrip1
        Me.dgvPatienten.Location = New System.Drawing.Point(3, 33)
        Me.dgvPatienten.Name = "dgvPatienten"
        Me.dgvPatienten.Size = New System.Drawing.Size(1024, 573)
        Me.dgvPatienten.TabIndex = 1
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AufrufenToolStripMenuItem, Me.InBehandlungToolStripMenuItem, Me.FertigToolStripMenuItem, Me.ToolStripSeparator2, Me.PrioritätToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(152, 98)
        '
        'AufrufenToolStripMenuItem
        '
        Me.AufrufenToolStripMenuItem.Name = "AufrufenToolStripMenuItem"
        Me.AufrufenToolStripMenuItem.Size = New System.Drawing.Size(151, 22)
        Me.AufrufenToolStripMenuItem.Text = "Aufrufen"
        '
        'InBehandlungToolStripMenuItem
        '
        Me.InBehandlungToolStripMenuItem.Name = "InBehandlungToolStripMenuItem"
        Me.InBehandlungToolStripMenuItem.Size = New System.Drawing.Size(151, 22)
        Me.InBehandlungToolStripMenuItem.Text = "In Behandlung"
        '
        'FertigToolStripMenuItem
        '
        Me.FertigToolStripMenuItem.Name = "FertigToolStripMenuItem"
        Me.FertigToolStripMenuItem.Size = New System.Drawing.Size(151, 22)
        Me.FertigToolStripMenuItem.Text = "Fertig"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(148, 6)
        '
        'PrioritätToolStripMenuItem
        '
        Me.PrioritätToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NormalToolStripMenuItem, Me.DringendToolStripMenuItem, Me.NotfallToolStripMenuItem})
        Me.PrioritätToolStripMenuItem.Name = "PrioritätToolStripMenuItem"
        Me.PrioritätToolStripMenuItem.Size = New System.Drawing.Size(151, 22)
        Me.PrioritätToolStripMenuItem.Text = "Priorität"
        '
        'NormalToolStripMenuItem
        '
        Me.NormalToolStripMenuItem.Name = "NormalToolStripMenuItem"
        Me.NormalToolStripMenuItem.Size = New System.Drawing.Size(123, 22)
        Me.NormalToolStripMenuItem.Text = "Normal"
        '
        'DringendToolStripMenuItem
        '
        Me.DringendToolStripMenuItem.Name = "DringendToolStripMenuItem"
        Me.DringendToolStripMenuItem.Size = New System.Drawing.Size(123, 22)
        Me.DringendToolStripMenuItem.Text = "Dringend"
        '
        'NotfallToolStripMenuItem
        '
        Me.NotfallToolStripMenuItem.Name = "NotfallToolStripMenuItem"
        Me.NotfallToolStripMenuItem.Size = New System.Drawing.Size(123, 22)
        Me.NotfallToolStripMenuItem.Text = "Notfall"
        '
        'lblTagesansicht
        '
        Me.lblTagesansicht.BackColor = System.Drawing.SystemColors.Info
        Me.lblTagesansicht.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblTagesansicht.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTagesansicht.ForeColor = System.Drawing.Color.Green
        Me.lblTagesansicht.Location = New System.Drawing.Point(0, 0)
        Me.lblTagesansicht.Name = "lblTagesansicht"
        Me.lblTagesansicht.Size = New System.Drawing.Size(1030, 30)
        Me.lblTagesansicht.TabIndex = 0
        Me.lblTagesansicht.Text = "LIVE - Aktuelle Warteschlange"
        Me.lblTagesansicht.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.btnCheckIn)
        Me.Panel3.Controls.Add(Me.btnExport)
        Me.Panel3.Controls.Add(Me.btnFertig)
        Me.Panel3.Controls.Add(Me.btnInBehandlung)
        Me.Panel3.Controls.Add(Me.btnAufruf)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel3.Location = New System.Drawing.Point(0, 611)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(1030, 50)
        Me.Panel3.TabIndex = 2
        '
        'btnCheckIn
        '
        Me.btnCheckIn.Location = New System.Drawing.Point(19, 8)
        Me.btnCheckIn.Name = "btnCheckIn"
        Me.btnCheckIn.Size = New System.Drawing.Size(90, 30)
        Me.btnCheckIn.TabIndex = 4
        Me.btnCheckIn.Text = "Check - In"
        Me.btnCheckIn.UseVisualStyleBackColor = True
        '
        'btnExport
        '
        Me.btnExport.Location = New System.Drawing.Point(478, 8)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(100, 30)
        Me.btnExport.TabIndex = 3
        Me.btnExport.Text = "Export"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'btnFertig
        '
        Me.btnFertig.Location = New System.Drawing.Point(362, 8)
        Me.btnFertig.Name = "btnFertig"
        Me.btnFertig.Size = New System.Drawing.Size(100, 30)
        Me.btnFertig.TabIndex = 2
        Me.btnFertig.Text = "Fertig"
        Me.btnFertig.UseVisualStyleBackColor = True
        '
        'btnInBehandlung
        '
        Me.btnInBehandlung.Location = New System.Drawing.Point(246, 8)
        Me.btnInBehandlung.Name = "btnInBehandlung"
        Me.btnInBehandlung.Size = New System.Drawing.Size(100, 30)
        Me.btnInBehandlung.TabIndex = 1
        Me.btnInBehandlung.Text = "In Behandlung"
        Me.btnInBehandlung.UseVisualStyleBackColor = True
        '
        'btnAufruf
        '
        Me.btnAufruf.Location = New System.Drawing.Point(130, 8)
        Me.btnAufruf.Name = "btnAufruf"
        Me.btnAufruf.Size = New System.Drawing.Size(100, 30)
        Me.btnAufruf.TabIndex = 0
        Me.btnAufruf.Text = "Aufruf"
        Me.btnAufruf.UseVisualStyleBackColor = True
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblStatus, Me.lblVerbindung, Me.lblZeit})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 686)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(1284, 22)
        Me.StatusStrip1.TabIndex = 2
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'lblStatus
        '
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(1130, 17)
        Me.lblStatus.Spring = True
        Me.lblStatus.Text = "Bereit"
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblVerbindung
        '
        Me.lblVerbindung.Name = "lblVerbindung"
        Me.lblVerbindung.Size = New System.Drawing.Size(90, 17)
        Me.lblVerbindung.Text = "Verbindung: OK"
        '
        'lblZeit
        '
        Me.lblZeit.Name = "lblZeit"
        Me.lblZeit.Size = New System.Drawing.Size(49, 17)
        Me.lblZeit.Text = "00:00:00"
        '
        'timerRefresh
        '
        '
        'FormPAS
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1284, 708)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Name = "FormPAS"
        Me.Text = "PAS - PatientenAufrufSystem"
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        CType(Me.dgvPatienten, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ToolStrip1 As ToolStrip
    Friend WithEvents btnNeu As ToolStripButton
    Friend WithEvents btnBearbeiten As ToolStripButton
    Friend WithEvents btnLoeschen As ToolStripButton
    Friend WithEvents btnDrucken As ToolStripButton
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents btnOhneTermin As ToolStripButton
    Friend WithEvents btnMitTermin As ToolStripButton
    Friend WithEvents btnEingetroffen As ToolStripButton
    Friend WithEvents btnOffeneTermine As ToolStripButton
    Friend WithEvents btnNichtErschienen As ToolStripButton
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents Panel1 As Panel
    Friend WithEvents MonthCalendar1 As MonthCalendar
    Friend WithEvents TreeView1 As TreeView
    Friend WithEvents Label2 As Label
    Friend WithEvents Panel2 As Panel
    Friend WithEvents lblTagesansicht As Label
    Friend WithEvents dgvPatienten As DataGridView
    Friend WithEvents Panel3 As Panel
    Friend WithEvents btnExport As Button
    Friend WithEvents btnFertig As Button
    Friend WithEvents btnInBehandlung As Button
    Friend WithEvents btnAufruf As Button
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents lblStatus As ToolStripStatusLabel
    Friend WithEvents lblVerbindung As ToolStripStatusLabel
    Friend WithEvents lblZeit As ToolStripStatusLabel
    Friend WithEvents timerRefresh As Timer
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents AufrufenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents InBehandlungToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents FertigToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
    Friend WithEvents PrioritätToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents NormalToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DringendToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents NotfallToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents btnCheckIn As Button
End Class