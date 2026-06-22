'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: "you type this a lot" shortcut offer.
'

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' Shown when the user repeatedly types the same long phrase. Offers to bind it
''' to a short macro abbreviation (saved through <see cref="MacroEngine"/>).
''' </summary>
Public Class ShortcutSuggestionWindow
    Inherits Form

    Private ReadOnly _phrase As String
    Private txtAbbr As TextBox

    Public Sub New(ByVal phrase As String)
        _phrase = If(phrase, "").Trim()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "Create Shortcut"
        Me.Size = New Size(420, 230)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.TopMost = True
        Me.ShowInTaskbar = False
        Me.DoubleBuffered = True
        Me.BackColor = Color.FromArgb(20, 20, 22)

        Dim pnlTitle As New Panel() With {.Dock = DockStyle.Top, .Height = 36, .BackColor = Color.FromArgb(28, 28, 30)}
        AddHandler pnlTitle.MouseDown, AddressOf Header_MouseDown
        Dim lblTitle As New Label() With {
            .Text = "💡 Create a Quick Shortcut",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.White, .Location = New Point(12, 8), .AutoSize = True}
        AddHandler lblTitle.MouseDown, AddressOf Header_MouseDown
        pnlTitle.Controls.Add(lblTitle)
        Me.Controls.Add(pnlTitle)

        Dim lblInfo As New Label() With {
            .Text = "You've typed this phrase several times:",
            .Font = New Font("Segoe UI", 9.0!), .ForeColor = Color.LightGray,
            .Location = New Point(15, 50), .Size = New Size(390, 18)}
        Me.Controls.Add(lblInfo)

        Dim lblPhrase As New Label() With {
            .Text = If(_phrase.Length > 60, _phrase.Substring(0, 57) & "...", _phrase),
            .Font = New Font("Nirmala UI", 10.0!, FontStyle.Bold), .ForeColor = Color.FromArgb(0, 200, 150),
            .Location = New Point(15, 72), .Size = New Size(390, 40)}
        Me.Controls.Add(lblPhrase)

        Dim lblAbbr As New Label() With {
            .Text = "Type a short abbreviation (e.g. !my):",
            .Font = New Font("Segoe UI", 9.0!), .ForeColor = Color.LightGray,
            .Location = New Point(15, 118), .Size = New Size(390, 18)}
        Me.Controls.Add(lblAbbr)

        txtAbbr = New TextBox() With {
            .Font = New Font("Segoe UI", 10.0!), .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42), .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(15, 140), .Size = New Size(390, 26)}
        Me.Controls.Add(txtAbbr)

        Dim btnSave As New Button() With {
            .Text = "💾 Save Shortcut", .Font = New Font("Segoe UI", 9.0!, FontStyle.Bold),
            .ForeColor = Color.White, .BackColor = Color.FromArgb(0, 180, 137),
            .FlatStyle = FlatStyle.Flat, .Size = New Size(190, 32), .Location = New Point(15, 180), .Cursor = Cursors.Hand}
        btnSave.FlatAppearance.BorderSize = 0
        AddHandler btnSave.Click, AddressOf BtnSave_Click
        Me.Controls.Add(btnSave)

        Dim btnSkip As New Button() With {
            .Text = "Not now", .Font = New Font("Segoe UI", 9.0!),
            .ForeColor = Color.White, .BackColor = Color.FromArgb(48, 48, 50),
            .FlatStyle = FlatStyle.Flat, .Size = New Size(190, 32), .Location = New Point(215, 180), .Cursor = Cursors.Hand}
        btnSkip.FlatAppearance.BorderSize = 0
        AddHandler btnSkip.Click, Sub() Me.Close()
        Me.Controls.Add(btnSkip)

        ApplyRoundedCorners()
    End Sub

    Private Sub BtnSave_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim abbr As String = txtAbbr.Text.Trim()
        If String.IsNullOrEmpty(abbr) Then
            MessageBox.Show("Please enter an abbreviation.", "Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        MacroEngine.AddMacro(abbr, _phrase)
        MessageBox.Show("Shortcut saved. Type '" & abbr & "' then space to expand it.", "Shortcut Created", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Me.Close()
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
