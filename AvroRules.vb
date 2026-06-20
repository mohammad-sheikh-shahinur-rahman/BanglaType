'
'
'   This program Is free software; you can redistribute it And/Or modify
'   it under the terms Of the GNU General Public License As published by
'   the Free Software Foundation; either version 3 Of the License, Or
'   (at your option) any later version.
'
'   BanglaType Lite "Advance" additions: Avro Phonetic rule data.
'
'   This is a VB.NET port of the open OmicronLab Avro Phonetic dictionary.
'   Vowels resolve to their full form at a word boundary / after a vowel and to
'   their "kar" form after a consonant; consonant clusters map to conjuncts.
'   Covers the common-case set (~95%); rare conjunct/reph edge cases can be
'   extended by adding patterns here.
'

Module AvroRules

    ' --- compact builders ------------------------------------------------

    Private Function M(ByVal type As String, ByVal scope As String) As AvroMatch
        Return New AvroMatch With {.Type = type, .Scope = scope, .Value = ""}
    End Function

    Private Function M(ByVal type As String, ByVal scope As String, ByVal value As String) As AvroMatch
        Return New AvroMatch With {.Type = type, .Scope = scope, .Value = value}
    End Function

    Private Function R(ByVal replace As String, ParamArray matches() As AvroMatch) As AvroRule
        Return New AvroRule With {.Replace = replace, .Matches = New List(Of AvroMatch)(matches)}
    End Function

    Private Function Pat(ByVal find As String, ByVal replace As String) As AvroPattern
        Return New AvroPattern With {.Find = find, .Replace = replace, .Rules = Nothing}
    End Function

    Private Function Pat(ByVal find As String, ByVal replace As String, ParamArray rules() As AvroRule) As AvroPattern
        Return New AvroPattern With {.Find = find, .Replace = replace, .Rules = New List(Of AvroRule)(rules)}
    End Function

    ' Common reusable matches
    Private Function PrevCons() As AvroMatch
        Return M("prefix", "consonant")
    End Function
    Private Function PrevPunc() As AvroMatch
        Return M("prefix", "punctuation")
    End Function
    Private Function PrevVowel() As AvroMatch
        Return M("prefix", "vowel")
    End Function

    Public Function BuildPatterns() As List(Of AvroPattern)
        Dim p As New List(Of AvroPattern)()

        ' ---- digits ----
        p.Add(Pat("0", "০")) : p.Add(Pat("1", "১")) : p.Add(Pat("2", "২")) : p.Add(Pat("3", "৩"))
        p.Add(Pat("4", "৪")) : p.Add(Pat("5", "৫")) : p.Add(Pat("6", "৬")) : p.Add(Pat("7", "৭"))
        p.Add(Pat("8", "৮")) : p.Add(Pat("9", "৯"))

        ' ---- multi-char consonant clusters (longest first helps readability) ----
        p.Add(Pat("bhl", "ভ্ল"))
        p.Add(Pat("psh", "পশ"))
        p.Add(Pat("bdh", "ব্ধ"))
        p.Add(Pat("cch", "চ্ছ"))
        p.Add(Pat("cNG", "চ্ঞ"))
        p.Add(Pat("jjh", "জ্ঝ"))
        p.Add(Pat("jNG", "জ্ঞ"))
        p.Add(Pat("ddh", "দ্ধ"))
        p.Add(Pat("dgh", "দ্ঘ"))
        p.Add(Pat("dbh", "দ্ভ"))
        p.Add(Pat("Dgh", "ড্ঘ"))
        p.Add(Pat("tth", "ত্থ"))
        p.Add(Pat("ndh", "ন্ধ"))
        p.Add(Pat("nth", "ন্থ"))
        p.Add(Pat("nTh", "ন্ঠ"))
        p.Add(Pat("mbh", "ম্ভ"))
        p.Add(Pat("ssh", "শ্শ"))
        p.Add(Pat("sth", "স্থ"))
        p.Add(Pat("kkh", "ক্ষ"))
        p.Add(Pat("ksh", "ক্ষ"))
        p.Add(Pat("Sht", "ষ্ট"))
        p.Add(Pat("rri", "ঋ", R("ৃ", PrevCons())))
        p.Add(Pat("rRi", "ঋ", R("ৃ", PrevCons())))

        ' ---- k ----
        p.Add(Pat("kk", "ক্ক"))
        p.Add(Pat("kt", "ক্ত"))
        p.Add(Pat("kl", "ক্ল"))
        p.Add(Pat("km", "ক্ম"))
        p.Add(Pat("kh", "খ"))
        p.Add(Pat("k", "ক"))
        p.Add(Pat("q", "ক"))

        ' ---- g ----
        p.Add(Pat("gg", "গ্গ"))
        p.Add(Pat("gn", "গ্ন"))
        p.Add(Pat("gl", "গ্ল"))
        p.Add(Pat("gm", "গ্ম"))
        p.Add(Pat("gdh", "গ্ধ"))
        p.Add(Pat("gh", "ঘ"))
        p.Add(Pat("g", "গ"))

        ' ---- ng / nga ----
        p.Add(Pat("Ng", "ঙ"))
        p.Add(Pat("NG", "ঙ"))
        p.Add(Pat("ngo", "ঙ্গ"))
        p.Add(Pat("ng", "ং"))

        ' ---- c / ch ----
        p.Add(Pat("cc", "চ্চ"))
        p.Add(Pat("chh", "ছ"))
        p.Add(Pat("ch", "ছ"))
        p.Add(Pat("c", "চ"))

        ' ---- j ----
        p.Add(Pat("jj", "জ্জ"))
        p.Add(Pat("jh", "ঝ"))
        p.Add(Pat("j", "জ"))
        p.Add(Pat("J", "জ"))
        p.Add(Pat("z", "য"))
        p.Add(Pat("Z", "য"))

        ' ---- T / D (retroflex, capital) ----
        p.Add(Pat("TT", "ট্ট"))
        p.Add(Pat("Th", "ঠ"))
        p.Add(Pat("T", "ট"))
        p.Add(Pat("DD", "ড্ড"))
        p.Add(Pat("Dh", "ঢ"))
        p.Add(Pat("D", "ড"))

        ' ---- t / d (dental) ----
        p.Add(Pat("tt", "ত্ত"))
        p.Add(Pat("tm", "ত্ম"))
        p.Add(Pat("tn", "ত্ন"))
        p.Add(Pat("th", "থ"))
        p.Add(Pat("t", "ত"))
        p.Add(Pat("dd", "দ্দ"))
        p.Add(Pat("dg", "দ্গ"))
        p.Add(Pat("db", "দ্ব"))
        p.Add(Pat("dm", "দ্ম"))
        p.Add(Pat("dh", "ধ"))
        p.Add(Pat("d", "দ"))

        ' ---- n / N ----
        p.Add(Pat("nc", "ঞ্চ"))
        p.Add(Pat("nj", "ঞ্জ"))
        p.Add(Pat("nd", "ন্দ"))
        p.Add(Pat("nt", "ন্ত"))
        p.Add(Pat("nT", "ন্ট"))
        p.Add(Pat("nD", "ন্ড"))
        p.Add(Pat("nn", "ন্ন"))
        p.Add(Pat("nm", "ন্ম"))
        p.Add(Pat("ns", "ন্স"))
        p.Add(Pat("n", "ন"))
        p.Add(Pat("NN", "ণ্ণ"))
        p.Add(Pat("N", "ণ"))

        ' ---- p / f ----
        p.Add(Pat("pp", "প্প"))
        p.Add(Pat("pl", "প্ল"))
        p.Add(Pat("pt", "প্ত"))
        p.Add(Pat("ps", "প্স"))
        p.Add(Pat("ph", "ফ"))
        p.Add(Pat("p", "প"))
        p.Add(Pat("ff", "ফ্ফ"))
        p.Add(Pat("fl", "ফ্ল"))
        p.Add(Pat("f", "ফ"))

        ' ---- b / v ----
        p.Add(Pat("bb", "ব্ব"))
        p.Add(Pat("bd", "ব্দ"))
        p.Add(Pat("bj", "ব্জ"))
        p.Add(Pat("bl", "ব্ল"))
        p.Add(Pat("bh", "ভ"))
        p.Add(Pat("b", "ব"))
        p.Add(Pat("vv", "ভ্ভ"))
        p.Add(Pat("v", "ভ"))

        ' ---- m ----
        p.Add(Pat("mm", "ম্ম"))
        p.Add(Pat("mn", "ম্ন"))
        p.Add(Pat("ml", "ম্ল"))
        p.Add(Pat("mp", "ম্প"))
        p.Add(Pat("mb", "ম্ব"))
        p.Add(Pat("mf", "ম্ফ"))
        p.Add(Pat("m", "ম"))

        ' ---- s / S / sh ----
        p.Add(Pat("Sh", "ষ"))
        p.Add(Pat("sh", "শ"))
        p.Add(Pat("ss", "স্স"))
        p.Add(Pat("st", "স্ত"))
        p.Add(Pat("sT", "ষ্ট"))
        p.Add(Pat("sk", "স্ক"))
        p.Add(Pat("sp", "স্প"))
        p.Add(Pat("sf", "স্ফ"))
        p.Add(Pat("sm", "স্ম"))
        p.Add(Pat("sl", "স্ল"))
        p.Add(Pat("S", "শ"))
        p.Add(Pat("s", "স"))

        ' ---- h ----
        p.Add(Pat("hh", "হ্হ"))
        p.Add(Pat("h", "হ"))

        ' ---- l ----
        p.Add(Pat("ll", "ল্ল"))
        p.Add(Pat("l", "ল"))

        ' ---- r (reph / ro-fola handled via rule) ----
        p.Add(Pat("rr", "র্", R("র্", PrevCons())))
        p.Add(Pat("r", "র",
            R("্র", PrevCons(), M("prefix", "!exact", "r"), M("prefix", "!exact", "y"), M("prefix", "!exact", "w"))))
        p.Add(Pat("R", "র", R("্র", PrevCons())))

        ' ---- y / w / x (semi-vowels & specials) ----
        p.Add(Pat("y", "য়",
            R("্য", PrevCons()),
            R("ইয়", PrevPunc())))
        p.Add(Pat("w", "ও",
            R("্ব", PrevCons())))
        p.Add(Pat("x", "ক্স",
            R("এক্স", PrevPunc())))

        ' ---- chandrabindu / hasant ----
        p.Add(Pat("^", "ঁ"))
        p.Add(Pat(",,", "্"))

        ' ---- vowels (full form by default; kar after a consonant) ----
        p.Add(Pat("aa", "আ", R("া", PrevCons())))
        p.Add(Pat("a", "আ",
            R("া", PrevCons()),
            R("আ", PrevPunc())))
        p.Add(Pat("oo", "উ", R("ু", PrevCons())))
        p.Add(Pat("oi", "ঐ", R("ৈ", PrevCons())))
        p.Add(Pat("ou", "ঔ", R("ৌ", PrevCons())))
        p.Add(Pat("o", "ও",
            R("ো", PrevCons()),
            R("অ", PrevPunc())))
        p.Add(Pat("O", "ও", R("ো", PrevCons())))
        p.Add(Pat("ii", "ঈ", R("ী", PrevCons())))
        p.Add(Pat("i", "ই", R("ি", PrevCons())))
        p.Add(Pat("I", "ঈ", R("ী", PrevCons())))
        p.Add(Pat("uu", "ঊ", R("ূ", PrevCons())))
        p.Add(Pat("u", "উ", R("ু", PrevCons())))
        p.Add(Pat("U", "ঊ", R("ূ", PrevCons())))
        p.Add(Pat("ee", "ঈ", R("ী", PrevCons())))
        p.Add(Pat("e", "এ", R("ে", PrevCons())))
        p.Add(Pat("OI", "ঐ", R("ৈ", PrevCons())))
        p.Add(Pat("OU", "ঔ", R("ৌ", PrevCons())))

        ' ---- punctuation ----
        p.Add(Pat(".", "।"))

        Return p
    End Function

End Module
