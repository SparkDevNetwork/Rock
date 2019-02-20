using System.Text;

namespace ImageSafeInterop
{
    public struct CheckData
    {
        public bool HasError { get; set; }

        /// <summary>
        /// Gets or sets the image data.
        /// </summary>
        /// <value>
        /// The image data.
        /// </value>
        public byte[] ImageData { get; set; }

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
        public string ScannedCheckMicrData { get; set; }

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
                if (!string.IsNullOrEmpty(AccountNumber) && AccountNumber.Length > 4)
                {
                    int length = AccountNumber.Length;
                    string result = new string('x', length - 4) + AccountNumber.Substring(length - 4);
                    return result;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public StringBuilder Errors { get; set; }
    }
}
