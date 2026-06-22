'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: built-in themes + application.
'

Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Owns the set of built-in themes and applies a chosen theme to the topbar
''' window and the suggestion popup. Themes are defined in code (no disk files),
''' so EnsureBuiltInThemes() simply guarantees the table is populated.
''' </summary>
Public Module ThemeManager

    Private ReadOnly _themes As New List(Of Theme)()

    ''' <summary>Populates the built-in theme table on first use.</summary>
    Public Sub EnsureBuiltInThemes()
        If _themes.Count > 0 Then Return

        ' Default warm cream scheme (matches AppSettings.ThemeName default).
        _themes.Add(New Theme("BanglaType Cream") With {
            .TopbarBack = Color.FromArgb(248, 244, 236),
            .BorderColor = Color.FromArgb(225, 215, 198),
            .ButtonFore = Color.FromArgb(96, 84, 66),
            .OnColor = Color.FromArgb(0, 180, 137),
            .OffColor = Color.FromArgb(222, 75, 57),
            .SuggestBack = Color.FromArgb(40, 36, 30),
            .SuggestFore = Color.FromArgb(245, 240, 230),
            .SuggestSelectBack = Color.FromArgb(0, 180, 137),
            .SuggestSelectFore = Color.White})

        _themes.Add(New Theme("Light") With {
            .TopbarBack = Color.FromArgb(242, 242, 244),
            .BorderColor = Color.FromArgb(230, 230, 230),
            .ButtonFore = Color.FromArgb(140, 140, 140),
            .OnColor = Color.FromArgb(0, 180, 137),
            .OffColor = Color.FromArgb(222, 75, 57),
            .SuggestBack = Color.FromArgb(250, 250, 252),
            .SuggestFore = Color.FromArgb(30, 30, 30),
            .SuggestSelectBack = Color.FromArgb(0, 180, 137),
            .SuggestSelectFore = Color.White})

        _themes.Add(New Theme("Dark") With {
            .TopbarBack = Color.FromArgb(28, 28, 30),
            .BorderColor = Color.FromArgb(60, 60, 64),
            .ButtonFore = Color.FromArgb(210, 210, 214),
            .OnColor = Color.FromArgb(0, 200, 150),
            .OffColor = Color.FromArgb(240, 90, 72),
            .SuggestBack = Color.FromArgb(20, 20, 22),
            .SuggestFore = Color.White,
            .SuggestSelectBack = Color.FromArgb(0, 180, 137),
            .SuggestSelectFore = Color.White})

        _themes.Add(New Theme("Ocean") With {
            .TopbarBack = Color.FromArgb(232, 240, 247),
            .BorderColor = Color.FromArgb(190, 210, 228),
            .ButtonFore = Color.FromArgb(40, 78, 110),
            .OnColor = Color.FromArgb(0, 150, 160),
            .OffColor = Color.FromArgb(232, 110, 70),
            .SuggestBack = Color.FromArgb(24, 42, 58),
            .SuggestFore = Color.FromArgb(225, 238, 248),
            .SuggestSelectBack = Color.FromArgb(0, 150, 160),
            .SuggestSelectFore = Color.White})

        _themes.Add(New Theme("Midnight Purple") With {
            .TopbarBack = Color.FromArgb(33, 28, 44),
            .BorderColor = Color.FromArgb(64, 54, 84),
            .ButtonFore = Color.FromArgb(222, 214, 236),
            .OnColor = Color.FromArgb(120, 200, 160),
            .OffColor = Color.FromArgb(236, 110, 120),
            .SuggestBack = Color.FromArgb(26, 22, 36),
            .SuggestFore = Color.White,
            .SuggestSelectBack = Color.FromArgb(140, 110, 220),
            .SuggestSelectFore = Color.White})

        ' Premium Cyberpunk Neon
        _themes.Add(New Theme("Neon Cyberpunk") With {
            .TopbarBack = Color.FromArgb(17, 17, 24),
            .BorderColor = Color.FromArgb(45, 45, 68),
            .ButtonFore = Color.FromArgb(103, 233, 244),
            .OnColor = Color.FromArgb(255, 0, 127),
            .OffColor = Color.FromArgb(255, 80, 0),
            .SuggestBack = Color.FromArgb(12, 12, 18),
            .SuggestFore = Color.FromArgb(210, 200, 240),
            .SuggestSelectBack = Color.FromArgb(255, 0, 127),
            .SuggestSelectFore = Color.White})

        ' Premium Amoled Pitch Black
        _themes.Add(New Theme("Amoled Pitch Black") With {
            .TopbarBack = Color.FromArgb(0, 0, 0),
            .BorderColor = Color.FromArgb(30, 30, 30),
            .ButtonFore = Color.FromArgb(220, 220, 220),
            .OnColor = Color.FromArgb(57, 255, 20),
            .OffColor = Color.FromArgb(255, 36, 0),
            .SuggestBack = Color.FromArgb(0, 0, 0),
            .SuggestFore = Color.FromArgb(235, 235, 235),
            .SuggestSelectBack = Color.FromArgb(0, 201, 87),
            .SuggestSelectFore = Color.Black})

        ' Premium Synthwave Dracula
        _themes.Add(New Theme("Synthwave Dracula") With {
            .TopbarBack = Color.FromArgb(40, 42, 54),
            .BorderColor = Color.FromArgb(68, 71, 90),
            .ButtonFore = Color.FromArgb(139, 233, 253),
            .OnColor = Color.FromArgb(189, 147, 249),
            .OffColor = Color.FromArgb(255, 121, 198),
            .SuggestBack = Color.FromArgb(24, 25, 32),
            .SuggestFore = Color.FromArgb(248, 248, 242),
            .SuggestSelectBack = Color.FromArgb(80, 250, 123),
            .SuggestSelectFore = Color.Black})

        ' Premium Sakura Blossom
        _themes.Add(New Theme("Sakura Blossom") With {
            .TopbarBack = Color.FromArgb(255, 240, 245),
            .BorderColor = Color.FromArgb(244, 194, 194),
            .ButtonFore = Color.FromArgb(188, 74, 93),
            .OnColor = Color.FromArgb(224, 60, 113),
            .OffColor = Color.FromArgb(255, 140, 105),
            .SuggestBack = Color.FromArgb(255, 250, 250),
            .SuggestFore = Color.FromArgb(50, 50, 60),
            .SuggestSelectBack = Color.FromArgb(255, 182, 193),
            .SuggestSelectFore = Color.FromArgb(50, 50, 60)})

        ' Premium Forest Moss
        _themes.Add(New Theme("Forest Moss") With {
            .TopbarBack = Color.FromArgb(26, 36, 33),
            .BorderColor = Color.FromArgb(46, 61, 56),
            .ButtonFore = Color.FromArgb(163, 190, 140),
            .OnColor = Color.FromArgb(235, 203, 139),
            .OffColor = Color.FromArgb(191, 97, 106),
            .SuggestBack = Color.FromArgb(18, 26, 23),
            .SuggestFore = Color.FromArgb(229, 233, 240),
            .SuggestSelectBack = Color.FromArgb(143, 188, 187),
            .SuggestSelectFore = Color.Black})

        ' Premium Nordic Frost
        _themes.Add(New Theme("Nordic Frost") With {
            .TopbarBack = Color.FromArgb(236, 239, 244),
            .BorderColor = Color.FromArgb(216, 222, 233),
            .ButtonFore = Color.FromArgb(76, 86, 106),
            .OnColor = Color.FromArgb(136, 192, 208),
            .OffColor = Color.FromArgb(208, 135, 112),
            .SuggestBack = Color.FromArgb(46, 52, 64),
            .SuggestFore = Color.FromArgb(236, 239, 244),
            .SuggestSelectBack = Color.FromArgb(136, 192, 208),
            .SuggestSelectFore = Color.FromArgb(46, 52, 64)})

        ' Premium Solarized Light
        _themes.Add(New Theme("Solarized Light") With {
            .TopbarBack = Color.FromArgb(253, 246, 227),
            .BorderColor = Color.FromArgb(238, 232, 213),
            .ButtonFore = Color.FromArgb(88, 110, 117),
            .OnColor = Color.FromArgb(133, 153, 0),
            .OffColor = Color.FromArgb(203, 75, 22),
            .SuggestBack = Color.FromArgb(7, 54, 66),
            .SuggestFore = Color.FromArgb(253, 246, 227),
            .SuggestSelectBack = Color.FromArgb(38, 139, 210),
            .SuggestSelectFore = Color.White})

        ' Premium Solarized Dark
        _themes.Add(New Theme("Solarized Dark") With {
            .TopbarBack = Color.FromArgb(7, 54, 66),
            .BorderColor = Color.FromArgb(0, 43, 54),
            .ButtonFore = Color.FromArgb(147, 161, 161),
            .OnColor = Color.FromArgb(133, 153, 0),
            .OffColor = Color.FromArgb(211, 54, 130),
            .SuggestBack = Color.FromArgb(0, 43, 54),
            .SuggestFore = Color.FromArgb(253, 246, 227),
            .SuggestSelectBack = Color.FromArgb(38, 139, 210),
            .SuggestSelectFore = Color.White})

        ' Premium Monokai Pro
        _themes.Add(New Theme("Monokai Pro") With {
            .TopbarBack = Color.FromArgb(45, 42, 46),
            .BorderColor = Color.FromArgb(64, 61, 65),
            .ButtonFore = Color.FromArgb(252, 250, 242),
            .OnColor = Color.FromArgb(169, 220, 118),
            .OffColor = Color.FromArgb(255, 97, 136),
            .SuggestBack = Color.FromArgb(34, 31, 34),
            .SuggestFore = Color.FromArgb(252, 250, 242),
            .SuggestSelectBack = Color.FromArgb(255, 216, 117),
            .SuggestSelectFore = Color.Black})

        ' Premium Retro Terminal
        _themes.Add(New Theme("Retro Terminal") With {
            .TopbarBack = Color.FromArgb(10, 10, 10),
            .BorderColor = Color.FromArgb(0, 60, 0),
            .ButtonFore = Color.FromArgb(0, 255, 0),
            .OnColor = Color.FromArgb(0, 255, 0),
            .OffColor = Color.FromArgb(255, 170, 0),
            .SuggestBack = Color.FromArgb(10, 10, 10),
            .SuggestFore = Color.FromArgb(0, 255, 0),
            .SuggestSelectBack = Color.FromArgb(0, 100, 0),
            .SuggestSelectFore = Color.White})
    End Sub

    ''' <summary>Names of all built-in themes, in display order.</summary>
    Public Function ListThemeNames() As IEnumerable(Of String)
        EnsureBuiltInThemes()
        Dim names As New List(Of String)()
        For Each t As Theme In _themes
            names.Add(t.Name)
        Next
        Return names
    End Function

    ''' <summary>Returns the theme with the given name, or the default theme if unknown.</summary>
    Public Function LoadTheme(ByVal name As String) As Theme
        EnsureBuiltInThemes()
        If Not String.IsNullOrEmpty(name) Then
            For Each t As Theme In _themes
                If String.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase) Then Return t
            Next
        End If
        Return _themes(0)
    End Function

    ''' <summary>Applies a theme to the topbar window and (if present) the suggestion popup.</summary>
    Public Sub Apply(ByVal ui As MainUI, ByVal t As Theme, ByVal sugg As SuggestionWindow)
        If t Is Nothing Then Return

        If ui IsNot Nothing Then
            ui.ThemeTopbarBack = t.TopbarBack
            ui.ThemeBorderColor = t.BorderColor
            ui.currentButtonFore = t.ButtonFore
            ui.OnColorTheme = t.OnColor
            ui.OffColorTheme = t.OffColor

            Try
                ApplyButtonFore(ui, t.ButtonFore)
                ui.BackColor = t.TopbarBack
                ui.Invalidate(True)
            Catch
            End Try
        End If

        If sugg IsNot Nothing Then
            Try
                sugg.ApplyTheme(t)
            Catch
            End Try
        End If
    End Sub

    Private Sub ApplyButtonFore(ByVal parent As Control, ByVal fore As Color)
        For Each c As Control In parent.Controls
            If TypeOf c Is Button Then
                c.ForeColor = fore
            End If
            If c.HasChildren Then ApplyButtonFore(c, fore)
        Next
    End Sub

End Module
