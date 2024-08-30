using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    /// <summary>
    /// Defines the main parent category of lithologies, which is the group (metamorphic, sedidementary, etc.)
    /// </summary>
    public class Lithology
    {
        public string GroupTypeCode { get; set; }
        public string GroupTypeDescription { get; set; }

        public List<LithologyDetail> lithologyDetails { get; set; }

        public Lithology(string groupTypeCode) 
        {
            GroupTypeCode = groupTypeCode;
            lithologyDetails = new List<LithologyDetail>();
        }

    }

    /// <summary>
    /// Defines the lithology detail aka rock name (gabbro, dolomite, granite, etc.)
    /// </summary>
    public class LithologyDetail
    {
        public string DetailCode { get; set; }
        public string DetailDescription { get; set; }
    }
}
