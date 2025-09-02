Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports System.Linq

Public Class PatientenGridManager
    Private ReadOnly dgvPatienten As DataGridView
    Private ReadOnly parent As FormPAS
    Private isUpdating As Boolean = False
    Private _farbCache As Dictionary(Of String, Color)

    Public Sub New(grid As DataGridView, parentForm As FormPAS)
        dgvPatienten = grid
        parent = parentForm
        LoadeFarbenAusDB()
    End Sub
    Public Sub InitializeGrid()
        dgvPatienten.Columns.Clear()
        dgvPatienten.AutoGenerateColumns = False
        dgvPatienten.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvPatienten.MultiSelect = False
        dgvPatienten.AllowUserToAddRows = False

        ' Spalten definieren
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "PatientenID",
            .HeaderText = "Pat-Nr",
            .Width = 70,
            .ReadOnly = True
        })

        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Name",
            .HeaderText = "Name",
            .Width = 150,
            .ReadOnly = True
        })

        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Ankunftszeit",
            .HeaderText = "Ankunft",
            .Width = 70,
            .ReadOnly = True,
            .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "HH:mm"}
        })

        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Wartezeit",
            .HeaderText = "Wartet",
            .Width = 60,
            .ReadOnly = True
        })

        ' Zimmer als normale TextBox-Spalte (keine ComboBox mehr)
        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Zimmer",
            .HeaderText = "Zimmer",
            .Width = 150,
            .ReadOnly = True
        })

        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Status",
            .HeaderText = "Status",
            .Width = 100,
            .ReadOnly = True
        })

        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Prioritaet",
            .HeaderText = "Priorität",
            .Width = 80,
            .ReadOnly = True
        })

        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "PrioritaetWert",
            .HeaderText = "PrioWert",
            .Width = 0,
            .Visible = False
        })

        dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Bemerkung",
            .HeaderText = "Bemerkung",
            .Width = 150,
            .ReadOnly = False
        })

        AddHandler dgvPatienten.DataError, AddressOf dgvPatienten_DataError
    End Sub

    Private Sub LoadeFarbenAusDB()
        _farbCache = New Dictionary(Of String, Color)

        Try
            Using conn As New SqlConnection(ConfigModule.SqlConnectionString)
                conn.Open()

                ' Prüfen ob Tabelle existiert
                Dim checkCmd As New SqlCommand(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Farbkonfiguration'", conn)

                If CInt(checkCmd.ExecuteScalar()) > 0 Then
                    ' Farben aus DB laden
                    Dim query = "SELECT StatusName, FarbeHex FROM Farbkonfiguration"
                    Using cmd As New SqlCommand(query, conn)
                        Using reader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim statusName = reader.GetString(0).ToLower()
                                Dim hexColor = reader.GetString(1)
                                Try
                                    _farbCache(statusName) = ColorTranslator.FromHtml(hexColor)
                                    Logger.Debug($"Farbe geladen: {statusName} = {hexColor}")
                                Catch
                                    ' Bei Fehler Standard-Farbe verwenden
                                End Try
                            End While
                        End Using
                    End Using
                End If
            End Using
        Catch ex As Exception
            Logger.Debug($"Fehler beim Laden der Farben: {ex.Message}")
        End Try

        ' Falls keine Farben geladen wurden, Standards setzen
        If _farbCache.Count = 0 Then
            SetzeStandardFarben()
        End If
    End Sub

    Private Sub SetzeStandardFarben()
        _farbCache("wartend") = Color.FromArgb(255, 152, 0)      ' Kräftiges Orange
        _farbCache("aufgerufen") = Color.FromArgb(76, 175, 80)   ' Kräftiges Grün
        _farbCache("inbehandlung") = Color.FromArgb(33, 150, 243) ' Kräftiges Blau
        _farbCache("fertig") = Color.FromArgb(158, 158, 158)     ' Mittelgrau
        _farbCache("notfall") = Color.FromArgb(244, 67, 54)      ' Kräftiges Rot
        _farbCache("notfallblink") = Color.FromArgb(255, 205, 210) ' Hellrot
    End Sub

    Private Function GetFarbe(status As String) As Color
        Dim key = status.ToLower().Replace(" ", "")
        If _farbCache.ContainsKey(key) Then
            Return _farbCache(key)
        End If
        Return Color.White
    End Function

    Public Sub FaerbeZeilenNachStatus()
        For Each row As DataGridViewRow In dgvPatienten.Rows
            If row.IsNewRow Then Continue For

            Dim status = row.Cells("Status").Value?.ToString()
            Dim prioritaet = CInt(If(row.Cells("PrioritaetWert").Value, 0))

            SetzeZeilenFarbe(row, status, prioritaet)
        Next
        dgvPatienten.Refresh()
    End Sub

    Private Sub SetzeZeilenFarbe(row As DataGridViewRow, status As String, prioritaet As Integer)
        ' Bei Notfall mit Status Wartend/Aufgerufen - Blink-Timer übernimmt
        If prioritaet = 2 AndAlso (status = "Wartend" OrElse status = "Aufgerufen") Then
            Return ' Blink-Timer macht die Arbeit
        End If

        ' Farbe aus Cache holen
        Dim backColor As Color = GetFarbe(status)
        row.DefaultCellStyle.BackColor = backColor

        ' Text-Farbe anpassen basierend auf Hintergrund
        If backColor.GetBrightness() < 0.5 Then
            row.DefaultCellStyle.ForeColor = Color.White
        Else
            row.DefaultCellStyle.ForeColor = Color.Black
        End If

        ' Priorität Dringend
        If prioritaet = 1 Then
            row.DefaultCellStyle.Font = New Font(dgvPatienten.Font, FontStyle.Bold)
        End If
    End Sub

    Public Sub SortierePatienten()
        If dgvPatienten.Rows.Count = 0 Then Return

        ' Merke aktuelle PatientenID und ScrollPosition
        Dim selectedPatID As String = Nothing
        Dim scrollPosition = dgvPatienten.FirstDisplayedScrollingRowIndex

        If dgvPatienten.CurrentRow IsNot Nothing Then
            selectedPatID = dgvPatienten.CurrentRow.Cells("PatientenID").Value?.ToString()
        End If

        Dim sortedList = dgvPatienten.Rows.Cast(Of DataGridViewRow)().
        Where(Function(r) Not r.IsNewRow).
        OrderBy(Function(r)
                    If r.Cells("Status").Value?.ToString() = "Fertig" Then
                        Return 0
                    Else
                        Return 1
                    End If
                End Function).
        ThenByDescending(Function(r)
                             If r.Cells("Status").Value?.ToString() <> "Fertig" Then
                                 Return CInt(If(r.Cells("PrioritaetWert").Value, 0))
                             Else
                                 Return -1
                             End If
                         End Function).
        ThenBy(Function(r) r.Cells("Ankunftszeit").Value).
        ToList()

        ' Sortierung wie vorher...
        'Dim sortedList = dgvPatienten.Rows.Cast(Of DataGridViewRow)().[...]

        ' Suspend Layout für Performance
        dgvPatienten.SuspendLayout()

        dgvPatienten.Rows.Clear()
        dgvPatienten.Rows.AddRange(sortedList.ToArray())

        ' Selektion komplett entfernen
        dgvPatienten.ClearSelection()
        dgvPatienten.CurrentCell = Nothing

        ' Scroll-Position wiederherstellen
        If scrollPosition >= 0 AndAlso scrollPosition < dgvPatienten.Rows.Count Then
            dgvPatienten.FirstDisplayedScrollingRowIndex = scrollPosition
        End If

        dgvPatienten.ResumeLayout()
    End Sub

    Public Function GetPrioritaetText(prioritaet As Integer) As String
        Select Case prioritaet
            Case 2
                Return "NOTFALL"
            Case 1
                Return "Dringend"
            Case Else
                Return "Normal"
        End Select
    End Function

    Private Sub dgvPatienten_DataError(sender As Object, e As DataGridViewDataErrorEventArgs)
        e.ThrowException = False
    End Sub

    Public Sub RefreshGrid()
        dgvPatienten.Refresh()
    End Sub

    Public Sub ClearGrid()
        dgvPatienten.Rows.Clear()
    End Sub
End Class