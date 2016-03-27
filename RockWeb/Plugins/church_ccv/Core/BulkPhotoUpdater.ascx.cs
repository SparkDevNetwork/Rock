using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Plugins.church_ccv.Core
{
    [DisplayName( "Bulk Photo Updater" )]
    [Category( "CCV > Core" )]
    [Description( "Block for mass updating person photos." )]
    public partial class BulkPhotoUpdater : Rock.Web.UI.RockBlock
    {
        #region Fields

        RockContext _rockContext = new RockContext();

        #endregion

        #region Properties

        protected int SkipCounter
        {
            get
            {
                object skipCounter = ViewState["SkipCounter"];
                return skipCounter != null ? ( int ) skipCounter : 0;
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
                return currentPhotoId != null ? ( int ) currentPhotoId : 0;
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                UpdatePhotoList();

                ShowDetail( hfPhotoIds.Value.SplitDelimitedValues().FirstOrDefault().AsIntegerOrNull() );
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

            imgPhoto.BinaryFileId = photoId;

            var binaryFileService = new BinaryFileService( _rockContext );
            var personService = new PersonService( _rockContext );

            lProgressBar.Text = string.Format(
                        @"{0} of {1}",
                       SkipCounter + 1,
                       hfPhotoIds.Value.SplitDelimitedValues().Count() );

            // Get photo data so we can see dimensions
            var photoData = binaryFileService.Queryable().Where( b => b.Id == photoId ).FirstOrDefault();
            System.Drawing.Image image = System.Drawing.Image.FromStream( photoData.ContentStream, useEmbeddedColorManagement: false, validateImageData: false );

            var person = personService.Queryable()
                        .Where( p => p.PhotoId == photoId )
                        .FirstOrDefault();

            imgPhoto.Label = String.Empty;
            lName.Text = string.Format( "<span class='first-word'>{0}</span> {1}", person.NickName, person.LastName );
            lAge.Text = person.Age.ToString() + " yrs old";
            lGender.Text = person.Gender.ToString();
            lConnectionStatus.Text = person.ConnectionStatusValue.ToString();

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
            }
        }

        /// <summary>
        /// Updates the list of photo ids. 
        /// </summary>
        protected void UpdatePhotoList()
        {
            var attributeValueService = new AttributeValueService( _rockContext );
            var personService = new PersonService( _rockContext );

            List<int> dataViewPersonIds = new List<int>();

            // get a list of people already processed
            var processedPeople = attributeValueService.GetByAttributeId( 16933 )
                .Where( a => a.Value == "True" )
                .Select( a => a.EntityId )
                .ToList();

            // filter by dataview
            var dataViewId = dvpDataView.SelectedValueAsInt();
            if ( dataViewId.HasValue )
            {
                dataViewPersonIds = new List<int>();
                var dataView = new DataViewService( _rockContext ).Get( dataViewId.Value );
                if ( dataView != null )
                {
                    var errorMessages = new List<string>();
                    var dvPersonService = new PersonService( _rockContext );
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

            if ( dataViewPersonIds.Any() )
            {
                photoIds = photoIds.Where( p => dataViewPersonIds.Contains( p.Id ) );
            }

            var photoIdList = photoIds.Select( p => p.PhotoId ).ToList();

            hfPhotoIds.Value = photoIdList.AsDelimited( "," );

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
            person.SetAttributeValue( "PhotoProcessed", "True" );
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
            UpdatePhotoList();
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
    }
}