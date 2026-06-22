

Imports System.Runtime.InteropServices

Public Class NativeMethods

    <DllImport("user32.dll", SetLastError:=True)>
    Friend Shared Function SendInput(ByVal cInputs As Int32, ByRef pInputs As INPUT, ByVal cbSize As Int32) As Int32
    End Function

    <StructLayout(LayoutKind.Explicit, Pack:=1, Size:=28)>
    Friend Structure INPUT
        <FieldOffset(0)> Public dwType As InputType
        <FieldOffset(4)> Public ki As KEYBDINPUT
        <FieldOffset(4)> Public hi As HARDWAREINPUT
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Friend Structure KEYBDINPUT
        Public wVk As Int16
        Public wScan As Int16
        Public dwFlags As KEYEVENTF
        Public time As Int32
        Public dwExtraInfo As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Friend Structure HARDWAREINPUT
        Public uMsg As Int32
        Public wParamL As Int16
        Public wParamH As Int16
    End Structure

    Friend Enum InputType As Integer
        Keyboard = 1
        Hardware = 2
    End Enum

    <Flags()>
    Friend Enum KEYEVENTF As Integer
        KEYDOWN = 0
        EXTENDEDKEY = 1
        KEYUP = 2
        [UNICODE] = 4
        SCANCODE = 8
    End Enum


    <Flags()>
    Friend Enum MOUSEEVENTF As Integer
        MOVE = &H1
        LEFTDOWN = &H2
        LEFTUP = &H4
        RIGHTDOWN = &H8
        RIGHTUP = &H10
        MIDDLEDOWN = &H20
        MIDDLEUP = &H40
        XDOWN = &H80
        XUP = &H100
        VIRTUALDESK = &H400
        WHEEL = &H800
        ABSOLUTE = &H8000
    End Enum

    ' --- Caret / cursor positioning (used by the suggestion window) -------

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure POINT
        Public x As Integer
        Public y As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure RECT
        Public left As Integer
        Public top As Integer
        Public right As Integer
        Public bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure GUITHREADINFO
        Public cbSize As Integer
        Public flags As Integer
        Public hwndActive As IntPtr
        Public hwndFocus As IntPtr
        Public hwndCapture As IntPtr
        Public hwndMenuOwner As IntPtr
        Public hwndMoveSize As IntPtr
        Public hwndCaret As IntPtr
        Public rcCaret As RECT
    End Structure

    <DllImport("user32.dll")>
    Friend Shared Function GetGUIThreadInfo(ByVal idThread As Integer, ByRef lpgui As GUITHREADINFO) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Shared Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Friend Shared Function GetWindowThreadProcessId(ByVal hWnd As IntPtr, ByRef lpdwProcessId As Integer) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Friend Shared Function GetWindowText(ByVal hWnd As IntPtr, ByVal lpString As System.Text.StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    Private Shared cachedHWnd As IntPtr = IntPtr.Zero
    Private Shared cachedProcessName As String = ""
    Private Shared cachedWindowTitle As String = ""
    Private Shared cachedTime As DateTime = DateTime.MinValue

    Public Shared Function GetActiveAppInfo(ByRef processName As String, ByRef windowTitle As String) As Boolean
        Try
            Dim hWnd As IntPtr = GetForegroundWindow()
            If hWnd = IntPtr.Zero Then Return False

            Dim now As DateTime = DateTime.Now
            If hWnd = cachedHWnd AndAlso (now - cachedTime).TotalMilliseconds < 2000 Then
                processName = cachedProcessName
                windowTitle = cachedWindowTitle
                Return True
            End If

            Dim pid As Integer = 0
            GetWindowThreadProcessId(hWnd, pid)
            If pid > 0 Then
                Dim procName As String = ""
                Try
                    Using proc As System.Diagnostics.Process = System.Diagnostics.Process.GetProcessById(pid)
                        procName = proc.ProcessName
                    End Using
                Catch
                    procName = "unknown"
                End Try

                Dim sb As New System.Text.StringBuilder(256)
                GetWindowText(hWnd, sb, 256)
                Dim winTitle As String = sb.ToString()

                cachedHWnd = hWnd
                cachedProcessName = procName
                cachedWindowTitle = winTitle
                cachedTime = now

                processName = procName
                windowTitle = winTitle
                Return True
            End If
        Catch
        End Try
        Return False
    End Function

    <DllImport("user32.dll")>
    Friend Shared Function ClientToScreen(ByVal hWnd As IntPtr, ByRef lpPoint As POINT) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Shared Function GetCursorPos(ByRef lpPoint As POINT) As Boolean
    End Function

    ''' <summary>
    ''' Best-effort screen position of the text caret in the focused control of the
    ''' foreground window. Falls back to the mouse cursor position when no caret is found.
    ''' </summary>
    Friend Shared Function CaretScreenPos() As Drawing.Point
        Try
            Dim hFore As IntPtr = GetForegroundWindow()
            If hFore <> IntPtr.Zero Then
                Dim tid As Integer = GetWindowThreadProcessId(hFore, IntPtr.Zero)
                Dim gti As New GUITHREADINFO()
                gti.cbSize = Marshal.SizeOf(gti)
                If GetGUIThreadInfo(tid, gti) AndAlso gti.hwndCaret <> IntPtr.Zero Then
                    Dim pt As New POINT() With {.x = gti.rcCaret.left, .y = gti.rcCaret.bottom}
                    If ClientToScreen(gti.hwndCaret, pt) Then
                        Return New Drawing.Point(pt.x, pt.y)
                    End If
                End If
            End If
        Catch
        End Try

        Dim cur As New POINT()
        If GetCursorPos(cur) Then Return New Drawing.Point(cur.x, cur.y)
        Return New Drawing.Point(100, 100)
    End Function

End Class
