Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D

Public Class ShortcutSuggestionWindow
    Inherits Form

    Private lblTitle As Label
    Private lblInfo As Label
    Private txtShortcut As TextBox
    Private lblPhrase As Label
    Private btnCreate As Button
    Private btnIgnore As Button

    Private PhraseText As String

    Public Sub New(ByVal phrase As String)
        PhraseText = phrase
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "Smart Shortcut Suggestion"
        Me.Size = New Size(420, 240)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.TopMost = True
        Me.ShowInTaskbar = False
        Me.DoubleBuffered = True
        Me.BackColor = Color.FromArgb(28, 28, 30)

        lblTitle = New Label() With {
            .Text = "🧠 AI Smart Shortcut Suggestion",
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(20, 15),
            .Size = New Size(380, 25)
        }

        lblInfo = New Label() With {
            .Text = "You type this phrase frequently. Want to create a shortcut?",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.DarkGray,
            .Location = New Point(20, 45),
            .Size = New Size(380, 20)
        }

        lblPhrase = New Label() With {
            .Text = """" & PhraseText & """",
            .Font = New Font("Nirmala UI", 10.0!, FontStyle.Italic),
            .ForeColor = Color.FromArgb(0, 180, 137),
            .Location = New Point(20, 70),
            .Size = New Size(380, 45),
            .TextAlign = ContentAlignment.MiddleLeft
        }

        Dim lblTrigger As New Label() With {
            .Text = "Shortcut Key (e.g. ;ok):",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.White,
            .Location = New Point(20, 125),
            .Size = New Size(150, 20),
            .TextAlign = ContentAlignment.MiddleLeft
        }

        txtShortcut = New TextBox() With {
            .Text = SuggestDefaultShortcut(PhraseText),
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(48, 48, 50),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(170, 123),
            .Size = New Size(220, 24)
        }

        btnCreate = New Button() With {
            .Text = "Create Shortcut",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(0, 180, 137),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(170, 32),
            .Location = New Point(220, 175),
            .Cursor = Cursors.Hand
        }
        btnCreate.FlatAppearance.BorderSize = 0
        AddHandler btnCreate.Click, AddressOf CreateShortcut_Click

        btnIgnore = New Button() With {
            .Text = "Ignore",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.DarkGray,
            .BackColor = Color.FromArgb(48, 48, 50),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(100, 32),
            .Location = New Point(110, 175),
            .Cursor = Cursors.Hand
        }
        btnIgnore.FlatAppearance.BorderSize = 0
        AddHandler btnIgnore.Click, Sub() Me.Close()

        Me.Controls.Add(lblTitle)
        Me.Controls.Add(lblInfo)
        Me.Controls.Add(lblPhrase)
        Me.Controls.Add(lblTrigger)
        Me.Controls.Add(txtShortcut)
        Me.Controls.Add(btnCreate)
        Me.Controls.Add(btnIgnore)

        ApplyRoundedCorners()
    End Sub

    Private Function SuggestDefaultShortcut(ByVal phrase As String) As String
        ' Simple logic to take letters or default to a prefix
        Dim cleaned As String = phrase.Trim().ToLower()
        If cleaned.Length > 0 Then
            ' If it starts with a Bangla letter, take its first character transliterated or just use first letter
            Return ";" & cleaned.Substring(0, Math.Min(3, cleaned.Length)).Replace(" ", "")
        End If
        Return ";s1"
    End Function

    Private Sub CreateShortcut_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim sc As String = txtShortcut.Text.Trim()
        If String.IsNullOrEmpty(sc) Then
            MessageBox.Show("Please enter a shortcut trigger prefix.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If Not sc.StartsWith(";") AndAlso Not sc.StartsWith("!") Then
            sc = ";" & sc ' Force prefix to avoid collision
        End If

        ' Add to macro engine
        MacroEngine.AddMacro(sc, PhraseText)
        MessageBox.Show("Shortcut '" & sc & "' created successfully for: " & vbCrLf & """" & PhraseText & """", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Me.Close()
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
        Using path As GraphicsPath = GetRoundedRectPath(New Rectangle(0, 0, Width, Height), 12)
            Me.Region = New Region(path)
        End Using
    End Sub

    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)
        MyBase.OnPaintBackground(e)
        Dim rect As New Rectangle(0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Using path As GraphicsPath = GetRoundedRectPath(rect, 12)
            Using p As New Pen(Color.FromArgb(60, 60, 64), 1.5)
                e.Graphics.DrawPath(p, path)
            End Using
        End Using
    End Sub
End Class
