Imports System.Windows.Forms
Imports System.Drawing

Public Class SetupWizardWindow
    Inherits Form

    ' Wizard steps/panels
    Private panels As New List(Of Panel)()
    Private currentStep As Integer = 0

    ' Navigation Controls
    Private btnNext As Button
    Private btnBack As Button
    Private lblStepTitle As Label
    Private pnlProgress As Panel
    Private pnlDots As Panel

    ' Options Controls
    ' Step 1: Language
    Private radLangEn As RadioButton
    Private radLangBn As RadioButton
    
    ' Step 2: Theme
    Private lstThemes As ListBox

    ' Step 3: Banglish Preference
    Private chkBanglish As CheckBox

    ' Step 4: Privacy
    Private chkPrivacy As CheckBox

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "BanglaType Keyboard - First Run Setup"
        Me.Size = New Size(500, 420)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowIcon = False
        Me.BackColor = Color.FromArgb(245, 246, 248)
        
        ' Header/Title
        lblStepTitle = New Label() With {
            .Font = New Font("Segoe UI", 14.0!, FontStyle.Bold),
            .ForeColor = Color.FromArgb(51, 51, 51),
            .Location = New Point(20, 20),
            .Size = New Size(460, 30),
            .TextAlign = ContentAlignment.MiddleLeft
        }
        Me.Controls.Add(lblStepTitle)

        ' Create steps
        CreateStep1_Language()
        CreateStep2_Theme()
        CreateStep3_Preferences()
        CreateStep4_Privacy()

        ' Progress Indicator (Dots)
        pnlDots = New Panel() With {
            .Location = New Point(20, 325),
            .Size = New Size(200, 25),
            .BackColor = Color.Transparent
        }
        Me.Controls.Add(pnlDots)
        AddHandler pnlDots.Paint, AddressOf pnlDots_Paint

        ' Back Button
        btnBack = New Button() With {
            .Text = "← Back",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .Location = New Point(280, 320),
            .Size = New Size(90, 32),
            .FlatStyle = FlatStyle.Flat,
            .BackColor = Color.White,
            .ForeColor = Color.FromArgb(100, 100, 100)
        }
        btnBack.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200)
        AddHandler btnBack.Click, AddressOf btnBack_Click
        Me.Controls.Add(btnBack)

        ' Next Button
        btnNext = New Button() With {
            .Text = "Next →",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Bold),
            .Location = New Point(380, 320),
            .Size = New Size(90, 32),
            .FlatStyle = FlatStyle.Flat,
            .BackColor = Color.FromArgb(0, 180, 137),
            .ForeColor = Color.White
        }
        btnNext.FlatAppearance.BorderSize = 0
        AddHandler btnNext.Click, AddressOf btnNext_Click
        Me.Controls.Add(btnNext)

        ShowStep(0)
    End Sub

    Private Sub CreateStep1_Language()
        Dim pnl As New Panel() With {
            .Location = New Point(20, 70),
            .Size = New Size(445, 230),
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }
        
        Dim lblDesc As New Label() With {
            .Text = "Welcome to BanglaType! Select your primary display language below:",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Regular),
            .ForeColor = Color.FromArgb(80, 80, 80),
            .Location = New Point(15, 20),
            .Size = New Size(415, 45)
        }
        pnl.Controls.Add(lblDesc)

        radLangEn = New RadioButton() With {
            .Text = " English (Recommended)",
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Bold),
            .Location = New Point(40, 80),
            .Size = New Size(300, 30),
            .Checked = True,
            .ForeColor = Color.FromArgb(51, 51, 51)
        }
        pnl.Controls.Add(radLangEn)

        radLangBn = New RadioButton() With {
            .Text = " বাংলা (Bengali)",
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Bold),
            .Location = New Point(40, 130),
            .Size = New Size(300, 30),
            .ForeColor = Color.FromArgb(51, 51, 51)
        }
        pnl.Controls.Add(radLangBn)

        panels.Add(pnl)
        Me.Controls.Add(pnl)
    End Sub

    Private Sub CreateStep2_Theme()
        Dim pnl As New Panel() With {
            .Location = New Point(20, 70),
            .Size = New Size(445, 230),
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblDesc As New Label() With {
            .Text = "Choose your keyboard theme style. You can change this later in settings:",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Regular),
            .ForeColor = Color.FromArgb(80, 80, 80),
            .Location = New Point(15, 15),
            .Size = New Size(415, 40)
        }
        pnl.Controls.Add(lblDesc)

        lstThemes = New ListBox() With {
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Regular),
            .Location = New Point(30, 60),
            .Size = New Size(385, 140),
            .BorderStyle = BorderStyle.FixedSingle
        }
        
        Try
            For Each themeName As String In ThemeManager.ListThemeNames()
                lstThemes.Items.Add(themeName)
            Next
            If lstThemes.Items.Count > 0 Then
                lstThemes.SelectedIndex = 0
            End If
        Catch
            lstThemes.Items.Add("BanglaType Cream")
            lstThemes.Items.Add("BanglaType Charcoal")
            lstThemes.SelectedIndex = 0
        End Try

        pnl.Controls.Add(lstThemes)
        panels.Add(pnl)
        Me.Controls.Add(pnl)
    End Sub

    Private Sub CreateStep3_Preferences()
        Dim pnl As New Panel() With {
            .Location = New Point(20, 70),
            .Size = New Size(445, 230),
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblDesc As New Label() With {
            .Text = "Configure typing preferences:",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.FromArgb(51, 51, 51),
            .Location = New Point(15, 15),
            .Size = New Size(415, 25)
        }
        pnl.Controls.Add(lblDesc)

        chkBanglish = New CheckBox() With {
            .Text = "Enable Banglish Transliteration Mode by default",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .Location = New Point(25, 55),
            .Size = New Size(395, 30),
            .Checked = True,
            .ForeColor = Color.FromArgb(51, 51, 51)
        }
        pnl.Controls.Add(chkBanglish)

        Dim lblBanglishDesc As New Label() With {
            .Text = "Allows you to type Bangla using English letters (e.g. typing 'ami' produces 'আমি'). Switchable anytime with F12.",
            .Font = New Font("Segoe UI", 8.5!, FontStyle.Regular),
            .ForeColor = Color.FromArgb(120, 120, 120),
            .Location = New Point(45, 85),
            .Size = New Size(375, 45)
        }
        pnl.Controls.Add(lblBanglishDesc)

        Dim chkSugg As New CheckBox() With {
            .Text = "Enable Predictive Suggestion Bar",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .Location = New Point(25, 140),
            .Size = New Size(395, 30),
            .Checked = True,
            .ForeColor = Color.FromArgb(51, 51, 51)
        }
        pnl.Controls.Add(chkSugg)

        Dim lblSuggDesc As New Label() With {
            .Text = "Shows real-time word predictions, autocomplete suggestions, and AI corrections.",
            .Font = New Font("Segoe UI", 8.5!, FontStyle.Regular),
            .ForeColor = Color.FromArgb(120, 120, 120),
            .Location = New Point(45, 170),
            .Size = New Size(375, 30)
        }
        pnl.Controls.Add(lblSuggDesc)

        panels.Add(pnl)
        Me.Controls.Add(pnl)
    End Sub

    Private Sub CreateStep4_Privacy()
        Dim pnl As New Panel() With {
            .Location = New Point(20, 70),
            .Size = New Size(445, 230),
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblDesc As New Label() With {
            .Text = "Privacy & Safety Agreement",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .ForeColor = Color.FromArgb(51, 51, 51),
            .Location = New Point(15, 15),
            .Size = New Size(415, 25)
        }
        pnl.Controls.Add(lblDesc)

        Dim txtPrivacy As New TextBox() With {
            .Multiline = True,
            .ReadOnly = True,
            .ScrollBars = ScrollBars.Vertical,
            .Font = New Font("Segoe UI", 8.5!, FontStyle.Regular),
            .Location = New Point(20, 45),
            .Size = New Size(405, 110),
            .BackColor = Color.FromArgb(250, 250, 250),
            .ForeColor = Color.FromArgb(100, 100, 100),
            .Text = "BanglaType values your privacy. " & vbCrLf & _
                    "1. Your keystrokes and typing history are stored locally on your device and are never transmitted to any external server." & vbCrLf & _
                    "2. The optional voice typing features use standard local APIs and only connect to local listener services." & vbCrLf & _
                    "3. If you decide to use AI assistant features, only text queries you explicitly send are sent to the Gemini API securely." & vbCrLf & _
                    "4. To help us make the software more stable, we may collect anonymous crash statistics. No personally identifiable information is ever included."
        }
        pnl.Controls.Add(txtPrivacy)

        chkPrivacy = New CheckBox() With {
            .Text = "I agree to the privacy statement and offline usage policies.",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Bold),
            .Location = New Point(20, 175),
            .Size = New Size(405, 30),
            .ForeColor = Color.FromArgb(0, 180, 137)
        }
        pnl.Controls.Add(chkPrivacy)

        panels.Add(pnl)
        Me.Controls.Add(pnl)
    End Sub

    Private Sub ShowStep(ByVal stepIndex As Integer)
        currentStep = stepIndex
        
        ' Set titles
        Select Case currentStep
            Case 0
                lblStepTitle.Text = "Step 1 of 4: Choose Display Language"
            Case 1
                lblStepTitle.Text = "Step 2 of 4: Select Interface Theme"
            Case 2
                lblStepTitle.Text = "Step 3 of 4: Keyboard Preferences"
            Case 3
                lblStepTitle.Text = "Step 4 of 4: Privacy & Consent"
        End Select

        ' Show/Hide panels
        For i As Integer = 0 To panels.Count - 1
            panels(i).Visible = (i = currentStep)
        Next

        ' Update Navigation Buttons
        btnBack.Enabled = (currentStep > 0)
        
        If currentStep = panels.Count - 1 Then
            btnNext.Text = "Finish ✓"
            btnNext.BackColor = Color.FromArgb(0, 180, 137)
        Else
            btnNext.Text = "Next →"
            btnNext.BackColor = Color.FromArgb(0, 180, 137)
        End If

        pnlDots.Invalidate()
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs)
        If currentStep > 0 Then
            ShowStep(currentStep - 1)
        End If
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs)
        ' Validate Step 4
        If currentStep = 3 Then
            If Not chkPrivacy.Checked Then
                MessageBox.Show("Please review and check the privacy agreement checkbox to complete setup.", "Privacy Consent Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Save options
            SaveWizardSettings()
            Me.DialogResult = DialogResult.OK
            Me.Close()
            Return
        End If

        ShowStep(currentStep + 1)
    End Sub

    Private Sub SaveWizardSettings()
        Try
            ' 1. Language
            AppSettings.DefaultLang = If(radLangBn.Checked, "bn", "en")
            
            ' 2. Theme
            If lstThemes.SelectedItem IsNot Nothing Then
                AppSettings.ThemeName = lstThemes.SelectedItem.ToString()
            End If

            ' 3. Banglish
            AppSettings.Activated = chkBanglish.Checked
            If chkBanglish.Checked Then
                AppSettings.LastLayoutTag = 9999 ' Phonetic
            End If

            ' 4. Privacy Consent
            AppSettings.PrivacyConsent = chkPrivacy.Checked
            
            ' Setup completed
            AppSettings.FirstRun = False
            AppSettings.Save()

            ' Apply theme instantly if possible
            If System.Windows.Forms.Application.OpenForms.Count > 0 Then
                Dim main As MainUI = TryCast(System.Windows.Forms.Application.OpenForms(0), MainUI)
                If main IsNot Nothing Then
                    main.ApplyTheme(AppSettings.ThemeName)
                    main.UpdateModeUI()
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to save wizard settings: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub pnlDots_Paint(sender As Object, e As PaintEventArgs)
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
        
        Dim dotSize As Integer = 10
        Dim dotSpacing As Integer = 18
        Dim startX As Integer = 10
        Dim startY As Integer = (pnlDots.Height - dotSize) \ 2

        For i As Integer = 0 To panels.Count - 1
            Dim x As Integer = startX + (i * dotSpacing)
            Using brush As New SolidBrush(If(i = currentStep, Color.FromArgb(0, 180, 137), Color.FromArgb(200, 200, 200)))
                e.Graphics.FillEllipse(brush, x, startY, dotSize, dotSize)
            End Using
        Next
    End Sub
End Class
