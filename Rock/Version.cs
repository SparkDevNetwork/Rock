using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Rock
{
    public static class Version
    {
        public static System.Version Current
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
    }
}