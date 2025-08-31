Imports System.IO
Imports System.Text
Imports PAS.GDTParser

Public Class GDTParser
    ' GDT-Feldkennungen (die wichtigsten)
    Public Const SATZART As String = "8000"
    Public Const PATIENTENNUMMER As String = "8001"
    Public Const GDT_VERSION As String = "9218"
    Public Const PATIENTENKENNUNG As String = "3000"
    Public Const NACHNAME As String = "3101"
    Public Const VORNAME As String = "3102"
    Public Const GEBURTSDATUM As String = "3103"
    Public Const GESCHLECHT As String = "3110"
    Public Const WOHNORT As String = "3106"
    Public Const STRASSE As String = "3107"

    Public Class XMLPatient
        Public Property PatientenID As String
        Public Property ScheinID As String
        Public Property Name As String
        Public Property Nachname As String
        Public Property Vorname As String
        Public Property Geburtsdatum As Date?
        Public Property Geschlecht As String
        Public Property Strasse As String
        Public Property PLZ As String
        Public Property Ort As String
        Public Property Ankunftszeit As DateTime
        Public Property Status As String
        Public Property Zimmer As String
        Public Property Prioritaet As Integer
        Public Property Bemerkung As String
    End Class

    Public Class GDTPatient
        Public Property PatientenNummer As String
        Public Property PatientenKennung As String
        Public Property Nachname As String
        Public Property Vorname As String
        Public Property Geburtsdatum As String
        Public Property Geschlecht As String
        Public Property Wohnort As String
        Public Property Strasse As String
        Public Property GDTVersion As String

        ' Computed Properties
        Public ReadOnly Property VollName As String
            Get
                Return $"{Vorname} {Nachname}".Trim()
            End Get
        End Property

        Public ReadOnly Property PatientenID As String
            Get
                ' Verwende PatientenKennung als primäre ID, fallback auf PatientenNummer
                Return If(String.IsNullOrEmpty(PatientenKennung), PatientenNummer, PatientenKennung)
            End Get
        End Property

        Public ReadOnly Property GeschlechtText As String
            Get
                Select Case Geschlecht
                    Case "1"
                        Return "männlich"
                    Case "2"
                        Return "weiblich"
                    Case Else
                        Return "unbekannt"
                End Select
            End Get
        End Property

        Public ReadOnly Property FormatierteAdresse As String
            Get
                Dim adresse As String = ""
                If Not String.IsNullOrEmpty(Strasse) Then
                    adresse = Strasse
                End If
                If Not String.IsNullOrEmpty(Wohnort) Then
                    If Not String.IsNullOrEmpty(adresse) Then
                        adresse += ", "
                    End If
                    adresse += Wohnort
                End If
                Return adresse
            End Get
        End Property
    End Class

    Public Shared Function ParseMedicalOfficeXML(xmlPath As String) As XMLPatient
        Try
            If Not File.Exists(xmlPath) Then Return Nothing

            Dim doc = XDocument.Load(xmlPath)
            Dim patient As New XMLPatient

            ' Session für Schein-ID
            Dim sessionElement = doc.Descendants("session").FirstOrDefault()
            If sessionElement IsNot Nothing Then
                Dim sessionParts = sessionElement.Value.Split("#"c)
                If sessionParts.Length > 0 Then
                    patient.ScheinID = sessionParts(0)  ' Neue Property in PatientInfo
                    Logger.Debug($"ScheinID aus XML: {patient.ScheinID}")
                End If
            End If

            ' Patientendaten
            patient.PatientenID = doc.Descendants("patnr").FirstOrDefault()?.Value
            Logger.Debug($"PatientenID aus XML: {patient.PatientenID}")

            patient.Nachname = doc.Descendants("nachname").FirstOrDefault()?.Value
            Logger.Debug($"Nachname aus XML: {patient.Nachname}")

            patient.Vorname = doc.Descendants("vorname").FirstOrDefault()?.Value
            Logger.Debug($"Vorname aus XML: {patient.Vorname}")

            patient.Name = $"{patient.Nachname}, {patient.Vorname}"
            Logger.Debug($"Name gesetzt: {patient.Name}")

            ' Geburtsdatum parsen (Format: YYYYMMDD)
            Dim gebString = doc.Descendants("geburtstag").FirstOrDefault()?.Value
            If gebString?.Length = 8 Then
                Dim jahr = gebString.Substring(0, 4)
                Dim monat = gebString.Substring(4, 2)
                Dim tag = gebString.Substring(6, 2)
                patient.Geburtsdatum = New DateTime(CInt(jahr), CInt(monat), CInt(tag))
                Logger.Debug($"Geburtsdatum gesetzt: {patient.Geburtsdatum}")
            End If

            ' Weitere Felder
            patient.Ankunftszeit = DateTime.Now
            patient.Status = "Wartend"
            patient.Zimmer = "Wartezimmer"
            patient.Prioritaet = 0
            patient.Bemerkung = ""

            Return patient

        Catch ex As Exception
            Logger.Debug($"XML-Parse-Fehler: {ex.Message}")
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' Parst eine GDT-Datei und extrahiert Patientendaten
    ''' </summary>
    ''' <param name="filePath">Pfad zur GDT-Datei</param>
    ''' <returns>GDTPatient-Objekt mit den extrahierten Daten</returns>
    Public Shared Function ParseGDTFile(filePath As String) As GDTPatient
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"GDT-Datei nicht gefunden: {filePath}")
        End If

        Dim patient As New GDTPatient()

        Try
            ' GDT-Datei ist normalerweise in CP850 (DOS) kodiert, aber versuchen wir UTF-8
            'Dim lines() As String = File.ReadAllLines(filePath, Encoding.UTF8)
            ' GDT-Datei korrekt mit CP850 lesen
            'Dim lines() As String = File.ReadAllLines(filePath, Encoding.GetEncoding(437))


            ' Teste verschiedene Encodings
            Dim lines() As String = Nothing
            Dim fileBytes = File.ReadAllBytes(filePath)

            ' Versuche zu erkennen welches Encoding
            ' Prüfe ob Umlaute vorhanden sind (typische Byte-Werte)
            Dim has850Chars = fileBytes.Any(Function(b) b = 132 Or b = 142 Or b = 148 Or b = 153 Or b = 129 Or b = 154)
            Dim has437Chars = fileBytes.Any(Function(b) b = 225 Or b = 154 Or b = 129)

            If has850Chars Then
                lines = File.ReadAllLines(filePath, Encoding.GetEncoding(850))
                Logger.Debug("GDT gelesen mit CP850")
            ElseIf has437Chars Then
                lines = File.ReadAllLines(filePath, Encoding.GetEncoding(437))
                Logger.Debug("GDT gelesen mit CP437")
            Else
                ' Fallback: Windows-1252 (ANSI)
                lines = File.ReadAllLines(filePath, Encoding.GetEncoding(1252))
                Logger.Debug("GDT gelesen mit Windows-1252")
            End If


            For Each line As String In lines
                If line.Length >= 7 Then
                    ' GDT-Format: LLL KKKK DDDD...
                    ' LLL = 3-stellige Länge
                    ' KKKK = 4-stellige Feldkennung  
                    ' DDDD = Daten

                    Dim length As String = line.Substring(0, 3)
                    Dim fieldId As String = line.Substring(3, 4)

                    ' Daten extrahieren (ab Position 7)
                    Dim data As String = ""
                    If line.Length > 7 Then
                        data = line.Substring(7)
                    End If

                    Logger.Debug($"GDT Feld {fieldId}: {data}")

                    'Wird nicht mehr verwendet, weil die Datei mit der korrekten Codierung eingelesen wird
                    'data = GDTUmlautConverter.ConvertUmlaute(data, False)

                    ' Felder zuordnen
                    Select Case fieldId
                        Case PATIENTENNUMMER
                            patient.PatientenNummer = data
                        Case PATIENTENKENNUNG
                            patient.PatientenKennung = data
                        Case NACHNAME
                            patient.Nachname = data
                        Case VORNAME
                            patient.Vorname = data
                        Case GEBURTSDATUM
                            patient.Geburtsdatum = data
                        Case GESCHLECHT
                            patient.Geschlecht = data
                        Case WOHNORT
                            patient.Wohnort = data
                        Case STRASSE
                            patient.Strasse = data
                        Case GDT_VERSION
                            patient.GDTVersion = data
                    End Select
                End If
            Next

            Return patient

        Catch ex As Exception
            Throw New Exception($"Fehler beim Parsen der GDT-Datei: {ex.Message}")
        End Try
    End Function

    ''' <summary>
    ''' Überwacht einen Ordner auf neue GDT-Dateien
    ''' </summary>
    ''' <param name="watchFolder">Zu überwachender Ordner</param>
    ''' <param name="filePattern">Dateiname-Pattern (z.B. "*.gdt")</param>
    ''' <param name="callback">Callback-Funktion die bei neuer Datei aufgerufen wird</param>
    Public Shared Function StartGDTFileWatcher(watchFolder As String, filePattern As String, callback As Action(Of GDTPatient)) As FileSystemWatcher
        If Not Directory.Exists(watchFolder) Then
            Throw New DirectoryNotFoundException($"Überwachungsordner nicht gefunden: {watchFolder}")
        End If

        Dim watcher As New FileSystemWatcher()
        watcher.Path = watchFolder
        watcher.Filter = filePattern
        watcher.NotifyFilter = NotifyFilters.CreationTime Or NotifyFilters.LastWrite
        watcher.EnableRaisingEvents = True

        AddHandler watcher.Created, Sub(sender, e)
                                        Try
                                            ' Kurz warten, falls Datei noch geschrieben wird
                                            Threading.Thread.Sleep(100)

                                            Dim patient As GDTPatient = ParseGDTFile(e.FullPath)
                                            callback(patient)

                                            ' Optional: Datei nach Verarbeitung löschen oder verschieben
                                            ' File.Delete(e.FullPath)

                                        Catch ex As Exception
                                            ' Fehler beim Parsen - könnte in Log geschrieben werden
                                            Logger.Debug($"Fehler beim Verarbeiten der GDT-Datei {e.FullPath}: {ex.Message}")
                                        End Try
                                    End Sub

        Return watcher
    End Function

    ''' <summary>
    ''' Erstellt eine Test-GDT-Datei für Entwicklungszwecke
    ''' </summary>
    Public Shared Sub CreateTestGDTFile(filePath As String, patient As GDTPatient)
        Dim sb As New StringBuilder()

        ' Berechne Gesamtlänge (vereinfacht)
        sb.AppendLine("013800065000")  ' Satzart 8000 mit geschätzter Länge

        If Not String.IsNullOrEmpty(patient.PatientenNummer) Then
            sb.AppendLine($"{(7 + patient.PatientenNummer.Length).ToString("000")}{PATIENTENNUMMER}{patient.PatientenNummer}")
        End If

        sb.AppendLine($"014{GDT_VERSION}01.00")  ' GDT Version

        If Not String.IsNullOrEmpty(patient.PatientenKennung) Then
            sb.AppendLine($"{(7 + patient.PatientenKennung.Length).ToString("000")}{PATIENTENKENNUNG}{patient.PatientenKennung}")
        End If

        If Not String.IsNullOrEmpty(patient.Nachname) Then
            sb.AppendLine($"{(7 + patient.Nachname.Length).ToString("000")}{NACHNAME}{patient.Nachname}")
        End If

        If Not String.IsNullOrEmpty(patient.Vorname) Then
            sb.AppendLine($"{(7 + patient.Vorname.Length).ToString("000")}{VORNAME}{patient.Vorname}")
        End If

        If Not String.IsNullOrEmpty(patient.Geburtsdatum) Then
            sb.AppendLine($"{(7 + patient.Geburtsdatum.Length).ToString("000")}{GEBURTSDATUM}{patient.Geburtsdatum}")
        End If

        If Not String.IsNullOrEmpty(patient.Geschlecht) Then
            sb.AppendLine($"{(7 + patient.Geschlecht.Length).ToString("000")}{GESCHLECHT}{patient.Geschlecht}")
        End If

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8)
    End Sub


