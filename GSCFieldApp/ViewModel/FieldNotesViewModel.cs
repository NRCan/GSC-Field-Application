using CommunityToolkit.Mvvm.ComponentModel;
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
        }
    }
}
