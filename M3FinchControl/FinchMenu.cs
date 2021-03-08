using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinchAPI;

//***************************************************************
//             Title: Finch Control
//  Application Type: Console
//       Description: This app is designed to control the finch
//                    robot using the Finch API
//      File Purpose: This file defines the finch menu class used
//                    through out the rest of the program.
//            Author: Josh Pion
//      Date Created: 2/17/2021
//     Last Modified: 2/26/2021
//***************************************************************

namespace M3FinchControl
{
    #region Finch Menu Class
    /// <summary>
    /// The MenuProperties structue contains the configuation for the Finch Menu
    /// </summary>
    struct MenuProperties
    {
        public int selectorCol;
        public int firstLine;
        public int lastLine;
        public int currentOption;
        public int consoleWidth;
        public int consoleHeight;
        public int outputTop;
        public int outputBottom;
        public int outputLeft;
        public int outputRight;
        public int maxCharPerLine;
        public int maxOutputChars;
    }
    class Menu
    {
        public Menu()
        {
            properties.selectorCol = 0;
            properties.firstLine = 0;
            properties.lastLine = 0;
            properties.currentOption = 0;
            properties.consoleHeight = 25;
            properties.consoleWidth = 80;
            properties.maxCharPerLine = 80;
            properties.maxOutputChars = 2000;
            selectionIndicator = "*";
            inputString = "";
            outputString = inputString;
            formatedOutput = new string[1];
            enterKeyPressed = false;
            onHoverUpdate = false;
            selectedOption = "";
            previousSelection = 0;
        }
        public void SetFinchRobot(Finch finch)
        {
            myFinch = finch;
        }
        public void RecallPreviousSelection()
        {
            //clear the option selector
            Console.SetCursorPosition(properties.selectorCol, properties.firstLine + properties.currentOption);
            for (int curChar = 0; curChar < selectionIndicator.Length; ++curChar)
                Console.Write(" ");

            //update all the nessassary bs...
            properties.currentOption = previousSelection;
            selectedOption = optionIDs[properties.currentOption];
        }
        #region Menu Template Loading Method
        /// <summary>
        /// this function is used to load a preformated menu template. Using tags contained in the template
        /// it can configures the various other data members of the class. At least thats what I was hoping 
        /// when I wrote this...
        /// </summary>
        /// <param name="templateFilename">File path and name for a valid preformated menu template</param>
        public void LoadTemplate(string templateFilename)
        {
            //
            // variables
            //
            string[] file = { };
            string[] info = { };
            string[] data = { };
            int startLine = 0;
            int endLine = 0;

            //
            // Constants
            //
            const string INFO_OPEN = "<info>";
            const string INFO_CLOSE = "</info>";
            const string MENU_OPEN = "<menu>";
            const string MENU_CLOSE = "</menu>";

            //load the file
            file = System.IO.File.ReadAllLines(templateFilename);

            //locate the begining and end of the menu section
            for (int i = 0; i < file.Length; ++i)
            {
                //find the line containing the <menu> tag
                if (file[i].Contains(MENU_OPEN))
                {
                    startLine = i + 1;
                }
                //find the line containing the </menu> tag
                else if (file[i].Contains(MENU_CLOSE))
                {
                    endLine = i;
                    break;
                }
            }

            //copy the menu section to the template
            Array.Resize(ref template, endLine - startLine);
            Array.Copy(file, startLine, template, 0, endLine - startLine);

            //set the window width/height
            properties.consoleHeight = template.Length;
            properties.consoleWidth = template[0].Length;

            //locate the begining and end of the info section
            for (int i = 0; i < file.Length; ++i)
            {
                //find the line containing the <info> tag
                if (file[i].Contains(INFO_OPEN))
                {
                    startLine = i + 1;
                }
                //find the line containing the </menu> tag
                else if (file[i].Contains(INFO_CLOSE))
                {
                    endLine = i;
                    break;
                }
            }

            //separate the info section
            Array.Resize(ref info, endLine - startLine);
            Array.Copy(file, startLine, info, 0, endLine - startLine);

            //pull data from the info section and assign it to their variables

            if (SearchInfoTag(info, "selectorCol", out data)[0] != "FAILED_TO_LOCATE")
            {
                int.TryParse(data[0], out properties.selectorCol);
            }
            if (SearchInfoTag(info, "firstLine", out data)[0] != "FAILED_TO_LOCATE")
            {
                int.TryParse(data[0], out properties.firstLine);
            }
            if (SearchInfoTag(info, "selectionIndicator", out data)[0] != "FAILED_TO_LOCATE")
            {
                selectionIndicator = data[0];
            }
            if (SearchInfoTag(info, "output", out data)[0] != "FAILED_TO_LOCATE")
            {
                //verify the output region is valid the assign the variables
                if (VerifyOutputRegion(data))
                {
                    //set output region
                    properties.outputLeft = int.Parse(data[0]);
                    properties.outputTop = int.Parse(data[1]);
                    properties.outputRight = int.Parse(data[2]);
                    properties.outputBottom = int.Parse(data[3]);

                    //set output region max lines
                    Array.Resize(ref formatedOutput, properties.outputBottom - properties.outputTop + 1);

                    //set the max number of output characters
                    properties.maxCharPerLine = properties.outputRight - properties.outputLeft + 1;
                    properties.maxOutputChars = properties.maxCharPerLine * formatedOutput.Length;
                }
            }
            if (SearchInfoTag(info, "options", out data)[0] != "FAILED_TO_LOCATE")
            {
                //assign the option IDs
                optionIDs = data;

                //configure last option based on where the number of options availiable 
                properties.lastLine = properties.firstLine + optionIDs.Length - 1;
            }
        }
        #endregion

