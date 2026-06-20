'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   Software distributed under the License Is distributed On an "AS IS"
'   basis, WITHOUT WARRANTY Of ANY KIND, either express Or implied. See the
'   License for the specific language governing rights And limitations
'   under the License.
'
'   The Initial Developer of this Code is Mohammad Sheikh Shahinur Rahman
'   Copyright© Mohammad Sheikh Shahinur Rahman. All Rights Reserved
'
'

Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text

Public Class MainUI

    Public Const WM_NCLBUTTONDOWN As Integer = &HA1
    Public Const HT_CAPTION As Integer = &H2

    Public Shared isPhonetic As Boolean = False
    Public Shared isPhoneticSelected As Boolean = False
    Public Shared IsSafeMode As Boolean = False

    ' Borno Lite "Advance" additions
    Public Shared isAvroSelected As Boolean = False
    Public Shared AvroEngine As AvroParser
    Public Shared SuggWindow As SuggestionWindow
    Public Shared floatKeyboard As FloatingKeyboardWindow = Nothing

    Public Shared ClipboardHistory As New List(Of String)()
    Private WithEvents clipboardTimer As Timer
    Private lastClipboardText As String = ""

    ' Theme-driven colors (defaults match the Light theme so paint works pre-theme).
    Public OnColorTheme As Color = Color.FromArgb(0, 180, 137)
    Public OffColorTheme As Color = Color.FromArgb(222, 75, 57)
    Public ThemeTopbarBack As Color = Color.FromArgb(242, 242, 244)
    Public ThemeBorderColor As Color = Color.FromArgb(230, 230, 230)
    Public currentButtonFore As Color = Color.FromArgb(140, 140, 140)

    Private trayMenu As ContextMenuStrip
    Private settingsMenu As ContextMenuStrip
    Private Const AVRO_TAG As Integer = 9998
    Private Shared voiceBrowserProcess As System.Diagnostics.Process = Nothing

    <DllImportAttribute("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
    End Function

    <DllImportAttribute("user32.dll")>
    Public Shared Function ReleaseCapture() As Boolean
    End Function

    Public Shared Parser As Object

    Private Sub writeBornoLayout(ByVal layout As String)
        Dim baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "keyboard layouts")
        Dim filePath = Path.Combine(baseDir, layout & ".kbl")
        If Not File.Exists(filePath) Then
            Directory.CreateDirectory(baseDir)
            File.WriteAllBytes(filePath, My.Resources.Borno)
        End If
    End Sub

    Private Sub writeBornoEncodingLayout(ByVal layout As String)
        Dim baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "keyboard layouts")
        Dim filePath = Path.Combine(baseDir, layout & ".kbl")
        If Not File.Exists(filePath) Then
            Directory.CreateDirectory(baseDir)
            File.WriteAllBytes(filePath, My.Resources.Borno_Encoding)
        End If
    End Sub

    Private Sub writeNationalLayout(ByVal layout As String)
        Dim baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "keyboard layouts")
        Dim filePath = Path.Combine(baseDir, layout & ".kbl")
        If Not File.Exists(filePath) Then
            Directory.CreateDirectory(baseDir)
            File.WriteAllBytes(filePath, My.Resources.National)
        End If
    End Sub
    Private Sub MainUI_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' Parse command line arguments for Safe Mode
            Dim args As String() = Environment.GetCommandLineArgs()
            For Each arg As String In args
                If arg.ToLower() = "--safe" Then
                    IsSafeMode = True
                    Exit For
                End If
            Next

            ' Load persisted settings + prepare the "Advance" subsystems.
            AppSettings.Load()

            ' First-run Setup Wizard
            If AppSettings.FirstRun AndAlso Not IsSafeMode Then
                Using wizard As New SetupWizardWindow()
                    wizard.ShowDialog()
                End Using
            End If

            ThemeManager.EnsureBuiltInThemes()
            SuggestionEngine.EnsureLoaded()
            AvroEngine = New AvroParser()

            If Not IsSafeMode Then
                SuggWindow = New SuggestionWindow()
                ' Start Clipboard Monitoring
                clipboardTimer = New Timer()
                clipboardTimer.Interval = 1000 ' 1 second
                AddHandler clipboardTimer.Tick, AddressOf ClipboardTimer_Tick
                clipboardTimer.Start()
            End If

            writeBornoLayout("Borno")
            writeBornoEncodingLayout("Borno Encoding")
            writeNationalLayout("National")

            ' Robust DLL load with Try...Catch to handle platform mismatch or missing dependencies on 64-bit Systems.
            Try
                Dim filePath = Path.Combine(Application.StartupPath & "\data\lib\", "libcpphonetic.dll")
                If File.Exists(filePath) Then
                    Dim oType As Type
                    Dim oAssembly As Assembly
                    oAssembly = Assembly.LoadFrom(filePath)
                    oType = oAssembly.GetType("libPhoneticParser.Parser")
                    Parser = Activator.CreateInstance(oType)
                    isPhonetic = True
                    Dim dynItem As New ToolStripMenuItem() With {.Text = " BanglaType Phonetic", .Name = "bp", .Tag = 9999}
                    AddHandler dynItem.Click, AddressOf mnuItem_Clicked
                    LayoutList.Items.Add(dynItem)
                Else
                    isPhonetic = False
                End If
            Catch ex As Exception
                isPhonetic = False
                Debug.WriteLine("Failed to load libcpphonetic.dll: " & ex.ToString())
            End Try

            ' Open Avro Phonetic layout (works without the closed DLL).
            If AppSettings.AvroEnabled Then
                Dim avroItem As New ToolStripMenuItem() With {.Text = " Avro Phonetic (Open)", .Name = "avro", .Tag = AVRO_TAG}
                AddHandler avroItem.Click, AddressOf mnuItem_Clicked
                LayoutList.Items.Add(avroItem)
            End If

            'Tray Icon
            Try
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
                NotifyIcon1.Icon = Icon
            Catch
            End Try
            NotifyIcon1.Visible = False

            ' Logo PictureBox
            Try
                logoBox.Image = My.Resources.borno_lite
            Catch
            End Try

            'Topbar UI
            Width = 190
            Height = 32
            Top = 0
            Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (Width / 2) 'Center Topbar
            TopMost = True
            HookKeyboard()
            LayoutParser.SearchForLayouts()

            ' Modernize buttons
            buttonClose.BackgroundImage = Nothing
            buttonClose.Text = "✕"
            buttonClose.Font = New Font("Segoe UI", 8.0!, FontStyle.Regular)
            buttonClose.ForeColor = Color.FromArgb(140, 140, 140)
            buttonClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35)
            buttonClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(241, 112, 122)

            BuildSettingsMenu()

            ' Apply saved theme, restore the last layout + on/off state, build the tray menu.
            ApplyTheme(AppSettings.ThemeName)
            RestoreLastLayout()
            RestoreActivation()
            BuildTrayMenu()
            UpdateModeUI()
            ApplyRoundedCorners()

            ' Safe Mode Notification and Automatic Updater Run
            If IsSafeMode Then
                MessageBox.Show("BanglaType has been started in Safe Mode. Suggestions and Clipboard Manager are disabled for stability.", "Safe Mode Active", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                ' Check for updates in background
                CheckForUpdates(silent:=True)
            End If
        Catch ex As Exception
            MessageBox.Show("An error occurred during BanglaType initialization: " & ex.Message & vbCrLf & ex.StackTrace, "BanglaType Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Protected Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Const CS_DROPSHADOW As Integer = &H20000
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
            Return cp
        End Get
    End Property

    Private Function GetRoundedRectPath(ByVal rect As Rectangle, ByVal radius As Integer) As System.Drawing.Drawing2D.GraphicsPath
        Dim path As New System.Drawing.Drawing2D.GraphicsPath()
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
        Using path As System.Drawing.Drawing2D.GraphicsPath = GetRoundedRectPath(New Rectangle(0, 0, Width, Height), 15)
            Me.Region = New Region(path)
        End Using
    End Sub

    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)
        MyBase.OnPaintBackground(e)
        BackColor = ThemeTopbarBack
        Dim rect As New Rectangle(0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1)
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
        Using path As System.Drawing.Drawing2D.GraphicsPath = GetRoundedRectPath(rect, 15)
            Using p As New Pen(ThemeBorderColor, 1)
                e.Graphics.DrawPath(p, path)
            End Using
        End Using
    End Sub

    ''' <summary>Loads and applies a theme by name, persisting the choice.</summary>
    Public Sub ApplyTheme(ByVal name As String)
        Dim t As Theme = ThemeManager.LoadTheme(name)
        ThemeManager.Apply(Me, t, SuggWindow)
        AppSettings.ThemeName = t.Name
        AppSettings.Save()
    End Sub

    ''' <summary>Re-selects the layout chosen in a previous session, if still available.</summary>
    Private Sub RestoreLastLayout()
        If AppSettings.LastLayoutTag < 0 Then Return
        crlay = AppSettings.LastLayoutTag
        isPhoneticSelected = (crlay = 9999)
        isAvroSelected = (crlay = AVRO_TAG)
        If isPhoneticSelected OrElse isAvroSelected Then
            LastPhoneticTag = crlay
        Else
            LastFixedTag = crlay
        End If
    End Sub

    ''' <summary>Restores Bangla on/off from settings and reflects it on the indicator.</summary>
    Private Sub RestoreActivation()
        isActivated = AppSettings.Activated
        UpdateModeUI()
    End Sub

    Private Sub buttonClose_Click(sender As Object, e As EventArgs) Handles buttonClose.Click
        Application.Exit()
    End Sub

    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles buttonInfo.Click
        Using dlg As New AboutWindow()
            dlg.ShowDialog()
        End Using
    End Sub

    Private Sub DragWindow_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown, logoBox.MouseDown, btnMode.MouseDown
        If e.Button = MouseButtons.Left Then
            ReleaseCapture()
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0)
        End If
    End Sub

    Public Sub mnuItem_Clicked(sender As Object, e As EventArgs)
        LayoutList.Hide()
        Dim item As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        ApplySelection(item)
    End Sub

    ''' <summary>Activates a layout menu item (shared by the topbar list, tray menu, and restore-on-start).</summary>
    Public Sub ApplySelection(ByVal item As ToolStripMenuItem)
        If item Is Nothing Then Return
        ResetInternalVals()
        HideSuggest()
        crlay = Convert.ToInt32(item.Tag)

        isPhoneticSelected = (crlay = 9999)
        isAvroSelected = (crlay = AVRO_TAG)

        isActivated = True
        AppSettings.Activated = True

        If isPhoneticSelected OrElse isAvroSelected Then
            LastPhoneticTag = crlay
        Else
            LastFixedTag = crlay
        End If

        AppSettings.LastLayoutTag = crlay
        AppSettings.Save()
        UpdateModeUI()
    End Sub

    ' --- tray menu --------------------------------

    Private Sub BuildTrayMenu()
        trayMenu = New ContextMenuStrip()

        Dim miToggle As New ToolStripMenuItem("Toggle Mode")
        AddHandler miToggle.Click, Sub() ToggleActivation()
        trayMenu.Items.Add(miToggle)

        ' Layout submenu mirrors the topbar list.
        Dim miLayouts As New ToolStripMenuItem("Layout")
        For Each it As Object In LayoutList.Items
            Dim src As ToolStripMenuItem = TryCast(it, ToolStripMenuItem)
            If src Is Nothing Then Continue For
            Dim child As New ToolStripMenuItem(src.Text.Trim())
            child.Tag = src.Tag
            AddHandler child.Click, Sub(s As Object, e As EventArgs)
                                        Dim c As ToolStripMenuItem = CType(s, ToolStripMenuItem)
                                        ApplySelection(New ToolStripMenuItem(c.Text) With {.Tag = c.Tag})
                                    End Sub
            miLayouts.DropDownItems.Add(child)
        Next
        trayMenu.Items.Add(miLayouts)

        ' Theme submenu.
        Dim miThemes As New ToolStripMenuItem("Theme")
        For Each name As String In ThemeManager.ListThemeNames()
            Dim tn As String = name
            Dim child As New ToolStripMenuItem(tn)
            AddHandler child.Click, Sub() ApplyTheme(tn)
            miThemes.DropDownItems.Add(child)
        Next
        trayMenu.Items.Add(miThemes)

        Dim miHotkey As New ToolStripMenuItem("Shortcut Key")
        Dim keysList As String() = {"F12", "F10", "F9", "F8", "Ctrl + Space"}
        For Each k As String In keysList
            Dim keyName As String = k
            Dim child As New ToolStripMenuItem(keyName)
            AddHandler child.Click, Sub()
                                        AppSettings.Hotkey = keyName
                                        AppSettings.Save()
                                    End Sub
            miHotkey.DropDownItems.Add(child)
        Next
        trayMenu.Items.Add(miHotkey)

        trayMenu.Items.Add(New ToolStripSeparator())

        Dim miUnicode As New ToolStripMenuItem("Output: Unicode") With {.Name = "outUnicode"}
        AddHandler miUnicode.Click, Sub() SetOutputMode("Unicode")
        trayMenu.Items.Add(miUnicode)

        Dim miAnsi As New ToolStripMenuItem("Output: ANSI (Bijoy)") With {.Name = "outAnsi"}
        AddHandler miAnsi.Click, Sub() SetOutputMode("ANSI")
        trayMenu.Items.Add(miAnsi)

        Dim miSugg As New ToolStripMenuItem("Suggestions") With {.Name = "sugg"}
        AddHandler miSugg.Click, Sub() ToggleSuggestions()
        trayMenu.Items.Add(miSugg)

        Dim miClipboardTray As New ToolStripMenuItem("Clipboard Manager") With {.Name = "clip"}
        trayMenu.Items.Add(miClipboardTray)

        Dim miPhrasesTray As New ToolStripMenuItem("Quick Phrases")
        Dim phrasesList As String() = {"ধন্যবাদ", "কেমন আছেন?", "আসসালামু আলাইকুম", "শুভ সকাল", "আমি ভালো আছি"}
        For Each p As String In phrasesList
            Dim phrase As String = p
            Dim child As New ToolStripMenuItem(phrase)
            AddHandler child.Click, Sub() PasteText(phrase)
            miPhrasesTray.DropDownItems.Add(child)
        Next
        trayMenu.Items.Add(miPhrasesTray)

        Dim miStartWithWindows As New ToolStripMenuItem("Start with Windows") With {.Name = "autostart"}
        AddHandler miStartWithWindows.Click, Sub()
                                                 Dim enabled As Boolean = IsAutoStartEnabled()
                                                 SetAutoStart(Not enabled)
                                             End Sub
        trayMenu.Items.Add(miStartWithWindows)

        Dim miConverter As New ToolStripMenuItem("Text Converter...")
        AddHandler miConverter.Click, Sub()
                                          Using dlg As New ConverterWindow()
                                              dlg.ShowDialog()
                                          End Using
                                      End Sub
        trayMenu.Items.Add(miConverter)

        Dim miAnalyticsTray As New ToolStripMenuItem("Typing Analytics Dashboard...")
        AddHandler miAnalyticsTray.Click, Sub()
                                              Using dlg As New AnalyticsWindow()
                                                  dlg.ShowDialog()
                                              End Using
                                          End Sub
        trayMenu.Items.Add(miAnalyticsTray)

        Dim miDictManagerTray As New ToolStripMenuItem("Custom Dictionary Manager...")
        AddHandler miDictManagerTray.Click, Sub()
                                                Using dlg As New DictionaryManagerWindow()
                                                    dlg.ShowDialog()
                                                End Using
                                            End Sub
        trayMenu.Items.Add(miDictManagerTray)

        Dim miWordCustomizerTray As New ToolStripMenuItem("Word Customizer...")
        AddHandler miWordCustomizerTray.Click, Sub()
                                                   Using dlg As New WordCustomizerWindow()
                                                       dlg.ShowDialog()
                                                   End Using
                                               End Sub
        trayMenu.Items.Add(miWordCustomizerTray)

        Dim miLayoutBuilderTray As New ToolStripMenuItem("Custom Layout Builder...")
        AddHandler miLayoutBuilderTray.Click, Sub()
                                                  Using dlg As New LayoutBuilderWindow()
                                                      dlg.ShowDialog()
                                                  End Using
                                              End Sub
        trayMenu.Items.Add(miLayoutBuilderTray)

        Dim miAIAssistantTray As New ToolStripMenuItem("AI Format Assistant...")
        AddHandler miAIAssistantTray.Click, Sub()
                                                Using dlg As New GeminiAIWindow()
                                                    dlg.ShowDialog()
                                                End Using
                                            End Sub
        trayMenu.Items.Add(miAIAssistantTray)

        Dim miVoiceTray As New ToolStripMenuItem("Voice Typing (Speech-to-Text)...")
        AddHandler miVoiceTray.Click, Sub() TriggerVoiceTyping()
        trayMenu.Items.Add(miVoiceTray)

        Dim miStickersTray As New ToolStripMenuItem("Stickers & GIFs...")
        AddHandler miStickersTray.Click, Sub()
                                             Using dlg As New StickersWindow()
                                                 dlg.ShowDialog()
                                             End Using
                                         End Sub
        trayMenu.Items.Add(miStickersTray)

        Dim miFloatingTray As New ToolStripMenuItem("Floating Keyboard Mode") With {.Name = "floatingKeyboard"}
        AddHandler miFloatingTray.Click, Sub() ToggleFloatingKeyboard()
        trayMenu.Items.Add(miFloatingTray)

        trayMenu.Items.Add(New ToolStripSeparator())

        Dim miAbout As New ToolStripMenuItem("About")
        AddHandler miAbout.Click, Sub()
                                      Using dlg As New AboutWindow()
                                          dlg.ShowDialog()
                                      End Using
                                  End Sub
        trayMenu.Items.Add(miAbout)

        Dim miExit As New ToolStripMenuItem("Exit")
        AddHandler miExit.Click, Sub() Application.Exit()
        trayMenu.Items.Add(miExit)

        AddHandler trayMenu.Opening, AddressOf TrayMenu_Opening

        NotifyIcon1.ContextMenuStrip = trayMenu
        NotifyIcon1.Text = "BanglaType"
        NotifyIcon1.Visible = True
    End Sub

    Private Sub TrayMenu_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs)
        ' Reflect current state on the checkable items.
        SetCheck("outUnicode", AppSettings.OutputMode <> "ANSI")
        SetCheck("outAnsi", AppSettings.OutputMode = "ANSI")
        SetCheck("sugg", AppSettings.SuggestionsEnabled)
        SetCheck("autostart", IsAutoStartEnabled())
        SetCheck("floatingKeyboard", floatKeyboard IsNot Nothing AndAlso floatKeyboard.Visible)

        ' Update Layout checked checkmarks
        For Each it As Object In trayMenu.Items
            Dim mi As ToolStripMenuItem = TryCast(it, ToolStripMenuItem)
            If mi IsNot Nothing AndAlso mi.Text = "Layout" Then
                For Each child As Object In mi.DropDownItems
                    Dim cMi As ToolStripMenuItem = TryCast(child, ToolStripMenuItem)
                    If cMi IsNot Nothing AndAlso cMi.Tag IsNot Nothing Then
                        cMi.Checked = (Convert.ToInt32(cMi.Tag) = crlay)
                    End If
                Next
            ElseIf mi IsNot Nothing AndAlso mi.Text = "Shortcut Key" Then
                For Each child As Object In mi.DropDownItems
                    Dim cMi As ToolStripMenuItem = TryCast(child, ToolStripMenuItem)
                    If cMi IsNot Nothing Then
                        cMi.Checked = (cMi.Text = AppSettings.Hotkey)
                    End If
                Next
            ElseIf mi IsNot Nothing AndAlso mi.Name = "clip" Then
                mi.DropDownItems.Clear()
                If ClipboardHistory.Count = 0 Then
                    mi.DropDownItems.Add(New ToolStripMenuItem("(History Empty)") With {.Enabled = False})
                Else
                    For Each item As String In ClipboardHistory
                        Dim txt As String = item
                        Dim label As String = If(txt.Length > 20, txt.Substring(0, 17) & "...", txt)
                        Dim child As New ToolStripMenuItem(label)
                        AddHandler child.Click, Sub() PasteText(txt)
                        mi.DropDownItems.Add(child)
                    Next
                End If
            End If
        Next
    End Sub

    Private Sub SetCheck(ByVal name As String, ByVal value As Boolean)
        For Each it As Object In trayMenu.Items
            Dim mi As ToolStripMenuItem = TryCast(it, ToolStripMenuItem)
            If mi IsNot Nothing AndAlso mi.Name = name Then
                mi.Checked = value
                Return
            End If
        Next
    End Sub

    Public Sub ToggleActivation()
        ResetInternalVals()
        isActivated = Not isActivated
        AppSettings.Activated = isActivated
        AppSettings.Save()

        ' Update active layout if toggled on
        If isActivated Then
            If crlay = 9999 OrElse crlay = 9998 Then
                isPhoneticSelected = (crlay = 9999)
                isAvroSelected = (crlay = 9998)
            Else
                isPhoneticSelected = False
                isAvroSelected = False
            End If
        End If

        UpdateModeUI()
        HideSuggest()
    End Sub

    Private Sub SetOutputMode(ByVal mode As String)
        AppSettings.OutputMode = mode
        AppSettings.Save()
    End Sub

    Private Sub ToggleSuggestions()
        AppSettings.SuggestionsEnabled = Not AppSettings.SuggestionsEnabled
        AppSettings.Save()
        If Not AppSettings.SuggestionsEnabled Then HideSuggest()
    End Sub

    Private Sub MainUI_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            NotifyIcon1.Visible = True
            Me.Hide()
            NotifyIcon1.BalloonTipText = "BanglaType is running on Tray"
            NotifyIcon1.ShowBalloonTip(10)
        End If
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        NotifyIcon1.Visible = False
    End Sub

    Private Sub MainUI_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        NotifyIcon1.Visible = False
        SuggestionEngine.SaveUserDebounced(force:=True)
        AppSettings.Save()
    End Sub

    Private Sub LayoutList_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles LayoutList.Opening
        LayoutList.Renderer = New LayoutList_PopUp
    End Sub

    Public Sub UpdateModeUI()
        Dim modeText As String = "English"
        If isActivated Then
            If (isPhonetic AndAlso isPhoneticSelected) OrElse isAvroSelected Then
                modeText = "Banglish"
            Else
                modeText = "Bangla"
            End If
        End If

        btnMode.Text = modeText
        UpdateTrayIcon()

        If floatKeyboard IsNot Nothing AndAlso floatKeyboard.Visible Then
            floatKeyboard.UpdateKeyLabels()
        End If
    End Sub

    Public Sub CycleMode()
        ResetInternalVals()
        Dim currentMode As String = "English"
        If isActivated Then
            If (isPhonetic AndAlso isPhoneticSelected) OrElse isAvroSelected Then
                currentMode = "Banglish"
            Else
                currentMode = "Bangla"
            End If
        End If

        If currentMode = "English" Then
            isActivated = True
            crlay = LastPhoneticTag
            isPhoneticSelected = (crlay = 9999)
            isAvroSelected = (crlay = 9998)
        ElseIf currentMode = "Banglish" Then
            isActivated = True
            crlay = LastFixedTag
            isPhoneticSelected = False
            isAvroSelected = False
        Else
            isActivated = False
        End If

        AppSettings.Activated = isActivated
        AppSettings.LastLayoutTag = crlay
        AppSettings.Save()
        UpdateModeUI()
    End Sub

    Private Sub btnMode_Click(sender As Object, e As EventArgs) Handles btnMode.Click
        CycleMode()
    End Sub

    Private Sub btnMode_MouseUp(sender As Object, e As MouseEventArgs) Handles btnMode.MouseUp
        If e.Button = MouseButtons.Right Then
            For Each it As Object In LayoutList.Items
                Dim mi As ToolStripMenuItem = TryCast(it, ToolStripMenuItem)
                If mi IsNot Nothing AndAlso mi.Tag IsNot Nothing Then
                    mi.Checked = (Convert.ToInt32(mi.Tag) = crlay)
                End If
            Next
            LayoutList.Show(btnMode, New Point(e.X, e.Y))
        End If
    End Sub

    Private Sub BuildSettingsMenu()
        settingsMenu = New ContextMenuStrip()

        Dim miThemes As New ToolStripMenuItem("Quick Theme")
        For Each name As String In ThemeManager.ListThemeNames()
            Dim tn As String = name
            Dim child As New ToolStripMenuItem(tn)
            AddHandler child.Click, Sub() ApplyTheme(tn)
            miThemes.DropDownItems.Add(child)
        Next
        settingsMenu.Items.Add(miThemes)

        Dim miSugg As New ToolStripMenuItem("Suggestion Bar")
        Dim miSuggEnable As New ToolStripMenuItem("Enabled")
        AddHandler miSuggEnable.Click, Sub()
                                           AppSettings.SuggestionsEnabled = Not AppSettings.SuggestionsEnabled
                                           AppSettings.Save()
                                           If Not AppSettings.SuggestionsEnabled Then HideSuggest()
                                       End Sub
        miSugg.DropDownItems.Add(miSuggEnable)
        settingsMenu.Items.Add(miSugg)

        Dim miHotkey As New ToolStripMenuItem("Shortcut Key")
        Dim keysList As String() = {"F12", "F10", "F9", "F8", "Ctrl + Space"}
        For Each k As String In keysList
            Dim keyName As String = k
            Dim child As New ToolStripMenuItem(keyName)
            AddHandler child.Click, Sub()
                                        AppSettings.Hotkey = keyName
                                        AppSettings.Save()
                                    End Sub
            miHotkey.DropDownItems.Add(child)
        Next
        settingsMenu.Items.Add(miHotkey)

        Dim miStartWithWindows As New ToolStripMenuItem("Start with Windows") With {.Name = "autostart"}
        AddHandler miStartWithWindows.Click, Sub()
                                                 Dim enabled As Boolean = IsAutoStartEnabled()
                                                 SetAutoStart(Not enabled)
                                             End Sub
        settingsMenu.Items.Add(miStartWithWindows)

        Dim miConverter As New ToolStripMenuItem("Text Converter...")
        AddHandler miConverter.Click, Sub()
                                          Using dlg As New ConverterWindow()
                                              dlg.ShowDialog()
                                          End Using
                                      End Sub
        settingsMenu.Items.Add(miConverter)

        Dim miAnalytics As New ToolStripMenuItem("Typing Analytics Dashboard...")
        AddHandler miAnalytics.Click, Sub()
                                          Using dlg As New AnalyticsWindow()
                                              dlg.ShowDialog()
                                          End Using
                                      End Sub
        settingsMenu.Items.Add(miAnalytics)

        Dim miDictManager As New ToolStripMenuItem("Custom Dictionary Manager...")
        AddHandler miDictManager.Click, Sub()
                                            Using dlg As New DictionaryManagerWindow()
                                                dlg.ShowDialog()
                                            End Using
                                        End Sub
        settingsMenu.Items.Add(miDictManager)

        Dim miWordCustomizer As New ToolStripMenuItem("Word Customizer...")
        AddHandler miWordCustomizer.Click, Sub()
                                               Using dlg As New WordCustomizerWindow()
                                                   dlg.ShowDialog()
                                               End Using
                                           End Sub
        settingsMenu.Items.Add(miWordCustomizer)

        Dim miLayoutBuilder As New ToolStripMenuItem("Custom Layout Builder...")
        AddHandler miLayoutBuilder.Click, Sub()
                                              Using dlg As New LayoutBuilderWindow()
                                                  dlg.ShowDialog()
                                              End Using
                                          End Sub
        settingsMenu.Items.Add(miLayoutBuilder)

        Dim miAIAssistant As New ToolStripMenuItem("AI Format Assistant...")
        AddHandler miAIAssistant.Click, Sub()
                                            Using dlg As New GeminiAIWindow()
                                                dlg.ShowDialog()
                                            End Using
                                        End Sub
        settingsMenu.Items.Add(miAIAssistant)

        Dim miUpdate As New ToolStripMenuItem("Check for Updates...")
        AddHandler miUpdate.Click, Sub() CheckForUpdates(silent:=False)
        settingsMenu.Items.Add(miUpdate)

        Dim miVoice As New ToolStripMenuItem("Voice Typing (Speech-to-Text)...")
        AddHandler miVoice.Click, Sub() TriggerVoiceTyping()
        settingsMenu.Items.Add(miVoice)

        Dim miStickers As New ToolStripMenuItem("Stickers & GIFs...")
        AddHandler miStickers.Click, Sub()
                                         Using dlg As New StickersWindow()
                                             dlg.ShowDialog()
                                         End Using
                                     End Sub
        settingsMenu.Items.Add(miStickers)

        Dim miFloating As New ToolStripMenuItem("Floating Keyboard Mode") With {.Name = "floatingKeyboard"}
        AddHandler miFloating.Click, Sub() ToggleFloatingKeyboard()
        settingsMenu.Items.Add(miFloating)

        Dim miClipboard As New ToolStripMenuItem("Clipboard Manager")
        settingsMenu.Items.Add(miClipboard)

        Dim miPhrases As New ToolStripMenuItem("Quick Phrases")
        Dim phrases As String() = {"ধন্যবাদ", "কেমন আছেন?", "আসসালামু আলাইকুম", "শুভ সকাল", "আমি ভালো আছি"}
        For Each p As String In phrases
            Dim phrase As String = p
            Dim child As New ToolStripMenuItem(phrase)
            AddHandler child.Click, Sub() PasteText(phrase)
            miPhrases.DropDownItems.Add(child)
        Next
        settingsMenu.Items.Add(miPhrases)

        AddHandler settingsMenu.Opening, Sub(sender As Object, e As System.ComponentModel.CancelEventArgs)
                                             miSuggEnable.Checked = AppSettings.SuggestionsEnabled
                                             miStartWithWindows.Checked = IsAutoStartEnabled()
                                             For Each item As ToolStripItem In miHotkey.DropDownItems
                                                 Dim cMi As ToolStripMenuItem = TryCast(item, ToolStripMenuItem)
                                                 If cMi IsNot Nothing Then
                                                     cMi.Checked = (cMi.Text = AppSettings.Hotkey)
                                                 End If
                                             Next
                                             For Each item As ToolStripItem In settingsMenu.Items
                                                 Dim cMi As ToolStripMenuItem = TryCast(item, ToolStripMenuItem)
                                                 If cMi IsNot Nothing AndAlso cMi.Name = "floatingKeyboard" Then
                                                     cMi.Checked = (floatKeyboard IsNot Nothing AndAlso floatKeyboard.Visible)
                                                 End If
                                             Next
                                             ' Update Clipboard Manager items
                                             miClipboard.DropDownItems.Clear()
                                             If ClipboardHistory.Count = 0 Then
                                                 miClipboard.DropDownItems.Add(New ToolStripMenuItem("(History Empty)") With {.Enabled = False})
                                             Else
                                                 For Each item As String In ClipboardHistory
                                                     Dim txt As String = item
                                                     Dim label As String = If(txt.Length > 20, txt.Substring(0, 17) & "...", txt)
                                                     Dim child As New ToolStripMenuItem(label)
                                                     AddHandler child.Click, Sub() PasteText(txt)
                                                     miClipboard.DropDownItems.Add(child)
                                                 Next
                                             End If
                                         End Sub

        settingsMenu.Items.Add(New ToolStripSeparator())

        Dim miLicense As New ToolStripMenuItem("License Manager...")
        AddHandler miLicense.Click, Sub()
                                        Using dlg As New LicenseWindow()
                                            dlg.ShowDialog()
                                        End Using
                                    End Sub
        settingsMenu.Items.Add(miLicense)

        Dim miAbout As New ToolStripMenuItem("About BanglaType...")
        AddHandler miAbout.Click, Sub()
                                      Using dlg As New AboutWindow()
                                          dlg.ShowDialog()
                                      End Using
                                  End Sub
        settingsMenu.Items.Add(miAbout)
    End Sub

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        If settingsMenu Is Nothing Then
            BuildSettingsMenu()
        End If
        settingsMenu.Show(btnSettings, New Point(0, btnSettings.Height))
    End Sub

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function DestroyIcon(ByVal hIcon As IntPtr) As Boolean
    End Function

    Private lastIconHandle As IntPtr = IntPtr.Zero

    Public Sub UpdateTrayIcon()
        Dim modeText As String = "E"
        Dim backColor As Color = Color.FromArgb(140, 140, 140) ' Gray for English
        If isActivated Then
            If (isPhonetic AndAlso isPhoneticSelected) OrElse isAvroSelected Then
                modeText = "Bl"
                backColor = Color.FromArgb(222, 75, 57) ' Red/Orange for Banglish
            Else
                modeText = "B"
                backColor = Color.FromArgb(0, 180, 137) ' Green/Teal for Bangla
            End If
        End If

        Try
            Using bmp As New Bitmap(16, 16)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
                    g.Clear(Color.Transparent)

                    ' Draw a nice rounded colored circle
                    Using brush As New SolidBrush(backColor)
                        g.FillEllipse(brush, 0, 0, 16, 16)
                    End Using

                    ' Draw white bold letter
                    Using font As New Font("Segoe UI", 9.0!, FontStyle.Bold)
                        Using brush As New SolidBrush(Color.White)
                            Dim sf As New StringFormat()
                            sf.Alignment = StringAlignment.Center
                            sf.LineAlignment = StringAlignment.Center
                            g.DrawString(modeText, font, brush, New RectangleF(0, -1, 16, 16), sf)
                        End Using
                    End Using
                End Using

                Dim hIcon As IntPtr = bmp.GetHicon()
                Dim newIcon As Icon = Icon.FromHandle(hIcon)
                NotifyIcon1.Icon = newIcon

                ' Destroy old icon to prevent GDI leak
                If lastIconHandle <> IntPtr.Zero Then
                    DestroyIcon(lastIconHandle)
                End If
                lastIconHandle = hIcon
            End Using
        Catch ex As Exception
            Debug.WriteLine("Failed to update tray icon: " & ex.Message)
        End Try
    End Sub

    Public Function IsAutoStartEnabled() As Boolean
        Try
            Using key As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", False)
                If key IsNot Nothing Then
                    Dim val As Object = key.GetValue("BanglaType")
                    If val IsNot Nothing AndAlso val.ToString().Contains(Application.ExecutablePath) Then
                        Return True
                    End If
                End If
            End Using
        Catch
        End Try
        Return False
    End Function

    Public Sub SetAutoStart(ByVal enable As Boolean)
        Try
            Using key As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
                If key IsNot Nothing Then
                    If enable Then
                        key.SetValue("BanglaType", """" & Application.ExecutablePath & """")
                    Else
                        key.DeleteValue("BanglaType", False)
                    End If
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Failed to modify autostart registry entry: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ClipboardTimer_Tick(sender As Object, e As EventArgs)
        Try
            If Clipboard.ContainsText() Then
                Dim txt As String = Clipboard.GetText().Trim()
                If txt.Length > 0 AndAlso txt <> lastClipboardText Then
                    lastClipboardText = txt
                    ClipboardHistory.Remove(txt)
                    ClipboardHistory.Insert(0, txt)
                    If ClipboardHistory.Count > 10 Then
                        ClipboardHistory.RemoveAt(10)
                    End If
                End If
            End If
        Catch
        End Try
    End Sub

    Public Shared Sub PasteText(ByVal text As String)
        Try
            Dim oldText As String = ""
            Dim hadText As Boolean = Clipboard.ContainsText()
            If hadText Then oldText = Clipboard.GetText()

            Clipboard.SetText(text)

            System.Threading.Thread.Sleep(50)
            SendKeys.SendWait("^v")

            Dim t As New Timer()
            t.Interval = 500
            AddHandler t.Tick, Sub(sender As Object, e As EventArgs)
                                   t.Stop()
                                   Try
                                       If hadText AndAlso Not String.IsNullOrEmpty(oldText) Then
                                           Clipboard.SetText(oldText)
                                       Else
                                           Clipboard.Clear()
                                       End If
                                   Catch
                                   End Try
                                   t.Dispose()
                               End Sub
            t.Start()
        Catch
            SendKeys.SendWait(text)
        End Try
    End Sub

    Private httpListener As System.Net.HttpListener
    Private listenerThread As System.Threading.Thread

    Public Sub StartVoiceListener()
        Try
            If httpListener IsNot Nothing AndAlso httpListener.IsListening Then Return
            
            httpListener = New System.Net.HttpListener()
            httpListener.Prefixes.Add("http://localhost:9001/")
            httpListener.Start()
            
            listenerThread = New System.Threading.Thread(AddressOf ListenLoop)
            listenerThread.IsBackground = True
            listenerThread.Start()
        Catch ex As Exception
            Debug.WriteLine("Failed to start voice listener: " & ex.Message)
        End Try
    End Sub

    Private Sub ListenLoop()
        While httpListener IsNot Nothing AndAlso httpListener.IsListening
            Try
                Dim context As System.Net.HttpListenerContext = httpListener.GetContext()
                Dim req As System.Net.HttpListenerRequest = context.Request
                Dim resp As System.Net.HttpListenerResponse = context.Response
                
                Dim text As String = req.QueryString("text")
                If Not String.IsNullOrEmpty(text) Then
                    Me.BeginInvoke(Sub()
                                       PasteText(text)
                                   End Sub)
                End If
                
                resp.Headers.Add("Access-Control-Allow-Origin", "*")
                Dim buffer() As Byte = System.Text.Encoding.UTF8.GetBytes("OK")
                resp.ContentLength64 = buffer.Length
                resp.OutputStream.Write(buffer, 0, buffer.Length)
                resp.OutputStream.Close()
            Catch
            End Try
        End While
    End Sub

    Private Function GetVoiceTypingHtmlPath() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "voice_typing.html")
    End Function

    Private Sub EnsureVoiceTypingHtml()
        Try
            Dim folder As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType")
            Directory.CreateDirectory(folder)
            Dim htmlPath As String = GetVoiceTypingHtmlPath()
            
            Dim sb As New StringBuilder()
            sb.AppendLine("<!DOCTYPE html>")
            sb.AppendLine("<html lang=""en"">")
            sb.AppendLine("<head>")
            sb.AppendLine("    <meta charset=""UTF-8"">")
            sb.AppendLine("    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">")
            sb.AppendLine("    <title>BanglaType Voice Typing</title>")
            sb.AppendLine("    <style>")
            sb.AppendLine("        body {")
            sb.AppendLine("            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;")
            sb.AppendLine("            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);")
            sb.AppendLine("            display: flex;")
            sb.AppendLine("            justify-content: center;")
            sb.AppendLine("            align-items: center;")
            sb.AppendLine("            height: 100vh;")
            sb.AppendLine("            margin: 0;")
            sb.AppendLine("        }")
            sb.AppendLine("        .card {")
            sb.AppendLine("            background: rgba(255, 255, 255, 0.85);")
            sb.AppendLine("            backdrop-filter: blur(10px);")
            sb.AppendLine("            border-radius: 20px;")
            sb.AppendLine("            padding: 30px;")
            sb.AppendLine("            box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.15);")
            sb.AppendLine("            border: 1px solid rgba(255, 255, 255, 0.18);")
            sb.AppendLine("            text-align: center;")
            sb.AppendLine("            width: 400px;")
            sb.AppendLine("        }")
            sb.AppendLine("        h2 {")
            sb.AppendLine("            color: #333;")
            sb.AppendLine("            margin-bottom: 20px;")
            sb.AppendLine("        }")
            sb.AppendLine("        .mic-btn {")
            sb.AppendLine("            background-color: #DE4B39;")
            sb.AppendLine("            color: white;")
            sb.AppendLine("            border: none;")
            sb.AppendLine("            border-radius: 50%;")
            sb.AppendLine("            width: 80px;")
            sb.AppendLine("            height: 80px;")
            sb.AppendLine("            font-size: 32px;")
            sb.AppendLine("            cursor: pointer;")
            sb.AppendLine("            outline: none;")
            sb.AppendLine("            box-shadow: 0 4px 15px rgba(222, 75, 57, 0.4);")
            sb.AppendLine("            transition: all 0.3s ease;")
            sb.AppendLine("            position: relative;")
            sb.AppendLine("        }")
            sb.AppendLine("        .mic-btn.active {")
            sb.AppendLine("            animation: pulse 1.5s infinite;")
            sb.AppendLine("            background-color: #00B489;")
            sb.AppendLine("            box-shadow: 0 4px 15px rgba(0, 180, 137, 0.4);")
            sb.AppendLine("        }")
            sb.AppendLine("        @keyframes pulse {")
            sb.AppendLine("            0% { transform: scale(1); }")
            sb.AppendLine("            50% { transform: scale(1.1); }")
            sb.AppendLine("            100% { transform: scale(1); }")
            sb.AppendLine("        }")
            sb.AppendLine("        .status {")
            sb.AppendLine("            margin-top: 15px;")
            sb.AppendLine("            font-size: 14px;")
            sb.AppendLine("            color: #666;")
            sb.AppendLine("            font-weight: 500;")
            sb.AppendLine("        }")
            sb.AppendLine("        .transcript-box {")
            sb.AppendLine("            margin-top: 20px;")
            sb.AppendLine("            background: white;")
            sb.AppendLine("            border-radius: 10px;")
            sb.AppendLine("            padding: 15px;")
            sb.AppendLine("            height: 120px;")
            sb.AppendLine("            overflow-y: auto;")
            sb.AppendLine("            border: 1px solid #ddd;")
            sb.AppendLine("            text-align: left;")
            sb.AppendLine("            font-size: 16px;")
            sb.AppendLine("            color: #333;")
            sb.AppendLine("        }")
            sb.AppendLine("        .language-select {")
            sb.AppendLine("            margin-top: 15px;")
            sb.AppendLine("            padding: 5px 10px;")
            sb.AppendLine("            border-radius: 5px;")
            sb.AppendLine("            border: 1px solid #ccc;")
            sb.AppendLine("            font-size: 14px;")
            sb.AppendLine("        }")
            sb.AppendLine("    </style>")
            sb.AppendLine("</head>")
            sb.AppendLine("<body>")
            sb.AppendLine("    <div class=""card"">")
            sb.AppendLine("        <h2>BanglaType Voice Typing</h2>")
            sb.AppendLine("        <button id=""micBtn"" class=""mic-btn"">🎤</button>")
            sb.AppendLine("        <div id=""status"" class=""status"">Click the microphone to start speaking</div>")
            sb.AppendLine("        <select id=""langSelect"" class=""language-select"">")
            sb.AppendLine("            <option value=""bn-BD"">Bangla (Bangladesh)</option>")
            sb.AppendLine("            <option value=""en-US"">English (United States)</option>")
            sb.AppendLine("        </select>")
            sb.AppendLine("        <textarea id=""transcript"" class=""transcript-box"">Your speech will appear here... Feel free to edit this text before inserting.</textarea>")
            sb.AppendLine("        <button id=""insertBtn"" class=""insert-btn"">📋 Insert to Active App</button>")
            sb.AppendLine("    </div>")
            sb.AppendLine("    <script>")
            sb.AppendLine("        const micBtn = document.getElementById('micBtn');")
            sb.AppendLine("        const insertBtn = document.getElementById('insertBtn');")
            sb.AppendLine("        const status = document.getElementById('status');")
            sb.AppendLine("        const transcriptDiv = document.getElementById('transcript');")
            sb.AppendLine("        const langSelect = document.getElementById('langSelect');")
            sb.AppendLine("        let recognition;")
            sb.AppendLine("        let isListening = false;")
            sb.AppendLine("        if ('webkitSpeechRecognition' in window || 'SpeechRecognition' in window) {")
            sb.AppendLine("            const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;")
            sb.AppendLine("            recognition = new SpeechRecognition();")
            sb.AppendLine("            recognition.continuous = true;")
            sb.AppendLine("            recognition.interimResults = false;")
            sb.AppendLine("            recognition.onstart = () => {")
            sb.AppendLine("                isListening = true;")
            sb.AppendLine("                micBtn.classList.add('active');")
            sb.AppendLine("                status.innerText = ""Listening... Speak now"";")
            sb.AppendLine("            };")
            sb.AppendLine("            recognition.onend = () => {")
            sb.AppendLine("                isListening = false;")
            sb.AppendLine("                micBtn.classList.remove('active');")
            sb.AppendLine("                status.innerText = ""Stopped listening. Click to start again"";")
            sb.AppendLine("                if (window.location.search.includes('mode=hidden')) {")
            sb.AppendLine("                    setTimeout(() => {")
            sb.AppendLine("                        try { recognition.start(); } catch(e) {}")
            sb.AppendLine("                    }, 500);")
            sb.AppendLine("                }")
            sb.AppendLine("            };")
            sb.AppendLine("            recognition.onresult = (event) => {")
            sb.AppendLine("                const result = event.results[event.results.length - 1][0].transcript.trim();")
            sb.AppendLine("                if (window.location.search.includes('mode=hidden')) {")
            sb.AppendLine("                    fetch(`http://localhost:9001/?text=${encodeURIComponent(result + ' ')}`)")
            sb.AppendLine("                        .catch(err => console.error(err));")
            sb.AppendLine("                }")
            sb.AppendLine("                if (transcriptDiv.value.startsWith('Your speech')) {")
            sb.AppendLine("                    transcriptDiv.value = result;")
            sb.AppendLine("                } else {")
            sb.AppendLine("                    transcriptDiv.value += ' ' + result;")
            sb.AppendLine("                }")
            sb.AppendLine("            };")
            sb.AppendLine("            recognition.onerror = (event) => {")
            sb.AppendLine("                console.error(""Speech recognition error:"", event.error);")
            sb.AppendLine("                status.innerText = ""Error: "" + event.error;")
            sb.AppendLine("            };")
            sb.AppendLine("        } else {")
            sb.AppendLine("            status.innerText = ""Speech recognition is not supported in this browser. Please use Chrome or Edge."";")
            sb.AppendLine("            micBtn.disabled = true;")
            sb.AppendLine("        }")
            sb.AppendLine("        micBtn.addEventListener('click', () => {")
            sb.AppendLine("            if (isListening) {")
            sb.AppendLine("                recognition.stop();")
            sb.AppendLine("            } else {")
            sb.AppendLine("                recognition.lang = langSelect.value;")
            sb.AppendLine("                recognition.start();")
            sb.AppendLine("            }")
            sb.AppendLine("        });")
            sb.AppendLine("        insertBtn.addEventListener('click', () => {")
            sb.AppendLine("            const text = transcriptDiv.value;")
            sb.AppendLine("            fetch(`http://localhost:9001/?text=${encodeURIComponent(text)}`)")
            sb.AppendLine("                .catch(err => console.log(""Failed to send text to app:"", err));")
            sb.AppendLine("        });")
            sb.AppendLine("        window.addEventListener('DOMContentLoaded', () => {")
            sb.AppendLine("            const urlParams = new URLSearchParams(window.location.search);")
            sb.AppendLine("            if (urlParams.get('mode') === 'hidden') {")
            sb.AppendLine("                const lang = urlParams.get('lang') || 'bn-BD';")
            sb.AppendLine("                langSelect.value = lang;")
            sb.AppendLine("                recognition.lang = lang;")
            sb.AppendLine("                isListening = true;")
            sb.AppendLine("                recognition.start();")
            sb.AppendLine("            }")
            sb.AppendLine("        });")
            sb.AppendLine("    </script>")
            sb.AppendLine("</body>")
            sb.AppendLine("</html>")
            
            File.WriteAllText(htmlPath, sb.ToString(), Encoding.UTF8)
        Catch
        End Try
    End Sub

    Public Shared Sub ToggleFloatingKeyboard()
        If floatKeyboard Is Nothing OrElse floatKeyboard.IsDisposed Then
            floatKeyboard = New FloatingKeyboardWindow()
        End If
        If floatKeyboard.Visible Then
            floatKeyboard.Hide()
        Else
            floatKeyboard.Show()
            floatKeyboard.UpdateKeyLabels()
        End If
    End Sub

    Public Sub ReloadAllLayouts()
        LayoutList.Items.Clear()
        
        Dim filePath = Path.Combine(Application.StartupPath & "\data\lib\", "libcpphonetic.dll")
        If File.Exists(filePath) Then
            Dim dynItem As New ToolStripMenuItem() With {.Text = " BanglaType Phonetic", .Name = "bp", .Tag = 9999}
            AddHandler dynItem.Click, AddressOf mnuItem_Clicked
            LayoutList.Items.Add(dynItem)
        Else
            isPhonetic = False
        End If

        If AppSettings.AvroEnabled Then
            Dim avroItem As New ToolStripMenuItem() With {.Text = " Avro Phonetic (Open)", .Name = "avro", .Tag = AVRO_TAG}
            AddHandler avroItem.Click, AddressOf mnuItem_Clicked
            LayoutList.Items.Add(avroItem)
        End If

        LayoutParser.SearchForLayouts()
        BuildTrayMenu()
    End Sub

    Private Sub TriggerVoiceTyping()
        StartVoiceListener()
        EnsureVoiceTypingHtml()
        Try
            Process.Start(GetVoiceTypingHtmlPath())
        Catch ex As Exception
            MessageBox.Show("Failed to open speech recognizer: " & ex.Message)
        End Try
    End Sub

    Private Function GetCurrentLangCode() As String
        If isActivated Then
            Return "bn-BD"
        Else
            Return "en-US"
        End If
    End Function

    Private Function GetBrowserPath() As String
        Return "msedge.exe"
    End Function

    Private Sub btnVoice_Click(sender As Object, e As EventArgs) Handles btnVoice.Click
        TriggerVoiceTyping()
    End Sub

    Public Shared Sub CheckForUpdates(ByVal silent As Boolean)
        Dim t As New System.Threading.Thread(Sub()
            Try
                Dim updateUrl As String = "https://raw.githubusercontent.com/shahinur/borno-lite/main/update.json"
                Dim request As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(updateUrl), System.Net.HttpWebRequest)
                request.Method = "GET"
                request.Timeout = 5000
                Using response As System.Net.WebResponse = request.GetResponse()
                    Using stream As System.IO.Stream = response.GetResponseStream()
                        Using reader As New System.IO.StreamReader(stream, Encoding.UTF8)
                            Dim json As String = reader.ReadToEnd()
                            Dim versionStr As String = ""
                            Dim downloadUrl As String = ""
                            
                            Dim vMatch = System.Text.RegularExpressions.Regex.Match(json, """version""\s*:\s*""([^""]+)""")
                            If vMatch.Success Then versionStr = vMatch.Groups(1).Value
                            
                            Dim uMatch = System.Text.RegularExpressions.Regex.Match(json, """url""\s*:\s*""([^""]+)""")
                            If uMatch.Success Then downloadUrl = uMatch.Groups(1).Value
                            
                            If Not String.IsNullOrEmpty(versionStr) AndAlso Not String.IsNullOrEmpty(downloadUrl) Then
                                Dim currentVer As New Version(Application.ProductVersion)
                                Dim remoteVer As New Version(versionStr)
                                
                                If remoteVer > currentVer Then
                                    Dim main As Form = Application.OpenForms.Cast(Of Form)().FirstOrDefault(Function(f) TypeOf f Is MainUI)
                                    If main IsNot Nothing Then
                                        main.BeginInvoke(Sub()
                                            Dim res As DialogResult = MessageBox.Show(main, 
                                                "A new version of BanglaType (v" & versionStr & ") is available." & vbCrLf & _
                                                "Would you like to download and install it now?", 
                                                "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
                                            If res = DialogResult.Yes Then
                                                Try
                                                    System.Diagnostics.Process.Start(downloadUrl)
                                                Catch ex As Exception
                                                    MessageBox.Show("Failed to open update link: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                                End Try
                                            End If
                                        End Sub)
                                    End If
                                Else
                                    If Not silent Then
                                        Dim main As Form = Application.OpenForms.Cast(Of Form)().FirstOrDefault(Function(f) TypeOf f Is MainUI)
                                        If main IsNot Nothing Then
                                            main.BeginInvoke(Sub()
                                                MessageBox.Show(main, "You are using the latest version of BanglaType (v" & Application.ProductVersion & ").", "Up to Date", MessageBoxButtons.OK, MessageBoxIcon.Information)
                                            End Sub)
                                        End If
                                    End If
                                End If
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                If Not silent Then
                    Dim main As Form = Application.OpenForms.Cast(Of Form)().FirstOrDefault(Function(f) TypeOf f Is MainUI)
                    If main IsNot Nothing Then
                        main.BeginInvoke(Sub()
                            MessageBox.Show(main, "Failed to check for updates: " & ex.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        End Sub)
                    End If
                End If
            End Try
        End Sub)
        t.IsBackground = True
        t.Start()
    End Sub
End Class
