Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Collections.Generic

Public Class DictionaryManagerWindow
    Inherits Form

    Private pnlTitle As Panel
    Private lblTitle As Label
    Private btnClose As Button

    Private lstWords As ListBox
    Private txtSearch As TextBox
    Private txtNewWord As TextBox
    Private btnAdd As Button
    Private btnDelete As Button
    Private btnClear As Button

    Private userWordsList As New List(Of String)()

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        ' Form Setup
        Me.Text = "Custom Dictionary Manager"
        Me.Size = New Size(400, 360)
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
            .Text = "📖 Custom Dictionary Manager",
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

        ' Search Input
        Dim lblSearch As New Label() With {
            .Text = "Filter words:",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(15, 52),
            .Size = New Size(100, 18)
        }
        Me.Controls.Add(lblSearch)

        txtSearch = New TextBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(15, 72),
            .Size = New Size(370, 24)
        }
        AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
        Me.Controls.Add(txtSearch)

        ' Word ListBox
        lstWords = New ListBox() With {
            .Font = New Font("Nirmala UI", 10.0!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(28, 28, 30),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(15, 105),
            .Size = New Size(245, 180),
            .DrawMode = DrawMode.OwnerDrawFixed,
            .ItemHeight = 22
        }
        AddHandler lstWords.DrawItem, AddressOf LstWords_DrawItem
        Me.Controls.Add(lstWords)

        ' Add Word Controls
        Dim lblAdd As New Label() With {
            .Text = "Add word:",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(15, 295),
            .Size = New Size(100, 18)
        }
        Me.Controls.Add(lblAdd)

        txtNewWord = New TextBox() With {
            .Font = New Font("Nirmala UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(15, 315),
            .Size = New Size(245, 26)
        }
        Me.Controls.Add(txtNewWord)

        btnAdd = New Button() With {
            .Text = "➕ Add",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(0, 180, 137),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(110, 26),
            .Location = New Point(275, 315),
            .Cursor = Cursors.Hand
        }
        btnAdd.FlatAppearance.BorderSize = 0
        AddHandler btnAdd.Click, AddressOf BtnAdd_Click
        Me.Controls.Add(btnAdd)

        ' Side Actions
        btnDelete = New Button() With {
            .Text = "🗑️ Delete Selected",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(222, 75, 57),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(110, 32),
            .Location = New Point(275, 105),
            .Cursor = Cursors.Hand
        }
        btnDelete.FlatAppearance.BorderSize = 0
        AddHandler btnDelete.Click, AddressOf BtnDelete_Click
        Me.Controls.Add(btnDelete)

        btnClear = New Button() With {
            .Text = "🧹 Clear All",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(48, 48, 50),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(110, 32),
            .Location = New Point(275, 150),
            .Cursor = Cursors.Hand
        }
        btnClear.FlatAppearance.BorderSize = 0
        AddHandler btnClear.Click, AddressOf BtnClear_Click
        Me.Controls.Add(btnClear)

        ' Initial Load
        LoadDictionary()
        ApplyRoundedCorners()
    End Sub

    Private Sub LoadDictionary()
        userWordsList = SuggestionEngine.GetAllUserWords()
        userWordsList.Sort()
        FilterAndShowWords()
    End Sub

    Private Sub FilterAndShowWords()
        lstWords.Items.Clear()
        Dim filter As String = txtSearch.Text.Trim().ToLower()

        For Each word In userWordsList
            If String.IsNullOrEmpty(filter) OrElse word.ToLower().Contains(filter) Then
                lstWords.Items.Add(word)
            End If
        Next
    End Sub

    Private Sub TxtSearch_TextChanged(sender As Object, e As EventArgs)
        FilterAndShowWords()
    End Sub

    Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
        Dim word As String = txtNewWord.Text.Trim()
        If String.IsNullOrEmpty(word) Then Return

        SuggestionEngine.AddCustomWord(word)
        txtNewWord.Clear()
        LoadDictionary()
        MessageBox.Show("Word '" & word & "' added to custom dictionary.", "Dictionary Updated", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
        If lstWords.SelectedItem Is Nothing Then
            MessageBox.Show("Please select a word to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim word As String = lstWords.SelectedItem.ToString()
        If MessageBox.Show("Are you sure you want to delete '" & word & "' from custom dictionary?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            SuggestionEngine.RemoveCustomWord(word)
            LoadDictionary()
        End If
    End Sub

    Private Sub BtnClear_Click(sender As Object, e As EventArgs)
        If userWordsList.Count = 0 Then Return
        If MessageBox.Show("Are you sure you want to delete ALL custom words? This cannot be undone.", "Confirm Clear All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            For Each word In userWordsList
                SuggestionEngine.RemoveCustomWord(word)
            Next
            LoadDictionary()
        End If
    End Sub

    Private Sub LstWords_DrawItem(sender As Object, e As DrawItemEventArgs)
        If e.Index < 0 Then Return
        e.DrawBackground()

        Dim isSelected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
        Dim text As String = lstWords.Items(e.Index).ToString()

        Using bgBrush As New SolidBrush(If(isSelected, Color.FromArgb(0, 180, 137), Color.FromArgb(28, 28, 30)))
            e.Graphics.FillRectangle(bgBrush, e.Bounds)
        End Using

        Using textBrush As New SolidBrush(Color.White)
            Dim sf As New StringFormat() With {.LineAlignment = StringAlignment.Center}
            Dim textBounds As New RectangleF(e.Bounds.X + 5, e.Bounds.Y, e.Bounds.Width - 10, e.Bounds.Height)
            e.Graphics.DrawString(text, lstWords.Font, textBrush, textBounds, sf)
        End Using

        e.DrawFocusRectangle()
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
