//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;

namespace Rock.Field.Types
    
    /// <summary>
    /// Field used to save and dispaly a text value
    /// </summary>
    [Serializable]
    public class Date : FieldType
        
        /// <summary>
        /// Formats date display
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
            
            string formattedValue = string.Empty;

			DateTimeOffset dateValue = DateTimeOffset.MinValue;
			if ( DateTimeOffset.TryParse( value, out dateValue ) )
			    
				formattedValue = dateValue.DateTime.ToShortDateString();

				if ( configurationValues != null &&
					configurationValues.ContainsKey( "format" ) &&
					!String.IsNullOrWhiteSpace( configurationValues["format"].Value ) )
				    
					try
					    
						formattedValue = dateValue.ToString( configurationValues["format"].Value );
					}
					catch
					    
						formattedValue = dateValue.DateTime.ToShortDateString();
					}
				}

				if ( !condensed )
				    
					if ( configurationValues != null &&
						configurationValues.ContainsKey( "displayDiff" ) )
					    
						bool displayDiff = false;
						if ( bool.TryParse( configurationValues["displayDiff"].Value, out displayDiff ) && displayDiff )
							formattedValue += " " + dateValue.ToElapsedString( true, false );
					}
				}
			}

			return formattedValue;
		}

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
		public override List<string> ConfigurationKeys()
		    
			var keys = base.ConfigurationKeys();
			keys.Add( "format" );
			keys.Add( "displayDiff" );
			return keys;
		}

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
		public override System.Collections.Generic.List<System.Web.UI.Control> ConfigurationControls()
		    
			var controls = base.ConfigurationControls();

			TextBox textbox = new TextBox();
			controls.Add( textbox );

			HtmlGenericControl lbl = new HtmlGenericControl( "label" );
			controls.Add( lbl );
			lbl.AddCssClass( "checkbox" );

			CheckBox cbDisplayDiff = new CheckBox();
			lbl.Controls.Add( cbDisplayDiff );

			return controls;
		}

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
		public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
		    
			base.SetConfigurationValues( controls, configurationValues );

			if ( controls != null && controls.Count >= 2 )
			    
				int i = controls.Count - 2;
				if ( controls[i] != null && controls[i] is TextBox &&
					configurationValues.ContainsKey( "format" ) )
					( (TextBox)controls[i] ).Text = configurationValues["format"].Value ?? string.Empty;
				i++;
				if ( controls[i] != null && controls[i] is HtmlGenericControl &&
					controls[i].Controls.Count > 0 && controls[i].Controls[0] is CheckBox &&
					configurationValues.ContainsKey( "displayDiff" ) )
				    
					bool displayDiff = false;
					if ( !bool.TryParse( configurationValues["displayDiff"].Value ?? "False", out displayDiff ) )
						displayDiff = false;

					( (CheckBox)controls[i].Controls[0] ).Checked = displayDiff;
				}
			}
		}

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
		public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
		    
			var values = base.ConfigurationValues( controls );
			values.Add( "format", new ConfigurationValue( "Date Format", "The format string to use for date (default is system short date)", "" ) );
			values.Add( "displayDiff", new ConfigurationValue( "Display Date Span", "Display the number of years between value and current date", "False" ) );

			if ( controls != null && controls.Count >= 2 )
			    
				int i = controls.Count - 2;
				if ( controls[i] != null && controls[i] is TextBox )
					values["format"].Value = ( (TextBox)controls[i] ).Text;
				i++;
				if ( controls[i] != null && controls[i] is HtmlGenericControl &&
					controls[i].Controls.Count > 0 && controls[i].Controls[0] is CheckBox )
					values["displayDiff"].Value = ( (CheckBox)controls[i].Controls[0] ).Checked.ToString();
			}

			return values;
		}
	}
}