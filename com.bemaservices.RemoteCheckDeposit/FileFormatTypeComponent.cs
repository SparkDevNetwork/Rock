using System;
using System.IO;
using System.Collections.Generic;

using Rock;
using Rock.Attribute;
using Rock.Extension;
using Rock.Model;

using com.bemaservices.RemoteCheckDeposit.Model;

namespace com.bemaservices.RemoteCheckDeposit
{
    /// <summary>
    /// Base class for Image Cash Letter file format type components.
    /// </summary>
    public abstract class FileFormatTypeComponent : Component
    {
        #region Properties

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();

                defaults.Add( "Active", "True" );
                defaults.Add( "Order", "0" );

                return defaults;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileFormatTypeComponent" /> class.
        /// </summary>
        public FileFormatTypeComponent()
            : base( false )
        {
            //
            // The false in the base( false) call above is to prevent the creation of the attributes
            // until they are saved.
            //
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Use GetAttributeValue( ImageCashLetterFileFormat fileFormat, string key) instead. File Format Type
        /// component attribute values are specific to the File Format instance (rather than global).
        /// This method will throw an exception.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">File Format Type Component attributes are saved specific to the file format type, which requires that the current financial gateway is included in order to load or retrieve values. Use the GetAttributeValue( ImageCashLetterFileFormat fileFormat, string key ) method instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "File Format Type Component attributes are saved specific to the file format type, which requires that the current financial gateway is included in order to load or retrieve values. Use the GetAttributeValue( ImageCashLetterFileFormat fileFormat, string key ) method instead." );
        }

        /// <summary>
        /// Component do not have an order, always return 0.
        /// </summary>
        public override int Order
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Component cannot be made inactive, always return true.
        /// </summary>
        public override bool IsActive
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the attribute value for the file format
        /// </summary>
        /// <param name="fileFormat">The file format.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( ImageCashLetterFileFormat fileFormat, string key )
        {
            if ( fileFormat.AttributeValues == null )
            {
                fileFormat.LoadAttributes();
            }

            var values = fileFormat.AttributeValues;
            if ( values != null && values.ContainsKey( key ) )
            {
                var keyValues = values[key];
                if ( keyValues != null )
                {
                    return keyValues.Value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// If the component supports importing financial data from an X9.37 file it should
        /// override this method to return true.
        /// </summary>
        /// <returns>True if the component supports importing data.</returns>
        public virtual bool SupportsImport()
        {
            return false;
        }

        /// <summary>
        /// Imports batches from a data file. The batches should not be added to the database
        /// but only initialized.
        /// </summary>
        /// <param name="fileFormat">The <see cref="ImageCashLetterFileFormat" /> that is being used to import this data.</param>
        /// <param name="content">A <see cref="Stream"/> that can be used to access the file data.</param>
        /// <param name="errorMessages">On return will contain a list of error messages if not empty.</param>
        /// <returns>A list of FinancialBatches that are ready to be saved to the database.</returns>
        public virtual List<FinancialBatch> ImportBatches( ImageCashLetterFileFormat fileFormat, Stream content, out List<string> errorMessages )
        {
            throw new NotImplementedException( "Importing of batches is not supported by this component." );
        }

        /// <summary>
        /// Exports a collection of batches to a binary file that can be downloaded by the user
        /// and sent to their financial institution. The returned BinaryFile should not have been
        /// saved to the database yet.
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="errorMessages">On return will contain a list of error messages if not empty.</param>
        /// <returns>A <see cref="Stream" /> of data that should be downloaded to the user in a file.</returns>
        public abstract Stream ExportBatches( ExportOptions options, out List<string> errorMessages );

        #endregion

        #region System Settings

        /// <summary>
        /// Gets the system setting. This is just a helper method.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetSystemSetting( string key )
        {
            key = "com.bemaservices.RemoteCheckDeposit." + key;

            return Rock.Web.SystemSettings.GetValue( key );
        }

        /// <summary>
        /// Sets the system setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected void SetSystemSetting( string key, string value )
        {
            key = "com.bemaservices.RemoteCheckDeposit." + key;

            Rock.Web.SystemSettings.SetValue( key, value );
        }

        #endregion
    }
}
