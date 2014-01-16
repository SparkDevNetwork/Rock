//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Block for displaying the history of changes to a particular user.
    /// </summary>
    [DisplayName( "Person History" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Block for displaying the history of changes to a particular user." )]
    public partial class PersonHistory : PersonBlock
    {

        #region Fields

        private int personEntityTypeId = int.MinValue;
        private int groupEntityTypeId = int.MinValue;
        private Dictionary<int, string> families = new Dictionary<int, string>();

        #endregion

        #region Base Control Methods


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ddlCategory.Items.Clear();
            ddlCategory.Items.Add( new ListItem( "", "" ) );
            var personCategory = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid() );
            foreach ( var category in personCategory.Categories.OrderBy( c => c.Order ) )
            {
                ddlCategory.Items.Add( new ListItem( category.Name, category.Id.ToString() ) );
            }

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gHistory.DataKeyNames = new string[] { "id" };
            gHistory.GridRebind += gHistory_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            personEntityTypeId = EntityTypeCache.Read( typeof( Person ) ).Id;
            groupEntityTypeId = EntityTypeCache.Read( typeof( Group ) ).Id;

            if ( Person != null )
            {
                new PersonService().GetFamilies( Person ).ToList().ForEach( f => families.Add( f.Id, f.Name ) );
            }

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Category", ddlCategory.SelectedValue );
            gfSettings.SaveUserPreference( "Summary Contains", tbSummary.Text );
            int? personId = ppWhoFilter.PersonId;
            gfSettings.SaveUserPreference( "Who", personId.HasValue ? personId.ToString() : string.Empty );
            gfSettings.SaveUserPreference( "Date Range", drpDates.DelimitedValues );

            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfSettings_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Category":
                    {
                        int categoryId = int.MinValue;
                        if ( int.TryParse( e.Value, out categoryId ) )
                        {
                            e.Value = CategoryCache.Read( categoryId ).Name;
                        }
                        break;
                    }
                case "Summary Contains":
                    {
                        break;
                    }
                case "Who":
                    {
                        int personId = int.MinValue;
                        if ( int.TryParse( e.Value, out personId ) )
                        {
                            var person = new PersonService().Get( personId );
                            if ( person != null )
                            {
                                e.Value = person.FullName;
                            }
                        }
                        break;
                    }
                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gHistory_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlCategory.SetValue( gfSettings.GetUserPreference( "Category" ) );
            tbSummary.Text = gfSettings.GetUserPreference( "Summary Contains" );
            int personId = int.MinValue;
            if ( int.TryParse( gfSettings.GetUserPreference( "Who" ), out personId ) )
            {
                var person = new PersonService().Get( personId );
                if ( person != null )
                {
                    ppWhoFilter.SetValue( person );
                }
                else
                {
                    gfSettings.SaveUserPreference( "Who", string.Empty );
                }
            }
            drpDates.DelimitedValues = gfSettings.GetUserPreference( "Date Range" );

        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if (Person != null)
            {
                var familyIds = families.Select( f => f.Key ).ToList();
                var qry = new HistoryService().Queryable()
                    .Where( h =>
                        ( h.EntityTypeId == personEntityTypeId && h.EntityId == Person.Id ) ||
                        ( h.EntityTypeId == groupEntityTypeId && familyIds.Contains( h.EntityId ) ) );

                int categoryId = int.MinValue;
                if (int.TryParse(gfSettings.GetUserPreference("Category"), out categoryId))
                {
                    qry = qry.Where( h => h.CategoryId == categoryId);
                }

                string summary = gfSettings.GetUserPreference("Summary Contains");
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    qry = qry.Where( h => h.Summary.Contains( summary ) );
                }

                int personId = int.MinValue;
                if ( int.TryParse( gfSettings.GetUserPreference( "Who" ), out personId ) )
                {
                    qry = qry.Where( h => h.CreatedByPersonId == personId );
                }

                var drp = new DateRangePicker();
                drp.DelimitedValues = gfSettings.GetUserPreference( "Date Range" );
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( h => h.CreationDateTime >= drp.LowerValue.Value );
                }
                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( h => h.CreationDateTime < upperDate );
                }

                SortProperty sortProperty = gHistory.SortProperty;
                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }
                else
                {
                    qry = qry.OrderByDescending( t => t.CreationDateTime );
                }


                gHistory.DataSource = qry.Select( h => new
                {
                    Id = h.Id,
                    CategoryId = h.CategoryId,
                    Category = h.Category.Name,
                    EntityTypeId = h.EntityTypeId,
                    EntityId = h.EntityId,
                    Caption = h.Caption ?? string.Empty,
                    Summary = h.Summary,
                    RelatedEntityTypeId = h.RelatedEntityTypeId ?? 0,
                    RelatedEntityId = h.RelatedEntityId ?? 0,
                    CreatedByPersonId = h.CreatedByPersonId,
                    PersonName = h.CreatedByPerson.NickName + " " + h.CreatedByPerson.LastName,
                    CreationDateTime = h.CreationDateTime
                } ).ToList();
                gHistory.DataBind();
            }

        }

        /// <summary>
        /// Formats the summary.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="summary">The summary.</param>
        /// <returns></returns>
        protected string FormatSummary( int entityTypeId, int entityId, string summary )
        {
            if ( entityTypeId == groupEntityTypeId && families.ContainsKey( entityId ) )
            {
                return string.Format( "[{0}] {1}", families[entityId], summary );
            }

            return summary;
        }

        /// <summary>
        /// Formats the caption.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="relatedEntityTypeId">The related entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        protected string FormatCaption( int categoryId, string caption, int? relatedEntityTypeId, int? entityId )
        {
            // TODO: add an attribute to the person categories that allows formatting a url
            return caption;
        }

        #endregion
    }
}