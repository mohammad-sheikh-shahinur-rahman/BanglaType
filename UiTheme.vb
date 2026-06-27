'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: shared dialog styler.
'

Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Shared, theme-aware styling for the secondary dialog windows (About, Converter,
''' etc.). Reads the live colour scheme from the running MainUI topbar so every
''' window matches the user's chosen theme, and gives buttons a flat, modern look
''' with hover feedback. Falls back to a soft cream scheme when no MainUI is open
''' (e.g. a dialog shown during the first-run wizard).
''' </summary>
Public Module UiTheme

    ' --- live theme colours ---------------------------------------------------

    Private Function Main() As MainUI
        For Each f As Form In Application.OpenForms
            Dim m As MainUI = TryCast(f, MainUI)
            If m IsNot Nothing Then Return m
        Next
        Return Nothing
    End Function

    Public Function SurfaceBack() As Color
        Dim m = Main()
        If m IsNot Nothing Then Return m.ThemeTopbarBack
        Return Color.FromArgb(248, 244, 236)
    End Function

    Public Function BorderTone() As Color
        Dim m = Main()
        If m IsNot Nothing Then Return m.ThemeBorderColor
        Return Color.FromArgb(225, 215, 198)
    End Function

    Public Function ForeTone() As Color
        Dim m = Main()
        If m IsNot Nothing Then Return m.currentButtonFore
        Return Color.FromArgb(96, 84, 66)
    End Function

    Public Function Accent() As Color
        Dim m = Main()
        If m IsNot Nothing Then Return m.OnColorTheme
        Return Color.FromArgb(0, 180, 137)
    End Function

    ' --- colour helpers -------------------------------------------------------

    ''' <summary>Linear blend between two colours (t = 0 → a, t = 1 → b).</summary>
    Public Function Blend(ByVal a As Color, ByVal b As Color, ByVal t As Double) As Color
        If t < 0 Then t = 0
        If t > 1 Then t = 1
        Return Color.FromArgb(
            CInt(a.R + (b.R - a.R) * t),
            CInt(a.G + (b.G - a.G) * t),
            CInt(a.B + (b.B - a.B) * t))
    End Function

    ''' <summary>Black or white, whichever reads better on the given background.</summary>
    Public Function ContrastText(ByVal bg As Color) As Color
        Dim lum As Double = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255.0
        If lum > 0.6 Then Return Color.FromArgb(25, 25, 28)
        Return Color.White
    End Function

    ' --- public entry points --------------------------------------------------

    ''' <summary>Applies the active theme to a whole dialog and its controls.</summary>
    Public Sub Style(ByVal form As Form)
        If form Is Nothing Then Return
        Try
            form.BackColor = SurfaceBack()
            form.ForeColor = ForeTone()
            form.Font = New Font("Segoe UI", 9.5!, FontStyle.Regular)
            StyleChildren(form)
        Catch
        End Try
    End Sub

    Private Sub StyleChildren(ByVal parent As Control)
        For Each c As Control In parent.Controls
            If TypeOf c Is Button Then
                StyleButton(CType(c, Button))
            ElseIf TypeOf c Is Label Then
                c.ForeColor = ForeTone()
            ElseIf TypeOf c Is LinkLabel Then
                CType(c, LinkLabel).LinkColor = Accent()
            ElseIf TypeOf c Is TextBox Then
                CType(c, TextBox).BorderStyle = BorderStyle.FixedSingle
            ElseIf TypeOf c Is CheckBox OrElse TypeOf c Is RadioButton OrElse TypeOf c Is GroupBox Then
                c.ForeColor = ForeTone()
            End If
            If c.HasChildren Then StyleChildren(c)
        Next
    End Sub

    ''' <summary>Gives a button a flat, theme-tinted look with hover feedback.</summary>
    Public Sub StyleButton(ByVal b As Button)
        Dim surface As Color = Blend(SurfaceBack(), ForeTone(), 0.1)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 1
        b.FlatAppearance.BorderColor = BorderTone()
        b.FlatAppearance.MouseOverBackColor = Blend(surface, Accent(), 0.28)
        b.FlatAppearance.MouseDownBackColor = Blend(surface, Accent(), 0.45)
        b.BackColor = surface
        b.ForeColor = ForeTone()
        b.UseVisualStyleBackColor = False
        b.Cursor = Cursors.Hand
        If b.Font Is Nothing Then b.Font = New Font("Segoe UI", 9.5!)
    End Sub

    ''' <summary>Turns a button into a filled accent (primary) action button.</summary>
    Public Sub MakePrimary(ByVal b As Button)
        Dim a As Color = Accent()
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.FlatAppearance.MouseOverBackColor = Blend(a, Color.White, 0.16)
        b.FlatAppearance.MouseDownBackColor = Blend(a, Color.Black, 0.14)
        b.BackColor = a
        b.ForeColor = ContrastText(a)
        b.UseVisualStyleBackColor = False
        b.Cursor = Cursors.Hand
        b.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
    End Sub

End Module
