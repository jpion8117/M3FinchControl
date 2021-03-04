using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3FinchControl
{
    enum timeUnits
    {
        milliseconds = 1,
        seconds = 1000,
        minutes = 60000,
        hours = 3600000
    }
    class DataRecorder
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="numberOfRecords">How many times the finch will take data. Default is 8</param>
        /// <param name="timeBetweenRecords">How much time will pass between data points. Default is 10</param>
        /// <param name="timeUnit">The unit of time used in the delay. This can be set to one of 3 options with timeUnits.seconds being the 
        /// default. The other options available are timeUnits.milliseconds and timeUnits.minutes</param>
        public DataRecorder(int numberOfRecords = 8, int timeBetweenRecords = 10, timeUnits timeUnit = timeUnits.seconds)
        {
            NUMBER_OF_RECORDS = numberOfRecords;
            TIME_BETWEEN_RECORDS = timeBetweenRecords;
            TIME_SCALE = (int)timeUnit;
            TIME_UNIT = timeUnit;

            temperatures = new int[NUMBER_OF_RECORDS];
            dataAccuired = false;
        }

        /// <summary>
        /// function begins recording data and ends when all data points have been recorded
        /// </summary>
        /// <param name="myFinch">Finch object used to control the finch</param>
        /// <param name="curentMenu">Reference to the current menu to handle IO operations</param>
        public void Start(FinchAPI.Finch myFinch, ref Menu curentMenu)
        {
            string line;

            //let the user know the finch is in recording mode
            myFinch.setLED(255, 128, 0);

            //write the data table head
            curentMenu.Clear();
            curentMenu.WriteLine(" Entry  | Temp C   | Temp F");
            curentMenu.WriteLine("========|==========|=======");

            for (int i = 0; i < NUMBER_OF_RECORDS; ++i)
            {
                //get the temperature and convert it to an int
                //What! I like whole numbers unless there is a good reason for decimal points...
                temperatures[i] = (int)myFinch.getTemperature();

                //add most recent data entry to the output
                line = "#" + i.ToString("D3") + "    | " + temperatures[i].ToString("D3") + "\u00b0C    | " + CelsiusToFahrenheit(temperatures[i]).ToString("D3") + "\u00b0F";
                curentMenu.WriteLine(line);

                //pause the system until it's time to take another data point
                myFinch.wait(TIME_BETWEEN_RECORDS * TIME_SCALE);
            }

            //mark the data as aquired so other parts of the program may call on it
            dataAccuired = true;

            //let the user know the data is finished recording
            myFinch.setLED(0, 255, 0);
            myFinch.noteOn(5000);
            myFinch.wait(500);
            myFinch.noteOff();

            curentMenu.WriteLine("\nData Recording Complete.");
            curentMenu.WriteLine("You may view the data by selecting the \"View Recordings\" option.");
            curentMenu.WriteLine("Press any key to continue...");
            Console.ReadKey();
            curentMenu.Clear();
        }
        int CelsiusToFahrenheit(int tempC)
        {
            return (int)(tempC * 1.8 + 32);
        }

        public readonly int NUMBER_OF_RECORDS;
        public readonly int TIME_BETWEEN_RECORDS;
        public readonly int TIME_SCALE;
        public readonly timeUnits TIME_UNIT;
        public int[] temperatures
        {
            private set;
            get;
        }
        public bool dataAccuired
        {
            private set;
            get;
        }

    }
}
