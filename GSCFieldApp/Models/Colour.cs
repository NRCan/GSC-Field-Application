using GSCFieldApp.Dictionaries;
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

            string colourString = string.Empty;

            //Make sure to not add extra concat characters if properties are empty
            if (this.generic != null && this.generic != string.Empty)
            {
                colourString = this.generic;

                if (this.intensity != null && this.intensity != string.Empty)
                {
                    colourString = colourString + DatabaseLiterals.KeywordConcatCharacter + this.intensity;

                    if (this.qualifier != null && this.qualifier != string.Empty)
                    {
                        colourString = colourString + DatabaseLiterals.KeywordConcatCharacter + this.qualifier;
                    }
                }
            }

            return colourString;
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

            if (inColourString!= null )
            {

                List<string> splittedColour = inColourString.Split(DatabaseLiterals.KeywordConcatCharacter).ToList();

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
            }

            return outputColour;
        }
    }
}
