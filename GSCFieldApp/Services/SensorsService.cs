using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Services
{
    public class SensorsService
    {
        //Localize
        public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        /// <summary>
        /// Will output the azimuth and dip values of the current device
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<double, double>> GetAzimDip()
        {
            double compassReading = 0;
            double accelerometerZ = 0;

            Tuple<double, double> outputAzimDip = new Tuple<double, double>(compassReading, accelerometerZ);

            bool continuTakingValues = true;

            //Prevent compass overstart since it might be already in use in the map page
            bool isAlreadyOn = false;
            if (Compass.Default.IsSupported && Compass.Default.IsMonitoring)
            {
                isAlreadyOn = true;
            }

            try
            {
                while (continuTakingValues)
                {
                    // Get compass heading (azimuth)
                    if (Compass.Default.IsSupported)
                    {
                        
                        var tcs = new TaskCompletionSource<double>();

                        void CompassReadingChanged(object sender, CompassChangedEventArgs e)
                        {
                            compassReading = e.Reading.HeadingMagneticNorth;
                            tcs.TrySetResult(compassReading);
                        }

                        Compass.Default.ReadingChanged += CompassReadingChanged;
                        if (!isAlreadyOn)
                        {
                            Compass.Default.Start(SensorSpeed.Default);
                        }
                        
                        // Wait for reading with timeout
                        await Task.WhenAny(tcs.Task, Task.Delay(500));

                        if (!isAlreadyOn)
                        {
                            Compass.Default.Stop();
                        }

                        Compass.Default.ReadingChanged -= CompassReadingChanged;

                        outputAzimDip = new Tuple<double, double>((int)Math.Round(compassReading), outputAzimDip.Item2);
                    }

                    // Get accelerometer data for dip calculation
                    if (Accelerometer.Default.IsSupported)
                    {
                        var tcs = new TaskCompletionSource<double>();

                        void AccelerometerReadingChanged(object sender, AccelerometerChangedEventArgs e)
                        {
                            accelerometerZ = e.Reading.Acceleration.Z;
                            tcs.TrySetResult(accelerometerZ);
                        }

                        Accelerometer.Default.ReadingChanged += AccelerometerReadingChanged;
                        Accelerometer.Default.Start(SensorSpeed.Default);

                        // Wait for reading with timeout
                        await Task.WhenAny(tcs.Task, Task.Delay(500));

                        Accelerometer.Default.Stop();
                        Accelerometer.Default.ReadingChanged -= AccelerometerReadingChanged;

                        // Calculate dip angle from Z component (tilt from vertical)
                        double dip = Math.Acos(Math.Abs(accelerometerZ)) * (180 / Math.PI);
                        outputAzimDip = new Tuple<double, double>(outputAzimDip.Item1, (int)Math.Round(Math.Clamp(dip, 0, 90)));
                    }

                    continuTakingValues = await Shell.Current.DisplayAlert(LocalizationResourceManager["SensorServiceDisplayTitle"].ToString(),
                        string.Format(LocalizationResourceManager["SensorServiceDisplayMessage"].ToString(), outputAzimDip.Item1, outputAzimDip.Item2),
                        LocalizationResourceManager["SensorServiceRetryButton"].ToString(),
                        LocalizationResourceManager["GenericButtonOk"].ToString());

                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(LocalizationResourceManager["GenericErrorTitle"].ToString(), 
                    ex.Message, LocalizationResourceManager["GenericButtonOk"].ToString());
                new ErrorToLogFile(ex).WriteToFile();
            }

            return outputAzimDip;

        }
    }
}
