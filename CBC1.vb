Module Module1

    '=======================================================
    'Conceptual Block Cipher no. 1
    '=======================================================
    '- This will be included in the document as proof of iterative development
    '- It shows the algorithms have been developed separately to the program and efficiently tested 
    '- Good programming practise of creating an effective program before implementing it to the final application
    '- Written by Kavian Shirkoohi, April 2017

    'Program can either encrypt or decrypt a message
    'Encryption algorithms use the substitution-permutation network

    'Initialize public variables
    Public state(3, 3) As Byte
    Public sbox = New Byte(3, 3) {{0, 1, 2, 3}, {0, 2, 4, 6}, {1, 2, 3, 4}, {2, 4, 6, 8}}
    Public encLvl As Integer
    Public keySize As Integer
    Public rounds As Integer
    Public plaintext(15) As Byte
    Public ciphertext(15) As Byte
    Public key As String

    '-------------------------------------------------------
    'User interface 
    '-------------------------------------------------------

    'Focusing only on user interface aspects required for functionality and debugging

    Sub NewLine()
        Console.WriteLine("")
    End Sub

    Sub EnterChoice()
        Console.WriteLine("Press 1 for encryption, 2 for decryption:")

        Dim choice As Integer
        choice = Console.ReadLine()

        Select Case choice
            Case 1
                EnterInputs(1)
            Case 2
                EnterInputs(2)
        End Select
    End Sub

    Sub EnterInputs(choice As Integer)
        Select Case choice
            Case 1
                NewLine()
                Console.WriteLine("Enter alphanumeric message to encrypt, max 16 characters:")

                Dim message As String
                message = Console.ReadLine()
                plaintext = StringToByteArray(message, 15)

                NewLine()
                Console.WriteLine("Select the level of encryption 128, 192 or 256-bit:")
                encLvl = Console.ReadLine()

                Select Case encLvl
                    Case 128
                        keySize = 16
                        rounds = 10
                    Case 192
                        keySize = 24
                        rounds = 12
                    Case 256
                        keySize = 32
                        rounds = 14
                End Select

                NewLine()
                Console.WriteLine("Enter the key to encrypt the message with, max" + Str(keySize) + " characters:")
                key = Console.ReadLine()

                Encryption(FillState(plaintext), ExpandKey(StringToByteArray(key, keySize)), rounds)

                'Note: the user MUST remember their key and the encryption level to decrypt

            Case 2
                NewLine()
                Console.WriteLine("Enter 16-number hexadecimal message to decrypt:")

                Dim message As String
                message = Console.ReadLine()
                ciphertext = HexToByteArray(message, 15)

                NewLine()
                Console.WriteLine("Select the level of encryption 128, 192 or 256-bit that was used:")
                encLvl = Console.ReadLine()

                Select Case encLvl
                    Case 128
                        keySize = 16
                        rounds = 10
                    Case 192
                        keySize = 24
                        rounds = 12
                    Case 256
                        keySize = 32
                        rounds = 14
                End Select

                NewLine()
                Console.WriteLine("Enter the key that was used to decrypt the message with:")
                key = Console.ReadLine()

                Decryption(FillState(ciphertext), ExpandKey(StringToByteArray(key, keySize)), rounds)
        End Select
    End Sub

    '-------------------------------------------------------
    'Essential operations reused throughout
    '-------------------------------------------------------

    'Converts a simple string to its ASCII array of characters in bytes
    Function StringToByteArray(text As String, size As Integer) As Byte()
        Dim textArray(size) As Byte
        For i = 0 To Len(text) - 1
            textArray(i) = Asc(text(i))
        Next
        Return textArray
    End Function

    'Parse a string of 2-digit hexadecimal numbers separated by spaces
    'Evaluates each element and converts it to its decimal value in bytes
    'Returns the array of decimal numbers as bytes
    Function HexToByteArray(cipherhex As String, size As Integer) As Byte()
        Dim decArray(size) As Byte
        Dim currentBlock As String
        Dim i As Integer
        Dim k As Integer
        While i < 46
            currentBlock = ""
            currentBlock = cipherhex(i) + cipherhex(i + 1)
            'Convert two digits to ASCII in blocks
            currentBlock = cipherhex(i) + cipherhex(i + 1)
            i += 3
            'Modular counter for 16-char list
            If i <> 0 Then
                k = (i / 3) - 1
            Else
                k = 0
            End If
            decArray(k) = Convert.ToInt32(currentBlock, 16)
        End While
        Return decArray
    End Function

    'Input a decimal ASCII value
    'Converts it to binary string and performs circular left shift of string of 2 bytes
    'Returns the value of the shifted binary number in decimal
    Function CircularLeftShift(input As Byte, howMany As Integer)
        Dim bin As String
        Dim binArray(7) As Char
        bin = Int32.Parse(Convert.ToString(input, 2)).ToString("00000000")
        For i = 1 To howMany
            For k = 0 To 7
                Select Case k
                    Case 7
                        binArray(k) = bin(0)
                    Case Else
                        binArray(k) = bin(k + 1)
                End Select
            Next
            bin = ""
            For n = 0 To 7
                bin += binArray(n)
            Next
        Next
        Return Convert.ToInt32(bin, 2)
    End Function

    'Input a decimal ASCII value
    'Converts it to binary string and performs circular right shift of string of 2 bytes
    'Returns the value of the shifted binary number in decimal
    Function CircularRightShift(input As Byte, howMany As Integer)
        Dim bin As String
        Dim binArray(7) As Char
        bin = Int32.Parse(Convert.ToString(input, 2)).ToString("00000000")
        For i = 1 To howMany
            For k = 0 To 7
                Select Case k
                    Case 0
                        binArray(k) = bin(7)
                    Case Else
                        binArray(k) = bin(k - 1)
                End Select
            Next
            bin = ""
            For n = 0 To 7
                bin += binArray(n)
            Next
        Next
        Return Convert.ToInt32(bin, 2)
    End Function

    'Input a 4x4 array of bytes
    'Creates an empty array of same dimensions, and fills it with swapped elements from input array
    'Returns the the array of bytes as the transposed array of the input
    Function Transpose2DByteArray(array(,) As Byte) As Byte(,)
        Dim currentItem As Byte
        Dim transposedItem As Byte
        Dim transposedArray(3, 3) As Byte
        'For k = 0 To iterations - 1
        'Create array containing transposed values
        For i = 0 To 3
            currentItem = 0
            transposedItem = 0
            'Console.WriteLine("Column " + Str(i + 1))
            For j = 0 To 3
                currentItem = array(i, j)
                transposedItem = array(j, i)
                transposedArray(i, j) = transposedItem
            Next
            NewLine()
        Next
        'Populate the array with the new values from the transposed array
        For i = 0 To 3
            For j = 0 To 3
                'Console.WriteLine("Element" + Str(j + 1) + " before:" + Str(array(i, j)))
                array(i, j) = transposedArray(i, j)
                'Console.WriteLine("Element" + Str(j + 1) + " after:" + Str(array(i, j)))
            Next
            NewLine()
        Next
        'Next
        Return array
    End Function

    '-------------------------------------------------------
    'Main structural procedures
    '-------------------------------------------------------

    'Notes:
    '- Output of encryption/decryption will always be an array of hex numbers
    '- Array can be converted to a string for user output
    '- By using hex, values won't be lost for numbers out of the ASCII range

    'Input an ASCII array e.g. plaintext or ciphertext
    'Convert characters to bytes and input to state matrix
    'Returns the populated 2D state matrix
    Function FillState(messageArray As Byte()) As Byte(,)
        NewLine()
        Dim k As Integer
        For i = 0 To 3
            For j = 0 To 3
                state(i, j) = messageArray(k)
                k += 1
            Next
            NewLine()
        Next
        Return state
    End Function

    'Input key's ASCII array
    'Work out how many duplicates are needed to provide enough bytes for the required round keys
    'Divides duplicates into round keys as length of original key might not match dimensions of state
    'Returns the 2D expanded key array, containing enough 16-character round keys
    Function ExpandKey(keyArray() As Byte) As Byte(,)
        'Initialize expanded key holding enough round keys as there are rounds
        Dim expandedKey(rounds - 1, 15) As Byte

        'Determine how many duplicates of the key are needed
        Dim duplicates As Integer
        Select Case keySize
            Case 16
                duplicates = 10 '(10*16)/16 = 10 round keys
            Case 24
                duplicates = 8 '(8*24)/16 = 12 round keys
            Case 32
                duplicates = 8 '(8*32)/16 = 14 round keys
        End Select

        'Calculate the capacity of the expanded key by total number of elements
        Dim totalBytes As Integer
        totalBytes = keySize * duplicates

        'Duplicate the key until enough bytes exist to be divided into round keys
        Dim duplicate(totalBytes - 1) As Byte
        Dim k As Integer
        For i = 0 To duplicates - 1
            For j = 0 To keySize - 1
                duplicate(k) = keyArray(j)
                k += 1
            Next
        Next

        'Divide the duplicates into 16 character groups, to fill the expanded key with round keys
        'Each round key must be unique and different to the other round keys
        'This process must be repeatable with the same key, for decryption
        Dim l As Integer
        For i = 0 To rounds - 1
            For j = 0 To 15
                'Starting value
                expandedKey(i, j) = duplicate(l)

                'Key expansion process begins
                'These operations are designed to keep the number within 1 byte

                'Zero values must never be null
                If expandedKey(i, j) = 0 Then
                    expandedKey(i, j) = i + j
                End If

                'Perform differential operations

                'XOR with neighbour values
                If j > 0 Then
                    expandedKey(i, j) = expandedKey(i, j) Xor expandedKey(i, j - 1)
                Else
                    expandedKey(i, j) = expandedKey(i, j) Xor expandedKey(i, j + 1)
                End If

                'Add j to the value
                expandedKey(i, j) += j

                'Circular left shift of i bits
                expandedKey(i, j) = CircularLeftShift(expandedKey(i, j), i)

                'Establish upper and lower bounds to keep in range
                'Don't use this for main encryption as it can't be reversed
                'It's important these expressions are separate, as the second corrects the first
                If expandedKey(i, j) > 127 Then
                    expandedKey(i, j) -= 127
                End If
                If expandedKey(i, j) < 33 Then
                    expandedKey(i, j) += 33
                End If

                'Console.WriteLine(Str(duplicate(l)) + " >>" + Str(expandedKey(i, j)))
                'Console.WriteLine((Int32.Parse(Convert.ToString(duplicate(l), 2)).ToString("00000000") + " >> " + Int32.Parse(Convert.ToString((expandedKey(i, j)), 2)).ToString("00000000")))
                l += 1
            Next
        Next
        Return expandedKey
    End Function

    'Input state and expanded key as ASCII arrays, rounds as integer
    'Performs the encryption procedure as many times as the rounds specified
    'Returns a string of hexadecimal numbers, to preserve the ASCII values 
    Function Encryption(state As Byte(,), expandedKey As Byte(,), rounds As Integer) As String()
        Console.WriteLine("The modified state matrix holds the encrypted message:")

        'Transpose the matrix once
        state = Transpose2DByteArray(state)

        For r = 0 To rounds - 1
            Console.WriteLine("Round" + Str(r + 1))
            For i = 0 To 3
                Console.WriteLine("Column" + Str(i + 1) + ":")
                For j = 0 To 3

                    'XOR with round key
                    state(i, j) = state(i, j) Xor expandedKey(r, j)

                    'Make element unique by XORing with S-Box
                    state(i, j) = state(i, j) Xor sbox(i, j)

                    'Circular left shift of 2 bits
                    CircularLeftShift(state(i, j), 2)

                    Console.WriteLine(state(i, j))
                Next
                Console.WriteLine("")
            Next
        Next

        'Return array of 2-digit hexadecimal character strings
        'Overloading ciphertext for contextual return value
        Dim k As Integer
        Dim ciphertext(15) As String
        Dim output As String
        output = ""
        For i = 0 To 3
            For j = 0 To 3
                ciphertext(k) += Hex(state(i, j)).PadLeft(2, "0"c)
                output += ciphertext(k) + " "
                k += 1
            Next
        Next
        Console.WriteLine(output)
        Return ciphertext
    End Function

    'Input a string array of hexadecimal numbers which is parsed, along with the expanded key and rounds used
    'Performes the inverse of the encryption procedure as many times as the rounds specified
    'Returns the original string of the message, which will be identical to the original user input
    Function Decryption(state As Byte(,), expandedKey As Byte(,), rounds As Integer) As String()
        Console.WriteLine("The inversely-modified state matrix holds the decrypted message:")
        For r = 0 To rounds - 1
            Console.WriteLine("Round" + Str(r + 1))
            For i = 0 To 3
                Console.WriteLine("Column" + Str(i + 1) + ":")
                For j = 0 To 3
                    'Reverse order of operations

                    'Circular right shift of 2 bits
                    CircularRightShift(state(i, j), 2)

                    'Make element unique by XORing with S-Box
                    state(i, j) = state(i, j) Xor sbox(3 - i, 3 - j)

                    'XOR with round key
                    state(i, j) = state(i, j) Xor expandedKey(r, j)

                    Console.WriteLine(state(i, j))
                Next
                Console.WriteLine("")
            Next
        Next

        'Reverse transposition
        state = Transpose2DByteArray(state)

        'Return array of 2-digit hexadecimal character strings
        'Overloading ciphertext for contextual return value
        Dim k As Integer
        Dim plaintext(15) As String
        Dim output As String
        output = ""
        For i = 0 To 3
            For j = 0 To 3
                'plaintext(k) += Hex(state(i, j)).PadLeft(2, "0"c)
                plaintext(k) = Chr(state(i, j))
                output += plaintext(k)
                k += 1
            Next
        Next
        Console.WriteLine(output)
        Return plaintext
    End Function

    Sub Main()
        'Start of program
        EnterChoice()

        Console.ReadLine() 'Leave window open
    End Sub

End Module
