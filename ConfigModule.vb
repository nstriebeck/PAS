Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Net.Http
Imports Newtonsoft.Json

' Struktur für Bereiche/Zimmer
Public Class Bereich
    Public Property ID As Integer
    Public Property Bezeichnung As String
    Public Property Typ As String
    Public Property Aktiv As Boolean
    Public Property Reihenfolge As Integer
End Class

' Struktur für Patientendaten
Public Class PatientInfo
    Public Property PatientenID As String
    Public Property ScheinID As String
    Public Property Name As String
    Public Property Vorname As String
    Public Property Nachname As String
    Public Property Geburtsdatum As String
    Public Property Ankunftszeit As DateTime
    Public Property Zimmer As String
    Public Property Status As String
    Public Property Prioritaet As Integer
    Public Property Bemerkung As String
    Public Property Wartezeit As Integer ' NEU - in Minuten
End Class

' Klasse für Historie-Einträge
Public Class HistorieEintrag
    Public Property PatientenID As String
    Public Property ScheinID As String
    Public Property Name As String
    Public Property Geburtsdatum As String
    Public Property Ankunftszeit As DateTime
    Public Property AufrufZeit As DateTime?
    Public Property BehandlungStart As DateTime?
    Public Property BehandlungEnde As DateTime?
    Public Property Wartezeit As Integer
    Public Property Zimmer As String
    Public Property Bemerkung As String
    Public Property Mitarbeiter As String
End Class

Public Module ConfigModule
    ' Zentrale Datenbank-Konfiguration
    Public ReadOnly Property SqlConnectionString As String
        Get
            ' Aus App.config laden oder fallback
            Dim connStr = ConfigurationManager.ConnectionStrings("PASDatabase")?.ConnectionString
            If String.IsNullOrEmpty(connStr) Then
                connStr = "Server=SILINSQL\PatientenAufruf;Database=PAS_Database;User Id=sa;Password=PatientenAufruf4711;"
            End If
            Return connStr
        End Get
    End Property

    Public ReadOnly HttpClient As New HttpClient()
    Public ReadOnly ServiceUrl As String = ConfigurationManager.AppSettings("PASServerUrl")
    Public BereicheListe As List(Of Bereich) = New List(Of Bereich)

    Public Async Function LadeBereicheAsync() As Task
        Try
            Logger.Debug($"Lade Bereiche von: {ServiceUrl}/api/bereiche")
            Dim response = Await HttpClient.GetAsync($"{ServiceUrl}/api/bereiche")

            If response.IsSuccessStatusCode Then
                Dim json = Await response.Content.ReadAsStringAsync()
                BereicheListe = JsonConvert.DeserializeObject(Of List(Of Bereich))(json)
                Logger.Debug($"Bereiche geladen: {BereicheListe.Count}")
            Else
                Logger.Debug($"Server antwortet mit: {response.StatusCode}")
                LadeBereicheAusDatenbank() ' Ohne Await im Catch
            End If
        Catch ex As Exception
            Logger.Debug($"Fehler beim Server-Abruf: {ex.Message}")
            LadeBereicheAusDatenbank() ' Ohne Await im Catch
        End Try
    End Function

    Private Sub LadeBereicheAusDatenbank()
        Try
            Using conn As New SqlConnection(SqlConnectionString) ' Zentrale Property verwenden
                conn.Open()
                Dim cmd As New SqlCommand("SELECT * FROM Bereiche WHERE Aktiv = 1 ORDER BY Reihenfolge", conn)  ' SqlCommand
                Dim reader = cmd.ExecuteReader()

                BereicheListe.Clear()
                While reader.Read()
                    BereicheListe.Add(New Bereich With {
                        .ID = CInt(reader("ID")),
                        .Bezeichnung = reader("Bezeichnung").ToString(),
                        .Typ = If(reader("Typ") Is DBNull.Value, "", reader("Typ").ToString()),
                        .Aktiv = True,
                        .Reihenfolge = CInt(reader("Reihenfolge"))
                    })
                End While
                Logger.Debug($"Bereiche aus DB geladen: {BereicheListe.Count}")
            End Using
        Catch ex As Exception
            Logger.Debug($"Fehler beim DB-Zugriff: {ex.Message}")
            ' Fallback auf Standardwerte
            BereicheListe.Clear()
            BereicheListe.Add(New Bereich With {.ID = 1, .Bezeichnung = "Anmeldung", .Typ = "Sonstige", .Aktiv = True, .Reihenfolge = 1})
            BereicheListe.Add(New Bereich With {.ID = 2, .Bezeichnung = "Wartezimmer", .Typ = "Zimmer", .Aktiv = True, .Reihenfolge = 2})
            BereicheListe.Add(New Bereich With {.ID = 3, .Bezeichnung = "Zimmer 1", .Typ = "Zimmer", .Aktiv = True, .Reihenfolge = 3})
        End Try
    End Sub
End Module