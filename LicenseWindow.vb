Imports System.Windows.Forms
Imports System.Drawing
Imports System.IO

Public Class LicenseWindow
    Inherits Form

    Private lblTitle As Label
    Private lblVersion As Label
    Private txtLicense As TextBox
    Private btnClose As Button

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "BanglaType License Manager"
        Me.Size = New Size(500, 550)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowIcon = False
        Me.ShowInTaskbar = True

        ' Title Label
        lblTitle = New Label()
        lblTitle.Text = "BanglaType End User License Agreement"
        lblTitle.Font = New Font("Segoe UI", 12.0!, FontStyle.Bold)
        lblTitle.Location = New Point(15, 14)
        lblTitle.Size = New Size(470, 26)

        ' Version subtitle — always reflects the running build's version.
        lblVersion = New Label()
        lblVersion.Text = "Version " & Application.ProductVersion & "  •  Release Build"
        lblVersion.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
        lblVersion.Location = New Point(17, 41)
        lblVersion.Size = New Size(470, 18)

        ' License TextBox
        txtLicense = New TextBox()
        txtLicense.Multiline = True
        txtLicense.ReadOnly = True
        txtLicense.ScrollBars = ScrollBars.Vertical
        txtLicense.Font = New Font("Consolas", 9.0!, FontStyle.Regular)
        txtLicense.Location = New Point(15, 64)
        txtLicense.Size = New Size(455, 388)
        txtLicense.BackColor = Color.White
        txtLicense.ForeColor = Color.Black

        ' Load License Text
        Dim licenseText As String = ""
        Dim licensePath As String = Path.Combine(Application.StartupPath, "LICENSE")
        If File.Exists(licensePath) Then
            Try
                licenseText = File.ReadAllText(licensePath)
            Catch ex As Exception
                licenseText = GetDefaultLicenseText()
            End Try
        Else
            ' Try project root directory folder fallback for testing in development
            Dim projectLicensePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "LICENSE")
            If File.Exists(projectLicensePath) Then
                Try
                    licenseText = File.ReadAllText(projectLicensePath)
                Catch
                    licenseText = GetDefaultLicenseText()
                End Try
            Else
                licenseText = GetDefaultLicenseText()
            End If
        End If

        ' Always reflect the running build's version in the EULA, regardless of
        ' what the on-disk LICENSE file or the embedded fallback happens to say.
        licenseText = StampCurrentVersion(licenseText)
        txtLicense.Text = licenseText

        ' Close Button
        btnClose = New Button()
        btnClose.Text = "Close"
        btnClose.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular)
        btnClose.Location = New Point(395, 465)
        btnClose.Size = New Size(75, 30)
        AddHandler btnClose.Click, Sub(sender, e) Me.Close()

        ' Add Controls
        Me.Controls.Add(lblTitle)
        Me.Controls.Add(txtLicense)
        Me.Controls.Add(btnClose)

        ' Apply the active theme through the shared styler.
        UiTheme.Style(Me)
        UiTheme.MakePrimary(btnClose)
        lblTitle.ForeColor = UiTheme.Accent()
        txtLicense.BackColor = UiTheme.Blend(UiTheme.SurfaceBack(), UiTheme.ForeTone(), 0.06)
        txtLicense.ForeColor = UiTheme.ForeTone()
    End Sub

    ''' <summary>
    ''' Rewrites the "Version:" line in the EULA text to the running assembly's
    ''' version so the displayed licence is never stale. The version string is
    ''' derived from AssemblyInfo, so each release picks it up automatically with
    ''' no manual edit to this window.
    ''' </summary>
    Private Function StampCurrentVersion(ByVal text As String) As String
        Try
            Dim ver As Version = Reflection.Assembly.GetExecutingAssembly().GetName().Version
            Dim shortVer As String = String.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build)
            Dim re As New System.Text.RegularExpressions.Regex("(?im)^(\s*Version:\s*).*$")
            If re.IsMatch(text) Then
                Return re.Replace(text, "${1}" & shortVer, 1)
            End If
        Catch
        End Try
        Return text
    End Function

    Private Function GetDefaultLicenseText() As String
        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine("BanglaType Keyboard - End User License Agreement (EULA)")
        sb.AppendLine()
        sb.AppendLine("Software Name: BanglaType Keyboard")
        sb.AppendLine("Version: 1.0.4")
        sb.AppendLine("Developer: Mohammad Sheikh Shahinur Rahman")
        sb.AppendLine("Effective Date: 21 June 2026")
        sb.AppendLine()
        sb.AppendLine("----------------------------------------------------------------------")
        sb.AppendLine()
        sb.AppendLine("1. LICENSE GRANT")
        sb.AppendLine()
        sb.AppendLine("This software (""BanglaType Keyboard"") is licensed, not sold. The developer")
        sb.AppendLine("grants you a limited, non-exclusive, non-transferable, revocable license to")
        sb.AppendLine("install and use the software on personal or business Windows devices strictly")
        sb.AppendLine("in accordance with this agreement.")
        sb.AppendLine()
        sb.AppendLine("2. PERMITTED USE")
        sb.AppendLine()
        sb.AppendLine("You are allowed to:")
        sb.AppendLine("  - Install and use the Software on personal or commercial Windows PCs")
        sb.AppendLine("  - Use BanglaType Keyboard for typing in Bangla, English, and Banglish")
        sb.AppendLine("  - Use portable and installed versions for personal productivity")
        sb.AppendLine("  - Create personal backups of the software")
        sb.AppendLine()
        sb.AppendLine("3. RESTRICTIONS")
        sb.AppendLine()
        sb.AppendLine("You are strictly NOT allowed to:")
        sb.AppendLine("  - Reverse engineer, decompile, or disassemble the software")
        sb.AppendLine("  - Modify, rename, or rebrand the software as your own product")
        sb.AppendLine("  - Remove or hide copyright, branding, or ownership information")
        sb.AppendLine("  - Sell, resell, sublicense, or distribute modified versions")
        sb.AppendLine("  - Use the software in any illegal, harmful, or unauthorized system")
        sb.AppendLine("  - Extract or reuse internal AI logic, prediction engine, or transliteration")
        sb.AppendLine("    system")
        sb.AppendLine()
        sb.AppendLine("4. INTELLECTUAL PROPERTY RIGHTS")
        sb.AppendLine()
        sb.AppendLine("All rights, title, and interest in BanglaType Keyboard, including but not")
        sb.AppendLine("limited to source code, UI design, branding, algorithms, and documentation,")
        sb.AppendLine("are exclusively owned by the developer. No ownership rights are transferred to")
        sb.AppendLine("the user under this license.")
        sb.AppendLine()
        sb.AppendLine("5. OPEN SOURCE COMPONENTS (IF ANY)")
        sb.AppendLine()
        sb.AppendLine("If any component of BanglaType Keyboard is released under an open-source")
        sb.AppendLine("license, those components will be governed by their respective licenses. In")
        sb.AppendLine("case of conflict, this EULA applies to proprietary components.")
        sb.AppendLine()
        sb.AppendLine("6. UPDATES AND MODIFICATIONS")
        sb.AppendLine()
        sb.AppendLine("The developer reserves the right to:")
        sb.AppendLine("  - Modify, update, or improve the software at any time")
        sb.AppendLine("  - Add or remove features without prior notice")
        sb.AppendLine("  - Introduce premium or paid features in future versions")
        sb.AppendLine("  - Change licensing terms in updated versions")
        sb.AppendLine()
        sb.AppendLine("7. PRIVACY POLICY")
        sb.AppendLine()
        sb.AppendLine("BanglaType Keyboard is designed with a privacy-first approach:")
        sb.AppendLine("  - Works fully offline (unless explicitly enabled features require internet)")
        sb.AppendLine("  - Does not intentionally collect personal data")
        sb.AppendLine("  - No keystroke logging or external data transmission without consent")
        sb.AppendLine()
        sb.AppendLine("However, users are responsible for enabling any optional cloud or AI features.")
        sb.AppendLine()
        sb.AppendLine("8. DISCLAIMER OF WARRANTY")
        sb.AppendLine()
        sb.AppendLine("The software is provided ""AS IS"" and ""AS AVAILABLE"" without warranties of any")
        sb.AppendLine("kind. The developer does not guarantee:")
        sb.AppendLine("  - Error-free performance")
        sb.AppendLine("  - Compatibility with all Windows versions")
        sb.AppendLine("  - Continuous availability of features")
        sb.AppendLine()
        sb.AppendLine("9. LIMITATION OF LIABILITY")
        sb.AppendLine()
        sb.AppendLine("Under no circumstances shall the developer be liable for any direct, indirect,")
        sb.AppendLine("incidental, or consequential damages arising from the use or inability to use")
        sb.AppendLine("the software.")
        sb.AppendLine()
        sb.AppendLine("10. TERMINATION")
        sb.AppendLine()
        sb.AppendLine("This license is automatically terminated if you violate any terms of this")
        sb.AppendLine("agreement. Upon termination, you must:")
        sb.AppendLine("  - Uninstall the software immediately")
        sb.AppendLine("  - Delete all copies from your system")
        sb.AppendLine()
        sb.AppendLine("11. COMMERCIAL USE")
        sb.AppendLine()
        sb.AppendLine("Commercial use is allowed only in its original unmodified form unless written")
        sb.AppendLine("permission is granted by the developer. Custom licensing for enterprise or OEM")
        sb.AppendLine("use may be offered separately.")
        sb.AppendLine()
        sb.AppendLine("12. GOVERNING LAW")
        sb.AppendLine()
        sb.AppendLine("This agreement shall be governed by applicable copyright and intellectual")
        sb.AppendLine("property laws.")
        sb.AppendLine()
        sb.AppendLine("----------------------------------------------------------------------")
        sb.AppendLine()
        sb.AppendLine("(C) 2026 Mohammad Sheikh Shahinur Rahman.")
        sb.AppendLine("All rights reserved. Unauthorized use, reproduction, or distribution is")
        sb.AppendLine("strictly prohibited.")
        Return sb.ToString()
    End Function
End Class
