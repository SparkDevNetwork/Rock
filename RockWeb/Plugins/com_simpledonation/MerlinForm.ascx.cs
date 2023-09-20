using System;
using System.ComponentModel;
using System.Web.UI;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Attribute;
using Rock.Model;
using DocumentFormat.OpenXml.Wordprocessing;


namespace Plugins.com_simpledonation
{
    /// <summary>
    /// Merlin Form block for Simple Donation customers to add to any RockRMS page.
    /// </summary>
    [DisplayName( "Merlin Form" )]
    [Category( "Simple Donation" )]
    [Description( "Merlin Form block for Simple Donation users to add a multi-fund and business giving option to any Rock page." )]

    #region Block Attributes

    [TextField(
        "Merlin Widget Key",
        Key = AttributeKey.MerlinFormWidgetId,
        Description = "Enter the key for your Merlin widget provided by Simple Donation (found in your SD admin, not RockRMS). Shares the same key as a widget.",
        DefaultValue = "",
        Order = 1 )]

    [ColorField(
        "Primary Color",
        Key = AttributeKey.MerlinFormPrimaryColor,
        Description = "The primary color for your Merlin Form buttons.",
        DefaultValue = "004371",
        Order = 2 )]

    #endregion Block Attributes
    public partial class 
        SimpleDonationMerlinForm : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        public static class AttributeKey
        {
            public const string MerlinFormWidgetId = "MerlinFormWidgetId";
            public const string MerlinFormPrimaryColor = "MerlinFormPrimaryColor";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        protected static class PageParameterKey
        {
            public const string StarkId = "StarkId";
        }

        #endregion PageParameterKeys

        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += MerlinFormBlock_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            {
                MerlinFormSettings();
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );


            if ( !Page.IsPostBack )
            {
				{
					MerlinFormSettings();
				}
            }
        }

        #endregion

        #region Events

        protected void MerlinFormBlock_BlockUpdated( object sender, EventArgs e )
        {
            {
                MerlinFormSettings();
            }
        }
		
		protected void MerlinFormSettings()
            {
            var rockContext = new RockContext(); // set the context to pull from
                var domainUrl = new AttributeValueService( rockContext ).Queryable() 
                    .FirstOrDefault( av => av.Attribute.Key == "Domain" && av.Attribute.Description == "Your Simple Donation API Domain" && av.Attribute.FieldType.Name == "Url Link" ); // pulls the Value from AttributeValue table for the Attribute with Key of 'Domain' and Descripton of 'Your Simple Donation API Domain'

            string FormWidgetIdCheck = this.GetAttributeValue( "MerlinFormWidgetId" );
            string MerlinFormWidgetId = string.Concat( "?key=", this.GetAttributeValue( "MerlinFormWidgetId" ) ); // Retrieves the Merlin widget id block attribute
            string PrimaryColorSelection = string.Concat( "data-merlin-primary-color='", this.GetAttributeValue( "MerlinFormPrimaryColor" ).Split( '#' ).Last(), "'" ); // Retrieves the Merlin primary color block attribute and removes any # from the hex code if present

            merlinFormWarning.Visible = !FormWidgetIdCheck.IsNotNullOrWhiteSpace(); // Show warning to populate Merlin widget Id upon first launch
            merlinFormControl.Visible = FormWidgetIdCheck.IsNotNullOrWhiteSpace(); // Do not show Give Now button if there is no Merlin widget Id present, currently requires a forced page refresh for user upon setting save


            // Constructs the Merlin Form script html for button
            string MerlinFormFull = string.Concat( "<div id='merlin-form'", PrimaryColorSelection, "></div><script async src='https://merlinform.simpledonation.com/assets/form-install-script.js", MerlinFormWidgetId, "'></script>" );
            // Constructs the missing Merlin Form widget Id warning
            string MerlinFormWarningFull = string.Concat( "<div class='alert alert-warning'><div class='row'><div class='col-md-1 text-center'><i class='fa fa-hat-wizard fa-4x' style='padding: 10px 0; margin: 0;'></i></div><div class='col-md-11'><p>Merlin Form is <em>magical</em>, but we cannot guess your widget key.<br/>Please copy your Merlin key from your <b><a href='", domainUrl, "/admin' target='_blank'>", domainUrl, "/admin</a></b> dashboard and paste it into the block settings or simply contact <b><a href='mailto:happy@simpledonation.com?subject=Help,%20I%20need%20my%20Merlin%20Widget%20key!' target='_blank'>happy@simpledonation.com</a></b> and we'll provide this for you</i>.</p><p>If you'd like to see how to setup your Merlin Form block visit <b><a href='https://help.simpledonation.com/article/91-merlin-forms' target='_blank'>help.simpledonation.com</a></b></p></div></div></div><div class='alert alert-danger'><div class='row'><div class='col-md-1 text-center'><i class='fa fa-exclamation-triangle fa-4x' style='padding: 0 0 10px 0;'></i></div><div class='col-md-11'><p>Ensure that the page Cache Duration is set to zero '0' or in v12 the Cacheability Type to 'No-Cache' with Max Age or Max Shared Age set to zero '0' in the Advanced page setting for any page utilizing the Merlin block.</p><p>If you'd like to see how to setup your Merlin Form block visit <b><a href='https://help.simpledonation.com/article/91-merlin-forms' target='_blank'>help.simpledonation.com</a></b></p></div></div></div>" );
            
            merlinFormControl.Text = MerlinFormFull; // sends final consrtucted script with user settings to webform
            merlinFormWarning.Text = MerlinFormWarningFull; // sends final consrtucted missing widget Id warning to webform
        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}