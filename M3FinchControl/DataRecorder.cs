using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3FinchControl
{
    enum timeUnits
    {
        miliseconds = 1,
        seconds = 1000,
        minutes = 60000
    }
    class DataRecorder
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="numberOfRecords">How many times the finch will take data. Default is 8</param>
        /// <param name="timeBetweenRecords">How much time will pass between data points. Default is 10</param>
        /// <param name="timeUnit">The unit of time used in the delay. This can be set to one of 3 options with timeUnits.seconds being the 
        /// default. The other options available are timeUnits.miliseconds and timeUnits.minutes</param>
        public DataRecorder(int numberOfRecords = 8, int timeBetweenRecords = 10, timeUnits timeUnit = timeUnits.seconds)
        {
            NUMBER_OF_RECORDS = numberOfRecords;
            TIME_BETWEEN_RECORDS = timeBetweenRecords;
            TIME_SCALE = (int)timeUnit;

            temperatures = new int[NUMBER_OF_RECORDS];
        }

        /// <summary>
        /// function begins recording data and ends when all data points have been recorded
        /// </summary>
        /// <param name="myFinch">Finch object used to control the finch</param>
        /// <param name="curentMenu">Reference to the current menu to handle IO operations</param>
        public void Start(FinchAPI.Finch myFinch, ref Menu curentMenu)
        {
            string line;

            //write the data table head
            curentMenu.Clear();
            curentMenu.WriteLine(" Entry  | Temp C   | Temp F");

            for(int i = 0; i < NUMBER_OF_RECORDS; ++i)
            {
                //get the temperature and convert it to an int
                //What! I like whole numbers unless there is a good reason for decimal points...
                temperatures[i] = (int)myFinch.getTemperature();

                //add most recent data entry to the output
                line = "#" + i.ToString("D3") + "    | " + temperatures[i].ToString("D3") + "\u00b0C    | " + CelsiusToFahrenheit(temperatures[i]).ToString("D3") + "\u00b0F";
                curentMenu.WriteLine(line);

                //pause the system until it's time to take another data point
                System.Threading.Thread.Sleep(TIME_BETWEEN_RECORDS * TIME_SCALE);
            }
        }
        int CelsiusToFahrenheit(int tempC)
        {
            return (int)(tempC * 1.8 + 32);
        }

        private readonly int NUMBER_OF_RECORDS;
        private readonly int TIME_BETWEEN_RECORDS;
        private readonly int TIME_SCALE;
        private int[] temperatures;

    }
}
