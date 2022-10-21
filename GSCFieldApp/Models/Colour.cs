using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    /// <summary>
    /// A class to build colour text based on different selected attributes from user
    /// </summary>
    public class Colour
    {
        //Properties
        public string generic { get; set; }
        public string intensity { get; set; }
        public string qualifier { get; set; }

        //Overrides
        public override string ToString()
        {
            return this.generic + " " + this.intensity + " " + this.qualifier;
        }

        public Colour()
        {
            //Init to empty
            this.generic = string.Empty;
            this.intensity = string.Empty;
            this.qualifier = string.Empty;
        }

        public Colour fromString(string inColourString = "")
        {
            //Vars
            Colour outputColour = new Colour();

            List<string> splittedColour = inColourString.Split(" ").ToList();

            if (splittedColour != null && splittedColour.Count > 0)
            {
                if (splittedColour.Count >= 1)
                {
                    outputColour.generic = splittedColour[0];      
                }

                if (splittedColour.Count >= 2)
                {
                    outputColour.intensity = splittedColour[1];
                }

                if (splittedColour.Count >= 3)
                {
                    outputColour.qualifier = splittedColour[2];
                }
            }

            return outputColour;
        }
    }
}
