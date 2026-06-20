Imports System.IO
Imports System.Text

Public Module MacroEngine
    Private macros As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private loaded As Boolean = False

    Private Function MacroFolder() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType")
    End Function

    Private Function MacroPath() As String
        Return Path.Combine(MacroFolder(), "macros.txt")
    End Function

    Public Sub Load()
        If loaded Then Return
        loaded = True
        Try
            Directory.CreateDirectory(MacroFolder())
            If Not File.Exists(MacroPath()) Then
                Dim defaultMacros As New StringBuilder()
                defaultMacros.AppendLine("# Format: Abbreviation [TAB] Replacement Text")
                defaultMacros.AppendLine("!th" & vbTab & "ধন্যবাদ")
                defaultMacros.AppendLine("!as" & vbTab & "আসসালামু আলাইকুম")
                defaultMacros.AppendLine("!ms" & vbTab & "নমস্কার")
                defaultMacros.AppendLine("!bt" & vbTab & "BanglaType")
                defaultMacros.AppendLine(";ok" & vbTab & "ঠিক আছে 👍")
                defaultMacros.AppendLine(";gm" & vbTab & "Good morning ☀️")
                File.WriteAllText(MacroPath(), defaultMacros.ToString(), Encoding.UTF8)
            End If

            macros.Clear()
            For Each line As String In File.ReadAllLines(MacroPath(), Encoding.UTF8)
                If String.IsNullOrWhiteSpace(line) OrElse line.StartsWith("#") Then Continue For
                Dim parts() As String = line.Split(ControlChars.Tab)
                If parts.Length >= 2 Then
                    Dim key As String = parts(0).Trim()
                    Dim value As String = parts(1).Trim()
                    If key.Length > 0 AndAlso value.Length > 0 Then
                        macros(key) = value
                    End If
                End If
            Next
        Catch
        End Try
    End Sub

    Public Sub AddMacro(ByVal abbreviation As String, ByVal replacement As String)
        Load()
        macros(abbreviation) = replacement
        Try
            Dim sb As New StringBuilder()
            sb.AppendLine("# Format: Abbreviation [TAB] Replacement Text")
            For Each kvp In macros
                sb.AppendLine(kvp.Key & vbTab & kvp.Value)
            Next
            File.WriteAllText(MacroPath(), sb.ToString(), Encoding.UTF8)
        Catch
        End Try
    End Sub

    Public Function IsMacro(ByVal key As String) As Boolean
        Load()
        Return macros.ContainsKey(key)
    End Function

    Public Function GetReplacement(ByVal key As String) As String
        Load()
        Dim value As String = Nothing
        If macros.TryGetValue(key, value) Then
            Return value
        End If
        Return key
    End Function
End Module
