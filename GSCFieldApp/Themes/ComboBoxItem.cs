using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace GSCFieldApp.Themes
{
    /// <summary>
    /// A class to be used with comboboxes to updated selected items
    /// </summary>
    public class ComboBoxItem
    {
        public Visibility _canRemoveItem = Visibility.Visible;

        public string itemName { get; set; }
        public string itemValue { get; set; }
        public Visibility canRemoveItem { get { return _canRemoveItem; } set { _canRemoveItem = value; } }
    }

    /// <summary>
    /// A class to be used with combobox item when a bunch of selected values needs to be saved
    /// as a string with piped values.
    /// </summary>
    public class ConcatenatedCombobox
    {
        /// <summary>
        /// From a given observable collection of combobox items, will
        /// build a string output with pipe separated values.
        /// </summary>
        /// <param name="inCollection"></param>
        /// <returns></returns>
        public string PipeValues(ObservableCollection<ComboBoxItem> inCollection)
        {
            //Variable
            string output = string.Empty;

            //Iterate through values
            foreach (Themes.ComboBoxItem cboxItems in inCollection)
            {
                if (output == string.Empty)
                {
                    output = cboxItems.itemValue;
                }
                else
                {
                    output = output + Dictionaries.DatabaseLiterals.KeywordConcatCharacter + cboxItems.itemValue;
                }
            }

            return output;


        }

        /// <summary>
        /// From a given string that contains pipe seperated values, will
        /// unpipe them and output a list of those values.
        /// </summary>
        /// <param name="inString"></param>
        /// <returns></returns>
        public List<string> UnpipeString(string inString)
        {
            //Variables
            List<string> outputRawList = inString.Split(Dictionaries.DatabaseLiterals.KeywordConcatCharacter.Trim().ToCharArray()).ToList();
            List<string> outputList = new List<string>();
            foreach (string val in outputRawList)
            {
                outputList.Add(val.Trim());
            }

            return outputList;

        }



    }

}
