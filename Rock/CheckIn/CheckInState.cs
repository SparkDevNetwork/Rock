//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Rock.CheckIn
{
    /// <summary>
    /// Object for maintaining the state of a check-in kiosk and workflow
    /// </summary>
    [DataContract]
    public class CheckInState : DotLiquid.ILiquidizable
    {
        /// <summary>
        /// Gets or sets the kiosk status
        /// </summary>
        /// <value>
        /// The kiosk.
        /// </value>
        [DataMember]
        public KioskStatus Kiosk { get; set; }

        /// <summary>
        /// Gets or sets the check in status
        /// </summary>
        /// <value>
        /// The check in.
        /// </value>
        [DataMember]
        public CheckInStatus CheckIn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInState" /> class.
        /// </summary>
        /// <param name="kioskStatus">The kiosk status.</param>
        public CheckInState( KioskStatus kioskStatus )
        {
            Kiosk = kioskStatus;
            CheckIn = new CheckInStatus();
        }

        /// <summary>
        /// Creates a new CheckInState object Froms a json string.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static CheckInState FromJson( string json )
        {
            return JsonConvert.DeserializeObject( json, typeof( CheckInState ) ) as CheckInState;
          
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add( "Kiosk", Kiosk );
            dictionary.Add( "CheckIn", CheckIn );
            return dictionary;
        }
    }
}