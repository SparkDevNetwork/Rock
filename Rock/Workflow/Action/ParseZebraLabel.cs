//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Parse Zebra Label
    /// </summary>
    [Description( "Parses an uploaded Zebra Label for any available merge codes" )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Parse Zebra Label")]
    public class ParseZebraLabel : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( WorkflowAction action, IEntity entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if ( entity is Model.BinaryFile )
            {
               var binaryFile = (Model.BinaryFile) entity;
                if ( binaryFile.BinaryFileType.Guid != new Guid( SystemGuid.BinaryFiletype.CHECKIN_LABEL ) )
                {
                    errorMessages.Add( "Binary file is not a check-in label" );
                    return false;
                }

                StringBuilder sb = new StringBuilder();

                foreach ( Match match in Regex.Matches( 
                    System.Text.Encoding.Default.GetString( binaryFile.Data.Content ),
                    @"(?<=\^FD)[^\^FS]*(?=\^FS)" ) )
                {
                    sb.AppendFormat( "{0}^|", match.Value );
                }

                binaryFile.LoadAttributes();

                var newValues = new List<AttributeValue>();

                var attributeValue = new AttributeValue();
                attributeValue.Value = sb.ToString();
                newValues.Add( attributeValue );

                binaryFile.AttributeValues["MergeCodes"] = newValues;
                Rock.Attribute.Helper.SaveAttributeValues( binaryFile, null );
            }
            
            return true;
        }
    }
}