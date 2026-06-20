Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text
Imports System.Net
Imports System.IO

Public Class GeminiAIWindow
    Inherits Form

    Private pnlTitle As Panel
    Private lblTitle As Label
    Private btnClose As Button

    Private txtInput As TextBox
    Private txtOutput As TextBox
    Private cmbPrompt As ComboBox
    Private txtCustomPrompt As TextBox
    Private txtApiKey As TextBox
    Private btnGenerate As Button
    Private btnInsert As Button
    Private btnSaveKey As Button

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        ' Setup Form
        Me.Text = "BanglaType Gemini AI Assistant"
        Me.Size = New Size(500, 480)
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
            .Text = "✨ BanglaType Gemini AI Assistant",
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

        ' Selected Text Panel
        Dim lblInput As New Label() With {
            .Text = "Source Text (Selected):",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(15, 45),
            .Size = New Size(150, 18)
        }
        Me.Controls.Add(lblInput)

        txtInput = New TextBox() With {
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .Font = New Font("Nirmala UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(32, 32, 35),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(15, 65),
            .Size = New Size(470, 90)
        }
        Me.Controls.Add(txtInput)

        ' Preset Prompts ComboBox
        Dim lblPrompt As New Label() With {
            .Text = "Select AI Action:",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(15, 165),
            .Size = New Size(150, 18)
        }
        Me.Controls.Add(lblPrompt)

        cmbPrompt = New ComboBox() With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(32, 32, 35),
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(15, 185),
            .Size = New Size(470, 26)
        }
        cmbPrompt.Items.Add("🔍 Correct spelling, grammar, & punctuation (Bangla/English)")
        cmbPrompt.Items.Add("💼 Rewrite in a formal professional tone")
        cmbPrompt.Items.Add("😊 Rewrite in a friendly casual tone")
        cmbPrompt.Items.Add("🌐 Translate between English and Bangla")
        cmbPrompt.Items.Add("✏️ Custom prompt (Type below)")
        cmbPrompt.SelectedIndex = 0
        AddHandler cmbPrompt.SelectedIndexChanged, AddressOf CmbPrompt_SelectedIndexChanged
        Me.Controls.Add(cmbPrompt)

        ' Custom prompt Textbox
        txtCustomPrompt = New TextBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.Gray,
            .BackColor = Color.FromArgb(32, 32, 35),
            .BorderStyle = BorderStyle.FixedSingle,
            .Text = "Enter custom prompt constraints here...",
            .Location = New Point(15, 220),
            .Size = New Size(470, 24),
            .Enabled = False
        }
        AddHandler txtCustomPrompt.Enter, Sub()
                                             If txtCustomPrompt.Text = "Enter custom prompt constraints here..." Then
                                                 txtCustomPrompt.Text = ""
                                                 txtCustomPrompt.ForeColor = Color.White
                                             End If
                                         End Sub
        AddHandler txtCustomPrompt.Leave, Sub()
                                             If String.IsNullOrWhiteSpace(txtCustomPrompt.Text) Then
                                                 txtCustomPrompt.Text = "Enter custom prompt constraints here..."
                                                 txtCustomPrompt.ForeColor = Color.Gray
                                             End If
                                         End Sub
        Me.Controls.Add(txtCustomPrompt)

        ' API Key Config Panel
        Dim lblApiKey As New Label() With {
            .Text = "Gemini API Key:",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(15, 255),
            .Size = New Size(100, 18)
        }
        Me.Controls.Add(lblApiKey)

        txtApiKey = New TextBox() With {
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(32, 32, 35),
            .BorderStyle = BorderStyle.FixedSingle,
            .UseSystemPasswordChar = True,
            .Location = New Point(115, 253),
            .Size = New Size(260, 24)
        }
        txtApiKey.Text = AppSettings.GeminiApiKey
        Me.Controls.Add(txtApiKey)

        btnSaveKey = New Button() With {
            .Text = "💾 Save Key",
            .Font = New Font("Segoe UI", 8.5!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(48, 48, 50),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(95, 24),
            .Location = New Point(390, 252),
            .Cursor = Cursors.Hand
        }
        btnSaveKey.FlatAppearance.BorderSize = 0
        AddHandler btnSaveKey.Click, AddressOf BtnSaveKey_Click
        Me.Controls.Add(btnSaveKey)

        ' Output Display
        Dim lblOutput As New Label() With {
            .Text = "Result:",
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .Location = New Point(15, 288),
            .Size = New Size(100, 18)
        }
        Me.Controls.Add(lblOutput)

        txtOutput = New TextBox() With {
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .Font = New Font("Nirmala UI", 9.5!, FontStyle.Regular),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(32, 32, 35),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(15, 308),
            .Size = New Size(470, 100),
            .ReadOnly = True
        }
        Me.Controls.Add(txtOutput)

        ' Action Buttons
        btnGenerate = New Button() With {
            .Text = "✨ Generate content",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(0, 180, 137),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(220, 36),
            .Location = New Point(265, 424),
            .Cursor = Cursors.Hand
        }
        btnGenerate.FlatAppearance.BorderSize = 0
        AddHandler btnGenerate.Click, AddressOf BtnGenerate_Click
        Me.Controls.Add(btnGenerate)

        btnInsert = New Button() With {
            .Text = "📋 Insert / Replace Text",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(48, 48, 50),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(220, 36),
            .Location = New Point(15, 424),
            .Cursor = Cursors.Hand,
            .Enabled = False
        }
        btnInsert.FlatAppearance.BorderSize = 0
        AddHandler btnInsert.Click, AddressOf BtnInsert_Click
        Me.Controls.Add(btnInsert)

        ' Auto grab selected text using Ctrl+C
        GrabSelectedText()
        ApplyRoundedCorners()
    End Sub

    Private Sub GrabSelectedText()
        Try
            ' Send Ctrl + C to target window
            SendKeys.SendWait("^c")
            System.Threading.Thread.Sleep(100)
            If Clipboard.ContainsText() Then
                txtInput.Text = Clipboard.GetText()
            End If
        Catch
        End Try
    End Sub

    Private Sub CmbPrompt_SelectedIndexChanged(sender As Object, e As EventArgs)
        If cmbPrompt.SelectedIndex = 4 Then
            txtCustomPrompt.Enabled = True
            txtCustomPrompt.ForeColor = Color.White
            If txtCustomPrompt.Text = "Enter custom prompt constraints here..." Then
                txtCustomPrompt.Text = ""
            End If
        Else
            txtCustomPrompt.Enabled = False
            txtCustomPrompt.ForeColor = Color.Gray
            txtCustomPrompt.Text = "Enter custom prompt constraints here..."
        End If
    End Sub

    Private Sub BtnSaveKey_Click(sender As Object, e As EventArgs)
        AppSettings.GeminiApiKey = txtApiKey.Text.Trim()
        AppSettings.Save()
        MessageBox.Show("Gemini API key saved successfully.", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub BtnGenerate_Click(sender As Object, e As EventArgs)
        Dim apiKey As String = txtApiKey.Text.Trim()
        If String.IsNullOrEmpty(apiKey) Then
            MessageBox.Show("Please enter a valid Gemini API Key. You can get a free key from Google AI Studio.", "API Key Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim sourceText As String = txtInput.Text.Trim()
        If String.IsNullOrEmpty(sourceText) Then
            MessageBox.Show("Source text is empty. Please enter or select some text to format.", "Input Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Build prompt
        Dim instruction As String = ""
        Select Case cmbPrompt.SelectedIndex
            Case 0 : instruction = "Correct all spelling, grammar, and punctuation mistakes in the text below. Keep the original language (Bangla or English). Respond ONLY with the corrected text, no explanations."
            Case 1 : instruction = "Rewrite the text below in a formal, professional tone. Maintain the core meaning. Respond ONLY with the revised text, no explanations."
            Case 2 : instruction = "Rewrite the text below in a friendly, warm, and casual tone. Maintain the core meaning. Respond ONLY with the revised text, no explanations."
            Case 3 : instruction = "Translate the text below. If it is in Bangla, translate to English. If it is in English, translate to Bangla. Respond ONLY with the translation."
            Case 4
                Dim cp As String = txtCustomPrompt.Text.Trim()
                If cp = "Enter custom prompt constraints here..." Then cp = ""
                instruction = "Format the text below according to this custom prompt: " & cp & ". Respond ONLY with the output."
        End Select

        Dim finalPrompt As String = instruction & vbCrLf & vbCrLf & "TEXT:" & vbCrLf & sourceText

        btnGenerate.Enabled = False
        btnGenerate.Text = "⏳ Generating..."
        txtOutput.Clear()

        Dim thread As New System.Threading.Thread(Sub()
                                                      Dim result As String = CallGeminiApi(apiKey, finalPrompt)
                                                      Me.BeginInvoke(Sub()
                                                                         txtOutput.Text = result
                                                                         btnGenerate.Enabled = True
                                                                         btnGenerate.Text = "✨ Generate content"
                                                                         If Not String.IsNullOrEmpty(result) Then
                                                                             btnInsert.Enabled = True
                                                                             btnInsert.BackColor = Color.FromArgb(0, 180, 137)
                                                                         End If
                                                                     End Sub)
                                                  End Sub)
        thread.IsBackground = True
        thread.Start()
    End Sub

    Private Function CallGeminiApi(ByVal apiKey As String, ByVal prompt As String) As String
        Try
            Dim url As String = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=" & apiKey
            Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
            request.Method = "POST"
            request.ContentType = "application/json"
            request.Timeout = 10000 ' 10 seconds

            ' Prepare escaped prompt JSON
            Dim cleanPrompt As String = prompt.Replace("\", "\\").Replace("""", "\""").Replace(vbCrLf, "\n").Replace(vbLf, "\n").Replace(vbCr, "\n")
            Dim jsonBody As String = "{""contents"": [{""parts"": [{""text"": """ & cleanPrompt & """}]}]}"
            Dim bytes() As Byte = Encoding.UTF8.GetBytes(jsonBody)
            
            Using requestStream As Stream = request.GetRequestStream()
                requestStream.Write(bytes, 0, bytes.Length)
            End Using

            Using response As WebResponse = request.GetResponse()
                Using responseStream As Stream = response.GetResponseStream()
                    Using reader As New StreamReader(responseStream, Encoding.UTF8)
                        Dim jsonResponse As String = reader.ReadToEnd()
                        Return ExtractTextFromGeminiJson(jsonResponse)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Return "Error calling Gemini AI: " & ex.Message
        End Try
    End Function

    Private Function ExtractTextFromGeminiJson(ByVal json As String) As String
        Try
            Dim searchStr As String = """text"": """
            Dim idx As Integer = json.IndexOf(searchStr)
            If idx >= 0 Then
                Dim start As Integer = idx + searchStr.Length
                Dim sb As New StringBuilder()
                Dim i As Integer = start
                While i < json.Length
                    Dim c As Char = json(i)
                    If c = """"c Then
                        If json(i - 1) <> "\"c Then
                            Exit While
                        End If
                    End If
                    sb.Append(c)
                    i += 1
                End While
                
                Dim rawText As String = sb.ToString()
                rawText = rawText.Replace("\n", vbCrLf)
                rawText = rawText.Replace("\r", "")
                rawText = rawText.Replace("\t", vbTab)
                rawText = rawText.Replace("\""", """")
                rawText = rawText.Replace("\\", "\")
                Return rawText
            End If
        Catch
        End Try
        Return ""
    End Function

    Private Sub BtnInsert_Click(sender As Object, e As EventArgs)
        Dim resultText As String = txtOutput.Text.Trim()
        If String.IsNullOrEmpty(resultText) Then Return

        Try
            ' Overwrite selected text in target app
            Clipboard.SetText(resultText)
            System.Threading.Thread.Sleep(50)
            SendKeys.SendWait("^v")
        Catch
            MessageBox.Show("Failed to paste formatting text.")
        End Try
        Me.Close()
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
