using System;
using System.Collections.Generic;
using System.Linq;

namespace com.bemaservices.RemoteCheckDeposit
{
    /// <summary>
    /// A helper class for working with MICR data from Rock.
    /// </summary>
    public class Micr
    {
        /// <summary>
        /// Enum for readability to fetch out the data we are after
        /// </summary>
        protected enum FIELD
        {
            /// <summary>
            /// Field 1
            /// </summary>
            CHECK_AMOUNT,

            /// <summary>
            /// Field 2
            /// </summary>
            CHECK_NUMBER,

            /// <summary>
            /// Field 3
            /// </summary>
            ACCOUNT_NUMBER,

            /// <summary>
            /// Field 5
            /// </summary>
            ROUTING_NUMBER,

            /// <summary>
            /// Field 6
            /// </summary>
            EXTERNAL_PROCESSING_CODE,

            /// <summary>
            /// Field 7
            /// </summary>
            AUX_ON_US
        }

        public enum RangerCommonSymbols
        {
            RangerRejectSymbol = '!'
        }

        public enum RangerE13BMicrSymbols
        {
            E13B_AmountSymbol = 'b',
            E13B_OnUsSymbol = 'c',
            E13B_TransitSymbol = 'd',
            E13B_DashSymbol = '-',
            E13B_RejectSymbol = RangerCommonSymbols.RangerRejectSymbol
        }

        #region Fields

        /// <summary>
        /// The content of the MICR data after it has been justified.
        /// </summary>
        private string _content;
        private string _accountNumber;
        private string _routingNumber;
        private string _checkNumber;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Micr"/> class.
        /// </summary>
        /// <param name="content">The content of the MICR data, expected to be in Ranger Driver format.</param>
        /// <exception cref="System.ArgumentException">Argument does not contain valid micr data - content</exception>
        public Micr( string content )
        {
            //
            // Verify the data is valid.
            //
            if ( content == null )
            {
                _content = string.Empty;
            }
            else if ( !content.Contains( ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol ) || !content.Contains( ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol ) )
            {
                throw new ArgumentException( "Argument does not contain valid micr data.", "content" );
            }
            else
            {
                // DO NOT USE:
                // MICR data should be aligned to have the left-most Routing Number symbol
                // at position 43 (from the right-side of the string). So we need to adjust
                // the length of the string so that the first 'd' we find is followed by 42
                // characters.
                //
                //int index = content.IndexOf( 'd' );
                //int length = content.Length - ( content.Length - 43 - index );

                //if ( content.Length > length )
                //{
                //    content = content.Substring( 0, length );
                //}
                //else if ( content.Length < length )
                //{
                //    content = content.PadRight( length );
                //}

                _content = content;

                //NEW STUFF
                //COPIED FROM CHECK SCANNER
                string remainingMicr = _content;
                _accountNumber = string.Empty;
                _routingNumber = string.Empty;
                _checkNumber = string.Empty;

                // there should always be two transit symbols ('d').  The transit number is between them
                int transitSymbol1 = remainingMicr.IndexOf( ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol );
                int transitSymbol2 = remainingMicr.LastIndexOf( ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol );
                int transitStart = transitSymbol1 + 1;
                int transitLength = transitSymbol2 - transitSymbol1 - 1;
                if ( transitLength > 0 )
                {
                    _routingNumber = remainingMicr.Substring( transitStart, transitLength );
                    remainingMicr = remainingMicr.Remove( transitStart - 1, transitLength + 2 );
                }

                char[] separatorSymbols = new char[] { ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol, ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ( char ) RangerE13BMicrSymbols.E13B_AmountSymbol };

                // the last 'On-Us' symbol ('c') signifies the end of the account number
                int lastOnUsPosition = remainingMicr.LastIndexOf( ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol );
                if ( lastOnUsPosition > 0 )
                {
                    int accountNumberDigitPosition = lastOnUsPosition - 1;

                    // read all digits to the left of the last 'OnUs' until you run into another seperator symbol
                    while ( accountNumberDigitPosition >= 0 )
                    {
                        char accountNumberDigit = remainingMicr[accountNumberDigitPosition];
                        if ( separatorSymbols.Contains( accountNumberDigit ) )
                        {
                            break;
                        }
                        else
                        {
                            _accountNumber = accountNumberDigit + _accountNumber;
                            _accountNumber = _accountNumber.Trim();
                        }

                        accountNumberDigitPosition--;
                    }

                    remainingMicr = remainingMicr.Remove( accountNumberDigitPosition + 1, lastOnUsPosition - accountNumberDigitPosition );
                }

                // any remaining digits that aren't the account number and transit number are probably the check number
                string[] remainingMicrParts = remainingMicr.Split( new char[] { ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ' ' }, StringSplitOptions.RemoveEmptyEntries );
                string otherData = null;
                if ( remainingMicrParts.Any() )
                {
                    // Now that we've indentified Routing and AccountNumber, the remaining MICR part is probably the CheckNumber. However, there might be multiple Parts left. We'll have to make a best guess on which chunk is the CheckNumber.
                    // In those cases, assume the 'longest' chunk to the CheckNumber. (Other chunks tend to be short 1 or 2 digit numbers that mean something special to the bank)
                    _checkNumber = remainingMicrParts.OrderBy( p => p.Length ).Last();

                    _checkNumber = _checkNumber.Trim().TrimStart( '0' ); //remove the leading zeros, if any, on a check number. Just need bare minimum.

                    // throw any remaining data into 'otherData' (a reject symbol could be in the other data)
                    remainingMicr = remainingMicr.Replace( ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ' ' );
                    remainingMicr = remainingMicr.Replace( _checkNumber, string.Empty );
                    otherData = remainingMicr;
                }
            }
        }

