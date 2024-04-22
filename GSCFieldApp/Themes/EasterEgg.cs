using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace GSCFieldApp.Themes
{
    public class EasterEgg
    {
        /// <summary>
        /// Will add some random squashed mosquitoes on screen from a given parent control
        /// </summary>
        /// <param name="r"></param>
        public void ShowMosquito(RelativePanel inParentPanel, int r = 0)
        {

            Random random = new Random(DateTime.Now.Millisecond * DateTime.Now.Year / (DateTime.Now.Second + 1));
            if (r == 0)
            {
                r = random.Next(0, 100);
            }

            int mosquitoCount = random.Next(1, 3);

            if (r >= 40 && r <= 44)
            {
                while (mosquitoCount > 0)
                {
                    int X = 0;
                    int Y = 0;

                    X = ((int)(random.Next(0, Convert.ToInt16(inParentPanel.ActualWidth))));
                    Y = ((int)(random.Next(0, Convert.ToInt16(inParentPanel.ActualHeight))));

                    CompositeTransform transform = new CompositeTransform
                    {
                        TranslateX = X,
                        TranslateY = Y,
                        Rotation = random.Next(0, 360)
                    };

                    BitmapImage mosquitoSourceImage = new BitmapImage(new Uri("ms-appx:///Assets/mosquito.png"));

                    Image newMosquito = new Image
                    {
                        RenderTransform = transform,
                        Source = mosquitoSourceImage,
                        Height = 60,
                        Width = 60,
                        Name = "mosquito" + mosquitoCount.ToString(),
                        Visibility = Visibility.Visible
                    };
                    inParentPanel.Children.Add(newMosquito);
                    inParentPanel.UpdateLayout();


                    mosquitoCount--;
                }


            }

        }

        /// <summary>
        /// Will barrel roll user control
        /// </summary>
        public async void DoABarrelRollAsync(UserControl inParentPage)
        {
            int rotation = 0;
            while (rotation < 361)
            {
                //Build transform
                CompositeTransform transform = new CompositeTransform
                {
                    Rotation = rotation
                };
                inParentPage.RenderTransform = transform;

                //Refresh
                await Task.Delay(5);
                inParentPage.UpdateLayout();

                //Next
                rotation++;
            }

        }

        /// <summary>
        /// Will flip on x assix the user control
        /// </summary>
        public void pilf(UserControl inParentPage)
        {

            //Build transform
            CompositeTransform transform = inParentPage.RenderTransform as CompositeTransform;
            if (transform == null)
            {
                transform = new CompositeTransform();
            }
            if (transform.ScaleX != -1)
            {
                transform.ScaleX = -1;
            }
            else
            {
                transform.ScaleX = 1;
            }

            inParentPage.RenderTransform = transform;

            //Refresh

            inParentPage.UpdateLayout();

        }


        /// <summary>
        /// Will change app color to be pink
        /// </summary>
        public void UnicornThemeAsync()
        {
            Color pinkUnicorn = GetSolidColorBrush("#ff69b4").Color;
            SetAppColor(pinkUnicorn);
        }

        /// <summary>
        /// Will get a solid color brush from an hex color code
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            if (hex.Length == 8)
            {
                byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
                byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
                byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
                byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
                SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));

                return myBrush;
            }
            else if (hex.Length == 6)
            {
                byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
                byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
                byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
                SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));

                return myBrush;
            }

            else
            {
                return null;
            }


        }

        /// <summary>
        /// Will set a color in all app controls.
        /// </summary>
        /// <param name="inColor"></param>
        public void SetAppColor(Color inColor)
        {
            var mergedDico = Application.Current.Resources.MergedDictionaries.FirstOrDefault();
            (Application.Current.Resources["FieldStationColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldStationColorLightBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["FieldMineralColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldMineralColorLightBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["FieldMineralAlterationColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldMineralAlterationColorLightBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["FieldStructureColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldStructureColorLightBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["FieldFossilColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldFossilColorLightBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["FieldObsColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldObsColorLightBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["FieldPhotoColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldPhotoColorLightBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["FieldPflowColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldPflowColorLightBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["FieldSampleColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldSampleColorLigthBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["FieldEarthMatColorBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["FieldEarthMatColorLightBrush"] as SolidColorBrush).Color = inColor;

            (Application.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush).Color = inColor;
            (Application.Current.Resources["CustomColorBrush"] as SolidColorBrush).Color = inColor;

        }

    }
}
