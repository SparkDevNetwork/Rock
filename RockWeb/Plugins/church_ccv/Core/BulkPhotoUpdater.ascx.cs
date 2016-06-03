using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using ImageResizer;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Plugins.church_ccv.Core
{
    [DisplayName( "Bulk Photo Updater" )]
    [Category( "CCV > Core" )]
    [Description( "Block for mass updating person photos." )]

    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Photo Processed Attribute", "A person attribute used to track which person photos have been proceesed.", true, false, "", "" )]
    public partial class BulkPhotoUpdater : Rock.Web.UI.RockBlock
    {
        #region Fields

        private Guid? _photoProcessedAttribute = new Guid();

        #endregion

        #region Properties

        protected int SkipCounter
        {
            get
            {
                object skipCounter = ViewState["SkipCounter"];
                return skipCounter != null ? (int)skipCounter : 0;
            }

            set
            {
                ViewState["SkipCounter"] = value;
            }
        }

        protected int? CurrentPhotoPersonId
        {
            get
            {
                return ViewState["CurrentPhotoPersonId"] as int? ?? 0;
            }

            set
            {
                ViewState["CurrentPhotoPersonId"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            dvpDataView.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            imgPhoto.FileSaved += new EventHandler( imgPhoto_FileSaved );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbWarning.Visible = false;
            nbConfigurationWarning.Visible = false;

            _photoProcessedAttribute = GetAttributeValue( "PhotoProcessedAttribute" ).AsGuidOrNull();

            if ( _photoProcessedAttribute != null )
            {
                if ( !Page.IsPostBack )
                {
                    var dataViewVal = this.GetBlockUserPreference( "DataView" );
                    dvpDataView.SetValue( dataViewVal );

                    UpdatePhotoList();

                    ShowPersonDetail( hfPersonIds.Value.SplitDelimitedValues().FirstOrDefault().AsIntegerOrNull() );
                }
            }
            else
            {
                nbConfigurationWarning.Text = "An attribute needs to be configured in block settings";
                nbConfigurationWarning.Visible = true;
                pnlView.Visible = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail page.
        /// </summary>
        protected void ShowPersonDetail( int? personId )
        {
            CurrentPhotoPersonId = personId;
            hfOrphanedPhotoIds.Value = string.Empty;

            if ( CurrentPhotoPersonId != null && CurrentPhotoPersonId != 0 && personId.HasValue )
            {
                var rockContext = new RockContext();

                var personService = new PersonService( rockContext );
                var person = personService.Get( personId.Value );

                lProgressBar.Text = string.Format(
                            @"<span class='label label-info'>{0} of {1}</span>",
                           SkipCounter + 1,
                           hfPersonIds.Value.SplitDelimitedValues().Count() );

                // Get photo data so we can see dimensions
                int? photoId = null;
                if ( person != null && person.PhotoId.HasValue )
                {
                    photoId = person.PhotoId.Value;
                }

                UpdatePhotoDetails( photoId );

                if ( person != null )
                {
                    lName.Text = string.Format( "<span class='first-word'>{0}</span> {1}", person.NickName, person.LastName );

                    if ( person.Age != null )
                    {
                        lAge.Text = person.Age.ToString() + " yrs old";
                    }
                    
                    lGender.Text = person.Gender.ToString();

                    if ( person.ConnectionStatusValue != null)
                    {
                        lConnectionStatus.Text = person.ConnectionStatusValue.ToString();
                    }  
                }

                pnlDetails.Visible = true;
            }
            else
            {
                lProgressBar.Text = string.Empty;

                pnlDetails.Visible = false;
                nbWarning.Visible = true;
            }
        }

        /// <summary>
        /// Updates the photo details.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        private void UpdatePhotoDetails( int? photoId )
        {
            imgPhoto.BinaryFileId = photoId;
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            var photoData = binaryFileService.Queryable().Where( b => b.Id == photoId ).FirstOrDefault();
            UpdatePhotoDetails( photoData );

            imgPhoto.Label = string.Empty;
        }

        /// <summary>
        /// Updates the details.
        /// </summary>
        /// <param name="photoData">The photo data.</param>
        private void UpdatePhotoDetails( BinaryFile photoData )
        {
            var photoDateTime = photoData.ModifiedDateTime ?? photoData.CreatedDateTime;
            if ( photoDateTime.HasValue )
            {
                lPhotoDate.Text = photoDateTime.Value.ToString( "G" );
            }
            else
            {
                lPhotoDate.Text = "unknown date/time";
            }

            using ( System.Drawing.Image image = System.Drawing.Image.FromStream( photoData.ContentStream ) )
            {
                if ( image.Width == image.Height )
                {
                    lDimenions.Text = @"<span class='label label-success'>" + image.Width.ToString() + " X " + image.Height.ToString() + "</span>";
                }
                else
                {
                    lDimenions.Text = @"<span class='label label-danger'>" + image.Width.ToString() + " X " + image.Height.ToString() + "</span>";
                }

                if ( image.Width >= 500 || image.Height >= 500 )
                {
                    lSizeCheck.Text = "<span class='label label-danger'>Too Large</span>";
                    lSizeCheck.Visible = true;
                }
                else
                {
                    lSizeCheck.Visible = false;
                }

                btnShrink.Visible = image.Width > 500;
                nbShrinkWidth.Visible = btnShrink.Visible;

                var photoKiloBytes = photoData.ContentStream.Length / 1024;
                if ( photoKiloBytes > 1024 )
                {
                    lByteSizeCheck.Text = string.Format( @"<span class='label label-danger'>{0}KB</span>", photoData.ContentStream.Length / 1024 );
                    lByteSizeCheck.Visible = true;
                }
                else if ( photoKiloBytes > 100 )
                {
                    lByteSizeCheck.Text = string.Format( @"<span class='label label-warning'>{0}KB</span>", photoData.ContentStream.Length / 1024 );
                    lByteSizeCheck.Visible = true;
                }
                else
                {
                    lByteSizeCheck.Text = string.Format( @"<span class='label label-info'>{0}KB</span>", photoData.ContentStream.Length / 1024 );
                    lByteSizeCheck.Visible = true;
                }
            }
        }

        /// <summary>
        /// Updates the list of photo ids. 
        /// </summary>
        protected void UpdatePhotoList()
        {
            var rockContext = new RockContext();
            var attributeValueService = new AttributeValueService( rockContext );
            var personService = new PersonService( rockContext );

            IQueryable<int> dataViewPersonIdQry = null;

            // get a qry of people already processed
            var processedPeopleQry = attributeValueService
                .GetByAttributeId( AttributeCache.Read( _photoProcessedAttribute.Value ).Id )
                .Where( a => a.Value == "True" )
                .Select( a => a.EntityId );

            // get a list of people from data view
            var dataViewId = dvpDataView.SelectedValueAsInt();
            if ( dataViewId.HasValue )
            {
                var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                if ( dataView != null )
                {
                    var errorMessages = new List<string>();
                    var dvPersonService = new PersonService( rockContext );
                    ParameterExpression paramExpression = dvPersonService.ParameterExpression;
                    Expression whereExpression = dataView.GetExpression( dvPersonService, paramExpression, out errorMessages );

                    dataViewPersonIdQry = dvPersonService
                        .Queryable().AsNoTracking()
                        .Where( paramExpression, whereExpression )
                        .Select( p => p.Id );
                }
            }

            // get a list of person photos
            var qryPerson = personService
                .Queryable().AsNoTracking()
                .Where( p => p.PhotoId != null )
                .Where( p => !processedPeopleQry.Contains( p.Id ) );

            // if data view is selected, then filter the results
            if ( dataViewPersonIdQry != null )
            {
                qryPerson = qryPerson.Where( p => dataViewPersonIdQry.Contains( p.Id ) );
            }

            var personIdList = qryPerson.Select( p => p.Id ).ToList();
            hfPersonIds.Value = personIdList.AsDelimited( "," );

            SkipCounter = 0;

            ShowPersonDetail( hfPersonIds.Value.SplitDelimitedValues().FirstOrDefault().AsIntegerOrNull() );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );

            var person = personService.Queryable()
                    .Where( p => p.Id == CurrentPhotoPersonId )
                    .FirstOrDefault();

            // set photo processed attribute so we know that this photo is done
            Helper.SaveAttributeValue( person, AttributeCache.Read( _photoProcessedAttribute.Value ), "True", rockContext );

            if ( person.PhotoId != imgPhoto.BinaryFileId )
            {
                // note Person PreSave changes ensures that BinaryPhoto.IsTemporary is set to False
                person.PhotoId = imgPhoto.BinaryFileId;
            }

            BinaryFileService binaryFileService = new BinaryFileService( rockContext );

            // if they used the ImageEditor, and cropped it, the uncropped file is still in BinaryFile. So clean it up
            if ( imgPhoto.CropBinaryFileId.HasValue )
            {
                if ( imgPhoto.CropBinaryFileId != person.PhotoId )
                {
                    
                    var binaryFile = binaryFileService.Get( imgPhoto.CropBinaryFileId.Value );
                    if ( binaryFile != null && binaryFile.IsTemporary )
                    {
                        string errorMessage;
                        if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                        {
                            binaryFileService.Delete( binaryFile );
                        }
                    }
                }

                rockContext.SaveChanges();
            }

            rockContext.SaveChanges();
            var orphanedPhotoIds = hfOrphanedPhotoIds.Value.Split( ',' ).AsIntegerList();
            var orphanedPhotos = binaryFileService.GetByIds( orphanedPhotoIds );
            foreach ( var photo in orphanedPhotos )
            {
                string errorMessage;
                if ( binaryFileService.CanDelete( photo, out errorMessage ) )
                {
                    photo.IsTemporary = true;
                }
            }

            rockContext.SaveChanges();


            SkipCounter += 1;
            ShowPersonDetail( hfPersonIds.Value.SplitDelimitedValues().Skip( SkipCounter ).FirstOrDefault().AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the Click event of the btnSkip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSkip_Click( object sender, EventArgs e )
        {
            SkipCounter += 1;

            ShowPersonDetail( hfPersonIds.Value.SplitDelimitedValues().Skip( SkipCounter ).FirstOrDefault().AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the dvpDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void dvpDataView_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetBlockUserPreference( "DataView", dvpDataView.SelectedValue, true );
            UpdatePhotoList();
        }

        /// <summary>
        /// Handles the imgPhoto_FileSaved event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void imgPhoto_FileSaved( object sender, EventArgs e )
        {
            if ( imgPhoto.CropBinaryFileId.HasValue )
            {
                btnShrink.Visible = false;
                nbShrinkWidth.Visible = false;
                var rockContext = new RockContext();
                var binaryFileService = new BinaryFileService( rockContext );

                // Get photo data so we can see dimensions
                var photoData = binaryFileService.Get( imgPhoto.BinaryFileId.Value );

                UpdatePhotoDetails( photoData );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowPersonDetail( hfPersonIds.Value.SplitDelimitedValues().Skip( SkipCounter ).FirstOrDefault().AsIntegerOrNull() );
        }

        #endregion

        /// <summary>
        /// Gets the resized photo.
        /// </summary>
        /// <param name="photoData">The photo data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        private static int GetResizedPhotoId( int photoId, ResizeSettings settings )
        {
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            var photoData = binaryFileService.Get( photoId );

            // ImageResizer is limited to 3600 on resize
            if ( settings.MaxWidth > 3600 || settings.MaxWidth < 100 )
            {
                settings.MaxWidth = 3600;
            }

            int resizedPhotoId = 0;

            using ( MemoryStream resizedStream = new MemoryStream() )
            {
                ImageBuilder.Current.Build( photoData.ContentStream, resizedStream, settings );

                var resizedPhotoData = new BinaryFile();
                resizedPhotoData.ContentStream = resizedStream;
                resizedPhotoData.BinaryFileTypeId = photoData.BinaryFileTypeId;
                resizedPhotoData.FileName = photoData.FileName;
                resizedPhotoData.MimeType = photoData.MimeType;
                resizedPhotoData.IsTemporary = true;

                binaryFileService.Add( resizedPhotoData );
                rockContext.SaveChanges();

                resizedPhotoId = resizedPhotoData.Id;
            }

            return resizedPhotoId;
        }

        /// <summary>
        /// Handles the Click event of the btnShrink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShrink_Click( object sender, EventArgs e )
        {
            ResizeSettings settings = new ResizeSettings();
            settings.MaxWidth = nbShrinkWidth.Text.AsIntegerOrNull() ?? 3600;
            hfOrphanedPhotoIds.Value += string.Format( ",{0}", imgPhoto.BinaryFileId );
            var resizedPhotoId = GetResizedPhotoId( imgPhoto.BinaryFileId.Value, settings );
            UpdatePhotoDetails( resizedPhotoId );
        }

        /// <summary>
        /// Handles the Click event of the btnRotate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRotate_Click( object sender, EventArgs e )
        {
            ResizeSettings settings = new ResizeSettings();
            settings.Rotate = 90;
            hfOrphanedPhotoIds.Value += string.Format( ",{0}", imgPhoto.BinaryFileId );
            var resizedPhotoId = GetResizedPhotoId( imgPhoto.BinaryFileId.Value, settings );
            UpdatePhotoDetails( resizedPhotoId );
        }
    }
}