﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Rock;

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

                    foreach ( Type type in assembly.GetTypes().OfType<Type>().OrderBy( a => a.FullName ) )
                    {
                        if ( type.Namespace != null && !type.Namespace.StartsWith( "Rock.Data" ) && !type.IsAbstract && type.GetCustomAttribute<NotMappedAttribute>() == null )
                        {
                            foreach ( Type interfaceType in type.GetInterfaces() )
                            {
                                if ( interfaceType == entityInterface )
                                {
                                    cblModels.Items.Add( type );
                                }
                            }
                        }
                    }
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
            string serviceFolder = Path.Combine( RootFolder().FullName, "Rock" );
            string restFolder = Path.Combine( RootFolder().FullName, "Rock.Rest" );
            string dataViewFolder = Path.Combine( RootFolder().FullName, "Rock" );
            string rockClientFolder = Path.Combine( RootFolder().FullName, "Rock.Client" );

            if ( cbService.Checked  )
            {
                fbdServiceOutput.SelectedPath = serviceFolder;
                if ( fbdServiceOutput.ShowDialog() == DialogResult.OK )
                {
                    serviceFolder = fbdServiceOutput.SelectedPath;
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
                            WriteServiceFile( serviceFolder, type );
                        }

                        if ( cbRest.Checked )
                        {
                            WriteRESTFile( restFolder, type );
                        }

                        if ( cbClient.Checked )
                        {
                            WriteRockClientFile( rockClientFolder, type );
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

            var properties = GetEntityProperties( type );

            var sb = new StringBuilder();

            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <copyright>" );
            sb.AppendLine( "// Copyright 2013 by the Spark Development Network" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Licensed under the Apache License, Version 2.0 (the \"License\");" );
            sb.AppendLine( "// you may not use this file except in compliance with the License." );
            sb.AppendLine( "// You may obtain a copy of the License at" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// http://www.apache.org/licenses/LICENSE-2.0" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Unless required by applicable law or agreed to in writing, software" );
            sb.AppendLine( "// distributed under the License is distributed on an \"AS IS\" BASIS," );
            sb.AppendLine( "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied." );
            sb.AppendLine( "// See the License for the specific language governing permissions and" );
            sb.AppendLine( "// limitations under the License." );
            sb.AppendLine( "// </copyright>" );
            sb.AppendLine( "//" );

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
            sb.AppendFormat( "    public partial class {0}Service : Service<{0}>" + Environment.NewLine, type.Name );
            sb.AppendLine( "    {" );

            sb.AppendLine( "        /// <summary>" );
            sb.AppendFormat( "        /// Initializes a new instance of the <see cref=\"{0}Service\"/> class" + Environment.NewLine, type.Name );
            sb.AppendLine( "        /// </summary>" );
            sb.AppendLine( "        /// <param name=\"context\">The context.</param>");
            sb.AppendFormat( "        public {0}Service(RockContext context) : base(context)" + Environment.NewLine, type.Name );
            sb.AppendLine( "        {" );
            sb.AppendLine( "        }" );

            sb.Append( GetCanDeleteCode( rootFolder, type ) );

            sb.AppendLine( "    }" );

            sb.AppendFormat( @"
    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class {0}ExtensionMethods
    {{
        /// <summary>
        /// Clones this {0} object to a new {0} object
        /// </summary>
        /// <param name=""source"">The source.</param>
        /// <param name=""deepCopy"">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static {0} Clone( this {0} source, bool deepCopy )
        {{
            if (deepCopy)
            {{
                return source.Clone() as {0};
            }}
            else
            {{
                var target = new {0}();
                target.CopyPropertiesFrom( source );
                return target;
            }}
        }}
", type.Name );

            sb.AppendFormat( @"
        /// <summary>
        /// Copies the properties from another {0} object to this {0} object
        /// </summary>
        /// <param name=""target"">The target.</param>
        /// <param name=""source"">The source.</param>
        public static void CopyPropertiesFrom( this {0} target, {0} source )
        {{
", type.Name);

            foreach ( var property in properties )
            {
                sb.AppendFormat( "            target.{0} = source.{0};" + Environment.NewLine, property.Key );
            }

            sb.Append( @"
        }
    }
" );
            sb.AppendLine( "}" );

            var file = new FileInfo( Path.Combine( NamespaceFolder( rootFolder, type.Namespace ).FullName, "CodeGenerated", type.Name + "Service.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// 
        /// </summary>
        private class TableColumnInfo
        {
            public string Table { get; set; }
            public string Column { get; set; }
            public bool Ignore { get; set; }
        }

        /// <summary>
        /// Gets the can delete code.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private string GetCanDeleteCode( string rootFolder, Type type )
        {
            var di = new DirectoryInfo( rootFolder );
            var file = new FileInfo( Path.Combine( di.Parent.FullName, @"RockWeb\web.ConnectionStrings.config" ) );
            if ( !file.Exists )
                return string.Empty;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load( file.FullName );
            XmlNode root = xmlDoc.DocumentElement;
            XmlNode node = root.SelectNodes( "add[@name = \"RockContext\"]" )[0];
            SqlConnection sqlconn = new SqlConnection( node.Attributes["connectionString"].Value );
            sqlconn.Open();

            string sql = @"
select * from
(
select 
  OBJECT_NAME([fk].[parent_object_id]) [parentTable], 
  OBJECT_NAME([fk].[referenced_object_id]) [refTable], 
  [cc].[name] [columnName],
  [fk].[delete_referential_action] [CascadeAction]
from 
sys.foreign_key_columns [fkc]
join sys.foreign_keys [fk]
on fkc.constraint_object_id = fk.object_id
join sys.columns cc
on fkc.parent_column_id = cc.column_id
where cc.object_id = fk.parent_object_id
and [fk].[delete_referential_action_desc] != 'CASCADE'
) sub
where [refTable] = '{0}'
order by [parentTable], [columnName] 
";

            SqlCommand sqlCommand = sqlconn.CreateCommand();
            TableAttribute tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if ( tableAttribute == null )
            {
                // not a real table
                return string.Empty;
            }

            sqlCommand.CommandText = string.Format( sql, tableAttribute.Name );

            var reader = sqlCommand.ExecuteReader();

            List<TableColumnInfo> parentTableColumnNameList = new List<TableColumnInfo>();
            while ( reader.Read() )
            {
                string parentTable = reader["parentTable"] as string;
                string columnName = reader["columnName"] as string;
                bool ignoreCanDelete = false;

                Type parentEntityType = Type.GetType( string.Format( "Rock.Model.{0}, {1}", parentTable, type.Assembly.FullName ));
                if (parentEntityType != null)
                {
                    PropertyInfo columnProp = parentEntityType.GetProperty( columnName );
                    if (columnProp != null)
                    {
                        if (columnProp.GetCustomAttribute<Rock.Data.IgnoreCanDelete>() != null)
                        {
                            ignoreCanDelete = true;
                        }
                    }
                }

                parentTableColumnNameList.Add( new TableColumnInfo { Table = parentTable, Column = columnName, Ignore = ignoreCanDelete } );
            }

            // detect associative table where more than one key is referencing the same table.  EF will automatically take care of it on the DELETE
            List<string> parentTablesToIgnore = parentTableColumnNameList.GroupBy( a => a.Table ).Where( g => g.Count() > 1 ).Select( s => s.Key ).ToList();

            string canDeleteBegin = string.Format( @"
        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name=""item"">The item.</param>
        /// <param name=""errorMessage"">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( {0} item, out string errorMessage )
        {{
            errorMessage = string.Empty;
", type.Name );

            string canDeleteMiddle = string.Empty;

            foreach ( var item in parentTableColumnNameList )
            {
                // Ignore custom tables
                if ( item.Table.StartsWith( "_" ) )
                {
                    continue;
                }

                if ( parentTablesToIgnore.Contains( item.Table ) || item.Ignore)
                {
                    canDeleteMiddle += string.Format(
@"            
            // ignoring {0},{1} 
", item.Table, item.Column);
                    continue;
                }

                string parentTable = item.Table;
                string columnName = item.Column;

                canDeleteMiddle += string.Format(
@" 
            if ( new Service<{0}>( Context ).Queryable().Any( a => a.{1} == item.Id ) )
            {{
                errorMessage = string.Format( ""This {{0}} is assigned to a {{1}}."", {2}.FriendlyTypeName, {0}.FriendlyTypeName );
                return false;
            }}  
",
                    parentTable,
                    columnName,
                    type.Name 
                    );
            }


            string canDeleteEnd = @"            return true;
        }
";


            return canDeleteBegin + canDeleteMiddle + canDeleteEnd;

        }

        /// <summary>
        /// Writes the REST file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteRESTFile( string rootFolder, Type type )
        {
            string pluralizedName = type.Name.Pluralize();

            string baseName = new DirectoryInfo( rootFolder ).Name;
            if ( baseName.EndsWith( ".Rest", StringComparison.OrdinalIgnoreCase ) )
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

            restNamespace = restNamespace.Replace( ".Model", ".Controllers" );

            var sb = new StringBuilder();

            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <copyright>" );
            sb.AppendLine( "// Copyright 2013 by the Spark Development Network" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Licensed under the Apache License, Version 2.0 (the \"License\");" );
            sb.AppendLine( "// you may not use this file except in compliance with the License." );
            sb.AppendLine( "// You may obtain a copy of the License at" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// http://www.apache.org/licenses/LICENSE-2.0" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Unless required by applicable law or agreed to in writing, software" );
            sb.AppendLine( "// distributed under the License is distributed on an \"AS IS\" BASIS," );
            sb.AppendLine( "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied." );
            sb.AppendLine( "// See the License for the specific language governing permissions and" );
            sb.AppendLine( "// limitations under the License." );
            sb.AppendLine( "// </copyright>" );
            sb.AppendLine( "//" );
            sb.AppendLine( "" );

            sb.AppendFormat( "using {0};" + Environment.NewLine, type.Namespace );
            sb.AppendLine( "" );

            sb.AppendFormat( "namespace {0}" + Environment.NewLine, restNamespace );
            sb.AppendLine( "{" );
            sb.AppendLine( "    /// <summary>" );
            sb.AppendFormat( "    /// {0} REST API" + Environment.NewLine, pluralizedName );
            sb.AppendLine( "    /// </summary>" );
            sb.AppendFormat( "    public partial class {0}Controller : Rock.Rest.ApiController<{1}.{2}>" + Environment.NewLine, pluralizedName, type.Namespace, type.Name );
            sb.AppendLine( "    {" );
            sb.AppendFormat( "        public {0}Controller() : base( new {1}.{2}Service( new Rock.Data.RockContext() ) ) {{ }} " + Environment.NewLine, pluralizedName, type.Namespace, type.Name );
            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );


            var file = new FileInfo( Path.Combine( NamespaceFolder( rootFolder, restNamespace ).FullName, "CodeGenerated", pluralizedName + "Controller.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Writes the Service file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
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
                    return GetKeyName( propertyType.GetGenericArguments()[0] ) + "?";
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
                return GetKeyName( propertyType );
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
                case "DbGeography": return "object";
            }

            return typeName;
        }

        private string GetKeyName( Type type )
        {
            if ( type.IsEnum )
            {
                return GetKeyName( "Int32" ) + " /* " + type.Name + "*/";
            }
            else
            {
                return GetKeyName( type.Name );
            }
        }

        private Dictionary<Type, Dictionary<string, string>> _entityProperties { get; set; }

        private Dictionary<string, string> GetEntityProperties( Type type )
        {
            _entityProperties = _entityProperties ?? new Dictionary<Type, Dictionary<string, string>>();
            if (_entityProperties.ContainsKey(type))
            {
                return _entityProperties[type];
            }
            
            var properties = new Dictionary<string, string>();

            var interfaces = type.GetInterfaces();

            foreach ( var property in type.GetProperties() )
            {
                var getMethod = property.GetGetMethod();
                if ( getMethod.IsVirtual )
                {
                    if ( !getMethod.IsFinal )
                    {
                        continue;
                    }

                    bool interfaceProperty = false;
                    foreach ( Type interfaceType in interfaces )
                    {
                        if ( interfaceType.GetProperties().Any( p => p.Name == property.Name ) )
                        {
                            interfaceProperty = true;
                            break;
                        }
                    }
                    if ( !interfaceProperty )
                    {
                        continue;
                    }
                }

                if ( !property.GetCustomAttributes( typeof( DatabaseGeneratedAttribute ) ).Any() )
                {
                    if ( property.SetMethod != null && property.SetMethod.IsPublic  && property.GetMethod.IsPublic)
                    {
                        properties.Add( property.Name, PropertyTypeName( property.PropertyType ) );
                    }
                }
            }

            _entityProperties[type] = properties;

            return properties;
        }

        /// <summary>
        /// Writes the DTO file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteRockClientFile( string rootFolder, Type type )
        {
            string lcName = type.Name.Substring( 0, 1 ).ToLower() + type.Name.Substring( 1 );

            var properties = GetEntityProperties( type );//.Where( p => p.Key != "Id" && p.Key != "Guid" );

            bool isSecured = typeof( Rock.Security.ISecured ).IsAssignableFrom( type );

            var sb = new StringBuilder();

            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <auto-generated>" );
            sb.AppendLine( "//     This code was generated by the Rock.CodeGeneration project" );
            sb.AppendLine( "//     Changes to this file will be lost when the code is regenerated." );
            sb.AppendLine( "// </auto-generated>" );
            sb.AppendLine( "//------------------------------------------------------------------------------" );
            sb.AppendLine( "// <copyright>" );
            sb.AppendLine( "// Copyright 2013 by the Spark Development Network" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Licensed under the Apache License, Version 2.0 (the \"License\");" );
            sb.AppendLine( "// you may not use this file except in compliance with the License." );
            sb.AppendLine( "// You may obtain a copy of the License at" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// http://www.apache.org/licenses/LICENSE-2.0" );
            sb.AppendLine( "//" );
            sb.AppendLine( "// Unless required by applicable law or agreed to in writing, software" );
            sb.AppendLine( "// distributed under the License is distributed on an \"AS IS\" BASIS," );
            sb.AppendLine( "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied." );
            sb.AppendLine( "// See the License for the specific language governing permissions and" );
            sb.AppendLine( "// limitations under the License." );
            sb.AppendLine( "// </copyright>" );
            sb.AppendLine( "//" );
            sb.AppendLine( "using System;" );
            sb.AppendLine( "" );
            sb.AppendLine( "" );

            sb.AppendFormat( "namespace Rock.Client" + Environment.NewLine, type.Namespace );
            sb.AppendLine( "{" );
            sb.AppendLine( "    /// <summary>" );
            sb.AppendFormat( "    /// Simple Client Model for {0}" + Environment.NewLine, type.Name );
            sb.AppendLine( "    /// </summary>" );
            
                sb.AppendFormat( "    public partial class {0}" + Environment.NewLine, type.Name );
            

            sb.AppendLine( "    {" );

            foreach ( var property in properties )
            {
                
                    sb.AppendLine( "        /// <summary />" );
                    sb.AppendFormat( "        public {0} {1} {{ get; set; }}" + Environment.NewLine, property.Value, property.Key );
                    sb.AppendLine( "" );
            }

            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );

            //var file = new FileInfo( Path.Combine( NamespaceFolder( rootFolder, type.Namespace ).FullName, "CodeGenerated", type.Name + "Dto.cs" ) );
            var file = new FileInfo( Path.Combine( rootFolder , "CodeGenerated", type.Name + ".cs" ) );
            WriteFile( file, sb );
        }

    }
}
