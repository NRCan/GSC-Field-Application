using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldNotesViewModel : ObservableObject
    {
        #region PROPERTIES

        //public ObservableCollection<FieldNoteGroup> _fieldNotes = new ObservableCollection<FieldNoteGroup>();
        //public ObservableCollection<FieldNoteGroup> FieldNotes { get { return _fieldNotes; } set { _fieldNotes = value; } }
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

    }
}
