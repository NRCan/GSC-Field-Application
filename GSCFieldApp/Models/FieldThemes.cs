using CommunityToolkit.Mvvm.ComponentModel;
using GSCFieldApp.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    /// <summary>
    /// A class to hide or make visible some controls based on
    /// which type of field book user has created or selected
    /// </summary>
    public partial class FieldThemes: ObservableObject
    {
        //Themes
        private bool _bedrockVisibility = true; //Visibility for extra fields
        private bool _surficialVisibility = true; //Visibility for extra fields

        public bool BedrockVisibility { get { return _bedrockVisibility; } set { _bedrockVisibility = value; } }
        public bool SurficialVisibility { get { return _surficialVisibility; } set { _surficialVisibility = value; } }

        public FieldThemes() { SetFieldVisibility(); }

        /// <summary>
        /// Will set visibility based on a bedrock or surficial field book
        /// </summary>
        public async Task SetFieldVisibility()
        {
            //Prefered theme should be saved on field book selected. Defaults to bedrock.
            string preferedTheme = Preferences.Get(nameof(DatabaseLiterals.FieldUserInfoPName), DatabaseLiterals.ApplicationThemeBedrock);
            if (preferedTheme == DatabaseLiterals.ApplicationThemeBedrock)
            {
                _bedrockVisibility = true;
                _surficialVisibility = false;
            }
            else if (preferedTheme == DatabaseLiterals.ApplicationThemeSurficial)
            {
                _bedrockVisibility = false;
                _surficialVisibility = true;
            }


            OnPropertyChanged(nameof(BedrockVisibility));
            OnPropertyChanged(nameof(SurficialVisibility));
        }
    }
}
