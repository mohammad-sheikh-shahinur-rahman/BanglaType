Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Text

''' <summary>
''' BanglaType Notepad with a built-in, self-contained Banglish (Avro Phonetic)
''' typing engine plus a full set of advanced features: live word suggestions,
''' auto-correct, text-expansion macros, Bijoy ANSI export, find &amp; replace,
''' zoom, print, recent files, dark mode, AI assist and voice typing.
'''
''' Unlike the rest of the app, the notepad does NOT depend on the global keyboard
''' hook being toggled on: it transliterates roman input to Bangla locally,
''' word-by-word, directly inside its editor. While the notepad is focused and
''' phonetic mode is on, the global hook is bypassed so the two engines never
''' fight over the same keystroke.
''' </summary>
Public Class NotepadWindow
    Inherits Form

    Private mainMenu As MenuStrip
    Private fileMenu, editMenu, formatMenu, viewMenu, banglaMenu As ToolStripMenuItem
    Private mnuRecent As ToolStripMenuItem

    Private mnuPhonetic, mnuSuggestions, mnuAutoCorrect, mnuMacros As ToolStripMenuItem
    Private mnuWordWrap, mnuDarkMode As ToolStripMenuItem

    Private txtEditor As TextBox
    Private statusBar As StatusStrip
    Private statusLabel As ToolStripStatusLabel
    Private modeLabel As ToolStripStatusLabel

    Private currentFilePath As String = ""
    Private isModified As Boolean = False

    ' --- Banglish (Avro phonetic) typing state ---
    Private ReadOnly parser As AvroParser
    Private phoneticMode As Boolean = True
    Private romanBuffer As String = ""
    Private wordStart As Integer = 0
    Private renderedLen As Integer = 0

    ' --- advanced feature toggles ---
    Private suggestionsEnabled As Boolean = True
    Private autoCorrectEnabled As Boolean = True
    Private macrosEnabled As Boolean = True
    Private isDark As Boolean = False

    ' --- suggestion popup (reuses the app's non-activating window) ---
    Private suggWin As SuggestionWindow

    ' --- printing ---
    Private WithEvents printDoc As New PrintDocument()
    Private printText As String = ""
    Private printCharIndex As Integer = 0

    Private Const RecentMax As Integer = 8

    Public Sub New()
        parser = If(MainUI.AvroEngine, New AvroParser())
        Try
            SuggestionEngine.EnsureLoaded()
            MacroEngine.Load()
        Catch
        End Try
        suggWin = New SuggestionWindow()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(820, 620)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimumSize = New Size(420, 320)
        Me.KeyPreview = True

        ' Main Editor TextBox
        txtEditor = New TextBox() With {
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .Dock = DockStyle.Fill,
            .Font = New Font("Nirmala UI", 12.0!, FontStyle.Regular),
            .BorderStyle = BorderStyle.None,
            .AcceptsTab = True,
            .HideSelection = False
        }
        AddHandler txtEditor.TextChanged, AddressOf TxtEditor_TextChanged
        AddHandler txtEditor.KeyPress, AddressOf TxtEditor_KeyPress
        AddHandler txtEditor.KeyDown, AddressOf TxtEditor_KeyDown
        AddHandler txtEditor.MouseUp, AddressOf TxtEditor_MouseUp
        AddHandler txtEditor.MouseWheel, AddressOf TxtEditor_MouseWheel

        BuildMenu()

        ' Status Strip
        statusBar = New StatusStrip()
        statusLabel = New ToolStripStatusLabel() With {
            .Text = "Lines: 1 | Words: 0 | Characters: 0",
            .Spring = True,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        modeLabel = New ToolStripStatusLabel() With {.TextAlign = ContentAlignment.MiddleRight}
        statusBar.Items.Add(statusLabel)
        statusBar.Items.Add(modeLabel)

        Me.Controls.Add(txtEditor)
        Me.Controls.Add(mainMenu)
        Me.Controls.Add(statusBar)
        Me.MainMenuStrip = mainMenu

        UpdateTitle()
        UpdateStatus()
        UpdateModeIndicator()
        ApplyTheme()
    End Sub

    Private Sub BuildMenu()
        mainMenu = New MenuStrip()

        ' File
        fileMenu = New ToolStripMenuItem("&File")
        mnuRecent = New ToolStripMenuItem("Recent &Files")
        fileMenu.DropDownItems.AddRange(New ToolStripItem() {
            New ToolStripMenuItem("&New", Nothing, AddressOf MnuNew_Click, Keys.Control Or Keys.N),
            New ToolStripMenuItem("&Open...", Nothing, AddressOf MnuOpen_Click, Keys.Control Or Keys.O),
            mnuRecent,
            New ToolStripMenuItem("&Save", Nothing, AddressOf MnuSave_Click, Keys.Control Or Keys.S),
            New ToolStripMenuItem("Save &As...", Nothing, AddressOf MnuSaveAs_Click),
            New ToolStripSeparator(),
            New ToolStripMenuItem("Print Pre&view...", Nothing, AddressOf MnuPrintPreview_Click),
            New ToolStripMenuItem("&Print...", Nothing, AddressOf MnuPrint_Click, Keys.Control Or Keys.P),
            New ToolStripSeparator(),
            New ToolStripMenuItem("E&xit", Nothing, AddressOf MnuExit_Click)})

        ' Edit
        editMenu = New ToolStripMenuItem("&Edit")
        editMenu.DropDownItems.AddRange(New ToolStripItem() {
            New ToolStripMenuItem("&Undo", Nothing, AddressOf MnuUndo_Click, Keys.Control Or Keys.Z),
            New ToolStripSeparator(),
            New ToolStripMenuItem("Cu&t", Nothing, AddressOf MnuCut_Click, Keys.Control Or Keys.X),
            New ToolStripMenuItem("&Copy", Nothing, AddressOf MnuCopy_Click, Keys.Control Or Keys.C),
            New ToolStripMenuItem("&Paste", Nothing, AddressOf MnuPaste_Click, Keys.Control Or Keys.V),
            New ToolStripMenuItem("De&lete", Nothing, AddressOf MnuDelete_Click, Keys.Delete),
            New ToolStripSeparator(),
            New ToolStripMenuItem("&Find...", Nothing, AddressOf MnuFind_Click, Keys.Control Or Keys.F),
            New ToolStripMenuItem("&Replace...", Nothing, AddressOf MnuReplace_Click, Keys.Control Or Keys.H),
            New ToolStripSeparator(),
            New ToolStripMenuItem("Select &All", Nothing, AddressOf MnuSelectAll_Click, Keys.Control Or Keys.A),
            New ToolStripMenuItem("Time/&Date", Nothing, AddressOf MnuTimeDate_Click, Keys.F5)})

        ' Format
        formatMenu = New ToolStripMenuItem("F&ormat")
        mnuWordWrap = New ToolStripMenuItem("&Word Wrap", Nothing, AddressOf MnuWordWrap_Click) With {.Checked = True}
        formatMenu.DropDownItems.AddRange(New ToolStripItem() {
            mnuWordWrap,
            New ToolStripMenuItem("&Font...", Nothing, AddressOf MnuFont_Click),
            New ToolStripSeparator(),
            New ToolStripMenuItem("Zoom &In", Nothing, AddressOf MnuZoomIn_Click, Keys.Control Or Keys.Oemplus),
            New ToolStripMenuItem("Zoom &Out", Nothing, AddressOf MnuZoomOut_Click, Keys.Control Or Keys.OemMinus),
            New ToolStripMenuItem("&Reset Zoom", Nothing, AddressOf MnuZoomReset_Click, Keys.Control Or Keys.D0)})

        ' View
        viewMenu = New ToolStripMenuItem("&View")
        mnuDarkMode = New ToolStripMenuItem("&Dark Mode", Nothing, AddressOf MnuDarkMode_Click)
        viewMenu.DropDownItems.Add(mnuDarkMode)

        ' Bangla
        banglaMenu = New ToolStripMenuItem("&Bangla")
        mnuPhonetic = New ToolStripMenuItem("Phonetic (&Banglish) Typing", Nothing, AddressOf MnuPhonetic_Click, Keys.Control Or Keys.M) With {.Checked = phoneticMode}
        mnuSuggestions = New ToolStripMenuItem("Word &Suggestions", Nothing, AddressOf MnuSuggestions_Click) With {.Checked = suggestionsEnabled}
        mnuAutoCorrect = New ToolStripMenuItem("&Auto-correct", Nothing, AddressOf MnuAutoCorrect_Click) With {.Checked = autoCorrectEnabled}
        mnuMacros = New ToolStripMenuItem("Text &Expansion (Macros)", Nothing, AddressOf MnuMacros_Click) With {.Checked = macrosEnabled}
        banglaMenu.DropDownItems.AddRange(New ToolStripItem() {
            mnuPhonetic, mnuSuggestions, mnuAutoCorrect, mnuMacros,
            New ToolStripSeparator(),
            New ToolStripMenuItem("&Copy as Bijoy ANSI", Nothing, AddressOf MnuCopyBijoy_Click),
            New ToolStripMenuItem("Save as Bijoy ANSI...", Nothing, AddressOf MnuSaveBijoy_Click),
            New ToolStripSeparator(),
            New ToolStripMenuItem("AI Assist (&Gemini)...", Nothing, AddressOf MnuAI_Click),
            New ToolStripMenuItem("&Voice Typing", Nothing, AddressOf MnuVoice_Click),
            New ToolStripSeparator(),
            New ToolStripMenuItem("How to type Banglish...", Nothing, AddressOf MnuPhoneticHelp_Click)})

        mainMenu.Items.AddRange(New ToolStripItem() {fileMenu, editMenu, formatMenu, viewMenu, banglaMenu})

        RebuildRecentMenu()
    End Sub

    ' === Banglish (Avro phonetic) typing engine =========================

    Private Shared Function IsWordChar(ByVal ch As Char) As Boolean
        Return (ch >= "a"c AndAlso ch <= "z"c) OrElse
               (ch >= "A"c AndAlso ch <= "Z"c) OrElse
               (ch >= "0"c AndAlso ch <= "9"c)
    End Function

    Private Sub ResetComposition()
        romanBuffer = ""
        renderedLen = 0
        HideSuggestions()
        UpdateModeIndicator()
    End Sub

    Private Sub RenderBuffer()
        Dim bangla As String = If(romanBuffer.Length = 0, "", parser.Parse(romanBuffer))
        If wordStart < 0 OrElse wordStart > txtEditor.TextLength Then
            ResetComposition()
            Return
        End If
        If wordStart + renderedLen > txtEditor.TextLength Then
            renderedLen = txtEditor.TextLength - wordStart
        End If
        txtEditor.Select(wordStart, renderedLen)
        txtEditor.SelectedText = bangla
        renderedLen = bangla.Length
    End Sub

    Private Sub TxtEditor_KeyPress(sender As Object, e As KeyPressEventArgs)
        If Not phoneticMode Then Return
        Dim ch As Char = e.KeyChar

        If Char.IsControl(ch) Then
            ResetComposition()
            Return
        End If

        If IsWordChar(ch) Then
            If romanBuffer.Length = 0 Then
                wordStart = txtEditor.SelectionStart
                renderedLen = txtEditor.SelectionLength
            End If
            romanBuffer &= ch
            RenderBuffer()
            ShowSuggestions()
            UpdateModeIndicator()
            e.Handled = True
        Else
            ' Punctuation / symbol -> commit the word, then let the char insert normally.
            CommitWord()
            ResetComposition()
        End If
    End Sub

    Private Sub TxtEditor_KeyDown(sender As Object, e As KeyEventArgs)
        If Not phoneticMode Then Return

        ' Candidate-window navigation while it is visible.
        If suggWin IsNot Nothing AndAlso suggWin.Visible AndAlso suggWin.Count > 0 Then
            Select Case e.KeyCode
                Case Keys.Up
                    suggWin.MoveSelection(-1) : e.Handled = True : e.SuppressKeyPress = True : Return
                Case Keys.Down
                    suggWin.MoveSelection(1) : e.Handled = True : e.SuppressKeyPress = True : Return
                Case Keys.Tab
                    AcceptSuggestion(suggWin.SelectedIndex) : e.Handled = True : e.SuppressKeyPress = True : Return
                Case Keys.Escape
                    HideSuggestions() : e.Handled = True : e.SuppressKeyPress = True : Return
            End Select
        End If

        Select Case e.KeyCode
            Case Keys.Back
                If romanBuffer.Length > 0 Then
                    e.Handled = True
                    e.SuppressKeyPress = True
                    romanBuffer = romanBuffer.Substring(0, romanBuffer.Length - 1)
                    RenderBuffer()
                    If romanBuffer.Length = 0 Then
                        ResetComposition()
                    Else
                        ShowSuggestions()
                    End If
                    UpdateModeIndicator()
                Else
                    ResetComposition()
                End If
            Case Keys.Left, Keys.Right, Keys.Up, Keys.Down,
                 Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown,
                 Keys.Escape, Keys.Delete, Keys.Enter, Keys.Return, Keys.Tab
                CommitWord()
                ResetComposition()
        End Select
    End Sub

    Private Sub TxtEditor_MouseUp(sender As Object, e As MouseEventArgs)
        ResetComposition()
    End Sub

    Private Sub TxtEditor_MouseWheel(sender As Object, e As MouseEventArgs)
        If (Control.ModifierKeys And Keys.Control) = Keys.Control Then
            If e.Delta > 0 Then
                ZoomBy(1)
            Else
                ZoomBy(-1)
            End If
        End If
    End Sub

    ''' <summary>Applies macros / auto-correct to the just-finished word and learns it.</summary>
    Private Sub CommitWord()
        If romanBuffer.Length = 0 Then Return
        Dim bangla As String = parser.Parse(romanBuffer)
        Dim finalWord As String

        Dim replacement As String = Nothing
        If macrosEnabled Then
            If MacroEngine.IsMacro(romanBuffer) Then
                replacement = MacroEngine.GetReplacement(romanBuffer)
            ElseIf MacroEngine.IsMacro(bangla) Then
                replacement = MacroEngine.GetReplacement(bangla)
            End If
        End If

        If replacement IsNot Nothing Then
            finalWord = replacement
        Else
            finalWord = bangla
            If autoCorrectEnabled Then
                Dim corrected As String = SuggestionEngine.GetAutoCorrection(finalWord)
                If Not String.IsNullOrEmpty(corrected) Then finalWord = corrected
            End If
        End If

        If finalWord <> bangla Then
            If wordStart >= 0 AndAlso wordStart + renderedLen <= txtEditor.TextLength Then
                txtEditor.Select(wordStart, renderedLen)
                txtEditor.SelectedText = finalWord
                renderedLen = finalWord.Length
            End If
        End If

        Try
            If suggestionsEnabled AndAlso bangla.Length > 0 Then SuggestionEngine.Learn(bangla)
        Catch
        End Try
    End Sub

    ' === suggestion popup ===============================================

    Private Function CaretScreenPoint() As Point
        Dim idx As Integer = txtEditor.SelectionStart
        Dim p As Point = txtEditor.GetPositionFromCharIndex(Math.Max(0, idx - 1))
        Dim lh As Integer = txtEditor.Font.Height
        Return txtEditor.PointToScreen(New Point(p.X + 6, p.Y + lh))
    End Function

    Private Sub ShowSuggestions()
        Try
            If Not suggestionsEnabled OrElse Not phoneticMode OrElse suggWin Is Nothing Then
                HideSuggestions() : Return
            End If
            Dim w As String = If(romanBuffer.Length = 0, "", parser.Parse(romanBuffer))
            If String.IsNullOrEmpty(w) Then HideSuggestions() : Return
            Dim list As List(Of String) = SuggestionEngine.GetSuggestions(w)
            If list Is Nothing OrElse list.Count = 0 Then HideSuggestions() : Return
            suggWin.ShowCandidates(list, CaretScreenPoint())
        Catch
            HideSuggestions()
        End Try
    End Sub

    Private Sub HideSuggestions()
        Try
            If suggWin IsNot Nothing Then suggWin.HideWindow()
        Catch
        End Try
    End Sub

    Private Sub AcceptSuggestion(ByVal index As Integer)
        Try
            Dim full As String = suggWin.ItemAt(index)
            If String.IsNullOrEmpty(full) Then Return
            If wordStart >= 0 AndAlso wordStart + renderedLen <= txtEditor.TextLength Then
                txtEditor.Select(wordStart, renderedLen)
                txtEditor.SelectedText = full
                renderedLen = full.Length
            End If
            Try
                SuggestionEngine.Learn(full)
            Catch
            End Try
            ResetComposition()
        Catch
            ResetComposition()
        End Try
    End Sub

    ' === Bangla menu handlers ===========================================

    Private Sub MnuPhonetic_Click(sender As Object, e As EventArgs)
        SetPhoneticMode(Not phoneticMode)
    End Sub

    Private Sub SetPhoneticMode(ByVal enabled As Boolean)
        phoneticMode = enabled
        mnuPhonetic.Checked = enabled
        ResetComposition()
        UpdateModeIndicator()
        Keyboard.NotepadPhoneticActive = (enabled AndAlso Me.ContainsFocus)
    End Sub

    Private Sub MnuSuggestions_Click(sender As Object, e As EventArgs)
        suggestionsEnabled = Not suggestionsEnabled
        mnuSuggestions.Checked = suggestionsEnabled
        If Not suggestionsEnabled Then HideSuggestions()
    End Sub

    Private Sub MnuAutoCorrect_Click(sender As Object, e As EventArgs)
        autoCorrectEnabled = Not autoCorrectEnabled
        mnuAutoCorrect.Checked = autoCorrectEnabled
    End Sub

    Private Sub MnuMacros_Click(sender As Object, e As EventArgs)
        macrosEnabled = Not macrosEnabled
        mnuMacros.Checked = macrosEnabled
    End Sub

    Private Sub MnuCopyBijoy_Click(sender As Object, e As EventArgs)
        Try
            Dim src As String = If(txtEditor.SelectionLength > 0, txtEditor.SelectedText, txtEditor.Text)
            If String.IsNullOrEmpty(src) Then Return
            Clipboard.SetText(BijoyConverter.UnicodeToBijoy(src))
            modeLabel.Text = "Copied as Bijoy ANSI ✓"
        Catch ex As Exception
            MessageBox.Show("Could not copy as Bijoy: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub MnuSaveBijoy_Click(sender As Object, e As EventArgs)
        Using sfd As New SaveFileDialog()
            sfd.Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
            sfd.Title = "Save as Bijoy ANSI"
            If sfd.ShowDialog() = DialogResult.OK Then
                Try
                    ' Bijoy ANSI is legacy single-byte text; write with the Windows ANSI codepage.
                    File.WriteAllText(sfd.FileName, BijoyConverter.UnicodeToBijoy(txtEditor.Text), Encoding.GetEncoding(1252))
                Catch ex As Exception
                    MessageBox.Show("Could not save: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub MnuAI_Click(sender As Object, e As EventArgs)
        Try
            Dim seed As String = If(txtEditor.SelectionLength > 0, txtEditor.SelectedText, "")
            Using dlg As New GeminiAIWindow(seed)
                dlg.ShowDialog()
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not open AI assistant: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub MnuVoice_Click(sender As Object, e As EventArgs)
        Try
            Dim main As MainUI = TryCast(Application.OpenForms("MainUI"), MainUI)
            If main IsNot Nothing Then
                main.StartVoiceTyping()
            Else
                MessageBox.Show("Voice typing needs the BanglaType main window running.", "Voice Typing", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Could not start voice typing: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub MnuPhoneticHelp_Click(sender As Object, e As EventArgs)
        Dim msg As String =
            "Banglish (Phonetic) typing converts Roman letters to Bangla as you type each word." & vbCrLf & vbCrLf &
            "Examples:" & vbCrLf &
            "   ami      ->  আমি" & vbCrLf &
            "   bangla   ->  বাংলা" & vbCrLf &
            "   kemon    ->  কেমন" & vbCrLf & vbCrLf &
            "Tips:" & vbCrLf &
            "   - A word converts while you type; press Space/Enter/punctuation to finish it." & vbCrLf &
            "   - Suggestions appear below the word: ↑/↓ to choose, Tab to accept, Esc to dismiss." & vbCrLf &
            "   - Capital letters change the letter (T = ট, t = ত)." & vbCrLf &
            "   - Toggle Banglish any time with Ctrl+M to type plain English."
        MessageBox.Show(msg, "How to type Banglish", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub UpdateModeIndicator()
        If modeLabel Is Nothing Then Return
        If phoneticMode Then
            Dim preview As String = ""
            If romanBuffer.Length > 0 Then preview = "   " & romanBuffer & " → " & parser.Parse(romanBuffer)
            modeLabel.Text = "⌨ Banglish: ON (Ctrl+M)" & preview
            modeLabel.ForeColor = If(isDark, Color.MediumSpringGreen, Color.SeaGreen)
        Else
            modeLabel.Text = "⌨ Banglish: OFF (Ctrl+M)"
            modeLabel.ForeColor = Color.Gray
        End If
    End Sub

    ' === Find & Replace =================================================

    Private Sub MnuFind_Click(sender As Object, e As EventArgs)
        ShowFindReplace(False)
    End Sub

    Private Sub MnuReplace_Click(sender As Object, e As EventArgs)
        ShowFindReplace(True)
    End Sub

    Private findDlg As FindReplaceForm

    Private Sub ShowFindReplace(ByVal showReplace As Boolean)
        If findDlg Is Nothing OrElse findDlg.IsDisposed Then
            findDlg = New FindReplaceForm(txtEditor)
            findDlg.Owner = Me
        End If
        findDlg.SetReplaceVisible(showReplace)
        If txtEditor.SelectionLength > 0 Then findDlg.SetFindText(txtEditor.SelectedText)
        findDlg.Show()
        findDlg.BringToFront()
        findDlg.FocusFind()
    End Sub

    ' === Zoom ===========================================================

    Private Sub MnuZoomIn_Click(sender As Object, e As EventArgs)
        ZoomBy(1)
    End Sub

    Private Sub MnuZoomOut_Click(sender As Object, e As EventArgs)
        ZoomBy(-1)
    End Sub

    Private Sub MnuZoomReset_Click(sender As Object, e As EventArgs)
        txtEditor.Font = New Font(txtEditor.Font.FontFamily, 12.0!, txtEditor.Font.Style)
    End Sub

    Private Sub ZoomBy(ByVal steps As Integer)
        Dim newSize As Single = txtEditor.Font.Size + steps
        If newSize < 6 Then newSize = 6
        If newSize > 72 Then newSize = 72
        txtEditor.Font = New Font(txtEditor.Font.FontFamily, newSize, txtEditor.Font.Style)
    End Sub

    ' === Printing =======================================================

    Private Sub MnuPrint_Click(sender As Object, e As EventArgs)
        Try
            printText = txtEditor.Text
            printCharIndex = 0
            Using pd As New PrintDialog()
                pd.Document = printDoc
                If pd.ShowDialog() = DialogResult.OK Then printDoc.Print()
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not print: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub MnuPrintPreview_Click(sender As Object, e As EventArgs)
        Try
            printText = txtEditor.Text
            printCharIndex = 0
            Using ppd As New PrintPreviewDialog()
                ppd.Document = printDoc
                CType(ppd, Form).WindowState = FormWindowState.Maximized
                ppd.ShowDialog()
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not preview: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub printDoc_PrintPage(sender As Object, e As PrintPageEventArgs) Handles printDoc.PrintPage
        Dim font As Font = txtEditor.Font
        Dim fmt As New StringFormat()
        Dim remaining As String = If(printCharIndex < printText.Length, printText.Substring(printCharIndex), "")
        Dim charsFitted, linesFilled As Integer
        e.Graphics.MeasureString(remaining, font, New SizeF(e.MarginBounds.Width, e.MarginBounds.Height),
                                 fmt, charsFitted, linesFilled)
        e.Graphics.DrawString(remaining.Substring(0, charsFitted), font, Brushes.Black, e.MarginBounds, fmt)
        printCharIndex += charsFitted
        e.HasMorePages = printCharIndex < printText.Length
    End Sub

    ' === View / theme ===================================================

    Private Sub MnuDarkMode_Click(sender As Object, e As EventArgs)
        isDark = Not isDark
        mnuDarkMode.Checked = isDark
        ApplyTheme()
    End Sub

    Private Sub ApplyTheme()
        Dim back As Color, fore As Color, bar As Color
        If isDark Then
            back = Color.FromArgb(30, 30, 32) : fore = Color.Gainsboro : bar = Color.FromArgb(45, 45, 48)
        Else
            back = Color.White : fore = Color.Black : bar = SystemColors.Control
        End If
        Me.BackColor = bar
        txtEditor.BackColor = back
        txtEditor.ForeColor = fore
        mainMenu.BackColor = bar
        mainMenu.ForeColor = fore
        statusBar.BackColor = bar
        statusLabel.ForeColor = fore
        UpdateModeIndicator()
    End Sub

    ' === recent files ===================================================

    Private Shared Function RecentFolder() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType")
    End Function

    Private Shared Function RecentPath() As String
        Return Path.Combine(RecentFolder(), "notepad_recent.txt")
    End Function

    Private Shared Function LoadRecent() As List(Of String)
        Dim items As New List(Of String)()
        Try
            If File.Exists(RecentPath()) Then
                For Each line As String In File.ReadAllLines(RecentPath(), Encoding.UTF8)
                    If Not String.IsNullOrWhiteSpace(line) AndAlso File.Exists(line.Trim()) Then items.Add(line.Trim())
                Next
            End If
        Catch
        End Try
        Return items
    End Function

    Private Sub AddRecent(ByVal path As String)
        If String.IsNullOrEmpty(path) Then Return
        Try
            Dim items As List(Of String) = LoadRecent()
            items.RemoveAll(Function(p) String.Equals(p, path, StringComparison.OrdinalIgnoreCase))
            items.Insert(0, path)
            While items.Count > RecentMax
                items.RemoveAt(items.Count - 1)
            End While
            Directory.CreateDirectory(RecentFolder())
            File.WriteAllLines(RecentPath(), items.ToArray(), Encoding.UTF8)
        Catch
        End Try
        RebuildRecentMenu()
    End Sub

    Private Sub RebuildRecentMenu()
        If mnuRecent Is Nothing Then Return
        mnuRecent.DropDownItems.Clear()
        Dim items As List(Of String) = LoadRecent()
        If items.Count = 0 Then
            mnuRecent.DropDownItems.Add(New ToolStripMenuItem("(no recent files)") With {.Enabled = False})
            Return
        End If
        For Each p As String In items
            Dim path As String = p
            Dim item As New ToolStripMenuItem(Path.GetFileName(path)) With {.ToolTipText = path}
            AddHandler item.Click, Sub() OpenPath(path)
            mnuRecent.DropDownItems.Add(item)
        Next
    End Sub

    ' === window / focus plumbing ========================================

    Protected Overrides Sub OnActivated(e As EventArgs)
        MyBase.OnActivated(e)
        Keyboard.NotepadPhoneticActive = phoneticMode
    End Sub

    Protected Overrides Sub OnDeactivate(e As EventArgs)
        MyBase.OnDeactivate(e)
        Keyboard.NotepadPhoneticActive = False
        ResetComposition()
    End Sub

    Private Sub UpdateTitle()
        Dim name As String = If(String.IsNullOrEmpty(currentFilePath), "Untitled", Path.GetFileName(currentFilePath))
        Me.Text = "BanglaType Notepad - " & name
    End Sub

    Private Sub TxtEditor_TextChanged(sender As Object, e As EventArgs)
        isModified = True
        UpdateStatus()
    End Sub

    Private Sub UpdateStatus()
        Dim text As String = txtEditor.Text
        Dim charCount As Integer = text.Length
        Dim lineCount As Integer = txtEditor.Lines.Length
        Dim wordCount As Integer = 0
        If Not String.IsNullOrWhiteSpace(text) Then
            wordCount = text.Split(New Char() {" "c, vbTab(0), vbCr(0), vbLf(0)}, StringSplitOptions.RemoveEmptyEntries).Length
        End If
        statusLabel.Text = String.Format("Lines: {0} | Words: {1} | Characters: {2}", lineCount, wordCount, charCount)
    End Sub

    ' === File menu handlers =============================================

    Private Sub MnuNew_Click(sender As Object, e As EventArgs)
        If ConfirmSaveIfNeeded() Then
            ResetComposition()
            txtEditor.Clear()
            currentFilePath = ""
            isModified = False
            UpdateTitle()
        End If
    End Sub

    Private Sub MnuOpen_Click(sender As Object, e As EventArgs)
        If ConfirmSaveIfNeeded() Then
            Using ofd As New OpenFileDialog()
                ofd.Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
                If ofd.ShowDialog() = DialogResult.OK Then OpenPath(ofd.FileName)
            End Using
        End If
    End Sub

    Private Sub OpenPath(ByVal path As String)
        Try
            ResetComposition()
            txtEditor.Text = File.ReadAllText(path)
            currentFilePath = path
            isModified = False
            UpdateTitle()
            AddRecent(path)
        Catch ex As Exception
            MessageBox.Show("Could not open file: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub MnuSave_Click(sender As Object, e As EventArgs)
        SaveFile()
    End Sub

    Private Sub MnuSaveAs_Click(sender As Object, e As EventArgs)
        SaveFileAs()
    End Sub

    Private Sub MnuExit_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    Private Function SaveFile() As Boolean
        If String.IsNullOrEmpty(currentFilePath) Then
            Return SaveFileAs()
        Else
            Try
                File.WriteAllText(currentFilePath, txtEditor.Text, New UTF8Encoding(True))
                isModified = False
                AddRecent(currentFilePath)
                Return True
            Catch ex As Exception
                MessageBox.Show("Could not save file: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End Try
        End If
    End Function

    Private Function SaveFileAs() As Boolean
        Using sfd As New SaveFileDialog()
            sfd.Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
            If sfd.ShowDialog() = DialogResult.OK Then
                Try
                    File.WriteAllText(sfd.FileName, txtEditor.Text, New UTF8Encoding(True))
                    currentFilePath = sfd.FileName
                    isModified = False
                    UpdateTitle()
                    AddRecent(sfd.FileName)
                    Return True
                Catch ex As Exception
                    MessageBox.Show("Could not save file: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
        Return False
    End Function

    Private Function ConfirmSaveIfNeeded() As Boolean
        If isModified Then
            Dim result As DialogResult = MessageBox.Show("Do you want to save changes to this document?", "BanglaType Notepad", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If result = DialogResult.Yes Then
                Return SaveFile()
            ElseIf result = DialogResult.Cancel Then
                Return False
            End If
        End If
        Return True
    End Function

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Keyboard.NotepadPhoneticActive = False
        If Not ConfirmSaveIfNeeded() Then
            e.Cancel = True
        Else
            HideSuggestions()
            If suggWin IsNot Nothing Then suggWin.Dispose()
        End If
        MyBase.OnFormClosing(e)
    End Sub

    ' === Edit menu handlers =============================================

    Private Sub MnuUndo_Click(sender As Object, e As EventArgs)
        ResetComposition()
        If txtEditor.CanUndo Then txtEditor.Undo()
    End Sub

    Private Sub MnuCut_Click(sender As Object, e As EventArgs)
        ResetComposition()
        txtEditor.Cut()
    End Sub

    Private Sub MnuCopy_Click(sender As Object, e As EventArgs)
        txtEditor.Copy()
    End Sub

    Private Sub MnuPaste_Click(sender As Object, e As EventArgs)
        ResetComposition()
        txtEditor.Paste()
    End Sub

    Private Sub MnuDelete_Click(sender As Object, e As EventArgs)
        ResetComposition()
        txtEditor.SelectedText = ""
    End Sub

    Private Sub MnuSelectAll_Click(sender As Object, e As EventArgs)
        ResetComposition()
        txtEditor.SelectAll()
    End Sub

    Private Sub MnuTimeDate_Click(sender As Object, e As EventArgs)
        ResetComposition()
        Dim timeDateStr As String = DateTime.Now.ToString("g")
        Dim selectionIndex As Integer = txtEditor.SelectionStart
        txtEditor.Text = txtEditor.Text.Insert(selectionIndex, timeDateStr)
        txtEditor.SelectionStart = selectionIndex + timeDateStr.Length
    End Sub

    ' === Format menu handlers ===========================================

    Private Sub MnuWordWrap_Click(sender As Object, e As EventArgs)
        mnuWordWrap.Checked = Not mnuWordWrap.Checked
        txtEditor.WordWrap = mnuWordWrap.Checked
        txtEditor.ScrollBars = If(mnuWordWrap.Checked, ScrollBars.Vertical, ScrollBars.Both)
    End Sub

    Private Sub MnuFont_Click(sender As Object, e As EventArgs)
        Using fd As New FontDialog()
            fd.Font = txtEditor.Font
            If fd.ShowDialog() = DialogResult.OK Then txtEditor.Font = fd.Font
        End Using
    End Sub
End Class


''' <summary>Lightweight modeless Find &amp; Replace dialog bound to a TextBox.</summary>
Friend Class FindReplaceForm
    Inherits Form

    Private ReadOnly target As TextBox
    Private ReadOnly txtFind As New TextBox()
    Private ReadOnly txtReplace As New TextBox()
    Private ReadOnly chkCase As New CheckBox()
    Private ReadOnly lblReplace As New Label()
    Private ReadOnly btnReplace As New Button()
    Private ReadOnly btnReplaceAll As New Button()

    Public Sub New(ByVal editor As TextBox)
        target = editor
        Me.Text = "Find & Replace"
        Me.FormBorderStyle = FormBorderStyle.FixedToolWindow
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Size(420, 150)
        Me.ShowInTaskbar = False

        Dim lblFind As New Label() With {.Text = "Find:", .Location = New Point(12, 16), .AutoSize = True}
        txtFind.SetBounds(110, 12, 200, 24)
        lblReplace.Text = "Replace with:" : lblReplace.Location = New Point(12, 48) : lblReplace.AutoSize = True
        txtReplace.SetBounds(110, 44, 200, 24)
        chkCase.Text = "Match case" : chkCase.Location = New Point(110, 76) : chkCase.AutoSize = True

        Dim btnFind As New Button() With {.Text = "Find Next"}
        btnFind.SetBounds(322, 11, 86, 26)
        AddHandler btnFind.Click, AddressOf FindNext_Click

        btnReplace.Text = "Replace" : btnReplace.SetBounds(322, 43, 86, 26)
        AddHandler btnReplace.Click, AddressOf Replace_Click

        btnReplaceAll.Text = "Replace All" : btnReplaceAll.SetBounds(322, 75, 86, 26)
        AddHandler btnReplaceAll.Click, AddressOf ReplaceAll_Click

        Dim btnClose As New Button() With {.Text = "Close"}
        btnClose.SetBounds(322, 110, 86, 26)
        AddHandler btnClose.Click, Sub() Me.Hide()

        Me.Controls.AddRange(New Control() {lblFind, txtFind, lblReplace, txtReplace, chkCase, btnFind, btnReplace, btnReplaceAll, btnClose})
        Me.AcceptButton = btnFind
        Me.CancelButton = btnClose
    End Sub

    Public Sub SetReplaceVisible(ByVal visible As Boolean)
        lblReplace.Visible = visible
        txtReplace.Visible = visible
        btnReplace.Visible = visible
        btnReplaceAll.Visible = visible
        Me.Text = If(visible, "Find & Replace", "Find")
    End Sub

    Public Sub SetFindText(ByVal text As String)
        txtFind.Text = text
    End Sub

    Public Sub FocusFind()
        txtFind.Focus()
        txtFind.SelectAll()
    End Sub

    Private Function Comparison() As StringComparison
        Return If(chkCase.Checked, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub FindNext_Click(sender As Object, e As EventArgs)
        Dim needle As String = txtFind.Text
        If String.IsNullOrEmpty(needle) Then Return
        Dim start As Integer = target.SelectionStart + target.SelectionLength
        Dim idx As Integer = target.Text.IndexOf(needle, Math.Min(start, target.TextLength), Comparison())
        If idx < 0 Then idx = target.Text.IndexOf(needle, 0, Comparison()) ' wrap around
        If idx >= 0 Then
            target.Select(idx, needle.Length)
            target.ScrollToCaret()
        Else
            MessageBox.Show("Cannot find """ & needle & """", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub Replace_Click(sender As Object, e As EventArgs)
        If target.SelectionLength > 0 AndAlso String.Equals(target.SelectedText, txtFind.Text, Comparison()) Then
            target.SelectedText = txtReplace.Text
        End If
        FindNext_Click(sender, e)
    End Sub

    Private Sub ReplaceAll_Click(sender As Object, e As EventArgs)
        Dim needle As String = txtFind.Text
        If String.IsNullOrEmpty(needle) Then Return
        Dim text As String = target.Text
        Dim sb As New StringBuilder()
        Dim i As Integer = 0
        Dim count As Integer = 0
        While i < text.Length
            Dim idx As Integer = text.IndexOf(needle, i, Comparison())
            If idx < 0 Then
                sb.Append(text.Substring(i))
                Exit While
            End If
            sb.Append(text.Substring(i, idx - i))
            sb.Append(txtReplace.Text)
            i = idx + needle.Length
            count += 1
        End While
        If count > 0 Then target.Text = sb.ToString()
        MessageBox.Show(count & " replacement(s) made.", "Replace All", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class
