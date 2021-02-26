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
        static Menu[] menus = new Menu[Enum.GetNames(typeof(title)).Length];
        static Finch myFinch = new Finch();
        static DataRecorder recorder = new DataRecorder();
        static string name = "";
        static bool finchConnected = false;
        static bool running = true;
        static int currentMenu = 0;

        enum title
        {
            main,
            connect,
            talentShow,
            recorderMenu,
            recorderLogsMenu
        }
        static void Main(string[] args)
        {
            //create the menu instances
            for (int menuNum = 0; menuNum < Enum.GetNames(typeof(title)).Length; ++menuNum)
            {
                menus[menuNum] = new Menu();
            }

            // *********************
            // * Main menu section *
            // *********************

            //configure console color
            //future revisions may also put this in the config (.jp) file
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            
            //load menus
            menus[(int)title.main].LoadTemplate("config\\MainMenu.txt");
            menus[(int)title.connect].LoadTemplate("config\\ConnectFinch.txt");
            menus[(int)title.talentShow].LoadTemplate("config\\TalentShow.txt");
            menus[(int)title.recorderMenu].LoadTemplate("config\\DataRecorder.txt");
            menus[(int)title.recorderLogsMenu].LoadTemplate("config\\ViewDataRecords.txt");

            //refresh main menu
            menus[(int)title.main].RefreshMenu(true);

            while (running) 
            {
                //updates input/output of the menu
                menus[currentMenu].RefreshMenu();

                //checks if user pressed the enter key
                if (menus[currentMenu].enterKeyPressed)
                {
                    //reads the option pressed by the user and acts on it.
                    MenuNavigation();
                }
            }
        }

        #region Basic Finch Controls
        static void PlayNote(int note, int waitDuring, int waitAfter)
        {
            myFinch.noteOn(note);
            myFinch.wait(waitDuring);
            myFinch.noteOff();
            myFinch.wait(waitAfter);
        }

        static int CelsiusToFahrenheit(double tempC)
        {
            return (int)(tempC * 1.8 + 32);
        }
        #endregion

        #region Menu Navigation Methods
        static void DisplayMenuError(string error = "Invalid selection, Press any key to continue...")
        {
            //clear the menu IO
            menus[currentMenu].Clear();

            //write the line and display it in the output area
            menus[currentMenu].WriteLine(error);

            //wait for user input
            Console.ReadKey();
        }

        static void MenuNavigation()
        {
            switch (menus[currentMenu].selectedOption)
            {
                // *********************
                // * main menu options *
                // *********************
                case "connectMenu":
                    //switch to the connect menu
                    currentMenu = (int)title.connect;
                    menus[(int)title.connect].RefreshMenu(true);
                    break;

                case "talentShowMenu":
                    currentMenu = (int)title.talentShow;
                    menus[(int)title.talentShow].RefreshMenu(true);
                    break;

                case "dataRecordMenu":
                    currentMenu = (int)title.recorderMenu;
                    menus[currentMenu].RefreshMenu(true);
                    break;

                // **********************
                // * Connect Finch Menu *
                // **********************
                case "connect":
                    name = ConnectFinch();

                    //verify the finch is connected
                    finchConnected = name != "___FAILED_TO_CONNECT___";

                    break;

                // ********************
                // * Talent Show Menu *
                // ********************
                case "lightSound":
                    LightAndSound();
                    break;
                case "dance":
                    DanceMonkeyDance();
                    break;
                case "mix":
                    MixUp();
                    break;

                // **********************
                // * Data Recorder Menu *
                // **********************
                case "startRecorder":
                    recorder.Start(myFinch, ref menus[currentMenu]);
                    break;

                case "configureRecorder":
                    configureRecorder();
                    break;

                case "viewRecords":
                    currentMenu = (int)title.recorderLogsMenu;
                    menus[(int)title.recorderLogsMenu].RefreshMenu(true);
                    break;

                // ***************************
                // * Data Recorder Logs Menu *
                // ***************************
                case "returnDataRecord":
                    currentMenu = (int)title.recorderMenu;
                    menus[currentMenu].RefreshMenu(true);
                    break;

                // *******************************
                // * Universal Selection Options *
                // *******************************
                case "returnMain":
                    currentMenu = (int)title.main; 
                    menus[(int)title.main].RefreshMenu(true);
                    break;

                case "quit":
                    DisconnectFinch();
                    running = false;
                    break;

                default:
                    DisplayMenuError();
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
        static void configureRecorder()
        {
            // *************
            // * Variables *
            // *************
            string userInput = "";
            int dataPoints = 8;
            int timeBetween = 10;
            timeUnits units = timeUnits.seconds;
            bool validating;

            // ************************
            // * Input and Validation *
            // ************************

            //userPrompt: use default or custom configuration
            validating = true;
            while (validating)
            {
                menus[currentMenu].Clear();

                menus[currentMenu].WriteLine("Default Configuration...");
                menus[currentMenu].WriteLine("                         Data Points: 8");
                menus[currentMenu].WriteLine("                               Delay: 10");
                menus[currentMenu].WriteLine("                         Delay Units: Seconds");
                menus[currentMenu].Write("\nWould you like to use the default configuration? ");

                userInput = menus[currentMenu].ReadLine().ToLower();

                if (userInput == "yes" | userInput == "y")
                {
                    return;
                }
                else if (userInput == "no" | userInput == "n")
                {
                    validating = false;
                }
                else
                {
                    menus[currentMenu].Clear();
                    menus[currentMenu].WriteLine("Please enter yes or no. Press any key to continue...");
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
                    units = timeUnits.miliseconds;
                    validating = false;
                }
                else if (userInput == "minutes" | userInput == "min")
                {
                    units = timeUnits.minutes;
                    validating = false;
                }
                else if (userInput == "default" | userInput == "d")
                {
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
                menus[currentMenu].Write("How much time in " + units + "do you want to pass between data points? (You may also type default to use the default: 10) ");
                userInput = menus[currentMenu].ReadLine().ToLower();

                if (int.TryParse(userInput, out timeBetween))
                {
                    if (timeBetween > 0 & timeBetween <= 120)
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
            recorder = new DataRecorder(dataPoints, timeBetween, units);
        }

        #endregion

        #region Connections
        static string ConnectFinch()
        {
            // *************
            // * Variables *
            // *************
            string name = "";
            string userInput = "";
            bool namingFinch = true;

            while (namingFinch)
            {
                //inform the user that we're going to try connecting to the finch
                menus[(int)title.connect].Clear();
                menus[(int)title.connect].WriteLine("We are going to attemt to connect to your Finch.\n");
                menus[(int)title.connect].Write("What would you like to name it?  ");

                //wait for user enter a name and press enter
                name = menus[(int)title.connect].ReadLine();

                menus[(int)title.connect].Clear();
                menus[(int)title.connect].Write("Are you sure you want to name it " + name + "? ");

                //wait for user answer and evaluate
                userInput = menus[(int)title.connect].ReadLine();
                if (userInput.ToLower() == "yes")
                {
                    namingFinch = false;
                }
                else if (userInput.ToLower() == "y") 
                {
                    namingFinch = false;
                }
            }

            if (myFinch.connect())
            {
                menus[(int)title.connect].WriteLine("\n\nConnection to " + name + " was successful!\n\n");

                myFinch.setLED(0, 255, 0);

                System.Threading.Thread.Sleep(3000);

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

