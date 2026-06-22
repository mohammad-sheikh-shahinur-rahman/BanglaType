Imports System.Windows.Forms
Imports System.Drawing
Imports System.IO

Public Class NotepadWindow
    Inherits Form

    Private mainMenu As MenuStrip
    Private fileMenu As ToolStripMenuItem
    Private editMenu As ToolStripMenuItem
    Private formatMenu As ToolStripMenuItem
    
    Private mnuNew As ToolStripMenuItem
    Private mnuOpen As ToolStripMenuItem
    Private mnuSave As ToolStripMenuItem
    Private mnuSaveAs As ToolStripMenuItem
    Private mnuExit As ToolStripMenuItem
    
    Private mnuUndo As ToolStripMenuItem
    Private mnuCut As ToolStripMenuItem
    Private mnuCopy As ToolStripMenuItem
    Private mnuPaste As ToolStripMenuItem
    Private mnuDelete As ToolStripMenuItem
    Private mnuSelectAll As ToolStripMenuItem
    Private mnuTimeDate As ToolStripMenuItem
    
    Private mnuWordWrap As ToolStripMenuItem
    Private mnuFont As ToolStripMenuItem
    
    Private txtEditor As TextBox
    Private statusBar As StatusStrip
    Private statusLabel As ToolStripStatusLabel
    
    Private currentFilePath As String = ""
    Private isModified As Boolean = False

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "BanglaType Notepad - Untitled"
        Me.Size = New Size(800, 600)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimumSize = New Size(400, 300)
        
        ' Main Editor TextBox
        txtEditor = New TextBox() With {
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Regular),
            .BorderStyle = BorderStyle.None,
            .AcceptsTab = True
        }
        AddHandler txtEditor.TextChanged, AddressOf TxtEditor_TextChanged

        ' Menu Structure
        mainMenu = New MenuStrip()
        
        ' File Menu
        fileMenu = New ToolStripMenuItem("&File")
        mnuNew = New ToolStripMenuItem("&New", Nothing, AddressOf MnuNew_Click, Keys.Control Or Keys.N)
        mnuOpen = New ToolStripMenuItem("&Open...", Nothing, AddressOf MnuOpen_Click, Keys.Control Or Keys.O)
        mnuSave = New ToolStripMenuItem("&Save", Nothing, AddressOf MnuSave_Click, Keys.Control Or Keys.S)
        mnuSaveAs = New ToolStripMenuItem("Save &As...", Nothing, AddressOf MnuSaveAs_Click)
        mnuExit = New ToolStripMenuItem("E&xit", Nothing, AddressOf MnuExit_Click)
        
        fileMenu.DropDownItems.AddRange(New ToolStripItem() {mnuNew, mnuOpen, mnuSave, mnuSaveAs, New ToolStripSeparator(), mnuExit})
        
        ' Edit Menu
        editMenu = New ToolStripMenuItem("&Edit")
        mnuUndo = New ToolStripMenuItem("&Undo", Nothing, AddressOf MnuUndo_Click, Keys.Control Or Keys.Z)
        mnuCut = New ToolStripMenuItem("Cu&t", Nothing, AddressOf MnuCut_Click, Keys.Control Or Keys.X)
        mnuCopy = New ToolStripMenuItem("&Copy", Nothing, AddressOf MnuCopy_Click, Keys.Control Or Keys.C)
        mnuPaste = New ToolStripMenuItem("&Paste", Nothing, AddressOf MnuPaste_Click, Keys.Control Or Keys.V)
        mnuDelete = New ToolStripMenuItem("De&lete", Nothing, AddressOf MnuDelete_Click, Keys.Delete)
        mnuSelectAll = New ToolStripMenuItem("Select &All", Nothing, AddressOf MnuSelectAll_Click, Keys.Control Or Keys.A)
        mnuTimeDate = New ToolStripMenuItem("Time/&Date", Nothing, AddressOf MnuTimeDate_Click, Keys.F5)
        
        editMenu.DropDownItems.AddRange(New ToolStripItem() {mnuUndo, New ToolStripSeparator(), mnuCut, mnuCopy, mnuPaste, mnuDelete, New ToolStripSeparator(), mnuSelectAll, mnuTimeDate})
        
        ' Format Menu
        formatMenu = New ToolStripMenuItem("F&ormat")
        mnuWordWrap = New ToolStripMenuItem("&Word Wrap", Nothing, AddressOf MnuWordWrap_Click) With {.Checked = True}
        mnuFont = New ToolStripMenuItem("&Font...", Nothing, AddressOf MnuFont_Click)
        
        formatMenu.DropDownItems.AddRange(New ToolStripItem() {mnuWordWrap, mnuFont})
        
        mainMenu.Items.AddRange(New ToolStripItem() {fileMenu, editMenu, formatMenu})

        ' Status Strip
        statusBar = New StatusStrip()
        statusLabel = New ToolStripStatusLabel() With {
            .Text = "Lines: 1 | Words: 0 | Characters: 0"
        }
        statusBar.Items.Add(statusLabel)

        ' Adding controls
        Me.Controls.Add(txtEditor)
        Me.Controls.Add(mainMenu)
        Me.Controls.Add(statusBar)
        Me.MainMenuStrip = mainMenu

        UpdateStatus()
    End Sub

    Private Sub TxtEditor_TextChanged(sender As Object, e As EventArgs)
        isModified = True
        UpdateStatus()
    End Sub

    Private Sub UpdateStatus()
        Dim text As String = txtEditor.Text
        Dim charCount As Integer = text.Length
        Dim lineCount As Integer = txtEditor.Lines.Length
        
        ' Simple word count
        Dim wordCount As Integer = 0
        If Not String.IsNullOrWhiteSpace(text) Then
            Dim words As String() = text.Split(New Char() {" "c, vbTab(0), vbCr(0), vbLf(0)}, StringSplitOptions.RemoveEmptyEntries)
            wordCount = words.Length
        End If

        statusLabel.Text = String.Format("Lines: {0} | Words: {1} | Characters: {2}", lineCount, wordCount, charCount)
    End Sub

    ' File Menu Handlers
    Private Sub MnuNew_Click(sender As Object, e As EventArgs)
        If ConfirmSaveIfNeeded() Then
            txtEditor.Clear()
            currentFilePath = ""
            isModified = False
            Me.Text = "BanglaType Notepad - Untitled"
        End If
    End Sub

    Private Sub MnuOpen_Click(sender As Object, e As EventArgs)
        If ConfirmSaveIfNeeded() Then
            Using ofd As New OpenFileDialog()
                ofd.Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
                If ofd.ShowDialog() = DialogResult.OK Then
                    Try
                        txtEditor.Text = File.ReadAllText(ofd.FileName)
                        currentFilePath = ofd.FileName
                        isModified = False
                        Me.Text = "BanglaType Notepad - " & Path.GetFileName(ofd.FileName)
                    Catch ex As Exception
                        MessageBox.Show("Could not open file: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                End If
            End Using
        End If
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
                File.WriteAllText(currentFilePath, txtEditor.Text)
                isModified = False
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
                    File.WriteAllText(sfd.FileName, txtEditor.Text)
                    currentFilePath = sfd.FileName
                    isModified = False
                    Me.Text = "BanglaType Notepad - " & Path.GetFileName(sfd.FileName)
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
        If Not ConfirmSaveIfNeeded() Then
            e.Cancel = True
        End If
        MyBase.OnFormClosing(e)
    End Sub

    ' Edit Menu Handlers
    Private Sub MnuUndo_Click(sender As Object, e As EventArgs)
        If txtEditor.CanUndo Then
            txtEditor.Undo()
        End If
    End Sub

    Private Sub MnuCut_Click(sender As Object, e As EventArgs)
        txtEditor.Cut()
    End Sub

    Private Sub MnuCopy_Click(sender As Object, e As EventArgs)
        txtEditor.Copy()
    End Sub

    Private Sub MnuPaste_Click(sender As Object, e As EventArgs)
        txtEditor.Paste()
    End Sub

    Private Sub MnuDelete_Click(sender As Object, e As EventArgs)
        txtEditor.SelectedText = ""
    End Sub

    Private Sub MnuSelectAll_Click(sender As Object, e As EventArgs)
        txtEditor.SelectAll()
    End Sub

    Private Sub MnuTimeDate_Click(sender As Object, e As EventArgs)
        Dim timeDateStr As String = DateTime.Now.ToString("g")
        Dim selectionIndex As Integer = txtEditor.SelectionStart
        txtEditor.Text = txtEditor.Text.Insert(selectionIndex, timeDateStr)
        txtEditor.SelectionStart = selectionIndex + timeDateStr.Length
    End Sub

    ' Format Menu Handlers
    Private Sub MnuWordWrap_Click(sender As Object, e As EventArgs)
        mnuWordWrap.Checked = Not mnuWordWrap.Checked
        txtEditor.WordWrap = mnuWordWrap.Checked
        If mnuWordWrap.Checked Then
            txtEditor.ScrollBars = ScrollBars.Vertical
        Else
            txtEditor.ScrollBars = ScrollBars.Both
        End If
    End Sub

    Private Sub MnuFont_Click(sender As Object, e As EventArgs)
        Using fd As New FontDialog()
            fd.Font = txtEditor.Font
            If fd.ShowDialog() = DialogResult.OK Then
                txtEditor.Font = fd.Font
            End If
        End Using
    End Sub
End Class
