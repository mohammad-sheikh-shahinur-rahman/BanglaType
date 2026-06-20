Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Collections.Generic
Imports System.IO
Imports System.Xml
Imports System.Text

Public Class LayoutBuilderWindow
    Inherits Form

    Private pnlTitle As Panel
    Private lblTitle As Label
    Private btnClose As Button

    Private cmbLayouts As ComboBox
    Private txtLayoutName As TextBox
    Private dgvKeys As DataGridView
    Private btnSave As Button

    Private Structure KeyDef
        Public vkCode As Integer
        Public name As String
        Public defNormal As String
        Public defShift As String
    End Structure

    Private commonKeys As List(Of KeyDef)

    Public Sub New()
        InitializeCommonKeys()
        InitializeComponent()
        LoadLayoutDropdown()
    End Sub

    Private Sub InitializeCommonKeys()
        commonKeys = New List(Of KeyDef)()
        ' A-Z
        For vk As Integer = 65 To 90
            Dim charName As String = ChrW(vk).ToString()
            commonKeys.Add(New KeyDef With {.vkCode = vk, .name = charName, .defNormal = "", .defShift = ""})
        Next
        ' 0-9
        For vk As Integer = 48 To 57
            Dim charName As String = ChrW(vk).ToString()
            commonKeys.Add(New KeyDef With {.vkCode = vk, .name = charName, .defNormal = "", .defShift = ""})
        Next
        ' Special chars
        commonKeys.Add(New KeyDef With {.vkCode = 186, .name = "; (Semicolon)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 187, .name = "= (Equal)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 188, .name = ", (Comma)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 189, .name = "- (Minus)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 190, .name = ". (Period)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 191, .name = "/ (Slash)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 192, .name = "` (Tilde)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 219, .name = "[ (Open Bracket)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 220, .name = "\ (Backslash)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 221, .name = "] (Close Bracket)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 222, .name = "' (Quote)", .defNormal = "", .defShift = ""})
        commonKeys.Add(New KeyDef With {.vkCode = 32, .name = "Space", .defNormal = " ", .defShift = " "})
    End Sub

    Private Sub InitializeComponent()
        ' Setup Form
        Me.Text = "Custom Layout Builder"
        Me.Size = New Size(560, 520)
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
            .Text = "⌨️ BanglaType Custom Layout Builder",
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

        ' Select Layout to edit
        Dim lblSelect As New Label() With {
            .Text = "Select Base Layout:",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(15, 52),
            .Size = New Size(120, 20)
        }
        Me.Controls.Add(lblSelect)

        cmbLayouts = New ComboBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(140, 48),
            .Size = New Size(160, 25),
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        AddHandler cmbLayouts.SelectedIndexChanged, AddressOf CmbLayouts_SelectedIndexChanged
        Me.Controls.Add(cmbLayouts)

        ' Layout Name
        Dim lblName As New Label() With {
            .Text = "Layout Name:",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(320, 52),
            .Size = New Size(90, 20)
        }
        Me.Controls.Add(lblName)

        txtLayoutName = New TextBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(38, 38, 42),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(410, 48),
            .Size = New Size(135, 24)
        }
        Me.Controls.Add(txtLayoutName)

        ' DataGridView for mappings
        dgvKeys = New DataGridView() With {
            .Location = New Point(15, 90),
            .Size = New Size(530, 360),
            .BackgroundColor = Color.FromArgb(28, 28, 30),
            .ForeColor = Color.White,
            .GridColor = Color.FromArgb(48, 48, 52),
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .RowHeadersVisible = False,
            .BorderStyle = BorderStyle.None,
            .SelectionMode = DataGridViewSelectionMode.CellSelect
        }
        dgvKeys.EnableHeadersVisualStyles = False
        dgvKeys.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(38, 38, 42)
        dgvKeys.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvKeys.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        dgvKeys.DefaultCellStyle.BackColor = Color.FromArgb(28, 28, 30)
        dgvKeys.DefaultCellStyle.ForeColor = Color.White
        dgvKeys.DefaultCellStyle.Font = New Font("Nirmala UI", 9.5!, FontStyle.Regular)

        dgvKeys.Columns.Add("colKey", "Key")
        dgvKeys.Columns("colKey").ReadOnly = True
        dgvKeys.Columns("colKey").Width = 120

        dgvKeys.Columns.Add("colNormal", "Normal Output")
        dgvKeys.Columns("colNormal").Width = 110

        dgvKeys.Columns.Add("colShift", "Shift Output")
        dgvKeys.Columns("colShift").Width = 110

        dgvKeys.Columns.Add("colNO", "N_O (Attr)")
        dgvKeys.Columns("colNO").Width = 80

        dgvKeys.Columns.Add("colSO", "S_O (Attr)")
        dgvKeys.Columns("colSO").Width = 80

        dgvKeys.Columns.Add("colVk", "vkCode")
        dgvKeys.Columns("colVk").Visible = False

        Me.Controls.Add(dgvKeys)

        ' Action Button
        btnSave = New Button() With {
            .Text = "💾 Save Layout",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(0, 180, 137),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(530, 36),
            .Location = New Point(15, 465),
            .Cursor = Cursors.Hand
        }
        btnSave.FlatAppearance.BorderSize = 0
        AddHandler btnSave.Click, AddressOf BtnSave_Click
        Me.Controls.Add(btnSave)

        ApplyRoundedCorners()
    End Sub

    Private Sub LoadLayoutDropdown()
        cmbLayouts.Items.Clear()
        cmbLayouts.Items.Add("(New Layout...)")
        
        Dim baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "keyboard layouts")
        If Directory.Exists(baseDir) Then
            For Each file As String In Directory.GetFiles(baseDir, "*.kbl")
                cmbLayouts.Items.Add(Path.GetFileNameWithoutExtension(file))
            Next
        End If
        cmbLayouts.SelectedIndex = 0
    End Sub

    Private Sub CmbLayouts_SelectedIndexChanged(sender As Object, e As EventArgs)
        dgvKeys.Rows.Clear()
        
        Dim selectedLayout As String = cmbLayouts.SelectedItem.ToString()
        If selectedLayout = "(New Layout...)" Then
            txtLayoutName.Text = "MyCustom"
            For Each k In commonKeys
                dgvKeys.Rows.Add(k.name, k.defNormal, k.defShift, "", "", k.vkCode)
            Next
            Return
        End If

        txtLayoutName.Text = selectedLayout
        Dim baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "keyboard layouts")
        Dim filePath = Path.Combine(baseDir, selectedLayout & ".kbl")
        
        If File.Exists(filePath) Then
            Try
                Dim parser As New LayoutParser()
                parser.Init(filePath)
                
                For Each k In commonKeys
                    Dim normal As String = ""
                    Dim shift As String = ""
                    Dim no As String = ""
                    Dim so As String = ""
                    
                    If parser.Key.ContainsKey(k.vkCode) Then
                        Dim arr As ArrayList = parser.Key(k.vkCode)
                        If arr.Count > 0 Then normal = If(arr(0) IsNot Nothing, arr(0).ToString(), "")
                        If arr.Count > 1 Then shift = If(arr(1) IsNot Nothing, arr(1).ToString(), "")
                        If arr.Count > 2 Then no = If(arr(2) IsNot Nothing, arr(2).ToString(), "")
                        If arr.Count > 3 Then so = If(arr(3) IsNot Nothing, arr(3).ToString(), "")
                    End If
                    
                    dgvKeys.Rows.Add(k.name, normal, shift, no, so, k.vkCode)
                Next
            Catch ex As Exception
                MessageBox.Show("Error loading layout: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub BtnSave_Click(sender As Object, e As EventArgs)
        Dim layoutName As String = txtLayoutName.Text.Trim()
        If String.IsNullOrEmpty(layoutName) OrElse layoutName = "(New Layout...)" Then
            MessageBox.Show("Please enter a valid Layout Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        For Each c As Char In Path.GetInvalidFileNameChars()
            If layoutName.Contains(c) Then
                MessageBox.Show("Layout Name contains invalid characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
        Next

        Dim baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "keyboard layouts")
        Directory.CreateDirectory(baseDir)
        Dim filePath = Path.Combine(baseDir, layoutName & ".kbl")

        Try
            Dim settings As New XmlWriterSettings()
            settings.Indent = True
            settings.IndentChars = "  "
            settings.Encoding = Encoding.UTF8
            
            Using writer As XmlWriter = XmlWriter.Create(filePath, settings)
                writer.WriteStartDocument()
                writer.WriteStartElement("Layout")
                writer.WriteElementString("Name", layoutName)
                
                writer.WriteStartElement("Keys")
                For Each row As DataGridViewRow In dgvKeys.Rows
                    Dim vk As Integer = Convert.ToInt32(row.Cells("colVk").Value)
                    Dim normal As String = If(row.Cells("colNormal").Value IsNot Nothing, row.Cells("colNormal").Value.ToString(), "")
                    Dim shift As String = If(row.Cells("colShift").Value IsNot Nothing, row.Cells("colShift").Value.ToString(), "")
                    Dim no As String = If(row.Cells("colNO").Value IsNot Nothing, row.Cells("colNO").Value.ToString(), "")
                    Dim so As String = If(row.Cells("colSO").Value IsNot Nothing, row.Cells("colSO").Value.ToString(), "")
                    
                    writer.WriteStartElement("Key")
                    writer.WriteAttributeString("vkCode", vk.ToString())
                    If Not String.IsNullOrEmpty(no) Then writer.WriteAttributeString("N_O", no)
                    If Not String.IsNullOrEmpty(so) Then writer.WriteAttributeString("S_O", so)
                    
                    writer.WriteElementString("Normal", normal)
                    writer.WriteElementString("Shift", shift)
                    writer.WriteEndElement()
                Next
                writer.WriteEndElement()
                
                writer.WriteStartElement("Combinations")
                writer.WriteEndElement()
                
                writer.WriteEndElement()
                writer.WriteEndDocument()
            End Using

            MessageBox.Show("Layout '" & layoutName & "' saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            
            For Each f As Form In Application.OpenForms
                If TypeOf f Is MainUI Then
                    CType(f, MainUI).ReloadAllLayouts()
                    Exit For
                End If
            Next
            
            Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error saving layout: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
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
