using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Prayer.PrayerRequestDetail
{
    public class PrayerRequestDetailAddPersonResponseBag
    {
        /// <summary>
        /// Pass the value of the Nick Name of the person to the front end
        /// </summary>
        public string nickName;

        /// <summary>
        /// Pass the value of the  Name of the person to the front end
        /// </summary>
        public string lastName;

        /// <summary>
        /// Pass the value of the  email of the person to the front end
        /// </summary>
        public string email;
    }
}
