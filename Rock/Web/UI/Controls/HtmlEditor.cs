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
using CKEditor.NET;
using DotLiquid;

namespace Rock.Web.UI.Controls
{
    [ToolboxData( "<{0}:HtmlEditor runat=server></{0}:HtmlEditor>" )]
    public class HtmlEditor : CKEditorControl
    {
        /// <summary>
        /// Gets a value indicating whether merge field tree browser will be displayed.
        /// </summary>
        private bool ShowMergeFieldBrowser
        {
            get 
            {
                return GlobalAttributeKeys.Any() || IncludeAppSettings ||
                    MergeObjectTypes.Any() || MergeFields.Any();
            }
        }

        /// <summary>
        /// Gets or sets the global attribute key masks to display attributes for.  Any global attribute 
        /// that starts with the designated text will be included
        /// </summary>
        public List<string> GlobalAttributeKeys
        {
            get
            {
                var keys = ViewState["GlobalAttributeKeys"] as List<string>;
                if ( keys == null )
                {
                    keys = new List<string>();
                    ViewState["GlobalAttributeKeys"] = keys;
                }
                return keys;
            }

            set { ViewState["GlobalAttributeKeys"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether app setting merge fields will be included
        /// </summary>
        public bool IncludeAppSettings
        {
            get { return ViewState["IncludeAppSettings"] as bool? ?? false; }
            set { ViewState["IncludeAppSettings"] = value; }
        }

        /// <summary>
        /// Gets or sets the merge object types.
        /// </summary>
        public List<Type> MergeObjectTypes
        {
            get
            {
                var mergeObjectTypes = ViewState["MergeObjectTypes"] as List<Type>;
                if ( mergeObjectTypes == null )
                {
                    mergeObjectTypes = new List<Type>();
                    ViewState["MergeObjectTypes"] = mergeObjectTypes;
                }
                return mergeObjectTypes;
            }

            set { ViewState["MergeObjectTypes"] = value; }
        }

        /// <summary>
        /// Gets or sets the merge fields.
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        public List<string> MergeFields
        {
            get
            {
                var mergeFields = ViewState["MergeFields"] as List<string>;
                if ( mergeFields == null )
                {
                    mergeFields = new List<string>();
                    ViewState["MergeFields"] = mergeFields;
                }
                return mergeFields;
            }

            set { ViewState["MergeFields"] = value; }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( ShowMergeFieldBrowser )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "span9" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                base.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "merge-field span3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                string treeviewId = string.Format( "treeview-{0}", this.ID );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "merge-field-content" );
                writer.AddAttribute( HtmlTextWriterAttribute.Id, treeviewId );
                writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                if ( GlobalAttributeKeys.Any() )
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    writer.Write("GlobalAttribute");

                    writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                    var globalAttributeValues = Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues;

                    GlobalAttributeKeys.ForEach( g => {
                        globalAttributeValues
                        .Where( v => v.Key.StartsWith( g, StringComparison.CurrentCultureIgnoreCase ) )
                        .OrderBy( v => v.Key )
                        .ToList()
                        .ForEach( v =>
                        {
                            writer.RenderBeginTag( HtmlTextWriterTag.Li );
                            writer.Write( v.Key );
                            writer.RenderEndTag();
                        } );
                    });

                    writer.RenderEndTag();  // ul

                    writer.RenderEndTag();  // li
                }

                if ( IncludeAppSettings )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Li );
                    writer.Write( "AppSetting" );

                    writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                    foreach ( string settingKey in System.Configuration.ConfigurationManager.AppSettings )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Li );
                        writer.Write( settingKey );
                        writer.RenderEndTag();
                    }

                    writer.RenderEndTag();  // ul

                    writer.RenderEndTag();  // li
                }

                if ( MergeObjectTypes.Any() )
                {
                    foreach ( Type type in MergeObjectTypes )
                    {
                        RenderLiquidFields( type.Name, type, writer, new List<string>() );
                    }
                }

                if ( MergeFields.Any() )
                {
                    foreach ( string field in MergeFields )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Li );
                        writer.Write( field );
                        writer.RenderEndTag();
                    }
                }

                writer.RenderEndTag();  // ul

                writer.RenderEndTag();  // span3

                writer.RenderEndTag();  // row-fluid

                string script = string.Format( "$('#{0}').kendoTreeView();", treeviewId );
                ScriptManager.RegisterStartupScript( this, this.GetType(), "MergeFieldTreeView", script, true );

            }
            else
            {
                base.RenderControl( writer );
            }
        }

        private void RenderLiquidFields(string title, Type type, HtmlTextWriter writer, List<string> alreadyReflected )
        {
            if ( type.IsGenericType && 
                type.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                type.GetGenericArguments().Length == 1 )
            {
                title = title + " (collection)";
                type = type.GetGenericArguments()[0];
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Li );
            writer.Write( title );

            if ( !alreadyReflected.Contains( type.FullName) && typeof( ILiquidizable ).IsAssignableFrom( type ) )
            {
                alreadyReflected.Add( type.FullName );

                writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                foreach ( var propInfo in type.GetProperties() )
                {
                    RenderLiquidFields( propInfo.Name, propInfo.PropertyType, writer, alreadyReflected.ToList() );
                }

                writer.RenderEndTag();
            }

            writer.RenderEndTag();

        }
    }
}