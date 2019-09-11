// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Humanizer;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.ValueFilter"/> control for editing a simple filter
    /// </summary>
    [ToolboxData( "<{0}:ValueFilter runat=server></{0}:ValueFilter>" )]
    public class ValueFilter : CompositeControl, IRockControl
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get
            {
                return ViewState["Label"] as string ?? string.Empty;
            }

            set
            {
                ViewState["Label"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private HiddenField _hfData;
        private CustomValidator _customValidator;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return ( string ) ViewState["ValidationGroup"];
            }
            set
            {
                ViewState["ValidationGroup"] = value;

                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ValidationGroup = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the filter mode selection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the filter mode selection should be hidden; otherwise, <c>false</c>.
        /// </value>
        public bool HideFilterMode
        {
            get
            {
                return ( bool? ) ViewState["HideFilterMode"] ?? false;
            }
            set
            {
                ViewState["HideFilterMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the supported comparison types.
        /// </summary>
        /// <value>
        /// The supported comparison types.
        /// </value>
        public Model.ComparisonType ComparisonTypes
        {
            get
            {
                return ( Model.ComparisonType? ) ViewState["ComparisonTypes"] ?? ( Reporting.ComparisonHelper.StringFilterComparisonTypes | Model.ComparisonType.RegularExpression );
            }
            set
            {
                ViewState["ComparisonTypes"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public CompoundFilterExpression Filter
        {
            get
            {
                EnsureChildControls();

                return ( CompoundFilterExpression ) FilterExpression.FromJsonOrNull( _hfData.Value ) ?? new CompoundFilterExpression();
            }
            set
            {
                EnsureChildControls();

                _hfData.Value = value.ToJson();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFilter"/> class.
        /// </summary>
        public ValueFilter()
            : base()
        {
            RockControlHelper.Init( this );
            RequiredFieldValidator = null;
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfData = new HiddenField
            {
                ID = $"{ this.ID }_hfData",
            };
            Controls.Add( _hfData );

            _customValidator = new CustomValidator
            {
                ID = ID + "_cfv",
                CssClass = "validation-error help-inline js-filtered-text-validator",
                ClientValidationFunction = "Rock.controls.valueFilter.clientValidate",
                ErrorMessage = RequiredErrorMessage,
                Enabled = true,
                Display = ValidatorDisplay.Dynamic,
                ValidationGroup = ValidationGroup
            };
            Controls.Add( _customValidator );
        }

        /// <summary>
        /// Called just before rendering begins on the page.
        /// </summary>
        /// <param name="e">The EventArgs that describe this event.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            RegisterStartupScript();
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                {
                    _hfData.RenderControl( writer );
                }
                writer.RenderEndTag();

                _customValidator.RenderControl( writer );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        private void RegisterStartupScript()
        {
            string errorMessage;

            if ( !string.IsNullOrWhiteSpace( RequiredErrorMessage ) )
            {
                errorMessage = RequiredErrorMessage;
            }
            else if ( !string.IsNullOrWhiteSpace( Label ) )
            {
                errorMessage = Label + " Is Required";
            }
            else
            {
                errorMessage = "Filter Field Is Required";
            }

            var comparisionTypeList = ComparisonTypes.GetFlags<Model.ComparisonType>().OrderBy( v => v )
                .Select( v => new
                {
                    Value = ( int ) v,
                    Text = v.Humanize( LetterCasing.Title )
                } )
                .ToList();

            var script = string.Format(
@"
Rock.controls.valueFilter.initialize({{
    controlId: '{0}',
    required: {1},
    requiredMessage: '{2}',
    btnToggleOnClass: '{3}',
    btnToggleOffClass: '{4}',
    hideFilterMode: {5},
    comparisonTypes: {6}
}});
",
                this.ClientID, // {0}
                this.Required.ToString().ToLower(), // {1}
                errorMessage.Replace( "'", "\\'" ), // {2}
                "btn-info", // {3}
                "btn-default", // {4}
                this.HideFilterMode.ToString().ToLower(), // {5}
                comparisionTypeList.ToJson() // {6}
                );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "ValueFilterInitialization_" + this.ClientID, script, true );
        }

        #endregion
    }

    /// <summary>
    /// Defines the base filter expression class that concrete filter expressions
    /// must inherit from. These define the ability for a user to create a filter
    /// and then evaluate an object against that filter.
    /// </summary>
    public abstract class FilterExpression
    {
        /// <summary>
        /// Gets the expression that will be used to evaluate this filter.
        /// </summary>
        /// <param name="target">The target object whose property will be evaluated.</param>
        /// <param name="propertyName">Name of the property on the target to be evaluated.</param>
        /// <returns>A LINQ Expression that will evaluate this comparison.</returns>
        public virtual Expression GetExpression( object target, string propertyName )
        {
            var property = Expression.Property( Expression.Constant( target ), propertyName );

            return GetExpression( property );
        }

        /// <summary>
        /// Gets the expression that will be used to evaluate this filter.
        /// </summary>
        /// <param name="property">The property that contains the value to be compared.</param>
        /// <returns>A LINQ Expression that will evaluate this comparison.</returns>
        public abstract Expression GetExpression( MemberExpression property );

        /// <summary>
        /// Evaluates the filter against the target's property.
        /// </summary>
        /// <param name="target">The target object whose property will be evaluated.</param>
        /// <param name="propertyName">Name of the property on the target to be evaluated.</param>
        /// <returns><c>true</c> if the evaluation is truthful, <c>false</c> otherwise.</returns>
        public virtual bool Evaluate( object target, string propertyName )
        {
            var expression = GetExpression( target, propertyName );
            var expressionFunc = Expression.Lambda<Func<bool>>( expression ).Compile();

            return expressionFunc();
        }

        /// <summary>
        /// Evaluates the filter against a property value.
        /// </summary>
        /// <param name="property">The property that contains the value to be compared.</param>
        /// <returns><c>true</c> if the evaluation is truthful, <c>false</c> otherwise.</returns>
        public virtual bool Evaluate( MemberExpression property )
        {
            var expression = GetExpression( property );
            var expressionFunc = Expression.Lambda<Func<bool>>( expression ).Compile();

            return expressionFunc();
        }

        /// <summary>
        /// Creates a FilterExpression from the given JSON data.
        /// </summary>
        /// <param name="value">The JSON string value.</param>
        /// <returns>A FilterExpression object or null if the data was invalid.</returns>
        public static FilterExpression FromJsonOrNull( string value )
        {
            try
            {
                return FromJObject( Newtonsoft.Json.Linq.JObject.Parse( value ) );
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a FilterExpression from the given JSON object.
        /// </summary>
        /// <param name="jobject">The JSON object.</param>
        /// <returns>A FilterExpression object.</returns>
        public static FilterExpression FromJObject( Newtonsoft.Json.Linq.JObject jobject )
        {
            if ( jobject["Filters"] != null )
            {
                return new CompoundFilterExpression( jobject );
            }
            else
            {
                return new ComparisonFilterExpression( jobject );
            }
        }
    }

    /// <summary>
    /// Defines the information required to build a compound LINQ expression from
    /// multiple filters.
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.FilterExpression" />
    public class CompoundFilterExpression : FilterExpression
    {
        #region Properties

        /// <summary>
        /// Gets or sets the type of the expression grouping.
        /// </summary>
        /// <value>
        /// The type of the expression grouping.
        /// </value>
        public Model.FilterExpressionType ExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the child expressions.
        /// </summary>
        /// <value>
        /// The child expressions.
        /// </value>
        public List<FilterExpression> Filters { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundFilterExpression"/> class.
        /// </summary>
        public CompoundFilterExpression()
        {
            ExpressionType = Model.FilterExpressionType.GroupAny;
            Filters = new List<FilterExpression>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundFilterExpression"/> class.
        /// </summary>
        /// <param name="jobject">The JSON object that contains the data.</param>
        public CompoundFilterExpression( Newtonsoft.Json.Linq.JObject jobject )
            : this()
        {
            ExpressionType = ( Model.FilterExpressionType ) jobject.Value<int>( "ExpressionType" );
            foreach ( Newtonsoft.Json.Linq.JObject filter in jobject.Value<Newtonsoft.Json.Linq.JArray>( "Filters" ) )
            {
                Filters.Add( FilterExpression.FromJObject( filter ) );
            }
            
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the expression that will be used to evaluate this comparison.
        /// </summary>
        /// <param name="property">The property that contains the value to be compared.</param>
        /// <returns>A LINQ Expression that will evaluate this comparison.</returns>
        public override Expression GetExpression( MemberExpression property )
        {
            Expression expression = null;

            if ( Filters.Count == 0 )
            {
                return Expression.Constant( true );
            }

            switch ( ExpressionType )
            {
                case Model.FilterExpressionType.GroupAll:
                case Model.FilterExpressionType.GroupAnyFalse:
                    foreach ( var comparison in Filters )
                    {
                        var expr = comparison.GetExpression( property );

                        if ( expression == null )
                        {
                            expression = expr;
                        }
                        else
                        {
                            expression = Expression.AndAlso( expression, expr );
                        }
                    }

                    if ( ExpressionType == Model.FilterExpressionType.GroupAnyFalse )
                    {
                        //
                        // If only one of the conditions must be false, invert the expression so
                        // that it becomes the logical equivalent of "NOT ALL".
                        //
                        expression = Expression.Not( expression );
                    }

                    return expression;

                case Model.FilterExpressionType.GroupAny:
                case Model.FilterExpressionType.GroupAllFalse:
                    foreach ( var comparison in Filters )
                    {
                        var expr = comparison.GetExpression( property );

                        if ( expression == null )
                        {
                            expression = expr;
                        }
                        else
                        {
                            expression = Expression.OrElse( expression, expr );
                        }
                    }

                    if ( ExpressionType == Model.FilterExpressionType.GroupAllFalse )
                    {
                        //
                        // If all of the conditions must be false, invert the expression so
                        // that it becomes the logical equivalent of "NOT ANY".
                        //
                        expression = Expression.Not( expression );
                    }

                    return expression;

                default:
                    throw new Exception( $"Unknown expression type { ExpressionType }" );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string operationWord;
            string prefixWord = string.Empty;

            switch ( ExpressionType )
            {
                case Model.FilterExpressionType.GroupAll:
                    operationWord = " And ";
                    break;

                case Model.FilterExpressionType.GroupAny:
                    operationWord = " Or ";
                    break;

                default:
                    operationWord = " ?? ";
                    break;
            }

            var text = string.Join( operationWord, Filters.Select( f => f.ToString() ) );

            if ( ExpressionType == Model.FilterExpressionType.GroupAllFalse || ExpressionType == Model.FilterExpressionType.GroupAnyFalse )
            {
                text = $"Not ({ text })";
            }

            return text;
        }

        #endregion
    }

    /// <summary>
    /// Defines the information required to build a LINQ expression that will compare values.
    /// </summary>
    public class ComparisonFilterExpression : FilterExpression
    {
        #region Properties

        /// <summary>
        /// Gets or sets the value of this expression.
        /// </summary>
        /// <value>
        /// The value of this expression.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the second value of this expression.
        /// </summary>
        /// <value>
        /// The second value of this expression.
        /// </value>
        public string Value2 { get; set; }

        /// <summary>
        /// Gets or sets the comparision operation to use when building the expression.
        /// </summary>
        /// <value>
        /// The comparison operation to use when building the expression.
        /// </value>
        public Model.ComparisonType Comparison { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonFilterExpression"/> class.
        /// </summary>
        public ComparisonFilterExpression()
        {
            Value = string.Empty;
            Comparison = Model.ComparisonType.Contains;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonFilterExpression"/> class.
        /// </summary>
        /// <param name="jobject">The JSON object that contains the data.</param>
        public ComparisonFilterExpression( Newtonsoft.Json.Linq.JObject jobject )
            : this()
        {
            Value = jobject.Value<string>( "Value" );
            Comparison = ( Model.ComparisonType ) jobject.Value<int>( "Comparison" );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the expression that will be used to evaluate this comparison.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>A LINQ Expression that will evaluate this comparison.</returns>
        public override Expression GetExpression( MemberExpression property )
        {
            object value = Value.ToLower();
            object value2 = !string.IsNullOrEmpty( Value2 ) ? Value2.ToLower() : null;

            if ( property.Type != typeof( string ) )
            {
                value = Convert.ChangeType( value, property.Type );
                if ( value2 != null )
                {
                    value2 = Convert.ChangeType( value2, property.Type );
                }
            }

            //
            // Handle processing of Regular Expressions since they are not supported
            // by the ComparisonHelper.
            //
            if ( Comparison == Model.ComparisonType.RegularExpression )
            {
                Expression valueExpression;

                if ( property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                {
                    // if Nullable Type compare on the .Value of the property (if it HasValue)
                    valueExpression = Expression.Property( property, "Value" );
                }
                else
                {
                    valueExpression = property;
                }

                if ( valueExpression.Type == typeof( string ) )
                {
                    var miToLower = typeof( string ).GetMethod( "ToLower", new Type[] { } );
                    valueExpression = Expression.Call( valueExpression, miToLower );
                }

                var methodInfo = typeof( System.Text.RegularExpressions.Regex )
                    .GetMethod( "IsMatch", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof( string ), typeof( string ), typeof( System.Text.RegularExpressions.RegexOptions ) }, null );

                return Expression.Call( null, methodInfo, valueExpression, Expression.Constant( value ), Expression.Constant( System.Text.RegularExpressions.RegexOptions.IgnoreCase ) );
            }

            if ( property.Type == typeof( string ) )
            {
                var fakeObject = new
                {
                    Value = Expression.Lambda<Func<string>>( property ).Compile()().ToStringSafe().ToLower()
                };
                property = Expression.Property( Expression.Constant( fakeObject ), "Value" );
            }

            return Rock.Reporting.ComparisonHelper.ComparisonExpression( Comparison, property, Expression.Constant( value ), value2 != null ? Expression.Constant( value2 ) : null );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            switch ( Comparison )
            {
                case Model.ComparisonType.Between:
                    return string.Empty;

                case Model.ComparisonType.Contains:
                    return $"Contains '{ Value }'";

                case Model.ComparisonType.DoesNotContain:
                    return $"Does Not contain '{ Value }'";

                case Model.ComparisonType.EndsWith:
                    return $"Ends With '{ Value }'";

                case Model.ComparisonType.EqualTo:
                    return $"Equal To '{ Value }'";

                case Model.ComparisonType.GreaterThan:
                    return $"Greater Than '{ Value }'";

                case Model.ComparisonType.GreaterThanOrEqualTo:
                    return $"Greater Than Or Equal To '{ Value }'";

                case Model.ComparisonType.IsBlank:
                    return "Is Blank";

                case Model.ComparisonType.IsNotBlank:
                    return "Is Not Blank";

                case Model.ComparisonType.LessThan:
                    return $"Less Than '{ Value }'";

                case Model.ComparisonType.LessThanOrEqualTo:
                    return $"Less Than Or Equal To '{ Value }'";

                case Model.ComparisonType.NotEqualTo:
                    return $"Not Equal To";

                case Model.ComparisonType.RegularExpression:
                    return $"Matches Expression '{ Value }'";

                case Model.ComparisonType.StartsWith:
                    return $"Starts With '{ Value }'";

                default:
                    return $"{ Comparison.ToString() } '{ Value }'";
            }
        }

        #endregion
    }
}
