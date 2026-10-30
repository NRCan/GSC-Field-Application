using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace GSCFieldApp.Models
{
    public class EasterEggs
    {
        //Some initial variables
        private bool _mosquitoMoving = true;
        private double _previousMagnitude = 0.0;
        private Image _mosquitoImage = null;

        public EasterEggs() { }

        /// <summary>
        /// Will add some random squashed mosquitoes on screen from a given parent control
        /// and will make them sort of fly through screen, going out and coming back in.
        /// Additionnaly tapping the mosquito will stop it and show a squished mosquito image instead.
        /// </summary>
        /// <param name="r"></param>
        public async Task ShowMosquito(Grid inParentPanel, int r = 0)
        {
            //Get initial random number for showing or not the mosquito (4% chances)
            Random random = new Random(DateTime.Now.Millisecond * DateTime.Now.Year / (DateTime.Now.Second + 1));
            if (r == 0)
            {
                r = random.Next(0, 100);
            }

            int mosquitoCount = 1; //random.Next(1, 3);

            if (r >= 40 && r <= 44)
            {
                while (mosquitoCount > 0)
                {

                    //Set initial location, within form/grid area
                    int X = 0;
                    int Y = 0;
                    X = ((int)(random.Next(0, Convert.ToInt16(inParentPanel.Window.Width))));
                    Y = ((int)(random.Next(0, Convert.ToInt16(inParentPanel.Window.Height))));

                    //Create mosquito image
                    ImageSource mosquitoSourceImage = ImageSource.FromFile("mosquito.png");
                    _mosquitoImage = new Image
                    {
                        TranslationX = X,
                        TranslationY = Y,
                        Source = mosquitoSourceImage,
                        HeightRequest = 60,
                        WidthRequest = 60,
                        IsVisible = true,
                        ZIndex=10000,
                        Opacity=1.0,
                    };

                    //Add a tap event so user can tap it to make it stop moving around
                    TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += (s, e) =>
                    {
                        StopMosquito();
                    };
                    _mosquitoImage.GestureRecognizers.Add(tapGestureRecognizer);

                    //Add a slap function to kill the mosquito
                    Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
                    if (Accelerometer.IsSupported && !Accelerometer.IsMonitoring)
                    {
                        Accelerometer.Start(SensorSpeed.Game);
                    }
                    
                    //Add to grid and force refresh
                    inParentPanel.Children.Add(_mosquitoImage);
                    inParentPanel.InvalidateMeasure();

                    //Make it fly around
                    await MoveImage(_mosquitoImage, inParentPanel.Width, inParentPanel.Height);

                    //Decrease count
                    mosquitoCount--;
                }


            }
        }

        /// <summary>
        /// Meant to detect a sudden slap on the screen, which would make the mosquito stop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            //Calculate magnitude of change from a possible slap
            double changeMagnitude = Math.Sqrt(
                Math.Pow(e.Reading.Acceleration.X, 2) + 
                Math.Pow(e.Reading.Acceleration.Y, 2) + 
                Math.Pow(e.Reading.Acceleration.Z, 2));

            double magnitudeDifference = Math.Abs(changeMagnitude - _previousMagnitude);
            _previousMagnitude = changeMagnitude;

            if (magnitudeDifference > 1.5)
            {
                StopMosquito();
            }
        }

        /// <summary>
        /// Will control the mosquito flying image through the screen
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private async Task MoveImage(Image image, double width, double height)
        {
            Random rand = new Random();
            double x = image.X;
            double y = image.Y;

            //path variance
            double vx = rand.Next(-5,5);
            double vy = vx - 1.0;

            // Adjust these limits to match your screen/container size
            double maxX = width - image.Width;
            double maxY = height - image.Height;

            while (_mosquitoMoving)
            {

                //Calculate move
                x += vx;
                y += vy;

                //make sure it bounces back in
                if (x < -maxX || x > maxX)
                {
                    vx = -vx;
                }
                if (y < -maxY || y > maxY)
                {
                    vy = -vy;
                }
                    
                //Set new placement
                image.TranslationX = x;
                image.TranslationY = y;

                //Slow down a bit the movement
                await Task.Delay(15); 

            }
        }

        /// <summary>
        /// Stop the mosquito movement and accelerometer at the same time 
        /// and show the bloodied version of it
        /// </summary>
        private void StopMosquito()
        {
            if (_mosquitoMoving)
            {
                Random rand = new Random();

                //Make it stop moving
                _mosquitoMoving = false;
                _mosquitoImage.Source = ImageSource.FromFile("mosquito_b.png"); 
                _mosquitoImage.Rotation = rand.Next(0, 360);

                //Stop accel.
                if (Accelerometer.IsSupported && Accelerometer.IsMonitoring)
                {
                    Accelerometer.Stop();
                }

            }
        }
    }
}
