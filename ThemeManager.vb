'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: theme loading + application.
'

Imports System.Drawing
Imports System.IO
Imports System.Xml.Linq

''' <summary>
''' Loads themes from data\themes\*.theme (XML) and applies a theme to the live UI.
''' Built-in themes are written on first run, mirroring the keyboard-layout pattern.
''' </summary>
Module ThemeManager

    Private Function ThemesFolder() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "themes")
    End Function

    ''' <summary>Writes built-in theme files on first run.</summary>
    Public Sub EnsureBuiltInThemes()
        Try
            Directory.CreateDirectory(ThemesFolder())
            WriteIfMissing("BanglaType Default", "#F2F2F4", "#E6E6E6", "#141414", "#00B489", "#DE4B39", "#FAFAFA", "#141414", "#C9DEF5", "#0A0A0A")
            WriteIfMissing("BanglaType Air", "#E8F0FE", "#D2E3FC", "#174EA6", "#1967D2", "#D93025", "#E8F0FE", "#174EA6", "#D2E3FC", "#174EA6")
            WriteIfMissing("BanglaType Blue", "#E3F2FD", "#BBDEFB", "#0D47A1", "#1565C0", "#D32F2F", "#E3F2FD", "#0D47A1", "#BBDEFB", "#0D47A1")
            WriteIfMissing("BanglaType Classic", "#37474F", "#455A64", "#ECEFF1", "#26A69A", "#EF5350", "#37474F", "#ECEFF1", "#546E7A", "#FFFFFF")
            WriteIfMissing("BanglaType Cream", "#FDF6F0", "#F5E8DD", "#5F4B3C", "#D08C60", "#C2593F", "#FDF6F0", "#5F4B3C", "#F5E8DD", "#5F4B3C")
            WriteIfMissing("BanglaType Glass", "#E0F7FA", "#B2EBF2", "#006064", "#00838F", "#C62828", "#E0F7FA", "#006064", "#B2EBF2", "#006064")
            WriteIfMissing("BanglaType Light", "#FFFFFF", "#F0F0F0", "#202020", "#00B489", "#DE4B39", "#FFFFFF", "#202020", "#F0F0F0", "#202020")
            WriteIfMissing("BanglaType Mist", "#ECEFF1", "#CFD8DC", "#263238", "#37474F", "#C62828", "#ECEFF1", "#263238", "#CFD8DC", "#263238")
            WriteIfMissing("BanglaType Pearl", "#FDF6E3", "#EEE8D5", "#073642", "#859900", "#DC322F", "#FDF6E3", "#073642", "#EEE8D5", "#073642")
            WriteIfMissing("BanglaType Silk", "#F3E5F5", "#E1BEE7", "#4A148C", "#6A1B9A", "#C2185B", "#F3E5F5", "#4A148C", "#E1BEE7", "#4A148C")
        Catch
        End Try
    End Sub

    Private Sub WriteIfMissing(name As String, back As String, border As String, btnFore As String,
                               onc As String, offc As String, sBack As String, sFore As String, sSelBack As String, sSelFore As String)
        Dim p As String = Path.Combine(ThemesFolder(), name & ".theme")
        If File.Exists(p) Then Return
        Dim doc As New XDocument(
            New XElement("Theme",
                New XElement("Name", name),
                New XElement("TopbarBack", back),
                New XElement("TopbarBorder", border),
                New XElement("ButtonFore", btnFore),
                New XElement("OnColor", onc),
                New XElement("OffColor", offc),
                New XElement("SuggestBack", sBack),
                New XElement("SuggestFore", sFore),
                New XElement("SuggestSelBack", sSelBack),
                New XElement("SuggestSelFore", sSelFore)))
        doc.Save(p)
    End Sub

    ''' <summary>Returns all available theme names (built-ins first).</summary>
    Public Function ListThemeNames() As List(Of String)
        EnsureBuiltInThemes()
        Dim names As New List(Of String)()
        Try
            For Each f As String In Directory.GetFiles(ThemesFolder(), "*.theme")
                names.Add(Path.GetFileNameWithoutExtension(f))
            Next
        Catch
        End Try
        If names.Count = 0 Then names.Add("Light")
        Return names
    End Function

    ''' <summary>Loads a theme by name; falls back to a default Light theme if missing/corrupt.</summary>
    Public Function LoadTheme(ByVal name As String) As Theme
        EnsureBuiltInThemes()
        Dim t As New Theme With {.Name = name}
        Try
            Dim p As String = Path.Combine(ThemesFolder(), name & ".theme")
            If File.Exists(p) Then
                Dim root As XElement = XDocument.Load(p).Root
                t.Name = ValOr(root, "Name", name)
                t.TopbarBack = ParseColor(ValOr(root, "TopbarBack", ""), t.TopbarBack)
                t.TopbarBorder = ParseColor(ValOr(root, "TopbarBorder", ""), t.TopbarBorder)
                t.ButtonFore = ParseColor(ValOr(root, "ButtonFore", ""), t.ButtonFore)
                t.OnColor = ParseColor(ValOr(root, "OnColor", ""), t.OnColor)
                t.OffColor = ParseColor(ValOr(root, "OffColor", ""), t.OffColor)
                t.SuggestBack = ParseColor(ValOr(root, "SuggestBack", ""), t.SuggestBack)
                t.SuggestFore = ParseColor(ValOr(root, "SuggestFore", ""), t.SuggestFore)
                t.SuggestSelBack = ParseColor(ValOr(root, "SuggestSelBack", ""), t.SuggestSelBack)
                t.SuggestSelFore = ParseColor(ValOr(root, "SuggestSelFore", ""), t.SuggestSelFore)
            End If
        Catch
        End Try
        Return t
    End Function

    ''' <summary>Applies a theme to the topbar form, the on/off indicator, and the suggestion popup.</summary>
    Public Sub Apply(ByVal form As MainUI, ByVal t As Theme, ByVal sugg As SuggestionWindow)
        If form Is Nothing OrElse t Is Nothing Then Return

        ' Expose colors that other code paths read (border paint + on/off toggling).
        form.ThemeTopbarBack = t.TopbarBack
        form.ThemeBorderColor = t.TopbarBorder
        form.OnColorTheme = t.OnColor
        form.OffColorTheme = t.OffColor
        form.currentButtonFore = t.ButtonFore

        form.BackColor = t.TopbarBack
        ApplyButton(form.btnMode, t.TopbarBack, t.ButtonFore)
        ApplyButton(form.btnSettings, t.TopbarBack, t.ButtonFore)
        ApplyButton(form.btnVoice, t.TopbarBack, t.ButtonFore)
        ApplyButton(form.buttonClose, t.TopbarBack, t.ButtonFore)

        If sugg IsNot Nothing Then
            sugg.ApplyTheme(t.SuggestBack, t.SuggestFore, t.SuggestSelBack, t.SuggestSelFore, t.TopbarBorder)
        End If

        form.Invalidate()
    End Sub

    Private Sub ApplyButton(ByVal b As Button, ByVal back As Color, ByVal fore As Color)
        If b Is Nothing Then Return
        b.ForeColor = fore
        ' Keep transparent-styled buttons blending with the topbar.
        b.BackColor = Color.Transparent
    End Sub

    Private Function ValOr(root As XElement, name As String, fallback As String) As String
        If root Is Nothing Then Return fallback
        Dim e As XElement = root.Element(name)
        If e Is Nothing OrElse String.IsNullOrWhiteSpace(e.Value) Then Return fallback
        Return e.Value.Trim()
    End Function

    Private Function ParseColor(ByVal hex As String, ByVal fallback As Color) As Color
        Try
            If String.IsNullOrWhiteSpace(hex) Then Return fallback
            hex = hex.TrimStart("#"c)
            If hex.Length = 6 Then
                Dim r As Integer = Convert.ToInt32(hex.Substring(0, 2), 16)
                Dim g As Integer = Convert.ToInt32(hex.Substring(2, 2), 16)
                Dim b As Integer = Convert.ToInt32(hex.Substring(4, 2), 16)
                Return Color.FromArgb(r, g, b)
            End If
        Catch
        End Try
        Return fallback
    End Function

End Module
