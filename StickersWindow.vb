Imports System.Windows.Forms
Imports System.Drawing

Public Class StickersWindow
    Inherits Form

    Private flowLayout As FlowLayoutPanel
    Private lblTitle As Label

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "BanglaType Stickers"
        Me.Size = New Size(400, 320)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowIcon = False
        Me.TopMost = True

        lblTitle = New Label() With {
            .Text = "Select a Sticker to Paste:",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .Location = New Point(15, 10),
            .Size = New Size(370, 20)
        }

        flowLayout = New FlowLayoutPanel() With {
            .Location = New Point(15, 35),
            .Size = New Size(365, 230),
            .AutoScroll = True
        }

        ' Create and add stickers
        Dim stickerList As New List(Of StickerItem)()
        stickerList.Add(New StickerItem("👍", "সাবাস!", Color.FromArgb(24, 119, 242))) ' Blue
        stickerList.Add(New StickerItem("❤️", "ভালোবাসা", Color.FromArgb(222, 75, 57))) ' Red
        stickerList.Add(New StickerItem("😂", "হাহা", Color.FromArgb(255, 193, 7))) ' Yellow
        stickerList.Add(New StickerItem("🙏", "ধন্যবাদ", Color.FromArgb(0, 180, 137))) ' Green
        stickerList.Add(New StickerItem("😮", "অসাধারণ!", Color.FromArgb(255, 112, 67))) ' Orange
        stickerList.Add(New StickerItem("😢", "দুঃখিত", Color.FromArgb(69, 90, 100))) ' Slate

        For Each item In stickerList
            Dim tempItem = item
            Dim btn As New PictureBox() With {
                .Size = New Size(100, 100),
                .SizeMode = PictureBoxSizeMode.CenterImage,
                .Cursor = Cursors.Hand,
                .Margin = New Padding(5)
            }
            Dim bmp As Bitmap = CreateStickerBitmap(tempItem.Emoji, tempItem.Text, tempItem.BackColor)
            btn.Image = bmp
            AddHandler btn.Click, Sub()
                                      Try
                                          Clipboard.SetImage(bmp)
                                          System.Threading.Thread.Sleep(100)
                                          SendKeys.SendWait("^v")
                                          Me.Close()
                                      Catch ex As Exception
                                          MessageBox.Show("Failed to copy sticker: " & ex.Message)
                                      End Try
                                  End Sub
            flowLayout.Controls.Add(btn)
        Next

        Me.Controls.Add(lblTitle)
        Me.Controls.Add(flowLayout)

        ' Apply Current Theme if available
        Try
            If System.Windows.Forms.Application.OpenForms.Count > 0 Then
                Dim main As MainUI = TryCast(System.Windows.Forms.Application.OpenForms(0), MainUI)
                if main IsNot Nothing Then
                    Me.BackColor = main.ThemeTopbarBack
                    lblTitle.ForeColor = main.currentButtonFore
                End If
            End If
        Catch
        End Try
    End Sub

    Private Function CreateStickerBitmap(ByVal emoji As String, ByVal text As String, ByVal backColor As Color) As Bitmap
        Dim bmp As New Bitmap(100, 100)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
            g.Clear(Color.Transparent)

            ' Draw card background
            Using path As New System.Drawing.Drawing2D.GraphicsPath()
                Dim rect As New Rectangle(5, 5, 90, 90)
                Dim radius As Integer = 10
                Dim r2 As Integer = radius * 2
                path.StartFigure()
                path.AddArc(rect.X, rect.Y, r2, r2, 180, 90)
                path.AddArc(rect.Right - r2, rect.Y, r2, r2, 270, 90)
                path.AddArc(rect.Right - r2, rect.Bottom - r2, r2, r2, 0, 90)
                path.AddArc(rect.X, rect.Bottom - r2, r2, r2, 90, 90)
                path.CloseFigure()

                Using brush As New SolidBrush(backColor)
                    g.FillPath(brush, path)
                End Using
            End Using

            ' Draw emoji
            Using fontEmoji As New Font("Segoe UI Emoji", 24.0!, FontStyle.Regular)
                Using brush As New SolidBrush(Color.White)
                    Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    g.DrawString(emoji, fontEmoji, brush, New RectangleF(5, 10, 90, 45), sf)
                End Using
            End Using

            ' Draw text
            Using fontText As New Font("Segoe UI", 10.0!, FontStyle.Bold)
                Using brush As New SolidBrush(Color.White)
                    Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    g.DrawString(text, fontText, brush, New RectangleF(5, 55, 90, 35), sf)
                End Using
            End Using
        End Using
        Return bmp
    End Function

    Private Structure StickerItem
        Public Emoji As String
        Public Text As String
        Public BackColor As Color
        Public Sub New(ByVal e As String, ByVal t As String, ByVal c As Color)
            Emoji = e
            Text = t
            BackColor = c
        End Sub
    End Structure
End Class
