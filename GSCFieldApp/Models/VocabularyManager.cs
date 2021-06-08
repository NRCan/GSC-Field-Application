using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableDictionaryManager)]
    public class VocabularyManager
    {

        [PrimaryKey, Column(DatabaseLiterals.FieldDictionaryManagerCodedTheme)]
        public string ThemeName { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerAssignTable)]
        public string ThemeTable { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerAssignField)]
        public string ThemeField { get; set; }
        
        [Column(DatabaseLiterals.FieldDictionaryManagerCodedThemeDescription)]
        public string ThemeNameDesc { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerEditable)]
        public string ThemeEditable { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerSpecificTo)]
        public string ThemeProjectType { get; set; }
    }
}