        /// <summary>
        /// This function examines the data passed to it and determines if it contains a valid set of points defining an output area in the console
        /// </summary>
        /// <param name="data">Data loaded from the info section of the configuration file</param>
        /// <returns>A boolean value indicating whether the data strings provided create a valid output area or not</returns>
        private bool VerifyOutputRegion(string[] data)
        {
            //
            // variables
            //

            MenuProperties tempCopy = properties;
            bool topBottomVerified = false;
            bool leftRightVerified = false;
            bool[] validNumbers = { false, false, false, false };

            //make sure all points were loaded
            if (data.Length == 4)
            {
                //copy the data
                validNumbers[0] = int.TryParse(data[0], out tempCopy.outputLeft);
                validNumbers[1] = int.TryParse(data[1], out tempCopy.outputTop);
                validNumbers[2] = int.TryParse(data[2], out tempCopy.outputRight);
                validNumbers[3] = int.TryParse(data[3], out tempCopy.outputBottom);

                //make sure all fields contained numerical data
                if (!validNumbers[0] & !validNumbers[1] & !validNumbers[2] & !validNumbers[3])
                    return false;

                //verify data is logical (inbounds and not conflicting with each other)
                //check top and bottom parameters
                //first check: is it beyond the consoleHeight parameter?
                if (tempCopy.outputBottom < properties.consoleHeight & tempCopy.outputTop < properties.consoleHeight)
                {
                    //second check: is it greater than 0?
                    if (tempCopy.outputTop > 0 & tempCopy.outputBottom > 0)
                    {
                        //third check: is outputTop less than outputBottom?
                        if (tempCopy.outputTop <= tempCopy.outputBottom - 2)
                        {
                            //outputTop and outputBottom are now verified
                            topBottomVerified = true;
                        }
                    }
                }
                //check left and right parameters
                //first check: is it beyond the consoleWidth parameter
                if (tempCopy.outputLeft < properties.consoleWidth & tempCopy.outputRight < properties.consoleWidth)
                {
                    //second check: is it greater than 0?
                    if (tempCopy.outputLeft > 0 & tempCopy.outputRight > 0)
                    {
                        //third check: Is outputLeft less than output right?
                        if (tempCopy.outputLeft <= tempCopy.outputRight - 50)
                        {
                            leftRightVerified = true;
                        }
                    }
                }
            }

            //return true if all checks pass
            return leftRightVerified & topBottomVerified;
        }

