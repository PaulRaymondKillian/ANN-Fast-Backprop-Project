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
    Public countHiddenLayers As Integer = 1
    Dim countOutputNeurons As Integer
    Public epochs As Integer

    Public outputInterval As Integer
    Public outputDeltaInterval As Integer

    'Create Exemplar Set
    Dim exemplarCount As Integer
    Dim exemplarInputs(,) As Double
    Dim exemplarOutputs(,) As Double

    'Dynamic Rho Variables
    Public rho As Double
    Public rhoInterval As Integer
    Public rhoMultiplier As Double

    'Neuron arrays
    Dim inputValues() As Double
    Public hiddenValues(,) As Double
    Public outputValues(,) As Double

    'Weight arrays
    Public ihWeights(,) As Double
    Public hoWeights(,) As Double
    Public maxWeightValue As Double
    Public maxBiasValue As Double

    'Store Weight Changes
    Public ihWeightsHOLD(,) As Double
    Public hoWeightsHOLD(,) As Double

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
        ReDim hiddenValues(countHiddenNeurons - 1, 2)
        ReDim outputValues(countOutputNeurons - 1, 2)
        ReDim ihWeights(countInputNeurons - 1, countHiddenNeurons - 1)
        ReDim hoWeights(countHiddenNeurons - 1, countOutputNeurons - 1)
        ReDim ihWeightsHOLD(countInputNeurons - 1, countHiddenNeurons - 1)
        ReDim hoWeightsHOLD(countHiddenNeurons - 1, countOutputNeurons - 1)

        'Randomize Weight Arrays
        For i As Integer = 0 To countInputNeurons - 1
            For j As Integer = 0 To countHiddenNeurons - 1
                ihWeights(i, j) = RandomWeight(maxWeightValue)
            Next
        Next

        For i As Integer = 0 To countHiddenNeurons - 1
            For j As Integer = 0 To countOutputNeurons - 1
                hoWeights(i, j) = RandomWeight(maxWeightValue)
            Next
        Next

        'Randomize Bias Values
        For i As Integer = 0 To countHiddenNeurons - 1
            hiddenValues(i, BIAS) = RandomWeight(maxBiasValue)
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
                                        "Descr: Input File is Invalid or Corrupt")

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

        ''''''''''''''''''''''This is reading in the line regarding the
        line = sr.ReadLine  ''number of hidden layers which we ignore because this
        ''''''''''''''''''''''type of network only handles 1 hidden layer

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

        ''''''''''''''''''''''This is reading in the line regarding the
        line = sr.ReadLine  ''the momentum term but this network does not
        ''''''''''''''''''''''use momentum so we will ignore this.

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
        sw.WriteLine("1")   'This network only has 1 hidden layer, so always write out a 1
        sw.WriteLine(countOutputNeurons)
        sw.WriteLine(countInputNeurons * countHiddenNeurons + countHiddenNeurons * countOutputNeurons + countHiddenNeurons + countOutputNeurons)

        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            For inputNeuron As Integer = 0 To countInputNeurons - 1
                sw.WriteLine(ihWeights(inputNeuron, hiddenNeuron))
            Next
            sw.WriteLine(hiddenValues(hiddenNeuron, BIAS))
        Next

        For outputNeuron As Integer = 0 To countOutputNeurons - 1
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                sw.WriteLine(hoWeights(hiddenNeuron, outputNeuron))
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
                hiddenValues(hiddenNeuron, VALUE) += inputValues(inputNeuron) * ihWeights(inputNeuron, hiddenNeuron)
            Next
            hiddenValues(hiddenNeuron, VALUE) = Activation(hiddenValues(hiddenNeuron, VALUE) + hiddenValues(hiddenNeuron, BIAS))
        Next

        'Calculate Output Neuron Values
        For outputNeuron As Integer = 0 To countOutputNeurons - 1
            outputValues(outputNeuron, VALUE) = 0    'Reset to 0
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                outputValues(outputNeuron, VALUE) += hiddenValues(hiddenNeuron, VALUE) * hoWeights(hiddenNeuron, outputNeuron)
            Next
            outputValues(outputNeuron, VALUE) += outputValues(outputNeuron, BIAS)
        Next
    End Sub

    Public Sub Backprop(ByVal iteration As Integer)
        'Calculate Output Neuron Deltas
        For i As Integer = 0 To countOutputNeurons - 1
            outputValues(i, DELTA) = -2 * (exemplarOutputs(iteration, i) - outputValues(i, VALUE))
        Next

        'Calculate Hidden Neuron Deltas
        Dim totalDeltas As Double = 0
        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            hiddenValues(hiddenNeuron, DELTA) = 0   'Reset Value
            For outputNeuron As Integer = 0 To countOutputNeurons - 1
                hiddenValues(hiddenNeuron, DELTA) += fPrime(hiddenValues(hiddenNeuron, VALUE)) * outputValues(outputNeuron, DELTA) * hoWeights(hiddenNeuron, outputNeuron)
                totalDeltas += hiddenValues(hiddenNeuron, DELTA)
            Next
        Next

        'Calculate Hidden-Output Weights in BATCH MODE
        ''Utilizing Momemtum
        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            For outputNeuron As Integer = 0 To countOutputNeurons - 1
                hoWeightsHOLD(hiddenNeuron, outputNeuron) += -rho * outputValues(outputNeuron, DELTA) * hiddenValues(hiddenNeuron, VALUE)
            Next
        Next

        'Calculate Input-Hidden Weights
        ''Utilizing Momentum
        For inputNeuron As Integer = 0 To countInputNeurons - 1
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                ihWeightsHOLD(inputNeuron, hiddenNeuron) += -rho * hiddenValues(hiddenNeuron, DELTA) * inputValues(inputNeuron)
            Next
        Next
    End Sub

    Public Sub TrainEpoch(ByVal UseRhoMultiplier As Boolean, ByVal SimulatedAnnealing As Boolean)
        Fitness = 0 'Reset Fitness Value
        ResetWeightHolds()

        For iteration As Integer = 0 To exemplarCount - 1
            'No training is actually done here, only stores the weight changes to be made based on current ANN
            run(iteration)
            Backprop(iteration)
        Next

        'Now Update All Weight Values Stored
        UpdateWeights()

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
                hoWeights(hiddenNeuron, outputNeuron) = hoWeights(hiddenNeuron, outputNeuron) + ((RandomNumber(annealingMaxPercent) / 100) * hoWeights(hiddenNeuron, outputNeuron))
            Next
        Next

        For inputNeuron As Integer = 0 To countInputNeurons - 1
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                ihWeights(inputNeuron, hiddenNeuron) = ihWeights(inputNeuron, hiddenNeuron) + ((RandomNumber(annealingMaxPercent) / 100) * ihWeights(inputNeuron, hiddenNeuron))
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
                ACopy.ihWeights(i, j) = ihWeights(i, j)
            Next
        Next

        For i As Integer = 0 To countHiddenNeurons - 1
            For j As Integer = 0 To countOutputNeurons - 1
                ACopy.hoWeights(i, j) = hoWeights(i, j)
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

    Public Sub ResetWeightHolds()
        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            For outputNeuron As Integer = 0 To countOutputNeurons - 1
                hoWeightsHOLD(hiddenNeuron, outputNeuron) = 0
            Next
        Next

        'Calculate Input-Hidden Weights
        ''Utilizing Momentum
        For inputNeuron As Integer = 0 To countInputNeurons - 1
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                ihWeightsHOLD(inputNeuron, hiddenNeuron) = 0
            Next
        Next
    End Sub

    Public Sub UpdateWeights()

        For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
            For outputNeuron As Integer = 0 To countOutputNeurons - 1
                hoWeights(hiddenNeuron, outputNeuron) += hoWeightsHOLD(hiddenNeuron, outputNeuron)
            Next
        Next

        For inputNeuron As Integer = 0 To countInputNeurons - 1
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                ihWeights(inputNeuron, hiddenNeuron) += ihWeightsHOLD(inputNeuron, hiddenNeuron)
            Next
        Next
    End Sub

End Class
