using System;
using System.Diagnostics;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Field;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core
{
    [TestClass]
    public class FieldTypesListing : DatabaseTestsBase
    {
        [TestMethod]
        public void GetSupportedRegistrationConditionalFields()
        {
            var notSupportedBuilder = new StringBuilder();
            var supportedBuilder = new StringBuilder();
            foreach ( var fieldType in FieldTypeCache.All() )
            {
                try
                {
                    if ( fieldType.Field.HasFilterControl() )
                    {
                        //var qualifiers = attribute.AttributeQualifiers.ToDictionary( k => k.Key, v => new ConfigurationValue( v.Value ) );

                        // get the editControl to see if the FieldType supports a ChangeHandler for it (but don't actually use the control)
                        var editControl = fieldType.Field.EditControl( new System.Collections.Generic.Dictionary<string, ConfigurationValue>(), $"temp_editcontrol_attribute_TEST" );

                        if ( fieldType.Field.HasChangeHandler( editControl ) )
                        {
                            supportedBuilder.AppendLine( $"'{fieldType.Name}" );
                            continue;
                        }
                    }
                    else
                    {

                    }

                    notSupportedBuilder.AppendLine( $"{fieldType.Name}" );
                }
                catch(Exception ex )
                {
                    Debug.WriteLine( ex.Message );
                }
                    
            }

            Debug.WriteLine( "Supported Field Types:" );
            Debug.WriteLine( supportedBuilder.ToString() );


            Debug.WriteLine( "Unsupported Field Types:" );
            Debug.WriteLine( notSupportedBuilder.ToString() );
        }
    }
}
