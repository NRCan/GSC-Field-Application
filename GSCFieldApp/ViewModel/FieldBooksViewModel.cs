using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    public class FieldBooksViewModel
    {
        public FieldBooksViewModel() 
        {

            //TEST db resource to file
            DataAccess dataAccess = new DataAccess();
            dataAccess.CreateDatabaseFromResource();

        }
    }
}
