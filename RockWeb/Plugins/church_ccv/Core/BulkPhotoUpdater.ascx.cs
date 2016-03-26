using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        RockContext rockContext = new RockContext();

        #endregion

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

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);
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
                var attributeValueService = new AttributeValueService( rockContext );
                var personService = new PersonService( rockContext );

                // get a list of people already processed
                var processedPeople = attributeValueService.GetByAttributeId( 16933 )
                    .Where( a => a.Value == "True" )
                    .Select( a => a.EntityId )
                    .ToList();

                // get a list of person photos
                var photoIds = personService.Queryable()
                    .Where( p => p.PhotoId != null )
                    .Where( p => !processedPeople.Contains( p.Id ) )
                    .Select( p => p.PhotoId ).ToList();

                hfPhotoIds.Value = photoIds.AsDelimited(",");

                ShowDetail( hfPhotoIds.Value.SplitDelimitedValues().FirstOrDefault().AsIntegerOrNull() );
            }
        }

        #endregion

        #region Methods

        protected void ShowDetail(int? photoId)
        {
            CurrentPhotoId = photoId;

            imgPhoto.BinaryFileId = photoId;

            var binaryFileService = new BinaryFileService( rockContext );
            var personService = new PersonService( rockContext );

            lProgressBar.Text = string.Format(
                        @"{0} of {1}",
                       SkipCounter + 1,
                       hfPhotoIds.Value.SplitDelimitedValues().Count() );

            // Get photo data so we can see dimensions
            var photoData = binaryFileService.Queryable().Where( b => b.Id == photoId ).FirstOrDefault();
            System.Drawing.Image image = System.Drawing.Image.FromStream( photoData.ContentStream, useEmbeddedColorManagement: false, validateImageData: false );

            var person = personService.Queryable()
                        .Where( p => p.PhotoId == photoId )
                        .Select( p => new
                        {
                            Name = p.NickName + " " + p.LastName,
                            Gender = p.Gender.ToString(),
                            ConnectionStatus = p.ConnectionStatusValue.Value.ToString()
                        } )
                        .FirstOrDefault();

            imgPhoto.Label = person.Name;
            lGender.Text = person.Gender;
            lConnectionStatus.Text = person.ConnectionStatus;

            string imageSquared;

            if ( image.Width == image.Height )
            {
                imageSquared = @"<li style='color: #5c9e7d;'><i class='fa fa-check-circle'></i> The image is square (" + image.Width.ToString() + " X " + image.Height.ToString() + ")</li>";
            }
            else
            {
                imageSquared = @"<li style='color: #bb5454;'><i class='fa fa-times-circle'></i> The image is not square (" + image.Width.ToString() + " X " + image.Height.ToString() + ")</li>";
            }

            string imageLarge;

            if ( image.Width <= 500 || image.Height <= 500 )
            {
                imageLarge = @"<li style='color: #5c9e7d;'><i class='fa fa-check-circle'></i> The image is not too large.</li>";
            }
            else
            {
                imageLarge = @"<li style='color: #bb5454;'><i class='fa fa-times-circle'></i> The image is too large.</li>";
            }

            lChecks.Text = string.Format(
                    @"{0}{1}",
                    imageSquared, 
                    imageLarge );

        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click(object sender, EventArgs e)
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
        protected void btnSkip_Click(object sender, EventArgs e)
        {
            SkipCounter += 1;

            ShowDetail( hfPhotoIds.Value.SplitDelimitedValues().Skip( SkipCounter ).FirstOrDefault().AsIntegerOrNull() );
        }

        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            // todo
        }

        #endregion
    }
}