        #endregion

        #region Static Methods

        public static bool IsValid( string content )
        {
            return IsValid( content, out _);
        }

        /// <summary>
        /// Returns true if the MICR content is valid. This is not a perfect check, but
        /// it should give a strong indication on the MICR string being valid or not.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid( string content, out List<string> error )
        {
            error = new List<string>();

            if ( content == null || content.Length == 0 )
            {
                error.Add( "No MICR Information Provided. Manually Add Account and Item Numbers.");
            }
            else 
            {
                if (content.Contains("!"))
                {
                    error.Add("MICR Read Error. Check Account and Item Numbers For Accuracy.");
                }

                try
                {
                    var micr = new Micr(content);

                    if (micr.GetRoutingNumber().Length != 9 || !int.TryParse( micr.GetRoutingNumber(), out _ ) )
                    {
                        error.Add("MICR Routing Number Error. Must Be 9 Digits.");
                    }

                    if (micr.GetAccountNumber().Length < 4 || !int.TryParse( micr.GetAccountNumber(), out _) )
                    {
                        error.Add("MICR Account Number Error. Must Be At Least 4 Digits.");
                    }

                    if (micr.GetCheckNumber().Length < 1 || !int.TryParse( micr.GetCheckNumber(), out _) )
                    {
                        error.Add("MICR Check Number Error. Must Be At Least 2 Digits.");
                    }

                }
                catch
                {
                    error.Add("MICR Failed To Parse. Manually Add Account and Item Numbers.");
                }
            }
            


            if ( error.Any() )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Internal Methods

        ///// <summary>
        ///// Gets the characters in the specified range from the MICR.
        ///// </summary>
        ///// <param name="start">The starting position in the MICR, from the right.</param>
        ///// <param name="end">The ending position in the MICR (inclusive), from the right.</param>
        ///// <returns>A string containing the characters found in the specified range.</returns>
        //protected string GetCharacterFields( int start, int end )
        //{
        //    if ( start > _content.Length )
        //    {
        //        return string.Empty;
        //    }

        //    if ( end > _content.Length )
        //    {
        //        end = _content.Length;
        //    }

        //    return _content.Substring( _content.Length - end, end - start + 1 );
        //}

        ///// <summary>
        ///// Get a specific MICR field from the MICR data.
        ///// </summary>
        ///// <param name="FieldType">The MICR field to be retrieved.</param>
        ///// <returns>String containing the component value from the MICR line.</returns>
        //protected string GetField( FIELD FieldType )
        //{
        //    var f = string.Empty;

        //    switch ( FieldType )
        //    {
        //        // Account number
        //        case FIELD.ACCOUNT_NUMBER:
        //            {
        //                f = GetCharacterFields( 13, 32 );

        //                if ( f.IndexOf( 'c' ) != f.LastIndexOf( 'c' ) )
        //                {
        //                    return f.Substring( f.IndexOf( 'c' ) + 1, f.LastIndexOf( 'c' ) - f.IndexOf( 'c' ) - 1 ).Trim();
        //                }
        //                else
        //                {
        //                    return f.Substring( 0, f.IndexOf( 'c' ) ).Trim();
        //                }
        //            }

        //        // AUX OnUs
        //        case FIELD.AUX_ON_US:
        //            {
        //                return GetCharacterFields( 45, _content.Length ).Replace( "c", "" ).Trim();
        //            }

        //        // Check Amount
        //        case FIELD.CHECK_AMOUNT:
        //            {
        //                //return GetCharacterFields( 2, 11 ).Trim();

        //                //Return Null; dont fetch amount from check MICR
        //                return string.Empty;
        //            }

        //        // Check Number
        //        case FIELD.CHECK_NUMBER:
        //            {
        //                f = GetCharacterFields( 13, 32 );

        //                if ( f.IndexOf( 'c' ) != f.LastIndexOf( 'c' ) )
        //                {
        //                    return f.Substring( 0, f.IndexOf( 'c' ) /*- 1*/ ).Trim() + f.Substring( f.LastIndexOf( 'c' ) + 1 ).Trim();
        //                }
        //                else
        //                {
        //                    return f.Substring( f.IndexOf( 'c' ) + 1 ).Trim();
        //                }
        //            }

        //        // External Processing code
        //        case FIELD.EXTERNAL_PROCESSING_CODE:
        //            {
        //                return GetCharacterFields( 44, 44 ).Trim();
        //            }

        //        // Routing Number
        //        case FIELD.ROUTING_NUMBER:
        //            {
        //                return GetCharacterFields( 34, 42 ).Trim();
        //            }
        //    }

        //    return string.Empty;
        //}

        #endregion

        #region Methods

        ///// <summary>
        ///// Gets the amount of the check as specified on the MICR line.
        ///// </summary>
        ///// <returns>A string containing the amount characters.</returns>
        //public string GetCheckAmount()
        //{
        //    return GetField( FIELD.CHECK_AMOUNT );
        //}

        /// <summary>
        /// Gets the routing number from the MICR line.
        /// </summary>
        /// <returns>A string containing the routing number characters.</returns>
        public string GetRoutingNumber()
        {
            //return GetField( FIELD.ROUTING_NUMBER );
            return _routingNumber ?? string.Empty;
        }

        /// <summary>
        /// Gets the check number from the MICR line.
        /// </summary>
        /// <returns>A string containing, what is normally, the check number characters.</returns>
        public string GetCheckNumber()
        {
            //return GetField( FIELD.CHECK_NUMBER );
            return _checkNumber ?? string.Empty;
        }

        /// <summary>
        /// Gets the account number from the MICR line.
        /// </summary>
        /// <returns>A string containing the account number this check will draw on.</returns>
        public string GetAccountNumber()
        {
            //return GetField( FIELD.ACCOUNT_NUMBER );
            return _accountNumber ?? string.Empty;
        }

        /// <summary>
        /// Gets the EPC from the MICR line.
        /// </summary>
        /// <returns></returns>
        public string GetExternalProcessingCode()
        {
            //return GetField( FIELD.EXTERNAL_PROCESSING_CODE );
            return string.Empty;
        }

        /// <summary>
        /// GetAuxOnUs
        /// </summary>
        /// <returns>string</returns>
        public string GetAuxOnUs()
        {
            //return GetField( FIELD.AUX_ON_US );
            return string.Empty;
        }

        #endregion
    }
}
