using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;

namespace GSCFieldApp.Models
{
    public partial class Picklist: ObservableObject
    {
        //Database table name
        public string PicklistName { get; set; }

        //Database selected table field name
        public string PicklistField { get; set; }

        //Database selected table field value description
        public string PicklistFieldValueName { get; set; }

        //Database selected table field value code
        public string PicklistFieldValueCode { get; set; }

        //Is field value renderer in drop downs
        public string PicklistVisible { get; set; }

        //Is field value set as a default one
        public string PicklistDefault { get; set; }

        //Related parent value (structures and lithos have parent/children)
        public string PicklistParent { get; set; }

    }
}
