Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Drawing

Public Class UpdateManager
    Private ReadOnly parent As FormPAS
    Private ReadOnly gridManager As PatientenGridManager
    Private ReadOnly serverComm As ServerKommunikation

    Private _isPaused As Boolean = False

    Public Sub PauseUpdates()
        _isPaused = True
    End Sub

    Public Sub ResumeUpdates()
        _isPaused = False
    End Sub


    Public Sub New(parentForm As FormPAS, grid As PatientenGridManager, server As ServerKommunikation)
        parent = parentForm
        gridManager = grid
        serverComm = server
    End Sub

    'Public Async Sub IntelligentesUpdate()
    '    If _isPaused Then Return
    '    Try
    '        ' Prüfen ob Edit-Modus aktiv
    '        If parent.PatientenGrid.IsCurrentCellInEditMode OrElse Control.MouseButtons <> MouseButtons.None Then
    '            Return
    '        End If

    '        ' Prüfen ob von Planung/Historie gekommen
    '        Dim vonPlanungGekommen = False
    '        If parent.PatientenGrid.Rows.Count > 0 Then
    '            For Each row As DataGridViewRow In parent.PatientenGrid.Rows
    '                If Not row.IsNewRow AndAlso row.Cells("Status").Value?.ToString() = "Geplant" Then
    '                    vonPlanungGekommen = True
    '                    Exit For
    '                End If
    '            Next
    '        End If

    '        If vonPlanungGekommen Then
    '            parent.PatientenGrid.Rows.Clear()
    '        End If

    '        ' Neue Daten vom Server holen
    '        Dim neueDaten = Await serverComm.HoleDatenVomServer()
    '        Dim aktuellePatientenIDs As New HashSet(Of String)

    '        ' Existierende Zeilen sammeln
    '        Dim existierendeZeilen As New Dictionary(Of String, DataGridViewRow)
    '        For Each row As DataGridViewRow In parent.PatientenGrid.Rows
    '            If Not row.IsNewRow Then
    '                Dim patID = row.Cells("PatientenID").Value?.ToString()
    '                If Not String.IsNullOrEmpty(patID) Then
    '                    existierendeZeilen(patID) = row
    '                End If
    '            End If
    '        Next

    '        ' Daten aktualisieren oder hinzufügen
    '        For Each patient In neueDaten
    '            aktuellePatientenIDs.Add(patient.PatientenID)

    '            If existierendeZeilen.ContainsKey(patient.PatientenID) Then
    '                ' Existierende Zeile aktualisieren
    '                UpdateExistingRow(existierendeZeilen(patient.PatientenID), patient)
    '            Else
    '                ' Neue Zeile hinzufügen
    '                AddNewRow(patient)
    '            End If
    '        Next

    '        ' Entfernte Patienten löschen
    '        RemoveOldPatients(aktuellePatientenIDs)

    '        ' Grid aktualisieren
    '        gridManager.SortierePatienten()
    '        gridManager.FaerbeZeilenNachStatus()
    '        parent.PatientenGrid.ClearSelection()
    '        parent.UpdateStatusBar()

    '    Catch ex As Exception
    '        Logger.Debug($"Update-Fehler: {ex.Message}")
    '    End Try
    'End Sub

    Public Async Sub IntelligentesUpdate()
        If _isPaused Then Return
        Try
            ' Prüfen ob Edit-Modus aktiv
            If parent.PatientenGrid.IsCurrentCellInEditMode OrElse Control.MouseButtons <> MouseButtons.None Then
                Return
            End If

            ' Neue Daten vom Server holen
            Dim neueDaten = Await serverComm.HoleDatenVomServer()
            Dim aktuellePatientenIDs As New HashSet(Of String)

            ' WICHTIG: Existierende Zeilen sammeln BEVOR wir ändern
            Dim existierendeZeilen As New Dictionary(Of String, DataGridViewRow)

            ' Duplikat-Prüfung: PatientenIDs sammeln um doppelte zu entfernen
            Dim gefundeneIDs As New HashSet(Of String)

            For i As Integer = parent.PatientenGrid.Rows.Count - 1 To 0 Step -1
                Dim row = parent.PatientenGrid.Rows(i)
                If Not row.IsNewRow Then
                    Dim patID = row.Cells("PatientenID").Value?.ToString()
                    If Not String.IsNullOrEmpty(patID) Then
                        ' Duplikat gefunden - entfernen
                        If gefundeneIDs.Contains(patID) Then
                            Logger.Debug($"Duplikat entfernt: {patID}")
                            parent.PatientenGrid.Rows.RemoveAt(i)
                        Else
                            gefundeneIDs.Add(patID)
                            existierendeZeilen(patID) = row
                        End If
                    End If
                End If
            Next

            ' Daten aktualisieren oder hinzufügen
            For Each patient In neueDaten
                aktuellePatientenIDs.Add(patient.PatientenID)

                If existierendeZeilen.ContainsKey(patient.PatientenID) Then
                    ' Existierende Zeile aktualisieren
                    UpdateExistingRow(existierendeZeilen(patient.PatientenID), patient)
                Else
                    ' Neue Zeile hinzufügen - aber nur wenn nicht schon vorhanden
                    Dim bereitsVorhanden = False
                    For Each row As DataGridViewRow In parent.PatientenGrid.Rows
                        If Not row.IsNewRow AndAlso row.Cells("PatientenID").Value?.ToString() = patient.PatientenID Then
                            bereitsVorhanden = True
                            Exit For
                        End If
                    Next

                    If Not bereitsVorhanden Then
                        AddNewRow(patient)
                    End If
                End If
            Next

            ' Entfernte Patienten löschen
            RemoveOldPatients(aktuellePatientenIDs)

            ' Grid aktualisieren
            gridManager.SortierePatienten()
            gridManager.FaerbeZeilenNachStatus()
            parent.PatientenGrid.ClearSelection()
            parent.UpdateStatusBar()

        Catch ex As Exception
            Logger.Debug($"Update-Fehler: {ex.Message}")
        End Try
    End Sub
    Public Sub BereinigeDuplikate()
        Try
            Dim gefundeneIDs As New HashSet(Of String)
            Dim zuEntfernen As New List(Of Integer)

            For i As Integer = 0 To parent.PatientenGrid.Rows.Count - 1
                Dim row = parent.PatientenGrid.Rows(i)
                If Not row.IsNewRow Then
                    Dim patID = row.Cells("PatientenID").Value?.ToString()
                    If Not String.IsNullOrEmpty(patID) Then
                        If gefundeneIDs.Contains(patID) Then
                            zuEntfernen.Add(i)
                            Logger.Debug($"Duplikat-Bereinigung: {patID} entfernt")
                        Else
                            gefundeneIDs.Add(patID)
                        End If
                    End If
                End If
            Next

            ' Duplikate von hinten nach vorne entfernen
            For i As Integer = zuEntfernen.Count - 1 To 0 Step -1
                parent.PatientenGrid.Rows.RemoveAt(zuEntfernen(i))
            Next

            If zuEntfernen.Count > 0 Then
                Logger.Debug($"{zuEntfernen.Count} Duplikate bereinigt")
            End If

        Catch ex As Exception
            Logger.Debug($"Fehler bei Duplikat-Bereinigung: {ex.Message}")
        End Try
    End Sub

    Private Sub UpdateExistingRow(row As DataGridViewRow, patient As PatientInfo)
        ' Status-Update nur wenn nicht "Geplant"
        Dim currentStatus = row.Cells("Status").Value?.ToString()

        If currentStatus = "Geplant" Then
            ' Geplante Patienten nicht automatisch aktualisieren
            ' Nur Priorität aktualisieren falls geändert
            If row.Cells("PrioritaetWert").Value?.ToString() <> patient.Prioritaet.ToString() Then
                row.Cells("PrioritaetWert").Value = patient.Prioritaet
                row.Cells("Prioritaet").Value = gridManager.GetPrioritaetText(patient.Prioritaet)
            End If
            Return
        End If

        ' Status aktualisieren
        If row.Cells("Status").Value?.ToString() <> patient.Status Then
            row.Cells("Status").Value = patient.Status
        End If

        ' Zimmer aktualisieren (wenn nicht in Bearbeitung)
        If Not row.Cells("Zimmer").IsInEditMode Then
            If row.Cells("Zimmer").Value?.ToString() <> patient.Zimmer Then
                row.Cells("Zimmer").Value = patient.Zimmer
            End If
        End If

        ' Wartezeit berechnen
        Dim tatsaechlicheWartezeit = CInt(Math.Floor((DateTime.Now - patient.Ankunftszeit).TotalMinutes))
        Dim geschaetzteRestzeit = BerechneWartezeit(row)

        If patient.Status = "Wartend" Then
            row.Cells("Wartezeit").Value = $"{tatsaechlicheWartezeit} min (noch ca. {geschaetzteRestzeit} min)"
            'Else
            '   row.Cells("Wartezeit").Value = $"{tatsaechlicheWartezeit} min"
        End If

        ' Priorität aktualisieren
        Logger.Debug($"Patient {patient.PatientenID}: Server-Priorität={patient.Prioritaet}, Grid-Priorität={row.Cells("PrioritaetWert").Value}")

        If row.Cells("PrioritaetWert").Value?.ToString() <> patient.Prioritaet.ToString() Then
            Logger.Debug($"Ändere Priorität von {row.Cells("PrioritaetWert").Value} auf {patient.Prioritaet}")
            row.Cells("PrioritaetWert").Value = patient.Prioritaet
            row.Cells("Prioritaet").Value = gridManager.GetPrioritaetText(patient.Prioritaet)
        End If
    End Sub

    Private Sub AddNewRow(patient As PatientInfo)
        Dim index = parent.PatientenGrid.Rows.Add()
        Dim newRow = parent.PatientenGrid.Rows(index)

        newRow.Cells("PatientenID").Value = patient.PatientenID
        newRow.Cells("Name").Value = patient.Name
        newRow.Cells("Ankunftszeit").Value = patient.Ankunftszeit
        newRow.Cells("Wartezeit").Value = $"{CInt(Math.Floor((DateTime.Now - patient.Ankunftszeit).TotalMinutes))} min"
        newRow.Cells("Zimmer").Value = patient.Zimmer
        newRow.Cells("Status").Value = patient.Status
        newRow.Cells("PrioritaetWert").Value = patient.Prioritaet
        newRow.Cells("Prioritaet").Value = gridManager.GetPrioritaetText(patient.Prioritaet)
        newRow.Cells("Bemerkung").Value = patient.Bemerkung

        ' Highlight für neue Zeile
        newRow.DefaultCellStyle.BackColor = Color.LightGreen
        Task.Delay(2000).ContinueWith(Sub(t)
                                          parent.Invoke(Sub()
                                                            If newRow.Index >= 0 AndAlso newRow.Index < parent.PatientenGrid.Rows.Count Then
                                                                newRow.DefaultCellStyle.BackColor = Color.White
                                                            End If
                                                        End Sub)
                                      End Sub)
    End Sub

    Private Sub RemoveOldPatients(aktuellePatientenIDs As HashSet(Of String))
        For i As Integer = parent.PatientenGrid.Rows.Count - 1 To 0 Step -1
            If Not parent.PatientenGrid.Rows(i).IsNewRow Then
                Dim patID = parent.PatientenGrid.Rows(i).Cells("PatientenID").Value?.ToString()
                Dim status = parent.PatientenGrid.Rows(i).Cells("Status").Value?.ToString()

                ' Fertige Patienten bleiben den ganzen Tag sichtbar
                If status = "Fertig" Then
                    Continue For
                End If

                ' Neue Patienten nicht sofort löschen (30 Sekunden Schutz)
                Dim ankunftszeit = parent.PatientenGrid.Rows(i).Cells("Ankunftszeit").Value
                If ankunftszeit IsNot Nothing Then
                    Dim zeitDifferenz = DateTime.Now.Subtract(CDate(ankunftszeit)).TotalSeconds
                    If zeitDifferenz < 30 Then
                        Continue For
                    End If
                End If

                If Not String.IsNullOrEmpty(patID) AndAlso Not aktuellePatientenIDs.Contains(patID) Then
                    parent.PatientenGrid.Rows.RemoveAt(i)
                End If
            End If
        Next
    End Sub

    Private Function BerechneWartezeit(row As DataGridViewRow) As Integer
        ' Vereinfachte Berechnung - kann erweitert werden
        Dim basiszeitProPatient = 15 ' Minuten
        Dim position = row.Index
        Return position * basiszeitProPatient
    End Function


End Class
