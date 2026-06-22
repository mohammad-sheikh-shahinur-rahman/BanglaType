Imports System.IO
Imports System.Text
Imports System.Collections.Generic
Imports System.Net

Public Module SuggestionEngine

    Public Enum AppContextTone
        Casual
        Formal
    End Enum

    Public Enum SentimentTone
        Friendly
        Professional
        Angry
    End Enum

    Private freq As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
    Private bigrams As New Dictionary(Of String, Dictionary(Of String, Integer))(StringComparer.OrdinalIgnoreCase)
    Private customMappings As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private autoCorrect As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private englishWords As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private onlineCache As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)
    Private fetchingPrefix As String = ""
    Private loaded As Boolean = False
    Private dirty As Boolean = False
    Private lastSave As DateTime = DateTime.MinValue
    Private lastWord As String = ""

    Public Const MaxResults As Integer = 6

    Public Function IsRoman(ByVal text As String) As Boolean
        If String.IsNullOrEmpty(text) Then Return False
        For Each c As Char In text
            Dim code As Integer = AscW(c)
            If (code >= 65 AndAlso code <= 90) OrElse (code >= 97 AndAlso code <= 122) Then
                ' OK
            Else
                Return False
            End If
        Next
        Return True
    End Function

    Public Function DetectContextAndTone(ByVal prefix As String) As Tuple(Of AppContextTone, SentimentTone)
        Dim appTone As AppContextTone = AppContextTone.Casual
        Dim sentTone As SentimentTone = SentimentTone.Friendly

        ' 1. Detect app context
        Dim procName As String = ""
        Dim winTitle As String = ""
        If NativeMethods.GetActiveAppInfo(procName, winTitle) Then
            Dim lowerProc As String = procName.ToLower()
            Dim lowerTitle As String = winTitle.ToLower()

            If lowerProc.Contains("outlook") OrElse lowerProc.Contains("winword") OrElse _
               lowerProc.Contains("excel") OrElse lowerTitle.Contains("gmail") OrElse _
               lowerTitle.Contains("mail") OrElse lowerTitle.Contains("linkedin") Then
                appTone = AppContextTone.Formal
                sentTone = SentimentTone.Professional
            End If
        End If

        ' 2. Detect sentiment tone from prefix
        Dim lowerText As String = prefix.ToLower()
        If lowerText.Contains("baje") OrElse lowerText.Contains("angry") OrElse lowerText.Contains("rag") OrElse lowerText.Contains("faltu") Then
            sentTone = SentimentTone.Angry
        ElseIf lowerText.Contains("please") OrElse lowerText.Contains("anurodh") OrElse lowerText.Contains("office") OrElse lowerText.Contains("mail") Then
            sentTone = SentimentTone.Professional
        ElseIf lowerText.Contains("bhalo") OrElse lowerText.Contains("happy") OrElse lowerText.Contains("thanks") OrElse lowerText.Contains("dhonnobad") Then
            sentTone = SentimentTone.Friendly
        End If

        Return Tuple.Create(appTone, sentTone)
    End Function

    Private Function DictFolder() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BanglaType", "dictionary")
    End Function
    Private Function BasePath() As String
        Return Path.Combine(DictFolder(), "bn_words.txt")
    End Function
    Private Function UserPath() As String
        Return Path.Combine(DictFolder(), "user_words.txt")
    End Function
    Private Function BigramPath() As String
        Return Path.Combine(DictFolder(), "user_bigrams.txt")
    End Function

    Public Sub ResetContext()
        lastWord = ""
    End Sub

    ''' <summary>Loads base + user dictionaries + bigrams (idempotent).</summary>
    Public Sub EnsureLoaded()
        If loaded Then Return
        loaded = True
        Try
            Directory.CreateDirectory(DictFolder())
            If Not File.Exists(BasePath()) Then
                File.WriteAllText(BasePath(), SeedWords(), New UTF8Encoding(False))
            End If
            ReadFile(BasePath())
            If File.Exists(UserPath()) Then ReadFile(UserPath())
            If File.Exists(BigramPath()) Then ReadBigrams(BigramPath())
            LoadCustomMappings()
            LoadAutoCorrect()
            LoadEnglishWords()
            LoadJsonDataFiles()
        Catch
            ' Suggestions are best-effort; ignore IO errors.
        End Try
    End Sub

    Private Sub LoadJsonDataFiles()
        Dim dataFolder As String = ""
        Try
            Dim prodPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"))
            If Directory.Exists(prodPath) Then
                dataFolder = prodPath
            Else
                Dim devPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "data"))
                If Directory.Exists(devPath) Then
                    dataFolder = devPath
                End If
            End If
        Catch
        End Try

        If String.IsNullOrEmpty(dataFolder) Then Return

        ' 1. Load dictionary.json (Words list)
        Dim dictPath = Path.Combine(dataFolder, "dictionary.json")
        If File.Exists(dictPath) Then
            Try
                Dim content = File.ReadAllText(dictPath, Encoding.UTF8)
                ' Matches all Bengali words inside double quotes, e.g. "দংগল"
                Dim matches = System.Text.RegularExpressions.Regex.Matches(content, """([\u0980-\u09FF]+)""")
                For Each m As System.Text.RegularExpressions.Match In matches
                    Dim w As String = m.Groups(1).Value
                    If w.Length >= 2 Then
                        If Not freq.ContainsKey(w) Then
                            freq(w) = 1
                        End If
                    End If
                Next
            Catch
            End Try
        End If

        ' 2. Load autocorrect.json (custom phonetic suggestions/corrections)
        Dim autoPath = Path.Combine(dataFolder, "autocorrect.json")
        If File.Exists(autoPath) Then
            Try
                Dim content = File.ReadAllText(autoPath, Encoding.UTF8)
                ' Match "wrong": "right"
                Dim matches = System.Text.RegularExpressions.Regex.Matches(content, """([^""]+)""\s*:\s*""([^""]+)""")
                For Each m As System.Text.RegularExpressions.Match In matches
                    Dim wrong As String = m.Groups(1).Value.Trim().ToLower()
                    Dim right As String = m.Groups(2).Value.Trim()
                    If wrong.Length > 0 AndAlso right.Length > 0 AndAlso Not wrong.Equals(right) Then
                        ' Load into autocorrect if it maps to correct Bangla
                        ' If right contains Bengali letters, it is custom mapping
                        ' If right is English, it is raw autocorrect input replacement
                        If IsBengaliString(right) Then
                            customMappings(wrong) = right
                        Else
                            autoCorrect(wrong) = right
                        End If
                    End If
                Next
            Catch
            End Try
        End If

        ' 3. Load suffix.json (custom suffixes)
        Dim suffixPath = Path.Combine(dataFolder, "suffix.json")
        If File.Exists(suffixPath) Then
            Try
                Dim content = File.ReadAllText(suffixPath, Encoding.UTF8)
                ' Match "suffix_roman": "suffix_bangla"
                Dim matches = System.Text.RegularExpressions.Regex.Matches(content, """([^""]+)""\s*:\s*""([^""]+)""")
                For Each m As System.Text.RegularExpressions.Match In matches
                    Dim wrong As String = m.Groups(1).Value.Trim().ToLower()
                    Dim right As String = m.Groups(2).Value.Trim()
                    If wrong.Length > 0 AndAlso right.Length > 0 Then
                        customMappings(wrong) = right
                    End If
                Next
            Catch
            End Try
        End If
    End Sub

    Private Function IsBengaliString(ByVal s As String) As Boolean
        For Each c As Char In s
            Dim code As Integer = Convert.ToInt32(c)
            If code >= &H980 AndAlso code <= &H9FF Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub ReadFile(ByVal path As String)
        For Each line As String In File.ReadAllLines(path, Encoding.UTF8)
            If String.IsNullOrWhiteSpace(line) Then Continue For
            Dim parts() As String = line.Split(ControlChars.Tab)
            Dim w As String = parts(0).Trim()
            If w.Length = 0 Then Continue For
            Dim f As Integer = 1
            If parts.Length > 1 Then Integer.TryParse(parts(1).Trim(), f)
            Dim cur As Integer
            If freq.TryGetValue(w, cur) Then
                freq(w) = Math.Max(cur, f)
            Else
                freq(w) = f
            End If
        Next
    End Sub

    Private Sub ReadBigrams(ByVal path As String)
        Try
            For Each line As String In File.ReadAllLines(path, Encoding.UTF8)
                If String.IsNullOrWhiteSpace(line) Then Continue For
                Dim parts() As String = line.Split(ControlChars.Tab)
                If parts.Length >= 3 Then
                    Dim w1 As String = parts(0).Trim()
                    Dim w2 As String = parts(1).Trim()
                    Dim f As Integer = 1
                    Integer.TryParse(parts(2).Trim(), f)
                    If w1.Length > 0 AndAlso w2.Length > 0 Then
                        Dim subDict As Dictionary(Of String, Integer) = Nothing
                        If Not bigrams.TryGetValue(w1, subDict) Then
                            subDict = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
                            bigrams(w1) = subDict
                        End If
                        subDict(w2) = f
                    End If
                End If
            Next
        Catch
        End Try
    End Sub

    ''' <summary>Returns up to MaxResults words that start with prefix, ranked by frequency + emojis.</summary>
    Public Function GetSuggestions(ByVal prefix As String) As List(Of String)
        EnsureLoaded()
        Dim result As New List(Of String)()
        If String.IsNullOrEmpty(prefix) Then Return result

        Dim lower As String = prefix.ToLower()

        ' 1. Custom Mappings (Exact match or starts with)
        For Each kvp In customMappings
            If kvp.Key.Equals(lower, StringComparison.OrdinalIgnoreCase) Then
                If Not result.Contains(kvp.Value) Then result.Add(kvp.Value)
            End If
        Next
        For Each kvp In customMappings
            If kvp.Key.StartsWith(lower, StringComparison.OrdinalIgnoreCase) AndAlso Not kvp.Key.Equals(lower, StringComparison.OrdinalIgnoreCase) Then
                If Not result.Contains(kvp.Value) Then result.Add(kvp.Value)
            End If
        Next

        ' 2. Online suggestions (from Cache)
        SyncLock onlineCache
            If onlineCache.ContainsKey(prefix) Then
                For Each w In onlineCache(prefix)
                    If Not result.Contains(w) Then result.Add(w)
                Next
            End If
        End SyncLock

        ' Trigger async fetching if not cached and length >= 2
        If Not onlineCache.ContainsKey(prefix) AndAlso prefix.Length >= 2 AndAlso fetchingPrefix <> prefix Then
            fetchingPrefix = prefix
            Dim thread As New System.Threading.Thread(Sub() FetchOnlineSuggestionsAsync(prefix))
            thread.IsBackground = True
            thread.Start()
        End If

        ' Detect App Context and Sentiment Tone
        Dim appTone As AppContextTone = AppContextTone.Casual
        Dim sentTone As SentimentTone = SentimentTone.Friendly
        Try
            Dim ct = DetectContextAndTone(prefix)
            appTone = ct.Item1
            sentTone = ct.Item2
        Catch
        End Try

        ' 3) Mood-based / Context-based suggestions
        If appTone = AppContextTone.Formal Then
            ' Formal tone suggestions
            If "dhonnobad".StartsWith(lower) Then result.Add("ধন্যবাদ")
            If "shubheccha".StartsWith(lower) Then result.Add("শুভেচ্ছা")
            If "anurodh".StartsWith(lower) Then result.Add("অনুরোধ")
            If "abedon".StartsWith(lower) Then result.Add("আবেদন")
        Else
            ' Casual / Emoji suggestions based on sentiment tone
            If sentTone = SentimentTone.Angry Then
                result.Add("😠")
                result.Add("😡")
                If "rag".StartsWith(lower) Then result.Add("রাগ")
                If "baje".StartsWith(lower) Then result.Add("বাজে")
            ElseIf sentTone = SentimentTone.Professional Then
                result.Add("💼")
                If "dhonnobad".StartsWith(lower) Then result.Add("ধন্যবাদ")
            Else
                ' Friendly tone - standard mood suggestions
                If lower.Contains("bhalo") OrElse lower.Contains("happy") OrElse lower.Contains("hashi") Then
                    result.Add("😊")
                    result.Add("😃")
                ElseIf lower.Contains("sad") OrElse lower.Contains("khrap") OrElse lower.Contains("kanna") Then
                    result.Add("😢")
                    result.Add("😭")
                ElseIf lower.Contains("valobashi") OrElse lower.Contains("love") Then
                    result.Add("❤️")
                    result.Add("😍")
                ElseIf lower.Contains("dhonnobad") OrElse lower.Contains("thanks") Then
                    result.Add("🙏")
                    result.Add("👍")
                End If
            End If
        End If

        ' Universal Language Switch AI fallback:
        ' If typing in English/mixed, allow keeping the exact English word.
        If IsRoman(prefix) AndAlso Not result.Contains(prefix) Then
            result.Add(prefix)
        End If

        ' 4) Prefix-matching words from frequency dictionary
        Dim matches As New List(Of KeyValuePair(Of String, Integer))()
        For Each kv As KeyValuePair(Of String, Integer) In freq
            If kv.Key.Length > prefix.Length AndAlso kv.Key.StartsWith(prefix, StringComparison.Ordinal) Then
                matches.Add(kv)
            End If
        Next
        matches.Sort(Function(a, b) b.Value.CompareTo(a.Value))

        For Each kv As KeyValuePair(Of String, Integer) In matches
            If Not result.Contains(kv.Key) Then
                result.Add(kv.Key)
            End If
            If result.Count >= MaxResults Then Exit For
        Next

        Return result
    End Function

    ''' <summary>Returns next-word predictions based on the previous word.</summary>
    Public Function GetNextWordSuggestions(ByVal prevWord As String) As List(Of String)
        EnsureLoaded()
        Dim result As New List(Of String)()
        If String.IsNullOrEmpty(prevWord) Then Return result

        Dim subDict As Dictionary(Of String, Integer) = Nothing
        If bigrams.TryGetValue(prevWord, subDict) Then
            Dim matches As New List(Of KeyValuePair(Of String, Integer))()
            For Each kv In subDict
                matches.Add(kv)
            Next
            matches.Sort(Function(a, b) b.Value.CompareTo(a.Value))
            For Each kv In matches
                result.Add(kv.Key)
                If result.Count >= MaxResults Then Exit For
            Next
        End If
        Return result
    End Function

    ''' <summary>Records word occurrence and updates bigrams.</summary>
    Public Sub Learn(ByVal word As String)
        If String.IsNullOrWhiteSpace(word) Then Return
        EnsureLoaded()
        
        ' Learn single word
        Dim cur As Integer
        If freq.TryGetValue(word, cur) Then
            freq(word) = cur + 1
        Else
            freq(word) = 2
        End If

        ' Learn bigram
        If Not String.IsNullOrEmpty(lastWord) Then
            Dim subDict As Dictionary(Of String, Integer) = Nothing
            If Not bigrams.TryGetValue(lastWord, subDict) Then
                subDict = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
                bigrams(lastWord) = subDict
            End If
            Dim bf As Integer = 0
            If subDict.TryGetValue(word, bf) Then
                subDict(word) = bf + 1
            Else
                subDict(word) = 1
            End If
        End If
        lastWord = word

        dirty = True
        SaveUserDebounced()
    End Sub

    Public Sub SaveUserDebounced(Optional ByVal force As Boolean = False)
        If Not dirty Then Return
        If Not force AndAlso (DateTime.Now - lastSave).TotalSeconds < 5 Then Return
        Try
            Directory.CreateDirectory(DictFolder())
            
            ' Save words
            Dim sbWords As New StringBuilder()
            For Each kv As KeyValuePair(Of String, Integer) In freq
                If kv.Value >= 2 Then sbWords.Append(kv.Key).Append(ControlChars.Tab).Append(kv.Value).Append(vbLf)
            Next
            File.WriteAllText(UserPath(), sbWords.ToString(), New UTF8Encoding(False))

            ' Save bigrams
            Dim sbBigrams As New StringBuilder()
            For Each kvp1 In bigrams
                For Each kvp2 In kvp1.Value
                    sbBigrams.Append(kvp1.Key).Append(ControlChars.Tab).Append(kvp2.Key).Append(ControlChars.Tab).Append(kvp2.Value).Append(vbLf)
                Next
            Next
            File.WriteAllText(BigramPath(), sbBigrams.ToString(), New UTF8Encoding(False))

            dirty = False
            lastSave = DateTime.Now
        Catch
        End Try
    End Sub

    Private Function SeedWords() As String
        Dim words() As String = {
            "আমি", "আমরা", "আমার", "আমাদের", "তুমি", "তোমার", "তোমাদের", "সে", "তারা", "তাদের",
            "এই", "ওই", "সেই", "এটা", "ওটা", "কি", "কী", "কেন", "কখন", "কোথায়",
            "বাংলা", "বাংলাদেশ", "ভাষা", "মানুষ", "দেশ", "শহর", "গ্রাম", "নদী", "আকাশ", "মাটি",
            "ভালো", "ভালোবাসা", "ভালোবাসি", "সুন্দর", "বন্ধু", "পরিবার", "মা", "বাবา", "ভাই", "বোন",
            "স্কুল", "কলেজ", "বিশ্ববিদ্যালয়", "শিক্ষা", "ছাত্র", "শিক্ষক", "বই", "কলম", "পড়া", "লেখা",
            "কাজ", "সময়", "জীবন", "পৃথিবী", "সূর্য", "চাঁদ", "তারা", "ফুল", "পাখি", "গাছ",
            "খাবার", "পানি", "ভাত", "মাছ", "চা", "দুধ", "ফল", "সবজি", "রান্না", "ক্ষুধা",
            "ধন্যবাদ", "স্বাগতম", "শুভেচ্ছা", "অভিনন্দন", "দুঃখিত", "ক্ষমা", "অনুগ্রহ", "সাহায্য",
            "ভালোবাসার", "প্রিয়", "খুশি", "আনন্দ", "হাসি", "কান্না", "স্বপ্ন", "আশা", " hishabe",
            "গান", "গাই", "নাচ", "ছবি", "সিনেমা", "খেলা", "ক্রিকেট", "ফুটবল", "খেলোয়াড়",
            "প্রযুক্তি", "কম্পিউটার", "মোবাইল", "ইন্টারনেট", "সফটওয়্যার", "প্রোগ্রাম", " hishabe",
            "ভাষায়", "বাংলায়", "করি", "করছি", "করব", "হবে", "হয়েছে", "যাব", "যাচ্ছি", "এসেছি"
        }
        Dim sb As New StringBuilder()
        For Each w As String In words
            sb.Append(w).Append(ControlChars.Tab).Append("3").Append(vbLf)
        Next
        Return sb.ToString()
    End Function

    Public Function GetAllUserWords() As List(Of String)
        EnsureLoaded()
        Dim list As New List(Of String)()
        Try
            Dim path As String = UserPath()
            If File.Exists(path) Then
                For Each line As String In File.ReadAllLines(path, Encoding.UTF8)
                    If String.IsNullOrWhiteSpace(line) Then Continue For
                    Dim parts() As String = line.Split(ControlChars.Tab)
                    Dim w As String = parts(0).Trim()
                    If w.Length > 0 AndAlso Not list.Contains(w) Then
                        list.Add(w)
                    End If
                Next
            End If
        Catch
        End Try
        Return list
    End Function

    Public Sub AddCustomWord(ByVal word As String)
        word = word.Trim()
        If String.IsNullOrEmpty(word) Then Return
        EnsureLoaded()
        freq(word) = Math.Max(2, If(freq.ContainsKey(word), freq(word), 2))
        dirty = True
        SaveUserDebounced(force:=True)
    End Sub

    Public Sub RemoveCustomWord(ByVal word As String)
        word = word.Trim()
        If String.IsNullOrEmpty(word) Then Return
        EnsureLoaded()
        If freq.ContainsKey(word) Then
            freq.Remove(word)
        End If
        dirty = True
        SaveUserDebounced(force:=True)
    End Sub

    ' --- Custom Word Customizer (Phonetic / Special Character Mappings) ---

    Private Function CustomMappingsPath() As String
        Return Path.Combine(DictFolder(), "custom_mappings.txt")
    End Function

    Private Sub LoadCustomMappings()
        Try
            customMappings.Clear()
            Dim path As String = CustomMappingsPath()
            If File.Exists(path) Then
                For Each line As String In File.ReadAllLines(path, Encoding.UTF8)
                    If String.IsNullOrWhiteSpace(line) Then Continue For
                    Dim parts() As String = line.Split(ControlChars.Tab)
                    If parts.Length >= 2 Then
                        Dim key As String = parts(0).Trim().ToLower()
                        Dim target As String = parts(1).Trim()
                        If key.Length > 0 AndAlso target.Length > 0 Then
                            customMappings(key) = target
                        End If
                    End If
                Next
            End If
        Catch
        End Try
    End Sub

    Public Sub SaveCustomPhoneticMappings(ByVal targetWord As String, ByVal variations As List(Of String))
        EnsureLoaded()
        Try
            ' Remove any existing mapping for these variations
            For Each v As String In variations
                Dim cleanV As String = v.Trim().ToLower()
                If cleanV.Length > 0 Then
                    customMappings(cleanV) = targetWord
                End If
            Next

            ' Also ensure the targetWord is added to our frequency dict to support prediction
            AddCustomWord(targetWord)

            ' Write all mappings to file
            Dim sb As New StringBuilder()
            For Each kvp In customMappings
                sb.Append(kvp.Key).Append(ControlChars.Tab).Append(kvp.Value).Append(vbLf)
            Next
            File.WriteAllText(CustomMappingsPath(), sb.ToString(), New UTF8Encoding(False))
        Catch
        End Try
    End Sub

    ' --- Auto-correction (common Bangla spelling mistakes) ---

    Private Function AutoCorrectPath() As String
        Return Path.Combine(DictFolder(), "autocorrect.txt")
    End Function

    ''' <summary>Loads the wrong->right correction table, seeding a starter file on first run.</summary>
    Private Sub LoadAutoCorrect()
        Try
            autoCorrect.Clear()
            Dim path As String = AutoCorrectPath()
            If Not File.Exists(path) Then
                File.WriteAllText(path, SeedAutoCorrect(), New UTF8Encoding(False))
            End If
            For Each line As String In File.ReadAllLines(path, Encoding.UTF8)
                If String.IsNullOrWhiteSpace(line) OrElse line.StartsWith("#") Then Continue For
                Dim parts() As String = line.Split(ControlChars.Tab)
                If parts.Length >= 2 Then
                    Dim wrong As String = parts(0).Trim()
                    Dim right As String = parts(1).Trim()
                    If wrong.Length > 0 AndAlso right.Length > 0 AndAlso Not wrong.Equals(right) Then
                        autoCorrect(wrong) = right
                    End If
                End If
            Next
        Catch
        End Try
    End Sub

    ''' <summary>Returns the corrected spelling for a Bangla word, or "" when no correction applies.</summary>
    Public Function GetAutoCorrection(ByVal word As String) As String
        If String.IsNullOrEmpty(word) Then Return ""
        EnsureLoaded()
        Dim fixedWord As String = Nothing
        If autoCorrect.TryGetValue(word, fixedWord) Then Return fixedWord
        Return ""
    End Function

    Private Function SeedAutoCorrect() As String
        ' Tab-separated wrong<TAB>right pairs. Users may add their own lines.
        Dim pairs() As String = {
            "আমী" & vbTab & "আমি",
            "তুমী" & vbTab & "তুমি",
            "করছী" & vbTab & "করছি",
            "করব়" & vbTab & "করব",
            "ভালবাসা" & vbTab & "ভালোবাসা",
            "ভালোবাশা" & vbTab & "ভালোবাসা",
            "দুঃখীত" & vbTab & "দুঃখিত",
            "ধন্যবাধ" & vbTab & "ধন্যবাদ",
            "ধন্যবা" & vbTab & "ধন্যবাদ",
            "অভিনন্ধন" & vbTab & "অভিনন্দন",
            "বাংলাদেস" & vbTab & "বাংলাদেশ",
            "বাংলাদেষ" & vbTab & "বাংলাদেশ",
            "স্বাগতম়" & vbTab & "স্বাগতম",
            "ইনশাআল্লাহ" & vbTab & "ইনশাআল্লাহ",
            "আসসালামুআলাইকুম" & vbTab & "আসসালামু আলাইকুম"
        }
        Dim sb As New StringBuilder()
        sb.Append("# BanglaType auto-correction list  (wrong<TAB>right). Add your own lines below.").Append(vbLf)
        For Each p As String In pairs
            sb.Append(p).Append(vbLf)
        Next
        Return sb.ToString()
    End Function

    ' --- Mixed typing (keep recognised English words in English) ---

    Private Function EnglishWordsPath() As String
        Return Path.Combine(DictFolder(), "english_words.txt")
    End Function

    Private Sub LoadEnglishWords()
        Try
            englishWords.Clear()
            Dim path As String = EnglishWordsPath()
            If Not File.Exists(path) Then
                File.WriteAllText(path, SeedEnglishWords(), New UTF8Encoding(False))
            End If
            For Each line As String In File.ReadAllLines(path, Encoding.UTF8)
                Dim w As String = line.Trim()
                If w.Length >= 2 AndAlso Not w.StartsWith("#") Then englishWords.Add(w)
            Next
        Catch
        End Try
    End Sub

    ''' <summary>True when the romanized buffer is a recognised English word that should be left untransliterated.</summary>
    Public Function IsEnglishWord(ByVal roman As String) As Boolean
        If String.IsNullOrEmpty(roman) Then Return False
        If roman.Length < 3 Then Return False
        If Not IsRoman(roman) Then Return False
        EnsureLoaded()
        Return englishWords.Contains(roman)
    End Function

    Private Function SeedEnglishWords() As String
        Dim words() As String = {
            "school", "college", "university", "office", "computer", "laptop", "mobile", "phone",
            "internet", "online", "offline", "email", "software", "hardware", "website", "browser",
            "facebook", "google", "youtube", "twitter", "instagram", "whatsapp", "messenger",
            "video", "photo", "camera", "screen", "keyboard", "mouse", "printer", "scanner",
            "doctor", "engineer", "teacher", "student", "class", "exam", "result", "project",
            "meeting", "market", "bank", "hospital", "restaurant", "hotel", "ticket", "train",
            "manager", "password", "login", "logout", "account", "message", "group", "link",
            "page", "post", "share", "comment", "profile", "status", "update", "download",
            "upload", "network", "server", "data", "file", "folder", "download", "windows",
            "android", "iphone", "version", "update", "battery", "charger", "wifi", "router"
        }
        Dim sb As New StringBuilder()
        sb.Append("# BanglaType mixed-typing English word list. One word per line; add your own.").Append(vbLf)
        For Each w As String In words
            sb.Append(w).Append(vbLf)
        Next
        Return sb.ToString()
    End Function

    ' --- Online Google / Gemini AI Suggestion Fetcher ---

    Private Function ParseGoogleSuggestions(ByVal input As String, ByVal json As String) As List(Of String)
        Dim list As New List(Of String)()
        Try
            Dim searchStr As String = """" & input.Replace("""", "\""") & """,["
            Dim idx As Integer = json.IndexOf(searchStr, StringComparison.OrdinalIgnoreCase)
            If idx = -1 Then
                idx = json.IndexOf(",[")
                If idx <> -1 Then
                    idx = json.IndexOf("[", idx + 2)
                End If
            Else
                idx += searchStr.Length - 1
            End If

            If idx <> -1 Then
                Dim endIdx As Integer = json.IndexOf("]", idx)
                If endIdx <> -1 Then
                    Dim arrayContent As String = json.Substring(idx + 1, endIdx - idx - 1)
                    For Each w As String In arrayContent.Split(","c)
                        Dim clean As String = w.Trim(""""c, " "c)
                        If clean.Length > 0 AndAlso Not list.Contains(clean) Then
                            list.Add(clean)
                        End If
                    Next
                End If
            End If
        Catch
        End Try
        Return list
    End Function

    Private Sub FetchOnlineSuggestionsAsync(ByVal prefix As String)
        Dim onlineResults As New List(Of String)()
        
        ' 1. Google Input Tools Transliteration/Spellcheck API
        Try
            Dim googleUrl As String = "https://inputtools.google.com/request?text=" & Uri.EscapeDataString(prefix) & "&itc=bn-t-i0-und&num=5&cp=1&cs=1&ie=utf-8&oe=utf-8&app=demopage"
            Dim request As HttpWebRequest = CType(WebRequest.Create(googleUrl), HttpWebRequest)
            request.Method = "GET"
            request.Timeout = 1500
            
            Using response As WebResponse = request.GetResponse()
                Using stream As Stream = response.GetResponseStream()
                    Using reader As New StreamReader(stream, Encoding.UTF8)
                        Dim json As String = reader.ReadToEnd()
                        Dim googleWords As List(Of String) = ParseGoogleSuggestions(prefix, json)
                        For Each w In googleWords
                            If Not onlineResults.Contains(w) Then onlineResults.Add(w)
                        Next
                    End Using
                End Using
            End Using
        Catch
        End Try

        ' 2. Gemini AI Context Spellchecking / Transliteration
        If Not String.IsNullOrEmpty(AppSettings.GeminiApiKey) Then
            Try
                Dim apiKey As String = AppSettings.GeminiApiKey
                Dim prompt As String = "Suggest spelling corrections or transliterations in Bengali script for the word/prefix: '" & prefix & "'. Respond ONLY with a comma-separated list of up to 4 words, with no other text."
                Dim url As String = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=" & apiKey
                Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
                request.Method = "POST"
                request.ContentType = "application/json"
                request.Timeout = 2000
                
                Dim cleanPrompt As String = prompt.Replace("\", "\\").Replace("""", "\""")
                Dim jsonBody As String = "{""contents"": [{""parts"": [{""text"": """ & cleanPrompt & """}]}]}"
                Dim bytes() As Byte = Encoding.UTF8.GetBytes(jsonBody)
                
                Using requestStream As Stream = request.GetRequestStream()
                    requestStream.Write(bytes, 0, bytes.Length)
                End Using
                
                Using response As WebResponse = request.GetResponse()
                    Using responseStream As Stream = response.GetResponseStream()
                        Using reader As New StreamReader(responseStream, Encoding.UTF8)
                            Dim jsonResponse As String = reader.ReadToEnd()
                            Dim rawText As String = ExtractTextFromGeminiJson(jsonResponse)
                            If Not String.IsNullOrEmpty(rawText) Then
                                For Each w In rawText.Split(","c)
                                    Dim clean As String = w.Trim(" "c, vbCr, vbLf, ControlChars.Tab, """"c, "'"c)
                                    If clean.Length > 0 AndAlso Not onlineResults.Contains(clean) Then
                                        onlineResults.Add(clean)
                                    End If
                                Next
                            End If
                        End Using
                    End Using
                End Using
            Catch
            End Try
        End If

        ' Save results to cache
        SyncLock onlineCache
            onlineCache(prefix) = onlineResults
        End SyncLock

        ' If UI thread is running, trigger refresh of the candidate window
        For Each f As Form In Application.OpenForms
            If TypeOf f Is MainUI Then
                f.BeginInvoke(Sub()
                                  Try
                                      Keyboard.MaybeShowSuggestions()
                                  Catch
                                  End Try
                              End Sub)
                Exit For
            End If
        Next
    End Sub

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
                rawText = rawText.Replace("\n", " ")
                rawText = rawText.Replace("\r", "")
                rawText = rawText.Replace("\t", " ")
                rawText = rawText.Replace("\""", """")
                rawText = rawText.Replace("\\", "\")
                Return rawText
            End If
        Catch
        End Try
        Return ""
    End Function

    Public Function GetLoadedWordCount() As Integer
        EnsureLoaded()
        Return freq.Count
    End Function

    Public Sub OpenAutoCorrectInNotepad()
        EnsureLoaded()
        Try
            System.Diagnostics.Process.Start("notepad.exe", AutoCorrectPath())
        Catch ex As Exception
            MessageBox.Show("Could not open auto-correct file: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Module
