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

            lowAlarm.Add(new Alert(DEFAULT_MESSAGE_LOW, lowThreshold));
            highAlarm.Add(new Alert(DEFAULT_MESSAGE_HIGH, lowThreshold));

            //verify correct thresholds
            if (!VerifyAlertValidity())
            {
                throw new ArgumentOutOfRangeException("No condition exists to stop alarm! All low alerts must be less than all high alerts.");
            }

            //set the info strings
            SetInfoStrings();
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
        }

        static public void GetConfigFromUser()
        {
            // *************
            // * Variables *
            // *************
            string validatedInput = "";
            string prompt = "";
            string lowThresholdStr = "";
            string highThresholdStr = "";

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

            }
            else if (validatedInput == "no" | validatedInput == "n")
            {
                Program.menus[Program.currentMenu].RefreshMenu(true);
            }
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
            if (lowAlarm.Last() >= highAlarm[0]) 
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

        static string processAlarmCycle()
        {
            // *************
            // * Variables *
            // *************
            string message = "__NO_ALERT__";
            int sensorData = 0;

            //determine which sensor is being monitored and poll it
            switch(dataSensor)
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

            //check low threshold
            
            //check high threshold
            
            //return message
            return message;
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

        const string DEFAULT_MESSAGE_LOW = "Low threshold reached.";
        const string DEFAULT_MESSAGE_HIGH = "Upper threshold reached.";
    }
}
