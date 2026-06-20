'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   Borno Lite "Advance" additions: Unicode -> Legacy ANSI (Bijoy / SutonnyMJ)
'   converter for compatibility with older apps (e.g. Adobe Photoshop).
'
'   Two stages:
'     1) Reorder Unicode so its storage order matches the legacy visual order
'        (pre-base vowel signs move before the consonant cluster; reph moves
'        after; the two-part o/ou-kar are split).
'     2) Longest-match map each unit to its SutonnyMJ ANSI code.
'
'   The base glyph map below targets the SutonnyMJ code page. Conjunct coverage
'   is intentionally a starter set; extend CONJUNCTS / load overrides from
'   data\encoding\unicode_to_bijoy.txt to reach full fidelity for your font.
'

Imports System.IO
Imports System.Text

Module BijoyConverter

    ' Bengali Unicode code points we care about (for reordering logic).
    Private Const HASANTA As Char = ChrW(&H9CD)   ' ্
    Private Const I_KAR As Char = ChrW(&H9BF)     ' ি  (pre-base)
    Private Const E_KAR As Char = ChrW(&H9C7)     ' ে  (pre-base)
    Private Const OI_KAR As Char = ChrW(&H9C8)    ' ৈ  (pre-base)
    Private Const O_KAR As Char = ChrW(&H9CB)     ' ো (= ে + া)
    Private Const OU_KAR As Char = ChrW(&H9CC)    ' ৌ (= ে + ৗ)
    Private Const AA_KAR As Char = ChrW(&H9BE)    ' া
    Private Const OU_TAIL As Char = ChrW(&H9D7)   ' ৗ
    Private Const RA As Char = ChrW(&H9B0)        ' র

    Private overrides_ As Dictionary(Of String, String) = Nothing

    ''' <summary>Converts a Unicode Bangla word to SutonnyMJ ANSI text.</summary>
    Public Function UnicodeToBijoy(ByVal input As String) As String
        If String.IsNullOrEmpty(input) Then Return input
        Dim reordered As String = Reorder(input)
        Return MapToAnsi(reordered)
    End Function

    ' --- stage 1: reordering --------------------------------------------

    Private Function Reorder(ByVal s As String) As String
        ' Split the composite o/ou-kar into their stored parts first.
        s = s.Replace(O_KAR, E_KAR & AA_KAR)
        s = s.Replace(OU_KAR, E_KAR & OU_TAIL)

        Dim sb As New StringBuilder(s.Length + 8)
        Dim i As Integer = 0
        Dim n As Integer = s.Length

        While i < n
            ' Detect a leading reph: র + ্ + (consonant cluster)
            Dim hasReph As Boolean = False
            If i + 1 < n AndAlso s(i) = RA AndAlso s(i + 1) = HASANTA AndAlso (i + 2 < n) AndAlso IsConsonant(s(i + 2)) Then
                hasReph = True
                i += 2 ' skip র ্ for now; re-emit after the cluster
            End If

            ' Parse a consonant cluster: consonant (HASANTA consonant)*
            Dim clusterStart As Integer = i
            Dim clusterEnd As Integer = i
            If i < n AndAlso IsConsonant(s(i)) Then
                clusterEnd = i + 1
                While clusterEnd + 1 < n AndAlso s(clusterEnd) = HASANTA AndAlso IsConsonant(s(clusterEnd + 1))
                    clusterEnd += 2
                End While
            End If

            ' Trailing pre-base vowel sign that visually precedes the cluster.
            Dim preVowel As String = ""
            Dim postVowel As String = ""
            Dim trailing As String = ""
            Dim j As Integer = clusterEnd
            While j < n
                Dim c As Char = s(j)
                If c = I_KAR OrElse c = E_KAR OrElse c = OI_KAR Then
                    preVowel &= c
                    j += 1
                ElseIf IsPostVowelSign(c) Then
                    postVowel &= c
                    j += 1
                ElseIf IsNasalOrSign(c) Then
                    trailing &= c
                    j += 1
                Else
                    Exit While
                End If
            End While

            If clusterEnd > clusterStart Then
                ' Emit: [pre-base vowel] [cluster] [reph] [post vowel] [trailing]
                sb.Append(preVowel)
                sb.Append(s.Substring(clusterStart, clusterEnd - clusterStart))
                If hasReph Then sb.Append(RA & HASANTA)
                sb.Append(postVowel)
                sb.Append(trailing)
                i = j
            Else
                ' Not a consonant-led cluster; emit one char as-is.
                If hasReph Then sb.Append(RA & HASANTA)
                sb.Append(s(i))
                i += 1
            End If
        End While

        Return sb.ToString()
    End Function

    Private Function IsConsonant(ByVal c As Char) As Boolean
        Dim u As Integer = AscW(c)
        Return (u >= &H995 AndAlso u <= &H9B9) OrElse u = &H9DC OrElse u = &H9DD OrElse u = &H9DF OrElse u = &H9CE
    End Function

    Private Function IsPostVowelSign(ByVal c As Char) As Boolean
        Dim u As Integer = AscW(c)
        ' া ী ু ূ ৃ ৄ ৗ
        Return u = &H9BE OrElse u = &H9C0 OrElse u = &H9C1 OrElse u = &H9C2 OrElse u = &H9C3 OrElse u = &H9C4 OrElse u = &H9D7
    End Function

    Private Function IsNasalOrSign(ByVal c As Char) As Boolean
        Dim u As Integer = AscW(c)
        ' ং ঃ ঁ
        Return u = &H982 OrElse u = &H983 OrElse u = &H981
    End Function

    ' --- stage 2: ANSI mapping ------------------------------------------

    Private Function MapToAnsi(ByVal s As String) As String
        EnsureOverrides()
        Dim map As Dictionary(Of String, String) = BaseMap()
        Dim sb As New StringBuilder(s.Length)
        Dim i As Integer = 0
        Dim n As Integer = s.Length

        While i < n
            Dim matched As Boolean = False
            ' Longest match first (up to 5 code units to catch conjuncts).
            Dim maxLen As Integer = Math.Min(6, n - i)
            For len As Integer = maxLen To 1 Step -1
                Dim chunk As String = s.Substring(i, len)
                Dim val As String = Nothing
                If overrides_ IsNot Nothing AndAlso overrides_.TryGetValue(chunk, val) Then
                    sb.Append(val) : i += len : matched = True : Exit For
                ElseIf map.TryGetValue(chunk, val) Then
                    sb.Append(val) : i += len : matched = True : Exit For
                End If
            Next
            If Not matched Then
                sb.Append(s(i)) ' pass through unmapped (ASCII, spaces, etc.)
                i += 1
            End If
        End While

        Return sb.ToString()
    End Function

    ''' <summary>Loads optional user overrides/extensions from data\encoding\unicode_to_bijoy.txt.</summary>
    Private Sub EnsureOverrides()
        If overrides_ IsNot Nothing Then Return
        overrides_ = New Dictionary(Of String, String)(StringComparer.Ordinal)
        Try
            Dim p As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "encoding", "unicode_to_bijoy.txt")
            If File.Exists(p) Then
                For Each line As String In File.ReadAllLines(p, Encoding.UTF8)
                    If String.IsNullOrWhiteSpace(line) OrElse line.StartsWith("#") Then Continue For
                    Dim parts() As String = line.Split(ControlChars.Tab)
                    If parts.Length >= 2 AndAlso parts(0).Length > 0 Then overrides_(parts(0)) = parts(1)
                Next
            End If
        Catch
        End Try
    End Sub

    Private cachedMap As Dictionary(Of String, String) = Nothing

    Private Function BaseMap() As Dictionary(Of String, String)
        If cachedMap IsNot Nothing Then Return cachedMap
        Dim m As New Dictionary(Of String, String)(StringComparer.Ordinal)

        ' Independent vowels
        m("অ") = "A" : m("আ") = "Av" : m("ই") = "B" : m("ঈ") = "C" : m("উ") = "D"
        m("ঊ") = "E" : m("ঋ") = "F" : m("এ") = "G" : m("ঐ") = "H" : m("ও") = "I" : m("ঔ") = "J"

        ' Consonants
        m("ক") = "K" : m("খ") = "L" : m("গ") = "M" : m("ঘ") = "N" : m("ঙ") = "O"
        m("চ") = "P" : m("ছ") = "Q" : m("জ") = "R" : m("ঝ") = "S" : m("ঞ") = "T"
        m("ট") = "U" : m("ঠ") = "V" : m("ড") = "W" : m("ঢ") = "X" : m("ণ") = "Y"
        m("ত") = "Z" : m("থ") = "_" : m("দ") = "`" : m("ধ") = "a" : m("ন") = "b"
        m("প") = "c" : m("ফ") = "d" : m("ব") = "e" : m("ভ") = "f" : m("ম") = "g"
        m("য") = "h" : m("র") = "i" : m("ল") = "j" : m("শ") = "k" : m("ষ") = "l"
        m("স") = "m" : m("হ") = "n" : m("ড়") = "o" : m("ঢ়") = "p" : m("য়") = "q"
        m("ৎ") = "r" : m("ং") = "s" : m("ঃ") = "t" : m("ঁ") = "u"

        ' Vowel signs (kars)
        m("া") = "v" : m("ি") = "w" : m("ী") = "x" : m("ু") = "y" : m("ূ") = "~"
        m("ৃ") = "…" : m("ে") = "‡" : m("ৈ") = "‰" : m("ৗ") = "Š"

        ' Hasanta (linker)
        m(ChrW(&H9CD)) = "&"

        ' Digits
        m("০") = "0" : m("১") = "1" : m("২") = "2" : m("৩") = "3" : m("৪") = "4"
        m("৫") = "5" : m("৬") = "6" : m("৭") = "7" : m("৮") = "8" : m("৯") = "9"

        ' Punctuation
        m("।") = "|"

        ' Common conjuncts (starter set — extend via the override file for full coverage).
        m("ক্ষ") = "ÿ" : m("জ্ঞ") = "Á" : m("ঞ্জ") = "w�" : m("ত্ত") = "Ë"
        m("ন্ত") = "š‘" : m("ন্ধ") = "Ü" : m("ম্ব") = "¤^" : m("স্ত") = "¯Í"
        m("ক্ত") = "³" : m("ক্ক") = "°" : m("ল্ল") = "j­"

        cachedMap = m
        Return m
    End Function

    Private cachedReverseMap As Dictionary(Of String, String) = Nothing

    Private Function ReverseBaseMap() As Dictionary(Of String, String)
        If cachedReverseMap IsNot Nothing Then Return cachedReverseMap
        Dim rm As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Dim bm As Dictionary(Of String, String) = BaseMap()
        For Each kvp In bm
            If Not rm.ContainsKey(kvp.Value) Then
                rm(kvp.Value) = kvp.Key
            End If
        Next
        
        ' Manual overrides/additional mappings for visual compatibility
        rm("A") = "অ" : rm("Av") = "আ" : rm("B") = "ই" : rm("C") = "ঈ" : rm("D") = "উ"
        rm("E") = "ঊ" : rm("F") = "ঋ" : rm("G") = "এ" : rm("H") = "ঐ" : rm("I") = "ও" : rm("J") = "ঔ"
        
        cachedReverseMap = rm
        Return rm
    End Function

    ''' <summary>Converts legacy Bijoy/ANSI text to Standard Unicode.</summary>
    Public Function BijoyToUnicode(ByVal ansi As String) As String
        If String.IsNullOrEmpty(ansi) Then Return ansi
        Dim map As Dictionary(Of String, String) = ReverseBaseMap()
        
        ' Stage 1: Map ANSI visual characters to Unicode counterparts (maintaining visual ordering)
        Dim sb As New StringBuilder(ansi.Length)
        Dim i As Integer = 0
        Dim n As Integer = ansi.Length
        
        While i < n
            Dim matched As Boolean = False
            Dim maxLen As Integer = Math.Min(6, n - i)
            For len As Integer = maxLen To 1 Step -1
                Dim chunk As String = ansi.Substring(i, len)
                Dim val As String = Nothing
                If map.TryGetValue(chunk, val) Then
                    sb.Append(val) : i += len : matched = True : Exit For
                End If
            Next
            If Not matched Then
                sb.Append(ansi(i))
                i += 1
            End If
        End While
        
        Dim temp As String = sb.ToString()
        
        ' Stage 2: Reorder visual-order Unicode back to storage-order Unicode.
        Dim finalSb As New StringBuilder(temp.Length)
        Dim j As Integer = 0
        Dim lenTemp As Integer = temp.Length
        
        While j < lenTemp
            Dim c As Char = temp(j)
            If c = I_KAR OrElse c = E_KAR OrElse c = OI_KAR Then
                Dim vowelSign As Char = c
                j += 1
                
                Dim clusterStart As Integer = j
                Dim clusterEnd As Integer = j
                
                If j < lenTemp AndAlso IsConsonant(temp(j)) Then
                    clusterEnd = j + 1
                    While clusterEnd + 1 < lenTemp AndAlso temp(clusterEnd) = HASANTA AndAlso IsConsonant(temp(clusterEnd + 1))
                        clusterEnd += 2
                    End While
                End If
                
                If clusterEnd > clusterStart Then
                    finalSb.Append(temp.Substring(clusterStart, clusterEnd - clusterStart))
                    finalSb.Append(vowelSign)
                    j = clusterEnd
                Else
                    finalSb.Append(vowelSign)
                End If
            Else
                finalSb.Append(c)
                j += 1
            End If
        End While
        
        Return finalSb.ToString()
    End Function

End Module
