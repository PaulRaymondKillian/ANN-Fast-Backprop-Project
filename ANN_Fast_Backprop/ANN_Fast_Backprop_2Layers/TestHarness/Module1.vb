Module Module1

    Sub Main()
        Dim BatchBackprop As New ANN_Fast_Backprop_2Layers.Backprop("D:\Data Files\Fractions\Fractions.bprop")

        For i As Integer = 0 To 1000000
            BatchBackprop.TrainEpoch(True, False)
            If i Mod 1000 = 0 Then
                BatchBackprop.calcFitness()
                Console.WriteLine(BatchBackprop.Fitness & " Rho: " & BatchBackprop.rho)
            End If
        Next

        Console.Write("Please Press Enter to Exit...")
        Dim z As String = Console.ReadLine
    End Sub

End Module
