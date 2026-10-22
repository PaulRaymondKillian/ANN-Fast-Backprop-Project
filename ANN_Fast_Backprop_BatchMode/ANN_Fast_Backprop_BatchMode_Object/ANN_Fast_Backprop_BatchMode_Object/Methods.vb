Imports System.IO

Module Methods
    Const e As Double = Math.E
    Dim r As New Random(System.DateTime.Now.Millisecond)

    Public Function Activation(ByVal value As Double) As Double
        Return 1 / (1 + Math.Pow(e, -value))
    End Function

    Public Function fPrime(ByVal u As Double) As Double
        Return u * (1 - u)
    End Function

    Public Function RandomWeight(ByVal maxValue As Double) As Double
        If maxValue > 1 Or maxValue < -1 Then
            Return RandomNumber(maxValue - 1) + (RandomNumber(1000000) / 1000000)
        Else    'Random Weight is between 1 and -1

            Return (RandomNumber(1000000) / 1000000) * maxValue
        End If

    End Function

    Public Function RandomNumber(ByVal MaxNumber As Integer) As Integer
        Return r.Next(-MaxNumber, MaxNumber)
    End Function

    Public Function AbsVal(ByVal value As Double) As Double
        If value < 0 Then
            Return -value
        Else
            Return value
        End If
    End Function

    Public Function validInputFile(ByVal filePath As String) As Boolean
        'For now just return true
        Return True
    End Function

    Public Function numRowsInTextFile(ByVal filePath As String) As Integer
        Dim sr As StreamReader = File.OpenText(filePath)
        Dim rows As Integer = 0
        Dim line As String

        Do
            line = sr.ReadLine
            rows += 1
        Loop Until sr.EndOfStream

        Return rows

    End Function
End Module
