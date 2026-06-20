Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text

Public Class AnalyticsWindow
    Inherits Form

    Private pnlTitle As Panel
    Private lblTitle As Label
    Private btnClose As Button

    Private pnlWPM As Panel
    Private pnlAccuracy As Panel
    Private pnlMostUsed As Panel
    Private pnlTips As Panel

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        ' Setup Dashboard form
        Me.Text = "BanglaType Analytics"
        Me.Size = New Size(540, 420)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.TopMost = True
        Me.ShowInTaskbar = False
        Me.DoubleBuffered = True
        Me.BackColor = Color.FromArgb(20, 20, 22) ' Dark futuristic background

        ' Title Bar
        pnlTitle = New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 36,
            .BackColor = Color.FromArgb(28, 28, 30)
        }
        AddHandler pnlTitle.MouseDown, AddressOf Header_MouseDown

        lblTitle = New Label() With {
            .Text = "📊 BanglaType Analytics Dashboard",
            .Font = New Font("Segoe UI", 10.5!, FontStyle.Bold),
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

        ' Refresh Stats
        AnalyticsEngine.Load()

        Dim currentWpm As Integer = Math.Max(12, AnalyticsEngine.GetWPM())
        Dim accuracy As Double = AnalyticsEngine.GetAccuracy()
        Dim totalChars As Integer = AnalyticsEngine.TotalKeys

        ' Metric Panel 1: WPM Card
        pnlWPM = New Panel() With {
            .Location = New Point(15, 52),
            .Size = New Size(245, 120),
            .BackColor = Color.FromArgb(32, 32, 35)
        }
        AddHandler pnlWPM.Paint, Sub(s, e)
                                     DrawCardBackground(e.Graphics, pnlWPM.ClientRectangle, "Speed (WPM)", currentWpm & " WPM", Color.FromArgb(0, 180, 137))
                                 End Sub

        ' Metric Panel 2: Accuracy Card
        pnlAccuracy = New Panel() With {
            .Location = New Point(280, 52),
            .Size = New Size(245, 120),
            .BackColor = Color.FromArgb(32, 32, 35)
        }
        AddHandler pnlAccuracy.Paint, Sub(s, e)
                                          DrawCardBackground(e.Graphics, pnlAccuracy.ClientRectangle, "Accuracy %", accuracy.ToString("F1") & "%", Color.FromArgb(222, 75, 57))
                                      End Sub

        ' Metric Panel 3: Most Used Words Card
        pnlMostUsed = New Panel() With {
            .Location = New Point(15, 187),
            .Size = New Size(245, 215),
            .BackColor = Color.FromArgb(32, 32, 35)
        }
        AddHandler pnlMostUsed.Paint, AddressOf DrawMostUsedWords

        ' Metric Panel 4: AI Tips Card
        pnlTips = New Panel() With {
            .Location = New Point(280, 187),
            .Size = New Size(245, 215),
            .BackColor = Color.FromArgb(32, 32, 35)
        }
        AddHandler pnlTips.Paint, Sub(s, e)
                                      Dim tipsText As String = GetTypingTips(currentWpm, accuracy)
                                      DrawTipsCard(e.Graphics, pnlTips.ClientRectangle, tipsText)
                                  End Sub

        Me.Controls.Add(pnlWPM)
        Me.Controls.Add(pnlAccuracy)
        Me.Controls.Add(pnlMostUsed)
        Me.Controls.Add(pnlTips)

        ApplyRoundedCorners()
    End Sub

    Private Sub DrawCardBackground(ByVal g As Graphics, ByVal rect As Rectangle, ByVal title As String, ByVal value As String, ByVal themeColor As Color)
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.Clear(Color.FromArgb(32, 32, 35))

        ' Draw card boundary
        Using path As GraphicsPath = GetRoundedRectPath(New Rectangle(0, 0, rect.Width - 1, rect.Height - 1), 8)
            Using p As New Pen(Color.FromArgb(48, 48, 52), 1)
                g.DrawPath(p, path)
            End Using
        End Using

        ' Draw title text
        Using fontTitle As New Font("Segoe UI", 9.0!, FontStyle.Regular)
            Using brushTitle As New SolidBrush(Color.DarkGray)
                g.DrawString(title, fontTitle, brushTitle, New PointF(15, 15))
            End Using
        End Using

        ' Draw main value text
        Using fontValue As New Font("Segoe UI", 26.0!, FontStyle.Bold)
            Using brushValue As New SolidBrush(Color.White)
                g.DrawString(value, fontValue, brushValue, New PointF(15, 35))
            End Using
        End Using

        ' Draw custom accent line
        Using brushAccent As New SolidBrush(themeColor)
            g.FillRectangle(brushAccent, 15, 95, rect.Width - 30, 4)
        End Using
    End Sub

    Private Sub DrawMostUsedWords(ByVal sender As Object, ByVal e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.Clear(Color.FromArgb(32, 32, 35))

        ' Draw border
        Using path As GraphicsPath = GetRoundedRectPath(New Rectangle(0, 0, pnlMostUsed.Width - 1, pnlMostUsed.Height - 1), 8)
            Using p As New Pen(Color.FromArgb(48, 48, 52), 1)
                g.DrawPath(p, path)
            End Using
        End Using

        ' Title
        Using fontTitle As New Font("Segoe UI", 9.5!, FontStyle.Bold)
            Using brushTitle As New SolidBrush(Color.White)
                g.DrawString("🔝 Most Used Words", fontTitle, brushTitle, New PointF(15, 15))
            End Using
        End Using

        ' Sort word frequency
        Dim sortedWords As New List(Of KeyValuePair(Of String, Integer))()
        For Each kvp In AnalyticsEngine.WordCounts
            sortedWords.Add(kvp)
        Next
        sortedWords.Sort(Function(a, b) b.Value.CompareTo(a.Value))

        Dim itemsToShow As Integer = Math.Min(5, sortedWords.Count)
        Dim maxCount As Integer = If(itemsToShow > 0, sortedWords(0).Value, 1)

        If itemsToShow = 0 Then
            Using fontEmpty As New Font("Segoe UI", 9.0!, FontStyle.Italic)
                Using brushEmpty As New SolidBrush(Color.Gray)
                    g.DrawString("No words typed yet.", fontEmpty, brushEmpty, New PointF(15, 50))
                End Using
            End Using
        Else
            Dim startY As Integer = 45
            For i As Integer = 0 To itemsToShow - 1
                Dim word As String = sortedWords(i).Key
                Dim count As Integer = sortedWords(i).Value

                ' Draw word and count
                Using fontText As New Font("Nirmala UI", 9.0!, FontStyle.Regular)
                    Using brushText As New SolidBrush(Color.FromArgb(220, 220, 220))
                        g.DrawString(word & " (" & count & ")", fontText, brushText, New PointF(15, startY))
                    End Using
                End Using

                ' Draw frequency horizontal progress bar
                Dim barWidth As Integer = CInt((CDbl(count) / CDbl(maxCount)) * 200.0)
                Using brushBar As New SolidBrush(Color.FromArgb(0, 180, 137))
                    g.FillRectangle(brushBar, 15, startY + 18, barWidth, 4)
                End Using

                startY += 32
            Next
        End If
    End Sub

    Private Sub DrawTipsCard(ByVal g As Graphics, ByVal rect As Rectangle, ByVal text As String)
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.Clear(Color.FromArgb(32, 32, 35))

        ' Border
        Using path As GraphicsPath = GetRoundedRectPath(New Rectangle(0, 0, rect.Width - 1, rect.Height - 1), 8)
            Using p As New Pen(Color.FromArgb(48, 48, 52), 1)
                g.DrawPath(p, path)
            End Using
        End Using

        ' Render Tips
        Using fontTips As New Font("Segoe UI", 9.0!, FontStyle.Regular)
            Using brushTips As New SolidBrush(Color.FromArgb(200, 200, 200))
                g.DrawString(text, fontTips, brushTips, New RectangleF(15, 15, rect.Width - 30, rect.Height - 30))
            End Using
        End Using
    End Sub

    Private Function GetTypingTips(ByVal wpm As Integer, ByVal accuracy As Double) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("💡 AI Typing Habits & Advice:")
        sb.AppendLine()
        
        If accuracy < 90 Then
            sb.AppendLine("• Improve Precision: Your accuracy is " & accuracy.ToString("F1") & "%. Try slowing down slightly to form better muscle memory. High accuracy triggers faster speeds later.")
        Else
            sb.AppendLine("• Steady Control: Your accuracy of " & accuracy.ToString("F1") & "% is perfect! You can confidently try typing faster.")
        End If
        sb.AppendLine()

        If wpm < 30 Then
            sb.AppendLine("• Rhythm Check: Focus on steady keystroke flow. Minimize long breaks between sentences.")
        ElseIf wpm >= 30 AndAlso wpm < 50 Then
            sb.AppendLine("• Next-Word Prediction: Trust the candidate bar suggestions! Using them reduces keystrokes.")
        Else
            sb.AppendLine("• Elite Status: Speed of " & wpm & " WPM is excellent! Use text macros for fast replies.")
        End If

        Return sb.ToString()
    End Function

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
