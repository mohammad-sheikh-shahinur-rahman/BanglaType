Imports System.Windows.Forms
Imports System.Drawing

Public Class ConverterWindow
    Inherits Form

    Private lblUnicode As Label
    Private lblBijoy As Label
    Private txtUnicode As TextBox
    Private txtBijoy As TextBox
    Private btnToBijoy As Button
    Private btnToUnicode As Button
    Private btnClose As Button

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "BanglaType Text Converter"
        Me.Size = New Size(700, 520)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowIcon = False
        Me.ShowInTaskbar = True

        ' Labels
        lblUnicode = New Label() With {
            .Text = "Unicode Text (ইউনিকোড):",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .Location = New Point(15, 15),
            .Size = New Size(250, 20)
        }

        lblBijoy = New Label() With {
            .Text = "Bijoy / ANSI Text (বিজয় / SutonnyMJ):",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .Location = New Point(15, 235),
            .Size = New Size(300, 20)
        }

        ' TextBoxes
        txtUnicode = New TextBox() With {
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .Font = New Font("Segoe UI", 10.5!, FontStyle.Regular),
            .Location = New Point(15, 40),
            .Size = New Size(655, 140),
            .BackColor = Color.White,
            .ForeColor = Color.Black
        }

        txtBijoy = New TextBox() With {
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .Font = New Font("SutonnyMJ", 12.0!, FontStyle.Regular), ' Use standard SutonnyMJ for ANSI if installed
            .Location = New Point(15, 260),
            .Size = New Size(655, 140),
            .BackColor = Color.White,
            .ForeColor = Color.Black
        }

        ' Buttons
        btnToBijoy = New Button() With {
            .Text = "Convert Unicode to Bijoy (↓)",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .Location = New Point(150, 190),
            .Size = New Size(180, 32),
            .FlatStyle = FlatStyle.Flat
        }
        AddHandler btnToBijoy.Click, Sub(sender, e)
                                        Try
                                            txtBijoy.Text = BijoyConverter.UnicodeToBijoy(txtUnicode.Text)
                                        Catch ex As Exception
                                            MessageBox.Show("Conversion failed: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        End Try
                                    End Sub

        btnToUnicode = New Button() With {
            .Text = "Convert Bijoy to Unicode (↑)",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .Location = New Point(350, 190),
            .Size = New Size(180, 32),
            .FlatStyle = FlatStyle.Flat
        }
        AddHandler btnToUnicode.Click, Sub(sender, e)
                                          Try
                                              txtUnicode.Text = BijoyConverter.BijoyToUnicode(txtBijoy.Text)
                                          Catch ex As Exception
                                              MessageBox.Show("Conversion failed: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                          End Try
                                      End Sub

        btnClose = New Button() With {
            .Text = "Close",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .Location = New Point(595, 430),
            .Size = New Size(75, 30),
            .FlatStyle = FlatStyle.Flat
        }
        AddHandler btnClose.Click, Sub(sender, e) Me.Close()

        ' Add Controls
        Me.Controls.Add(lblUnicode)
        Me.Controls.Add(lblBijoy)
        Me.Controls.Add(txtUnicode)
        Me.Controls.Add(txtBijoy)
        Me.Controls.Add(btnToBijoy)
        Me.Controls.Add(btnToUnicode)
        Me.Controls.Add(btnClose)

        ' Apply Current Theme if available
        Try
            If System.Windows.Forms.Application.OpenForms.Count > 0 Then
                Dim main As MainUI = TryCast(System.Windows.Forms.Application.OpenForms(0), MainUI)
                If main IsNot Nothing Then
                    Me.BackColor = main.ThemeTopbarBack
                    lblUnicode.ForeColor = main.currentButtonFore
                    lblBijoy.ForeColor = main.currentButtonFore
                    
                    btnToBijoy.BackColor = main.ThemeBorderColor
                    btnToBijoy.ForeColor = main.currentButtonFore
                    btnToBijoy.FlatAppearance.BorderColor = main.ThemeBorderColor
                    
                    btnToUnicode.BackColor = main.ThemeBorderColor
                    btnToUnicode.ForeColor = main.currentButtonFore
                    btnToUnicode.FlatAppearance.BorderColor = main.ThemeBorderColor
                    
                    btnClose.BackColor = main.ThemeBorderColor
                    btnClose.ForeColor = main.currentButtonFore
                    btnClose.FlatAppearance.BorderColor = main.ThemeBorderColor
                End If
            End If
        Catch
        End Try
    End Sub
End Class
