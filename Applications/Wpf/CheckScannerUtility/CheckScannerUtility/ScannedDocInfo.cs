﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Drawing;
using System.IO;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// 
    /// </summary>
    public class ScannedDocInfo
    {
        /// <summary>
        /// Gets or sets the front image.
        /// </summary>
        /// <value>
        /// The front image.
        /// </value>
        public byte[] FrontImageData { get; set; }

        /// <summary>
        /// Gets the front image PNG bytes.
        /// </summary>
        /// <value>
        /// The front image PNG bytes.
        /// </value>
        public byte[] FrontImagePngBytes
        {
            get
            {
                if ( this.FrontImageData != null )
                {
                    Bitmap bmp = new Bitmap( new MemoryStream( this.FrontImageData ) );
                    MemoryStream pngStream = new MemoryStream();
                    bmp.Save( pngStream, System.Drawing.Imaging.ImageFormat.Png );
                    return pngStream.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the back image.
        /// </summary>
        /// <value>
        /// The back image.
        /// </value>
        public byte[] BackImageData { get; set; }

        /// <summary>
        /// Gets the back image PNG bytes.
        /// </summary>
        /// <value>
        /// The back image PNG bytes.
        /// </value>
        public byte[] BackImagePngBytes
        {
            get
            {
                if ( this.BackImageData != null )
                {
                    Bitmap bmp = new Bitmap( new MemoryStream( this.BackImageData ) );
                    MemoryStream pngStream = new MemoryStream();
                    bmp.Save( pngStream, System.Drawing.Imaging.ImageFormat.Png );
                    return pngStream.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the currency type value.
        /// </summary>
        /// <value>
        /// The currency type value.
        /// </value>
        public Rock.Client.DefinedValue CurrencyTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the source type value.
        /// </summary>
        /// <value>
        /// The source type value.
        /// </value>
        public Rock.Client.DefinedValue SourceTypeValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ScannedDocInfo"/> should be uploaded
        /// </summary>
        /// <value>
        ///   <c>true</c> if upload; otherwise, <c>false</c>.
        /// </value>
        public bool Upload { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public int? TransactionId { get; set; }

        #region Applies only to Scanned Checks

        /// <summary>
        /// Gets a value indicating whether this instance is check.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is check; otherwise, <c>false</c>.
        /// </value>
        public bool IsCheck
        {
            get
            {
                return this.CurrencyTypeValue != null && this.CurrencyTypeValue.Guid == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            }
        }

        /// <summary>
        /// Gets the MICR track data that was read off of the check
        /// </summary>
        /// <value>
        /// The scanned check micr.
        /// </value>
        public string ScannedCheckMicrData { get; set; }

        /// <summary>
        /// Gets the scanned check micr in the format "{RoutingNumber}_{AccountNumber}_{CheckNumber}";
        /// </summary>
        /// <value>
        /// The scanned check micr.
        /// </value>
        public string ScannedCheckMicrParts
        {
            get
            {
                if ( string.IsNullOrWhiteSpace( this.RoutingNumber ) && string.IsNullOrWhiteSpace( this.AccountNumber ) && string.IsNullOrWhiteSpace( this.CheckNumber ) )
                {
                    // if all three are blank, return empty string
                    return string.Empty;
                }
                else
                {
                    return string.Format( "{0}_{1}_{2}", this.RoutingNumber, this.AccountNumber, this.CheckNumber );
                }
            }
        }

        /// <summary>
        /// Gets or sets the routing number.
        /// </summary>
        /// <value>
        /// The routing number.
        /// </value>
        public string RoutingNumber { get; set; }

        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        /// <value>
        /// The account number.
        /// </value>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the check number.
        /// </summary>
        /// <value>
        /// The check number.
        /// </value>
        public string CheckNumber { get; set; }

        /// <summary>
        /// Any other MICR data that isn't the Routing, AccountNumber or CheckNumber
        /// </summary>
        /// <value>
        /// The other data.
        /// </value>
        public string OtherData { get; set; }

        /// <summary>
        /// Gets the masked account number.
        /// </summary>
        /// <value>
        /// The masked account number.
        /// </value>
        public string MaskedAccountNumber
        {
            get
            {
                int length = AccountNumber.Length;
                string result = new string( 'x', length - 4 ) + AccountNumber.Substring( length - 4 );
                return result;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [bad micr].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [bad micr]; otherwise, <c>false</c>.
        /// </value>
        public bool BadMicr { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ScannedDocInfo"/> is duplicate.
        /// </summary>
        /// <value>
        ///   <c>true</c> if duplicate; otherwise, <c>false</c>.
        /// </value>
        public bool Duplicate { get; set; }

        #endregion
    }
}
