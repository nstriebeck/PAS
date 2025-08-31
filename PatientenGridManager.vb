Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports System.Linq

Public Class PatientenGridManager
    Private ReadOnly dgvPatienten As DataGridView
    Private ReadOnly parent As FormPAS
    Private isUpdating As Boolean = False

    Public Sub New(grid As DataGridView, parentForm As FormPAS)
        dgvPatienten = grid
        parent = parentForm
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
        ' Bei Notfall im Wartend/Aufgerufen-Status nicht färben (Blink-Effekt hat Vorrang)
        If prioritaet = 2 AndAlso (status = "Wartend" OrElse status = "Aufgerufen") Then
            ' Nichts tun - lass den Blink-Timer die Farbe setzen
            Return
        End If

        Dim backColor As Color

        Select Case status
            Case "Wartend"
                backColor = Color.FromArgb(255, 230, 230)
            Case "Aufgerufen"
                backColor = Color.FromArgb(255, 255, 200)
            Case "InBehandlung"
                backColor = Color.FromArgb(230, 255, 230)
            Case "Fertig"
                backColor = Color.FromArgb(230, 230, 255)
            Case "Geplant"
                backColor = Color.LightCyan
            Case Else
                backColor = Color.White
        End Select

        ' Priorität Dringend
        If prioritaet = 1 Then
            row.DefaultCellStyle.ForeColor = Color.DarkOrange
        Else
            row.DefaultCellStyle.ForeColor = Color.Black
        End If

        row.DefaultCellStyle.BackColor = backColor


        '' Bei Notfall überschreiben (außer bei Blink-Effekt)
        'If prioritaet = 2 AndAlso Not parent.IsBlinking Then
        '    If status = "Fertig" Then
        '        row.Cells("Prioritaet").Style.BackColor = Color.LightCoral
        '    Else
        '        backColor = Color.FromArgb(255, 150, 150)
        '    End If
        '    row.DefaultCellStyle.ForeColor = Color.DarkRed
        '    row.DefaultCellStyle.Font = New Font(dgvPatienten.Font, FontStyle.Bold)
        'ElseIf prioritaet = 1 Then
        '    row.DefaultCellStyle.ForeColor = Color.DarkOrange
        'Else
        '    row.DefaultCellStyle.ForeColor = Color.Black
        'End If

        'row.DefaultCellStyle.BackColor = backColor
    End Sub

    Public Sub SortierePatienten()
        If dgvPatienten.Rows.Count = 0 Then Return

        Dim sortedList = dgvPatienten.Rows.Cast(Of DataGridViewRow)().
        Where(Function(r) Not r.IsNewRow).
        OrderBy(Function(r)
                    ' Erst nach Status (Fertig = 0, andere = 1)
                    If r.Cells("Status").Value?.ToString() = "Fertig" Then
                        Return 0
                    Else
                        Return 1
                    End If
                End Function).
        ThenByDescending(Function(r)
                             ' Dann nach Priorität (nur bei nicht-fertigen)
                             If r.Cells("Status").Value?.ToString() <> "Fertig" Then
                                 Return CInt(If(r.Cells("PrioritaetWert").Value, 0))
                             Else
                                 Return -1 ' Fertige ignorieren Priorität
                             End If
                         End Function).
        ThenBy(Function(r) r.Cells("Ankunftszeit").Value).
        ToList()

        dgvPatienten.Rows.Clear()
        dgvPatienten.Rows.AddRange(sortedList.ToArray())
        dgvPatienten.ClearSelection()
    End Sub

    Public Sub SetzeHistorieModus(aktivieren As Boolean)
        If aktivieren Then
            If Not dgvPatienten.Columns.Contains("Aufgerufen") Then
                dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
                    .Name = "Aufgerufen",
                    .HeaderText = "Aufgerufen",
                    .Width = 70,
                    .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "HH:mm"}
                })

                dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
                    .Name = "Behandlungsbeginn",
                    .HeaderText = "Beginn",
                    .Width = 70,
                    .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "HH:mm"}
                })

                dgvPatienten.Columns.Add(New DataGridViewTextBoxColumn With {
                    .Name = "Behandlungsende",
                    .HeaderText = "Ende",
                    .Width = 70,
                    .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "HH:mm"}
                })
            End If
        End If
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