'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: floating candidate / suggestion popup.
'

Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' A borderless, non-activating popup that lists word candidates near the caret.
''' Selection is driven entirely from the global keyboard hook (Up/Down/Tab/1-9),
''' so the window must never steal focus from the app the user is typing into.
''' </summary>
Public Class SuggestionWindow
    Inherits Form

    Private ReadOnly _list As New ListBox()
    Private _items As New List(Of String)()
    Private _selected As Integer = 0

    ' Theme colours (defaults match the Dark suggestion scheme).
    Private _back As Color = Color.FromArgb(28, 28, 30)
    Private _fore As Color = Color.White
    Private _selBack As Color = Color.FromArgb(0, 180, 137)
    Private _selFore As Color = Color.White

    Private Const RowHeight As Integer = 24
    Private Const MaxVisibleRows As Integer = 9

    Public Sub New()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.ShowInTaskbar = False
        Me.TopMost = True
        Me.StartPosition = FormStartPosition.Manual
        Me.MinimumSize = New Size(60, RowHeight)
        Me.DoubleBuffered = True
        Me.BackColor = _back
        Me.Padding = New Padding(1)

        _list.Dock = DockStyle.Fill
        _list.BorderStyle = BorderStyle.None
        _list.DrawMode = DrawMode.OwnerDrawFixed
        _list.ItemHeight = RowHeight
        _list.IntegralHeight = False
        _list.Font = New Font("Nirmala UI", 11.0!, FontStyle.Regular)
        _list.BackColor = _back
        _list.ForeColor = _fore
        AddHandler _list.DrawItem, AddressOf List_DrawItem
        AddHandler _list.MouseClick, AddressOf List_MouseClick
        Me.Controls.Add(_list)
    End Sub

    ''' <summary>Do not activate (take focus) when shown.</summary>
    Protected Overrides ReadOnly Property ShowWithoutActivation() As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Const WS_EX_NOACTIVATE As Integer = &H8000000
            Const WS_EX_TOOLWINDOW As Integer = &H80
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_NOACTIVATE Or WS_EX_TOOLWINDOW
            Return cp
        End Get
    End Property

    ' --- public API consumed by Keyboard.vb -------------------------------

    Public ReadOnly Property Count() As Integer
        Get
            Return _items.Count
        End Get
    End Property

    Public ReadOnly Property SelectedIndex() As Integer
        Get
            Return _selected
        End Get
    End Property

    Public Function ItemAt(ByVal index As Integer) As String
        If index < 0 OrElse index >= _items.Count Then Return ""
        Return _items(index)
    End Function

    ''' <summary>Shows the candidate list anchored just below the given screen point.</summary>
    Public Sub ShowCandidates(ByVal candidates As List(Of String), ByVal anchor As Point)
        If candidates Is Nothing OrElse candidates.Count = 0 Then
            HideWindow()
            Return
        End If

        _items = New List(Of String)(candidates)
        _selected = 0

        _list.BeginUpdate()
        _list.Items.Clear()
        Dim idx As Integer = 1
        For Each w As String In _items
            _list.Items.Add(If(idx <= 9, idx & "  " & w, "    " & w))
            idx += 1
        Next
        _list.EndUpdate()

        ' Size to content.
        Dim rows As Integer = Math.Min(_items.Count, MaxVisibleRows)
        Dim widest As Integer = 80
        Using g As Graphics = _list.CreateGraphics()
            For Each it As Object In _list.Items
                Dim w As Integer = CInt(g.MeasureString(it.ToString(), _list.Font).Width) + 24
                If w > widest Then widest = w
            Next
        End Using
        Me.Size = New Size(Math.Min(widest, 360), rows * RowHeight + 2)

        ' Keep it on screen.
        Dim scrArea As Rectangle = System.Windows.Forms.Screen.FromPoint(anchor).WorkingArea
        Dim x As Integer = anchor.X
        Dim y As Integer = anchor.Y + 20
        If x + Me.Width > scrArea.Right Then x = scrArea.Right - Me.Width
        If x < scrArea.Left Then x = scrArea.Left
        If y + Me.Height > scrArea.Bottom Then y = anchor.Y - Me.Height - 2
        If y < scrArea.Top Then y = scrArea.Top
        Me.Location = New Point(x, y)

        _list.SelectedIndex = _selected
        If Not Me.Visible Then Me.Show()
        Me.Refresh()
    End Sub

    Public Sub HideWindow()
        _items.Clear()
        _list.Items.Clear()
        _selected = 0
        If Me.Visible Then Me.Hide()
    End Sub

    ''' <summary>Moves the highlight by <paramref name="delta"/>, wrapping at the ends.</summary>
    Public Sub MoveSelection(ByVal delta As Integer)
        If _items.Count = 0 Then Return
        _selected += delta
        If _selected < 0 Then _selected = _items.Count - 1
        If _selected >= _items.Count Then _selected = 0
        _list.SelectedIndex = _selected
        _list.Invalidate()
    End Sub

    Public Sub ApplyTheme(ByVal t As Theme)
        If t Is Nothing Then Return
        _back = t.SuggestBack
        _fore = t.SuggestFore
        _selBack = t.SuggestSelectBack
        _selFore = t.SuggestSelectFore
        Me.BackColor = _back
        _list.BackColor = _back
        _list.ForeColor = _fore
        _list.Invalidate()
    End Sub

    ' --- rendering / mouse ------------------------------------------------

    Private Sub List_DrawItem(ByVal sender As Object, ByVal e As DrawItemEventArgs)
        If e.Index < 0 Then Return
        Dim selected As Boolean = (e.Index = _selected)
        Dim bg As Color = If(selected, _selBack, _back)
        Dim fg As Color = If(selected, _selFore, _fore)

        Using b As New SolidBrush(bg)
            e.Graphics.FillRectangle(b, e.Bounds)
        End Using
        Using b As New SolidBrush(fg)
            Dim sf As New StringFormat() With {.LineAlignment = StringAlignment.Center}
            Dim r As New RectangleF(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height)
            e.Graphics.DrawString(_list.Items(e.Index).ToString(), _list.Font, b, r, sf)
        End Using
    End Sub

    Private Sub List_MouseClick(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim idx As Integer = _list.IndexFromPoint(e.Location)
        If idx >= 0 AndAlso idx < _items.Count Then
            _selected = idx
            _list.Invalidate()
        End If
    End Sub

End Class
