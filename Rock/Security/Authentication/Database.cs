//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;

using Rock.Cms;

namespace Rock.Security.Authentication
    
    /// <summary>
    /// Authenticates a username/password using the Rock database
    /// </summary>
    [Description( "Database Authentication Provider" )]
    [Export(typeof(AuthenticationComponent))]
    [ExportMetadata("ComponentName", "Database")]
    public class Database : AuthenticationComponent
        
		private static byte[] encryptionKey;
		private static MachineKeySection machineKeyConfig = (MachineKeySection)ConfigurationManager.GetSection( "system.web/machineKey" );

		/// <summary>
		/// Initializes the <see cref="Database" /> class.
		/// </summary>
		/// <exception cref="System.Configuration.ConfigurationErrorsException">Cannot use an Auto Generated machine key</exception>
		static Database()
		    
			string configKey;

			configKey = machineKeyConfig.ValidationKey;
			if ( configKey.Contains( "AutoGenerate" ) )
			    
				throw new ConfigurationErrorsException( "Cannot use an Auto Generated machine key" );
			}

			encryptionKey = HexToByte( configKey );
		}

        /// <summary>
        /// Authenticates the specified user name.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
		public override Boolean Authenticate( User user, string password )
		    
			return EncodePassword( user, password ) == user.Password;
		}

		/// <summary>
		/// Encodes the password.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="password"></param>
		/// <returns></returns>
		public override String EncodePassword( User user, string password )
		    
			HMACSHA1 hash = new HMACSHA1();
			hash.Key = encryptionKey;
			return Convert.ToBase64String( hash.ComputeHash( Encoding.Unicode.GetBytes( password ) ) );
		}

		private static byte[] HexToByte( string hexString )
		    
			byte[] returnBytes = new byte[hexString.Length / 2];
			for ( int i = 0; i < returnBytes.Length; i++ )
				returnBytes[i] = Convert.ToByte( hexString.Substring( i * 2, 2 ), 16 );
			return returnBytes;
		}
	}
}