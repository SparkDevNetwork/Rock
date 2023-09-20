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
    /// Merlin block for Simple Donation customers to add to any RockRMS page.
    /// </summary>
    [DisplayName( "Update Recurring Gift" )]
    [Category( "Simple Donation" )]
    [Description( "Update Recurring Gift block for Simple Donation users to update their recurring gifts during a recurring migration." )]

    #region Block Attributes


    #endregion Block Attributes
    public partial class 
        SimpleDonationUpdateRecurringGift : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        public static class AttributeKey
        {
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        protected static class PageParameterKey
        {
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
            this.BlockUpdated += SimpleDonationUpdateRecurringGift_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            {
                UpdateRecurringGiftSettings();
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );


            if ( !Page.IsPostBack )
            {
				{
                    UpdateRecurringGiftSettings();
				}
            }
        }

        #endregion

        #region Events

        protected void SimpleDonationUpdateRecurringGift_BlockUpdated( object sender, EventArgs e )
        {
            {
                UpdateRecurringGiftSettings();
            }
        }
		
		protected void UpdateRecurringGiftSettings()
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}