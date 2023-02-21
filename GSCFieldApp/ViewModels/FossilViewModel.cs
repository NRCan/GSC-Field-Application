using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System.Collections.Generic;
using Template10.Mvvm;

namespace GSCFieldApp.ViewModels
{
    public class FossilViewModel: ViewModelBase
    {
        #region INIT DECLARATIONS

        public bool doFossilUpdate = false;

        //UI
        private List<Themes.ComboBoxItem> _fossilType = new List<Themes.ComboBoxItem>();
        private string _selectedFossilType = string.Empty;

        public string _fossilNote = string.Empty;//Default
        public int _fossilParentID = 0;
        public int _fossilID = 0;
        public string _fossilName = string.Empty;

        //Model init
        private readonly Fossil fossilModel = new Fossil();
        public DataIDCalculation fossilCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailFossil;
        readonly DataAccess accessData = new DataAccess();

        //Events and delegate
        public delegate void pflowEditEventHandler(object sender); //A delegate for execution events
        public event pflowEditEventHandler newFossilEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES
        public string FossilNote { get { return _fossilNote; } set { _fossilNote = value; } }
        public int FossilParentID { get { return _fossilParentID; } set { _fossilParentID = value; } }
        public int FossilID { get { return _fossilID; } set { _fossilID = value; } }
        public string FossilName { get { return _fossilName; } set { _fossilName = value; } }
        public List<Themes.ComboBoxItem> FossilType { get { return _fossilType; } set { _fossilType = value; } }
        public string SelectedFossilType { get { return _selectedFossilType; } set { _selectedFossilType = value; } }

        #endregion

        #region METHODS

        public FossilViewModel(FieldNotes inReportDetail)
        {
            //On init for new samples calculates values for default UI form
            _fossilParentID = inReportDetail.GenericID;
            _fossilID = fossilCalculator.CalculateFossilID();
            _fossilName = fossilCalculator.CalculateFossilAlias(_fossilParentID, inReportDetail.earthmat.EarthMatName);

            FillFossilType();
        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            //Keep
            existingDataDetailFossil = incomingData;

            //Set
            _fossilID = existingDataDetailFossil.fossil.FossilID;
            _fossilName = existingDataDetailFossil.fossil.FossilIDName;
            _fossilParentID = existingDataDetailFossil.fossil.FossilParentID;
            _fossilNote = existingDataDetailFossil.fossil.FossilNote;

            _selectedFossilType = existingDataDetailFossil.fossil.FossilType;

            //Update UI
            RaisePropertyChanged("FossilID");
            RaisePropertyChanged("FossilParentID");
            RaisePropertyChanged("FossilNote");
            RaisePropertyChanged("FossilName");

            //TODO Main direction parsing

            RaisePropertyChanged("SelectedFossilType");

            doFossilUpdate = true;
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Save
            fossilModel.FossilID = _fossilID;
            fossilModel.FossilIDName = _fossilName;
            fossilModel.FossilParentID = _fossilParentID;
            fossilModel.FossilNote = _fossilNote;


            if (SelectedFossilType != null)
            {
                fossilModel.FossilType = SelectedFossilType;
            }


            //Save model class
            accessData.SaveFromSQLTableObject(fossilModel, doFossilUpdate);

            //Launch an event call for everyone that an earthmat has been edited.
            if (newFossilEdit != null)
            {
                newFossilEdit(this);
            }
        }

        #endregion

        #region FILLS

        private void FillFossilType()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldFossilType;
            string tableName = Dictionaries.DatabaseLiterals.TableFossil;
            _fossilType = accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedFossilType);

            //Update UI
            RaisePropertyChanged("FossilType");
            RaisePropertyChanged("SelectedFossilType");
        }

        #endregion


    }
}
