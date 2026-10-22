Module Module1
    Sub Main()
        Const VALUE As Integer = 0
        Const BIAS As Integer = 1
        Const DELTA As Integer = 2
        Dim maxValueAllowed As Double = Math.Pow(Double.MaxValue, 1 / 3)

        Dim countInputNeurons As Integer = 1
        Dim countHiddenNeurons As Integer = 10
        Dim countOutputNeurons As Integer = 1
        Dim epochs As Integer = 1000
        Dim outputInterval As Integer = 1
        Dim rho As Double = 0.3
        Dim rhoInterval As Integer = 100
        Dim rhoMultiplier As Double = 0.999

        Dim inputValues() As Double
        Dim hiddenValues(,) As Double
        Dim outputValues(,) As Double
        Dim ihWeights(,) As Double
        Dim hoWeights(,) As Double
        Dim maxWeightValue As Double = 10


        'Create Exemplar Set
        Dim exemplarCount As Integer = 15
        Dim exemplarSet(exemplarCount - 1, 1) As Double
        For i As Integer = 1 To exemplarCount
            exemplarSet(i - 1, 0) = i
            exemplarSet(i - 1, 1) = 1 / i
        Next


        'Initialize Arrays
        ReDim inputValues(countInputNeurons - 1)
        ReDim hiddenValues(countHiddenNeurons - 1, 2)
        ReDim outputValues(countOutputNeurons - 1, 2)
        ReDim ihWeights(countInputNeurons - 1, countHiddenNeurons - 1)
        ReDim hoWeights(countHiddenNeurons - 1, countOutputNeurons - 1)

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






                'BEGIN BACKPROP TRAINING

                'Calculate Output Neuron Deltas
                For i As Integer = 0 To countOutputNeurons - 1
                    outputValues(i, DELTA) = -2 * (exemplarSet(iteration, 1) - outputValues(i, VALUE))

                    'Check/Set Max Value Allowed (To Prevent Overflow)
                    If outputValues(i, DELTA) > maxValueAllowed Then
                        outputValues(i, DELTA) = maxValueAllowed
                    ElseIf outputValues(i, DELTA) < -maxValueAllowed Then
                        outputValues(i, DELTA) = -maxValueAllowed
                    End If

                    If epoch Mod outputInterval = 0 Or epoch = 0 Then
                        Console.WriteLine("Epoch: " & epoch & " Rho: " & rho & "  Error: " & exemplarSet(iteration, 1) - outputValues(i, VALUE) & "    Actual: " & outputValues(i, VALUE) & "  Desired: " & exemplarSet(iteration, 1) & " MaxValueAllowed: " & maxValueAllowed)
                    End If

                Next

                'Calculate Hidden Neuron Deltas
                For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                    hiddenValues(hiddenNeuron, DELTA) = 0   'Reset Value
                    For outputNeuron As Integer = 0 To countOutputNeurons - 1
                        hiddenValues(hiddenNeuron, DELTA) += fPrime(hiddenValues(hiddenNeuron, VALUE)) * outputValues(outputNeuron, DELTA) * hoWeights(hiddenNeuron, outputNeuron)
                    Next
                Next

                'Calculate Hidden-Output Weights
                For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                    For outputNeuron As Integer = 0 To countOutputNeurons - 1
                        hoWeights(hiddenNeuron, outputNeuron) = hoWeights(hiddenNeuron, outputNeuron) - rho * outputValues(outputNeuron, DELTA) * hiddenValues(hiddenNeuron, VALUE)
                    Next
                Next

                'Calculate Input-Hidden Weights
                For inputNeuron As Integer = 0 To countInputNeurons - 1
                    For hiddenNeuron As Integer = 0 To countHiddenNeurons - 1
                        ihWeights(inputNeuron, hiddenNeuron) = ihWeights(inputNeuron, hiddenNeuron) - rho * hiddenValues(hiddenNeuron, DELTA) * inputValues(inputNeuron)
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
