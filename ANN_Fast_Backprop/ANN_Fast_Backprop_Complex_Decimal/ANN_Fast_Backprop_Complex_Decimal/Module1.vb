Module Module1

    Const VALUE As Integer = 0
    Const BIAS As Integer = 1
    Const DELTA As Integer = 2

    'Network Structure Variables
    Dim countInputNeurons As Integer = 1
    Dim countHiddenNeurons As Integer = 30
    Dim countOutputNeurons As Integer = 1
    Dim epochs As Integer = 5000

    Dim outputInterval As Integer = 100

    'Dynamic Rho Variables
    Dim rho As Decimal = 0.3
    Dim rhoInterval As Integer = 100
    Dim rhoMultiplier As Decimal = 0.999

    'Neuron arrays
    Dim inputValues() As Decimal
    Dim hiddenValues(,) As Decimal
    Dim outputValues(,) As Decimal

    'Weight arrays
    Dim ihWeights(,,) As Decimal 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Dim hoWeights(,,) As Decimal 'Increased dimension of weight array to include CURRENT/PREVIOUS weights (Momentum)
    Dim maxWeightValue As Decimal = 0.1

    'Momentum Related Terms
    Dim momentumScaler As Decimal = 0.5  'Suggested by notes 0.9 to 0.5
    Const CURRENT As Integer = 0
    Const PREVIOUS As Integer = 1

    'Simulated Annealing Terms
    Dim annealingInterval As Integer = 1000
    Dim annealingMaxPercent As Integer = 30

    Sub Main()
        'Create Exemplar Set
        Dim exemplarCount As Integer = 15
        Dim exemplarSet(exemplarCount - 1, 1) As Decimal
        For i As Integer = 1 To exemplarCount
            exemplarSet(i - 1, 0) = i
            exemplarSet(i - 1, 1) = 1 / i
        Next

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

        For epoch As Integer = 0 To epochs
            For iteration As Integer = 0 To exemplarCount - 1

                'Set input value(s)
                'For i As Integer = 0 To countInputNeurons - 1
                inputValues(0) = exemplarSet(iteration, 0)
                'Next

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
                    outputValues(i, DELTA) = -2 * (exemplarSet(iteration, 1) - outputValues(i, VALUE))

                    If epoch Mod outputInterval = 0 Or epoch = 0 Then
                        Console.WriteLine("Epoch: " & epoch & " Rho: " & rho & "  Error: " & exemplarSet(iteration, 1) - outputValues(i, VALUE) & " Actual: " & outputValues(i, VALUE) & "  Desired: " & exemplarSet(iteration, 1))
                    End If

                Next

                'Calculate Hidden Neuron Deltas
                Dim totalDeltas As Decimal = 0
                For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                    hiddenValues(hiddenNeuron, DELTA) = 0   'Reset Value
                    For outputNeuron As Integer = 0 To countOutputNeurons - 1
                        hiddenValues(hiddenNeuron, DELTA) += fPrime(hiddenValues(hiddenNeuron, VALUE)) * outputValues(outputNeuron, DELTA) * hoWeights(hiddenNeuron, outputNeuron, CURRENT)
                        totalDeltas += hiddenValues(hiddenNeuron, DELTA)
                    Next
                Next

                'Output Delta Information
                If epoch Mod outputInterval = 0 Or epoch = 0 Then
                    Console.WriteLine("Epoch: " & epoch & " Hidden Delta Totals: " & totalDeltas)
                End If


                'Calculate Hidden-Output Weights
                ''Utilizing Momemtum
                Dim previousWeightChange As Decimal
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

        Console.WriteLine("Press Enter to Exit...")
        Dim z As String = Console.ReadLine
    End Sub
End Module
