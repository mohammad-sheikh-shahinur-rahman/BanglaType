'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: open-source Avro Phonetic engine.
'   This is a from-scratch VB.NET port of the OmicronLab Avro Phonetic
'   transliteration algorithm. It does NOT use the closed libcpphonetic.dll.
'

''' <summary>One context condition on a pattern (prefix/suffix + scope).</summary>
Public Class AvroMatch
    Public Type As String     ' "prefix" or "suffix"
    Public Scope As String    ' "punctuation" | "vowel" | "consonant" | "exact", optionally "!"-prefixed
    Public Value As String    ' only used for the "exact" scope
End Class

''' <summary>A conditional replacement variant for a pattern.</summary>
Public Class AvroRule
    Public Matches As List(Of AvroMatch)
    Public Replace As String
End Class

''' <summary>A find/replace entry with optional context rules.</summary>
Public Class AvroPattern
    Public Find As String
    Public Replace As String
    Public Rules As List(Of AvroRule)
End Class

''' <summary>
''' Rule-based Avro Phonetic transliteration. Exposes Parse(input) so it can be
''' dropped into the same code path as the closed-DLL phonetic parser.
''' </summary>
Public Class AvroParser

    Private ReadOnly _patterns As List(Of AvroPattern)
    Private ReadOnly _maxLen As Integer
    Private Const Vowels As String = "aeiou"
    Private Const Consonants As String = "bcdfghjklmnpqrstvwxyz"
    Private Const CaseSensitive As String = "oiudgjnrstyz"

    Public Sub New()
        _patterns = AvroRules.BuildPatterns()
        Dim m As Integer = 0
        For Each p As AvroPattern In _patterns
            If p.Find IsNot Nothing AndAlso p.Find.Length > m Then m = p.Find.Length
        Next
        _maxLen = m
    End Sub

    ''' <summary>Transliterates a roman-input string to Bangla.</summary>
    Public Function Parse(ByVal input As String) As String
        If String.IsNullOrEmpty(input) Then Return ""
        Dim fixedStr As String = FixString(input)
        Dim output As New System.Text.StringBuilder()
        Dim len As Integer = fixedStr.Length
        Dim cur As Integer = 0

        While cur < len
            Dim start As Integer = cur
            Dim matched As Boolean = False

            Dim tryLen As Integer = _maxLen
            While tryLen > 0
                Dim [end] As Integer = cur + tryLen
                If [end] <= len Then
                    Dim chunk As String = fixedStr.Substring(cur, tryLen)
                    Dim pat As AvroPattern = FindPattern(chunk)
                    If pat IsNot Nothing Then
                        If pat.Rules IsNot Nothing AndAlso pat.Rules.Count > 0 Then
                            For Each rule As AvroRule In pat.Rules
                                If RuleMatches(rule, fixedStr, start, [end]) Then
                                    output.Append(rule.Replace)
                                    cur = [end] - 1
                                    matched = True
                                    Exit For
                                End If
                            Next
                        End If
                        If Not matched Then
                            output.Append(pat.Replace)
                            cur = [end] - 1
                            matched = True
                        End If
                        Exit While
                    End If
                End If
                tryLen -= 1
            End While

            If Not matched Then
                output.Append(fixedStr(cur))
            End If
            cur += 1
        End While

        Return output.ToString()
    End Function

    ' --- helpers ---------------------------------------------------------

    Private Function FindPattern(ByVal chunk As String) As AvroPattern
        ' Patterns are pre-sorted by descending Find length in AvroRules, but here
        ' we match an exact chunk, so a linear scan keyed by Find is fine for the size.
        For Each p As AvroPattern In _patterns
            If p.Find = chunk Then Return p
        Next
        Return Nothing
    End Function

    Private Function RuleMatches(ByVal rule As AvroRule, ByVal s As String, ByVal start As Integer, ByVal [end] As Integer) As Boolean
        For Each mt As AvroMatch In rule.Matches
            Dim scope As String = mt.Scope
            Dim negate As Boolean = False
            If scope.Length > 0 AndAlso scope(0) = "!"c Then
                negate = True
                scope = scope.Substring(1)
            End If

            Dim chk As Integer
            If mt.Type = "suffix" Then
                chk = [end]
            Else
                chk = start - 1
            End If

            Dim cond As Boolean
            Select Case scope
                Case "punctuation"
                    cond = (chk < 0 AndAlso mt.Type = "prefix") _
                        OrElse (chk >= s.Length AndAlso mt.Type = "suffix") _
                        OrElse (chk >= 0 AndAlso chk < s.Length AndAlso IsPunctuation(s(chk)))
                    If negate Then cond = Not cond
                Case "vowel"
                    cond = ((chk >= 0 AndAlso mt.Type = "prefix") OrElse (chk < s.Length AndAlso mt.Type = "suffix")) _
                        AndAlso chk >= 0 AndAlso chk < s.Length AndAlso IsVowel(s(chk))
                    If negate Then cond = Not cond
                Case "consonant"
                    cond = ((chk >= 0 AndAlso mt.Type = "prefix") OrElse (chk < s.Length AndAlso mt.Type = "suffix")) _
                        AndAlso chk >= 0 AndAlso chk < s.Length AndAlso IsConsonant(s(chk))
                    If negate Then cond = Not cond
                Case "exact"
                    Dim es As Integer, ee As Integer
                    If mt.Type = "suffix" Then
                        es = [end]
                        ee = [end] + mt.Value.Length
                    Else
                        es = start - mt.Value.Length
                        ee = start
                    End If
                    cond = IsExact(mt.Value, s, es, ee, negate)
                Case Else
                    cond = True
            End Select

            If Not cond Then Return False
        Next
        Return True
    End Function

    Private Shared Function IsExact(ByVal needle As String, ByVal hay As String, ByVal start As Integer, ByVal [end] As Integer, ByVal negate As Boolean) As Boolean
        Dim ok As Boolean = (start >= 0 AndAlso [end] <= hay.Length AndAlso hay.Substring(start, [end] - start) = needle)
        Return ok <> negate
    End Function

    Private Shared Function FixString(ByVal input As String) As String
        Dim sb As New System.Text.StringBuilder(input.Length)
        For Each c As Char In input
            If CaseSensitive.IndexOf(Char.ToLowerInvariant(c)) >= 0 Then
                sb.Append(c)
            Else
                sb.Append(Char.ToLowerInvariant(c))
            End If
        Next
        Return sb.ToString()
    End Function

    Private Shared Function IsVowel(ByVal c As Char) As Boolean
        Return Vowels.IndexOf(Char.ToLowerInvariant(c)) >= 0
    End Function

    Private Shared Function IsConsonant(ByVal c As Char) As Boolean
        Return Consonants.IndexOf(Char.ToLowerInvariant(c)) >= 0
    End Function

    Private Shared Function IsPunctuation(ByVal c As Char) As Boolean
        Return Not IsVowel(c) AndAlso Not IsConsonant(c)
    End Function

End Class
