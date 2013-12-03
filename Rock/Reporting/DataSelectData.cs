using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Reporting
{
    /// <summary>
    /// Class that contains the result of a DataSelectComponent Select Expression
    /// </summary>
    public class DataSelectData
    {
        /// <summary>
        /// The Id of whatever Entity. For example, Person.Id, Group.Id, etc
        /// </summary>
        public int EntityId;
        
        /// <summary>
        /// This will contain an Anonymous class that the DataSelectComponent creates
        /// </summary>
        public object Data;
    }
}
