using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Data
{
    public interface IExportable
    {
        string Export();
        void Import(string data);
    }
}
