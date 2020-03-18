using Rock.Data;

namespace com.bemaservices.RemoteCheckDeposit.Model
{
    /// <summary>
    /// Queries the database for instances of the ImageCashLetterFileFormat.
    /// </summary>
    public class ImageCashLetterFileFormatService : Service<ImageCashLetterFileFormat>
    {
        public ImageCashLetterFileFormatService( DbContext dbContext ) : base( dbContext )
        {
        }
    }
}
