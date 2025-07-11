using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Enums.Geography
{
    /// <summary>
    /// Represents the mode of travel for distance calculations.
    /// </summary>
    public enum TravelMode
    {
        /// <summary>
        /// Travel by driving a vehicle.
        /// </summary>
        Drive,

        /// <summary>
        /// Travel by walking.
        /// </summary>
        Walk,

        /// <summary>
        /// Travel by riding a bicycle.
        /// </summary>
        Bicycle
    }
}
