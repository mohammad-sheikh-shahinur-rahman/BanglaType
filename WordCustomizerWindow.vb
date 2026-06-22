Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Collections.Generic

Public Class WordCustomizerWindow
    Inherits Form

    Private pnlTitle As Panel
    Private lblTitle As Label
    Private btnClose As Button

    Private txtTargetWord As TextBox
    Private txtKey1 As TextBox
    Private txtKey2 As TextBox
    Private txtKey3 As TextBox
    Private txtKey4 As TextBox
    Private btnSave As Button

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        ' Setup Form
        Me.Text = "BanglaType Word Customizer"
        Me.Size = New Size(400, 320)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.TopMost = True
        Me.ShowInTaskbar = False
        Me.DoubleBuffered = True
        Me.BackColor = Color.FromArgb(20, 20, 22)

        ' Header Bar
        pnlTitle = New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 36,
            .BackColor = Color.FromArgb(28, 28, 30)
        }
        AddHandler pnlTitle.MouseDown, AddressOf Header_MouseDown

        lblTitle = New Label() With {
            .Text = "✏️ BanglaType Word Customizer",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(12, 8),
            .AutoSize = True
        }
        AddHandler lblTitle.MouseDown, AddressOf Header_MouseDown

        btnClose = New Button() With {
            .Text = "✕",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.DarkGray,
            .BackColor = Color.Transparent,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(36, 36),
            .Location = New Point(Me.Width - 36, 0),
            .Cursor = Cursors.Hand
        }
        btnClose.FlatAppearance.BorderSize = 0
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35)
        btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(241, 112, 122)
        AddHandler btnClose.Click, Sub() Me.Close()

        pnlTitle.Controls.Add(lblTitle)
        pnlTitle.Controls.Add(btnClose)
        Me.Controls.Add(pnlTitle)

        ' Target Word (Bangla)
        Dim lblTarget As New Label() With {
            .Text = "Target Bangla Word (or special characters):",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(15, 52),
            .Size = New Size(370, 18)
        }
        Me.Controls.Add(lblTarget)

        txtTargetWord = New TextBox() With {
            .Font = New Font("Nirmala UI", 10.5!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(15, 72),
            .Size = New Size(370, 26)
        }
        Me.Controls.Add(txtTargetWord)

        ' Phonetic Variations
        Dim lblKeys As New Label() With {
            .Text = "Phonetic Keywords (Up to 4 custom variations):",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(15, 112),
            .Size = New Size(370, 18)
        }
        Me.Controls.Add(lblKeys)

        txtKey1 = New TextBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(15, 135),
            .Size = New Size(175, 24)
        }
        Me.Controls.Add(txtKey1)

        txtKey2 = New TextBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(210, 135),
            .Size = New Size(175, 24)
        }
        Me.Controls.Add(txtKey2)

        txtKey3 = New TextBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(15, 175),
            .Size = New Size(175, 24)
        }
        Me.Controls.Add(txtKey3)

        txtKey4 = New TextBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(210, 175),
            .Size = New Size(175, 24)
        }
        Me.Controls.Add(txtKey4)

        ' Action Button
        btnSave = New Button() With {
            .Text = "💾 Save Word Customization",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(0, 180, 137),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(370, 36),
            .Location = New Point(15, 255),
            .Cursor = Cursors.Hand
        }
        btnSave.FlatAppearance.BorderSize = 0
        AddHandler btnSave.Click, AddressOf BtnSave_Click
        Me.Controls.Add(btnSave)

        ApplyRoundedCorners()
    End Sub

    Private Sub BtnSave_Click(sender As Object, e As EventArgs)
        Dim targetWord As String = txtTargetWord.Text.Trim()
        If String.IsNullOrEmpty(targetWord) Then
            MessageBox.Show("Please enter the target Bangla word.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim variations As New List(Of String)()
        AddIfValid(variations, txtKey1.Text)
        AddIfValid(variations, txtKey2.Text)
        AddIfValid(variations, txtKey3.Text)
        AddIfValid(variations, txtKey4.Text)

        If variations.Count = 0 Then
            MessageBox.Show("Please enter at least one phonetic variation keyword.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Save to custom phonetic database
        SuggestionEngine.SaveCustomPhoneticMappings(targetWord, variations)
        MessageBox.Show("Word Customization saved successfully for: " & vbCrLf & targetWord, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Me.Close()
    End Sub

    Private Sub AddIfValid(ByVal list As List(Of String), ByVal text As String)
        Dim clean As String = text.Trim().ToLower()
        If Not String.IsNullOrEmpty(clean) AndAlso Not list.Contains(clean) Then
            list.Add(clean)
        End If
    End Sub

    Private Sub Header_MouseDown(sender As Object, e As MouseEventArgs)
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
