using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3FinchControl
{
    #region Command Classes
    class BasicCommand
    {
        public BasicCommand(UserProgramming.Commands cmdName)
        {
            name = cmdName;
            modifier = "";

            SetShutoff(true, true, true);
        }

        public virtual void Run()
        {
            //any call to the Run method of the basic command will reset the finch
            if (shutoff.Motors) Program.myFinch.setMotors(0, 0);
            if (shutoff.LED) Program.myFinch.setLED(0, 0, 0);
            if (shutoff.Note) Program.myFinch.noteOff();
        }

        /// <summary>
        /// Function determines what elements the run call of the basic command shuts off
        /// </summary>
        /// <param name="Motors">shuts off motors</param>
        /// <param name="LED">shuts off LEDs</param>
        /// <param name="Note">Shuts off note</param>
        public void SetShutoff(bool Motors, bool LED, bool Note)
        {
            modifier = "";

            shutoff.Motors = Motors;
            shutoff.LED = LED;
            shutoff.Note = Note;

            if (Motors && LED && Note)
            {
                modifier = "A";
            }
            else
            {
                if (Motors) modifier += "M";
                if (LED) modifier += "L";
                if (Note) modifier += "B";
            }
        }

        private (bool Motors, bool LED, bool Note) shutoff;

        public UserProgramming.Commands name
        {
            protected set;
            get;
        }
        public string modifier
        {
            protected set;
            get;
        }
    }
    class MovementCommand : BasicCommand
    {
        public MovementCommand(UserProgramming.Commands cmdName, int cmdSpeed) : base(cmdName)
        {
            speed = cmdSpeed;
            modifier = cmdSpeed.ToString();
        }

        public override void Run()
        {
            switch(name)
            {
                case UserProgramming.Commands.Right:
                    Program.myFinch.setMotors(speed, (speed * -1));
                    break;
                case UserProgramming.Commands.Left:
                    Program.myFinch.setMotors((speed *-1), speed);
                    break;
                case UserProgramming.Commands.Forward:
                    Program.myFinch.setMotors(speed, speed);
                    break;
                case UserProgramming.Commands.Back:
                    Program.myFinch.setMotors((speed * -1), (speed * -1));
                    break;
            }

            Program.menus[Program.currentMenu].WriteLine($"Running Command: {name.ToString()} {modifier}...");
        }

        int speed;
    }
    class LEDCommand : BasicCommand
    {
        public LEDCommand(UserProgramming.Commands cmdName, int cmdR, int cmdG, int cmdB) : base(cmdName)
        {
            r = cmdR;
            g = cmdG;
            b = cmdB;

            modifier = rgbToColor(cmdR, cmdG, cmdB);
        }

        string rgbToColor(int r, int g, int b)
        {
            string colorName = "";
            (int r, int g, int b) color = (r, g, b);

            if (color == (255, 0, 0))
            {
                colorName = "Red";
            }
            else if (color == (0, 255, 0))
            {
                colorName = "Green";
            }
            else if (color == (0, 0, 255))
            {
                colorName = "Blue";
            }
            else if (color == (255, 255, 0))
            {
                colorName = "Yellow";
            }
            else if (color == (255, 0, 255))
            {
                colorName = "Purple";
            }
            else if (color == (255, 255, 255))
            {
                colorName = "White";
            }
            else if (color == (0, 255, 255))
            {
                colorName = "Aqua";
            }
            else if (color == (255, 155, 0))
            {
                colorName = "Orange";
            }
            else if (color == (0, 0, 0))
            {
                colorName = "Off";
            }

            return colorName;
        }

        public override void Run()
        {
            Program.myFinch.setLED(r, g, b);
            Program.menus[Program.currentMenu].WriteLine($"Running Command: {name.ToString()} {modifier}...");
        }

        int r;
        int g;
        int b;
    }
    class WaitCommand : BasicCommand
    {
        public WaitCommand(UserProgramming.Commands cmdName, int cmdTime) : base(cmdName)
        {
            time = cmdTime;
            modifier = cmdTime.ToString();
        }

        public override void Run()
        {
            Program.myFinch.wait(time);
            Program.menus[Program.currentMenu].WriteLine($"Running Command: {name.ToString()} {modifier}...");
        }

        int time;
    }
    class BuzzerCommand : BasicCommand
    {
        public BuzzerCommand(UserProgramming.Commands cmdName, int cmdFrequency) : base(cmdName)
        {
            frequency = cmdFrequency;
            modifier = cmdFrequency.ToString();
        }

        public override void Run()
        {
            if (frequency == 0) Program.myFinch.noteOff();
            else Program.myFinch.noteOn(frequency);
            Program.menus[Program.currentMenu].WriteLine($"Running Command: {name.ToString()} {modifier}...");
        }
        int frequency;
    }
    class SensorCommand : BasicCommand
    {
        public SensorCommand(UserProgramming.Commands cmdName) : base(cmdName) => output = 0;
        public override void Run()
        {
            switch (base.name)
            {
                case UserProgramming.Commands.GetLightLevel:
                    output = (int)Program.myFinch.getLightSensors().Average();
                    Program.menus[Program.currentMenu].WriteLine("The current light level is " + output.ToString());
                    break;
                case UserProgramming.Commands.GetTemperature:
                    output = Program.CelsiusToFahrenheit(Program.myFinch.getTemperature());
                    Program.menus[Program.currentMenu].WriteLine("The current temperature is " + output.ToString() + "\u00b0F");
                    break;
            }
        }
        int output;
    }
    #endregion

    static class UserProgramming
    {
        public enum Commands
        {
            Forward,
            Back,
            Right,
            Left,
            GetTemperature,
            GetLightLevel,
            SetLED,
            BuzzerOn,
            BuzzerOff,
            Wait,
            Stop,
            Done,
            Delete
        }

        private enum Colors
        {
            red,    // 255,0,0
            blue,   // 0,0,255
            green,  // 0,255,0
            yellow, // 255,255,0
            white,  // 255,255,255
            purple, // 255,0,255
            aqua,   // 0,255,255
            orange, // 255,155,0
            off     // 0,0,0 
        }

        static UserProgramming()
        {
            commandList = new List<BasicCommand>();
        }

        public static void SetCommands()
        {
            // *************
            // * Variables *
            // *************
            bool settingCommands = true;
            string userInput = "";
            string command = "";
            string modifier = "";
            string prompt = "";

            //set the input cursor visibility to true
            Program.menus[Program.currentMenu].showInputCursor = true;

            prompt = "Would you like to make changes to the current command list? ";
            userInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" });
            if (userInput == "no" | userInput == "n") settingCommands = false;
            else commandList.Clear();

            while (settingCommands)
            {
                //reset command and modifier variables
                command = "";
                modifier = "";

                prompt = "All current commands are visible in the left pannel. If the command list is longer \n" +
                         "than the pannel you may use the Page Up and Page Down keys to navigate. \n\n" +
                         
                         "Please enter a valid command or done to return to the previous menu... \n" +
                         "Command: ";

                //get the command from the user.
                //couldn't use the default input validation method because of it's inability to handle the compound commands
                Program.menus[Program.currentMenu].Clear();
                Program.menus[Program.currentMenu].Write(prompt);
                userInput = Program.menus[Program.currentMenu].ReadLine();

                //break the command away from any modifiers
                for (int index = 0; index < userInput.Length; ++index)
                {
                    if (userInput[index] == ' ') 
                    {
                        //skip over the space
                        index++;

                        //in the event the index is still within the string copy the remainder of the userInput string to the modifier string
                        if (index < userInput.Length) 
                        {
                            modifier = userInput.Substring(index);
                        }
                        
                        //end the separation search
                        break;
                    }

                    //copy all chars before the first space to the command string, this is assumed to be the command being issued
                    command += userInput[index];
                }

                //check the command against the command list
                switch (command.ToLower())
                {
                    case "forward":
                        if (modifier != "") 
                        {
                            //create a variable for the speed and set it to the default incase the parse fails
                            int speed = DEFAULT_SPEED;

                            //parse the modifier for a speed value
                            int.TryParse(modifier, out speed);

                            //validate whether a valid speed setting was entered
                            //error message
                            if (speed < 1 | speed >= 255)
                            {
                                Program.menus[Program.currentMenu].Clear();
                                Program.menus[Program.currentMenu].WriteLine("Invalid Modifier: Speed must be set between 1-255.");
                                Program.menus[Program.currentMenu].Write("Press any key to continue...");
                                Console.ReadKey();
                            }
                            //or save command
                            else
                            {
                                commandList.Add(new MovementCommand(Commands.Forward, speed));
                            }
                        }
                        else
                        {
                            commandList.Add(new MovementCommand(Commands.Forward, DEFAULT_SPEED));
                        }
                        break;
                    case "back":
                        if (modifier != "")
                        {
                            //create a variable for the speed and set it to the default incase the parse fails
                            int speed = DEFAULT_SPEED;

                            //parse the modifier for a speed value
                            int.TryParse(modifier, out speed);

                            //validate whether a valid speed setting was entered
                            //error message
                            if (speed < 1 | speed >= 255)
                            {
                                Program.menus[Program.currentMenu].Clear();
                                Program.menus[Program.currentMenu].WriteLine("Invalid Modifier: Speed must be set between 1-255.");
                                Program.menus[Program.currentMenu].Write("Press any key to continue...");
                                Console.ReadKey();
                            }
                            //or save command
                            else
                            {
                                commandList.Add(new MovementCommand(Commands.Back, speed));
                            }
                        }
                        else
                        {
                            commandList.Add(new MovementCommand(Commands.Back, DEFAULT_SPEED));
                        }
                        break;
                    case "left":
                        if (modifier != "")
                        {
                            //create a variable for the speed and set it to the default incase the parse fails
                            int speed = DEFAULT_SPEED;

                            //parse the modifier for a speed value
                            int.TryParse(modifier, out speed);

                            //validate whether a valid speed setting was entered
                            //error message
                            if (speed < 1 | speed >= 255)
                            {
                                Program.menus[Program.currentMenu].Clear();
                                Program.menus[Program.currentMenu].WriteLine("Invalid Modifier: Speed must be set between 1-255.");
                                Program.menus[Program.currentMenu].Write("Press any key to continue...");
                                Console.ReadKey();
                            }
                            //or save command
                            else
                            {
                                commandList.Add(new MovementCommand(Commands.Left, speed));
                            }
                        }
                        else
                        {
                            commandList.Add(new MovementCommand(Commands.Left, DEFAULT_SPEED));
                        }
                        break;
                    case "right":
                        if (modifier != "")
                        {
                            //create a variable for the speed and set it to the default incase the parse fails
                            int speed = DEFAULT_SPEED;

                            //parse the modifier for a speed value
                            int.TryParse(modifier, out speed);

                            //validate whether a valid speed setting was entered
                            //error message
                            if (speed < 1 | speed >= 255)
                            {
                                Program.menus[Program.currentMenu].Clear();
                                Program.menus[Program.currentMenu].WriteLine("Invalid Modifier: Speed must be set between 1-255.");
                                Program.menus[Program.currentMenu].Write("Press any key to continue...");
                                Console.ReadKey();
                            }
                            //or save command
                            else
                            {
                                commandList.Add(new MovementCommand(Commands.Right, speed));
                            }
                        }
                        else
                        {
                            commandList.Add(new MovementCommand(Commands.Right, DEFAULT_SPEED));
                        }
                        break;
                    case "getlightlevel":
                        commandList.Add(new SensorCommand(Commands.GetLightLevel));
                        break;
                    case "gettemperature":
                        commandList.Add(new SensorCommand(Commands.GetTemperature));
                        break;
                    case "setled":
                        if (modifier != "")
                        {
                            //create a variable for the speed and set it to the default incase the parse fails
                            int[] colorRGB = { 0, 0, 0 };
                            Colors namedColor;


                            //check if user entered named color
                            if (Enum.TryParse(modifier, out namedColor)) 
                            {
                                //user has entered a valid named color
                                colorRGB = ConvertColor(namedColor);

                                //set the command
                                commandList.Add(new LEDCommand(Commands.SetLED, colorRGB[0], colorRGB[1], colorRGB[2]));
                            }
                            else
                            {
                                Program.menus[Program.currentMenu].Clear();
                                Program.menus[Program.currentMenu].Write($"Invalid Modifier '{modifier}': The SetLED command can accept any of the following modifiers:\n\n");
                                foreach (string s in Enum.GetNames(typeof(Colors)))
                                {
                                    Program.menus[Program.currentMenu].WriteLine($"\t\t{s}");
                                }

                                Program.menus[Program.currentMenu].Write("\nYour command has not been issued. Press Enter to continue...");
                                Program.menus[Program.currentMenu].ReadLine();
                            }
                        }
                        else
                        {
                            commandList.Add(new LEDCommand(Commands.SetLED, 0, 0, 0));
                        }
                        break;
                    case "buzzeroff":
                        commandList.Add(new BuzzerCommand(Commands.BuzzerOff, 0));
                        break;
                    case "buzzeron":
                        if (modifier != "")
                        {
                            //create a variable for the speed and set it to the default incase the parse fails
                            int frequency = 0;

                            //check if user entered numeric amount
                            if (int.TryParse(modifier, out frequency))
                            {
                                //user has entered a valid frequency
                                if (frequency > 0 & frequency <= 22000)
                                {
                                    commandList.Add(new BuzzerCommand(Commands.BuzzerOn, frequency));
                                }
                                else
                                {
                                    Program.menus[Program.currentMenu].Clear();
                                    Program.menus[Program.currentMenu].Write($"Invalid Modifier '{modifier}': The BuzzerOn command can accept a modifier ranging from 1-22000\n");


                                    Program.menus[Program.currentMenu].Write("\nYour command has not been issued. Press Enter to continue...");
                                    Program.menus[Program.currentMenu].ReadLine();
                                }
                            }
                            else
                            {
                                Program.menus[Program.currentMenu].Clear();
                                Program.menus[Program.currentMenu].Write($"Invalid Modifier '{modifier}': The BuzzerOn command can accept a modifier ranging from 1-22000\n");

                                Program.menus[Program.currentMenu].Write("\nYour command has not been issued. Press Enter to continue...");
                                Program.menus[Program.currentMenu].ReadLine();
                            }
                        }
                        else
                        {
                            commandList.Add(new BuzzerCommand(Commands.BuzzerOn, 5000));
                        }
                        break;
                    case "wait":
                        //create a variable for the time and set it to the default incase the parse fails
                        int time = 250;
                        
                        if (modifier != "")
                        {

                            //check if user entered numeric amount
                            if (int.TryParse(modifier, out time))
                            {
                                //user has entered a valid time
                                if (time > 0)
                                {
                                    commandList.Add(new WaitCommand(Commands.Wait, time));
                                }
                                else
                                {
                                    Program.menus[Program.currentMenu].Clear();
                                    Program.menus[Program.currentMenu].Write($"Invalid Modifier '{modifier}': The wait command can only accept positive integers.\n");

                                    Program.menus[Program.currentMenu].Write("\nYour command has not been issued. Press Enter to continue...");
                                    Program.menus[Program.currentMenu].ReadLine();
                                }
                            }
                            else
                            {
                                Program.menus[Program.currentMenu].Clear();
                                Program.menus[Program.currentMenu].Write($"Invalid Modifier '{modifier}': The wait command can only accept a positive integer.\n");


                                Program.menus[Program.currentMenu].Write("\nYour command has not been issued. Press Enter to continue...");
                                Program.menus[Program.currentMenu].ReadLine();
                            }
                        }
                        else
                        {
                            commandList.Add(new WaitCommand(Commands.Wait, time));
                        }
                        break;
                    case "stop":
                        if (modifier != "")
                        {
                            //check if user entered numeric amount
                            if (modifier.ToLower().Contains("all") | modifier.ToLower().Contains("motors") | 
                                modifier.ToLower().Contains("led") | modifier.ToLower().Contains("buzzer"))
                            {
                                //things to stop
                                (bool Motors, bool LED, bool Buzzer) stop = (false, false, false);

                                //user has entered a valid modifier
                                //
                                if (modifier.ToLower().Contains("all"))
                                {
                                    stop = (true, true, true);
                                }
                                else
                                {
                                    //yes, these are intentionally NOT connected
                                    if (modifier.ToLower().Contains("motors"))
                                    {
                                        stop.Motors = true;
                                    }

                                    if (modifier.ToLower().Contains("led"))
                                    {
                                        stop.LED = true;
                                    }

                                    if (modifier.ToLower().Contains("buzzer"))
                                    {
                                        stop.Buzzer = true;
                                    }
                                }

                                //create the command
                                BasicCommand stopCommand = new BasicCommand(Commands.Stop);

                                //apply the modifiers
                                stopCommand.SetShutoff(stop.Motors, stop.LED, stop.Buzzer);

                                //add the command to the list
                                commandList.Add(stopCommand);
                            }
                            else
                            {
                                Program.menus[Program.currentMenu].Clear();
                                Program.menus[Program.currentMenu].Write($"Invalid Modifier '{modifier}': The stop command can only accept these modifiers.\n");
                                Program.menus[Program.currentMenu].Write("                                                           - All\n");
                                Program.menus[Program.currentMenu].Write("                                                           - Motors\n");
                                Program.menus[Program.currentMenu].Write("                                                           - LED\n");
                                Program.menus[Program.currentMenu].Write("                                                           - Buzzer\n");

                                Program.menus[Program.currentMenu].Write("\nYour command has not been issued. Press Enter to continue...");
                                Program.menus[Program.currentMenu].ReadLine();
                            }
                        }
                        else
                        {
                            commandList.Add(new BasicCommand(Commands.Stop));
                        }
                        break;
                    case "done":
                        settingCommands = false;
                        break;
                    case "help":
                        Program.menus[Program.currentMenu].Clear();
                        Program.menus[Program.currentMenu].WriteLine("Enter your command with the format 'command modifier'\n");
                        for (int i = 0; i < Enum.GetNames(typeof(Commands)).Length; ++i)  
                        {
                            string commandName = Enum.GetNames(typeof(Commands))[i];
                            string modifiersAvailable = "";

                            if (commandName == "SetLED")
                            {
                                modifiersAvailable = "Red, Blue, Green, Yellow, \n                                        " +
                                    "White, Purple, Aqua, Orange, Off";
                            }
                            else if (commandName == "BuzzerOn")
                            {
                                modifiersAvailable = "1 - 22,000";
                            }
                            else if (commandName == "Wait")
                            {
                                modifiersAvailable = "'Any positive integer'";
                            }
                            else if (commandName == "Forward" | commandName == "Back" | commandName == "Right" | commandName == "Left")
                            {
                                modifiersAvailable = "0 - 255";
                            }
                            else if (commandName == "Stop")
                            {
                                modifiersAvailable = "All, Motor, LED, Buzzer";
                            }

                            while (commandName.Length<15)
                            {
                                commandName += " ";
                            }

                            Program.menus[Program.currentMenu].WriteLine($"     Command: {commandName}     Modifiers: {modifiersAvailable}");
                        }

                        Program.menus[Program.currentMenu].showInputCursor = false;
                        Program.menus[Program.currentMenu].RefreshMenu();
                        Program.menus[Program.currentMenu].WriteLine("\n\nPress any key to continue...");

                        Console.ReadKey();
                        Program.menus[Program.currentMenu].showInputCursor = true;

                        break;
                    case "delete":
                        if (commandList.Count > 0) commandList.RemoveAt(commandList.Count - 1);
                        break;
                    default:
                        Program.menus[Program.currentMenu].showInputCursor = false;
                        Program.menus[Program.currentMenu].RefreshMenu();
                        Program.menus[Program.currentMenu].WriteLine("\n\nPlease enter a valid command, you may also type 'help' for a list of all commands and their modifiers");
                        Program.menus[Program.currentMenu].WriteLine("\nPress any key to continue...");

                        Console.ReadKey();
                        Program.menus[Program.currentMenu].showInputCursor = true;
                        break;
                }

                Program.menus[Program.currentMenu].RefreshMenu();
            }
        }

        static int[] ConvertColor(Colors color)
        {
            int[] rgb = { 0, 0, 0 };

            switch (color)
            {
                case Colors.red:
                    rgb[0] = 255;
                    rgb[1] = 0;
                    rgb[2] = 0;
                    break;
                case Colors.green:
                    rgb[0] = 0;
                    rgb[1] = 255;
                    rgb[2] = 0;
                    break;
                case Colors.blue:
                    rgb[0] = 0;
                    rgb[1] = 0;
                    rgb[2] = 255;
                    break;
                case Colors.aqua:
                    rgb[0] = 0;
                    rgb[1] = 255;
                    rgb[2] = 255;
                    break;
                case Colors.orange:
                    rgb[0] = 255;
                    rgb[1] = 155;
                    rgb[2] = 0;
                    break;
                case Colors.purple:
                    rgb[0] = 255;
                    rgb[1] = 0;
                    rgb[2] = 255;
                    break;
                case Colors.white:
                    rgb[0] = 255;
                    rgb[1] = 255;
                    rgb[2] = 255;
                    break;
                case Colors.yellow:
                    rgb[0] = 255;
                    rgb[1] = 255;
                    rgb[2] = 0;
                    break;
                case Colors.off:
                    rgb[0] = 0;
                    rgb[1] = 0;
                    rgb[2] = 0;
                    break;
            }

            return rgb;
        }

        static public List<BasicCommand> commandList
        {
            get;
            private set;
        }
        const int DEFAULT_SPEED = 100;
    }
}
