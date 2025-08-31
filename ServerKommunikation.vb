Imports System.Net.Http
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports System.Data.SqlClient
Imports System.Linq

Public Class ServerKommunikation
    Private ReadOnly parent As FormPAS
    Private ReadOnly httpClient As HttpClient
    Private ReadOnly serviceUrl As String

    Public Sub New(parentForm As FormPAS)
        parent = parentForm
        httpClient = ConfigModule.HttpClient
        serviceUrl = ConfigModule.ServiceUrl
    End Sub

    Public Async Function HoleDatenVomServer() As Task(Of List(Of PatientInfo))
        Try
            Dim response = Await httpClient.GetAsync($"{serviceUrl}/api/warteschlange")

            If response.IsSuccessStatusCode Then
                Dim json = Await response.Content.ReadAsStringAsync()
                Return JsonConvert.DeserializeObject(Of List(Of PatientInfo))(json)
            Else
                Logger.Debug($"Server-Fehler: {response.StatusCode}")
                Return New List(Of PatientInfo)
            End If
        Catch ex As Exception
            Logger.Debug($"Fehler beim Datenabruf: {ex.Message}")
            Return New List(Of PatientInfo)
        End Try
    End Function

    Public Async Sub PatientManuellHinzufuegen(patientenID As String, vorname As String, nachname As String,
                                              prioritaet As Integer, bemerkung As String, istBesucher As Boolean,
                                              terminZeit As DateTime, zimmer As String)
        Try
            Dim patientName = If(String.IsNullOrWhiteSpace(vorname), nachname, $"{nachname}, {vorname}")

            Logger.Debug($"Sende an Server - PatID: {patientenID}, Name: {patientName}, Priorität: {prioritaet}")

            Dim values As New Dictionary(Of String, String) From {
                {"patientenID", patientenID},
                {"name", patientName},
                {"vorname", vorname},
                {"nachname", nachname},
                {"status", "Wartend"},
                {"zimmer", If(String.IsNullOrEmpty(zimmer), If(istBesucher, "Anmeldung", "Wartezimmer"), zimmer)},
                {"prioritaet", prioritaet.ToString()},
                {"bemerkung", bemerkung},
                {"ankunftszeit", terminZeit.ToString("yyyy-MM-dd HH:mm:ss")},
                {"istBesucher", istBesucher.ToString()}
            }

            Dim content = New FormUrlEncodedContent(values)
            Dim response = Await httpClient.PostAsync($"{serviceUrl}/api/neuerpatient", content)

            If Not response.IsSuccessStatusCode Then
                parent.Invoke(Sub()
                                  MessageBox.Show($"Warnung: Patient wurde nur lokal hinzugefügt. Server-Fehler: {response.StatusCode}",
                                            "Server-Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                              End Sub)
            End If
        Catch ex As Exception
            Logger.Debug($"Fehler beim Senden an Server: {ex.Message}")
        End Try
    End Sub

    Public Async Function UpdatePatient(patientenID As String, name As String, prioritaet As Integer,
                                       zimmer As String, bemerkung As String, status As String) As Task(Of Boolean)
        Try
            Dim values As New Dictionary(Of String, String) From {
                {"patientenID", patientenID},
                {"name", name},
                {"prioritaet", prioritaet.ToString()},
                {"zimmer", zimmer},
                {"bemerkung", bemerkung},
                {"status", status}
            }

            Dim content = New FormUrlEncodedContent(values)
            Dim response = Await httpClient.PostAsync($"{serviceUrl}/api/updatepatient", content)

            If Not response.IsSuccessStatusCode Then
                Logger.Debug($"Fehler beim Update: {response.StatusCode}")
                Return False
            End If
            Return True
        Catch ex As Exception
            Logger.Debug($"Fehler beim Server-Update: {ex.Message}")
            Return False
        End Try
    End Function

    Public Async Function StatusUpdate(patientenID As String, status As String, zeitpunkt As DateTime) As Task(Of Boolean)
        Try
            Dim values As New Dictionary(Of String, String) From {
                {"patientenID", patientenID},
                {"status", status},
                {"zeitpunkt", zeitpunkt.ToString("yyyy-MM-dd HH:mm:ss")}
            }

            Dim content = New FormUrlEncodedContent(values)
            Dim response = Await httpClient.PostAsync($"{serviceUrl}/api/statusupdate", content)

            Return response.IsSuccessStatusCode
        Catch ex As Exception
            Logger.Debug($"Fehler beim Status-Update: {ex.Message}")
            Return False
        End Try
    End Function

    Public Async Function ZimmerWechsel(patientenID As String, zimmer As String) As Task(Of Boolean)
        Try
            Dim values As New Dictionary(Of String, String) From {
                {"patientenID", patientenID},
                {"zimmer", zimmer}
            }

            Dim content = New FormUrlEncodedContent(values)
            Dim response = Await httpClient.PostAsync($"{serviceUrl}/api/zimmerwechsel", content)

            Return response.IsSuccessStatusCode
        Catch ex As Exception
            Logger.Debug($"Fehler beim Zimmerwechsel: {ex.Message}")
            Return False
        End Try
    End Function

    Public Async Function LadeTermineFuerTag(datum As Date) As Task(Of List(Of PatientInfo))
        Try
            Dim response = Await httpClient.GetAsync($"{serviceUrl}/api/warteschlange?datum={datum:yyyy-MM-dd}")

            If response.IsSuccessStatusCode Then
                Dim json = Await response.Content.ReadAsStringAsync()
                Return JsonConvert.DeserializeObject(Of List(Of PatientInfo))(json)
            End If
        Catch ex As Exception
            Logger.Debug($"Fehler beim Laden der Termine: {ex.Message}")
        End Try

        Return New List(Of PatientInfo)
    End Function

    Public Async Function LadeHistorie(datum As Date) As Task(Of List(Of PatientInfo))
        Try
            ' Historie aus Datenbank laden
            Dim patienten As New List(Of PatientInfo)

            Using conn As New SqlConnection(ConfigModule.SqlConnectionString)
                Await conn.OpenAsync()

                Dim query = "SELECT * FROM dbo.Warteschlange WHERE CAST(Ankunft AS DATE) = @datum ORDER BY Ankunft"
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@datum", datum.Date)

                    Using reader = Await cmd.ExecuteReaderAsync()
                        While reader.Read()
                            patienten.Add(New PatientInfo With {
                                .PatientenID = reader("PatNr").ToString(),
                                .Name = reader("Name").ToString(),
                                .Ankunftszeit = CDate(reader("Ankunft")),
                                .Zimmer = reader("Bereich").ToString(),
                                .Status = reader("Status").ToString(),
                                .Prioritaet = CInt(If(IsDBNull(reader("Prioritaet")), 0, reader("Prioritaet"))),
                                .Bemerkung = If(IsDBNull(reader("Bemerkung")), "", reader("Bemerkung").ToString())
                            })
                        End While
                    End Using
                End Using
            End Using

            Return patienten
        Catch ex As Exception
            Logger.Debug($"Fehler beim Laden der Historie: {ex.Message}")
            Return New List(Of PatientInfo)
        End Try
    End Function
End Class