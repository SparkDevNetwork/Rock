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
    [DisplayName( "Recurring Migration Progress Bars" )]
    [Category( "Simple Donation" )]
    [Description( "Recurring Migration Progress Bars block for Simple Donation users to track and support recurring donors through transition." )]

    #region Block Attributes

    #endregion Block Attributes
    public partial class 
        SimpleDonationRecurringMigrationProgressBars : Rock.Web.UI.RockBlock
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
            this.BlockUpdated += SimpleDonationRecurringMigrationProgressBars_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            {
                RecurringMigrationProgressBarsSettings();
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );


            if ( !Page.IsPostBack )
            {
				{
                    RecurringMigrationProgressBarsSettings();
				}
            }
        }

        #endregion

        #region Events

        protected void SimpleDonationRecurringMigrationProgressBars_BlockUpdated( object sender, EventArgs e )
        {
            {
                RecurringMigrationProgressBarsSettings();
            }
        }
		
		protected void RecurringMigrationProgressBarsSettings()
        {
        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}