End Class

Public Module GDTUmlautConverter

    ''' <summary>
    ''' Konvertiert zwischen GDT-Codepage (CP437/850) und Windows-Codepage (1252)
    ''' </summary>
    ''' <param name="text">Zu konvertierender Text</param>
    ''' <param name="toGDT">True = Windows zu GDT, False = GDT zu Windows</param>
    Public Function ConvertUmlaute(text As String, Optional toGDT As Boolean = False) As String
        If String.IsNullOrEmpty(text) Then Return text

        Dim result As New StringBuilder(text.Length)

        For Each c As Char In text
            Dim charCode = AscW(c)

            If toGDT Then
                ' Windows (1252) zu GDT (CP437/850)
                Select Case charCode
                    Case 228 : result.Append(ChrW(132))  ' ä -> „
                    Case 196 : result.Append(ChrW(142))  ' Ä -> Ž
                    Case 246 : result.Append(ChrW(148))  ' ö -> "
                    Case 214 : result.Append(ChrW(153))  ' Ö -> ™
                    Case 252 : result.Append(ChrW(129))  ' ü -> 
                    Case 220 : result.Append(ChrW(154))  ' Ü -> š
                    Case 223 : result.Append(ChrW(225))  ' ß -> á
                    Case Else : result.Append(c)
                End Select
            Else
                ' GDT (CP437/850) zu Windows (1252)
                Select Case charCode
                    Case 132 : result.Append("ä")  ' „ -> ä
                    Case 142 : result.Append("Ä")  ' Ž -> Ä
                    Case 148 : result.Append("ö")  ' " -> ö
                    Case 153 : result.Append("Ö")  ' ™ -> Ö
                    Case 129 : result.Append("ü")  '  -> ü
                    Case 154 : result.Append("Ü")  ' š -> Ü
                    Case 225 : result.Append("ß")  ' á -> ß
                    Case Else : result.Append(c)
                End Select
            End If
        Next

        Return result.ToString()
    End Function

    ''' <summary>
    ''' Alternative: Nutzt .NET Encoding-Klassen für korrekte Konvertierung
    ''' </summary>
    Public Function ConvertUmlauteEncoding(text As String, fromGDT As Boolean) As String
        Try
            If fromGDT Then
                ' Von CP850 (GDT) nach UTF-8/Windows
                Dim bytes = Encoding.GetEncoding(850).GetBytes(text)
                Return Encoding.GetEncoding(1252).GetString(bytes)
            Else
                ' Von Windows nach CP850 (GDT)
                Dim bytes = Encoding.GetEncoding(1252).GetBytes(text)
                Return Encoding.GetEncoding(850).GetString(bytes)
            End If
        Catch ex As Exception
            ' Fallback zur manuellen Konvertierung
            Return ConvertUmlaute(text, Not fromGDT)
        End Try
    End Function






End Module