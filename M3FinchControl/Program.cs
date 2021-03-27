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
//            Author: Josh Pion
//      Date Created: 2/13/2021
//     Last Modified: 2/19/2021
//***************************************************************

namespace M3FinchControl
{
    class Program
    {
        // **********************************
        // * Global Program Class Variables *
        // **********************************
        static public Menu[] menus
        {
            get;
            private set;
        }
        static public Finch myFinch
        {
            get;
            private set;
        }
        static DataRecorder recorder = new DataRecorder();
        static string name = "";
        static bool finchConnected = false;
        static bool running = true;
        static public int currentMenu;

        enum title
        {
            main,
            connect,
            talentShow,
            recorderMenu,
            recorderLogsMenu,
            alarmSystem,
            userProgramming,
            themes
        }
        static void Main(string[] args)
        {
            //initilaize variables
            myFinch = new Finch();
            currentMenu = 0;
            menus = new Menu[Enum.GetNames(typeof(title)).Length];

            //create the menu instances
            for (int menuNum = 0; menuNum < Enum.GetNames(typeof(title)).Length; ++menuNum)
            {
                menus[menuNum] = new Menu();
            }

            // *********************
            // * Main menu section *
            // *********************

            //configure console color
            ////future revisions may also put this in the config (.jp) file
            //Console.ForegroundColor = ConsoleColor.DarkYellow;
            Menu.defaultForeground = ConsoleColor.DarkYellow;
            Menu.defaultBackground = ConsoleColor.Black;
            
            //load menus
            menus[(int)title.main].LoadTemplate("config\\MainMenu.txt");
            menus[(int)title.connect].LoadTemplate("config\\ConnectFinch.txt");
            menus[(int)title.talentShow].LoadTemplate("config\\TalentShow.txt");
            menus[(int)title.recorderMenu].LoadTemplate("config\\DataRecorder.txt");
            menus[(int)title.recorderLogsMenu].LoadTemplate("config\\ViewDataRecords.txt");
            menus[(int)title.alarmSystem].LoadTemplate("config\\AlarmSystem.txt");
            menus[(int)title.themes].LoadTemplate("config\\Themes.txt");

            //load the special menu class for the user programming menu
            menus[(int)title.userProgramming] = new UserProgrammingMenu();
            menus[(int)title.userProgramming].LoadTemplate("config\\UserProgramming.txt");

            //load the theme
            ThemeManager.LoadTheme();

            //refresh main menu
            menus[(int)title.main].RefreshMenu(true);


            while (running) 
            {
                //checks to see if the user moved the selection cursor
                if (menus[currentMenu].onHoverUpdate)
                {
                    //reads the current menu option and outputs hover info if there is any.
                    MenuNavigation(false);
                }

                //updates input/output of the menu
                menus[currentMenu].RefreshMenu();

                //checks if user pressed the enter key
                if (menus[currentMenu].enterKeyPressed)
                {
                    //clears the IO
                    menus[currentMenu].Clear();

                    //reads the option pressed by the user and acts on it.
                    MenuNavigation();
                }
            }

            //save the theme to the settings file
            ThemeManager.saveTheme();
        }

        #region Basic Finch Controls
        static void PlayNote(int note, int waitDuring, int waitAfter)
        {
            myFinch.noteOn(note);
            myFinch.wait(waitDuring);
            myFinch.noteOff();
            myFinch.wait(waitAfter);
        }

        static public int CelsiusToFahrenheit(double tempC)
        {
            return (int)(tempC * 1.8 + 32);
        }
        #endregion

