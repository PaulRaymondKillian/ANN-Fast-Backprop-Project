Module Module1

    Sub Main()
        Dim Backprop As New ANN_Fast_Backprop_Dyn_Obj_Dec.Backprop("D:\Data Files\Fractions\Fractions.bprop2")

        For i As Integer = 0 To 5000
            Backprop.TrainEpoch(True, False)
            If i Mod 1000 = 0 Then
                Backprop.calcFitness()
                Console.WriteLine(Backprop.Fitness & " Rho: " & Backprop.rho)
            End If
        Next
        Backprop.outputNetworkOutput("C:\a.txt")


        Console.Write("Please Press Enter to Exit...")
        Dim z As String = Console.ReadLine
    End Sub

End Module
