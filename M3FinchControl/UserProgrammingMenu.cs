using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3FinchControl
{
    class UserProgrammingMenu : Menu
    {
        public UserProgrammingMenu() : base() 
        {
            commandListWindow = new int[4] { 0, 0, 0, 0 };
            commandsShown = new List<BasicCommand>();
        }

        public override void RefreshMenu(bool fullReload = false)
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
            DisplayActiveCommands();

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
                }
                //else if (key.Key == ConsoleKey.PageUp)
                //{
                //    if (maxCommands > UserProgramming.commandList.Count)
                //    {
                //        //moves the first visible command back one
                //        firstVisibleCommand--;

                //        //ensures the firstVisibleCommand index stays inbounds
                //        if (firstVisibleCommand < 0) firstVisibleCommand = 0;
                //    }
                //}
                //else if (key.Key == ConsoleKey.PageDown)
                //{
                //    //move first visible command forward 1
                    
                //}
                else if (keyChar != '\u0000')
                {
                    //check for the backspace key
                    if (keyChar == (char)8)
                    {
                        //make sure there is input to delete
                        if (inputString.Length > 0)
                        {
                            //remove the last char from the input string
                            inputString = inputString.Remove(inputString.Length - 1);
                        }
                    }
                    else
                    {
                        // add char to the input string
                        inputString += keyChar;
                    }
                }
            }
        }
        private void DisplayActiveCommands()
        {
            //reset commands shown
            List<string> cmdWindowOutput = new List<string>();
            commandsShown.Clear();

            //get visible commands
            if(UserProgramming.commandList.Count() >= maxCommands)
            {
                for (int index = UserProgramming.commandList.Count() - maxCommands; index < UserProgramming.commandList.Count(); ++index)
                {
                    commandsShown.Add(UserProgramming.commandList[index]);
                }
            }
            else
            {
                for (int index = 0; index < UserProgramming.commandList.Count(); ++index)
                {
                    commandsShown.Add(UserProgramming.commandList[index]);
                }
            }

            //set command window output
            foreach (BasicCommand cmd in commandsShown)
            {
                cmdWindowOutput.Add($"{cmd.name.ToString()} : {cmd.modifier}");
            }

            //blank the additional lines
            while (cmdWindowOutput.Count < maxCommands) 
            {
                cmdWindowOutput.Add("                           ");
            }

            //display command list
            for (int i = 0; i < cmdWindowOutput.Count; i++)
            {
                //set the cursor position
                Console.SetCursorPosition(commandListWindow[0], commandListWindow[1] + i);

                //write the info
                Console.Write(cmdWindowOutput[i]);
            }
        }

        public override void LoadTemplate(string templateFilename)
        {
            base.LoadTemplate(templateFilename);

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

            //load the file
            file = System.IO.File.ReadAllLines(templateFilename);
    
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
            if (SearchInfoTag(info, "CommandWindow", out data)[0] != "FAILED_TO_LOCATE") 
            {
                commandListWindow = new int[4] { int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]) };
                maxCommands = commandListWindow[3] - commandListWindow[1];
            }
        }

        List<BasicCommand> commandsShown;
        int maxCommands;
        //int firstVisibleCommand = 0;
        int[] commandListWindow; //left, top, right, bottom
    }
}
