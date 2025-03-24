using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Services
{
    public static class ObservableCollectionHelper
    {
        /// <summary>
        /// Initializing and appending two collections 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        /// <param name="items"></param>
        public static void AddRange<T>(this ObservableCollection<T> coll, IEnumerable<T> items)
        {
            if (coll == null)
            {
                coll = new ObservableCollection<T>();
            }

            foreach (var item in items)
            {
                if (!coll.Contains(item))
                {
                    coll.Add(item);
                }
                
            }
        }
    }
}
