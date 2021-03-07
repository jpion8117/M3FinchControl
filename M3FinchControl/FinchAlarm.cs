using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3FinchControl
{
    static class FinchAlarm
    {
        static FinchAlarm()
        {
            lowAlarm = new List<Alert>();
            highAlarm = new List<Alert>();
            alarmPhaseOne = true;
            finalAlarmCycle = 0;
        }

        /// <summary>
        /// Defines the type of sensor to be used. At this point this can be either a light or temperature sensor
        /// </summary>
        public enum Sensor
        {
           light,
           lightRight,
           lightLeft,
           temperatureF,
           temperatureC
        }

        /// <summary>
        /// Defines the trigger and action of an alarm
        /// </summary>
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
        public struct Alert : IComparable<Alert>
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
        {
            public Alert(string action = "__NULL__", int trigger = 0)
            {
                actionMessage = action;
                triggerValue = trigger;
                activeAlert = false;
            }

            public int CompareTo(Alert other)
            {
                if (this.triggerValue > other.triggerValue)
                {
                    return 1;
                }
                else if (this.triggerValue < other.triggerValue)
                {
                    return -1;
                }

                return 0;
            }

            #region comparison operators
            public static bool operator ==(int trigger, Alert alert)
            {
                return trigger == alert.triggerValue;
            }
            public static bool operator !=(int trigger, Alert alert)
            {
                return trigger != alert.triggerValue;
            }
            public static bool operator >=(int trigger, Alert alert)
            {
                return trigger >= alert.triggerValue;
            }
            public static bool operator <=(int trigger, Alert alert)
            {
                return trigger <= alert.triggerValue;
            }
            public static bool operator >(int trigger, Alert alert)
            {
                return trigger > alert.triggerValue;
            }
            public static bool operator <(int trigger, Alert alert)
            {
                return trigger < alert.triggerValue;
            }
            public static bool operator ==(Alert alert, int trigger)
            {
                return trigger == alert.triggerValue;
            }
            public static bool operator !=(Alert alert, int trigger)
            {
                return trigger != alert.triggerValue;
            }
            public static bool operator >=(Alert alert, int trigger)
            {
                return trigger >= alert.triggerValue;
            }
            public static bool operator <=(Alert alert, int trigger)
            {
                return trigger <= alert.triggerValue;
            }
            public static bool operator >(Alert alert, int trigger)
            {
                return trigger > alert.triggerValue;
            }
            public static bool operator <(Alert alert, int trigger)
            {
                return trigger < alert.triggerValue;
            }
            public static bool operator >=(Alert trigger, Alert alert)
            {
                return trigger.triggerValue >= alert.triggerValue;
            }
            public static bool operator <=(Alert trigger, Alert alert)
            {
                return trigger.triggerValue <= alert.triggerValue;
            }
            public static bool operator >(Alert trigger, Alert alert)
            {
                return trigger.triggerValue > alert.triggerValue;
            }
            public static bool operator <(Alert trigger, Alert alert)
            {
                return trigger.triggerValue < alert.triggerValue;
            }
            #endregion

            public string actionMessage;
            public int triggerValue;
            public bool activeAlert;
        }

        /// <summary>
        /// Configure the alarm with the default alarm messages and only one low and high threshold
        /// </summary>
        /// <param name="lowThreshold">set the low threshold</param>
        /// <param name="highThreshold">set the high threshold</param>
        /// <param name="alarmType">set which sensor to use, light(average of both), lightRight, lightLeft, temperature(default)</param>
        static public void ConfigureAlarm(int lowThreshold, int highThreshold, Sensor alarmType = Sensor.temperatureF,
            int monitoringTime = 5, timeUnits unit = timeUnits.minutes)
        {
            //set basic parameters
            dataSensor = alarmType;
            unitOfTime = unit;
            timeToMonitor = monitoringTime;

            //verify correct thresholds
            if (!VerifyAlertValidity())
            {
                throw new ArgumentOutOfRangeException("No condition exists to stop alarm! All low alerts must be less than all high alerts.");
            }

            //set the info strings
            SetInfoStrings();

            //set the final alarm cycle
            finalAlarmCycle = (ulong)(timeToMonitor * (int)unitOfTime);
        }
        /// <summary>
        /// Configure the alarm with the default alarm messages and multiple low and high thresholds. All low thresholds must be less 
        /// than all high thresholds. 
        /// </summary>
        /// <param name="lowThresholds">set the low thresholds</param>
        /// <param name="highThresholds">set the high thresholds</param>
        /// <param name="alarmType">set which sensor to use, light(average of both), lightRight, lightLeft, temperature(default)</param>
        static public void ConfigureAlarm(int[] lowThresholds, int[] highThresholds, Sensor alarmType = Sensor.temperatureF,
            int monitoringTime = 5, timeUnits unit = timeUnits.minutes)
        {
            //set the dataSensor
            dataSensor = alarmType;
            unitOfTime = unit;
            timeToMonitor = monitoringTime;

            //set low thresholds
            for (int index = 0; index < lowThresholds.Length; ++index) 
            {
                lowAlarm.Add(new Alert(DEFAULT_MESSAGE_LOW, lowThresholds[index]));
            }

            //set high thresholds
            for (int index = 0; index < highThresholds.Length; ++index)
            {
                highAlarm.Add(new Alert(DEFAULT_MESSAGE_HIGH, highThresholds[index]));
            }

            //verify correct thresholds
            if (!VerifyAlertValidity())
            {
                throw new ArgumentOutOfRangeException("No condition exists to stop alarm! All low alerts must be less than all high alerts.");
            }

            //set the info strings
            SetInfoStrings();

            //set the final alarm cycle
            finalAlarmCycle = (ulong)(timeToMonitor * (int)unitOfTime);
        }
        /// <summary>
        /// Configure the alarm with the default alarm messages and only one low and high threshold
        /// </summary>
        /// <param name="lowAlerts">set the low thresholds and their messages</param>
        /// <param name="highAlerts">set the high thresholds and their messages</param>
        /// <param name="alarmType">set which sensor to use, light(average of both), lightRight, lightLeft, temperature(default)</param>
        static public void ConfigureAlarm(Alert[] lowAlerts, Alert[] highAlerts, Sensor alarmType = Sensor.temperatureF, 
            int monitoringTime = 5, timeUnits unit = timeUnits.minutes)
        {
            //set data sensor
            dataSensor = alarmType;
            unitOfTime = unit;
            timeToMonitor = monitoringTime;

            //set high thresholds
            for (int index = 0; index < highAlerts.Length; ++index)
            {
                lowAlarm.Add(highAlerts[index]);
            }

            //set low thresholds
            for (int index = 0; index < lowAlerts.Length; ++index)
            {
                highAlarm.Add(lowAlerts[index]);
            }

            //verify correct thresholds
            if (!VerifyAlertValidity())
            {
                throw new ArgumentOutOfRangeException("No condition exists to stop alarm! All low alerts must be less than all high alerts.");
            }

            //set the info strings
            SetInfoStrings();

            //set the final alarm cycle
            finalAlarmCycle = (ulong)(timeToMonitor * (int)unitOfTime);
        }

        static public void GetConfigFromUser()
        {
            // *************
            // * Variables *
            // *************
            string validatedInput = "";
            string option = "";
            string prompt = "";
            string lowThresholdStr = "";
            string highThresholdStr = "";
            int validatedInt = 0;
            bool makingSelection = true;
            int min = 1;
            int max = 0;

            //get thresholds in string form.
            for (int index = 0; index < lowAlarm.Count; ++index)
            {
                lowThresholdStr += lowAlarm[index].triggerValue.ToString();

                //add a space and a comma to every one except the last one.
                if (index < lowAlarm.Count - 1)
                {
                    lowThresholdStr += ", ";
                }
            }
            for (int index = 0; index < highAlarm.Count; ++index)
            {
                highThresholdStr += highAlarm[index].triggerValue.ToString();

                //add a space and a comma to every one except the last one.
                if (index < highAlarm.Count - 1)
                {
                    highThresholdStr += ", ";
                }
            }

            //ask the user if they want to change the configuration
            prompt = "Current Configuration..." +
                     "\n                         Time to run: " + timeToMonitor.ToString() + unitOfTimeStr +
                     "\n                   Sensor to monitor: " + dataSensorStr +
                     "\n              Low alert threshold(s): " + lowThresholdStr +
                     "\n             High alert threshold(s): " + highThresholdStr +
                     "\n\nWould you like to change the current configuration? ";

            validatedInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" }, 
                "Invalid Option: Please enter yes or no. Press any key to continue...");
            if (validatedInput == "yes" | validatedInput == "y")
            {
                //ask user what sensor they would like to use
                while (makingSelection)
                {
                    prompt = "What sensor would you like to monitor?\n" +
                             "     * Light\n" +
                             "     * Right Light\n" +
                             "     * Left Light\n" +
                             "     * Temperature (\u00b0F) (type: \"Temperature F)\"\n" +
                             "     * Temperature (\u00b0C) (type: \"Temperature C)\"\n" +
                             "\nChoice: ";
                    option = Program.ValidateInput(prompt,
                        new string[] { "Light", "Right Light", "Left Light", "Temperature F", "Temperature C" });

                    //confirm selection
                    prompt = "You selected " + option.ToLower() + " is this correct? ";
                    validatedInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" },
                        "Invalid Option: Please enter yes or no. Press any key to continue...");
                    if (validatedInput == "yes" | validatedInput == "y")
                    {
                        makingSelection = false;
                    }
                }

                //save configuration
                switch (option)
                {
                    case "Light":
                        dataSensor = Sensor.light;
                        break;
                    case "Right Light":
                        dataSensor = Sensor.lightRight;
                        break;
                    case "Left Light":
                        dataSensor = Sensor.lightLeft;
                        break;
                    case "Temperature F":
                        dataSensor = Sensor.temperatureF;
                        break;
                    case "Temperature C":
                        dataSensor = Sensor.temperatureC;
                        break;
                }

                //ask user what unit of time they want to use
                makingSelection = true;
                while (makingSelection)
                {
                    //ask user what sensor they would like to use
                    prompt = "What unit of time do you want to use to monitor for?\n" +
                             "     * Seconds\n" +
                             "     * Minutes\n" +
                             "     * Hours\n" +
                             "\nChoice: ";
                    option = Program.ValidateInput(prompt,
                        new string[] { "Seconds", "Minutes", "Hours" });

                    //confirm selection
                    prompt = "You selected " + option.ToLower() + " is this correct? ";
                    validatedInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" },
                        "Invalid Option: Please enter yes or no. Press any key to continue...");
                    if (validatedInput == "yes" | validatedInput == "y")
                    {
                        makingSelection = false;
                    }
                }

                //save configuration
                switch (option.ToLower())
                {
                    case "seconds":
                        unitOfTime = timeUnits.seconds;
                        min = 60;
                        max = 1200;
                        break;
                    case "minutes":
                        unitOfTime = timeUnits.minutes;
                        max = 120;
                        break;
                    case "hours":
                        max = 3;
                        unitOfTime = timeUnits.hours;
                        break;
                }

                //ask user to how long they want to monitor the sensor
                SetInfoStrings();
                prompt = "How long in" + unitOfTimeStr.ToLower() + "would you like to monitor for?\n" +
                         "    Test time (" + min.ToString() + "-" + max.ToString() + "): ";
                validatedInt = Program.ValidateInput(prompt, max, true, min, true);

                //save configuration
                timeToMonitor = validatedInt;

                //allow the user to set low thresholds
                lowAlarm = SetAlertThresholds("lower");

                //allow the user to set high thresholds
                highAlarm = SetAlertThresholds("higher");
            }
            else if (validatedInput == "no" | validatedInput == "n")
            {
                Program.menus[Program.currentMenu].Clear();
            }

            //set the final alarm cycle
            finalAlarmCycle = (ulong)(timeToMonitor * (int)unitOfTime);
        }

        static List<Alert> SetAlertThresholds(string currentRange)
        {
            bool makingSelection = true;
            string prompt = "";
            string validatedInput = "";
            string actionMessage = "";
            int triggerValue = 0;
            List<Alert> thresholdList = new List<Alert>();

            while (makingSelection)
            {
                //reset action message
                actionMessage = "";

                //variables to store the min/max settings
                int min = 0;
                int max = 0;
                string sensorType = "";

                //get the sensor's setting range
                if (dataSensor >= Sensor.light & dataSensor < Sensor.temperatureF)
                {
                    //set the min/max
                    min = 0;
                    max = 255;
                    sensorType = "\"light\"";
                }
                else if (dataSensor == Sensor.temperatureF)
                {
                    min = 20;
                    max = 110;
                    sensorType = "\"temperature \u00b0F\"";
                }
                else
                {
                    min = -10;
                    max = 45;
                    sensorType = "\"temperature \u00b0C\"";
                }

                // ******************************
                // * Begin prompt configuration *
                // ******************************

                //let the user know what sensor is being monitored
                prompt = "";
                prompt += "You are currently configuring the " + currentRange.ToLower() + " thresholds for the " + dataSensorStr.ToLower() + " sensor\n";
                prompt += "    - sensors of the type " + sensorType + " may have values between " +
                    min.ToString() + " and " + max.ToString() + "\n";

                //list the threshold alarms currently in use
                prompt += "\n\n";
                prompt += " Threshold | Sound | Light | Message \n";
                prompt += "===========|=======|=======|==============================================================\n";

                if (thresholdList.Count == 0)
                {
                    prompt += "           ***  There are currently no " + currentRange.ToLower() + " threshold alerts set  ***";
                }
                else
                {
                    for (int index = 0; index < thresholdList.Count; index++)
                    {
                        //local var
                        bool soundAlert = false;
                        bool lightAlert = false;
                        bool messageAlert = false;
                        string message = "";

                        //search for tags
                        soundAlert = CheckTag(thresholdList[index].actionMessage, SET_BUZZ_TAG_1) | CheckTag(thresholdList[index].actionMessage, SET_BUZZ_TAG_2);
                        lightAlert = CheckTag(thresholdList[index].actionMessage, SET_LED_TAG_1) | CheckTag(thresholdList[index].actionMessage, SET_LED_TAG_2);
                        messageAlert = ExtractTag(thresholdList[index].actionMessage, OUTPUT_MESSAGE_TAG, out message);

                        //        " Threshold | Sound | Light | Message ";
                        //        "===========|=======|=======|==============================================================";

                        //add the alert threshold to the prompt
                        prompt += "       " + thresholdList[index].triggerValue.ToString("D3") + " |";

                        //add yes or no for sound alert
                        if (soundAlert)
                        {
                            prompt += "  Yes  |";
                        }
                        else
                        {
                            prompt += "  No   |";
                        }

                        //add yes or no for light alert
                        if (lightAlert)
                        {
                            prompt += "  Yes  |";
                        }
                        else
                        {
                            prompt += "  No   |";
                        }

                        //if message is longer than the table length, shorten it
                        if (messageAlert)
                        {
                            if (message.Length > 57)
                            {
                                message = message.Remove(64);
                                message += "...";
                            }

                            //add the message to this line of the prompt
                            prompt += ' ' + message + '\n';
                        }
                        else
                        {
                            prompt += " -- output message disabled --\n";
                        }
                    }
                }

                prompt += "\n\nWould you like to add a new " + currentRange.ToLower() + " threshold alarm? ";

                // ****************************
                // * End prompt configuration *
                // ****************************

                //ask the user if they want to add another threshold
                validatedInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" },
                    "INVALID INPUT: Please enter yes or no.\n\nPress any key to continue...");

                //act on user response
                if (validatedInput == "yes" | validatedInput == "y")
                {
                    //localized variables
                    bool validAlert = false;
                    int validatedInt = 0;

                    while (!validAlert)
                    {
                        //ask user when the alert should be triggered
                        prompt = "Sensors of type " + sensorType.ToLower() + " can be set to trigger between " + min.ToString() + " and " + max.ToString() + ".\n" +
                                 "     At what threshold would you like to trigger this alert? ";

                        triggerValue = Program.ValidateInput(prompt, max, true, min, true);

                        //ask the user if they want to use sound alerts
                        prompt = "Would you like to include a sound feature with this alert? ";
                        validatedInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" },
                            "INVALID INPUT: Please enter yes or no.\n\nPress any key to continue...");

                        if (validatedInput == "yes" | validatedInput == "y")
                        {
                            //set the min/max setting for a sound based alert
                            min = 1000;
                            max = 20000;

                            //set first buzzer frequency
                            while (!validAlert)
                            {
                                prompt = "What would you like to use as the first frequency? (" + min.ToString() + "-" + max.ToString() + " or 0 for off): ";
                                validatedInt = Program.ValidateInput(prompt, max, true);

                                if(validatedInt == 0 | validatedInt >= min)
                                {
                                    actionMessage += SET_BUZZ_TAG_1 + validatedInt.ToString() + ";";
                                    break;
                                }
                            }

                            //set second buzzer frequency
                            while (!validAlert)
                            {
                                prompt = "What would you like to use as the second frequency? (" + min.ToString() + "-" + max.ToString() + " or 0 for off): ";
                                validatedInt = Program.ValidateInput(prompt, max, true);

                                if (validatedInt == 0 | validatedInt >= min)
                                {
                                    actionMessage += SET_BUZZ_TAG_2 + validatedInt.ToString() + ";";
                                    break;
                                }
                            }

                            validAlert = true;
                        }

                        //ask the user if they want to use light alerts
                        prompt = "Would you like to include a light feature with this alert? ";
                        validatedInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" },
                            "INVALID INPUT: Please enter yes or no.\n\nPress any key to continue...");
                        if (validatedInput == "yes" | validatedInput == "y")
                        {
                            //reset localized variables
                            validAlert = false;
                            validatedInt = 0;

                            //set the min/max setting for a sound based alert
                            min = 0;
                            max = 255;

                            int[] rgb = { 0, 0, 0 };
                            string[] rgbDescriptor = { "red", "green", "blue" };

                            while (!validAlert)
                            {
                                //set the first light color
                                for (int index = 0; index < rgb.Length; ++index)
                                {
                                    prompt = "Please enter a value for the color " + rgbDescriptor[index] + " between " + min.ToString() + "-" + max.ToString() + ": ";
                                    rgb[index] = Program.ValidateInput(prompt, max, true, min, true);
                                }

                                //confirm color selection
                                prompt = "Here is the RGB based color you have requested\n" +
                                         "    Red: " + rgb[0].ToString() + "\n" +
                                         "   Blue: " + rgb[1].ToString() + "\n" +
                                         "  Green: " + rgb[2].ToString() + "\n" +
                                         "\nIs this correct? ";

                                validatedInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" },
                                    "INVALID INPUT: Please enter yes or no.\n\nPress any key to continue...");

                                //add validated alert to the actionMessage and break loop
                                if (validatedInput == "yes" | validatedInput == "y")
                                {
                                    actionMessage += SET_LED_TAG_1 + rgb[0].ToString() + "," + rgb[1].ToString() + "," + rgb[2].ToString() + ";";
                                    validAlert = true;
                                }
                            }

                            //reset validAlert
                            validAlert = false;
                            while (!validAlert)
                            {
                                //set the second light color
                                for (int index = 0; index < rgb.Length; ++index)
                                {
                                    prompt = "Please enter a value for the color " + rgbDescriptor[index] + " between " + min.ToString() + "-" + max.ToString() + ": ";
                                    rgb[index] = Program.ValidateInput(prompt, max, true, min, true);
                                }

                                //confirm color selection
                                prompt = "Here is the RGB based color you have requested\n" +
                                         "    Red: " + rgb[0].ToString() + "\n" +
                                         "  Green: " + rgb[1].ToString() + "\n" +
                                         "   Blue: " + rgb[2].ToString() + "\n" +
                                         "\nIs this correct? ";

                                validatedInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" },
                                    "INVALID INPUT: Please enter yes or no.\n\nPress any key to continue...");

                                //add validated alert to the actionMessage and break loop
                                if (validatedInput == "yes" | validatedInput == "y")
                                {
                                    actionMessage += SET_LED_TAG_2 + rgb[0].ToString() + "," + rgb[1].ToString() + "," + rgb[2].ToString() + ";";
                                    validAlert = true;
                                }
                            }
                        }

                        //ask the user if they want to use message alerts
                        prompt = "Would you like to include a message feature with this alert? ";
                        validatedInput = Program.ValidateInput(prompt, new string[] { "yes", "y", "no", "n" },
                            "INVALID INPUT: Please enter yes or no.\n\nPress any key to continue...");
                        if (validatedInput == "yes" | validatedInput == "y")
                        {
                            prompt = "Please enter the message you would like to display when this threshold is crossed...\n\nMessage: ";
                            actionMessage += OUTPUT_MESSAGE_TAG + Program.ValidateInput(prompt, new char[] { ',', '#', ';' });
                        }

                        //if no alerts were set the 
                        if (actionMessage != "")
                        {
                            //add the validated alert
                            thresholdList.Add(new Alert(actionMessage, triggerValue));

                            //break loop
                            validAlert = true;
                        }
                        else
                        {
                            Program.menus[Program.currentMenu].Clear();
                            Program.menus[Program.currentMenu].WriteLine("No alert options selected! Press any key to continue...");
                            Console.ReadKey();
                        }
                    }
                }
                else
                {
                    makingSelection = false;
                }
            }

            //returns the fully built threshold list
            return thresholdList;
        }

        /// <summary>
        /// checks the thresholds to ensure they are valid (low not greater than high)
        /// </summary>
        /// <returns>true if thresholds are valid</returns>
        static bool VerifyAlertValidity()
        {
            //sort both low and high alerts
            lowAlarm.Sort();
            highAlarm.Sort();

            //check the first element of the high range against the last element of the low range
            if (lowAlarm.Count == 0 | highAlarm.Count == 0) 
            {
                return true;
            }
            else if (lowAlarm.Last() >= highAlarm[0]) 
            {
                return false;
            }

            return true;
        }
        static void SetInfoStrings()
        {
            //set data sensor string
            switch (dataSensor)
            {
                case Sensor.light:
                    dataSensorStr = "Light";
                    break;
                case Sensor.lightLeft:
                    dataSensorStr = "Left Light";
                    break;
                case Sensor.lightRight:
                    dataSensorStr = "Right Light";
                    break;
                case Sensor.temperatureF:
                    dataSensorStr = "Temperature (\u00b0F)";
                    break;
                case Sensor.temperatureC:
                    dataSensorStr = "Temperature (\u00b0C)";
                    break;
            }

            //set time unit string
            switch (unitOfTime)
            {
                case timeUnits.seconds:
                    unitOfTimeStr = " Seconds ";
                    break;
                case timeUnits.minutes:
                    unitOfTimeStr = " Minutes ";
                    break;
                case timeUnits.hours:
                    unitOfTimeStr = " Hours ";
                    break;
            }
        }

        static public void ProcessAlarmCycle(bool switchCycle)
        {
            // *************
            // * Variables *
            // *************
            int sensorData = 0;

            //determine which sensor is being monitored and poll it
            switch (dataSensor)
            {
                case Sensor.light:
                    sensorData = (int)Program.myFinch.getLightSensors().Average();
                    break;
                case Sensor.lightLeft:
                    sensorData = (int)Program.myFinch.getLeftLightSensor();
                    break;
                case Sensor.lightRight:
                    sensorData = (int)Program.myFinch.getRightLightSensor();
                    break;
                case Sensor.temperatureF:
                    sensorData = Program.CelsiusToFahrenheit(Program.myFinch.getTemperature());
                    break;
                case Sensor.temperatureC:
                    sensorData = (int)Program.myFinch.getTemperature();
                    break;
            }

            //flip phase every second
            alarmPhaseOne = switchCycle;

            //reset finch
            if (!alarmActive)
            {
                Program.myFinch.setLED(0, 255, 0);
                Program.myFinch.noteOff();
            }

            //set alarm active to false
            alarmActive = false;

            //check low threshold
            if (lowAlarm.Count != 0)
            {
                //sort the list
                lowAlarm.Sort();

                foreach (Alert currentCheck in lowAlarm)
                {
                    if (currentCheck.triggerValue > sensorData)
                    {
                        //process the alarm output
                        ProcessAlarmMessage(currentCheck.actionMessage);
                        alarmActive = true;
                        break;
                    }
                }
            }

            //check high threshold
            if (highAlarm.Count != 0)
            {
                //sort the list
                highAlarm.Sort();
                highAlarm.Reverse();

                for (int index = 0; index < highAlarm.Count; ++index)
                {
                    if (highAlarm[index].triggerValue < sensorData)
                    {
                        //process the alarm output
                        ProcessAlarmMessage(highAlarm[index].actionMessage);
                        alarmActive = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// gives various feedback actions based on the input of an "actionMessage" from an alarm that has been triggered
        /// </summary>
        /// <param name="actionMessage">actionMessage of the currently triggered alarm</param>
        static private void ProcessAlarmMessage(string actionMessage)
        {
            //create a list to store the actions
            List<string> actions = new List<string>();

            int actionStart = 0;

            //separate different action tags
            for (int index = 0; index < actionMessage.Length; ++index)
            {
                //look for the start char and note it's location
                if(actionMessage[index] == '#')
                {
                    actionStart = index;
                }
                //look for the end char and add the action message to the actions array
                else if (actionMessage[index] == ';')
                {
                    actions.Add(actionMessage.Substring(actionStart, index - actionStart + 1));
                }
            }

            //run the phase dependant actions
            for (int index = 0; index < actions.Count; ++index)
            {
                //determines if alarm is in phase 1 or 2
                if (alarmPhaseOne)
                {
                    if (actions[index].Contains(SET_LED_TAG_1))
                    {
                        // ************************
                        // * extract the settings *
                        // ************************

                        //remove the tag
                        actions[index] = actions[index].Remove(0, SET_LED_TAG_1.Length);

                        //get the RGB values to set the finch
                        int[] rgb = { 0, 0, 0 };
                        int currentRGB = 0;
                        string input = "";

                        for (int rgbIndex = 0; rgbIndex < actions[index].Length; ++rgbIndex)
                        {
                            //look for the ',' or ';'
                            if (actions[index][rgbIndex] == ',' | actions[index][rgbIndex] == ';')
                            {
                                if (int.TryParse(input, out rgb[currentRGB]))
                                {
                                    currentRGB++;
                                    input = "";
                                }
                                else
                                {
                                    //throws an exception if the argument rgb[currentRGB] contains anything other than numbers
                                    throw new ArgumentException("Invalid arguement in RGB string.");
                                }
                            }
                            else
                            {
                                input += actions[index][rgbIndex];
                            }
                        }

                        //set the light
                        Program.myFinch.setLED(rgb[0], rgb[1], rgb[2]);
                    }
                    else if (actions[index].Contains(SET_BUZZ_TAG_1))
                    {
                        // ************************
                        // * extract the settings *
                        // ************************

                        //remove the tag
                        actions[index] = actions[index].Remove(0, SET_BUZZ_TAG_1.Length);
                        
                        //variables needed for the extraction
                        int frequency = 0;
                        string input = "";

                        //extract the frequency setting
                        for (int freqIndex = 0; freqIndex < actions[index].Length; ++freqIndex)
                        {
                            if(actions[index][freqIndex] == ';')
                            {
                                if (!int.TryParse(input, out frequency))
                                {
                                    //throws an exception if the argument input contains anything other than numbers
                                    throw new ArgumentException("Invalid arguement in input string.");
                                }

                                break;
                            }

                            //add current char
                            input += actions[index][freqIndex];
                        }

                        //set the finch buzzer
                        if (frequency == 0)
                        {
                            Program.myFinch.noteOff();
                        }
                        else
                        {
                            Program.myFinch.noteOn(frequency);
                        }
                    }
                }
                else
                {
                    if (actions[index].Contains(SET_LED_TAG_2))
                    {
                        // ************************
                        // * extract the settings *
                        // ************************

                        //remove the tag
                        actions[index] = actions[index].Remove(0, SET_LED_TAG_1.Length);

                        //get the RGB values to set the finch
                        int[] rgb = { 0, 0, 0 };
                        int currentRGB = 0;
                        string input = "";

                        for (int rgbIndex = 0; rgbIndex < actions[index].Length; ++rgbIndex)
                        {
                            //look for the ',' or ';'
                            if (actions[index][rgbIndex] == ',' | actions[index][rgbIndex] == ';')
                            {
                                if (int.TryParse(input, out rgb[currentRGB]))
                                {
                                    currentRGB++;
                                    input = "";
                                }
                                else
                                {
                                    //throws an exception if the argument rgb[currentRGB] contains anything other than numbers
                                    throw new ArgumentException("Invalid arguement in RGB string.");
                                }
                            }
                            else
                            {
                                input += actions[index][rgbIndex];
                            }
                        }

                        //set the light
                        Program.myFinch.setLED(rgb[0], rgb[1], rgb[2]);
                    }
                    else if (actions[index].Contains(SET_BUZZ_TAG_2))
                    {
                        // ************************
                        // * extract the settings *
                        // ************************

                        //remove the tag
                        actions[index] = actions[index].Remove(0, SET_BUZZ_TAG_2.Length);

                        //variables needed for the extraction
                        int frequency = 0;
                        string input = "";

                        //extract the frequency setting
                        for (int freqIndex = 0; freqIndex < actions[index].Length; ++freqIndex)
                        {
                            if (actions[index][freqIndex] == ';')
                            {
                                if (!int.TryParse(input, out frequency))
                                {
                                    //throws an exception if the argument input contains anything other than numbers
                                    throw new ArgumentException("Invalid arguement in input string.");
                                }

                                break;
                            }

                            //add current char
                            input += actions[index][freqIndex];
                        }

                        //set the finch buzzer
                        if (frequency == 0)
                        {
                            Program.myFinch.noteOff();
                        }
                        else
                        {
                            Program.myFinch.noteOn(frequency);
                        }
                    }
                }

                if (actions[index].Contains(OUTPUT_MESSAGE_TAG))
                {
                    // ***********************
                    // * extract the message *
                    // ***********************

                    //remove the tag
                    actions[index] = actions[index].Remove(0, OUTPUT_MESSAGE_TAG.Length);

                    //output the message to the console
                    Program.menus[Program.currentMenu].WriteLine(actions[index], true);
                }
            }
        }

        /// <summary>
        /// Searches an actionMessage string for a specific data tag and extracts the data.
        /// </summary>
        /// <param name="actionMessage">Message to search</param>
        /// <param name="tag">Target tag</param>
        /// <param name="tagData">OUT: returns a string with the tag data</param>
        /// <returns>returns a boolean representing if the tag was found in the string</</returns>
        static private bool ExtractTag(string actionMessage, string tag, out string tagData)
        {
            //
            //
            //
            string data = "";
            bool beginSearch = false;
            int tagLocation = actionMessage.LastIndexOf(tag);

            //check if the tag is exists in the action message
            if (!actionMessage.Contains(tag))
            {
                tagData = "___TAG_NOT_FOUND___";
                return false;
            }

            for (int index = tagLocation; index < actionMessage.Length; ++index)
            {
                //find the tag start
                if (actionMessage[index] == ':')
                {
                    beginSearch = true;
                }
                //find the end
                else if (actionMessage[index] == ';')
                {
                    beginSearch = false;
                    break;
                }
                //process tag data
                else if (beginSearch)
                {
                    data += actionMessage[index];
                }
            }

            //return results
            tagData = data;
            return true;
        }

        /// <summary>
        /// Searches an actionMessage string for a specific data tag to see if it exists.
        /// </summary>
        /// <param name="actionMessage">Message to search</param>
        /// <param name="tag">Target tag</param>
        /// <returns>returns a boolean representing if the tag was found in the string</</returns>
        static private bool CheckTag(string actionMessage, string tag)
        {
            //check if the tag is exists in the action message
            if (!actionMessage.Contains(tag))
            {
                return false;
            }

            //if the tag exists it will return true
            return true;
        }

        static public Sensor dataSensor
        {
            private set;
            get;
        }
        static public Alert currentAlert
        {
            private set;
            get;
        }
        static public int timeToMonitor
        {
            private set;
            get;
        }
        static public timeUnits unitOfTime
        {
            private set;
            get;
        }
        static public string unitOfTimeStr
        {
            private set;
            get;
        }
        static public string dataSensorStr
        {
            private set;
            get;
        }

        static private List<Alert> lowAlarm;
        static private List<Alert> highAlarm;
        static private bool alarmPhaseOne;
        static private bool alarmActive;
        static public ulong finalAlarmCycle
        {
            private set;
            get;
        }

        //default action messages
        const string DEFAULT_MESSAGE_LOW = OUTPUT_MESSAGE_TAG + "Low threshold reached.;";
        const string DEFAULT_MESSAGE_HIGH = OUTPUT_MESSAGE_TAG + "Upper threshold reached.;";
        
        //action message tags
        const string OUTPUT_MESSAGE_TAG = "#OUTPUT_MESSAGE:";
        const string SET_LED_TAG_1 = "#SET_LED_1:";
        const string SET_BUZZ_TAG_1 = "#SET_BUZZ_1:";
        const string SET_LED_TAG_2 = "#SET_LED_2:";
        const string SET_BUZZ_TAG_2 = "#SET_BUZZ_2:";
    }
}