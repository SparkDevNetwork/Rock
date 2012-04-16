//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel.DataAnnotations;

namespace Rock.CRM
{
    public partial class PersonTrail
    {
        /// <summary>
        /// Gets a publicly viewable unique key for the model.
        /// </summary>
        [NotMapped]
        public string CurrentPublicKey
        {
            get
            {
                string identifier = this.CurrentId.ToString() + ">" + this.CurrentGuid.ToString();
                return Rock.Security.Encryption.EncryptString( identifier );
            }
        }

    }
}
