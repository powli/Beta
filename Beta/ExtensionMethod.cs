using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Beta
{
    public static class ExtensionMethods
    {
        private static Random _random = new Random();

        public static T GetRandom<T>(this IList<T> items)
        {
            return items[_random.Next(items.Count)];
        }        
    }
}