//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CheckScannerUtility
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class RockConfig
    {
        /// <summary>
        /// The file name
        /// </summary>
        private static string fileName = "rockConfig.xml";

        /// <summary>
        /// Gets or sets the rock URL.
        /// </summary>
        /// <value>
        /// The rock URL.
        /// </value>
        [XmlElement]
        [DataMember]
        public string RockURL { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [XmlElement]
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [XmlElement]
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the type of the image color.
        /// </summary>
        /// <value>
        /// The type of the image color.
        /// </value>
        [XmlElement]
        [DataMember]
        public int ImageColorType { get; set; }

        /// <summary>
        /// Gets or sets the MICR image COM port.
        /// </summary>
        /// <value>
        /// The MICR image COM port.
        /// </value>
        [XmlElement]
        [DataMember]
        public short MICRImageComPort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public enum InterfaceType
        {
            RangerApi = 0,
            MICRImageRS232 = 1
        }

        [XmlElement]
        [DataMember]
        public InterfaceType ScannerInterfaceType { get; set; }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            DataContractSerializer s = new DataContractSerializer( this.GetType() );
            FileStream fs = new FileStream( fileName, FileMode.Create );
            s.WriteObject( fs, this );
            fs.Close();
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <returns></returns>
        public static RockConfig Load()
        {
            try
            {
                if ( File.Exists( fileName ) )
                {
                    FileStream fs = new FileStream( fileName, FileMode.OpenOrCreate );
                    try
                    {

                        DataContractSerializer s = new DataContractSerializer( typeof( RockConfig ) );
                        var result = s.ReadObject( fs ) as RockConfig;
                        return result;
                    }
                    finally
                    {
                        fs.Close();
                    }
                }

                return new RockConfig();
            }
            catch
            {
                return new RockConfig();
            }
        }
    }
}
