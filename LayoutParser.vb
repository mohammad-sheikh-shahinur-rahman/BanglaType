'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   Software distributed under the License Is distributed On an "AS IS"
'   basis, WITHOUT WARRANTY Of ANY KIND, either express Or implied. See the
'   License for the specific language governing rights And limitations
'   under the License.
'
'   The Initial Developer of this Code is Mohammad Sheikh Shahinur Rahman
'   Copyright� Mohammad Sheikh Shahinur Rahman. All Rights Reserved
'
'

Imports Microsoft.VisualBasic.CompilerServices
Imports System.Xml
Imports System.Collections.ObjectModel
Imports Microsoft.VisualBasic.FileIO


Public Class LayoutParser

    Public ID As Integer
    Public Name As String
    Public Path As String



    Public Key As Dictionary(Of Integer, ArrayList)

    Public KeySequences As Dictionary(Of String, String)



    Public Sub New()
        Me.Key = New Dictionary(Of Integer, ArrayList)()
        Me.KeySequences = New Dictionary(Of String, String)()
    End Sub



    Public Shared Function SearchForLayouts() As Boolean
        Dim Lpath As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "keyboard layouts")

        Try
            ' Ensure layout directory exists
            If Not System.IO.Directory.Exists(Lpath) Then
                System.IO.Directory.CreateDirectory(Lpath)
            End If

            ' 1. Auto-discover JSON layouts in local folders: StartupPath\keyboard layout and StartupPath\data
            Dim localDirs As New List(Of String)()
            Dim startupPath As String = AppDomain.CurrentDomain.BaseDirectory
            
            Dim dirLayout As String = System.IO.Path.Combine(startupPath, "keyboard layout")
            If System.IO.Directory.Exists(dirLayout) Then localDirs.Add(dirLayout)
            
            Dim dirData As String = System.IO.Path.Combine(startupPath, "data")
            If System.IO.Directory.Exists(dirData) Then localDirs.Add(dirData)

            ' Fallback for development mode
            Dim devData As String = System.IO.Path.GetFullPath(System.IO.Path.Combine(startupPath, "..", "..", "data"))
            If System.IO.Directory.Exists(devData) Then
                If Not localDirs.Contains(devData) Then localDirs.Add(devData)
            End If

            For Each d In localDirs
                ' Search recursively for *.json files
                For Each jsonFile In System.IO.Directory.GetFiles(d, "*.json", System.IO.SearchOption.AllDirectories)
                    Dim fileName As String = System.IO.Path.GetFileName(jsonFile).ToLower()
                    
                    ' Skip non-layout json files
                    If fileName = "dictionary.json" OrElse fileName = "autocorrect.json" OrElse fileName = "suffix.json" Then
                        Continue For
                    End If

                    Try
                        Dim jsonContent As String = System.IO.File.ReadAllText(jsonFile, System.Text.Encoding.UTF8)
                        
                        ' Basic validation that this JSON contains a layout
                        If jsonContent.Contains("""layout""") Then
                            Dim nameMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, """name""\s*:\s*""([^""]*)""")
                            Dim layoutName As String = ""
                            If nameMatch.Success Then
                                layoutName = nameMatch.Groups(1).Value.Trim()
                            End If
                            If String.IsNullOrEmpty(layoutName) Then
                                layoutName = System.IO.Path.GetFileNameWithoutExtension(jsonFile)
                            End If

                            ' Convert JSON layout to KBL format
                            Dim kblContent As String = ConvertJsonToKbl(jsonContent, layoutName)
                            If Not String.IsNullOrEmpty(kblContent) Then
                                Dim destPath As String = System.IO.Path.Combine(Lpath, layoutName & ".kbl")
                                System.IO.File.WriteAllText(destPath, kblContent, System.Text.Encoding.UTF8)
                            End If
                        End If
                    Catch ex As Exception
                        ' Fail silently for individual bad json files
                    End Try
                Next
            Next
        Catch ex As Exception
            ' Fail silently for scan initialization errors
        End Try

        ' 2. Scan and load all *.kbl files from the AppData layout folder
        Dim readOnlyCollection As New List(Of String)()
        If System.IO.Directory.Exists(Lpath) Then
            readOnlyCollection.AddRange(System.IO.Directory.GetFiles(Lpath, "*.kbl", System.IO.SearchOption.AllDirectories))
        End If

        ' Also check if there are any static .kbl files in the local layout directory and load them
        Dim localLayoutDir As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keyboard layout")
        If System.IO.Directory.Exists(localLayoutDir) Then
            For Each f In System.IO.Directory.GetFiles(localLayoutDir, "*.kbl", System.IO.SearchOption.AllDirectories)
                If Not readOnlyCollection.Contains(f) Then
                    readOnlyCollection.Add(f)
                End If
            Next
        End If

        If readOnlyCollection.Count = 0 Then
            Return False
        End If

        layoutCount = readOnlyCollection.Count - 1
        Layout = New LayoutParser(layoutCount) {}
        
        Dim lcount As Integer = 0
        Do While lcount <= layoutCount
            Layout(lcount) = New LayoutParser()
            Layout(lcount).Init(readOnlyCollection(lcount))
            Layout(lcount).[Path] = readOnlyCollection(lcount)

            Dim dynItem As New ToolStripMenuItem() With {.Text = Layout(lcount).Name, .Name = lcount.ToString(), .Tag = lcount}
            MainUI.LayoutList.Items.Add(dynItem)
            AddHandler dynItem.Click, AddressOf MainUI.mnuItem_Clicked

            If Layout(lcount).Name.ToLower().Contains("national") Then
                LastFixedTag = lcount
            End If

            Layout(lcount).ID = lcount
            lcount += 1
        Loop

        Return True
    End Function

    Public Shared Function ConvertJsonToKbl(ByVal json As String, ByVal layoutName As String) As String
        Try
            Dim matches = System.Text.RegularExpressions.Regex.Matches(json, """Key_([a-zA-Z0-9_]+)_(Normal|AltGr)""\s*:\s*""([^""]*)""")
            Dim keyMap As New Dictionary(Of String, Dictionary(Of String, String))()

            For Each m As System.Text.RegularExpressions.Match In matches
                Dim keyName = m.Groups(1).Value
                Dim state = m.Groups(2).Value
                Dim val = m.Groups(3).Value

                If Not keyMap.ContainsKey(keyName) Then
                    keyMap(keyName) = New Dictionary(Of String, String)()
                End If
                keyMap(keyName)(state) = val
            Next

            Dim sb As New System.Text.StringBuilder()
            sb.AppendLine("<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>")
            sb.AppendLine("<Layout>")
            sb.AppendLine("  <Name>" & layoutName & "</Name>")
            sb.AppendLine("  <Keys>")

            sb.AppendLine("    <Key vkCode=""32"">")
            sb.AppendLine("      <Normal> </Normal>")
            sb.AppendLine("      <Shift> </Shift>")
            sb.AppendLine("    </Key>")

            Dim keyMappings As New Dictionary(Of Integer, Tuple(Of String, String))()

            For vk As Integer = 65 To 90
                Dim charLower = ChrW(vk + 32).ToString()
                Dim charUpper = ChrW(vk).ToString()
                keyMappings(vk) = Tuple.Create(charLower, charUpper)
            Next

            keyMappings(48) = Tuple.Create("0", "ParenRight")
            keyMappings(49) = Tuple.Create("1", "Exclaim")
            keyMappings(50) = Tuple.Create("2", "At")
            keyMappings(51) = Tuple.Create("3", "Hash")
            keyMappings(52) = Tuple.Create("4", "Dollar")
            keyMappings(53) = Tuple.Create("5", "Percent")
            keyMappings(54) = Tuple.Create("6", "Circum")
            keyMappings(55) = Tuple.Create("7", "Ampersand")
            keyMappings(56) = Tuple.Create("8", "Asterisk")
            keyMappings(57) = Tuple.Create("9", "ParenLeft")

            keyMappings(186) = Tuple.Create("Semicolon", "Colon")
            keyMappings(187) = Tuple.Create("Equals", "Plus")
            keyMappings(188) = Tuple.Create("Comma", "Less")
            keyMappings(189) = Tuple.Create("Minus", "UnderScore")
            keyMappings(190) = Tuple.Create("Period", "Greater")
            keyMappings(191) = Tuple.Create("Slash", "Question")
            keyMappings(192) = Tuple.Create("Grave", "Tilde")
            keyMappings(219) = Tuple.Create("BracketLeft", "BraceLeft")
            keyMappings(220) = Tuple.Create("BackSlash", "Bar")
            keyMappings(221) = Tuple.Create("BracketRight", "BraceRight")
            keyMappings(222) = Tuple.Create("Apostrophe", "Quote")

            For Each kvp In keyMappings
                Dim vk = kvp.Key
                Dim normKey = kvp.Value.Item1
                Dim shiftKey = kvp.Value.Item2

                Dim normVal = ""
                Dim shiftVal = ""

                If keyMap.ContainsKey(normKey) AndAlso keyMap(normKey).ContainsKey("Normal") Then
                    normVal = keyMap(normKey)("Normal")
                End If
                If keyMap.ContainsKey(shiftKey) AndAlso keyMap(shiftKey).ContainsKey("Normal") Then
                    shiftVal = keyMap(shiftKey)("Normal")
                End If

                If normVal = "E" Then normVal = ""
                If shiftVal = "E" Then shiftVal = ""

                normVal = EscapeXml(normVal)
                shiftVal = EscapeXml(shiftVal)

                sb.AppendLine("    <Key vkCode=""" & vk & """>")
                sb.AppendLine("      <Normal>" & normVal & "</Normal>")
                sb.AppendLine("      <Shift>" & shiftVal & "</Shift>")
                sb.AppendLine("    </Key>")
            Next

            sb.AppendLine("  </Keys>")
            sb.AppendLine("  <Combinations>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্অ</Input>")
            sb.AppendLine("      <Output>া</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্আ</Input>")
            sb.AppendLine("      <Output>া</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্ই</Input>")
            sb.AppendLine("      <Output>ি</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্ঈ</Input>")
            sb.AppendLine("      <Output>ী</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্উ</Input>")
            sb.AppendLine("      <Output>ু</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্ঊ</Input>")
            sb.AppendLine("      <Output>ূ</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্ঋ</Input>")
            sb.AppendLine("      <Output>ৃ</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্এ</Input>")
            sb.AppendLine("      <Output>ে</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্ঐ</Input>")
            sb.AppendLine("      <Output>ৈ</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্ও</Input>")
            sb.AppendLine("      <Output>ো</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("    <Combination>")
            sb.AppendLine("      <Input>্ঔ</Input>")
            sb.AppendLine("      <Output>ৌ</Output>")
            sb.AppendLine("    </Combination>")
            sb.AppendLine("  </Combinations>")
            sb.AppendLine("</Layout>")

            Return sb.ToString()
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Private Shared Function EscapeXml(ByVal val As String) As String
        If String.IsNullOrEmpty(val) Then Return ""
        Return val.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;").Replace("'", "&apos;")
    End Function



    'Got reading idea from: https://www.codeproject.com/Questions/696096/Load-A-Specific-XML-File

    Public Sub Init(ByVal Path As String)
        Dim xmlReaderSetting As XmlReaderSettings = New XmlReaderSettings() With
        {
            .CheckCharacters = True,
            .CloseInput = True
        }
        Using xmlReader As XmlReader = XmlReader.Create(Path, xmlReaderSetting)
            Try
                Try
                    xmlReader.ReadStartElement("Layout")
                    Me.Name = xmlReader.ReadElementString("Name")
                    xmlReader.ReadStartElement("Keys")
                    Do
                        Dim arrayLists As ArrayList = New ArrayList()
                        If (xmlReader.NodeType = XmlNodeType.Element Or xmlReader.NodeType = XmlNodeType.Attribute) Then
                            Dim [integer] As Integer = Conversions.ToInteger(xmlReader.GetAttribute("vkCode"))
                            arrayLists.Add(xmlReader.GetAttribute("N_O"))
                            arrayLists.Add(xmlReader.GetAttribute("S_O"))
                            xmlReader.Read()
                            arrayLists.Insert(0, xmlReader.ReadElementString("Normal"))
                            arrayLists.Insert(1, xmlReader.ReadElementString("Shift"))
                            Me.Key.Add([integer], arrayLists)
                        End If
                        xmlReader.Read()
                    Loop While Operators.CompareString(xmlReader.Name, "Keys", False) <> 0
                    xmlReader.ReadEndElement()
                    xmlReader.ReadStartElement("Combinations")
                    Do
                        If (xmlReader.NodeType = XmlNodeType.Element) Then
                            xmlReader.Read()
                            Dim sequence As String = xmlReader.ReadElementString("Input")
                            Dim output As String = xmlReader.ReadElementString("Output")
                            Me.KeySequences.Add(sequence, output)
                        End If
                        xmlReader.Read()
                    Loop While Operators.CompareString(xmlReader.Name, "Combinations", False) <> 0
                Catch exception As Exception

                End Try
            Finally
                xmlReader.Close()
            End Try
        End Using
    End Sub


End Class

