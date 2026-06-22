'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: theme model.
'

Imports System.Drawing

''' <summary>
''' A named colour scheme applied to the topbar, buttons and suggestion popup.
''' Colours default to the light scheme so a partially-populated theme still paints.
''' </summary>
Public Class Theme

    Public Name As String = "Light"

    ' Topbar / chrome
    Public TopbarBack As Color = Color.FromArgb(242, 242, 244)
    Public BorderColor As Color = Color.FromArgb(230, 230, 230)
    Public ButtonFore As Color = Color.FromArgb(140, 140, 140)

    ' Mode indicator colours
    Public OnColor As Color = Color.FromArgb(0, 180, 137)    ' Bangla active
    Public OffColor As Color = Color.FromArgb(222, 75, 57)   ' Banglish active

    ' Suggestion popup
    Public SuggestBack As Color = Color.FromArgb(28, 28, 30)
    Public SuggestFore As Color = Color.White
    Public SuggestSelectBack As Color = Color.FromArgb(0, 180, 137)
    Public SuggestSelectFore As Color = Color.White

    Public Sub New()
    End Sub

    Public Sub New(ByVal name As String)
        Me.Name = name
    End Sub

End Class
