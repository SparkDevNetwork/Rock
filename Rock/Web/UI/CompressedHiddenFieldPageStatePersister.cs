using System;
using System.IO;
using System.IO.Compression;
using System.Web.Security;
using System.Web.UI;

namespace Rock.Web.UI
{
    /// <summary>
    /// Optionally compresses the ViewState before storing it into a hidden field on the
    /// page to increase postback speed.
    /// </summary>
    public class CompressedHiddenFieldPageStatePersister : PageStatePersister
    {
        /// <summary>
        /// The size of the ViewState at which to compress it.
        /// </summary>
        public int SizeThreshold { get; set; }

        /// <summary>
        /// Instantiate a new PageStatePersister that will store the ViewState in a hidden field
        /// and optionally compress the data if it is greater than the size threshold.
        /// </summary>
        /// <param name="page">The Page whose ViewState we need to load or save.</param>
        /// <param name="sizeThreshold">If the length of the base64 encoded ViewState is greater than or equal to this number then it will be encrypted. A value of 0 means never encrypt.</param>
        public CompressedHiddenFieldPageStatePersister( Page page, int sizeThreshold )
            : base( page )
        {
            SizeThreshold = sizeThreshold;
        }

        //
        // Load ViewState and ControlState.
        //
        public override void Load()
        {
            string viewState = Page.Request.Form["__CVIEWSTATE"];
            byte[] bytes = Convert.FromBase64String( viewState );

            //
            // Decrypt.
            //
            if ( Page.Request.Form["__CVIEWSTATEENC"] == "1" )
            {
                bytes = MachineKey.Unprotect( bytes );
            }

            //
            // Uncompress.
            //
            if ( Page.Request.Form["__CVIEWSTATESIZE"] != "0" )
            {
                using ( MemoryStream output = new MemoryStream() )
                {
                    using ( MemoryStream input = new MemoryStream() )
                    {
                        input.Write( bytes, 0, bytes.Length );
                        input.Position = 0;
                        using ( GZipStream gzip = new GZipStream( input, CompressionMode.Decompress, true ) )
                        {
                            gzip.CopyTo( output );
                        }
                    }

                    bytes = output.ToArray();
                }
            }

            //
            // Deserialize the data back into ViewState and ControlState.
            //
            Pair pair = ( Pair ) new LosFormatter().Deserialize( Convert.ToBase64String( bytes ) );
            ViewState = pair.First;
            ControlState = pair.Second;
        }

        //
        // Persist any ViewState and ControlState.
        //
        public override void Save()
        {
            //
            // Serialize the ViewState and ControlState data for inclusion in the hidden field.
            //
            StringWriter writer = new StringWriter();
            new LosFormatter().Serialize( writer, new Pair( ViewState, ControlState ) );
            string viewStateString = writer.ToString();

            //
            // Get the uncompressed size and convert from Base64 to raw data.
            //
            int uncompressedSize = viewStateString.Length;
            byte[] bytes = Convert.FromBase64String( viewStateString );

            //
            // Compress if the size is past the threshhold.
            //
            if ( SizeThreshold != 0 && uncompressedSize >= SizeThreshold )
            {
                MemoryStream output = new MemoryStream();

                using ( GZipStream gzip = new GZipStream( output, CompressionMode.Compress, true ) )
                {
                    gzip.Write( bytes, 0, bytes.Length );
                }
                bytes = output.ToArray();

                ScriptManager.RegisterHiddenField( Page, "__CVIEWSTATESIZE", uncompressedSize.ToString() );
            }
            else
            {
                ScriptManager.RegisterHiddenField( Page, "__CVIEWSTATESIZE", "0" );
            }

            //
            // Encrypt unless we are told not to.
            //
            if ( Page.ViewStateEncryptionMode != ViewStateEncryptionMode.Never )
            {
                bytes = MachineKey.Protect( bytes );
                ScriptManager.RegisterHiddenField( Page, "__CVIEWSTATEENC", "1" );
            }
            else
            {
                ScriptManager.RegisterHiddenField( Page, "__CVIEWSTATEENC", "0" );
            }

            ScriptManager.RegisterHiddenField( Page, "__CVIEWSTATE", Convert.ToBase64String( bytes ) );
        }
    }
}
