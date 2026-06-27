Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Tests for the Unicode &lt;-&gt; legacy Bijoy/SutonnyMJ ANSI converter.
''' BijoyConverter is a pure, self-contained module so these are fully
''' deterministic and need no data files.
''' </summary>
<TestClass>
Public Class BijoyConverterTests

    <TestMethod>
    Public Sub UnicodeToBijoy_IndependentVowel()
        Assert.AreEqual("A", BijoyConverter.UnicodeToBijoy("অ"))
    End Sub

    <TestMethod>
    Public Sub UnicodeToBijoy_SingleConsonant()
        Assert.AreEqual("K", BijoyConverter.UnicodeToBijoy("ক"))
    End Sub

    <TestMethod>
    Public Sub UnicodeToBijoy_PreBaseVowelReordered()
        ' "আমি": আ -> Av ; ম + ি : the pre-base ি is stored before the consonant -> "wg"
        Assert.AreEqual("Avwg", BijoyConverter.UnicodeToBijoy("আমি"))
    End Sub

    <TestMethod>
    Public Sub UnicodeToBijoy_DigitsMapToAscii()
        Assert.AreEqual("123", BijoyConverter.UnicodeToBijoy("১২৩"))
    End Sub

    <TestMethod>
    Public Sub UnicodeToBijoy_DanriPunctuation()
        Assert.AreEqual("|", BijoyConverter.UnicodeToBijoy("।"))
    End Sub

    <TestMethod>
    Public Sub UnicodeToBijoy_EmptyAndNull()
        Assert.AreEqual("", BijoyConverter.UnicodeToBijoy(""))
        Assert.IsNull(BijoyConverter.UnicodeToBijoy(Nothing))
    End Sub

    <TestMethod>
    Public Sub BijoyToUnicode_SingleConsonant()
        Assert.AreEqual("ক", BijoyConverter.BijoyToUnicode("K"))
    End Sub

    <TestMethod>
    Public Sub BijoyToUnicode_IndependentVowel()
        Assert.AreEqual("আ", BijoyConverter.BijoyToUnicode("Av"))
    End Sub

    <TestMethod>
    Public Sub RoundTrip_SimpleConsonantStaysStable()
        ' A plain consonant string should survive a Unicode -> Bijoy -> Unicode round trip.
        Dim original As String = "কখগ"
        Dim ansi As String = BijoyConverter.UnicodeToBijoy(original)
        Dim back As String = BijoyConverter.BijoyToUnicode(ansi)
        Assert.AreEqual(original, back)
    End Sub

    <TestMethod>
    Public Sub AsciiPassesThroughUnchanged()
        Assert.AreEqual("Hello 123", BijoyConverter.UnicodeToBijoy("Hello 123"))
    End Sub

End Class
