Module Module1

    Const VALUE As Integer = 0
    Const BIAS As Integer = 1
    Const DELTA As Integer = 2

    Dim countInputNeurons As Integer = 1
    Dim countHiddenNeurons As Integer = 6
    Dim countOutputNeurons As Integer = 1
    Dim epochs As Integer = 100000
    Dim outputInterval As Integer = 1
    Dim rho As Decimal = 0.5
    Dim rhoInterval As Integer = 100
    Dim rhoMultiplier As Decimal = 0.99

    Dim inputValues() As Decimal
    Dim hiddenValues(,) As Decimal
    Dim outputValues(,) As Decimal
    Dim ihWeights(,,) As Decimal 'Increased dimension of weight array to include CURRENT/PREVIOUS weights
    Dim hoWeights(,,) As Decimal 'Increased dimension of weight array to include CURRENT/PREVIOUS weights
    Dim maxWeightValue As Decimal = 1

    'Momentum Related Terms
    Dim momentumScaler As Decimal = 0.3  'Suggested by notes 0.9 to 0.5
    Const CURRENT As Integer = 0
    Const PREVIOUS As Integer = 1

    Sub Main()
        ''Create Exemplar Set
        'Dim exemplarCount As Integer = 15
        'Dim exemplarSet(exemplarCount - 1, 1) As Decimal
        'For i As Integer = 1 To exemplarCount
        '    exemplarSet(i - 1, 0) = i
        '    exemplarSet(i - 1, 1) = 1 / i
        'Next

        'Create Exemplar Set
        Dim exemplarCount As Integer = 10
        Dim exemplarSet(exemplarCount - 1, 1) As Decimal
        For i As Integer = 1 To exemplarCount
            exemplarSet(i - 1, 0) = i / 5
            exemplarSet(i - 1, 1) = 1 + Math.Sin(((4 * Math.PI) / 4) * (i / 5))
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






                'BEGIN BACKPROP TRAINING

                'Calculate Output Neuron Deltas
                For i As Integer = 0 To countOutputNeurons - 1
                    outputValues(i, DELTA) = -2 * (exemplarSet(iteration, 1) - outputValues(i, VALUE))
                    'outputValues(i, DELTA) = exemplarSet(iteration, 1) - outputValues(i, VALUE)

                    If epoch Mod outputInterval = 0 Or epoch = 0 Then
                        Console.WriteLine("Epoch: " & epoch & " Rho: " & rho & "  Error: " & exemplarSet(iteration, 1) - outputValues(i, VALUE) & " Actual: " & outputValues(i, VALUE) & "  Desired: " & exemplarSet(iteration, 1))
                    End If

                Next

                'Calculate Hidden Neuron Deltas
                For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                    hiddenValues(hiddenNeuron, DELTA) = 0   'Reset Value
                    For outputNeuron As Integer = 0 To countOutputNeurons - 1
                        hiddenValues(hiddenNeuron, DELTA) += fPrime(hiddenValues(hiddenNeuron, VALUE)) * outputValues(outputNeuron, DELTA) * hoWeights(hiddenNeuron, outputNeuron, CURRENT)
                    Next
                Next

                'Calculate Hidden-Output Weights
                Dim previousWeightChange As Decimal
                For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                    For outputNeuron As Integer = 0 To countOutputNeurons - 1
                        previousWeightChange = hoWeights(hiddenNeuron, outputNeuron, CURRENT) - hoWeights(hiddenNeuron, outputNeuron, PREVIOUS)    'Store previous weight change
                        hoWeights(hiddenNeuron, outputNeuron, PREVIOUS) = hoWeights(hiddenNeuron, outputNeuron, CURRENT)    'Store current weight to be used as the next iteration's previous weight
                        hoWeights(hiddenNeuron, outputNeuron, CURRENT) = hoWeights(hiddenNeuron, outputNeuron, CURRENT) - rho * outputValues(outputNeuron, DELTA) * hiddenValues(hiddenNeuron, VALUE) + momentumScaler * previousWeightChange
                    Next
                Next

                'Calculate Input-Hidden Weights
                For inputNeuron As Integer = 0 To countInputNeurons - 1
                    For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                        previousWeightChange = ihWeights(inputNeuron, hiddenNeuron, CURRENT) - ihWeights(inputNeuron, hiddenNeuron, PREVIOUS)
                        ihWeights(inputNeuron, hiddenNeuron, PREVIOUS) = ihWeights(inputNeuron, hiddenNeuron, CURRENT)
                        ihWeights(inputNeuron, hiddenNeuron, CURRENT) = ihWeights(inputNeuron, hiddenNeuron, CURRENT) - rho * hiddenValues(hiddenNeuron, DELTA) * inputValues(inputNeuron) + momentumScaler * previousWeightChange
                    Next
                Next

            Next

            If epoch Mod rhoInterval = 0 Then
                rho *= rhoMultiplier
            End If

        Next

        Console.WriteLine("Press Enter to Exit...")
        Dim z As String = Console.ReadLine
    End Sub
End Module
