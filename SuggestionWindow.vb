'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: floating candidate (suggestion) window.
'

Imports System.Drawing
Imports System.Drawing.Drawing2D

''' <summary>
''' A borderless, top-most, non-activating popup that lists word candidates near
''' the caret. It never takes focus from the user's application (WS_EX_NOACTIVATE),
''' so typing continues uninterrupted while suggestions are shown.
''' </summary>
Public Class SuggestionWindow
    Inherits Form

    Private items As New List(Of String)()
    Private selected As Integer = 0

    Private rowHeight As Integer = 24
    Private padX As Integer = 10
    Private ReadOnly itemFont As Font = New Font("Nirmala UI", 11.0!, FontStyle.Regular)
    Private ReadOnly numFont As Font = New Font("Segoe UI", 8.0!, FontStyle.Regular)

    ' Theme colors (overridden by ThemeManager)
    Public BackTheme As Color = Color.FromArgb(250, 250, 250)
    Public ForeTheme As Color = Color.FromArgb(20, 20, 20)
    Public SelBackTheme As Color = Color.FromArgb(201, 222, 245)
    Public SelForeTheme As Color = Color.FromArgb(10, 10, 10)
    Public BorderTheme As Color = Color.FromArgb(210, 210, 210)
    Public NumTheme As Color = Color.FromArgb(140, 140, 140)

    Public Sub New()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.ShowInTaskbar = False
        Me.TopMost = True
        Me.StartPosition = FormStartPosition.Manual
        Me.DoubleBuffered = True
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint, True)
        Me.Visible = False
    End Sub

    ' Show without stealing focus from the foreground app.
    Protected Overrides ReadOnly Property ShowWithoutActivation() As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Const WS_EX_TOPMOST As Integer = &H8
            Const WS_EX_TOOLWINDOW As Integer = &H80
            Const WS_EX_NOACTIVATE As Integer = &H8000000
            Const CS_DROPSHADOW As Integer = &H20000
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TOPMOST Or WS_EX_TOOLWINDOW Or WS_EX_NOACTIVATE
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
            Return cp
        End Get
    End Property

    Public ReadOnly Property Count() As Integer
        Get
            Return items.Count
        End Get
    End Property

    Public ReadOnly Property SelectedIndex() As Integer
        Get
            Return selected
        End Get
    End Property

    ''' <summary>Returns the candidate at the given index (1-based pick from a number key), or "".</summary>
    Public Function ItemAt(ByVal index As Integer) As String
        If index >= 0 AndAlso index < items.Count Then Return items(index)
        Return ""
    End Function

    Public Function SelectedItem() As String
        Return ItemAt(selected)
    End Function

    Public Sub MoveSelection(ByVal delta As Integer)
        If items.Count = 0 Then Return
        selected = ((selected + delta) Mod items.Count + items.Count) Mod items.Count
        Invalidate()
    End Sub

    ''' <summary>Updates the list and repositions near <paramref name="anchorScreenPos"/> (the caret).</summary>
    Public Sub ShowCandidates(ByVal list As List(Of String), ByVal anchorScreenPos As Point)
        items = If(list, New List(Of String)())
        selected = 0
        If items.Count = 0 Then
            HideWindow()
            Return
        End If

        ' Size to content.
        Dim maxW As Integer = 60
        Using g As Graphics = Me.CreateGraphics()
            For i As Integer = 0 To items.Count - 1
                Dim line As String = (i + 1).ToString() & ".  " & items(i)
                Dim sz As SizeF = g.MeasureString(line, itemFont)
                maxW = Math.Max(maxW, CInt(Math.Ceiling(sz.Width)) + padX * 2)
            Next
        End Using
        Dim h As Integer = rowHeight * items.Count + 4

        ' Keep on screen: prefer below the caret, flip above if needed.
        Dim wa As Rectangle = Screen.FromPoint(anchorScreenPos).WorkingArea
        Dim x As Integer = anchorScreenPos.X
        Dim y As Integer = anchorScreenPos.Y + 20
        If x + maxW > wa.Right Then x = wa.Right - maxW
        If x < wa.Left Then x = wa.Left
        If y + h > wa.Bottom Then y = anchorScreenPos.Y - h - 2
        If y < wa.Top Then y = wa.Top

        Me.Bounds = New Rectangle(x, y, maxW, h)
        ApplyRoundedCorners()
        If Not Me.Visible Then Me.Show()
        Invalidate()
    End Sub

    Public Sub HideWindow()
        items.Clear()
        If Me.Visible Then Me.Hide()
    End Sub

    Public Sub ApplyTheme(ByVal back As Color, ByVal fore As Color, ByVal selBack As Color, ByVal selFore As Color, ByVal border As Color)
        BackTheme = back
        ForeTheme = fore
        SelBackTheme = selBack
        SelForeTheme = selFore
        BorderTheme = border
        Invalidate()
    End Sub

    Private Function GetRoundedRectPath(ByVal rect As Rectangle, ByVal radius As Integer) As System.Drawing.Drawing2D.GraphicsPath
        Dim path As New System.Drawing.Drawing2D.GraphicsPath()
        Dim r2 As Integer = radius * 2
        path.StartFigure()
        path.AddArc(rect.X, rect.Y, r2, r2, 180, 90)
        path.AddArc(rect.Right - r2, rect.Y, r2, r2, 270, 90)
        path.AddArc(rect.Right - r2, rect.Bottom - r2, r2, r2, 0, 90)
        path.AddArc(rect.X, rect.Bottom - r2, r2, r2, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    Private Sub ApplyRoundedCorners()
        Using path As System.Drawing.Drawing2D.GraphicsPath = GetRoundedRectPath(New Rectangle(0, 0, Width, Height), 6)
            Me.Region = New Region(path)
        End Using
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.Clear(BackTheme)

        For i As Integer = 0 To items.Count - 1
            Dim r As New Rectangle(0, i * rowHeight + 2, Me.Width, rowHeight)
            If i = selected Then
                Dim highlightRect As New Rectangle(4, i * rowHeight + 3, Me.Width - 8, rowHeight - 2)
                Using path As GraphicsPath = GetRoundedRectPath(highlightRect, 4)
                    Using b As New SolidBrush(SelBackTheme)
                        g.FillPath(b, path)
                    End Using
                End Using
            End If

            Dim numStr As String = (i + 1).ToString() & "."
            Using nb As New SolidBrush(NumTheme)
                g.DrawString(numStr, numFont, nb, New PointF(padX - 4, i * rowHeight + 7))
            End Using

            Dim fc As Color = If(i = selected, SelForeTheme, ForeTheme)
            Using fb As New SolidBrush(fc)
                g.DrawString(items(i), itemFont, fb, New PointF(padX + 16, i * rowHeight + 4))
            End Using
        Next

        ' Border
        Dim borderRect As New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
        Using path As GraphicsPath = GetRoundedRectPath(borderRect, 6)
            Using p As New Pen(BorderTheme, 1)
                g.DrawPath(p, path)
            End Using
        End Using
    End Sub

End Class
