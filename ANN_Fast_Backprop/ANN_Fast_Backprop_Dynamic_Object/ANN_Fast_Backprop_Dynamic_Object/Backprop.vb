Imports System.IO

Public Class Backprop

    Public ID As String 'Optional String To Use to Identifier the specific object

    Const VALUE As Integer = 0
    Const BIAS As Integer = 1
    Const DELTA As Integer = 2

    Public DataFileName As String

    'Network Structure Variables
    Dim countInputNeurons As Integer
    Public countHiddenNeurons As Integer
    Public countHiddenLayers As Integer
    Dim countOutputNeurons As Integer
    Public epochs As Integer

    Public outputInterval As Integer
    Public outputDeltaInterval As Integer

    'Create Exemplar Set
    Dim exemplarCount As Integer
    Dim exemplarInputs(,) As Double
    Dim exemplarOutputs(,) As Double

    Dim randomizedIndex() As Integer

    'Dynamic Rho Variables
    Public rho As Double
    Public rhoInterval As Integer
    Public rhoMultiplier As Double

    'Neuron arrays
    Dim inputValues() As Double
    Public hiddenValues(,,) As Double
    Public outputValues(,) As Double

    'Weight arrays
    Public ihWeights(,,) As Double 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Public hhWeights(,,,) As Double 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Public hoWeights(,,) As Double 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Public maxWeightValue As Double
    Public maxBiasValue As Double

    'Momentum Related Terms
    Public momentumScaler As Double  'Suggested by notes 0.9 to 0.5
    Const CURRENT As Integer = 0
    Const PREVIOUS As Integer = 1

    'Simulated Annealing Terms
    Public annealingInterval As Integer
    Public annealingMaxPercent As Integer

    'Interval Value When Program Should Ask If You Want to Save (Used for learning tracking)
    Public SaveInterval As Integer
    Public SaveDirectory As String
    Public SaveNameSuffix As String

    'Fitness Variable to determine accuracy/quality of backprop network
    Public Fitness As Double = 0


    Public Sub New(ByVal fileName As String)
        DataFileName = fileName
        importBackpropFromFile(DataFileName)

        'Initialize Arrays
        ReDim inputValues(countInputNeurons - 1)
        ReDim hiddenValues(countHiddenNeurons - 1, countHiddenLayers - 1, 2)
        ReDim outputValues(countOutputNeurons - 1, 2)
        ReDim ihWeights(countInputNeurons - 1, countHiddenNeurons - 1, 1)
        If countHiddenLayers > 1 Then
            ReDim hhWeights(countHiddenNeurons - 1, countHiddenNeurons - 1, countHiddenLayers - 2, 1)
        End If
        ReDim hoWeights(countHiddenNeurons - 1, countOutputNeurons - 1, 1)

        'Randomize Weight Arrays
        For i As Integer = 0 To countInputNeurons - 1
            For j As Integer = 0 To countHiddenNeurons - 1
                ihWeights(i, j, CURRENT) = RandomWeight(maxWeightValue)
                ihWeights(i, j, PREVIOUS) = ihWeights(i, j, CURRENT)    'Setting Previous Weight so first calculation is 0
            Next
        Next

        For layer As Integer = 0 To countHiddenLayers - 2
            For i As Integer = 0 To countHiddenNeurons - 1
                For j As Integer = 0 To countHiddenNeurons - 1
                    hhWeights(i, j, layer, CURRENT) = RandomWeight(maxWeightValue)
                    hhWeights(i, j, layer, PREVIOUS) = hhWeights(i, j, layer, CURRENT)  'Setting Previous Weight so first calculation is 0
                Next
            Next
        Next

        For i As Integer = 0 To countHiddenNeurons - 1
            For j As Integer = 0 To countOutputNeurons - 1
                hoWeights(i, j, CURRENT) = RandomWeight(maxWeightValue)
                hoWeights(i, j, PREVIOUS) = hoWeights(i, j, CURRENT)    'Setting Previous Weight so first calculation is 0
            Next
        Next

        'Randomize Bias Values
        For layer As Integer = 0 To countHiddenLayers - 1
            For i As Integer = 0 To countHiddenNeurons - 1
                hiddenValues(i, layer, BIAS) = RandomWeight(maxBiasValue)
            Next
        Next

        For i As Integer = 0 To countOutputNeurons - 1
            outputValues(i, BIAS) = RandomWeight(maxBiasValue)
        Next
    End Sub

    Public Sub importDataSet(ByVal filePath As String)
        Const _MethodName As String = "importDataSet(filePath)"
        Dim line As String
        Dim inputs() As String
        Dim sInputValues() As String
        Dim sOutputValues() As String
        exemplarCount = numRowsInTextFile(filePath)

        Dim sr As StreamReader = File.OpenText(filePath)
        line = sr.ReadLine

        inputs = line.Split("="c)

        If inputs.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If

        sInputValues = inputs(0).Split(","c)
        sOutputValues = inputs(1).Split(","c)

        countInputNeurons = Convert.ToInt32(sInputValues.Count)
        ReDim exemplarInputs(exemplarCount - 1, countInputNeurons - 1)
        countOutputNeurons = Convert.ToInt32(sOutputValues.Count)
        ReDim exemplarOutputs(exemplarCount - 1, countOutputNeurons - 1)

        For i As Integer = 0 To exemplarCount - 1

            For k As Integer = 0 To countInputNeurons - 1
                exemplarInputs(i, k) = Convert.ToDouble(sInputValues(k))
            Next

            For k As Integer = 0 To countOutputNeurons - 1
                exemplarOutputs(i, k) = Convert.ToDouble(sOutputValues(k))
            Next

            line = sr.ReadLine

            If Not line = Nothing Then
                inputs = line.Split("="c)

                If inputs.Count <> 2 Then
                    Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Data File invalid or corrupt")

                    Exit Sub
                End If

                sInputValues = inputs(0).Split(","c)
                sOutputValues = inputs(1).Split(","c)
            End If

        Next

        sr.Close()

    End Sub

    Public Sub importBackpropFromFile(ByVal filePath As String)
        Const _MethodName As String = "importDataSet(filePath)"
        Dim line As String
        Dim readLine() As String

        Dim sr As StreamReader = File.OpenText(filePath)

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        importDataSet(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        countHiddenNeurons = Convert.ToInt32(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        countHiddenLayers = Convert.ToInt32(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        maxWeightValue = Convert.ToDouble(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        maxBiasValue = Convert.ToDouble(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        epochs = Convert.ToInt32(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        outputInterval = Convert.ToInt32(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        outputDeltaInterval = Convert.ToInt32(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        rho = Convert.ToDouble(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        rhoMultiplier = Convert.ToDouble(readLine(1))


        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        rhoInterval = Convert.ToInt32(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        momentumScaler = Convert.ToDouble(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        annealingInterval = Convert.ToInt32(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        annealingMaxPercent = Convert.ToInt32(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        SaveInterval = Convert.ToInt32(readLine(1))

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        SaveDirectory = readLine(1)

        line = sr.ReadLine
        readLine = line.Split("="c)
        If readLine.Count <> 2 Then
            Throw New Exception("Error - Method: " & _MethodName & _
                                        "Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
            Exit Sub
        End If
        SaveNameSuffix = readLine(1)

        sr.Close()
    End Sub

    Public Sub outputNetwork(ByVal fileName As String)
        Dim sw As New StreamWriter(fileName)
        sw.WriteLine(countInputNeurons)
        sw.WriteLine(countHiddenNeurons)
        sw.WriteLine(countHiddenLayers)
        sw.WriteLine(countOutputNeurons)
        sw.WriteLine(((countInputNeurons + 1) * countHiddenNeurons) + ((countHiddenLayers - 1) * (countHiddenNeurons + 1) * countHiddenNeurons) + ((countHiddenNeurons + 1) * countOutputNeurons))

        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            For inputNeuron As Integer = 0 To countInputNeurons - 1
                sw.WriteLine(ihWeights(inputNeuron, hiddenNeuron, CURRENT))
            Next
            sw.WriteLine(hiddenValues(hiddenNeuron, 0, BIAS))
        Next

        If countHiddenLayers > 1 Then
            For layer As Integer = 0 To countHiddenLayers - 2
                For HiddenAbove As Integer = 0 To countHiddenNeurons - 1
                    For HiddenBelow As Integer = 0 To countHiddenNeurons - 1
                        sw.WriteLine(hhWeights(HiddenBelow, HiddenAbove, layer, CURRENT))
                    Next
                    sw.WriteLine(hiddenValues(HiddenAbove, layer + 1, BIAS))
                Next
            Next
        End If

        For outputNeuron As Integer = 0 To countOutputNeurons - 1
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                sw.WriteLine(hoWeights(hiddenNeuron, outputNeuron, CURRENT))
            Next
            sw.WriteLine(outputValues(outputNeuron, BIAS))
        Next

        sw.Close()

    End Sub

    Public Sub outputNetworkOutput(ByVal fileName As String)
        Dim sw As New StreamWriter(fileName)

        sw.WriteLine("Fitness: " & Fitness & "  Rho: " & rho)

        For i As Integer = 0 To exemplarCount - 1
            For j As Integer = 0 To countInputNeurons - 1
                sw.Write(exemplarInputs(i, j))
                If j > 0 Then
                    sw.Write(", ")
                End If
            Next

            run(i)
            sw.Write(" = ")
            For j As Integer = 0 To countOutputNeurons - 1
                sw.Write(outputValues(j, VALUE))
                If j > 0 Then
                    sw.Write(", ")
                End If
            Next
            sw.WriteLine()
        Next

        sw.Close()

    End Sub

    Public Sub run(ByVal exemplarIndex As Integer)
        For i As Integer = 0 To countInputNeurons - 1
            inputValues(i) = exemplarInputs(exemplarIndex, i)
        Next

        'Calculate First Layer of Hidden Neuron Values
        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            hiddenValues(hiddenNeuron, 0, VALUE) = 0   'Reset to 0
            For inputNeuron As Integer = 0 To countInputNeurons - 1
                hiddenValues(hiddenNeuron, 0, VALUE) += inputValues(inputNeuron) * ihWeights(inputNeuron, hiddenNeuron, CURRENT)
            Next
            hiddenValues(hiddenNeuron, 0, VALUE) = Activation(hiddenValues(hiddenNeuron, 0, VALUE) + hiddenValues(hiddenNeuron, 0, BIAS))
        Next

        'Calculate Hidden Layer Neuron Values
        If countHiddenLayers > 1 Then
            For layer As Integer = 1 To countHiddenLayers - 1
                For aboveHidden As Integer = 0 To countHiddenNeurons - 1
                    hiddenValues(aboveHidden, layer, VALUE) = 0   'Reset to 0
                    For belowHidden As Integer = 0 To countHiddenNeurons - 1
                        hiddenValues(aboveHidden, layer, VALUE) += hiddenValues(belowHidden, layer - 1, VALUE) * hhWeights(belowHidden, aboveHidden, layer - 1, CURRENT)
                    Next
                    hiddenValues(aboveHidden, layer, VALUE) = Activation(hiddenValues(aboveHidden, layer, VALUE) + hiddenValues(aboveHidden, layer, BIAS))
                Next
            Next
        End If

        'Calculate Output Neuron Values
        For outputNeuron As Integer = 0 To countOutputNeurons - 1
            outputValues(outputNeuron, VALUE) = 0    'Reset to 0
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                outputValues(outputNeuron, VALUE) += hiddenValues(hiddenNeuron, countHiddenLayers - 1, VALUE) * hoWeights(hiddenNeuron, outputNeuron, CURRENT)
            Next
            outputValues(outputNeuron, VALUE) += outputValues(outputNeuron, BIAS)
        Next
    End Sub

    Public Sub Backprop(ByVal iteration As Integer)
        'Calculate Output Neuron Deltas
        For i As Integer = 0 To countOutputNeurons - 1
            outputValues(i, DELTA) = -2 * (exemplarOutputs(iteration, i) - outputValues(i, VALUE))
        Next

        'Calculate Top Layer Hidden Neuron Deltas
        Dim totalDeltas As Double = 0
        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            hiddenValues(hiddenNeuron, countHiddenLayers - 1, DELTA) = 0   'Reset Value
            For outputNeuron As Integer = 0 To countOutputNeurons - 1
                hiddenValues(hiddenNeuron, countHiddenLayers - 1, DELTA) += fPrime(hiddenValues(hiddenNeuron, countHiddenLayers - 1, VALUE)) * outputValues(outputNeuron, DELTA) * hoWeights(hiddenNeuron, outputNeuron, CURRENT)
                totalDeltas += hiddenValues(hiddenNeuron, countHiddenLayers - 1, DELTA) 'Keep running total of delta values for analysis
            Next
        Next

        'Calculate Hidden Layer Neuron Deltas
        If countHiddenLayers > 1 Then
            For layer As Integer = countHiddenLayers - 2 To 0 Step -1
                For belowHidden As Integer = 0 To countHiddenNeurons - 1
                    hiddenValues(belowHidden, layer, DELTA) = 0 'Reset Value
                    For aboveHidden As Integer = 0 To countHiddenNeurons - 1
                        hiddenValues(belowHidden, layer, DELTA) += fPrime(hiddenValues(belowHidden, layer, VALUE)) * hiddenValues(aboveHidden, layer + 1, DELTA) * hhWeights(belowHidden, aboveHidden, layer, CURRENT)
                        totalDeltas += hiddenValues(belowHidden, layer, DELTA)  'Keep running total of delta values for analysis
                    Next
                Next
            Next
        End If

        'Calculate Hidden-Output Weights
        ''Utilizing Momemtum
        Dim previousWeightChange As Double
        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            For outputNeuron As Integer = 0 To countOutputNeurons - 1
                previousWeightChange = hoWeights(hiddenNeuron, outputNeuron, CURRENT) - hoWeights(hiddenNeuron, outputNeuron, PREVIOUS)    'Store previous weight change
                hoWeights(hiddenNeuron, outputNeuron, PREVIOUS) = hoWeights(hiddenNeuron, outputNeuron, CURRENT)    'Store current weight to be used as the next iteration's previous weight
                hoWeights(hiddenNeuron, outputNeuron, CURRENT) = hoWeights(hiddenNeuron, outputNeuron, CURRENT) - rho * outputValues(outputNeuron, DELTA) * hiddenValues(hiddenNeuron, countHiddenLayers - 1, VALUE) + momentumScaler * previousWeightChange
            Next
        Next

        'Calculate Hidden-Hidden Weights
        'Utilizing Momentum
        If countHiddenLayers > 1 Then
            For layer As Integer = countHiddenLayers - 2 To 0 Step -1
                For belowHidden As Integer = 0 To countHiddenNeurons - 1
                    For aboveHidden As Integer = 0 To countHiddenNeurons - 1
                        previousWeightChange = hhWeights(belowHidden, aboveHidden, layer, CURRENT) - hhWeights(belowHidden, aboveHidden, layer, PREVIOUS) 'Store previous weight change
                        hhWeights(belowHidden, aboveHidden, layer, PREVIOUS) = hhWeights(belowHidden, aboveHidden, layer, CURRENT)  'Store current weight to be used as next iteration's previous weight
                        hhWeights(belowHidden, aboveHidden, layer, CURRENT) = hhWeights(belowHidden, aboveHidden, layer, CURRENT) - rho * hiddenValues(aboveHidden, layer + 1, DELTA) * hiddenValues(belowHidden, layer, VALUE) + momentumScaler * previousWeightChange
                    Next
                Next
            Next
        End If

        'Calculate Input-Hidden Weights
        ''Utilizing Momentum
        For inputNeuron As Integer = 0 To countInputNeurons - 1
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                previousWeightChange = ihWeights(inputNeuron, hiddenNeuron, CURRENT) - ihWeights(inputNeuron, hiddenNeuron, PREVIOUS)
                ihWeights(inputNeuron, hiddenNeuron, PREVIOUS) = ihWeights(inputNeuron, hiddenNeuron, CURRENT)
                ihWeights(inputNeuron, hiddenNeuron, CURRENT) = ihWeights(inputNeuron, hiddenNeuron, CURRENT) - rho * hiddenValues(hiddenNeuron, 0, DELTA) * inputValues(inputNeuron) + momentumScaler * previousWeightChange
            Next
        Next
    End Sub

    Public Sub TrainEpoch(ByVal UseRhoMultiplier As Boolean, ByVal SimulatedAnnealing As Boolean)
        Fitness = 0 'Reset Fitness Value
        randomizedIndex = randomizeTheIndex(exemplarCount)

        For iteration As Integer = 0 To exemplarCount - 1
            run(randomizedIndex(iteration))
            Backprop(randomizedIndex(iteration))
        Next

        'Simulated Annealing
        ''Adjust each weight in the weights layers to be in a location 'near' its current location (simulating shaking the solution plane)
        If SimulatedAnnealing Then
            InitiateSimulatedAnnealing()
        End If

        'Dynamic Rho Value
        If UseRhoMultiplier Then
            rho *= rhoMultiplier
        End If

    End Sub

    Public Sub Train(ByVal Epochs As Integer)
        For i As Integer = 1 To Epochs
            If (i Mod rhoInterval) = 0 Then
                TrainEpoch(True, False)
            Else
                TrainEpoch(False, False)
            End If
        Next
    End Sub

    Public Sub InitiateSimulatedAnnealing()

        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            For outputNeuron As Integer = 0 To countOutputNeurons - 1
                hoWeights(hiddenNeuron, outputNeuron, CURRENT) = hoWeights(hiddenNeuron, outputNeuron, CURRENT) + ((RandomNumber(annealingMaxPercent) / 100) * hoWeights(hiddenNeuron, outputNeuron, CURRENT))
            Next
        Next

        If countHiddenLayers > 1 Then
            For layer As Integer = countHiddenLayers - 2 To 0 Step -1
                For belowHidden As Integer = 0 To countHiddenNeurons - 1
                    For aboveHidden As Integer = 0 To countHiddenNeurons - 1
                        hhWeights(belowHidden, aboveHidden, layer, CURRENT) = hhWeights(belowHidden, aboveHidden, layer, CURRENT) + ((RandomNumber(annealingMaxPercent) / 100) * hhWeights(belowHidden, aboveHidden, layer, CURRENT))
                    Next
                Next
            Next
        End If

        For inputNeuron As Integer = 0 To countInputNeurons - 1
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                ihWeights(inputNeuron, hiddenNeuron, CURRENT) = ihWeights(inputNeuron, hiddenNeuron, CURRENT) + ((RandomNumber(annealingMaxPercent) / 100) * ihWeights(inputNeuron, hiddenNeuron, CURRENT))
            Next
        Next

    End Sub

    Public Sub calcFitness()
        Fitness = 0
        For iteration As Integer = 0 To exemplarCount - 1
            run(iteration)
            For i As Integer = 0 To countOutputNeurons - 1
                Fitness += AbsVal(exemplarOutputs(iteration, i) - outputValues(i, VALUE))  'Error
            Next
        Next
    End Sub

    Public Function Copy() As Backprop
        Dim ACopy As New Backprop(DataFileName)

        ACopy.ID = ID
        ACopy.rho = rho
        ACopy.rhoInterval = rhoInterval
        ACopy.rhoMultiplier = rhoMultiplier

        For i As Integer = 0 To countInputNeurons - 1
            For j As Integer = 0 To countHiddenNeurons - 1
                ACopy.ihWeights(i, j, CURRENT) = ihWeights(i, j, CURRENT)
                ACopy.ihWeights(i, j, PREVIOUS) = ihWeights(i, j, PREVIOUS)
            Next
        Next

        If countHiddenLayers > 1 Then
            For layer As Integer = 0 To countHiddenLayers - 2
                For i As Integer = 0 To countHiddenNeurons - 1
                    For j As Integer = 0 To countHiddenNeurons - 1
                        ACopy.hhWeights(i, j, layer, CURRENT) = hhWeights(i, j, layer, CURRENT)
                        ACopy.hhWeights(i, j, layer, PREVIOUS) = hhWeights(i, j, layer, PREVIOUS)
                    Next
                Next
            Next
        End If

        For i As Integer = 0 To countHiddenNeurons - 1
            For j As Integer = 0 To countOutputNeurons - 1
                ACopy.hoWeights(i, j, CURRENT) = hoWeights(i, j, CURRENT)
                ACopy.hoWeights(i, j, PREVIOUS) = hoWeights(i, j, PREVIOUS)
            Next
        Next

        For layer As Integer = 0 To countHiddenLayers - 1
            For i As Integer = 0 To countHiddenNeurons - 1
                ACopy.hiddenValues(i, layer, BIAS) = hiddenValues(i, layer, BIAS)
            Next
        Next

        For i As Integer = 0 To countOutputNeurons - 1
            ACopy.outputValues(i, BIAS) = outputValues(i, BIAS)
        Next

        Return ACopy
    End Function

End Class
