Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Collections.Generic

Public Class SmartPredictionWindow
    Inherits Form

    Private pnlTitle As Panel
    Private lblTitle As Label
    Private btnClose As Button

    Private chkEnableSuggestions As CheckBox
    Private txtGeminiApiKey As TextBox
    Private btnToggleKeyMask As Button
    Private isKeyMasked As Boolean = True

    Private lblStatsLocal As Label
    Private lblStatsWords As Label

    Private btnDictManager As Button
    Private btnWordCustomizer As Button
    Private btnAutoCorrect As Button
    Private btnSave As Button

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        ' Setup Form
        Me.Text = "Smart Word Prediction Settings"
        Me.Size = New Size(420, 440)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.TopMost = True
        Me.ShowInTaskbar = False
        Me.DoubleBuffered = True
        Me.BackColor = Color.FromArgb(20, 20, 22)

        ' Header Bar
        pnlTitle = New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 38,
            .BackColor = Color.FromArgb(28, 28, 30)
        }
        AddHandler pnlTitle.MouseDown, AddressOf Header_MouseDown

        lblTitle = New Label() With {
            .Text = "⚙️ Smart Word Prediction Settings",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(12, 9),
            .AutoSize = True
        }
        AddHandler lblTitle.MouseDown, AddressOf Header_MouseDown

        btnClose = New Button() With {
            .Text = "✕",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.DarkGray,
            .BackColor = Color.Transparent,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(38, 38),
            .Location = New Point(Me.Width - 38, 0),
            .Cursor = Cursors.Hand
        }
        btnClose.FlatAppearance.BorderSize = 0
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35)
        btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(241, 112, 122)
        AddHandler btnClose.Click, Sub() Me.Close()

        pnlTitle.Controls.Add(lblTitle)
        pnlTitle.Controls.Add(btnClose)
        Me.Controls.Add(pnlTitle)

        ' Enable Suggestions Checkbox
        chkEnableSuggestions = New CheckBox() With {
            .Text = "Enable Predictive Suggestion Bar",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(20, 60),
            .Size = New Size(380, 28),
            .Checked = AppSettings.SuggestionsEnabled
        }
        Me.Controls.Add(chkEnableSuggestions)

        Dim lblSuggDesc As New Label() With {
            .Text = "Shows real-time word candidates, next-word predictions, and transliterations.",
            .Font = New Font("Segoe UI", 8.5!, FontStyle.Italic),
            .ForeColor = Color.Gray,
            .Location = New Point(42, 88),
            .Size = New Size(360, 18)
        }
        Me.Controls.Add(lblSuggDesc)

        ' Gemini API Key Field
        Dim lblGemini As New Label() With {
            .Text = "Gemini AI API Key (Optional for cloud suggestions):",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(20, 120),
            .Size = New Size(380, 18)
        }
        Me.Controls.Add(lblGemini)

        txtGeminiApiKey = New TextBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(20, 142),
            .Size = New Size(310, 24),
            .Text = AppSettings.GeminiApiKey,
            .UseSystemPasswordChar = True
        }
        Me.Controls.Add(txtGeminiApiKey)

        btnToggleKeyMask = New Button() With {
            .Text = "👁️",
            .Font = New Font("Segoe UI", 9.0!),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(55, 24),
            .Location = New Point(345, 142),
            .Cursor = Cursors.Hand
        }
        btnToggleKeyMask.FlatAppearance.BorderSize = 1
        btnToggleKeyMask.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 64)
        AddHandler btnToggleKeyMask.Click, AddressOf BtnToggleKeyMask_Click
        Me.Controls.Add(btnToggleKeyMask)

        ' Prediction Statistics Group
        Dim pnlStats As New Panel() With {
            .Location = New Point(20, 185),
            .Size = New Size(380, 75),
            .BackColor = Color.FromArgb(28, 28, 30),
            .BorderStyle = BorderStyle.None
        }
        
        lblStatsLocal = New Label() With {
            .Text = "📚 Offline Engine: Local dictionary active",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(10, 12),
            .Size = New Size(360, 20)
        }
        pnlStats.Controls.Add(lblStatsLocal)

        Dim totalWords As Integer = SuggestionEngine.GetLoadedWordCount()
        lblStatsWords = New Label() With {
            .Text = "📈 Memory Database: " & totalWords & " unique words loaded",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(10, 38),
            .Size = New Size(360, 20)
        }
        pnlStats.Controls.Add(lblStatsWords)
        Me.Controls.Add(pnlStats)

        ' Settings Action Buttons
        btnDictManager = New Button() With {
            .Text = "📖 Custom Dictionary Manager...",
            .Font = New Font("Segoe UI", 9.0!),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(180, 34),
            .Location = New Point(20, 280),
            .Cursor = Cursors.Hand
        }
        btnDictManager.FlatAppearance.BorderSize = 0
        AddHandler btnDictManager.Click, Sub()
                                             Using dlg As New DictionaryManagerWindow()
                                                 dlg.ShowDialog()
                                             End Using
                                             lblStatsWords.Text = "📈 Memory Database: " & SuggestionEngine.GetLoadedWordCount() & " unique words loaded"
                                         End Sub
        Me.Controls.Add(btnDictManager)

        btnWordCustomizer = New Button() With {
            .Text = "✏️ Word Customizer...",
            .Font = New Font("Segoe UI", 9.0!),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(180, 34),
            .Location = New Point(220, 280),
            .Cursor = Cursors.Hand
        }
        btnWordCustomizer.FlatAppearance.BorderSize = 0
        AddHandler btnWordCustomizer.Click, Sub()
                                                Using dlg As New WordCustomizerWindow()
                                                    dlg.ShowDialog()
                                                End Using
                                            End Sub
        Me.Controls.Add(btnWordCustomizer)

        btnAutoCorrect = New Button() With {
            .Text = "📝 Manage Auto-Correct Spellings...",
            .Font = New Font("Segoe UI", 9.0!),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(380, 34),
            .Location = New Point(20, 325),
            .Cursor = Cursors.Hand
        }
        btnAutoCorrect.FlatAppearance.BorderSize = 0
        AddHandler btnAutoCorrect.Click, Sub() SuggestionEngine.OpenAutoCorrectInNotepad()
        Me.Controls.Add(btnAutoCorrect)

        ' Save Button
        btnSave = New Button() With {
            .Text = "💾 Save Configuration",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(0, 180, 137),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(380, 38),
            .Location = New Point(20, 380),
            .Cursor = Cursors.Hand
        }
        btnSave.FlatAppearance.BorderSize = 0
        AddHandler btnSave.Click, AddressOf BtnSave_Click
        Me.Controls.Add(btnSave)

        ApplyRoundedCorners()
    End Sub

    Private Sub BtnToggleKeyMask_Click(sender As Object, e As EventArgs)
        isKeyMasked = Not isKeyMasked
        txtGeminiApiKey.UseSystemPasswordChar = isKeyMasked
        btnToggleKeyMask.Text = If(isKeyMasked, "👁️", "🙈")
    End Sub

    Private Sub BtnSave_Click(sender As Object, e As EventArgs)
        AppSettings.SuggestionsEnabled = chkEnableSuggestions.Checked
        AppSettings.GeminiApiKey = txtGeminiApiKey.Text.Trim()
        AppSettings.Save()

        If Not AppSettings.SuggestionsEnabled Then
            Keyboard.HideSuggest()
        End If

        MessageBox.Show("Smart Word Prediction settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Me.Close()
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