        #region Menu Navigation Methods
        static public bool ValidateInput(string prompt, string error = "Invalid Input: Enter 'yes' or 'no' press any key to continue...")
        {
            string validatedInput;

            validatedInput = ValidateInput(prompt, new string[] { "yes", "no", "n", "y" }, error);

            return (validatedInput == "yes" | validatedInput == "y");  
        }
        static public string ValidateInput(string prompt, string[] validInput, string error = "Invalid Input: Press any key to continue...")
        {
            bool validing = true;
            string userInput = "";
            int index = 0;

            while (validing)
            {
                //display prompt
                menus[currentMenu].Clear();
                menus[currentMenu].Write(prompt);
                userInput = menus[currentMenu].ReadLine();

                //check if input matches a valid input
                for (index = 0; index < validInput.Length; ++index)
                {
                    if (validInput[index].ToLower() == userInput.ToLower())
                    {
                        validing = false;
                        break;
                    }
                }

                //if a valid input is not found display an error message.
                if (validing)
                {
                    menus[currentMenu].WriteLine("\n" + error);
                    Console.ReadKey();
                }
            }

            //return the valid input that was identified
            return validInput[index];
        }
        static public int ValidateInput(string prompt, int max, bool includeMax = false, int min = 0, bool includeMin = true)
        {
            int output = 0;
            string userInput = "";
            string error = "\n\nINVALID INPUT: Please enter a whole number between ";
            bool invalidInput = false;

            //configure error message
            if (includeMin)
            {
                error += min.ToString() + "-";
            }
            else
            {
                error += (min + 1).ToString() + "-";
            }

            if (includeMax)
            {
                error += max.ToString() + ") ";
            }
            else
            {
                error += (max + 1).ToString() + ") ";
            }

            error += "\n\n Press any key to continue...";

            //a series of random characters for the loop to look for. I made it random so it's nearly imposible to type it in and skip validation
            const string VALIDATION_TRIGGER = "EuntufeLKJEFOIHJOFEohEOFhuohnEFKhEKJNEKHJkEFFNjohjEFEOUHonEFKjnEFOUhEFNKFJEHIUHFOENUIFGYIOIHJ$U&Y" +
                "^&RY&YHIFUNIUGHE(*&&GHFIUh97ye79efh78h97&FHIUeghg7huiwfiyh3HO*HF#NFUH(WHEIUWFNIH7iebnwKUHI&hiu3EFBNKUHI&h#FBIUKBfKUGHoiHOWNFOUh" +
                "iugufhEUBHfoiuhvuikBikukhvikBIUDfhfiunkjhsiufehubFibkhfbkuehkjfbKHKfhiukeb"; 

            while(userInput != VALIDATION_TRIGGER)
            {
                //reset invalidInput
                invalidInput = false;

                //display prompt
                menus[currentMenu].Clear();
                menus[currentMenu].Write(prompt);
                userInput = menus[currentMenu].ReadLine();

                //convert to an int
                if (int.TryParse(userInput, out output))
                {
                    //check if it exceeds the high threshold
                    if (includeMax)
                    {
                        if (output > max) 
                        {
                            invalidInput = true;
                        }
                    }
                    else
                    {
                        if (output >= max)
                        {
                            invalidInput = true;
                        }
                    }

                    //check if it exceeds the low threshold
                    if(includeMin)
                    {
                        if (output < min)
                        {
                            invalidInput = true;
                        }
                    }
                    else
                    {
                        if (output <= min)
                        {
                            invalidInput = true;
                        }
                    }

                    if(!invalidInput)
                    {
                        userInput = VALIDATION_TRIGGER;
                    }
                    else
                    {
                        menus[currentMenu].WriteLine(error);
                        Console.ReadKey();
                    }
                }
                else
                {
                    menus[currentMenu].WriteLine(error);
                    Console.ReadKey();
                }
            }

            return output;
        }
        static public string ValidateInput(string prompt, char[] illegalCharacters)
        {
            bool validing = true;
            bool illegalCharFound = false;
            string userInput = "";
            char illegalChar = ' ';

            while (validing)
            {
                //reset illegalCharFound
                illegalCharFound = false;

                menus[currentMenu].Clear();
                menus[currentMenu].Write(prompt);
                userInput = menus[currentMenu].ReadLine();

                //check if input matches a valid input
                for (int index = 0; index < userInput.Length; ++index)
                {
                    if(!illegalCharFound)
                    {
                        for (int charIndex = 0; charIndex < illegalCharacters.Length; ++charIndex) 
                        {
                            if(userInput[index] == illegalCharacters[charIndex])
                            {
                                illegalChar = illegalCharacters[charIndex];
                                illegalCharFound = true;
                                break;
                            }
                        }
                    }
                    //if the loop finished without finding any illegal chars validation is now complete
                    else if (index == userInput.Length - 1)
                    {
                        validing = false;
                    }
                    //stops the search if an illegal char was found. Slight efficiency improvement over continuing search loop.
                    else
                    {
                        break;
                    }
                }

                //if an invalid char was found, display error and start again.
                if (illegalCharFound)
                {
                    menus[currentMenu].WriteLine("\n" + "Invalid Input: illegal character '"+illegalChar +"' detected. \n\nPress any key to try again...");
                    Console.ReadKey();
                }

                validing = illegalCharFound;
            }

            //return the validated input string
            return userInput;
        }
        static void DisplayMenuError(string error = "Invalid selection, Press any key to continue...")
        {
            //clear the menu IO
            menus[currentMenu].Clear();

            //write the line and display it in the output area
            menus[currentMenu].WriteLine(error);

            //wait for user input
            Console.ReadKey();
        }
        /// <summary>
        /// This is the main method that handles all the navagation of the program
        /// when called with the default true parameter it will perform functions 
        /// assuming the user pressed the enter key. When called in alt mode, 
        /// (onSelect false) it will perform the on hover functions.
        /// </summary>
        /// <param name="onSelect">Toggles between normal navagation clicks and alt hover actions (true = click, false = hover)</param>
        static void MenuNavigation(bool onSelect = true)
        {
            //set the cursor to invisible
            menus[currentMenu].showInputCursor = false;

            //display a warning if the finch is not connected and the finches name if it is
            if (!onSelect & !finchConnected & currentMenu != (int)title.themes)
            {
                menus[currentMenu].WriteLine($"{Menu.CHANGE_COLOR}DarkRed;WARNING: finch robot not connected...");
                menus[currentMenu].WriteLine("            finch related functions will not work until");
                menus[currentMenu].WriteLine("            your finch is connected! \n");
                menus[currentMenu].Write($"{Menu.CHANGE_COLOR}default;");
            }
            else if (!onSelect & currentMenu != (int)title.themes)
            {
                menus[currentMenu].WriteLine($"{Menu.CHANGE_COLOR}DarkGreen;{name} is standing by for commands...\n{Menu.CHANGE_COLOR}default;");
            }

            switch (menus[currentMenu].selectedOption)
            {
                // *********************
                // * main menu options *
                // *********************
                case "connectMenu":
                    if (onSelect)
                    {
                        //switch to the connect menu
                        currentMenu = (int)title.connect;
                        menus[(int)title.connect].RefreshMenu(true);
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Go to the Finch connection menu...");
                    }
                    break;                    

                case "talentShowMenu":
                    if (onSelect)
                    {
                        currentMenu = (int)title.talentShow;
                        menus[(int)title.talentShow].RefreshMenu(true);
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Go to the Talent Show menu...");
                        menus[currentMenu].Write("\nObserve the finch participating in one of 3 exciting events!");
                    }
                    break;

                case "dataRecordMenu":
                    if (onSelect)
                    {
                        currentMenu = (int)title.recorderMenu;
                        menus[currentMenu].RefreshMenu(true);
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Go to the Data Recorder menu...");
                        menus[currentMenu].WriteLine("\nUse the finch to record temperatures over a set period of time!");
                    }
                    break;

                // **********************
                // * Connect Finch Menu *
                // **********************
                case "connect":
                    if (onSelect)
                    {
                        //show input cursor
                        menus[currentMenu].showInputCursor = true;

                        name = ConnectFinch();

                        //verify the finch is connected
                        finchConnected = name != "___FAILED_TO_CONNECT___";

                        //if the finch connected successfully, return to the main menu
                        if (finchConnected)
                        {
                            currentMenu = (int)title.main;
                            menus[currentMenu].RefreshMenu(true);
                        }
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Name your finch and attempt to connect to it...");
                    }
                    break;

                // ********************
                // * Talent Show Menu *
                // ********************
                case "lightSound":
                    if (onSelect)
                    {
                        LightAndSound();
                    }
                    else
                    {
                        menus[currentMenu].WriteLine(name + " will light up blue and slowly go from blue to red if warmed up.");
                        menus[currentMenu].WriteLine("if " + name + " warms up all the way it'll even play a little song!");
                    }
                    break;
                case "dance":
                    if (onSelect)
                    {
                        DanceMonkeyDance();
                    }
                    else
                    {
                        menus[currentMenu].WriteLine(name + " will drive in a small square for your amusement!");
                    }
                    break;
                case "mix":
                    if (onSelect)
                    {
                        MixUp();
                    }
                    else
                    {
                        menus[currentMenu].WriteLine(name + " will wiggle back and forth while playing notes and changing it's");
                        menus[currentMenu].WriteLine("face color!");
                    }
                    break;

                // **********************
                // * Data Recorder Menu *
                // **********************
                case "startRecorder":
                    if (onSelect)
                    {
                        recorder.Start(myFinch, ref menus[currentMenu]);
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Start recording data based on the following settings...");
                        menus[currentMenu].WriteLine("                         Data Points: " + recorder.NUMBER_OF_RECORDS.ToString());
                        menus[currentMenu].WriteLine("                               Delay: " + recorder.TIME_BETWEEN_RECORDS.ToString());
                        menus[currentMenu].WriteLine("                         Delay Units: " + recorder.TIME_UNIT);
                        menus[currentMenu].WriteLine("\nPress enter to begin...");
                    }
                    break;

                case "configureRecorder":
                    if (onSelect)
                    {
                        menus[currentMenu].showInputCursor = true;
                        ConfigureRecorder();
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Press Enter to change the configuration.\n");
                        menus[currentMenu].WriteLine("The current configuration is...");
                        menus[currentMenu].WriteLine("                         Data Points: " + recorder.NUMBER_OF_RECORDS.ToString());
                        menus[currentMenu].WriteLine("                               Delay: " + recorder.TIME_BETWEEN_RECORDS.ToString());
                        menus[currentMenu].WriteLine("                         Delay Units: " + recorder.TIME_UNIT);
                    }
                    break;

                case "viewRecords":
                    if (onSelect)
                    {
                        currentMenu = (int)title.recorderLogsMenu;
                        menus[(int)title.recorderLogsMenu].RefreshMenu(true);
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("View previously recorded records...");
                    }
                    break;

                // ***************************
                // * Data Recorder Logs Menu *
                // ***************************
                case "viewLast":
                    if (onSelect)
                    {
                        viewData();
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("If there is a data log available, you can view it here...");

                        if (recorder.dataAccuired) 
                        {
                            menus[currentMenu].WriteLine("\n\nThere currently is a record available to view.");
                        }
                        else
                        {
                            menus[currentMenu].WriteLine("\n\nThere are no records currently avaliable to view.");
                        }
                    }
                    break;

                case "saveDataLog":
                    if (onSelect)
                    {
                        menus[currentMenu].Clear();

                        if (ValidateInput("Would you like export the current data log? "))
                        {
                            menus[currentMenu].Write("\n\nFilename: ");
                            try
                            {
                                menus[currentMenu].showInputCursor = true;
                                recorder.SaveLog($"UserData\\{menus[currentMenu].ReadLine()}.log");
                            }
                            catch (ArgumentException)
                            {
                                menus[currentMenu].showInputCursor = false;
                                menus[currentMenu].WriteLine("\n\nFile already exists press any key to continue...");
                                Console.ReadKey();
                            }

                            menus[currentMenu].showInputCursor = false;
                        }

                        menus[currentMenu].Clear();
                    }
                    else
                    { 
                        menus[currentMenu].WriteLine("Save a data log to file...");
                        if (recorder.dataAccuired)
                        {
                            menus[currentMenu].WriteLine("\n\nThere currently is a record available to save.");
                        }
                        else
                        {
                            menus[currentMenu].WriteLine("\n\nThere are no records currently avaliable to save.");
                        }
                    }
                    break;

                case "loadDataLog":
                    if (onSelect)
                    {
                        menus[currentMenu].Clear();

                        if (ValidateInput("Would you like load a data log from file? "))
                        {
                            string[] logFromFile = null;

                            menus[currentMenu].Write("\n\nFilename: ");
                            try
                            {
                                menus[currentMenu].showInputCursor = true;
                                logFromFile = recorder.LoadLog($"UserData\\{menus[currentMenu].ReadLine()}");
                            }
                            catch (ArgumentException)
                            {
                                menus[currentMenu].showInputCursor = false;
                                menus[currentMenu].WriteLine("\n\nFile not found, press any key to continue...");
                                Console.ReadKey();
                            }

                            menus[currentMenu].showInputCursor = false;

                            menus[currentMenu].Clear();

                            if (logFromFile != null)
                            {
                                foreach (string line in logFromFile)
                                {
                                    menus[currentMenu].WriteLine(line);
                                }
                            }
                        }
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("\n\nLoad a data log and display it...");
                    }
                    break;

                case "returnDataRecord":
                    if (onSelect)
                    {
                        currentMenu = (int)title.recorderMenu;
                        menus[currentMenu].RefreshMenu(true);
                        menus[currentMenu].RecallPreviousSelection();
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Return to the Data Recorder Menu...");
                    }
                    break;
                // *********************
                // * Alarm System Menu *
                // *********************
                case "alarmSystemMenu":
                    if(onSelect)
                    {
                        //load the menu
                        currentMenu = (int)title.alarmSystem;
                        menus[currentMenu].RefreshMenu(true);

                        //configure default parameters for the module
                        FinchAlarm.ConfigureAlarm(45, 75, FinchAlarm.Sensor.temperatureF);
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Set alerts to monitor various systems...");
                    }
                    break;
                case "configureAlarm":
                    if(onSelect)
                    {
                        menus[currentMenu].showInputCursor = true;

                        FinchAlarm.GetConfigFromUser();

                        //display settings saved confirmation
                        menus[currentMenu].Clear();
                        menus[currentMenu].WriteLine("Settings saved." +                                                     
                                                 "\n\nPress any key to continue...");
                        Console.ReadKey();

                        menus[currentMenu].Clear();
                        menus[currentMenu].WriteLine("Configure the parameters of the alarm system.");
                        menus[currentMenu].WriteLine("\nCurrent configuration...");
                        menus[currentMenu].WriteLine("                       Time to monitor: " + FinchAlarm.timeToMonitor + FinchAlarm.unitOfTimeStr);
                        menus[currentMenu].WriteLine("                     Sensor to monitor: " + FinchAlarm.dataSensorStr);
                        menus[currentMenu].WriteLine("\nPress enter to modifiy the current configuration.");
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Configure the parameters of the alarm system.");
                        menus[currentMenu].WriteLine("\nCurrent configuration...");
                        menus[currentMenu].WriteLine("                       Time to monitor: " + FinchAlarm.timeToMonitor + FinchAlarm.unitOfTimeStr);
                        menus[currentMenu].WriteLine("                     Sensor to monitor: " + FinchAlarm.dataSensorStr);
                        menus[currentMenu].WriteLine("\nPress enter to modifiy the current configuration.");
                    }
                    break;
                case "startAlarm":
                    if (onSelect)
                    {
                        //stop me from doing stupid stuff!
                        if (finchConnected)
                        {
                            bool alarmActive = true;
                            bool swapCycle = false;
                            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                            System.Diagnostics.Stopwatch phaseSwapTimer = new System.Diagnostics.Stopwatch();
                            System.Diagnostics.Stopwatch animationTimer = new System.Diagnostics.Stopwatch();
                            string animation = ".";
                            int animationDotCount = 0;
                            int sensorData = 0;


                            //start the timers
                            timer.Start();
                            phaseSwapTimer.Start();
                            animationTimer.Start();

                            while (alarmActive)
                            {
                                    //inform user of escape proceedure
                                    menus[currentMenu].Clear();
                                    menus[currentMenu].WriteLine("Monitoring in progress. Press esc to abort monitoring...\n\n" +
                                                                 "Sensor: " + FinchAlarm.dataSensorStr +
                                                                 "\nCurrent: " + sensorData.ToString() + 
                                                                 "\n\n" + animation);

                                    FinchAlarm.ProcessAlarmCycle(swapCycle, out sensorData);

                                if (phaseSwapTimer.ElapsedMilliseconds >= 500)
                                {
                                    phaseSwapTimer.Restart();
                                    swapCycle = !swapCycle;
                                }

                                if (animationTimer.ElapsedMilliseconds > 250)
                                {
                                    animationTimer.Restart();

                                    if (animationDotCount < 30)
                                    {
                                        animation += "  .";
                                    }
                                    else
                                    {
                                        animation = ".";
                                        animationDotCount = 0;
                                    }

                                    animationDotCount++;
                                }

                                //check for escape key
                                if (Console.KeyAvailable)
                                {
                                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                                    {
                                        alarmActive = false;
                                    }
                                }
                                else if ((ulong)timer.ElapsedMilliseconds >= FinchAlarm.finalAlarmCycle)
                                {
                                    timer.Stop();
                                    timer.Reset();
                                    alarmActive = false;
                                }
                            }

                            //reset finch
                            myFinch.setLED(0, 255, 0);
                            myFinch.noteOff();

                            //let the user know the test has ended
                            menus[currentMenu].Clear();
                            menus[currentMenu].WriteLine("Monitoring has ended. Press any key to continue...");
                            Console.Beep();
                            Console.ReadKey();

                            //clear IO
                            menus[currentMenu].Clear();
                        }
                        else
                        {
                            menus[currentMenu].Clear();
                            menus[currentMenu].WriteLine("Can not start monitoring until Finch is connected, \n\nPress any key to continue...");
                            Console.Beep();
                            Console.Beep();
                            Console.Beep();
                            Console.Beep();
                            Console.Beep();
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Configure the parameters of the alarm system.");
                        menus[currentMenu].WriteLine("\nCurrent configuration...");
                        menus[currentMenu].WriteLine("                       Time to monitor: " + FinchAlarm.timeToMonitor + FinchAlarm.unitOfTimeStr);
                        menus[currentMenu].WriteLine("                     Sensor to monitor: " + FinchAlarm.dataSensorStr);
                        menus[currentMenu].WriteLine("\n\nPress enter to begin monitoring sensors.");
                    }
                    break;

                // *************************
                // * User Programming Menu *
                // *************************
                case "userProgrammingMenu":
                    if (onSelect)
                    {
                        currentMenu = (int)title.userProgramming;
                        menus[currentMenu].RefreshMenu(true);
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Load the user programming menu...");
                    }
                    break;
                case "setCommands":
                    if (onSelect)
                    {
                        UserProgramming.SetCommands();

                        menus[currentMenu].Clear();
                        menus[currentMenu].showInputCursor = false;
                        menus[currentMenu].WriteLine("Settings saved. Press any key to continue...");
                        menus[currentMenu].showInputCursor = true;
                        Console.ReadKey();

                        menus[currentMenu].RefreshOnHover();
                        menus[currentMenu].RecallPreviousSelection();
                    }
                    else
                    {
                        if (finchConnected)
                        {
                            menus[currentMenu].WriteLine($"Control {name} using a basic set of user friendly commands...");
                        }
                        else
                        {
                            menus[currentMenu].WriteLine($"Control your Finch using a basic set of user friendly commands...");
                        }
                    }
                    break;
                case "runCommands":
                    if (onSelect)
                    {
                        if (finchConnected)
                        {
                            foreach (BasicCommand command in UserProgramming.commandList)
                            {
                                command.Run();
                            }

                            //reset finch
                            myFinch.setLED(0, 255, 0);
                            myFinch.noteOff();
                            myFinch.setMotors(0, 0);

                            //let user know the commands are done
                            menus[currentMenu].showInputCursor = false;
                            menus[currentMenu].WriteLine("\nAll commands executed successfully. Press any key to continue...");
                            menus[currentMenu].showInputCursor = true;
                            Console.ReadKey();

                            menus[currentMenu].RefreshOnHover();
                            menus[currentMenu].RecallPreviousSelection();
                        }
                        else
                        {
                            menus[currentMenu].Clear();
                            menus[currentMenu].WriteLine("Please connect finch before continuing. Press any key to continue...");

                            Console.ReadKey();
                            menus[currentMenu].Clear();
                        }
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Execute the commands listed to the left...");
                    }
                    break;
                // **********************
                // * Theme menu options *
                // **********************
                case "themeMenu":
                    if (onSelect)
                    {
                        currentMenu = (int)title.themes;
                        menus[currentMenu].properties.currentOption = (int)ThemeManager.theme;
                        menus[currentMenu].RefreshMenu(true);
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Change the program theme...");
                    }
                    break;
                case "Classic":
                    if (onSelect)
                    {
                        //set the new theme
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //save the theme
                        Enum.TryParse(menus[currentMenu].selectedOption, out ThemeManager.theme);

                        //return to the main menu
                        currentMenu = (int)title.main;
                        menus[currentMenu].RefreshMenu(true);
                    }
                    else
                    {
                        //set the new theme to be previewed
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //refresh menu so the theme displays correctly
                        menus[currentMenu].RefreshMenu(true, false);

                        //display the help text at the bottom
                        menus[currentMenu].WriteLine("Move the selector to preview the themes, press enter on one to save your settings and return to the main menu.");
                    }
                    break;
                case "Fallout":
                    if (onSelect)
                    {
                        //set the new theme
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //save the theme
                        Enum.TryParse(menus[currentMenu].selectedOption, out ThemeManager.theme);

                        //return to the main menu
                        currentMenu = (int)title.main;
                        menus[currentMenu].RefreshMenu(true);
                    }
                    else
                    {
                        //set the new theme to be previewed
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //refresh menu so the theme displays correctly
                        menus[currentMenu].RefreshMenu(true, false);

                        //display the help text at the bottom
                        menus[currentMenu].WriteLine("Move the selector to preview the themes, press enter on one to save your settings and return to the main menu.");
                    }
                    break;
                case "BSOD":
                    if (onSelect)
                    {
                        //set the new theme
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //save the theme
                        Enum.TryParse(menus[currentMenu].selectedOption, out ThemeManager.theme);

                        //return to the main menu
                        currentMenu = (int)title.main;
                        menus[currentMenu].RefreshMenu(true);
                    }
                    else
                    {
                        //set the new theme to be previewed
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //refresh menu so the theme displays correctly
                        menus[currentMenu].RefreshMenu(true, false);

                        //display the help text at the bottom
                        menus[currentMenu].WriteLine("Move the selector to preview the themes, press enter on one to save your settings and return to the main menu.");
                    }
                    break;
                case "ET_Game":
                    if (onSelect)
                    {
                        //set the new theme
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //save the theme
                        Enum.TryParse(menus[currentMenu].selectedOption, out ThemeManager.theme);

                        //return to the main menu
                        currentMenu = (int)title.main;
                        menus[currentMenu].RefreshMenu(true);
                    }
                    else
                    {
                        //set the new theme to be previewed
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //refresh menu so the theme displays correctly
                        menus[currentMenu].RefreshMenu(true, false);

                        //display the help text at the bottom
                        menus[currentMenu].WriteLine("Move the selector to preview the themes, press enter on one to save your settings and return to the main menu.");
                    }
                    break;
                case "Zelda":
                    if (onSelect)
                    {
                        //set the new theme
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //save the theme
                        Enum.TryParse(menus[currentMenu].selectedOption, out ThemeManager.theme);

                        //return to the main menu
                        currentMenu = (int)title.main;
                        menus[currentMenu].RefreshMenu(true);
                    }
                    else
                    {
                        //set the new theme to be previewed
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //refresh menu so the theme displays correctly
                        menus[currentMenu].RefreshMenu(true, false);

                        //display the help text at the bottom
                        menus[currentMenu].WriteLine("Move the selector to preview the themes, press enter on one to save your settings and return to the main menu.");
                    }
                    break;
                case "Grayscale":
                    if (onSelect)
                    {
                        //set the new theme
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //save the theme
                        Enum.TryParse(menus[currentMenu].selectedOption, out ThemeManager.theme);

                        //return to the main menu
                        currentMenu = (int)title.main;
                        menus[currentMenu].RefreshMenu(true);
                    }
                    else
                    {
                        //set the new theme to be previewed
                        ThemeManager.SetTheme(menus[currentMenu].selectedOption);

                        //refresh menu so the theme displays correctly
                        menus[currentMenu].RefreshMenu(true, false);

                        //display the help text at the bottom
                        menus[currentMenu].WriteLine("Move the selector to preview the themes, press enter on one to save your settings and return to the main menu.");
                    }
                    break;
                case "returnMainFromThemeMenu":
                        
                    ThemeManager.SetTheme(ThemeManager.theme.ToString());
                    menus[currentMenu].RefreshMenu(true, false);
                    
                    if (onSelect)
                    {
                        currentMenu = (int)title.main;
                        menus[(int)title.main].RefreshMenu(true);
                    }                                        
                    break;
                // *******************************
                // * Universal Selection Options *
                // *******************************
                case "returnMain":
                    if (onSelect)
                    {
                        currentMenu = (int)title.main;
                        menus[(int)title.main].RefreshMenu(true);
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Return to the Main Menu...");
                    }
                    break;

                case "quit":
                    if (onSelect)
                    {
                        DisconnectFinch();
                        running = false;
                    }
                    else
                    {
                        menus[currentMenu].WriteLine("Disconect Finch and exit...");
                    }
                    break;

                default:
                    if (onSelect)
                    {
                        DisplayMenuError();
                    }
                    break;
            }
        }
        #endregion

        #region Talent Show Methods
        static void LightAndSound()
        {
            // *************
            // * Variables *
            // *************
            int finchTempFStart = CelsiusToFahrenheit(myFinch.getTemperature());
            int finchTempF = CelsiusToFahrenheit(myFinch.getTemperature());
            int[] rgb = { 0, 0, 0 };
            bool lightingUp = true;

            // *************
            // * Constants *
            // *************
            const int RED = 0;
            const int GREEN = 1;
            const int BLUE = 2;
            const int SCALE_FACTOR = 10;

            //begin loop
            while (lightingUp)
            {
                //refresh output
                menus[(int)title.talentShow].RefreshMenu();

                //check the finch's temperature
                finchTempF = CelsiusToFahrenheit(myFinch.getTemperature());

                if (finchTempF <= finchTempFStart + 4)
                {
                    rgb[RED] = 0;
                    rgb[GREEN] = 0;
                    rgb[BLUE] = 255;
                }
                else
                {
                    rgb[RED] = 0 + ((finchTempF - finchTempFStart) * SCALE_FACTOR);
                    rgb[GREEN] = 0;
                    rgb[BLUE] = 255 - ((finchTempF - finchTempFStart) * SCALE_FACTOR);

                    //make sure blue and green stay valid
                    if (rgb[BLUE] < 0) 
                    {
                        rgb[BLUE] = 0;
                    }

                    if (rgb[RED] >= 255)
                    {
                        PlayChestOpenSong();
                        rgb[RED] = 255;
                    }
                }

                //set finch rgb
                myFinch.setLED(rgb[RED], rgb[GREEN], rgb[BLUE]);

                if (menus[(int)title.talentShow].enterKeyPressed)
                {
                    MenuNavigation();

                    //if you're no longer in this loop, return control to correct loop
                    if (currentMenu != (int)title.talentShow)
                    {
                        return;
                    }
                }
            }
        }
        static void PlayChestOpenSong(int octave = 2)
        {
            PlayNote((int)Math.Pow(24.5, (double)octave), 100, 100);
            PlayNote((int)Math.Pow(27.5, (double)octave), 100, 100);
            PlayNote((int)Math.Pow(19.45, (double)octave), 100, 100);
            PlayNote((int)Math.Pow(27.5, (double)octave - 1), 100, 100);
            PlayNote((int)Math.Pow(16.35, (double)octave), 100, 100);
            PlayNote((int)Math.Pow(24.5, (double)octave - 1), 100, 100);
            PlayNote((int)Math.Pow(20.6, (double)octave), 100, 100);
            PlayNote((int)Math.Pow(25.96, (double)octave), 100, 100);
            PlayNote((int)Math.Pow(16.35, (double)octave), 100, 100);
        }
        static void DanceMonkeyDance()
        {
            // *************
            // * Constants *
            // *************
            const int TRAVEL_DURATION = 500;
            const int TURN_DURATION = 650;

            //falash LED white the set LED to green
            myFinch.setLED(0, 0, 0);
            myFinch.wait(1000);
            myFinch.setLED(0, 255, 0);

            //tell the user what the finch is doing
            menus[currentMenu].Clear();
            menus[currentMenu].WriteLine(name + ": Ok, but I'm not much of a dancer...");

            for (int i = 0; i < 4; ++i) 
            {
                //travel forward for set duration
                myFinch.setMotors(128, 128);
                myFinch.wait(TRAVEL_DURATION);

                //turn left
                myFinch.setMotors(-128, 128);
                myFinch.wait(TURN_DURATION);
            }

            //stop finch
            myFinch.setMotors(0, 0);

            if (myFinch.getAccelerations()[0] == 0 & myFinch.getAccelerations()[1] == 0)
            {
                menus[currentMenu].WriteLine("\n" + name + ": That's all I got! What do ya want from me?");
            }
        }
        static void MixUp()
        {
            // *************
            // * Constants *
            // *************
            const int MOVE_DELAY = 750;

            menus[currentMenu].Clear();
            menus[currentMenu].WriteLine(name + ": Well, here goes nothing!");

            for (int i = 0; i < 10; ++i)
            {
                //first set
                myFinch.setMotors(-128, 128);
                myFinch.setLED(0, 153, 255);
                myFinch.noteOn(5587);
                myFinch.wait(MOVE_DELAY);

                //second set
                myFinch.setMotors(128, -128);
                myFinch.setLED(255, 153, 51);
                myFinch.noteOn(4186);
                myFinch.wait(MOVE_DELAY);
            }

            //reset everything
            myFinch.setMotors(0, 0);
            myFinch.setLED(0, 255, 0);
            myFinch.noteOff();
        }
        #endregion

        #region Data Recorder Methods
        /// <summary>
        /// Get data recorder settings from the user and configure the DataRecorder to begin recording
        /// </summary>
        static void ConfigureRecorder()
        {
            // *************
            // * Variables *
            // *************
            string userInput = "";
            int dataPoints = recorder.NUMBER_OF_RECORDS;
            int timeBetween = recorder.TIME_BETWEEN_RECORDS;
            timeUnits units = recorder.TIME_UNIT;
            bool validating;

            // ************************
            // * Input and Validation *
            // ************************

            //userPrompt: use default or custom configuration
            validating = true;
            while (validating)
            {
                menus[currentMenu].Clear();

                menus[currentMenu].WriteLine("Would you like to reset the current configuration to the default...");
                menus[currentMenu].WriteLine("                         Data Points: 8");
                menus[currentMenu].WriteLine("                               Delay: 10");
                menus[currentMenu].WriteLine("                         Delay Units: seconds");
                menus[currentMenu].Write("\nWould you like to reset the configuration, use a custom one, or keep it the same? ");

                userInput = menus[currentMenu].ReadLine().ToLower();

                if (userInput.Contains("custom") | userInput == "c")
                {
                    validating = false;
                }
                else if (userInput.Contains("reset") | userInput == "r")
                {
                    recorder = new DataRecorder();
                }
                else if (userInput.Contains("keep") | userInput == "k")
                {
                    menus[currentMenu].Clear();
                    return;
                }
                else
                {
                    menus[currentMenu].Clear();
                    menus[currentMenu].WriteLine("Please enter a valid option (custom, reset, or keep). Press any key to continue...");
                    Console.ReadKey();
                }
            }

            //userPrompt: how many data points to capture.
            validating = true;
            while (validating)
            {
                menus[currentMenu].Clear();
                menus[currentMenu].Write("How many data points would you like to capture? (You may also type default to use the default: 8) ");
                userInput = menus[currentMenu].ReadLine().ToLower();

                if (int.TryParse(userInput, out dataPoints))
                {
                    if (dataPoints > 0 & dataPoints <= 50)
                    {
                        validating = false;
                    }
                    else
                    {
                        menus[currentMenu].Clear();
                        menus[currentMenu].WriteLine("Invalid Selection: Please enter a valid number between 1 - 30 or \"default.\" Press any key to continue...");
                        Console.ReadKey();
                    }
                }
                else if (userInput == "default" | userInput == "d")
                {
                    dataPoints = 8;
                    validating = false;
                }
                else
                {
                    menus[currentMenu].Clear();
                    menus[currentMenu].WriteLine("Invalid Selection: Please enter a valid number between 1 - 30 or \"default.\" Press any key to continue...");
                    Console.ReadKey();
                }
            }

            //userPrompt: time unit to use
            validating = true;
            while (validating)
            {
                menus[currentMenu].Clear();
                menus[currentMenu].Write("What unit of time would you like to use: milliseconds, seconds, or minutes? (You may also type default to use the default: seconds) ");
                userInput = menus[currentMenu].ReadLine().ToLower();

                if (userInput == "seconds" | userInput == "sec")
                {
                    units = timeUnits.seconds;
                    validating = false;
                }
                else if (userInput == "milliseconds" | userInput == "mil")
                {
                    units = timeUnits.milliseconds;
                    validating = false;
                }
                else if (userInput == "minutes" | userInput == "min")
                {
                    units = timeUnits.minutes;
                    validating = false;
                }
                else if (userInput == "default" | userInput == "d")
                {
                    units = timeUnits.seconds;
                    validating = false;
                }
                else
                {
                    menus[currentMenu].Clear();
                    menus[currentMenu].WriteLine("Invalid Selection: Please enter one of the following: milliseconds, seconds, minutes or \"default.\" Press any key to continue...");
                    Console.ReadKey();
                }
            }

            //userPrompt: how long between data points.
            validating = true;
            while (validating)
            {
                menus[currentMenu].Clear();
                menus[currentMenu].Write("How much time in " + units + " do you want to pass between data points? (You may also type default to use the default: 10) ");
                userInput = menus[currentMenu].ReadLine().ToLower();

                if (int.TryParse(userInput, out timeBetween))
                {
                    if ((timeBetween > 0 & timeBetween <= 120) & units != timeUnits.milliseconds)
                    {
                        validating = false;
                    }
                    else if ((timeBetween > 0 & timeBetween <= 60000) & units == timeUnits.milliseconds)
                    {
                        validating = false;
                    }
                    else
                    {
                        if (units == timeUnits.milliseconds)
                        {
                            menus[currentMenu].Clear();
                            menus[currentMenu].WriteLine("Invalid Selection: Please enter a valid number between 1 - 1000 or \"default.\" Press any key to continue...");
                            Console.ReadKey();
                        }
                        else
                        {
                            menus[currentMenu].Clear();
                            menus[currentMenu].WriteLine("Invalid Selection: Please enter a valid number between 1 - 120 or \"default.\" Press any key to continue...");
                            Console.ReadKey();
                        }
                    }
                }
                else if (userInput == "default" | userInput == "d")
                {
                    timeBetween = 10;
                    validating = false;
                }
                else
                {
                    menus[currentMenu].Clear();
                    menus[currentMenu].WriteLine("Invalid Selection: Please enter a valid number between 1 - 30 or \"default.\" Press any key to continue...");
                    Console.ReadKey();
                }
            }

            // *****************
            // * Configuration *
            // *****************
            menus[currentMenu].Clear();
            menus[currentMenu].WriteLine("New configuration saved. You may view/change the configuration \n" +
                                         "by reselecting \"Recorder Configuration.\" If you're ready to start \n" +
                                         "the data recording, select \"Start Recording.\"\n");
            menus[currentMenu].WriteLine("Press any key to continue...");
            Console.ReadKey();
            menus[currentMenu].Clear();

            recorder = new DataRecorder(dataPoints, timeBetween, units);
        }

        static void viewData()
        {
            string line = "";
            int average = 0;

            //make sure there is data to work with
            if (recorder.dataAccuired) 
            {
                //print out the header
                menus[currentMenu].Clear();
                menus[currentMenu].WriteLine(" Entry | Temp C | Temp F");
                menus[currentMenu].WriteLine("=======|========|=======");

                //print out the data
                for (int i = 0; i < recorder.temperatures.Length; ++i)
                {
                    line = "#" + i.ToString("D3") + "   | " + recorder.temperatures[i].ToString("D3") + "\u00b0C  | " + 
                        CelsiusToFahrenheit(recorder.temperatures[i]).ToString("D3") + "\u00b0F";
                    menus[currentMenu].WriteLine(line);
                }

                //give the user the average temperature
                average = (int)recorder.temperatures.Average();
                menus[currentMenu].WriteLine("\nThe average temperature was " + average.ToString("D3") + "\u00b0C or " + 
                    CelsiusToFahrenheit((double)average).ToString("D3") + "\u00b0F");
            }
            else
            {
                //show user an error
                menus[currentMenu].Clear();
                menus[currentMenu].WriteLine("Please record some data first.");
                menus[currentMenu].WriteLine("\nPress any key to continue...");
                Console.Beep();
                Console.ReadKey();
                menus[currentMenu].Clear();
            }
        }

        #endregion

        #region Connections
        static string ConnectFinch()
        {
            // *************
            // * Variables *
            // *************
            string name = "";
            bool namingFinch = true;
            bool existingName = false;
            int existingNameIndex = 0;
            List<string> settingsFile = new List<string>();

            //check settings file to see if there was a previous name used
            settingsFile.AddRange(System.IO.File.ReadAllLines(ThemeManager.SETTINGS_FILE));

            foreach (string setting in settingsFile) 
            {
                if (setting.Contains("previousName:"))
                {
                    for (int charIndex = setting.IndexOf(':') + 1; charIndex < setting.Length; charIndex++)
                    {
                        name += setting[charIndex];
                    }
                }
            }

            //see if the user wants to overwrite the stored name
            if (name != "")
            {
                namingFinch = !ValidateInput($"You last used the name {name}, would you like to connect with that name? ");
            }

            while (namingFinch)
            {
                //inform the user that we're going to try connecting to the finch
                menus[(int)title.connect].Clear();
                menus[(int)title.connect].WriteLine("We are going to attemt to connect to your Finch.\n");
                menus[(int)title.connect].Write("What would you like to name it? ");

                //wait for user enter a name and press enter
                name = menus[(int)title.connect].ReadLine();

                namingFinch = !ValidateInput($"Are you sure you want to name your finch {name}? ");
            }

            if (myFinch.connect())
            {
                menus[(int)title.connect].WriteLine($"\n\nConnection to {name} was successful!\n\nReturning to main menu...");

                myFinch.setLED(0, 255, 0);

                System.Threading.Thread.Sleep(1000);

                menus[(int)title.connect].Clear();
            }
            else
            {
                menus[(int)title.connect].Clear();
                Console.Beep();

                menus[(int)title.connect].WriteLine("\n\nConnection to " + name + " Failed!");
                menus[(int)title.connect].WriteLine("Please make sure your Finch is properly connected to your PC.");
                menus[(int)title.connect].WriteLine("\n\n Press any key to continue...");

                //set failure condition
                name = "___FAILED_TO_CONNECT___";

                Console.ReadKey();
            }

            //save the name to the settings file
            for (int index = 0; index < settingsFile.Count; index++)
            {
                if (settingsFile[index].Contains("previousName:"))
                {
                    existingName = true;
                    existingNameIndex = index;
                }
            }
            if (existingName)
            {
                settingsFile[existingNameIndex] = $"previousName:{name}";
            }
            else
            {
                settingsFile.Add($"previousName:{name}");
            }
            System.IO.File.WriteAllLines(ThemeManager.SETTINGS_FILE, settingsFile);

            return name;
        }

        static void DisconnectFinch()
        {
            if(finchConnected)
            {
                //clear IO
                menus[currentMenu].Clear();

                //inform user of disconnect
                menus[currentMenu].WriteLine("We will now attempt to en... err... disconnect... " + name + " from his... I mean your computer.");
                menus[currentMenu].WriteLine("\n\nPress any key to pull the plug...");

                Console.ReadKey();

                //disconnect finch
                myFinch.disConnect();
            }
        }
        #endregion
    }
}