        #region Info search functions
        /// <summary>
        /// Used to search an information string array for a specific data tag and extract the data from that tag
        /// </summary>
        /// <param name="searchField">This is the string array containing the information field being searched</param>
        /// <param name="tag">This is the data tag being searched for (don't include the '=')</param>
        /// <param name="dataOut">This is a redundant method of returning the results</param>
        /// <returns>method returns an array containing all individual fields of data contained in the data tag</returns>
        private string[] SearchInfoTag(string[] searchField, string tag, out string[] dataOut)
        {
            //
            // Variables
            //
            string[] localResult = { }; //stores the result locally to return later
            string data = "FAILED_TO_LOCATE";
            int[] dataStart = { 0, 0 };
            int[] dataEnd = { 0, 0 };
            bool dataFound = false;

            //
            // Constants
            //
            const int LINE = 0;
            const int CHAR = 1;

            //loop through each line searching for the tag to extract the data
            for (int i = 0; i < searchField.Length; ++i)
            {
                //when the tag is found search for the first { following it
                if (searchField[i].Contains(tag + "="))
                {
                    //clear data
                    data = "";

                    for (int j = i; j < searchField.Length; ++j)
                    {
                        for (int k = 0; k < searchField[j].Length; ++k)
                        {
                            //finds the starting line and character of the data to be extracted
                            if (searchField[j][k] == '{')
                            {
                                dataStart[LINE] = j;
                                dataStart[CHAR] = k;
                            }
                            //finds the ending line and character of the data to be extracted
                            else if (searchField[j][k] == '}')
                            {
                                dataEnd[LINE] = j;
                                dataEnd[CHAR] = k;
                                dataFound = true;
                                break;
                            }
                        }

                        if (dataFound)
                            break;
                    }
                }
                else if (dataFound)
                    break;
            }

            //reset dataFound
            dataFound = false;

            //begin data extraction loop(s)
            //if all data is on the same line, search only that line
            if (dataStart[LINE] == dataEnd[LINE])
            {
                for (int i = dataStart[CHAR] + 1; i < dataEnd[CHAR]; ++i)
                {
                    //check for new line character
                    if (searchField[dataStart[LINE]][i] == ',')
                    {
                        //add current data to the localResult
                        Array.Resize(ref localResult, localResult.Length + 1);
                        localResult[localResult.Length - 1] = data;

                        //reset data to accept additional input
                        data = "";
                    }
                    //if no newline character was found, add current char to data string
                    else
                    {
                        if (searchField[dataStart[LINE]][i] != ' ' & searchField[dataStart[LINE]][i] != '\t')
                            data += searchField[dataStart[LINE]][i];
                    }
                }
            }
            //if the data is on multiple lines, begin multiline search
            else
            {
                for (int i = dataStart[LINE]; i <= dataEnd[LINE]; ++i)
                {
                    for (int j = 0; j < searchField[i].Length; ++j)
                    {
                        //if this is the first line advance to the starting char
                        if (i == dataStart[LINE] & j < dataStart[CHAR])
                            j = dataStart[CHAR];
                        //if this is the last line and char exit data extraction loop
                        else if (i == dataEnd[LINE] & j == dataEnd[CHAR])
                        {
                            dataFound = true;
                            break;
                        }
                        //if the new line char is found, add data to localResult and reset data
                        else if (searchField[i][j] == ',')
                        {
                            Array.Resize(ref localResult, localResult.Length + 1);
                            localResult[localResult.Length - 1] = data;
                            data = "";
                        }
                        //add current char to data
                        else
                        {
                            if (searchField[i][j] != ' ' & searchField[i][j] != '\t')
                                data += searchField[i][j];
                        }
                    }

                    if (dataFound)
                        break;
                }
            }

            //add data to the localResult
            Array.Resize(ref localResult, localResult.Length + 1);
            localResult[localResult.Length - 1] = data;

            //return results
            dataOut = localResult;
            return localResult;
        }
        #endregion

