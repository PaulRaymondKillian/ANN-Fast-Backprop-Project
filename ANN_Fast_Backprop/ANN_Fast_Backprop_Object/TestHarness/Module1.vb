Module Module1

    Sub Main()
        Dim populationSize As Integer = 100
        Dim Backprop(populationSize) As ANN_Fast_Backprop.Backprop
        For i As Integer = 0 To populationSize - 1
            Backprop(i) = New ANN_Fast_Backprop.Backprop("D:\Data Files\Fractions\Fractions.bprop")
            Console.WriteLine(i)
        Next

        Console.Write("Please Press Enter to Exit...")
        Dim z As String = Console.ReadLine
    End Sub

End Module
