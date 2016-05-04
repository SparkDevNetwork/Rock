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

        protected int? CurrentPhotoId
        {
            get
            {
                object currentPhotoId = ViewState["CurrentPhotoId"];
                return currentPhotoId != null ? (int)currentPhotoId : 0;
            }

            set
            {
                ViewState["CurrentPhotoId"] = value;
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

                    ShowDetail( hfPhotoIds.Value.SplitDelimitedValues().FirstOrDefault().AsIntegerOrNull() );
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
        protected void ShowDetail( int? photoId )
        {
            CurrentPhotoId = photoId;

            if ( CurrentPhotoId != null && CurrentPhotoId != 0 )
            {
                imgPhoto.BinaryFileId = photoId;

                var rockContext = new RockContext();
                var binaryFileService = new BinaryFileService( rockContext );
                var personService = new PersonService( rockContext );

                lProgressBar.Text = string.Format(
                            @"<span class='label label-info'>{0} of {1}</span>",
                           SkipCounter + 1,
                           hfPhotoIds.Value.SplitDelimitedValues().Count() );

                // Get photo data so we can see dimensions
                var photoData = binaryFileService.Queryable().Where( b => b.Id == photoId ).FirstOrDefault();

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
                    nbShrinkPercent.Visible = btnShrink.Visible;

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

                var person = personService.Queryable()
                        .Where( p => p.PhotoId == photoId )
                        .FirstOrDefault();

                imgPhoto.Label = String.Empty;
                lName.Text = string.Format( "<span class='first-word'>{0}</span> {1}", person.NickName, person.LastName );
                lAge.Text = person.Age.ToString() + " yrs old";
                lGender.Text = person.Gender.ToString();
                lConnectionStatus.Text = person.ConnectionStatusValue.ToString();

                pnlDetails.Visible = true;
            }
            else
            {
                lProgressBar.Text = "";

                pnlDetails.Visible = false;
                nbWarning.Visible = true;
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

            List<int> dataViewPersonIds = new List<int>();

            // get a list of people already processed
            var processedPeople = attributeValueService
                .GetByAttributeId( AttributeCache.Read( _photoProcessedAttribute.Value ).Id )
                .Where( a => a.Value == "True" )
                .Select( a => a.EntityId )
                .ToList();

            // get a list of people from data view
            var dataViewId = dvpDataView.SelectedValueAsInt();
            if ( dataViewId.HasValue )
            {
                dataViewPersonIds = new List<int>();
                var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                if ( dataView != null )
                {
                    var errorMessages = new List<string>();
                    var dvPersonService = new PersonService( rockContext );
                    ParameterExpression paramExpression = dvPersonService.ParameterExpression;
                    Expression whereExpression = dataView.GetExpression( dvPersonService, paramExpression, out errorMessages );

                    var dataViewPersonIdQry = dvPersonService
                        .Queryable().AsNoTracking()
                        .Where( paramExpression, whereExpression )
                        .Select( p => p.Id );

                    dataViewPersonIds = dataViewPersonIdQry.ToList();
                }
            }

            // get a list of person photos
            var photoIds = personService
                .Queryable().AsNoTracking()
                .Where( p => p.PhotoId != null )
                .Where( p => !processedPeople.Contains( p.Id ) );

            // if data view is selected, then filter the results
            if ( dataViewPersonIds.Any() || ( !dataViewPersonIds.Any() && dataViewId.HasValue ) )
            {
                photoIds = photoIds.Where( p => dataViewPersonIds.Contains( p.Id ) );
            }

            var photoIdList = photoIds.Select( p => p.PhotoId ).ToList();
            hfPhotoIds.Value = photoIdList.AsDelimited( "," );

            SkipCounter = 0;

            ShowDetail( hfPhotoIds.Value.SplitDelimitedValues().FirstOrDefault().AsIntegerOrNull() );
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
            // todo
            RockContext rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );

            var person = personService.Queryable()
                    .Where( p => p.PhotoId == CurrentPhotoId )
                    .FirstOrDefault();

            // set photo processed attribute so we know that this photo is done
            person.LoadAttributes();
            person.SetAttributeValue( AttributeCache.Read( _photoProcessedAttribute.Value ).Key, "True" );
            person.SaveAttributeValues( rockContext );

            if ( person.PhotoId != imgPhoto.BinaryFileId )
            {
                person.PhotoId = imgPhoto.BinaryFileId;
            }

            // if they used the ImageEditor, and cropped it, the uncropped file is still in BinaryFile. So clean it up
            if ( imgPhoto.CropBinaryFileId.HasValue )
            {
                if ( imgPhoto.CropBinaryFileId != person.PhotoId )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
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

            SkipCounter += 1;
            ShowDetail( hfPhotoIds.Value.SplitDelimitedValues().Skip( SkipCounter ).FirstOrDefault().AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the Click event of the btnSkip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSkip_Click( object sender, EventArgs e )
        {
            SkipCounter += 1;

            ShowDetail( hfPhotoIds.Value.SplitDelimitedValues().Skip( SkipCounter ).FirstOrDefault().AsIntegerOrNull() );
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
                var rockContext = new RockContext();
                var binaryFileService = new BinaryFileService( rockContext );

                // Get photo data so we can see dimensions
                var photoData = binaryFileService.Get( imgPhoto.BinaryFileId.Value );

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
                        lSizeCheck.Text = @"<span class='label label-danger'>Too Large</span>";
                        lSizeCheck.Visible = true;
                    }
                    else
                    {
                        lSizeCheck.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( hfPhotoIds.Value.SplitDelimitedValues().Skip( SkipCounter ).FirstOrDefault().AsIntegerOrNull() );
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnShrink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShrink_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );

            // Get photo data
            var photoData = binaryFileService.Get( imgPhoto.BinaryFileId.Value );

            using ( System.Drawing.Image image = System.Drawing.Image.FromStream( photoData.ContentStream ) )
            {
                ResizeSettings settings = new ResizeSettings();
                var keepPercent = ( 100.00 - nbShrinkPercent.Value ) / 100.00;
                settings.MaxWidth = (int)( image.Width * keepPercent );
                
                // ImageResizer is limited to 3600 on resize
                settings.MaxHeight = 3600;
                if ( settings.MaxWidth > 3600 )
                {
                    settings.MaxWidth = 3600;
                }
                MemoryStream resizedStream = new MemoryStream();

                ImageBuilder.Current.Build( photoData.ContentStream, resizedStream, settings );
                photoData.ContentStream = resizedStream;
                rockContext.SaveChanges();

                ShowDetail( imgPhoto.BinaryFileId.Value );
            }

        }
    }
}