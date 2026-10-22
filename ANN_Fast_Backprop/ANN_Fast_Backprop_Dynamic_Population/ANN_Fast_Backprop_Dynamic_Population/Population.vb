Imports System.IO

Public Class Population
    Private Const _CLASSNAME As String = "Population"
    Private Population() As ANN_Fast_Backprop_Dynamic_Object.Backprop
    Private PopulationSize As Integer
    Private BackpropInputFile As String
    Private BestBackprop As ANN_Fast_Backprop_Dynamic_Object.Backprop
    Private BestBackpropFitness() As Double
    Private AvgGenerationFitness() As Double
    Private Started As DateTime

    Dim ConsoleLibrary As New Console_Library.Console_Library   'Used to change text color in console

    Public Sub New(ByVal InputFile As String, ByVal PopSize As Integer)
        Dim _METHODNAME As String = "New(InputFile,PopSize)"
        If PopSize <= 0 Then
            Throw New Exception("Error - Class: Population, Method: New(), Desc: Invalid PopulationSize parameter, must be greater than zero")
            Exit Sub
        End If
        Started = Now
        BackpropInputFile = InputFile
        PopulationSize = PopSize
        ReDim Population(PopulationSize - 1)
        Try
            For i As Integer = 0 To PopulationSize - 1
                Population(i) = New ANN_Fast_Backprop_Dynamic_Object.Backprop(BackpropInputFile)
                Population(i).ID = i
            Next
            'Set BestBackprop object to its initial value which will be over-written later
            BestBackprop = Population(0).Copy
            BestBackprop.Fitness = Double.MaxValue  'Set to max value so it will always be overwritten
        Catch ex As Exception
            Throw New Exception("Error - Class: " & _CLASSNAME & " Method: " & _METHODNAME & " Descr: Error creating population; input file may be invalid or corrupt.")
        End Try
    End Sub

    Public Sub RunPopulation(ByVal Epochs As Integer)
        Dim previousFitness As Double
        Dim currentFitness As Double
        For i As Integer = 0 To PopulationSize - 1
            Population(i).calcFitness()
            previousFitness = Population(i).Fitness
            Population(i).Train(Epochs)
            Population(i).calcFitness()
            currentFitness = Population(i).Fitness
            If currentFitness - previousFitness < 0 Then
                ConsoleLibrary.SetColor(Console_Library.Console_Library.ConsoleColor.Green)
            ElseIf currentFitness - previousFitness > 0 Then
                ConsoleLibrary.SetColor(Console_Library.Console_Library.ConsoleColor.Red)
            Else
                ConsoleLibrary.SetColor(Console_Library.Console_Library.ConsoleColor.White)
            End If
            Console.WriteLine(Population(i).ID & ": " & currentFitness & "   PreviousFitness: " & previousFitness & "    Diff: " & currentFitness - previousFitness)
        Next
    End Sub

    Public Sub CalculatePopulationFitness()
        ConsoleLibrary.SetColor(Console_Library.Console_Library.ConsoleColor.White)
        Console.WriteLine("Calculating Population Fitnesses")
        For i As Integer = 0 To PopulationSize - 1
            Population(i).calcFitness()
            Console.WriteLine(Population(i).ID & ": " & Population(i).Fitness)
        Next
    End Sub

    Public Sub SortPopulation()
        Dim sortedPopulation(PopulationSize - 1) As ANN_Fast_Backprop_Dynamic_Object.Backprop
        Dim sortedArray(PopulationSize - 1) As Boolean  'Set to false by default
        Dim bestFitnessValue As Double
        Dim bestFitnessIndex As Integer = -1

        'Selection Sort Algorithm
        For i As Integer = 0 To PopulationSize - 1
            bestFitnessValue = Double.MaxValue
            For j As Integer = 0 To PopulationSize - 1
                If Population(j).Fitness < bestFitnessValue And Not sortedArray(j) Then
                    bestFitnessValue = Population(j).Fitness
                    bestFitnessIndex = j
                End If
            Next
            sortedPopulation(i) = Population(bestFitnessIndex)
            sortedArray(bestFitnessIndex) = True
        Next

        Population = sortedPopulation
    End Sub

    Public Sub NewPopulation(ByVal GenerationMethod As Integer)
        If GenerationMethod = 1 Then
            Population = GenerateNewPopulation1(Population, BackpropInputFile, BestBackprop)
        ElseIf GenerationMethod = 2 Then
            Population = GenerateNewPopulation2(Population, BackpropInputFile, BestBackprop)
        ElseIf GenerationMethod = 3 Then
            Population = GenerateNewPopulation3(Population, BackpropInputFile, BestBackprop)
        ElseIf GenerationMethod = 4 Then
            Population = GenerateNewPopulation4(Population, BackpropInputFile, BestBackprop)
        ElseIf GenerationMethod = 5 Then
            Population = GenerateNewPopulation5(Population, BackpropInputFile, BestBackprop)
        ElseIf GenerationMethod = 6 Then
            Population = GenerateNewPopulation6(Population, BackpropInputFile, BestBackprop)
        End If

    End Sub

    Public Sub RunGeneration(ByVal EpochsToRun As Integer, ByVal NewGenerationMethod As Integer, ByVal GenerateNewPopulation As Boolean)
        RunPopulation(EpochsToRun)
        SortPopulation()
        RetainBestBackprop()
        If GenerateNewPopulation Then
            NewPopulation(NewGenerationMethod)
        End If
    End Sub

    Public Sub RunGenerations(ByVal Generations As Integer, ByVal EpochsToRun As Integer, ByVal NewGenerationMethod As Integer, ByVal SaveDirectory As String, ByVal FilePrefix As String)

        ReDim BestBackpropFitness(Generations - 1)
        ReDim AvgGenerationFitness(Generations)
        AvgGenerationFitness(0) = AveragePopulationFitness()
        For i As Integer = 0 To Generations - 1
            ConsoleLibrary.SetColor(Console_Library.Console_Library.ConsoleColor.Yellow)
            Console.WriteLine("Generation: " & i & "    Rho: " & Population(0).rho & "  BestBackprop: " & BestBackprop.Fitness)
            If i < Generations - 1 Then
                RunGeneration(EpochsToRun, NewGenerationMethod, True)
            Else
                RunGeneration(EpochsToRun, NewGenerationMethod, False)
            End If
            BestBackpropFitness(i) = BestBackprop.Fitness
            AvgGenerationFitness(i + 1) = AveragePopulationFitness()
        Next

        ConsoleLibrary.SetColor(Console_Library.Console_Library.ConsoleColor.White)

        BestBackprop.calcFitness()
        Console.WriteLine("Saving data to: " & SaveDirectory & "\" & FilePrefix & "\")
        Try
            Directory.CreateDirectory(SaveDirectory & "\" & FilePrefix)
            OutputPopulationData(SaveDirectory & "\" & FilePrefix & "\ProcessOutput.txt")
            BestBackprop.outputNetwork(SaveDirectory & "\" & FilePrefix & "\" & FilePrefix & "_Network.ANN")
            BestBackprop.outputNetworkOutput(SaveDirectory & "\" & FilePrefix & "\" & FilePrefix & "_Output.txt")
            WriteBPROPFile(SaveDirectory & "\" & FilePrefix & "\" & FilePrefix & ".BPROP", BestBackprop)
            WritePOPFile(SaveDirectory & "\" & FilePrefix & "\" & FilePrefix & ".POP", Generations, EpochsToRun, NewGenerationMethod, SaveDirectory, FilePrefix)
        Catch ex As Exception

        End Try
        Console.WriteLine("Data Saved Successfully!")
    End Sub

    Public Sub RetainBestBackprop()
        Population(0).calcFitness()
        BestBackprop.calcFitness()
        If Population(0).Fitness < BestBackprop.Fitness Or Double.IsNaN(BestBackprop.Fitness) Then
            BestBackprop = Population(0).Copy
            BestBackprop.ID = BestBackprop.ID & "-Best"
            BestBackprop.calcFitness()
        End If
    End Sub

    Public Function AveragePopulationFitness() As Double
        Dim totalFitness As Double = 0
        For i As Integer = 0 To PopulationSize - 1
            Population(i).calcFitness()
            totalFitness += Population(i).Fitness
        Next
        Return (totalFitness / PopulationSize)
    End Function

    Public Sub WriteBPROPFile(ByVal filePath As String, ByVal backprop As ANN_Fast_Backprop_Dynamic_Object.Backprop)
        Dim sw As New StreamWriter(filePath)
        sw.WriteLine("Dataset file location=" & backprop.DataFileName)
        sw.WriteLine("Hidden Neurons=" & backprop.countHiddenNeurons)
        sw.WriteLine("Hidden Layers=" & backprop.countHiddenLayers)
        sw.WriteLine("Max Weight Value=" & backprop.maxWeightValue)
        sw.WriteLine("Max Bias Value=" & backprop.maxBiasValue)
        sw.WriteLine("Epochs To Run=" & backprop.epochs)
        sw.WriteLine("Output Data Interval=" & backprop.outputInterval)
        sw.WriteLine("Output Delta Data Interval=" & backprop.outputDeltaInterval)
        sw.WriteLine("Initial Rho Value=" & backprop.rho)
        sw.WriteLine("Rho Multiplier=" & backprop.rhoMultiplier)
        sw.WriteLine("Dynamic Rho Interval=" & backprop.rhoInterval)
        sw.WriteLine("Momentum Scaler=" & backprop.momentumScaler)
        sw.WriteLine("Simulated Annealing Interval=" & backprop.annealingInterval)
        sw.WriteLine("Simulated Annleaing Max Percent=" & backprop.annealingMaxPercent)
        sw.WriteLine("Save Interval=" & backprop.SaveInterval)
        sw.WriteLine("Save Directory=" & backprop.SaveDirectory)
        sw.WriteLine("Save File Prefix=" & backprop.SaveNameSuffix)

        sw.Close()
    End Sub

    Public Sub WritePOPFile(ByVal filePath As String, ByVal Generations As Integer, ByVal EpochsToRun As Integer, ByVal NewGenMethod As Integer, ByVal SaveDirectory As String, ByVal FilePrefix As String)
        Dim sw As New StreamWriter(filePath)
        sw.WriteLine("BPROP File Location=" & BackpropInputFile)
        sw.WriteLine("Population Size=" & PopulationSize)
        sw.WriteLine("Generations=" & Generations)
        sw.WriteLine("Epochs Per Generations=" & EpochsToRun)
        sw.WriteLine("New Generation Method=" & NewGenMethod)
        sw.WriteLine("Save Directory=" & SaveDirectory)
        sw.WriteLine("File Prefix=" & FilePrefix)
        sw.Close()
    End Sub

    Public Sub OutputPopulationData(ByVal FilePath As String)
        Dim Finished As DateTime
        Finished = Now
        Dim TimeElapsed As TimeSpan
        TimeElapsed = Finished - Started

        Dim sw As New StreamWriter(FilePath)
        sw.WriteLine("ANN_Fast_Backprop_Dynamic_Population")
        sw.WriteLine()
        Console.WriteLine("BestBackprop ID: " & BestBackprop.ID & "   BestBackprop Fitness:" & BestBackprop.Fitness)
        sw.WriteLine("BestBackprop ID: " & BestBackprop.ID & "   BestBackprop Fitness:" & BestBackprop.Fitness)
        sw.WriteLine()
        Console.WriteLine("Initial Population Avg Fitness: " & AvgGenerationFitness(0))
        sw.WriteLine("Initial Population Avg Fitness: " & AvgGenerationFitness(0))
        For i As Integer = 0 To BestBackpropFitness.Length - 1
            Console.WriteLine("Best Backprop Fitness Generation " & i & ": " & BestBackpropFitness(i) & "   AvgFitness: " & AvgGenerationFitness(i + 1))
            sw.WriteLine("Best Backprop Fitness Generation " & i & ": " & BestBackpropFitness(i) & "   AvgFitness: " & AvgGenerationFitness(i + 1))
        Next
        sw.WriteLine()
        Console.WriteLine("Processed In: " & Convert.ToInt32(TimeElapsed.Hours) & " Hours, " & Convert.ToInt32(TimeElapsed.Minutes) & " Minutes, " & Convert.ToInt32(TimeElapsed.Seconds) & " Seconds.")
        sw.WriteLine("Processed In: " & Convert.ToInt32(TimeElapsed.Hours) & " Hours, " & Convert.ToInt32(TimeElapsed.Minutes) & " Minutes, " & Convert.ToInt32(TimeElapsed.Seconds) & " Seconds.")

        sw.Close()
    End Sub

End Class
