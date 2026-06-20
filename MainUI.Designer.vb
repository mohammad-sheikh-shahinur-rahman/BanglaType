<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainUI
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.logoBox = New System.Windows.Forms.PictureBox()
        Me.btnMode = New System.Windows.Forms.Button()
        Me.btnSettings = New System.Windows.Forms.Button()
        Me.btnVoice = New System.Windows.Forms.Button()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.buttonInfo = New System.Windows.Forms.Button()
        Me.NotifyIcon1 = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.LayoutList = New System.Windows.Forms.ContextMenuStrip(Me.components)
        CType(Me.logoBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'logoBox
        '
        Me.logoBox.Location = New System.Drawing.Point(8, 8)
        Me.logoBox.Name = "logoBox"
        Me.logoBox.Size = New System.Drawing.Size(16, 16)
        Me.logoBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.logoBox.TabIndex = 0
        Me.logoBox.TabStop = False
        '
        'btnMode
        '
        Me.btnMode.BackColor = System.Drawing.Color.Transparent
        Me.btnMode.FlatAppearance.BorderSize = 0
        Me.btnMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnMode.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnMode.Location = New System.Drawing.Point(28, 4)
        Me.btnMode.Name = "btnMode"
        Me.btnMode.Size = New System.Drawing.Size(70, 24)
        Me.btnMode.TabIndex = 1
        Me.btnMode.Text = "English"
        Me.btnMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.btnMode.UseVisualStyleBackColor = False
        '
        'btnVoice
        '
        Me.btnVoice.BackColor = System.Drawing.Color.Transparent
        Me.btnVoice.FlatAppearance.BorderSize = 0
        Me.btnVoice.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnVoice.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.btnVoice.Location = New System.Drawing.Point(102, 4)
        Me.btnVoice.Name = "btnVoice"
        Me.btnVoice.Size = New System.Drawing.Size(24, 24)
        Me.btnVoice.TabIndex = 5
        Me.btnVoice.Text = "🎤"
        Me.btnVoice.UseVisualStyleBackColor = False
        '
        'btnSettings
        '
        Me.btnSettings.BackColor = System.Drawing.Color.Transparent
        Me.btnSettings.FlatAppearance.BorderSize = 0
        Me.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSettings.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.btnSettings.Location = New System.Drawing.Point(130, 4)
        Me.btnSettings.Name = "btnSettings"
        Me.btnSettings.Size = New System.Drawing.Size(24, 24)
        Me.btnSettings.TabIndex = 2
        Me.btnSettings.Text = "🔧"
        Me.btnSettings.UseVisualStyleBackColor = False
        '
        'buttonClose
        '
        Me.buttonClose.BackColor = System.Drawing.Color.Transparent
        Me.buttonClose.FlatAppearance.BorderSize = 0
        Me.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.buttonClose.Font = New System.Drawing.Font("Segoe UI", 8.0!)
        Me.buttonClose.Location = New System.Drawing.Point(158, 4)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(24, 24)
        Me.buttonClose.TabIndex = 3
        Me.buttonClose.Text = "✕"
        Me.buttonClose.UseVisualStyleBackColor = False
        '
        'buttonInfo
        '
        Me.buttonInfo.Location = New System.Drawing.Point(0, 0)
        Me.buttonInfo.Name = "buttonInfo"
        Me.buttonInfo.Size = New System.Drawing.Size(0, 0)
        Me.buttonInfo.TabIndex = 4
        Me.buttonInfo.Visible = False
        '
        '
        'NotifyIcon1
        '
        Me.NotifyIcon1.Text = "BanglaType"
        '
        'LayoutList
        '
        Me.LayoutList.Name = "LayoutList"
        Me.LayoutList.Size = New System.Drawing.Size(61, 4)
        '
        'MainUI
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(190, 32)
        Me.Controls.Add(Me.logoBox)
        Me.Controls.Add(Me.btnMode)
        Me.Controls.Add(Me.btnVoice)
        Me.Controls.Add(Me.btnSettings)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.buttonInfo)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.Name = "MainUI"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.Text = "BanglaType"
        Me.TopMost = True
        CType(Me.logoBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents logoBox As PictureBox
    Friend WithEvents btnMode As Button
    Friend WithEvents btnSettings As Button
    Friend WithEvents btnVoice As Button
    Friend WithEvents buttonClose As Button
    Friend WithEvents buttonInfo As Button
    Friend WithEvents NotifyIcon1 As NotifyIcon
    Friend WithEvents LayoutList As ContextMenuStrip
End Class
