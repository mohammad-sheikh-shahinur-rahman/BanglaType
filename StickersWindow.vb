'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: emoji / sticker picker.
'

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' A simple emoji / kaomoji picker. Clicking a glyph pastes it into the active
''' application via <see cref="MainUI.PasteText"/>.
''' </summary>
Public Class StickersWindow
    Inherits Form

    Private ReadOnly _stickers As String() = New String() {
        "😀", "😁", "😂", "🤣", "😊", "😍", "😘", "😎",
        "🤔", "😢", "😭", "😡", "👍", "👎", "🙏", "👏",
        "❤️", "🔥", "🎉", "✨", "💯", "✅", "❌", "⭐",
        "🌹", "🌸", "☀️", "🌙", "🇧🇩", "📚", "☕", "🎂",
        "ভালো", "ধন্যবাদ", "শুভকামনা", "অভিনন্দন",
        "(ʘ‿ʘ)", "¯\_(ツ)_/¯", "(╯°□°)╯", "ᕕ( ᐛ )ᕗ"}

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "Stickers & GIFs"
        Me.Size = New Size(360, 320)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.TopMost = True
        Me.ShowInTaskbar = False
        Me.DoubleBuffered = True
        Me.BackColor = Color.FromArgb(20, 20, 22)

        Dim pnlTitle As New Panel() With {.Dock = DockStyle.Top, .Height = 36, .BackColor = Color.FromArgb(28, 28, 30)}
        AddHandler pnlTitle.MouseDown, AddressOf Header_MouseDown
        Dim lblTitle As New Label() With {
            .Text = "🎨 Stickers & Emoji",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.White, .Location = New Point(12, 8), .AutoSize = True}
        AddHandler lblTitle.MouseDown, AddressOf Header_MouseDown
        Dim btnClose As New Button() With {
            .Text = "✕", .Font = New Font("Segoe UI", 9.0!), .ForeColor = Color.DarkGray,
            .BackColor = Color.Transparent, .FlatStyle = FlatStyle.Flat,
            .Size = New Size(36, 36), .Location = New Point(Me.Width - 36, 0), .Cursor = Cursors.Hand}
        btnClose.FlatAppearance.BorderSize = 0
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35)
        AddHandler btnClose.Click, Sub() Me.Close()
        pnlTitle.Controls.Add(lblTitle)
        pnlTitle.Controls.Add(btnClose)
        Me.Controls.Add(pnlTitle)

        Dim flow As New FlowLayoutPanel() With {
            .Location = New Point(8, 44), .Size = New Size(344, 230),
            .AutoScroll = True, .BackColor = Color.FromArgb(20, 20, 22)}

        For Each s As String In _stickers
            Dim sticker As String = s
            Dim btn As New Button() With {
                .Text = sticker,
                .Font = New Font("Segoe UI Emoji", 12.0!),
                .ForeColor = Color.White, .BackColor = Color.FromArgb(38, 38, 42),
                .FlatStyle = FlatStyle.Flat, .Size = New Size(74, 40), .Cursor = Cursors.Hand}
            btn.FlatAppearance.BorderSize = 0
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 180, 137)
            AddHandler btn.Click, Sub() MainUI.PasteText(sticker)
            flow.Controls.Add(btn)
        Next
        Me.Controls.Add(flow)

        Dim lblHint As New Label() With {
            .Text = "Click a sticker to insert it into the active app.",
            .Font = New Font("Segoe UI", 8.0!), .ForeColor = Color.Gray,
            .Location = New Point(12, 282), .Size = New Size(336, 18)}
        Me.Controls.Add(lblHint)

        ApplyRoundedCorners()
    End Sub

    Private Sub Header_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            MainUI.ReleaseCapture()
            MainUI.SendMessage(Handle, MainUI.WM_NCLBUTTONDOWN, MainUI.HT_CAPTION, 0)
        End If
    End Sub

    Private Function GetRoundedRectPath(ByVal rect As Rectangle, ByVal radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
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
        Using path As GraphicsPath = GetRoundedRectPath(New Rectangle(0, 0, Width, Height), 10)
            Me.Region = New Region(path)
        End Using
    End Sub

    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)
        MyBase.OnPaintBackground(e)
        Dim rect As New Rectangle(0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Using path As GraphicsPath = GetRoundedRectPath(rect, 10)
            Using p As New Pen(Color.FromArgb(60, 60, 64), 1.5)
                e.Graphics.DrawPath(p, path)
            End Using
        End Using
    End Sub

End Class
