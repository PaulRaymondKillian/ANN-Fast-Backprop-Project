Module GenerateNewPopulation
    'It will be assumed the parameter populations will be sorted

    Public Function GenerateNewPopulation1(ByVal Population() As ANN_Fast_Backprop_BatchMode_Object.Backprop, ByVal filePath As String, ByVal bestBackprop As ANN_Fast_Backprop_BatchMode_Object.Backprop) As ANN_Fast_Backprop_BatchMode_Object.Backprop()
        Dim PopSize As Integer = Population.Length
        Dim intervalSize As Integer = PopSize / 20
        Dim newPopulation(PopSize - 1) As ANN_Fast_Backprop_BatchMode_Object.Backprop
        Dim currentIndex As Integer = 0

        'Copy Best Backprop
        Dim best As ANN_Fast_Backprop_BatchMode_Object.Backprop = bestBackprop.Copy
        newPopulation(currentIndex) = best
        currentIndex += 1

        'Simulate Anneal 1/20 PopSize of Best Backprop
        For i As Integer = 1 To intervalSize
            Dim copy As ANN_Fast_Backprop_BatchMode_Object.Backprop = bestBackprop.Copy
            copy.InitiateSimulatedAnnealing()
            copy.ID = copy.ID & "-BestSA" & i
            newPopulation(currentIndex) = copy
            currentIndex += 1
        Next

        'Clone the best 2/20 PopSize
        For i As Integer = 1 To 2 * intervalSize
            newPopulation(currentIndex) = Population(currentIndex)
            currentIndex += 1
        Next

        'Simulate 1/20 PopSize cases of annealing for the top 2/20 PopSize backprop objects
        For i As Integer = 1 To 2 * intervalSize
            For j As Integer = 1 To intervalSize
                Dim copy As ANN_Fast_Backprop_BatchMode_Object.Backprop = Population(i - 1).Copy
                copy.InitiateSimulatedAnnealing()
                copy.ID = copy.ID & "-SA" & j
                newPopulation(currentIndex) = copy
                currentIndex += 1
            Next
        Next

        'Randomize Rest of the Population
        Do Until currentIndex > Population.Length - 1
            newPopulation(currentIndex) = New ANN_Fast_Backprop_BatchMode_Object.Backprop(filePath)
            newPopulation(currentIndex).ID = "Random" & currentIndex
            currentIndex += 1
        Loop

        Return newPopulation
    End Function

    Public Function GenerateNewPopulation2(ByVal Population() As ANN_Fast_Backprop_BatchMode_Object.Backprop, ByVal filePath As String, ByVal bestBackprop As ANN_Fast_Backprop_BatchMode_Object.Backprop) As ANN_Fast_Backprop_BatchMode_Object.Backprop()
        Dim newPopulation(Population.Length - 1) As ANN_Fast_Backprop_BatchMode_Object.Backprop
        Dim currentIndex As Integer = 0

        'Copy Best Backprop
        Dim best As ANN_Fast_Backprop_BatchMode_Object.Backprop = bestBackprop.Copy
        newPopulation(currentIndex) = best
        currentIndex += 1

        'Clone the best 20 Backprop Objects
        For i As Integer = 0 To 19
            newPopulation(currentIndex) = Population(currentIndex)
            currentIndex += 1
        Next

        'Simulate 3 cases of annealing for the top 20 backprop objects
        For i As Integer = 0 To 19
            For j As Integer = 0 To 2
                newPopulation(currentIndex) = Population(i).Copy
                newPopulation(currentIndex).InitiateSimulatedAnnealing()
                newPopulation(currentIndex).ID = newPopulation(currentIndex).ID & "-SA" & j
                currentIndex += 1
            Next
        Next

        'Randomize Rest of the Population
        Do Until currentIndex > Population.Length - 1
            newPopulation(currentIndex) = New ANN_Fast_Backprop_BatchMode_Object.Backprop(filePath)
            newPopulation(currentIndex).ID = "Random" & currentIndex
            currentIndex += 1
        Loop

        Return newPopulation
    End Function

    Public Function GenerateNewPopulation3(ByVal Population() As ANN_Fast_Backprop_BatchMode_Object.Backprop, ByVal filePath As String, ByVal bestBackprop As ANN_Fast_Backprop_BatchMode_Object.Backprop) As ANN_Fast_Backprop_BatchMode_Object.Backprop()
        Dim newPopulation(Population.Length - 1) As ANN_Fast_Backprop_BatchMode_Object.Backprop
        Dim currentIndex As Integer = 0

        'Copy Best Backprop
        Dim best As ANN_Fast_Backprop_BatchMode_Object.Backprop = bestBackprop.Copy
        newPopulation(currentIndex) = best
        currentIndex += 1

        'Clone the best 1 Backprop Objects
        For i As Integer = 0 To 0
            newPopulation(currentIndex) = Population(currentIndex)
            currentIndex += 1
        Next

        'Simulate 3 cases of annealing for the top 20 backprop objects
        For i As Integer = 0 To 0
            For j As Integer = 0 To 49
                newPopulation(currentIndex) = Population(i).Copy
                newPopulation(currentIndex).InitiateSimulatedAnnealing()
                newPopulation(currentIndex).ID = newPopulation(currentIndex).ID & "-SA" & j
                currentIndex += 1
            Next
        Next

        'Randomize Rest of the Population
        Do Until currentIndex > Population.Length - 1
            newPopulation(currentIndex) = New ANN_Fast_Backprop_BatchMode_Object.Backprop(filePath)
            newPopulation(currentIndex).ID = "Random" & currentIndex
            currentIndex += 1
        Loop

        Return newPopulation
    End Function

    Public Function GenerateNewPopulation4(ByVal Population() As ANN_Fast_Backprop_BatchMode_Object.Backprop, ByVal filePath As String, ByVal bestBackprop As ANN_Fast_Backprop_BatchMode_Object.Backprop) As ANN_Fast_Backprop_BatchMode_Object.Backprop()
        Dim newPopulation(Population.Length - 1) As ANN_Fast_Backprop_BatchMode_Object.Backprop
        Dim currentIndex As Integer = 0

        'Copy Best Backprop
        Dim best As ANN_Fast_Backprop_BatchMode_Object.Backprop = bestBackprop.Copy
        newPopulation(currentIndex) = best
        currentIndex += 1

        'Clone the best 1 Backprop Objects
        For i As Integer = 0 To 29
            newPopulation(currentIndex) = Population(currentIndex)
            currentIndex += 1
        Next

        'Simulate 3 cases of annealing for the top 20 backprop objects
        For i As Integer = 0 To 29
            For j As Integer = 0 To 1
                newPopulation(currentIndex) = Population(i).Copy
                newPopulation(currentIndex).InitiateSimulatedAnnealing()
                newPopulation(currentIndex).ID = newPopulation(currentIndex).ID & "-SA" & j
                currentIndex += 1
            Next
        Next

        'Randomize Rest of the Population
        Do Until currentIndex > Population.Length - 1
            newPopulation(currentIndex) = New ANN_Fast_Backprop_BatchMode_Object.Backprop(filePath)
            newPopulation(currentIndex).ID = "Random" & currentIndex
            currentIndex += 1
        Loop

        Return newPopulation
    End Function

    Public Function GenerateNewPopulation5(ByVal Population() As ANN_Fast_Backprop_BatchMode_Object.Backprop, ByVal filePath As String, ByVal bestBackprop As ANN_Fast_Backprop_BatchMode_Object.Backprop) As ANN_Fast_Backprop_BatchMode_Object.Backprop()
        Dim newPopulation(Population.Length - 1) As ANN_Fast_Backprop_BatchMode_Object.Backprop
        Dim currentIndex As Integer = 0

        'Copy Best Backprop
        Dim best As ANN_Fast_Backprop_BatchMode_Object.Backprop = bestBackprop.Copy
        newPopulation(currentIndex) = best
        currentIndex += 1

        Do Until currentIndex > Population.Length - 1
            newPopulation(currentIndex) = bestBackprop.Copy
            newPopulation(currentIndex).InitiateSimulatedAnnealing()
            newPopulation(currentIndex).ID = newPopulation(currentIndex).ID & "-SA" & currentIndex
            currentIndex += 1
        Loop

        Return newPopulation
    End Function

    Public Function GenerateNewPopulation6(ByVal Population() As ANN_Fast_Backprop_BatchMode_Object.Backprop, ByVal filePath As String, ByVal bestBackprop As ANN_Fast_Backprop_BatchMode_Object.Backprop) As ANN_Fast_Backprop_BatchMode_Object.Backprop()
        Dim newPopulation(Population.Length - 1) As ANN_Fast_Backprop_BatchMode_Object.Backprop
        Dim currentIndex As Integer = 0

        'Copy Best Backprop
        Dim best As ANN_Fast_Backprop_BatchMode_Object.Backprop = bestBackprop.Copy
        newPopulation(currentIndex) = best
        currentIndex += 1

        'Randomize Entire Population
        Do Until currentIndex > Population.Length - 1
            newPopulation(currentIndex) = New ANN_Fast_Backprop_BatchMode_Object.Backprop(filePath)
            newPopulation(currentIndex).ID = "Random" & currentIndex
            currentIndex += 1
        Loop

        Return newPopulation
    End Function

End Module
