Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data.SqlClient
Imports System.Linq

Public Class BereicheManager
    Private ReadOnly treeView As TreeView
    Private ReadOnly parent As FormPAS
    Private isUpdating As Boolean = False

    Public Sub New(tree As TreeView, parentForm As FormPAS)
        treeView = tree
        parent = parentForm
    End Sub

    Public Sub InitializeTreeView()
        treeView.Nodes.Clear()
        treeView.CheckBoxes = True
    End Sub

    Public Sub BuildBereicheTreeView()
        Logger.Debug($"BuildBereicheTreeView aufgerufen, Anzahl Bereiche: {ConfigModule.BereicheListe.Count}")

        treeView.Nodes.Clear()
        treeView.CheckBoxes = True

        ' Gruppiere nach eindeutigen Typen
        Dim typenGruppen = ConfigModule.BereicheListe.AsEnumerable().
    Where(Function(b) b.Aktiv).
    GroupBy(Function(b) b.Typ).
    OrderBy(Function(g) If(g.Key = "Sonstige", 0, If(g.Key = "Diagnostik", 1, 2)))


        For Each gruppe In typenGruppen
            ' Parent-Node für jeden Typ erstellen
            Dim parentNode = treeView.Nodes.Add(gruppe.Key)

            ' Alle Bereiche dieses Typs als Child-Nodes
            For Each bereich In gruppe.OrderBy(Function(b) b.Reihenfolge)
                Dim childNode = parentNode.Nodes.Add(bereich.Bezeichnung)
                childNode.Checked = True
                Logger.Debug($"Typ: {gruppe.Key}, Bereich: {bereich.Bezeichnung}")
            Next

            parentNode.Checked = True
            parentNode.Expand()
        Next

        treeView.ExpandAll()
    End Sub

    Public Sub TreeView_AfterCheck(e As TreeViewEventArgs)
        If isUpdating Then Return

        isUpdating = True
        Try
            ' Bei Parent-Node alle Children mit-checken/unchecken
            If e.Node.Nodes.Count > 0 Then
                For Each childNode As TreeNode In e.Node.Nodes
                    childNode.Checked = e.Node.Checked
                Next
            End If

            ' Bei Child-Node: Parent anpassen
            If e.Node.Parent IsNot Nothing Then
                Dim parentNode = e.Node.Parent
                Dim anyChecked = False
                For Each childNode As TreeNode In parentNode.Nodes
                    If childNode.Checked Then
                        anyChecked = True
                        Exit For
                    End If
                Next
                parentNode.Checked = anyChecked
            End If
        Finally
            isUpdating = False
        End Try

        ' Filter anwenden
        ApplyZimmerFilter()
    End Sub

    Public Sub ApplyZimmerFilter()
        ' Sammle aktive Filter
        Dim aktiveFilter As New List(Of String)

        ' Durchlaufe alle Nodes
        For Each parentNode As TreeNode In treeView.Nodes
            For Each childNode As TreeNode In parentNode.Nodes
                If childNode.Checked Then
                    aktiveFilter.Add(childNode.Text)
                End If
            Next
        Next

        ' Filter auf Grid anwenden
        Dim dgvPatienten = parent.PatientenGrid

        For Each row As DataGridViewRow In dgvPatienten.Rows
            If row.IsNewRow Then Continue For

            Dim zimmerWert = row.Cells("Zimmer").Value?.ToString()

            If aktiveFilter.Count = 0 Then
                ' Keine Filter aktiv = alle anzeigen
                row.Visible = True
            Else
                ' Prüfen ob MINDESTENS EINES der gefilterten Zimmer im Zimmer-String vorkommt
                Dim zeigeZeile = False
                If Not String.IsNullOrEmpty(zimmerWert) Then
                    ' Zimmer-String in einzelne Zimmer aufteilen
                    Dim patientZimmer = zimmerWert.Split(","c).Select(Function(z) z.Trim()).ToList()

                    ' Prüfen ob mindestens ein gefiltertes Zimmer dabei ist
                    For Each filterZimmer In aktiveFilter
                        If patientZimmer.Contains(filterZimmer) Then
                            zeigeZeile = True
                            Exit For
                        End If
                    Next
                End If
                row.Visible = zeigeZeile
            End If
        Next

        ' Status aktualisieren
        UpdateFilterStatus(aktiveFilter)
    End Sub

    Private Sub UpdateFilterStatus(aktiveFilter As List(Of String))
        Dim dgvPatienten = parent.PatientenGrid
        Dim sichtbar = dgvPatienten.Rows.Cast(Of DataGridViewRow).
                    Where(Function(r) Not r.IsNewRow AndAlso r.Visible).Count()
        Dim total = dgvPatienten.Rows.Count
        If dgvPatienten.AllowUserToAddRows Then total -= 1

        Dim statusText As String
        If aktiveFilter.Count = 0 Then
            statusText = $"{total} Patienten (kein Filter aktiv)"
        Else
            statusText = $"{sichtbar} von {total} Patienten (Filter: {String.Join(", ", aktiveFilter)})"
        End If

        If TypeOf parent.StatusLabel Is ToolStripStatusLabel Then
            CType(parent.StatusLabel, ToolStripStatusLabel).Text = statusText
        End If
    End Sub

    Public Function GetAllCheckedNodes() As List(Of String)
        Dim checkedNodes As New List(Of String)

        For Each parentNode As TreeNode In treeView.Nodes
            For Each childNode As TreeNode In parentNode.Nodes
                If childNode.Checked Then
                    checkedNodes.Add(childNode.Text)
                End If
            Next
        Next

        Return checkedNodes
    End Function

    Public Sub CheckAllNodes(checked As Boolean)
        For Each node As TreeNode In treeView.Nodes
            node.Checked = checked
            For Each childNode As TreeNode In node.Nodes
                childNode.Checked = checked
            Next
        Next
    End Sub
End Class