using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Rock.CodeGeneration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Form1 : Form
    {
        PluralizationService pls = PluralizationService.CreateService( new CultureInfo( "en-US" ) );
        Assembly rockAssembly = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1" /> class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnLoad control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void btnLoad_Click( object sender, EventArgs e )
        {
			var entityInterface = typeof( Rock.Data.IEntity );
			rockAssembly = entityInterface.Assembly;
			FileInfo fi = new FileInfo( ( new System.Uri( rockAssembly.CodeBase ) ).AbsolutePath );

            ofdAssembly.InitialDirectory = fi.DirectoryName;
            ofdAssembly.Filter = "dll files (*.dll)|*.dll";
            ofdAssembly.FileName = "Rock.dll";
            ofdAssembly.RestoreDirectory = true;

            if ( ofdAssembly.ShowDialog() == DialogResult.OK )
            {
                Cursor = Cursors.WaitCursor;

                cblModels.Items.Clear();

                foreach ( var file in ofdAssembly.FileNames )
                {
                    var assembly = Assembly.LoadFrom( file );

					foreach ( Type type in assembly.GetTypes() )
						if ( type.Namespace != null && !type.Namespace.StartsWith( "Rock.Data" ) )
							foreach ( Type interfaceType in type.GetInterfaces() )
								if ( interfaceType == entityInterface )
									cblModels.Items.Add( type );
				}

                CheckAllItems( true );
                cbSelectAll.Checked = true;

                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbSelectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void cbSelectAll_CheckedChanged( object sender, EventArgs e )
        {
            CheckAllItems( cbSelectAll.Checked );
        }

        /// <summary>
        /// Handles the Click event of the btnGenerate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void btnGenerate_Click( object sender, EventArgs e )
        {
            string serviceDtoFolder = Path.Combine( RootFolder().FullName, "Rock" );
            string restFolder = Path.Combine( RootFolder().FullName, "Rock.Rest" );

            if ( cbService.Checked || cbDto.Checked )
            {
                fbdServiceDtoOutput.SelectedPath = serviceDtoFolder;
                if ( fbdServiceDtoOutput.ShowDialog() == DialogResult.OK )
                {
                    serviceDtoFolder = fbdServiceDtoOutput.SelectedPath;
                }
            }

            if ( cbRest.Checked )
            {
                fbdRestOutput.SelectedPath = restFolder;
                if ( fbdRestOutput.ShowDialog() == DialogResult.OK )
                {
                    restFolder = fbdRestOutput.SelectedPath;
                }
            }

            Cursor = Cursors.WaitCursor;

            if ( cblModels.CheckedItems.Count > 0 )
            {
                var rootFolder = RootFolder();
                if ( rootFolder != null )
                {
                    foreach ( object item in cblModels.CheckedItems )
                    {
                        Type type = (Type)item;

                        if ( cbService.Checked )
                        {
                            WriteServiceFile( serviceDtoFolder, type );
                        }

                        if ( cbDto.Checked )
                        {
                            WriteDtoFile( serviceDtoFolder, type );
                        }

                        if ( cbRest.Checked )
                        {
                            WriteRESTFile( restFolder, type );
                        }
                    }
                }
            }

            Cursor = Cursors.Default;

            MessageBox.Show( "Files have been generated" );
        }

        /// <summary>
        /// Checks or unchecks all the items in the list
        /// </summary>
        /// <param name="selected"></param>
        private void CheckAllItems( bool selected )
        {
            for ( int i = 0; i < cblModels.Items.Count; i++ )
            {
                cblModels.SetItemChecked( i, selected );
            }
        }

        /// <summary>
        /// Writes the Service file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteServiceFile( string rootFolder, Type type )
        {
            string repoConstructorParam = string.Empty;
            if ( !type.Assembly.Equals( rockAssembly ) )
            {
                foreach ( var context in Rock.Reflection.SearchAssembly( type.Assembly, typeof( System.Data.Entity.DbContext ) ) )
                {
                    repoConstructorParam = string.Format( " new EFRepository<{0}>( new {1}() ) ", type.Name, context.Value.FullName );
                    break;
                }
            }

            Type dataMemberType = typeof( System.Runtime.Serialization.DataMemberAttribute );

            var properties = new Dictionary<string, string>();
            foreach ( var property in type.GetProperties() )
            {
                if ( System.Attribute.IsDefined( property, dataMemberType ) )
                {
                    properties.Add( property.Name, PropertyTypeName( property.PropertyType ) );
                }
            }

            var sb = new StringBuilder();

            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-" );
            sb.AppendLine( "// SHAREALIKE 3.0 UNPORTED LICENSE:" );
            sb.AppendLine( "// http://creativecommons.org/licenses/by-nc-sa/3.0/" );
            sb.AppendLine( "//" );
            sb.AppendLine( "" );

            sb.AppendLine( "using System;" );
            sb.AppendLine( "using System.Linq;" );
            sb.AppendLine( "" );
            sb.AppendLine( "using Rock.Data;" );
            sb.AppendLine( "" );

            sb.AppendFormat( "namespace {0}" + Environment.NewLine, type.Namespace );
            sb.AppendLine( "{" );
            sb.AppendLine( "    /// <summary>" );
            sb.AppendFormat( "    /// {0} Service class" + Environment.NewLine, type.Name );
            sb.AppendLine( "    /// </summary>" );
            sb.AppendFormat( "    public partial class {0}Service : Service<{0}, {0}Dto>" + Environment.NewLine, type.Name );
            sb.AppendLine( "    {" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendFormat( "        /// Initializes a new instance of the <see cref=\"{0}Service\"/> class" + Environment.NewLine, type.Name );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendFormat( "        public {0}Service()" + Environment.NewLine, type.Name );
            sb.AppendFormat( "            : base({0})" + Environment.NewLine, repoConstructorParam );
            sb.AppendLine( "        {" );
            sb.AppendLine( "        }" );
            sb.AppendLine( "" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendFormat( "        /// Initializes a new instance of the <see cref=\"{0}Service\"/> class" + Environment.NewLine, type.Name );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendFormat( "        public {0}Service(IRepository<{0}> repository) : base(repository)" + Environment.NewLine, type.Name );
            sb.AppendLine( "        {" );
            sb.AppendLine( "        }" );
            sb.AppendLine( "" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendLine( "        /// Creates a new model" );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendFormat( "        public override {0} CreateNew()" + Environment.NewLine, type.Name );
            sb.AppendLine( "        {" );
            sb.AppendFormat( "            return new {0}();" + Environment.NewLine, type.Name );
            sb.AppendLine( "        }" );
            sb.AppendLine( "" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendLine( "        /// Query DTO objects" );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendLine( "        /// <returns>A queryable list of DTO objects</returns>" );
            sb.AppendFormat( "        public override IQueryable<{0}Dto> QueryableDto( )" + Environment.NewLine, type.Name );
            sb.AppendLine( "        {" );
            sb.AppendLine( "            return QueryableDto( this.Queryable() );" );
            sb.AppendLine( "        }" );
            sb.AppendLine( "" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendLine( "        /// Query DTO objects" );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendLine( "        /// <returns>A queryable list of DTO objects</returns>" );
            sb.AppendFormat( "        public IQueryable<{0}Dto> QueryableDto( IQueryable<{0}> items )" + Environment.NewLine, type.Name );
            sb.AppendLine( "        {" );
            sb.AppendFormat( "            return items.Select( m => new {0}Dto()" + Environment.NewLine, type.Name );
            sb.AppendLine( "                {" );
            foreach ( var property in properties )
            {
                sb.AppendFormat( "                    {0} = m.{0}," + Environment.NewLine, property.Key );
            }
            sb.AppendLine( "                });" );
            sb.AppendLine( "        }" );

            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );

            var file = new FileInfo( Path.Combine( NamespaceFolder( rootFolder, type.Namespace ).FullName, type.Name + "Service.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Writes the DTO file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteDtoFile( string rootFolder, Type type )
        {
            Type dataMemberType = typeof( System.Runtime.Serialization.DataMemberAttribute );
            string lcName = type.Name.Substring( 0, 1 ).ToLower() + type.Name.Substring( 1 );

            var properties = new Dictionary<string, string>();
            foreach ( var property in type.GetProperties() )
                if ( System.Attribute.IsDefined( property, dataMemberType ) )
                    properties.Add( property.Name, PropertyTypeName( property.PropertyType ) );

            var sb = new StringBuilder();

            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-" );
            sb.AppendLine( "// SHAREALIKE 3.0 UNPORTED LICENSE:" );
            sb.AppendLine( "// http://creativecommons.org/licenses/by-nc-sa/3.0/" );
            sb.AppendLine( "//" );
            sb.AppendLine( "using System;" );
            sb.AppendLine( "" );

            sb.AppendLine( "using Rock.Data;" );
            sb.AppendLine( "" );

            sb.AppendFormat( "namespace {0}" + Environment.NewLine, type.Namespace );
            sb.AppendLine( "{" );
            sb.AppendLine( "    /// <summary>" );
            sb.AppendFormat( "    /// Data Transfer Object for {0} object" + Environment.NewLine, type.Name );
            sb.AppendLine( "    /// </summary>" );
            //sb.AppendLine( "    [Serializable]" );
            sb.AppendFormat( "    public partial class {0}Dto : IDto" + Environment.NewLine, type.Name );
            // : Dto<{0}>
            sb.AppendLine( "    {" );

            sb.AppendLine( "" );
            sb.AppendLine( "#pragma warning disable 1591" );
            foreach ( var property in properties )
            {
                if ( !BaseDtoProperty( property.Key ) )
                {
                    sb.AppendFormat( "        public {0} {1} {{ get; set; }}" + Environment.NewLine, property.Value, property.Key );
                }
            }
            sb.AppendLine( "#pragma warning restore 1591" );

            sb.AppendLine( "" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendLine( "        /// Instantiates a new DTO object" );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendFormat( "        public {0}Dto ()" + Environment.NewLine, type.Name );
            sb.AppendLine( "        {" );
            sb.AppendLine( "        }" );
            sb.AppendLine( "" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendLine( "        /// Instantiates a new DTO object from the entity" );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendFormat( "        /// <param name=\"{0}\"></param>" + Environment.NewLine, lcName );
            sb.AppendFormat( "        public {0}Dto ( {0} {1} )" + Environment.NewLine, type.Name, lcName );
            sb.AppendLine( "        {" );
            sb.AppendFormat( "            CopyFromModel( {0} );" + Environment.NewLine, lcName );
            sb.AppendLine( "        }" );
            sb.AppendLine( "" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendLine( "        /// Copies the model property values to the DTO properties" );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendLine( "        /// <param name=\"model\">The model.</param>");
            sb.AppendLine( "        public void CopyFromModel( IEntity model )" );
            sb.AppendLine( "        {" );
            sb.AppendFormat( "            if ( model is {0} )" + Environment.NewLine, type.Name );
            sb.AppendLine( "            {" );
            sb.AppendFormat( "                var {0} = ({1})model;" + Environment.NewLine, lcName, type.Name );
            foreach ( var property in properties )
            {
                sb.AppendFormat( "                this.{0} = {1}.{0};" + Environment.NewLine, property.Key, lcName );
            }
            sb.AppendLine( "            }" );
            sb.AppendLine( "        }" );
            sb.AppendLine( "" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendLine( "        /// Copies the DTO property values to the entity properties" );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendLine( "        /// <param name=\"model\">The model.</param>");
            sb.AppendLine( "        public void CopyToModel ( IEntity model )" );
            sb.AppendLine( "        {" );
            sb.AppendFormat( "            if ( model is {0} )" + Environment.NewLine, type.Name );
            sb.AppendLine( "            {" );
            sb.AppendFormat( "                var {0} = ({1})model;" + Environment.NewLine, lcName, type.Name );
            foreach ( var property in properties )
            {
                sb.AppendFormat( "                {1}.{0} = this.{0};" + Environment.NewLine, property.Key, lcName );
            }
            sb.AppendLine( "            }" );
            sb.AppendLine( "        }" );

            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );

            var file = new FileInfo( Path.Combine( NamespaceFolder( rootFolder, type.Namespace ).FullName, type.Name + "Dto.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Writes the REST file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteRESTFile( string rootFolder, Type type )
        {
            Type dataMemberType = typeof( System.Runtime.Serialization.DataMemberAttribute );
            string pluralizedName = pls.Pluralize( type.Name );

            string baseName = new DirectoryInfo( rootFolder ).Name;
            if ( baseName.EndsWith( ".Rest" ) )
            {
                baseName = baseName.Substring( 0, baseName.Length - 5 );
            }

            string restNamespace = type.Namespace;
            if ( restNamespace.StartsWith( baseName + ".", true, null ) )
            {
                restNamespace = baseName + ".Rest" + restNamespace.Substring( baseName.Length );
            }
            else
            {
                restNamespace = ".Rest." + restNamespace;
            }

            var properties = new Dictionary<string, string>();
            foreach ( var property in type.GetProperties() )
            {
                if ( System.Attribute.IsDefined( property, dataMemberType ) )
                {
                    properties.Add( property.Name, PropertyTypeName( property.PropertyType ) );
                }
            }

            var sb = new StringBuilder();

            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-" );
            sb.AppendLine( "// SHAREALIKE 3.0 UNPORTED LICENSE:" );
            sb.AppendLine( "// http://creativecommons.org/licenses/by-nc-sa/3.0/" );
            sb.AppendLine( "//" );
            sb.AppendLine( "" );

            sb.AppendFormat( "using {0};" + Environment.NewLine, type.Namespace );
            sb.AppendLine( "" );

            sb.AppendFormat( "namespace {0}" + Environment.NewLine, restNamespace );
            sb.AppendLine( "{" );
            sb.AppendLine( "    /// <summary>" );
            sb.AppendFormat( "    /// {0} REST API" + Environment.NewLine, pluralizedName );
            sb.AppendLine( "    /// </summary>" );
            sb.AppendFormat( "    public partial class {0}Controller : Rock.Rest.ApiController<{1}.{2}, {1}.{2}Dto>" + Environment.NewLine, pluralizedName, type.Namespace, type.Name );
            sb.AppendLine( "    {" );
            sb.AppendFormat( "        public {0}Controller() : base( new {1}.{2}Service() ) {{ }} " + Environment.NewLine, pluralizedName, type.Namespace, type.Name );
            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );


            var file = new FileInfo( Path.Combine( NamespaceFolder( rootFolder, restNamespace ).FullName, pluralizedName + "Controller.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Gets the root folder to use as the base for the namespace folders
        /// </summary>
        /// <returns></returns>
        private DirectoryInfo RootFolder()
        {
            // Should probably be read from config file, or selected from directory dialog.  
            // For now, just traverses parent folders looking for Rock.sln file
            var dirInfo = new DirectoryInfo( Assembly.GetExecutingAssembly().Location );
            while ( dirInfo != null && !File.Exists( Path.Combine( dirInfo.FullName, "Rock.sln" ) ) )
            {
                dirInfo = dirInfo.Parent;
            }
            return dirInfo;
        }

        /// <summary>
        /// Gets the namespace folder for a selected type
        /// </summary>
        private DirectoryInfo NamespaceFolder( string rootFolder, string objectNamespace )
        {
            DirectoryInfo di = new DirectoryInfo( rootFolder );

            var commonParts = new List<string>();
            while ( objectNamespace.ToLower().StartsWith( di.Name.ToLower() + "." ) )
            {
                commonParts.Add( di.Name );
                objectNamespace = objectNamespace.Substring( di.Name.Length + 1 );
                di = di.Parent;
            }

            rootFolder = di.FullName;
            foreach ( string part in commonParts )
            {
                rootFolder = Path.Combine( rootFolder, part );
            }

            return new DirectoryInfo( Path.Combine( rootFolder, objectNamespace.Replace( '.', '\\' ) ) );
        }

        /// <summary>
        /// Get the property type name.  Will handle nullable and complex generic types.
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        private string PropertyTypeName( Type propertyType )
        {
            if ( propertyType.IsGenericType )
            {
                if ( propertyType.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                {
                    return GetKeyName( propertyType.GetGenericArguments()[0].Name ) + "?";
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat( "{0}<", propertyType.Name.Split( (char)96 )[0] );

                    foreach ( var argType in propertyType.GetGenericArguments() )
                    {
                        sb.AppendFormat( "{0}, ", PropertyTypeName( argType ) );
                    }

                    sb.Remove( sb.Length - 2, 2 );
                    sb.Append( ">" );

                    return sb.ToString();
                }
            }
            else
            {
                return GetKeyName( propertyType.Name );
            }
        }

        /// <summary>
        /// Writes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="sb">The sb.</param>
        private void WriteFile( FileInfo file, StringBuilder sb )
        {
            if ( !file.Directory.Exists )
            {
                file.Directory.Create();
            }

            using ( var outputFile = new StreamWriter( file.FullName ) )
            {
                outputFile.Write( sb.ToString() );
            }
        }

        /// <summary>
        /// Replace any type names that have an equivelant C# alias
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private string GetKeyName( string typeName )
        {
            switch ( typeName )
            {
                case "Boolean": return "bool";
                case "Byte": return "byte";
                case "Char": return "char";
                case "Decimal": return "decimal";
                case "Double": return "double";
                case "Single": return "float";
                case "Int32": return "int";
                case "Int64": return "long";
                case "SByte": return "sbyte";
                case "Int16": return "short";
                case "String": return "string";
            }

            return typeName;
        }

        /// <summary>
        /// Evaluates if property is part of the base DTO class
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private bool BaseDtoProperty( string propertyName )
        {
            return false;

            if ( propertyName == "Id" ) return true;
            if ( propertyName == "Guid" ) return true;
            if ( propertyName == "CreatedDateTime" ) return true;
            if ( propertyName == "ModifiedDateTime" ) return true;
            if ( propertyName == "CreatedByPersonId" ) return true;
            if ( propertyName == "ModifiedByPersonId" ) return true;

            return false;
        }
    }
}
