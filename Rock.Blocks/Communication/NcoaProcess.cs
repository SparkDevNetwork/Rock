using System.Collections.Generic;
using System.ComponentModel;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility.Settings.SparkData;
using Rock.ViewModels.Blocks.Communication.NcoaProcess;
using Rock.Web.Cache;
using Rock.NCOA;
using System.Linq;

namespace Rock.Blocks.Communication
{
    [DisplayName( "NCOA Process" )]
    [Category( "Communication" )]
    [Description( "Displays the NCOA Process Steps." )]
    [IconCssClass( "fa fa-list" )]

    [Rock.SystemGuid.EntityTypeGuid( "AFE1B685-B24C-41A2-BFDE-5F921EE75063" )]
    [Rock.SystemGuid.BlockTypeGuid( "C3B61806-9F45-4CCF-8866-07D116E629A5" )]
    public class NcoaProcess : RockBlockType
    {

        private SparkDataConfig sparkDataConfig = new SparkDataConfig();

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                sparkDataConfig = Ncoa.GetSettings();
                var bag = new NcoaProcessSavedSettingsBag();
                var dataViewService = new DataViewService( rockContext );
                var definedValueService = new DefinedValueService( rockContext );

                if ( sparkDataConfig.NcoaSettings.PersonDataViewId.HasValue )
                {
                    bag.PersonDataView = dataViewService.Get( sparkDataConfig.NcoaSettings.PersonDataViewId.Value ).ToListItemBag();
                }
                if ( sparkDataConfig.NcoaSettings.InactiveRecordReasonId.HasValue )
                {
                    bag.InactiveRecordReason = definedValueService.Get( sparkDataConfig.NcoaSettings.InactiveRecordReasonId.Value ).ToListItemBag();
                }
                bag.UploadFileReference = sparkDataConfig.NcoaSettings.UploadFileReference;
                bag.MinimumMoveDistance = sparkDataConfig.NcoaSettings.MinimumMoveDistance;
                bag.Is48MonthMoveChecked = sparkDataConfig.NcoaSettings.Is48MonthMoveChecked;
                bag.IsInvalidAddressesChecked = sparkDataConfig.NcoaSettings.IsInvalidAddressesChecked;

                return bag;
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Prepares and exports the file used for NCOA 
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult PrepareExportFile( string dataViewValue )
        {
            var ncoaService = new Ncoa();
            var bag = new NcoaProcessBag();

            var addresses = ncoaService.GetAddresses( dataViewValue ).Values.ToList();

            if ( addresses.Count < 100 )
            {
                return ActionBadRequest( "A minimum of 100 unique records with valid addresses are required in order to return NCOA move data from the USPS when your file is processed." );
            }

            bag.Addresses = addresses;
            bag.SuccessMessage = "Now go to <a style=\"color:#006dcc;text-decoration:none;border:0;\" href=\"https://app.truencoa.com/\">TrueNCOA</a> to upload the file there.  When they are finshed processing, you can import their results in Step 2.";

            sparkDataConfig = Ncoa.GetSettings();
            sparkDataConfig.NcoaSettings.PersonDataViewId = DataViewCache.Get( dataViewValue ).Id;
            Ncoa.SaveSettings( sparkDataConfig );

            return ActionOk( bag );
        }

        /// <summary>
        /// Processes the NCOA file imported by the individual.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult ProcessNcoaImportFile( NcoaProcessBag bag )
        {
            var ncoaService = new Ncoa();
            using ( var rockContext = new RockContext() )
            {
                if ( bag.NcoaFileUploadReference.Value != null )
                {
                    var inactiveReason = DefinedValueCache.Get( bag.InactiveReason.Value.AsGuid() );
                    var binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( bag.NcoaFileUploadReference.Value );
                    var stringContnet = binaryFile.ContentsToString();

                    ncoaService.NcoaRecordBuilder( stringContnet, out List<NcoaReturnRecord> ncoaReturnRecords );
                    var (successMessage, errorMessage) = ncoaService.PendingExport( inactiveReason, bag.MarkInvalidAsPrevious, bag.Mark48MonthAsPrevious, bag.MinMoveDistance, ncoaReturnRecords );

                    if ( !string.IsNullOrEmpty( errorMessage ) )
                    {
                        return ActionBadRequest( errorMessage );
                    }

                    sparkDataConfig = Ncoa.GetSettings();
                    sparkDataConfig.NcoaSettings.UploadFileReference = bag.NcoaFileUploadReference;
                    sparkDataConfig.NcoaSettings.IsInvalidAddressesChecked = bag.MarkInvalidAsPrevious;
                    sparkDataConfig.NcoaSettings.Is48MonthMoveChecked = bag.Mark48MonthAsPrevious;
                    sparkDataConfig.NcoaSettings.MinimumMoveDistance = bag.MinMoveDistance;
                    sparkDataConfig.NcoaSettings.InactiveRecordReasonId = inactiveReason.Id;
                    Ncoa.SaveSettings( sparkDataConfig );

                    bag.SuccessMessage = successMessage;

                    return ActionOk( bag );
                }
                return ActionBadRequest( "An error occurred while accessing your uploaded file." );
            }
        }

        #endregion Block Actions
    }
}
