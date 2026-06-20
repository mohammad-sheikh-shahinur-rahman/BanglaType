Imports System.Windows.Forms
Imports System.Drawing
Imports System.IO

Public Class LicenseWindow
    Inherits Form

    Private lblTitle As Label
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
        lblTitle.Location = New Point(15, 15)
        lblTitle.Size = New Size(470, 30)

        ' License TextBox
        txtLicense = New TextBox()
        txtLicense.Multiline = True
        txtLicense.ReadOnly = True
        txtLicense.ScrollBars = ScrollBars.Vertical
        txtLicense.Font = New Font("Consolas", 9.0!, FontStyle.Regular)
        txtLicense.Location = New Point(15, 50)
        txtLicense.Size = New Size(455, 400)
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

        ' Apply Current Theme if available
        Try
            If System.Windows.Forms.Application.OpenForms.Count > 0 Then
                Dim main As MainUI = TryCast(System.Windows.Forms.Application.OpenForms(0), MainUI)
                If main IsNot Nothing Then
                    Me.BackColor = main.ThemeTopbarBack
                    lblTitle.ForeColor = main.currentButtonFore
                    btnClose.BackColor = main.ThemeBorderColor
                    btnClose.ForeColor = main.currentButtonFore
                End If
            End If
        Catch
        End Try
    End Sub

    Private Function GetDefaultLicenseText() As String
        Return "GNU GENERAL PUBLIC LICENSE" & vbCrLf & _
               "Version 3, 29 June 2007" & vbCrLf & vbCrLf & _
               "Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>" & vbCrLf & _
               "Everyone is permitted to copy and distribute verbatim copies of this license document, but changing it is not allowed." & vbCrLf & vbCrLf & _
               "Preamble" & vbCrLf & vbCrLf & _
               "The GNU General Public License is a free, copyleft license for software and other kinds of works." & vbCrLf & vbCrLf & _
               "This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version." & vbCrLf & vbCrLf & _
               "This software is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details." & vbCrLf & vbCrLf & _
               "All rights reserved by Mohammad Sheikh Shahinur Rahman."
    End Function
End Class
