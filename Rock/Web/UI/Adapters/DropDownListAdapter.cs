//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;

namespace Rock.Web.UI.Adapters
{
    /// <summary>
    /// Custom control adapter for rendering a dropdown list
    /// </summary>
    public class DropDownListAdapter : WebControlAdapter
    {
        /// <summary>
        /// Generates the target-specific inner markup for the Web control to which the control adapter is attached.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderContents( HtmlTextWriter writer )
        {
            DropDownList list = Control as DropDownList;
            string currentOptionGroup;
            var renderedOptionGroups = new List<string>();

            foreach ( ListItem item in list.Items )
            {
                if ( item.Attributes["OptionGroup"] != null )
                {
                    //'The item is part of an option group'
                    currentOptionGroup = item.Attributes["OptionGroup"];

                    //'the option header was already written, just render the list item'
                    if ( renderedOptionGroups.Contains( currentOptionGroup ) )
                    {
                        RenderListItem( item, writer );
                    }
                    else
                    {
                        //the header was not written- do that first
                        if ( renderedOptionGroups.Count > 0 )
                        {
                            //need to close previous group'
                            RenderOptionGroupEndTag( writer ); 
                        }

                        RenderOptionGroupBeginTag( currentOptionGroup, writer );
                        renderedOptionGroups.Add( currentOptionGroup );

                        RenderListItem( item, writer );
                    }
                }
                else
                {
                    //default behavior: render the list item as normal'
                    RenderListItem( item, writer );
                }

                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( list.UniqueID, item.Value );
                }
            }

            if ( renderedOptionGroups.Count > 0 )
            {
                RenderOptionGroupEndTag( writer );
            }

            if ( Page != null && Page.ClientScript != null )
            {
                Page.ClientScript.RegisterForEventValidation( list.UniqueID );
            }
        }

        private void RenderOptionGroupBeginTag( string name, HtmlTextWriter writer )
        {
            writer.WriteBeginTag( "optgroup" );
            writer.WriteAttribute( "label", name );
            writer.Write( HtmlTextWriter.TagRightChar );
            writer.WriteLine();
        }

        private void RenderOptionGroupEndTag( HtmlTextWriter writer )
        {
            writer.WriteEndTag( "optgroup" );
            writer.WriteLine();
        }

        private void RenderListItem( ListItem item, HtmlTextWriter writer )
        {
            writer.WriteBeginTag( "option" );
            writer.WriteAttribute( "value", item.Value, true );
            if ( item.Selected )
            {
                writer.WriteAttribute( "selected", "selected", false );
            }

            foreach ( string key in item.Attributes.Keys )
            {
                if ( !key.Equals("optiongroup", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    writer.WriteAttribute( key, item.Attributes[key] );
                }
            }

            writer.Write( HtmlTextWriter.TagRightChar );

            HttpUtility.HtmlEncode( item.Text, writer );
            writer.WriteEndTag( "option" );
            writer.WriteLine();
        }
    }
}