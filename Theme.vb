'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   Borno Lite "Advance" additions: theme color model.
'

Imports System.Drawing

''' <summary>A named color scheme for the topbar, on/off indicator, and suggestion popup.</summary>
Public Class Theme
    Public Name As String = "Light"
    Public TopbarBack As Color = Color.FromArgb(242, 242, 244)
    Public TopbarBorder As Color = Color.FromArgb(230, 230, 230)
    Public ButtonFore As Color = Color.FromArgb(20, 20, 20)
    Public OnColor As Color = Color.FromArgb(0, 180, 137)
    Public OffColor As Color = Color.FromArgb(222, 75, 57)
    Public SuggestBack As Color = Color.FromArgb(250, 250, 250)
    Public SuggestFore As Color = Color.FromArgb(20, 20, 20)
    Public SuggestSelBack As Color = Color.FromArgb(201, 222, 245)
    Public SuggestSelFore As Color = Color.FromArgb(10, 10, 10)
End Class
