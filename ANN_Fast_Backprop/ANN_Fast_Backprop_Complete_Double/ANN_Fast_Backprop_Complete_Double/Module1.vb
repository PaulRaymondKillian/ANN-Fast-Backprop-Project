Imports System.IO

Module Module1

    Const VALUE As Integer = 0
    Const BIAS As Integer = 1
    Const DELTA As Integer = 2

    'Network Structure Variables
    Dim countInputNeurons As Integer = 1
    Dim countHiddenNeurons As Integer = 20
    Dim countOutputNeurons As Integer = 1
    Dim epochs As Integer = 100000

    Dim outputInterval As Integer = 100
    Dim outputDeltaInterval As Integer = 100

    'Create Exemplar Set
    Dim exemplarCount As Integer = 15
    Dim exemplarInputs(,) As Double
    Dim exemplarOutputs(,) As Double

    Dim randomizedIndex() As Integer

    'Dynamic Rho Variables
    Dim rho As Double = 0.3
    Dim rhoInterval As Integer = 100
    Dim rhoMultiplier As Double = 0.999

    'Neuron arrays
    Dim inputValues() As Double
    Dim hiddenValues(,) As Double
    Dim outputValues(,) As Double

    'Weight arrays
    Dim ihWeights(,,) As Double 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Dim hoWeights(,,) As Double 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Dim maxWeightValue As Double = 5

    'Momentum Related Terms
    Dim momentumScaler As Double = 0.5  'Suggested by notes 0.9 to 0.5
    Const CURRENT As Integer = 0
    Const PREVIOUS As Integer = 1

    'Simulated Annealing Terms
    Dim annealingInterval As Integer = 1000
    Dim annealingMaxPercent As Integer = 30

    'Interval Value When Program Should Ask If You Want to Save (Used for learning tracking)
    Dim SaveInterval As Integer = 100000
    Dim SaveDirectory As String = ""
    Dim SaveNameSuffix As String = ""

    Sub Main()

        Dim input As String
        Console.Write("Would you like to load backprop from file (y/n):  ")
        input = Console.ReadLine()
        If input.ToUpper() = "Y" Then
            'Load information from file for quicker use
            Console.Write("Backprop file location: ")
            input = Console.ReadLine
            importBackpropFromFile(input)
        Else
            Console.Write("Dataset file location: ")
            input = Console.ReadLine()
            importDataSet(input)

            Do
                Console.Write("Number of Hidden Neurons: ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToDouble(input) > 0
            countHiddenNeurons = Convert.ToDouble(input)

            Do
                Console.Write("Max Weight Value: ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToDouble(input) > 0
            maxWeightValue = Convert.ToDouble(input)

            Do
                Console.Write("Epochs to Run: ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToInt32(input) > 0
            epochs = Convert.ToInt32(input)

            Do
                Console.Write("Output data interval: ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToInt32(input) >= 0
            outputInterval = Convert.ToInt32(input)

            Do
                Console.Write("Output delta data interval: ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToInt32(input) >= 0
            outputDeltaInterval = Convert.ToInt32(input)

            Do
                Console.Write("Initial Rho Value: ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToDouble(input) > 0
            rho = Convert.ToDouble(input)

            Do
                Console.Write("Rho Multiplier: ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToDouble(input) > 0
            rhoMultiplier = Convert.ToDouble(input)

            Do
                Console.Write("Dynamic Rho Interval: ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToInt32(input) > 0
            rhoInterval = Convert.ToInt32(input)

            Do
                Console.Write("Momentum Scaler (0.7-0.2 suggested): ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToDouble(input) >= 0
            momentumScaler = Convert.ToDouble(input)

            Do
                Console.Write("Simulated Annealing Interval: ")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToInt32(input) >= 0
            annealingInterval = Convert.ToInt32(input)

            Do
                Console.Write("Simulated Annealing Max Percent (0-100%):")
                input = Console.ReadLine
            Loop Until IsNumeric(input) = True AndAlso Convert.ToDouble(input) >= 0
            annealingMaxPercent = Convert.ToDouble(input)

        End If

        'Initialize Arrays
        ReDim inputValues(countInputNeurons - 1)
        ReDim hiddenValues(countHiddenNeurons - 1, 2)
        ReDim outputValues(countOutputNeurons - 1, 2)
        ReDim ihWeights(countInputNeurons - 1, countHiddenNeurons - 1, 1)
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
                hoWeights(i, j, CURRENT) = RandomWeight(maxWeightValue)
                hoWeights(i, j, PREVIOUS) = 0
            Next
        Next

        'Randomize Bias Values
        For i As Integer = 0 To countHiddenNeurons - 1
            hiddenValues(i, BIAS) = RandomWeight(maxWeightValue)
        Next

        For i As Integer = 0 To countOutputNeurons - 1
            outputValues(i, BIAS) = RandomWeight(maxWeightValue)
        Next

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        For epoch As Integer = 0 To epochs

            'Asks if you want to save the network and output information (Used for learning tracking)
            If epoch = 0 Or epoch Mod SaveInterval = 0 Then
                outputNetwork(SaveDirectory & "\Epoch" & epoch & "_Network_" & SaveNameSuffix & ".txt")
                outputNetworkOutput(SaveDirectory & "\Epoch" & epoch & "_NetworkOutput_" & SaveNameSuffix & ".txt")
            End If

            randomizedIndex = randomizeTheIndex(exemplarCount)

            For iteration As Integer = 0 To exemplarCount - 1

                'Set input values
                For i As Integer = 0 To countInputNeurons - 1
                    inputValues(i) = exemplarInputs(randomizedIndex(iteration), i)
                Next

                'Calculate Hidden Neuron Values
                For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                    hiddenValues(hiddenNeuron, VALUE) = 0   'Reset to 0
                    For inputNeuron As Integer = 0 To countInputNeurons - 1
                        hiddenValues(hiddenNeuron, VALUE) += inputValues(inputNeuron) * ihWeights(inputNeuron, hiddenNeuron, CURRENT)
                    Next
                    hiddenValues(hiddenNeuron, VALUE) = Activation(hiddenValues(hiddenNeuron, VALUE) + hiddenValues(hiddenNeuron, BIAS))
                Next

                'Calculate Output Neuron Values
                For outputNeuron As Integer = 0 To countOutputNeurons - 1
                    outputValues(outputNeuron, VALUE) = 0    'Reset to 0
                    For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                        outputValues(outputNeuron, VALUE) += hiddenValues(hiddenNeuron, VALUE) * hoWeights(hiddenNeuron, outputNeuron, CURRENT)
                    Next
                    outputValues(outputNeuron, VALUE) += outputValues(outputNeuron, BIAS)
                Next

                '''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                'BEGIN BACKPROP TRAINING                                '
                '''''''''''''''''''''''''''''''''''''''''''''''''''''''''

                'Calculate Output Neuron Deltas
                For i As Integer = 0 To countOutputNeurons - 1
                    outputValues(i, DELTA) = -2 * (exemplarOutputs(randomizedIndex(iteration), i) - outputValues(i, VALUE))

                    If epoch Mod outputInterval = 0 Or epoch = 0 Then
                        Console.WriteLine("Epoch: " & epoch & " Rho: " & rho & "  Error: " & exemplarOutputs(randomizedIndex(iteration), i) - outputValues(i, VALUE) & " Actual: " & outputValues(i, VALUE) & "  Desired: " & exemplarOutputs(randomizedIndex(iteration), i))
                    End If

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

                'Output Delta Information
                If epoch Mod outputDeltaInterval = 0 Or epoch = 0 Then
                    Console.WriteLine("Epoch: " & epoch & " Hidden Delta Totals: " & totalDeltas)
                End If


                'Calculate Hidden-Output Weights
                ''Utilizing Momemtum
                Dim previousWeightChange As Double
                For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                    For outputNeuron As Integer = 0 To countOutputNeurons - 1
                        previousWeightChange = hoWeights(hiddenNeuron, outputNeuron, CURRENT) - hoWeights(hiddenNeuron, outputNeuron, PREVIOUS)    'Store previous weight change
                        hoWeights(hiddenNeuron, outputNeuron, PREVIOUS) = hoWeights(hiddenNeuron, outputNeuron, CURRENT)    'Store current weight to be used as the next iteration's previous weight
                        hoWeights(hiddenNeuron, outputNeuron, CURRENT) = hoWeights(hiddenNeuron, outputNeuron, CURRENT) - rho * outputValues(outputNeuron, DELTA) * hiddenValues(hiddenNeuron, VALUE) + momentumScaler * previousWeightChange
                    Next
                    'Update Bias Value for Neuron
                    'XXXXXXXXXXXXXXXXX
                    'XXXXX What to use for the "output value" to use for the bias updates? Is the bias in fact updated?
                    'hiddenValues(hiddenNeuron, BIAS) = hiddenValues(hiddenNeuron, BIAS) - rho * outputValues
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

            Next

            'Simulated Annealing
            ''Adjust each weight in the weights layers to be in a location 'near' its current location (simulating shaking the solution plane)
            If epoch Mod annealingInterval = 0 Then
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
            End If

            If epoch Mod rhoInterval = 0 Then
                rho *= rhoMultiplier
            End If

        Next

        Console.Write("Would you like to save this network? (y/n): ")
        input = Console.ReadLine
        If input.ToUpper = "Y" Then
            Console.Write("Export filename: ")
            input = Console.ReadLine
            outputNetwork(input)
        End If

        Console.Write("Would you like to save this network's output? (y/n): ")
        input = Console.ReadLine
        If input.ToUpper = "Y" Then
            Console.Write("Export filename: ")
            input = Console.ReadLine
            outputNetworkOutput(input)
        End If

        Console.WriteLine("Press Enter to Exit...")
        Dim z As String = Console.ReadLine
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
        sw.WriteLine("1")
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

        'Calculate Output Neuron Values
        For outputNeuron As Integer = 0 To countOutputNeurons - 1
            outputValues(outputNeuron, VALUE) = 0    'Reset to 0
            For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                outputValues(outputNeuron, VALUE) += hiddenValues(hiddenNeuron, VALUE) * hoWeights(hiddenNeuron, outputNeuron, CURRENT)
            Next
            outputValues(outputNeuron, VALUE) += outputValues(outputNeuron, BIAS)
        Next
    End Sub


End Module
