'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   Borno Lite "Advance" additions: persistent settings store.
'

Imports System.IO
Imports System.Xml.Linq

''' <summary>
''' Lightweight persistent settings for Borno Lite, stored as data\settings.xml.
''' Mirrors the existing first-run file pattern used for keyboard layouts.
''' </summary>
Module AppSettings

    ' Backing values (defaults applied here)
    Public LastLayoutTag As Integer = -1          ' -1 = nothing selected yet
    Public Activated As Boolean = False           ' Bangla mode on/off (mirrors isActivated)
    Public ThemeName As String = "BanglaType Cream"
    Public OutputMode As String = "Unicode"       ' "Unicode" or "ANSI"
    Public SuggestionsEnabled As Boolean = True
    Public AvroEnabled As Boolean = True          ' show the open Avro layout in the list
    Public Hotkey As String = "F12"               ' default toggle hotkey
    Public GeminiApiKey As String = ""
    Public FirstRun As Boolean = True             ' Show Setup Wizard if True
    Public PrivacyConsent As Boolean = False      ' User privacy consent
    Public DefaultLang As String = "en"           ' Default language ("bn" or "en")

    Private Function SettingsFolder() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType")
    End Function

    Private Function SettingsPath() As String
        Return Path.Combine(SettingsFolder(), "settings.xml")
    End Function

    ''' <summary>Loads settings from disk; silently keeps defaults if the file is missing/corrupt.</summary>
    Public Sub Load()
        Try
            Dim p As String = SettingsPath()
            If Not File.Exists(p) Then Return

            Dim doc As XDocument = XDocument.Load(p)
            Dim root As XElement = doc.Root
            If root Is Nothing Then Return

            LastLayoutTag = GetInt(root, "LastLayoutTag", LastLayoutTag)
            Activated = GetBool(root, "Activated", Activated)
            ThemeName = GetStr(root, "ThemeName", ThemeName)
            OutputMode = GetStr(root, "OutputMode", OutputMode)
            SuggestionsEnabled = GetBool(root, "SuggestionsEnabled", SuggestionsEnabled)
            AvroEnabled = GetBool(root, "AvroEnabled", AvroEnabled)
            Hotkey = GetStr(root, "Hotkey", Hotkey)
            GeminiApiKey = GetStr(root, "GeminiApiKey", GeminiApiKey)
            FirstRun = GetBool(root, "FirstRun", FirstRun)
            PrivacyConsent = GetBool(root, "PrivacyConsent", PrivacyConsent)
            DefaultLang = GetStr(root, "DefaultLang", DefaultLang)
        Catch
            ' Keep defaults on any read/parse error.
        End Try
    End Sub

    ''' <summary>Writes the current settings to disk, creating the data folder if needed.</summary>
    Public Sub Save()
        Try
            Directory.CreateDirectory(SettingsFolder())
            Dim doc As New XDocument(
                New XElement("BanglaType",
                     New XElement("LastLayoutTag", LastLayoutTag),
                     New XElement("Activated", Activated),
                     New XElement("ThemeName", ThemeName),
                     New XElement("OutputMode", OutputMode),
                     New XElement("SuggestionsEnabled", SuggestionsEnabled),
                     New XElement("AvroEnabled", AvroEnabled),
                     New XElement("Hotkey", Hotkey),
                     New XElement("GeminiApiKey", GeminiApiKey),
                     New XElement("FirstRun", FirstRun),
                     New XElement("PrivacyConsent", PrivacyConsent),
                     New XElement("DefaultLang", DefaultLang)))
            doc.Save(SettingsPath())
        Catch
            ' Best-effort persistence; ignore disk errors.
        End Try
    End Sub

    Private Function GetStr(root As XElement, name As String, fallback As String) As String
        Dim e As XElement = root.Element(name)
        If e Is Nothing OrElse String.IsNullOrEmpty(e.Value) Then Return fallback
        Return e.Value
    End Function

    Private Function GetInt(root As XElement, name As String, fallback As Integer) As Integer
        Dim e As XElement = root.Element(name)
        Dim v As Integer
        If e IsNot Nothing AndAlso Integer.TryParse(e.Value, v) Then Return v
        Return fallback
    End Function

    Private Function GetBool(root As XElement, name As String, fallback As Boolean) As Boolean
        Dim e As XElement = root.Element(name)
        Dim v As Boolean
        If e IsNot Nothing AndAlso Boolean.TryParse(e.Value, v) Then Return v
        Return fallback
    End Function

End Module
