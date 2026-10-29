using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    public class EasterEggs
    {
        public EasterEggs() { }

        /// <summary>
        /// Will add some random squashed mosquitoes on screen from a given parent control
        /// </summary>
        /// <param name="r"></param>
        public void ShowMosquito(ContentPage inParentPanel, int r = 0)
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

                    X = ((int)(random.Next(0, Convert.ToInt16(inParentPanel.Width))));
                    Y = ((int)(random.Next(0, Convert.ToInt16(inParentPanel.Height))));

                    ImageSource mosquitoSourceImage = ImageSource.FromFile("mosquito.png");

                    Image newMosquito = new Image
                    {
                        Rotation = random.Next(0, 360),
                        TranslationX = X,
                        TranslationY = Y,
                        Source = mosquitoSourceImage,
                        HeightRequest = 60,
                        WidthRequest = 60,
                        IsVisible = true,
                    };
                    inParentPanel.AddLogicalChild(newMosquito);

                    mosquitoCount--;
                }


            }
        }
    }
}
