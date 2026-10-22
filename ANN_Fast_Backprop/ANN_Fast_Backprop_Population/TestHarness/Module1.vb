Module Module1

    Sub Main()
        Dim startTime As New DateTime
        startTime = Now

        Dim Population As New ANN_Fast_Backprop_Population.Population("D:\Data Files\Fractions\Fractions.BPROP", 100)
        Population.RunGenerations(2, 400, 1, "D:\Data Files\Fractions", "Fractions_4xxx")
        outputTimeSpan(startTime)
        Console.Write("Please Press Enter to Exit...")
        Dim z As String = Console.ReadLine()
    End Sub

    Public Sub outputTimeSpan(ByVal startTime As DateTime)
        Dim endTime As New DateTime
        endTime = Now
        Dim timeSpan As New TimeSpan(endTime.Ticks - startTime.Ticks)
        Console.WriteLine("Time Elapsed: " & Convert.ToInt32(timeSpan.TotalHours) & " Hours " & Convert.ToInt32(timeSpan.TotalMinutes) & " Minutes " & Convert.ToInt32(timeSpan.TotalSeconds) & " Seconds")
    End Sub

End Module
