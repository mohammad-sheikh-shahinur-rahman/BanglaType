Imports System.Windows.Forms
Imports System.Drawing

Public Class AboutWindow
    Inherits Form

    Private lblTitle As Label
    Private lblDev As Label
    Private txtFeatures As TextBox
    Private btnClose As Button

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "About BanglaType"
        Me.Size = New Size(500, 520)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowIcon = False
        Me.ShowInTaskbar = True

        ' Title Label
        lblTitle = New Label() With {
            .Text = "BanglaType Keyboard v" & Application.ProductVersion,
            .Font = New Font("Segoe UI", 16.0!, FontStyle.Bold),
            .Location = New Point(15, 15),
            .Size = New Size(470, 35)
        }

        ' Developer Label
        lblDev = New Label() With {
            .Text = "Created by Mohammad Sheikh Shahinur Rahman" & vbCrLf & _
                    "Software Engineer | CTO | DevOps Architect | Independent Researcher | Writer & Poet" & vbCrLf & _
                    "Website: shahinurrahman.com  |  LinkedIn: mohammad-sheikh-shahinur-rahman" & vbCrLf & _
                    "Version: " & Application.ProductVersion & " (Release Build) | Copyright © Mohammad Sheikh Shahinur Rahman.",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .Location = New Point(15, 55),
            .Size = New Size(470, 70)
        }

        ' Features TextBox
        txtFeatures = New TextBox() With {
            .Multiline = True,
            .ReadOnly = True,
            .ScrollBars = ScrollBars.Vertical,
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .Location = New Point(15, 135),
            .Size = New Size(455, 275),
            .BackColor = Color.White,
            .ForeColor = Color.Black
        }

        ' Build Features Text
        Dim ft As New System.Text.StringBuilder()
        ft.AppendLine("⌨️ BanglaType Keyboard – User Documentation")
        ft.AppendLine()
        ft.AppendLine("BanglaType Keyboard is a modern, intelligent, and multilingual keyboard designed to make typing in Bangla, English, and Banglish faster, easier, and more expressive.")
        ft.AppendLine()
        ft.AppendLine("📖 QUICK START GUIDE")
        ft.AppendLine()
        ft.AppendLine("1. Hotkey Toggle:")
        ft.AppendLine("Press [F12] (or your customized hotkey) to cycle between English, Bangla, and Banglish modes.")
        ft.AppendLine()
        ft.AppendLine("2. Typing Modes:")
        ft.AppendLine("   • English: Standard QWERTY layout input.")
        ft.AppendLine("   • Bangla: Standard fixed layouts (National, BanglaType, etc.) or Avro Phonetic layout.")
        ft.AppendLine("   • Banglish: Type Bangla phonetically using English characters (e.g. typing 'ami' outputting 'আমি').")
        ft.AppendLine()
        ft.AppendLine("3. Voice Typing Mode (Speech-to-Text):")
        ft.AppendLine("Click the microphone button [🎤] next to settings. It opens a browser-based speech typing portal that allows you to speak (supporting both Bangla and English) and automatically paste text into your active target application.")
        ft.AppendLine()
        ft.AppendLine("4. Safe Mode Launch:")
        ft.AppendLine("If the application ever experiences startup instability due to platform dependencies or conflicting programs, launch the app with the command-line argument: --safe. This disables advanced suggestions and clipboard hooks, keeping typing completely stable.")
        ft.AppendLine()
        ft.AppendLine("🚀 CORE PRODUCTIVITY FEATURES")
        ft.AppendLine()
        ft.AppendLine("• AI-Powered Next Word Prediction & Spellchecking")
        ft.AppendLine("  Supports both offline local dictionaries and online suggestions via Google Input Tools and Gemini AI API.")
        ft.AppendLine()
        ft.AppendLine("• Custom Layout Builder")
        ft.AppendLine("  Design your own keyboard layout from the settings and compile it into a standard '.kbl' file.")
        ft.AppendLine("  Open Settings -> Custom Layout Builder to create your own configuration.")
        ft.AppendLine()
        ft.AppendLine("• Word Variation Customizer")
        ft.AppendLine("  Teach the phonetic engine your custom Romanized spellings. For example, if you prefer 'bangla' or 'vangla' for 'বাংলা', customize it in settings.")
        ft.AppendLine()
        ft.AppendLine("• Clipboard History Manager")
        ft.AppendLine("  Keeps a cache of your last 10 copied texts. Access it from the Settings -> Clipboard Manager menu to quickly paste historic clips.")
        ft.AppendLine()
        ft.AppendLine("• Personalization Themes")
        ft.AppendLine("  Includes built-in theme presets such as Cream, Charcoal, Dark, and Glassmorphism. Quick theme switching from the settings menu.")
        ft.AppendLine()
        ft.AppendLine("🔐 PRIVACY & COMPLIANCE")
        ft.AppendLine()
        ft.AppendLine("• BanglaType is 100% offline-first. Your keystrokes and typing patterns are processed and saved locally on your machine and are never transmitted.")
        ft.AppendLine("• No telemetry, tracking, or ads are included in the application.")
        
        txtFeatures.Text = ft.ToString()
        ' Selection at start to avoid focusing and highlighting text
        txtFeatures.SelectionStart = 0
        txtFeatures.SelectionLength = 0

        ' Close Button
        btnClose = New Button() With {
            .Text = "Close",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .Location = New Point(395, 430),
            .Size = New Size(75, 30),
            .FlatStyle = FlatStyle.Flat
        }
        AddHandler btnClose.Click, Sub(sender, e) Me.Close()

        ' Add Controls
        Me.Controls.Add(lblTitle)
        Me.Controls.Add(lblDev)
        Me.Controls.Add(txtFeatures)
        Me.Controls.Add(btnClose)

        ' Apply Current Theme if available
        Try
            If System.Windows.Forms.Application.OpenForms.Count > 0 Then
                Dim main As MainUI = TryCast(System.Windows.Forms.Application.OpenForms(0), MainUI)
                If main IsNot Nothing Then
                    Me.BackColor = main.ThemeTopbarBack
                    lblTitle.ForeColor = main.currentButtonFore
                    lblDev.ForeColor = main.currentButtonFore
                    btnClose.BackColor = main.ThemeBorderColor
                    btnClose.ForeColor = main.currentButtonFore
                    btnClose.FlatAppearance.BorderColor = main.ThemeBorderColor
                End If
            End If
        Catch
        End Try
    End Sub
End Class
