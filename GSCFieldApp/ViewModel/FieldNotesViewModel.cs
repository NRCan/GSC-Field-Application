using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldNotesViewModel : ObservableObject
    {
        #region PROPERTIES


        private bool _isStationVisible = true;
        public bool IsStationVisible { get { return _isStationVisible; } set { _isStationVisible = value; } }

        private bool _isEarthMatVisible = true;
        public bool IsEarthMatVisible { get { return _isEarthMatVisible; } set { _isEarthMatVisible = value; } }

        private bool _isSampleVisible = true;
        public bool IsSampleVisible { get { return _isSampleVisible; } set { _isSampleVisible = value; } }

        public List<FieldNoteGroup> FieldNotes { get; private set; } = new List<FieldNoteGroup>();

        private List<FieldNote> _stations = new List<FieldNote>();
        public List<FieldNote> Stations
        {
            get
            {
                IEnumerable<FieldNoteGroup> s = FieldNotes.Where(p => p.Name == "Station");
                if (s != null)
                {
                    return _stations = FieldNotes.Where(p => p.Name == "Station").ToList()[0];
                }
                else
                {
                    return _stations = new List<FieldNote>();
                }

            }
            set { _stations = value; }
        }
        private List<FieldNote> _earthmats = new List<FieldNote>();
        public List<FieldNote> EarthMats
        { 

            get
            {
                IEnumerable<FieldNoteGroup> s = FieldNotes.Where(p => p.Name == "Earth Material");
                if (s != null)
                {
                    return _earthmats = FieldNotes.Where(p => p.Name == "Earth Material").ToList()[0];
                }
                else
                {
                    return _earthmats = new List<FieldNote>();
                }

            }
            set { _earthmats = value; }
        }

        private List<FieldNote> _samples = new List<FieldNote>();
        public List<FieldNote> Samples
        {

            get
            {
                IEnumerable<FieldNoteGroup> s = FieldNotes.Where(p => p.Name == "Sample");
                if (s != null)
                {
                    return _earthmats = FieldNotes.Where(p => p.Name == "Sample").ToList()[0];
                }
                else
                {
                    return _earthmats = new List<FieldNote>();
                }

            }
            set { _earthmats = value; }
        }

        #endregion

        public FieldNotesViewModel()
        {
            FieldNotes.Add(new FieldNoteGroup("Station", new List<FieldNote>
            {
                new FieldNote
                {
                    Display_text_1 = "Outcrop",
                    Display_text_2 = "Nice Paysage",

                },
                new FieldNote
                {
                    Display_text_1 = "Bosquet",
                    Display_text_2 = "With grey rocks",

                },

            }));

            FieldNotes.Add(new FieldNoteGroup("Earth Material", new List<FieldNote>
            {
                new FieldNote
                {
                    Display_text_1 = "Granite",
                    Display_text_2 = "Black",

                },
                new FieldNote
                {
                    Display_text_1 = "Quartz",
                    Display_text_2 = "Pink",

                },
                new FieldNote
                {
                    Display_text_1 = "Dolomite",
                    Display_text_2 = "More like Dynamite",

                },
            }));

            FieldNotes.Add(new FieldNoteGroup("Sample", new List<FieldNote>{}));

        }

        #region RELAY

        /// <summary>
        /// Will reverse whatever is set has visibility on the records after a tap on header
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Hide(string inComingName)
        {
            if (inComingName != null && inComingName != string.Empty)
            {
                if (inComingName.ToLower().Contains(DatabaseLiterals.KeywordStation))
                {
                    IsStationVisible = !IsStationVisible; 
                    OnPropertyChanged(nameof(IsStationVisible));
                }

                if (inComingName.ToLower().Contains(DatabaseLiterals.KeywordEarthmat))
                {
                    IsEarthMatVisible = !IsEarthMatVisible;
                    OnPropertyChanged(nameof(IsEarthMatVisible));
                }

                if (inComingName.ToLower().Contains(DatabaseLiterals.KeywordSample))
                {
                    IsSampleVisible = !IsSampleVisible;
                    OnPropertyChanged(nameof(IsSampleVisible));
                }
            }

        }

        #endregion
    }
}
