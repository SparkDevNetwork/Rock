using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Rock;

namespace com.centralaz.Utility.MSSearch
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute( "code" )]
    [System.Web.Services.WebServiceBindingAttribute( Name = "QueryServiceSoap", Namespace = "http://microsoft.com/webservices/OfficeServer/QueryService" )]
    public partial class MSQueryService : System.Web.Services.Protocols.SoapHttpClientProtocol
    {
        private System.Threading.SendOrPostCallback QueryOperationCompleted;

        private System.Threading.SendOrPostCallback QueryExOperationCompleted;

        private System.Threading.SendOrPostCallback RegistrationOperationCompleted;

        private System.Threading.SendOrPostCallback StatusOperationCompleted;

        private System.Threading.SendOrPostCallback GetPortalSearchInfoOperationCompleted;

        private System.Threading.SendOrPostCallback GetSearchMetadataOperationCompleted;

        private System.Threading.SendOrPostCallback RecordClickOperationCompleted;

        private bool useDefaultCredentialsSetExplicitly;

        /// <remarks/>
        public MSQueryService()
        {
            if ( ( this.IsLocalFileSystemWebService( this.Url ) == true ) )
            {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else
            {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                if ( ( ( ( this.IsLocalFileSystemWebService( base.Url ) == true )
                            && ( this.useDefaultCredentialsSetExplicitly == false ) )
                            && ( this.IsLocalFileSystemWebService( value ) == false ) ) )
                {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }

        public new bool UseDefaultCredentials
        {
            get
            {
                return base.UseDefaultCredentials;
            }
            set
            {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        /// <remarks/>
        public event QueryCompletedEventHandler QueryCompleted;

        /// <remarks/>
        public event QueryExCompletedEventHandler QueryExCompleted;

        /// <remarks/>
        public event RegistrationCompletedEventHandler RegistrationCompleted;

        /// <remarks/>
        public event StatusCompletedEventHandler StatusCompleted;

        /// <remarks/>
        public event GetPortalSearchInfoCompletedEventHandler GetPortalSearchInfoCompleted;

        /// <remarks/>
        public event GetSearchMetadataCompletedEventHandler GetSearchMetadataCompleted;

        /// <remarks/>
        public event RecordClickCompletedEventHandler RecordClickCompleted;

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute( "urn:Microsoft.Search/Query", RequestNamespace = "urn:Microsoft.Search", ResponseNamespace = "urn:Microsoft.Search", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped )]
        public string Query( string queryXml )
        {
            object[] results = this.Invoke( "Query", new object[] { queryXml } );
            return ( (string)( results[0] ) );
        }

        /// <remarks/>
        public void QueryAsync( string queryXml )
        {
            this.QueryAsync( queryXml, null );
        }

        /// <remarks/>
        public void QueryAsync( string queryXml, object userState )
        {
            if ( ( this.QueryOperationCompleted == null ) )
            {
                this.QueryOperationCompleted = new System.Threading.SendOrPostCallback( this.OnQueryOperationCompleted );
            }
            this.InvokeAsync( "Query", new object[] { queryXml }, this.QueryOperationCompleted, userState );
        }

        private void OnQueryOperationCompleted( object arg )
        {
            if ( ( this.QueryCompleted != null ) )
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ( (System.Web.Services.Protocols.InvokeCompletedEventArgs)( arg ) );
                this.QueryCompleted( this, new QueryCompletedEventArgs( invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState ) );
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute( "http://microsoft.com/webservices/OfficeServer/QueryService/QueryEx", RequestNamespace = "http://microsoft.com/webservices/OfficeServer/QueryService", ResponseNamespace = "http://microsoft.com/webservices/OfficeServer/QueryService", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped )]
        public System.Data.DataSet QueryEx( string queryXml )
        {
            object[] results = this.Invoke( "QueryEx", new object[] { queryXml } );
            return ( (System.Data.DataSet)( results[0] ) );
        }

        /// <remarks/>
        public void QueryExAsync( string queryXml )
        {
            this.QueryExAsync( queryXml, null );
        }

        /// <remarks/>
        public void QueryExAsync( string queryXml, object userState )
        {
            if ( ( this.QueryExOperationCompleted == null ) )
            {
                this.QueryExOperationCompleted = new System.Threading.SendOrPostCallback( this.OnQueryExOperationCompleted );
            }
            this.InvokeAsync( "QueryEx", new object[] { queryXml }, this.QueryExOperationCompleted, userState );
        }

        private void OnQueryExOperationCompleted( object arg )
        {
            if ( ( this.QueryExCompleted != null ) )
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ( (System.Web.Services.Protocols.InvokeCompletedEventArgs)( arg ) );
                this.QueryExCompleted( this, new QueryExCompletedEventArgs( invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState ) );
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute( "urn:Microsoft.Search/Registration", RequestNamespace = "urn:Microsoft.Search", ResponseNamespace = "urn:Microsoft.Search", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped )]
        public string Registration( string registrationXml )
        {
            object[] results = this.Invoke( "Registration", new object[] { registrationXml } );
            return ( (string)( results[0] ) );
        }

        /// <remarks/>
        public void RegistrationAsync( string registrationXml )
        {
            this.RegistrationAsync( registrationXml, null );
        }

        /// <remarks/>
        public void RegistrationAsync( string registrationXml, object userState )
        {
            if ( ( this.RegistrationOperationCompleted == null ) )
            {
                this.RegistrationOperationCompleted = new System.Threading.SendOrPostCallback( this.OnRegistrationOperationCompleted );
            }
            this.InvokeAsync( "Registration", new object[] { registrationXml }, this.RegistrationOperationCompleted, userState );
        }

        private void OnRegistrationOperationCompleted( object arg )
        {
            if ( ( this.RegistrationCompleted != null ) )
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ( (System.Web.Services.Protocols.InvokeCompletedEventArgs)( arg ) );
                this.RegistrationCompleted( this, new RegistrationCompletedEventArgs( invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState ) );
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute( "urn:Microsoft.Search/Status", RequestNamespace = "urn:Microsoft.Search", ResponseNamespace = "urn:Microsoft.Search", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped )]
        public string Status()
        {
            object[] results = this.Invoke( "Status", new object[0] );
            return ( (string)( results[0] ) );
        }

        /// <remarks/>
        public void StatusAsync()
        {
            this.StatusAsync( null );
        }

        /// <remarks/>
        public void StatusAsync( object userState )
        {
            if ( ( this.StatusOperationCompleted == null ) )
            {
                this.StatusOperationCompleted = new System.Threading.SendOrPostCallback( this.OnStatusOperationCompleted );
            }
            this.InvokeAsync( "Status", new object[0], this.StatusOperationCompleted, userState );
        }

        private void OnStatusOperationCompleted( object arg )
        {
            if ( ( this.StatusCompleted != null ) )
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ( (System.Web.Services.Protocols.InvokeCompletedEventArgs)( arg ) );
                this.StatusCompleted( this, new StatusCompletedEventArgs( invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState ) );
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute( "http://microsoft.com/webservices/OfficeServer/QueryService/GetPortalSearchInfo", RequestNamespace = "http://microsoft.com/webservices/OfficeServer/QueryService", ResponseNamespace = "http://microsoft.com/webservices/OfficeServer/QueryService", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped )]
        public string GetPortalSearchInfo()
        {
            object[] results = this.Invoke( "GetPortalSearchInfo", new object[0] );
            return ( (string)( results[0] ) );
        }

        /// <remarks/>
        public void GetPortalSearchInfoAsync()
        {
            this.GetPortalSearchInfoAsync( null );
        }

        /// <remarks/>
        public void GetPortalSearchInfoAsync( object userState )
        {
            if ( ( this.GetPortalSearchInfoOperationCompleted == null ) )
            {
                this.GetPortalSearchInfoOperationCompleted = new System.Threading.SendOrPostCallback( this.OnGetPortalSearchInfoOperationCompleted );
            }
            this.InvokeAsync( "GetPortalSearchInfo", new object[0], this.GetPortalSearchInfoOperationCompleted, userState );
        }

        private void OnGetPortalSearchInfoOperationCompleted( object arg )
        {
            if ( ( this.GetPortalSearchInfoCompleted != null ) )
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ( (System.Web.Services.Protocols.InvokeCompletedEventArgs)( arg ) );
                this.GetPortalSearchInfoCompleted( this, new GetPortalSearchInfoCompletedEventArgs( invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState ) );
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute( "http://microsoft.com/webservices/OfficeServer/QueryService/GetSearchMetadata", RequestNamespace = "http://microsoft.com/webservices/OfficeServer/QueryService", ResponseNamespace = "http://microsoft.com/webservices/OfficeServer/QueryService", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped )]
        public System.Data.DataSet GetSearchMetadata()
        {
            object[] results = this.Invoke( "GetSearchMetadata", new object[0] );
            return ( (System.Data.DataSet)( results[0] ) );
        }

        /// <remarks/>
        public void GetSearchMetadataAsync()
        {
            this.GetSearchMetadataAsync( null );
        }

        /// <remarks/>
        public void GetSearchMetadataAsync( object userState )
        {
            if ( ( this.GetSearchMetadataOperationCompleted == null ) )
            {
                this.GetSearchMetadataOperationCompleted = new System.Threading.SendOrPostCallback( this.OnGetSearchMetadataOperationCompleted );
            }
            this.InvokeAsync( "GetSearchMetadata", new object[0], this.GetSearchMetadataOperationCompleted, userState );
        }

        private void OnGetSearchMetadataOperationCompleted( object arg )
        {
            if ( ( this.GetSearchMetadataCompleted != null ) )
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ( (System.Web.Services.Protocols.InvokeCompletedEventArgs)( arg ) );
                this.GetSearchMetadataCompleted( this, new GetSearchMetadataCompletedEventArgs( invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState ) );
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute( "urn:Microsoft.Search/RecordClick", RequestNamespace = "urn:Microsoft.Search", ResponseNamespace = "urn:Microsoft.Search", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped )]
        public void RecordClick( string clickInfoXml )
        {
            this.Invoke( "RecordClick", new object[] { clickInfoXml } );
        }

        /// <remarks/>
        public void RecordClickAsync( string clickInfoXml )
        {
            this.RecordClickAsync( clickInfoXml, null );
        }

        /// <remarks/>
        public void RecordClickAsync( string clickInfoXml, object userState )
        {
            if ( ( this.RecordClickOperationCompleted == null ) )
            {
                this.RecordClickOperationCompleted = new System.Threading.SendOrPostCallback( this.OnRecordClickOperationCompleted );
            }
            this.InvokeAsync( "RecordClick", new object[] {
                        clickInfoXml}, this.RecordClickOperationCompleted, userState );
        }

        private void OnRecordClickOperationCompleted( object arg )
        {
            if ( ( this.RecordClickCompleted != null ) )
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ( (System.Web.Services.Protocols.InvokeCompletedEventArgs)( arg ) );
                this.RecordClickCompleted( this, new System.ComponentModel.AsyncCompletedEventArgs( invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState ) );
            }
        }

        /// <remarks/>
        public new void CancelAsync( object userState )
        {
            base.CancelAsync( userState );
        }

        private bool IsLocalFileSystemWebService( string url )
        {
            if ( ( ( url == null )
                        || ( url == string.Empty ) ) )
            {
                return false;
            }
            System.Uri wsUri = new System.Uri( url );
            if ( ( ( wsUri.Port >= 1024 )
                        && ( string.Compare( wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase ) == 0 ) ) )
            {
                return true;
            }
            return false;
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    public delegate void QueryCompletedEventHandler( object sender, QueryCompletedEventArgs e );

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute( "code" )]
    public partial class QueryCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal QueryCompletedEventArgs( object[] results, System.Exception exception, bool cancelled, object userState ) :
            base( exception, cancelled, userState )
        {
            this.results = results;
        }

        /// <remarks/>
        public string Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ( (string)( this.results[0] ) );
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    public delegate void QueryExCompletedEventHandler( object sender, QueryExCompletedEventArgs e );

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute( "code" )]
    public partial class QueryExCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal QueryExCompletedEventArgs( object[] results, System.Exception exception, bool cancelled, object userState ) :
            base( exception, cancelled, userState )
        {
            this.results = results;
        }

        /// <remarks/>
        public System.Data.DataSet Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ( (System.Data.DataSet)( this.results[0] ) );
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    public delegate void RegistrationCompletedEventHandler( object sender, RegistrationCompletedEventArgs e );

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute( "code" )]
    public partial class RegistrationCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal RegistrationCompletedEventArgs( object[] results, System.Exception exception, bool cancelled, object userState ) :
            base( exception, cancelled, userState )
        {
            this.results = results;
        }

        /// <remarks/>
        public string Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ( (string)( this.results[0] ) );
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    public delegate void StatusCompletedEventHandler( object sender, StatusCompletedEventArgs e );

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute( "code" )]
    public partial class StatusCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {
        private object[] results;

        internal StatusCompletedEventArgs( object[] results, System.Exception exception, bool cancelled, object userState ) :
            base( exception, cancelled, userState )
        {
            this.results = results;
        }

        /// <remarks/>
        public string Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ( (string)( this.results[0] ) );
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    public delegate void GetPortalSearchInfoCompletedEventHandler( object sender, GetPortalSearchInfoCompletedEventArgs e );

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute( "code" )]
    public partial class GetPortalSearchInfoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetPortalSearchInfoCompletedEventArgs( object[] results, System.Exception exception, bool cancelled, object userState ) :
            base( exception, cancelled, userState )
        {
            this.results = results;
        }

        /// <remarks/>
        public string Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ( (string)( this.results[0] ) );
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    public delegate void GetSearchMetadataCompletedEventHandler( object sender, GetSearchMetadataCompletedEventArgs e );

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute( "code" )]
    public partial class GetSearchMetadataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetSearchMetadataCompletedEventArgs( object[] results, System.Exception exception, bool cancelled, object userState ) :
            base( exception, cancelled, userState )
        {
            this.results = results;
        }

        /// <remarks/>
        public System.Data.DataSet Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ( (System.Data.DataSet)( this.results[0] ) );
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute( "System.Web.Services", "4.0.30319.1" )]
    public delegate void RecordClickCompletedEventHandler( object sender, System.ComponentModel.AsyncCompletedEventArgs e );
}
