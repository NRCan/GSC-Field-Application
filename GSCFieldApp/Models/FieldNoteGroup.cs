using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSCFieldApp.Models
{
    public class FieldNoteGroup: List<FieldNote>
    {
        public string Name { get; private set; }

        public FieldNoteGroup(string name, List<FieldNote> notes) : base(notes)
        {
            Name = name;
        }

    }
}
