using System;
using System.ComponentModel;

using Rock;
using Rock.Data;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_newpointe.FileUpload
{

    [DisplayName( "File Upload" )]
    [Category( "NewPointe.org Web Blocks" )]
    [Description( "Simple block to upload files." )]

    [BinaryFileTypeField( "File Type", "The file type to use to save the file.", true, "2CF8A379-33BB-49C1-8CBB-DF8B822C3E75", "", 0 )]
    public partial class FileUpload : Rock.Web.UI.RockBlock
    {

        protected void Page_Load( object sender, EventArgs e )
        {
            fuFile.BinaryFileTypeGuid = GetAttributeValue( "FileTypeGUID" ).AsGuid();
        }

        protected void fuFile_FileUploaded( object sender, EventArgs e )
        {

            if ( fuFile.BinaryFileId.HasValue )
            {
                var rockContext = new RockContext();
                var file = new BinaryFileService( rockContext ).Get( fuFile.BinaryFileId.Value );

                if ( file != null && file.Path != null && file.Url != null )
                {
                    file.IsTemporary = false;
                    rockContext.SaveChanges();

                    Succeed( "<br />" + file.Path + "<br />" + file.Url );
                }
                else
                {
                    Fail( "Couldn't find the uploaded file." );
                }
            }
            else
            {
                Fail( "Couldn't find the uploaded file." );
            }

        }

        protected void Succeed( string message )
        {
            nbSuccess.Visible = true;
            nbSuccess.Title = "File Uploaded Successfully!";
            nbSuccess.NotificationBoxType = NotificationBoxType.Success;
            nbSuccess.Text = message;
        }

        protected void Fail( string message )
        {
            nbSuccess.Visible = true;
            nbSuccess.Title = "File Upload Failed!";
            nbSuccess.NotificationBoxType = NotificationBoxType.Danger;
            nbSuccess.Text = message;
        }

    }
}