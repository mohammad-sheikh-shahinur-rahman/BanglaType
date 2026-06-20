Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Collections
Imports System.Collections.Generic

Public Class FloatingKeyboardWindow
    Inherits Form

    Private pnlTitle As Panel
    Private lblTitle As Label
    Private btnClose As Button
    Private btnCompact As Button

    Private row1Panel As FlowLayoutPanel
    Private row2Panel As FlowLayoutPanel
    Private row3Panel As FlowLayoutPanel
    Private row4Panel As FlowLayoutPanel

    Private chkShiftActive As Boolean = False
    Private chkCapsActive As Boolean = False
    Private isCompactMode As Boolean = False

    ' Swipe/Gesture typing states
    Private isDraggingPath As Boolean = False
    Private gesturePath As New List(Of String)()
    Private keyOriginalColors As New Dictionary(Of Button, Color)()

    Private Structure KeyInfo
        Public Text As String
        Public VkCode As Integer
        Public Width As Integer
        Public Row As Integer
        Public Sub New(ByVal t As String, ByVal vk As Integer, ByVal w As Integer, ByVal r As Integer)
            Text = t
            VkCode = vk
            Width = w
            Row = r
        End Sub
    End Structure

    Private keysList As New List(Of KeyInfo)()

    Public Sub New()
        InitializeComponent()
    End Sub

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

    Private Sub InitializeComponent()
        Me.Text = "BanglaType Keyboard View"
        Me.Size = New Size(620, 220)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.TopMost = True
        Me.ShowInTaskbar = False
        Me.DoubleBuffered = True
        Me.BackColor = Color.FromArgb(20, 20, 22)

        ' Header Panel
        pnlTitle = New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 30,
            .BackColor = Color.FromArgb(28, 28, 30)
        }
        AddHandler pnlTitle.MouseDown, AddressOf Header_MouseDown

        lblTitle = New Label() With {
            .Text = "BanglaType Floating Keyboard",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(10, 7),
            .AutoSize = True
        }
        AddHandler lblTitle.MouseDown, AddressOf Header_MouseDown

        btnCompact = New Button() With {
            .Text = "📱 Compact Mode",
            .Font = New Font("Segoe UI", 8.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(48, 48, 50),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(110, 24),
            .Location = New Point(Me.Width - 150, 3),
            .Cursor = Cursors.Hand
        }
        btnCompact.FlatAppearance.BorderSize = 0
        AddHandler btnCompact.Click, Sub() ToggleCompactMode()

        btnClose = New Button() With {
            .Text = "✕",
            .Font = New Font("Segoe UI", 8.0!, FontStyle.Regular),
            .ForeColor = Color.DarkGray,
            .BackColor = Color.Transparent,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(30, 30),
            .Location = New Point(Me.Width - 30, 0),
            .Cursor = Cursors.Hand
        }
        btnClose.FlatAppearance.BorderSize = 0
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35)
        btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(241, 112, 122)
        AddHandler btnClose.Click, Sub() Me.Hide()

        pnlTitle.Controls.Add(lblTitle)
        pnlTitle.Controls.Add(btnCompact)
        pnlTitle.Controls.Add(btnClose)
        Me.Controls.Add(pnlTitle)

        ' Row panels
        row1Panel = New FlowLayoutPanel() With {
            .Location = New Point(6, 36),
            .Size = New Size(608, 40),
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Margin = New Padding(0)
        }

        row2Panel = New FlowLayoutPanel() With {
            .Location = New Point(6, 78),
            .Size = New Size(608, 40),
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Margin = New Padding(0)
        }

        row3Panel = New FlowLayoutPanel() With {
            .Location = New Point(6, 120),
            .Size = New Size(608, 40),
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Margin = New Padding(0)
        }

        row4Panel = New FlowLayoutPanel() With {
            .Location = New Point(6, 162),
            .Size = New Size(608, 40),
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Margin = New Padding(0)
        }

        Me.Controls.Add(row1Panel)
        Me.Controls.Add(row2Panel)
        Me.Controls.Add(row3Panel)
        Me.Controls.Add(row4Panel)

        PopulateKeys()
        CreateKeyButtons()
        UpdateKeyLabels()
        ApplyRoundedCorners()
    End Sub

    Private Sub PopulateKeys()
        keysList.Clear()
        ' Row 1
        keysList.Add(New KeyInfo("Esc", 27, 36, 1))
        keysList.Add(New KeyInfo("`", 192, 34, 1))
        keysList.Add(New KeyInfo("1", 49, 34, 1))
        keysList.Add(New KeyInfo("2", 50, 34, 1))
        keysList.Add(New KeyInfo("3", 51, 34, 1))
        keysList.Add(New KeyInfo("4", 52, 34, 1))
        keysList.Add(New KeyInfo("5", 53, 34, 1))
        keysList.Add(New KeyInfo("6", 54, 34, 1))
        keysList.Add(New KeyInfo("7", 55, 34, 1))
        keysList.Add(New KeyInfo("8", 56, 34, 1))
        keysList.Add(New KeyInfo("9", 57, 34, 1))
        keysList.Add(New KeyInfo("0", 48, 34, 1))
        keysList.Add(New KeyInfo("-", 189, 34, 1))
        keysList.Add(New KeyInfo("=", 187, 34, 1))
        keysList.Add(New KeyInfo("Back", 8, 54, 1))

        ' Row 2
        keysList.Add(New KeyInfo("Tab", 9, 48, 2))
        keysList.Add(New KeyInfo("Q", 81, 34, 2))
        keysList.Add(New KeyInfo("W", 87, 34, 2))
        keysList.Add(New KeyInfo("E", 69, 34, 2))
        keysList.Add(New KeyInfo("R", 82, 34, 2))
        keysList.Add(New KeyInfo("T", 84, 34, 2))
        keysList.Add(New KeyInfo("Y", 89, 34, 2))
        keysList.Add(New KeyInfo("U", 85, 34, 2))
        keysList.Add(New KeyInfo("I", 73, 34, 2))
        keysList.Add(New KeyInfo("O", 79, 34, 2))
        keysList.Add(New KeyInfo("P", 80, 34, 2))
        keysList.Add(New KeyInfo("[", 219, 34, 2))
        keysList.Add(New KeyInfo("]", 221, 34, 2))
        keysList.Add(New KeyInfo("\", 220, 40, 2))

        ' Row 3
        keysList.Add(New KeyInfo("Caps", 20, 56, 3))
        keysList.Add(New KeyInfo("A", 65, 34, 3))
        keysList.Add(New KeyInfo("S", 83, 34, 3))
        keysList.Add(New KeyInfo("D", 68, 34, 3))
        keysList.Add(New KeyInfo("F", 70, 34, 3))
        keysList.Add(New KeyInfo("G", 71, 34, 3))
        keysList.Add(New KeyInfo("H", 72, 34, 3))
        keysList.Add(New KeyInfo("J", 74, 34, 3))
        keysList.Add(New KeyInfo("K", 75, 34, 3))
        keysList.Add(New KeyInfo("L", 76, 34, 3))
        keysList.Add(New KeyInfo(";", 186, 34, 3))
        keysList.Add(New KeyInfo("'", 222, 34, 3))
        keysList.Add(New KeyInfo("Enter", 13, 62, 3))

        ' Row 4
        keysList.Add(New KeyInfo("Shift", 160, 70, 4))
        keysList.Add(New KeyInfo("Z", 90, 34, 4))
        keysList.Add(New KeyInfo("X", 88, 34, 4))
        keysList.Add(New KeyInfo("C", 67, 34, 4))
        keysList.Add(New KeyInfo("V", 86, 34, 4))
        keysList.Add(New KeyInfo("B", 66, 34, 4))
        keysList.Add(New KeyInfo("N", 78, 34, 4))
        keysList.Add(New KeyInfo("M", 77, 34, 4))
        keysList.Add(New KeyInfo(",", 188, 34, 4))
        keysList.Add(New KeyInfo(".", 190, 34, 4))
        keysList.Add(New KeyInfo("/", 191, 34, 4))
        keysList.Add(New KeyInfo("Space/Back", 32, 120, 4)) ' Smart space + back merge key
        keysList.Add(New KeyInfo("Mode", 999, 54, 4))
    End Sub

    Private Function GetKeyWidth(ByVal defaultWidth As Integer) As Integer
        If isCompactMode Then
            Select Case defaultWidth
                Case 34 : Return 21
                Case 36 : Return 21
                Case 40 : Return 25
                Case 48 : Return 28
                Case 54 : Return 32
                Case 56 : Return 34
                Case 62 : Return 36
                Case 70 : Return 40
                Case 120 : Return 68
                Case Else : Return CInt(defaultWidth * 0.6)
            End Select
        End If
        Return defaultWidth
    End Function

    Private Sub CreateKeyButtons()
        For Each key In keysList
            Dim calculatedWidth As Integer = GetKeyWidth(key.Width)
            Dim btn As New Button() With {
                .Tag = key.VkCode,
                .Size = New Size(calculatedWidth, 34),
                .Margin = New Padding(1, 1, 1, 1),
                .FlatStyle = FlatStyle.Flat,
                .Font = New Font("Segoe UI", 8.0!, FontStyle.Bold),
                .ForeColor = Color.White,
                .BackColor = Color.FromArgb(38, 38, 42),
                .Cursor = Cursors.Hand
            }
            btn.FlatAppearance.BorderSize = 1
            btn.FlatAppearance.BorderColor = Color.FromArgb(48, 48, 52)
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 52, 56)
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 70, 74)

            ' Hook dragging / mouse events for swipe gesture typing & smart space split click
            AddHandler btn.MouseDown, AddressOf Key_MouseDown
            AddHandler btn.MouseEnter, AddressOf Key_MouseEnter
            AddHandler btn.MouseUp, AddressOf Key_MouseUp

            Select Case key.Row
                Case 1 : row1Panel.Controls.Add(btn)
                Case 2 : row2Panel.Controls.Add(btn)
                Case 3 : row3Panel.Controls.Add(btn)
                Case 4 : row4Panel.Controls.Add(btn)
            End Select
        Next
    End Sub

    Private Sub ToggleCompactMode()
        isCompactMode = Not isCompactMode

        If isCompactMode Then
            Me.Size = New Size(390, 220)
            btnCompact.Text = "🖥️ Full Mode"
            Dim wa As Rectangle = Screen.PrimaryScreen.WorkingArea
            Me.Location = New Point(wa.Right - Me.Width - 12, wa.Bottom - Me.Height - 12)
        Else
            Me.Size = New Size(620, 220)
            btnCompact.Text = "📱 Compact Mode"
            Me.CenterToScreen()
        End If

        btnCompact.Location = New Point(Me.Width - 150, 3)
        btnClose.Location = New Point(Me.Width - 30, 0)

        ' Redraw panels
        row1Panel.Size = New Size(Me.Width - 12, 40)
        row2Panel.Size = New Size(Me.Width - 12, 40)
        row3Panel.Size = New Size(Me.Width - 12, 40)
        row4Panel.Size = New Size(Me.Width - 12, 40)

        row1Panel.Controls.Clear()
        row2Panel.Controls.Clear()
        row3Panel.Controls.Clear()
        row4Panel.Controls.Clear()

        CreateKeyButtons()
        UpdateKeyLabels()
        ApplyRoundedCorners()
    End Sub

    Public Sub UpdateKeyLabels()
        Dim isShift As Boolean = chkShiftActive
        Dim isCaps As Boolean = chkCapsActive
        Dim shiftActive As Boolean = isShift Xor isCaps

        Dim useFixedBangla As Boolean = isActivated AndAlso (Not MainUI.isPhoneticSelected) AndAlso (Not MainUI.isAvroSelected)

        For Each ctrl As Control In row1Panel.Controls
            UpdateSingleButton(ctrl, useFixedBangla, shiftActive)
        Next
        For Each ctrl As Control In row2Panel.Controls
            UpdateSingleButton(ctrl, useFixedBangla, shiftActive)
        Next
        For Each ctrl As Control In row3Panel.Controls
            UpdateSingleButton(ctrl, useFixedBangla, shiftActive)
        Next
        For Each ctrl As Control In row4Panel.Controls
            UpdateSingleButton(ctrl, useFixedBangla, shiftActive)
        Next

        ' Colors for active modifiers
        For Each ctrl As Control In row3Panel.Controls
            If ctrl.Tag IsNot Nothing AndAlso Convert.ToInt32(ctrl.Tag) = 20 Then
                ctrl.BackColor = If(chkCapsActive, Color.FromArgb(0, 180, 137), Color.FromArgb(38, 38, 42))
            End If
        Next
        For Each ctrl As Control In row4Panel.Controls
            If ctrl.Tag IsNot Nothing AndAlso Convert.ToInt32(ctrl.Tag) = 160 Then
                ctrl.BackColor = If(chkShiftActive, Color.FromArgb(0, 180, 137), Color.FromArgb(38, 38, 42))
            End If
        Next

        Dim main As MainUI = CType(Application.OpenForms("MainUI"), MainUI)
        If main IsNot Nothing Then
            lblTitle.Text = "BanglaType Keyboard (" & main.btnMode.Text & ")"
        End If
    End Sub

    Private Sub UpdateSingleButton(ByVal ctrl As Control, ByVal useFixedBangla As Boolean, ByVal shiftActive As Boolean)
        Dim btn As Button = TryCast(ctrl, Button)
        If btn IsNot Nothing AndAlso btn.Tag IsNot Nothing Then
            Dim vkCode As Integer = Convert.ToInt32(btn.Tag)

            If vkCode = 27 Then
                btn.Text = "Esc"
            ElseIf vkCode = 8 Then
                btn.Text = "Bk"
            ElseIf vkCode = 9 Then
                btn.Text = "Tab"
            ElseIf vkCode = 20 Then
                btn.Text = "Caps"
            ElseIf vkCode = 13 Then
                btn.Text = "Ent"
            ElseIf vkCode = 160 Then
                btn.Text = "Shft"
            ElseIf vkCode = 32 Then
                btn.Text = "Space/Bk"
            ElseIf vkCode = 999 Then
                btn.Text = "Mode"
            Else
                If useFixedBangla AndAlso Keyboard.Layout IsNot Nothing AndAlso Keyboard.crlay >= 0 AndAlso Keyboard.crlay < Keyboard.Layout.Length AndAlso Keyboard.Layout(Keyboard.crlay).Key.ContainsKey(vkCode) Then
                    Dim arr As ArrayList = Keyboard.Layout(Keyboard.crlay).Key(vkCode)
                    If arr IsNot Nothing AndAlso arr.Count >= 2 Then
                        Dim chr As String = ""
                        If shiftActive Then
                            chr = Convert.ToString(arr(1))
                        Else
                            chr = Convert.ToString(arr(0))
                        End If
                        btn.Text = If(String.IsNullOrEmpty(chr), GetEnglishChar(vkCode, shiftActive), chr)
                        btn.Font = New Font("Nirmala UI", 8.5!, FontStyle.Bold)
                    End If
                Else
                    Dim chr As String = GetEnglishChar(vkCode, shiftActive)
                    btn.Text = chr
                    btn.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
                End If
            End If
        End If
    End Sub

    Private Function GetEnglishChar(ByVal vk As Integer, ByVal shift As Boolean) As String
        Select Case vk
            Case 48 : Return If(shift, ")", "0")
            Case 49 : Return If(shift, "!", "1")
            Case 50 : Return If(shift, "@", "2")
            Case 51 : Return If(shift, "#", "3")
            Case 52 : Return If(shift, "$", "4")
            Case 53 : Return If(shift, "%", "5")
            Case 54 : Return If(shift, "^", "6")
            Case 55 : Return If(shift, "&", "7")
            Case 56 : Return If(shift, "*", "8")
            Case 57 : Return If(shift, "(", "9")
            Case 65 : Return If(shift, "A", "a")
            Case 66 : Return If(shift, "B", "b")
            Case 67 : Return If(shift, "C", "c")
            Case 68 : Return If(shift, "D", "d")
            Case 69 : Return If(shift, "E", "e")
            Case 70 : Return If(shift, "F", "f")
            Case 71 : Return If(shift, "G", "g")
            Case 72 : Return If(shift, "H", "h")
            Case 73 : Return If(shift, "I", "i")
            Case 74 : Return If(shift, "J", "j")
            Case 75 : Return If(shift, "K", "k")
            Case 76 : Return If(shift, "L", "l")
            Case 77 : Return If(shift, "M", "m")
            Case 78 : Return If(shift, "N", "n")
            Case 79 : Return If(shift, "O", "o")
            Case 80 : Return If(shift, "P", "p")
            Case 81 : Return If(shift, "Q", "q")
            Case 82 : Return If(shift, "R", "r")
            Case 83 : Return If(shift, "S", "s")
            Case 84 : Return If(shift, "T", "t")
            Case 85 : Return If(shift, "U", "u")
            Case 86 : Return If(shift, "V", "v")
            Case 87 : Return If(shift, "W", "w")
            Case 88 : Return If(shift, "X", "x")
            Case 89 : Return If(shift, "Y", "y")
            Case 90 : Return If(shift, "Z", "z")
            Case 186 : Return If(shift, ":", ";")
            Case 187 : Return If(shift, "+", "=")
            Case 188 : Return If(shift, "<", ",")
            Case 189 : Return If(shift, "_", "-")
            Case 190 : Return If(shift, ">", ".")
            Case 191 : Return If(shift, "?", "/")
            Case 192 : Return If(shift, "~", "`")
            Case 219 : Return If(shift, "{", "[")
            Case 220 : Return If(shift, "|", "\")
            Case 221 : Return If(shift, "}", "]")
            Case 222 : Return If(shift, """", "'")
            Case Else : Return ""
        End Select
    End Function

    ' Swipe Gesture typing / Split click triggers
    Private Sub Key_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            isDraggingPath = True
            gesturePath.Clear()
            keyOriginalColors.Clear()
            
            Dim btn As Button = CType(sender, Button)
            If btn.Tag IsNot Nothing Then
                Dim val As Integer = Convert.ToInt32(btn.Tag)
                If val >= 65 AndAlso val <= 90 Then
                    gesturePath.Add(btn.Text.Trim().ToLower())
                    keyOriginalColors(btn) = btn.BackColor
                    btn.BackColor = Color.FromArgb(0, 180, 137)
                End If
            End If
        End If
    End Sub

    Private Sub Key_MouseEnter(ByVal sender As Object, ByVal e As EventArgs)
        If isDraggingPath Then
            Dim btn As Button = CType(sender, Button)
            If btn.Tag IsNot Nothing Then
                Dim val As Integer = Convert.ToInt32(btn.Tag)
                If val >= 65 AndAlso val <= 90 Then
                    Dim letter As String = btn.Text.Trim().ToLower()
                    If letter.Length = 1 AndAlso (gesturePath.Count = 0 OrElse gesturePath(gesturePath.Count - 1) <> letter) Then
                        gesturePath.Add(letter)
                        If Not keyOriginalColors.ContainsKey(btn) Then
                            keyOriginalColors(btn) = btn.BackColor
                        End If
                        btn.BackColor = Color.FromArgb(0, 180, 137)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub Key_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim btn As Button = CType(sender, Button)
        If btn Is Nothing OrElse btn.Tag Is Nothing Then Return
        Dim vkCode As Integer = Convert.ToInt32(btn.Tag)

        If isDraggingPath Then
            isDraggingPath = False
            RestoreKeyColors()

            If gesturePath.Count >= 3 Then
                Dim swipeStr As String = String.Join("", gesturePath.ToArray())
                Dim bestWord As String = FindBestSwipeMatch(swipeStr)
                If Not String.IsNullOrEmpty(bestWord) Then
                    SendWord(bestWord)
                    gesturePath.Clear()
                    Return
                End If
            End If
        End If

        ' Fallback to single click
        ProcessSingleKey(vkCode, e)
    End Sub

    Private Sub RestoreKeyColors()
        For Each kvp In keyOriginalColors
            kvp.Key.BackColor = kvp.Value
        Next
        keyOriginalColors.Clear()
    End Sub

    Private Sub ProcessSingleKey(ByVal vkCode As Integer, ByVal e As MouseEventArgs)
        If vkCode = 32 Then
            ' Space / Back merge key
            If e.Button = MouseButtons.Right Then
                Keyboard.SimulateVirtualKey(8, False) ' Send Backspace
            Else
                Keyboard.SimulateVirtualKey(32, False) ' Send Space
            End If
        ElseIf vkCode = 20 Then
            chkCapsActive = Not chkCapsActive
            UpdateKeyLabels()
        ElseIf vkCode = 160 Then
            chkShiftActive = Not chkShiftActive
            UpdateKeyLabels()
        ElseIf vkCode = 999 Then
            Dim main As MainUI = CType(Application.OpenForms("MainUI"), MainUI)
            If main IsNot Nothing Then
                main.CycleMode()
                UpdateKeyLabels()
            End If
        Else
            Dim shiftState As Boolean = chkShiftActive Xor chkCapsActive
            Keyboard.SimulateVirtualKey(vkCode, shiftState)
            If chkShiftActive Then
                chkShiftActive = False
                UpdateKeyLabels()
            End If
        End If
    End Sub

    Private Function FindBestSwipeMatch(ByVal pattern As String) As String
        If pattern.Length < 3 Then Return ""

        ' Transliterate pattern directly if in phonetic/Avro mode
        Dim main As MainUI = CType(Application.OpenForms("MainUI"), MainUI)
        Dim isPhoneticMode As Boolean = (MainUI.isPhonetic AndAlso MainUI.isPhoneticSelected) OrElse MainUI.isAvroSelected
        
        If isPhoneticMode AndAlso MainUI.AvroEngine IsNot Nothing Then
            Dim translated As String = MainUI.AvroEngine.Parse(pattern)
            If Not String.IsNullOrEmpty(translated) Then
                Return translated
            End If
        End If

        ' Fallback: match from local analytics database words
        Dim candidates As New List(Of String)()
        For Each kvp In AnalyticsEngine.WordCounts
            Dim w As String = kvp.Key
            If w.Length >= 2 AndAlso w(0) = pattern(0) Then
                candidates.Add(w)
            End If
        Next

        Dim bestMatch As String = ""
        Dim bestScore As Double = 0.0

        For Each word In candidates
            If IsSubsequence(word.ToLower(), pattern) Then
                Dim score As Double = CDbl(word.Length) / CDbl(pattern.Length)
                If score > bestScore Then
                    bestScore = score
                    bestMatch = word
                End If
            End If
        Next

        Return bestMatch
    End Function

    Private Function IsSubsequence(ByVal word As String, ByVal pattern As String) As Boolean
        Dim wIdx As Integer = 0
        Dim pIdx As Integer = 0
        While wIdx < word.Length AndAlso pIdx < pattern.Length
            If word(wIdx) = pattern(pIdx) Then
                wIdx += 1
            End If
            pIdx += 1
        End While
        Return wIdx = word.Length
    End Function

    Private Sub SendWord(ByVal word As String)
        MainUI.PasteText(word & " ")
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
