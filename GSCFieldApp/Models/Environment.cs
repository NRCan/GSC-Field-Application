using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Dictionaries;
using SQLite;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableEnvironment)]
    internal class Environment
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldEarthMatID)]
        public string EarthMatID { get; set; }

        [PrimaryKey, Column(DatabaseLiterals.FieldEarthMatID)]
        public string EnvID { get; set; }
    }
}
