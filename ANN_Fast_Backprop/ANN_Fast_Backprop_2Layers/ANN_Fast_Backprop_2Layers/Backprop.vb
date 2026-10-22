Imports System.IO

Public Class Backprop

    Public ID As String 'Optional String To Use to Identifier the specific object

    Const VALUE As Integer = 0
    Const BIAS As Integer = 1
    Const DELTA As Integer = 2

    Dim DataFileName As String

    'Network Structure Variables
    Dim countInputNeurons As Integer
    Dim countHiddenNeurons As Integer
    Dim countOutputNeurons As Integer
    Dim epochs As Integer

    Dim outputInterval As Integer
    Dim outputDeltaInterval As Integer

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
    Public hiddenValues(,) As Double
    Public hiddenValues2(,) As Double
    Public outputValues(,) As Double

    'Weight arrays
    Public ihWeights(,,) As Double 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Public hhWeights(,,) As Double 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Public hoWeights(,,) As Double 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Dim maxWeightValue As Double
    Dim maxBiasValue As Double

    'Momentum Related Terms
    Dim momentumScaler As Double = 0.5  'Suggested by notes 0.9 to 0.5
    Const CURRENT As Integer = 0
    Const PREVIOUS As Integer = 1

    'Simulated Annealing Terms
    Dim annealingInterval As Integer = 1000
    Dim annealingMaxPercent As Integer = 30

    'Interval Value When Program Should Ask If You Want to Save (Used for learning tracking)
    Dim SaveInterval As Integer = 100000
    Dim SaveDirectory As String
    Dim SaveNameSuffix As String

    'Fitness Variable to determine accuracy/quality of backprop network
    Public Fitness As Double = 0


    Public Sub New(ByVal fileName As String)
        DataFileName = fileName
        importBackpropFromFile(DataFileName)

        'Initialize Arrays
        ReDim inputValues(countInputNeurons - 1)
        ReDim hiddenValues(countHiddenNeurons - 1, 2)
        ReDim hiddenValues2(countHiddenNeurons - 1, 2)
        ReDim outputValues(countOutputNeurons - 1, 2)
        ReDim ihWeights(countInputNeurons - 1, countHiddenNeurons - 1, 1)
        ReDim hhWeights(countInputNeurons - 1, countHiddenNeurons - 1, 1)
        ReDim hoWeights(countHiddenNeurons - 1, countOutputNeurons - 1, 1)

        'Randomize Weight Arrays
        For i As Integer = 0 To countInputNeurons - 1
            For j As Integer = 0 To countHiddenNeurons - 1
                ihWeights(i, j, CURRENT) = RandomWeight(maxWeightValue)
                ihWeights(i, j, PREVIOUS) = 0
            Next
        Next

        For i As Integer = 0 To countHiddenNeurons - 1
            For j As Integer = 0 To countOutputNeurons - 1
                hhWeights(i, j, CURRENT) = RandomWeight(maxWeightValue)
                hhWeights(i, j, PREVIOUS) = 0
            Next
        Next

        For i As Integer = 0 To countHiddenNeurons - 1
            For j As Integer = 0 To countOutputNeurons - 1
                hoWeights(i, j, CURRENT) = RandomWeight(maxWeightValue)
                hoWeights(i, j, PREVIOUS) = 0
            Next
        Next

        'Randomize Bias Values
        For i As Integer = 0 To countHiddenNeurons - 1
            hiddenValues(i, BIAS) = RandomWeight(maxBiasValue)
        Next

        'Randomize Bias Values
        For i As Integer = 0 To countHiddenNeurons - 1
            hiddenValues2(i, BIAS) = RandomWeight(maxBiasValue)
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
                                        "Descr: XXXXXXXXXXXXXXXXXXXXXXXX")

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
        sw.WriteLine("2")
        sw.WriteLine(countOutputNeurons)
        sw.WriteLine(countInputNeurons * countHiddenNeurons + countHiddenNeurons * countOutputNeurons + countHiddenNeurons + countOutputNeurons)

        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            For inputNeuron As Integer = 0 To countInputNeurons - 1
                sw.WriteLine(ihWeights(inputNeuron, hiddenNeuron, CURRENT))
            Next
            sw.WriteLine(hiddenValues(hiddenNeuron, BIAS))
        Next

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

        'Calculate Hidden Neuron Values
        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            hiddenValues(hiddenNeuron, VALUE) = 0   'Reset to 0
            For inputNeuron As Integer = 0 To countInputNeurons - 1
                hiddenValues(hiddenNeuron, VALUE) += inputValues(inputNeuron) * ihWeights(inputNeuron, hiddenNeuron, CURRENT)
            Next
            hiddenValues(hiddenNeuron, VALUE) = Activation(hiddenValues(hiddenNeuron, VALUE) + hiddenValues(hiddenNeuron, BIAS))
        Next

        'Calculate Hidden2 Neuron Values
        For hiddenNeuron2 As Integer = 0 To countHiddenNeurons - 1
            hiddenValues(hiddenNeuron2, VALUE) = 0   'Reset to 0
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                hiddenValues2(hiddenNeuron2, VALUE) += hiddenValues(hiddenNeuron, VALUE) * hhWeights(hiddenNeuron, hiddenNeuron2, CURRENT)
            Next
            hiddenValues2(hiddenNeuron2, VALUE) = Activation(hiddenValues2(hiddenNeuron2, VALUE) + hiddenValues2(hiddenNeuron2, BIAS))
        Next

        'Calculate Output Neuron Values
        For outputNeuron As Integer = 0 To countOutputNeurons - 1
            outputValues(outputNeuron, VALUE) = 0    'Reset to 0
            For hiddenNeuron2 As Integer = 0 To countHiddenNeurons - 1
                outputValues(outputNeuron, VALUE) += hiddenValues2(hiddenNeuron2, VALUE) * hoWeights(hiddenNeuron2, outputNeuron, CURRENT)
            Next
            outputValues(outputNeuron, VALUE) += outputValues(outputNeuron, BIAS)
        Next
    End Sub

    Public Sub Backprop(ByVal iteration As Integer)
        'Calculate Output Neuron Deltas
        For i As Integer = 0 To countOutputNeurons - 1
            outputValues(i, DELTA) = -2 * (exemplarOutputs(randomizedIndex(iteration), i) - outputValues(i, VALUE))
        Next

        'Calculate Hidden Neuron Deltas
        Dim totalDeltas As Double = 0
        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            hiddenValues(hiddenNeuron, DELTA) = 0   'Reset Value
            For outputNeuron As Integer = 0 To countOutputNeurons - 1
                hiddenValues(hiddenNeuron, DELTA) += fPrime(hiddenValues(hiddenNeuron, VALUE)) * outputValues(outputNeuron, DELTA) * hoWeights(hiddenNeuron, outputNeuron, CURRENT)
                totalDeltas += hiddenValues(hiddenNeuron, DELTA)
            Next
        Next

        'Calculate Hidden-Output Weights
        ''Utilizing Momemtum
        Dim previousWeightChange As Double
        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            For outputNeuron As Integer = 0 To countOutputNeurons - 1
                previousWeightChange = hoWeights(hiddenNeuron, outputNeuron, CURRENT) - hoWeights(hiddenNeuron, outputNeuron, PREVIOUS)    'Store previous weight change
                hoWeights(hiddenNeuron, outputNeuron, PREVIOUS) = hoWeights(hiddenNeuron, outputNeuron, CURRENT)    'Store current weight to be used as the next iteration's previous weight
                hoWeights(hiddenNeuron, outputNeuron, CURRENT) = hoWeights(hiddenNeuron, outputNeuron, CURRENT) - rho * outputValues(outputNeuron, DELTA) * hiddenValues(hiddenNeuron, VALUE) + momentumScaler * previousWeightChange
            Next
        Next

        'Calculate Input-Hidden Weights
        ''Utilizing Momentum
        For inputNeuron As Integer = 0 To countInputNeurons - 1
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                previousWeightChange = ihWeights(inputNeuron, hiddenNeuron, CURRENT) - ihWeights(inputNeuron, hiddenNeuron, PREVIOUS)
                ihWeights(inputNeuron, hiddenNeuron, PREVIOUS) = ihWeights(inputNeuron, hiddenNeuron, CURRENT)
                ihWeights(inputNeuron, hiddenNeuron, CURRENT) = ihWeights(inputNeuron, hiddenNeuron, CURRENT) - rho * hiddenValues(hiddenNeuron, DELTA) * inputValues(inputNeuron) + momentumScaler * previousWeightChange
            Next
        Next
    End Sub

    Public Sub TrainEpoch(ByVal UseRhoMultiplier As Boolean, ByVal SimulatedAnnealing As Boolean)
        Fitness = 0 'Reset Fitness Value
        randomizedIndex = randomizeTheIndex(exemplarCount)

        For iteration As Integer = 0 To exemplarCount - 1
            run(randomizedIndex(iteration))
            Backprop(iteration)
        Next

        'Simulated Annealing
        ''Adjust each weight in the weights layers to be in a location 'near' its current location (simulating shaking the solution plane)
        If SimulatedAnnealing Then
            InitiateSimulatedAnnealing()
        End If

        'Dynamic Rho Value
        If UseRhoMultiplier Then
            'Console.WriteLine("#####Rho: " & rho & "    Multiplier: " & rhoMultiplier & "   Interval: " & rhoInterval)
            rho *= rhoMultiplier
            'Console.WriteLine("*****Rho Multiplied: " & rho)
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

        'Copy Input-Hidden Weights
        For i As Integer = 0 To countInputNeurons - 1
            For j As Integer = 0 To countHiddenNeurons - 1
                ACopy.ihWeights(i, j, CURRENT) = ihWeights(i, j, CURRENT)
                ACopy.ihWeights(i, j, PREVIOUS) = ihWeights(i, j, PREVIOUS)
            Next
        Next

        For i As Integer = 0 To countHiddenNeurons - 1
            For j As Integer = 0 To countOutputNeurons - 1
                ACopy.hoWeights(i, j, CURRENT) = hoWeights(i, j, CURRENT)
                ACopy.hoWeights(i, j, PREVIOUS) = hoWeights(i, j, PREVIOUS)
            Next
        Next

        'Randomize Bias Values
        For i As Integer = 0 To countHiddenNeurons - 1
            ACopy.hiddenValues(i, BIAS) = hiddenValues(i, BIAS)
        Next

        For i As Integer = 0 To countOutputNeurons - 1
            ACopy.outputValues(i, BIAS) = outputValues(i, BIAS)
        Next

        Return ACopy
    End Function

End Class
