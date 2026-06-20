Imports System.IO
Imports System.Text
Imports System.Collections.Generic

Public Module AnalyticsEngine
    Public TotalKeys As Integer = 0
    Public TotalBackspaces As Integer = 0
    Public WordCounts As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

    ' For live WPM tracking
    Private currentSessionWords As Integer = 0
    Private sessionStartTime As DateTime = DateTime.MinValue
    Private lastKeyTime As DateTime = DateTime.MinValue

    Private Function StatsPath() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "analytics.json")
    End Function

    Public Sub Load()
        Try
            Dim path As String = StatsPath()
            If File.Exists(path) Then
                Dim json As String = File.ReadAllText(path, Encoding.UTF8)
                ' Simple JSON parser since we are in standard .NET v4.8 without external libraries
                TotalKeys = ParseJsonInt(json, "TotalKeys")
                TotalBackspaces = ParseJsonInt(json, "TotalBackspaces")
                
                Dim wordsPart As String = ExtractJsonValue(json, "WordCounts")
                If Not String.IsNullOrEmpty(wordsPart) Then
                    WordCounts.Clear()
                    ' Parse word dictionary e.g. "word1":12,"word2":4
                    Dim pairs() As String = wordsPart.Split(","c)
                    For Each p In pairs
                        Dim kv() As String = p.Split(":"c)
                        If kv.Length = 2 Then
                            Dim k As String = kv(0).Trim(""""c, "{"c, "}"c, " "c)
                            Dim v As Integer = 0
                            If Integer.TryParse(kv(1).Trim("}"c, " "c), v) Then
                                WordCounts(k) = v
                            End If
                        End If
                    Next
                End If
            End If
        Catch
        End Try
    End Sub

    Public Sub Save()
        Try
            Dim folder As String = Path.GetDirectoryName(StatsPath())
            Directory.CreateDirectory(folder)

            Dim sb As New StringBuilder()
            sb.AppendLine("{")
            sb.AppendLine("  ""TotalKeys"": " & TotalKeys & ",")
            sb.AppendLine("  ""TotalBackspaces"": " & TotalBackspaces & ",")
            sb.AppendLine("  ""WordCounts"": {")
            
            Dim list As New List(Of String)()
            For Each kvp In WordCounts
                list.Add("    """ & kvp.Key.Replace("""", "\""") & """: " & kvp.Value)
            Next
            sb.AppendLine(String.Join("," & vbCrLf, list.ToArray()))
            sb.AppendLine("  }")
            sb.AppendLine("}")

            File.WriteAllText(StatsPath(), sb.ToString(), Encoding.UTF8)
        Catch
        End Try
    End Sub

    Public Sub RecordKey(ByVal vkCode As Integer)
        TotalKeys += 1
        If vkCode = 8 Then
            TotalBackspaces += 1
        End If

        ' WPM logic
        Dim now As DateTime = DateTime.Now
        If sessionStartTime = DateTime.MinValue OrElse (now - lastKeyTime).TotalSeconds > 10 Then
            sessionStartTime = now
            currentSessionWords = 0
        End If
        lastKeyTime = now
    End Sub

    Public Sub RecordWord(ByVal word As String)
        word = word.Trim().Trim("."c, ","c, "?"c, "!"c, "।"c, ";"c)
        If word.Length < 2 Then Return

        Dim cnt As Integer = 0
        WordCounts.TryGetValue(word, cnt)
        WordCounts(word) = cnt + 1
        currentSessionWords += 1
    End Sub

    Public Function GetWPM() As Integer
        If sessionStartTime = DateTime.MinValue Then Return 0
        Dim secs As Double = (DateTime.Now - sessionStartTime).TotalSeconds
        If secs < 2 Then Return 0
        Dim min As Double = secs / 60.0
        Return CInt(Math.Round(currentSessionWords / min))
    End Function

    Public Function GetAccuracy() As Double
        If TotalKeys = 0 Then Return 100.0
        Dim acc As Double = (1.0 - (CDbl(TotalBackspaces) / CDbl(TotalKeys))) * 100.0
        Return Math.Max(0.0, Math.Min(100.0, acc))
    End Function

    ' Simple string-extract JSON helpers
    Private Function ParseJsonInt(ByVal json As String, ByVal key As String) As Integer
        Try
            Dim searchKey As String = """" & key & """"
            Dim idx As Integer = json.IndexOf(searchKey)
            If idx >= 0 Then
                Dim start As Integer = json.IndexOf(":", idx) + 1
                Dim [end] As Integer = json.IndexOf(","c, start)
                If [end] < 0 Then [end] = json.IndexOf("}"c, start)
                Dim valStr As String = json.Substring(start, [end] - start).Trim()
                Return Convert.ToInt32(valStr)
            End If
        Catch
        End Try
        Return 0
    End Function

    Private Function ExtractJsonValue(ByVal json As String, ByVal key As String) As String
        Try
            Dim searchKey As String = """" & key & """"
            Dim idx As Integer = json.IndexOf(searchKey)
            If idx >= 0 Then
                Dim start As Integer = json.IndexOf("{", idx)
                Dim [end] As Integer = json.IndexOf("}", start)
                Return json.Substring(start, [end] - start + 1)
            End If
        Catch
        End Try
        Return ""
    End Function
End Module
