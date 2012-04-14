//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Configuration;
using System.ComponentModel.DataAnnotations;

using Rock.Data;

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
                string encryptionPhrase = ConfigurationManager.AppSettings["EncryptionPhrase"];
                if ( String.IsNullOrWhiteSpace( encryptionPhrase ) )
                    encryptionPhrase = "Rock Rocks!";

                string identifier = this.CurrentId.ToString() + ">" + this.CurrentGuid.ToString();
                return Rock.Security.Encryption.EncryptString( identifier, encryptionPhrase );
            }
        }

    }
}
