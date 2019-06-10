using GSCFieldApp.Dictionaries;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableFavorites)]
    public class Favorite
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldFavoriteID)]
        public string FavID { get; set; }
    }
}
