Imports System.IO

Module Module1
    Private BPROPFileLocation As String
    Private PopFileLocation As String
    Private AppPreviousEntries(9) As String
    Private PopulationSize As Integer
    Private Generations As Integer
    Private EpochsPerGeneration As Integer
    Private NewGenerationMethod As Integer
    Private SaveDirectory As String
    Private FilePrefix As String

    Sub Main()
        Console.WriteLine("ANN Fast Backprop Population App (2009)")
        Console.WriteLine("Created by: Paul Killian")
        Console.WriteLine()
        Try
            LoadPopulationAppDataFile()
            ImportPopulationFile(PopFileLocation)

            Dim startTime As New DateTime
            startTime = Now

            Dim Population As New ANN_Fast_Backprop_Population.Population(BPROPFileLocation, PopulationSize)
            Population.RunGenerations(Generations, EpochsPerGeneration, NewGenerationMethod, SaveDirectory, FilePrefix)

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            Console.Write("Please Press Enter to Exit...")
            Dim z As String = Console.ReadLine()
        End Try
    End Sub

    Public Sub ImportPopulationFile(ByVal filePath As String)
        Dim sr As StreamReader
        Dim line As String
        Dim readLine() As String
        Try
            sr = File.OpenText(filePath)
            line = sr.ReadLine
            readLine = line.Split("="c)
            If readLine.Count <> 2 Then
                Throw New Exception("Error - ANN_Fast_Backprop_PopulationApp    Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
                Exit Sub
            End If
            BPROPFileLocation = (readLine(1))

            line = sr.ReadLine
            readLine = line.Split("="c)
            If readLine.Count <> 2 Then
                Throw New Exception("Error - ANN_Fast_Backprop_PopulationApp    Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
                Exit Sub
            End If
            PopulationSize = (Convert.ToInt32(readLine(1)))

            line = sr.ReadLine
            readLine = line.Split("="c)
            If readLine.Count <> 2 Then
                Throw New Exception("Error - ANN_Fast_Backprop_PopulationApp    Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
                Exit Sub
            End If
            Generations = Convert.ToInt32((readLine(1)))

            line = sr.ReadLine
            readLine = line.Split("="c)
            If readLine.Count <> 2 Then
                Throw New Exception("Error - ANN_Fast_Backprop_PopulationApp    Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
                Exit Sub
            End If
            EpochsPerGeneration = Convert.ToInt32((readLine(1)))

            line = sr.ReadLine
            readLine = line.Split("="c)
            If readLine.Count <> 2 Then
                Throw New Exception("Error - ANN_Fast_Backprop_PopulationApp    Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
                Exit Sub
            End If
            NewGenerationMethod = Convert.ToInt32((readLine(1)))

            line = sr.ReadLine
            readLine = line.Split("="c)
            If readLine.Count <> 2 Then
                Throw New Exception("Error - ANN_Fast_Backprop_PopulationApp    Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
                Exit Sub
            End If
            SaveDirectory = (readLine(1))

            line = sr.ReadLine
            readLine = line.Split("="c)
            If readLine.Count <> 2 Then
                Throw New Exception("Error - ANN_Fast_Backprop_PopulationApp    Descr: Invalid contents of input file. Inputs and DesiredOutput should be delimited by a single '=' character")
                Exit Sub
            End If
            FilePrefix = (readLine(1))

        Catch ex As Exception
            Throw New Exception("Error - Population File is Invalid or Corrupt")
        End Try
    End Sub

    Private Sub LoadPopulationAppDataFile()
        Try
            Dim numPreviousEntries As Integer = 0
            Dim sr As StreamReader
            sr = File.OpenText("AppPreviousEntries.txt")
            For i As Integer = 0 To 9
                If Not sr.EndOfStream Then
                    AppPreviousEntries(i) = sr.ReadLine
                    numPreviousEntries += 1
                    Console.WriteLine("Previous Entry " & i + 1 & ": " & AppPreviousEntries(i))
                End If
            Next
            sr.Close()
            If numPreviousEntries > 0 Then
                Console.Write("Enter Value of Previous Entry to Re-Execute (1 to " & numPreviousEntries & "), or 0 to Execute New File: ")
            Else
                GoTo EnterNewFile
            End If

            Dim input As String = Console.ReadLine
            If input = "1" Then
                PopFileLocation = AppPreviousEntries(0)
            ElseIf input = "2" Then
                If numPreviousEntries >= 2 Then
                    PopFileLocation = AppPreviousEntries(1)
                Else
                    GoTo EnterNewFile
                End If
            ElseIf input = "3" Then
                If numPreviousEntries >= 3 Then
                    PopFileLocation = AppPreviousEntries(2)
                Else
                    GoTo EnterNewFile
                End If
            ElseIf input = "4" Then
                If numPreviousEntries >= 4 Then
                    PopFileLocation = AppPreviousEntries(3)
                Else
                    GoTo EnterNewFile
                End If
            ElseIf input = "5" Then
                If numPreviousEntries >= 5 Then
                    PopFileLocation = AppPreviousEntries(4)
                Else
                    GoTo EnterNewFile
                End If
            ElseIf input = "6" Then
                If numPreviousEntries >= 6 Then
                    PopFileLocation = AppPreviousEntries(5)
                Else
                    GoTo EnterNewFile
                End If
            ElseIf input = "7" Then
                If numPreviousEntries >= 7 Then
                    PopFileLocation = AppPreviousEntries(6)
                Else
                    GoTo EnterNewFile
                End If
            ElseIf input = "8" Then
                If numPreviousEntries >= 8 Then
                    PopFileLocation = AppPreviousEntries(7)
                Else
                    GoTo EnterNewFile
                End If
            ElseIf input = "9" Then
                If numPreviousEntries >= 9 Then
                    PopFileLocation = AppPreviousEntries(8)
                Else
                    GoTo EnterNewFile
                End If
            ElseIf input = "10" Then
                If numPreviousEntries >= 10 Then
                    PopFileLocation = AppPreviousEntries(9)
                Else
                    GoTo EnterNewFile
                End If
            Else
                GoTo EnterNewFile
            End If
        Catch ex As Exception
            GoTo EnterNewFile
        End Try

        Exit Sub
EnterNewFile:
        Console.Write("Please Input Population File Location: ")
        PopFileLocation = Console.ReadLine

        'Move everything down 1
        For i As Integer = 9 To 1 Step -1
            AppPreviousEntries(i) = AppPreviousEntries(i - 1)
        Next
        AppPreviousEntries(0) = PopFileLocation

        Dim sw As StreamWriter
        sw = File.CreateText("AppPreviousEntries.txt")
        For i As Integer = 0 To 9
            If AppPreviousEntries(i) <> "" Then
                sw.WriteLine(AppPreviousEntries(i))
            End If
        Next
        sw.Close()
    End Sub

End Module
