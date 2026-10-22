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
            Return RandomNumber(maxValue - 1) + ((RandomNumber(maxValue) / (1 + Math.Abs((RandomNumber(maxValue) * 100)))))
        Else    'Random Weight is between 1 and -1
            Dim tempRand1 As Double = RandomNumber(maxValue * 100)
            Dim tempRand2 As Double = RandomNumber(maxValue * 100)
            If AbsVal(tempRand1) > AbsVal(tempRand2) Then
                Return tempRand2 / tempRand1
            Else
                Return tempRand1 / (tempRand2 * tempRand2)
            End If
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
End Module