        #region Menu Refresh method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullReload"></param>
        public void RefreshMenu(bool fullReload = false)
        {
            // *************
            // * Variables *
            // *************
            ConsoleKeyInfo key;
            char keyChar;

            //if the calling method requested a full reload, reload menu from template
            if (fullReload)
            {
                //reset the selector position
                properties.currentOption = 0;
                selectedOption = optionIDs[properties.currentOption];

                //clear the screen set on hover update to true
                Console.Clear();
                onHoverUpdate = true;

                //set console config
                Console.SetCursorPosition(0, 0);
                Console.WindowHeight = properties.consoleHeight;
                Console.WindowWidth = properties.consoleWidth;
                Console.CursorVisible = false;
                Console.BufferWidth = properties.consoleWidth;

                //output the template file to the console
                for (int i = 0; i < template.Length; ++i)
                {
                    Console.Write(template[i]);
                }

                //move the cursor back to 0,0
                Console.SetCursorPosition(0, 0);
            }
            else
            {
                onHoverUpdate = false;
            }
            //line the cursor back up
            Console.SetCursorPosition(properties.selectorCol, properties.firstLine + properties.currentOption);

            //write the selection indicator to the appropriate location
            Console.Write(selectionIndicator);

            //display current output
            DisplayOutput();

            // **************************************
            // * check for user input and act on it *
            // **************************************

            //reset menu option selected
            enterKeyPressed = false;

            //if the user pressed a key, read their input
            if (Console.KeyAvailable)
            {
                //gets the key that was pressed
                key = Console.ReadKey(true);
                keyChar = key.KeyChar;

                //check if the key was one of the arrows
                if (key.Key == ConsoleKey.UpArrow)
                {
                    // displays an error if the user is already at the top of the menu
                    if (properties.currentOption == 0)
                    {
                        Console.Beep();
                    }
                    else
                    {
                        //clear the option selector
                        Console.SetCursorPosition(properties.selectorCol, properties.firstLine + properties.currentOption);
                        for (int curChar = 0; curChar < selectionIndicator.Length; ++curChar)
                            Console.Write(" ");

                        //move selected option back
                        properties.currentOption--;

                        //set hover indcator to true and clear output
                        Clear();
                        onHoverUpdate = true;

                        //update selected option
                        selectedOption = optionIDs[properties.currentOption];

                        //if selected option is blank, skip it
                        while (selectedOption == "__NULL_LINE__")
                        {
                            properties.currentOption--;
                            selectedOption = optionIDs[properties.currentOption];
                            previousSelection = properties.currentOption;
                        }
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (properties.currentOption == properties.lastLine - properties.firstLine)
                    {
                        Console.Beep();
                    }
                    else
                    {
                        //clear the option selector
                        Console.SetCursorPosition(properties.selectorCol, properties.firstLine + properties.currentOption);
                        for (int curChar = 0; curChar < selectionIndicator.Length; ++curChar)
                            Console.Write(" ");

                        //advance selected option
                        properties.currentOption++;

                        //set hover indcator to true and clear output
                        Clear();
                        onHoverUpdate = true;

                        //update selected option
                        selectedOption = optionIDs[properties.currentOption];

                        //if selected option is blank, skip it
                        while (selectedOption == "__NULL_LINE__")
                        {
                            properties.currentOption++;
                            selectedOption = optionIDs[properties.currentOption];
                            previousSelection = properties.currentOption;
                        }
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    enterKeyPressed = true;
                    //inputString = "";
                }
                else if (keyChar != '\u0000')
                {
                    //check for the backspace key
                    if (keyChar == (char)8)
                    {
                        //make sure there is input to delete
                        if (inputString.Length > 0)
                        {
                            //remove previous input string from the output string;
                            int inputStringIndex = outputString.Length - inputString.Length;
                            if (inputStringIndex >= 0 & outputString.Length != 0 & inputString.Length > 0)
                                outputString = outputString.Remove(inputStringIndex);

                            //remove the last char from the input string
                            inputString = inputString.Remove(inputString.Length - 1);
                        }
                    }
                    else
                    {
                        //remove previous input string from the output string;
                        int inputStringIndex = outputString.Length - inputString.Length;
                        if (inputStringIndex >= 0 & outputString.Length != 0 & inputString.Length > 0)
                            outputString = outputString.Remove(inputStringIndex);

                        // add char to the input string
                        inputString += keyChar;

                    }
                }
            }
        }
        #endregion

        private void DisplayOutput()
        {
            //remove previous input string from the output string;
            int inputStringIndex = outputString.Length - inputString.Length;
            if (inputStringIndex >= 0 & outputString.Length != 0 & inputString.Length > 0 & inputString.Length > 0)
            {
                outputString = outputString.Remove(inputStringIndex);
            }

            //add the input string to the end of the output string
            outputString += inputString;

            //variable containing the currently formating line
            string line = "";

            int curChar = 0;
            int charsParsed = 0;

            //find the max output size
            int maxOutputLines = properties.outputBottom - properties.outputTop + 1;

            //clear the formatedOutput array
            for (int lineNum = 0; lineNum < formatedOutput.Length; ++lineNum)
            {
                formatedOutput[lineNum] = line;
            }
            //begin output string format loop
            for (int lineNum = 0; lineNum < formatedOutput.Length; ++lineNum)
            {
                //reset the line
                line = "";

                while (curChar < outputString.Length)
                {
                    //breaks loop when a logical break point is found near the end of the line
                    if (curChar - charsParsed == properties.maxCharPerLine - 1)
                    {
                        break;
                    }
                    else if (outputString[curChar] == '\n')
                    {
                        curChar++;
                        break;
                    }
                    else
                    {
                        line += outputString[curChar];
                    }

                    //advance curChar
                    curChar++;
                }

                //tells the next cycle where the system stopped looking for breaks
                charsParsed = curChar;

                //fill all remaining chars with spaces
                for (int i = line.Length - 1; i < properties.maxCharPerLine; ++i)
                {
                    line += " ";
                }

                //add current line to array
                formatedOutput[lineNum] = line;

                //if the output exceeds the max output area, discard first line and move other lines up
                if (lineNum == formatedOutput.Length - 1 & charsParsed < outputString.Length)
                {
                    for (int i = 0; i < formatedOutput.Length - 1; ++i)
                    {
                        formatedOutput[i] = formatedOutput[i + 1];
                    }

                    //decrement lineNum to continue parsing 
                    --lineNum;
                }
            }

            // *******************************************
            // * write the output string to the console. *
            // *******************************************

            //reset the cursor position
            Console.CursorVisible = false;
            Console.SetCursorPosition(properties.outputLeft, properties.outputTop);

            //display the output
            for (int lineNum = 0; lineNum < formatedOutput.Length; ++lineNum)
            {
                Console.CursorLeft = properties.outputLeft;
                Console.WriteLine(formatedOutput[lineNum]);
            }
        }

        #region Menu IO
        /// <summary>
        /// Simple function that clears the output string, works just like Console.Clear but only effects the menu IO area
        /// </summary>
        public void Clear()
        {
            //reset output string
            outputString = "";

            //reset input string
            inputString = "";

            //clear output console area
            Console.SetCursorPosition(properties.outputLeft, properties.outputTop);

            for (int line = 0; line < formatedOutput.Length; ++line)
            {
                Console.CursorTop = properties.outputTop + line;

                for (int curChar = 0; curChar < properties.maxCharPerLine; ++curChar)
                {
                    Console.CursorLeft = properties.outputLeft + curChar;
                    Console.Write(" ");
                }
            }
        }
        /// <summary>
        /// Simple function that writes a string to the output area inline, works just like Console.Write but only effects the menu IO area
        /// </summary>
        /// <param name="line">Line to be written to the menu output string</param>
        public void Write(string line)
        {
            outputString += line;

            DisplayOutput();
        }
        /// <summary>
        /// Simple function that writes a string to the output area with an end of line char at the end, works just like Console.WriteLine but only effects the menu IO area
        /// </summary>
        /// <param name="line">Line to be written to the menu output string<</param>
        /// <param name="spamGuard">prevents duplicate messages from being written to the output<</param>
        public void WriteLine(string line, bool spamGuard = false)
        {
            if(spamGuard & outputString.Contains(line))
            {
                return;
            }

            outputString += line + '\n';

            DisplayOutput();
        }
        public string ReadLine()
        {
            // *************
            // * Variables *
            // *************
            ConsoleKeyInfo key;
            char keyChar;
            bool gatheringInput = true;
            string methodOutput = "";

            while (gatheringInput)
            {
                //gets the key that was pressed
                key = Console.ReadKey(true);
                keyChar = key.KeyChar;

                if (key.Key == ConsoleKey.Enter)
                {
                    gatheringInput = false;
                }
                else if (keyChar != '\u0000')
                {
                    //check for the backspace key
                    if (keyChar == (char)8)
                    {
                        if (inputString.Length > 0)
                        {
                            //remove previous input string from the output string;
                            if (inputString != "")
                            {
                                int inputStringIndex = outputString.Length - inputString.Length;
                                if (inputStringIndex >= 0 & outputString.Length != 0 & inputString.Length > 0)
                                {
                                    outputString = outputString.Remove(inputStringIndex);
                                }
                            }

                            //remove the last char and the input identifier from the input string
                            inputString = inputString.Remove(inputString.Length -1);
                        }
                    }
                    //some valid char was pressed
                    else
                    {
                        //remove previous input string from the output string;
                        if (inputString != "")
                        {
                            int inputStringIndex = outputString.Length - inputString.Length;
                            if (inputStringIndex >= 0 & outputString.Length != 0 & inputString.Length > 0)
                            {
                                outputString = outputString.Remove(inputStringIndex);
                            }
                        }

                        // add char to the input string
                        inputString += keyChar;

                    }
                }
                //add the input string to the output string
                outputString += inputString;

                DisplayOutput();
            }

            methodOutput = inputString;
            inputString = "";
            return methodOutput;
        }
        #endregion

        //data members? not sure exactly what these are called in C#
        public static Finch myFinch; //static data member used to store the Finch robot object
        private MenuProperties properties;
        private string[] template;
        private string[] optionIDs;
        private string[] formatedOutput;
        private string selectionIndicator;
        private string outputString;
        private int previousSelection;
        public string inputString
        {
            get;
            private set;
        }
        public bool enterKeyPressed
        {
            get;
            private set;
        }
        public string selectedOption
        {
            get;
            private set;
        }
        public bool onHoverUpdate
        {
            get;
            private set;
        }
    }
    #endregion
}