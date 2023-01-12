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
    public class Contacts
    {
        //Properties
        public string type { get; set; }

        public string relatedEarthMaterialID { get; set; }

        //Overrides
        public override string ToString()
        {

            string contactDefinition = string.Empty;
            List<string> contacts = new List<string>();


            //Make sure to not add extra concat characters if properties are empty
            if (this.type != null && this.type != string.Empty)
            {
                contacts.Add(this.type);
            }


            if (this.relatedEarthMaterialID != null && this.relatedEarthMaterialID != string.Empty)
            {
                contacts.Add(this.relatedEarthMaterialID);
            }


            foreach (string c in contacts)
            { 
                if (contacts.IndexOf(c) == 0)
                {
                    contactDefinition = c;
                }
                else
                {
                    contactDefinition = contactDefinition + DatabaseLiterals.KeywordConcatCharacter2nd + c;
                }
            }

            return contactDefinition;
        }

        public Contacts()
        {
            //Init to empty
            this.type = string.Empty;
            this.relatedEarthMaterialID = string.Empty;

        }

        public Contacts fromString(string inContactString = "")
        {
            //Vars
            Contacts outputContact = new Contacts();

            if (inContactString != null )
            {

                List<string> splittedContact = inContactString.Split(DatabaseLiterals.KeywordConcatCharacter2nd).ToList();

                if (splittedContact != null && splittedContact.Count > 0)
                {
                    if (splittedContact.Count >= 1)
                    {
                        outputContact.type = splittedContact[0];
                    }

                    if (splittedContact.Count >= 2)
                    {
                        outputContact.relatedEarthMaterialID = splittedContact[1];
                    }

                }
            }

            return outputContact;
        }
    }
}
