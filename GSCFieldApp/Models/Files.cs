using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace GSCFieldApp.Models
{
    public class Files
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool FileVisible { get; set; } //Used for maps 
        public Visibility FileCanDelete { get; set; } //If file can be deleted by user.
    }
}
