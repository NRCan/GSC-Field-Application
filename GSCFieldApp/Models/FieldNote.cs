namespace GSCFieldApp.Models
{

    /// <summary>
    /// This class is mainly used to show information in the notes page.
    /// In order to know what to edit, an item needs to know about it's parents and have access to some 
    /// field values. This class is intended to play this role.
    /// </summary>
    public class FieldNote
    {
        #region PROPERTIES

        //Define here are the properties that can be used for edit and delete operation
        public int GenericID { get; set; }
        public string GenericTableName { get; set; }
        //Name of the field that holds the ids
        public string GenericFieldID { get; set; }

        public string GenericAliasName { get; set; }

        //Define here are the properties that can be used for add operation. TODO ParentTableName could be deduce from the relation inside the database.
        public int ParentID { get; set; }

        public string ParentTableName { get; set; }

        public bool isValid { get; set; }

        public string Display_text_1 { get; set; }
        public string Display_text_2 { get; set; }
        public string Display_text_3 { get; set; }
        public string IconFontText { get; set; }

        public string Date { get; set; }
        #endregion

        public FieldNote()
        {

        }
    }
}
