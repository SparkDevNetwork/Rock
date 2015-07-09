using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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
            if ( !Directory.Exists( lblAssemblyPath.Text ) || lblAssemblyPath.Text == string.Empty )
            {
                rockAssembly = typeof( Rock.Data.IEntity ).Assembly;
                FileInfo fi = new FileInfo( ( new System.Uri( rockAssembly.CodeBase ) ).AbsolutePath );
                lblAssemblyPath.Text = fi.FullName;
                lblAssemblyDateTime.Text = fi.LastWriteTime.ToElapsedString();
                toolTip1.SetToolTip( lblAssemblyDateTime, fi.LastWriteTime.ToString() );
            }

            ofdAssembly.InitialDirectory = Path.GetDirectoryName( lblAssemblyPath.Text );
            ofdAssembly.Filter = "dll files (*.dll)|*.dll";
            ofdAssembly.FileName = "Rock.dll";
            ofdAssembly.RestoreDirectory = true;

            var projectName = Path.GetFileNameWithoutExtension( lblAssemblyPath.Text );
            tbServiceFolder.Text = Path.Combine( RootFolder().FullName, projectName );
            tbRestFolder.Text = Path.Combine( RootFolder().FullName, projectName + ".Rest" );
            tbClientFolder.Text = Path.Combine( RootFolder().FullName, projectName + ".Client" );
            if ( projectName != "Rock" )
            {
                tbRestFolder.Text = Path.Combine( RootFolder().FullName, projectName + "\\Rest" );
            }

            if ( ofdAssembly.ShowDialog() == DialogResult.OK )
            {
                Cursor = Cursors.WaitCursor;

                cblModels.Items.Clear();

                foreach ( var file in ofdAssembly.FileNames )
                {
                    lblAssemblyPath.Text = file;
                    var assembly = Assembly.LoadFrom( file );

                    foreach ( Type type in assembly.GetTypes().OfType<Type>().OrderBy( a => a.FullName ) )
                    {
                        if ( type.Namespace != null && !type.Namespace.StartsWith( "Rock.Data" ) && !type.IsAbstract && type.GetCustomAttribute<NotMappedAttribute>() == null )
                        {
                            if ( typeof( Rock.Data.IEntity ).IsAssignableFrom( type ) || type.GetCustomAttribute( typeof( TableAttribute ) ) != null )
                            {
                                cblModels.Items.Add( type );
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
            string serviceFolder = tbServiceFolder.Text;
            string restFolder = tbRestFolder.Text;
            string rockClientFolder = tbClientFolder.Text;

            Cursor = Cursors.WaitCursor;

            progressBar1.Visible = true;
            progressBar1.Maximum = cblModels.CheckedItems.Count;
            progressBar1.Value = 0;
            if ( cblModels.CheckedItems.Count > 0 )
            {
                var rootFolder = RootFolder();
                if ( rootFolder != null )
                {
                    foreach ( object item in cblModels.CheckedItems )
                    {
                        progressBar1.Value++;
                        Type type = (Type)item;

                        // only generate Service and REST for IEntity types
                        if ( typeof( Rock.Data.IEntity ).IsAssignableFrom( type ) )
                        {

                            if ( cbService.Checked )
                            {
                                WriteServiceFile( serviceFolder, type );
                            }

                            if ( cbRest.Checked )
                            {
                                WriteRESTFile( restFolder, type );
                            }
                        }

                        if ( cbClient.Checked )
                        {
                            WriteRockClientFile( rockClientFolder, type );
                        }
                    }

                    if (cbClient.Checked)
                    {
                        WriteRockClientIncludeClientFiles( rockClientFolder, cblModels.CheckedItems.OfType<Type>().ToList() );
                        WriteRockClientSystemGuidFiles( rockClientFolder );
                        WriteRockClientEnumsFile( rockClientFolder );
                    }
                }
            }

            progressBar1.Visible = false;
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
            string dbContextFullName = Rock.Reflection.GetDbContextForEntityType( type ).GetType().FullName;
            if ( dbContextFullName.StartsWith( "Rock.Data." ) )
            {
                dbContextFullName = dbContextFullName.Replace( "Rock.Data.", "" );
            }

            var properties = GetEntityProperties( type, false );

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
            sb.AppendLine( "        /// <param name=\"context\">The context.</param>" );

            sb.AppendFormat( "        public {0}Service({1} context) : base(context)" + Environment.NewLine, type.Name, dbContextFullName );
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
", type.Name );

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
            public bool IsPartOfPrimaryKey { get; set; }
            public bool Ignore { get; set; }
            public override string ToString()
            {
                return string.Format( "{0} | {1} {2} {3}", Table, Column, IsPartOfPrimaryKey ? "| PrimaryKey" : null, Ignore ? "| Ignored" : null );
            }
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
  isnull(OBJECTPROPERTY(OBJECT_ID('[' + kcu.constraint_name + ']'), 'IsPrimaryKey'), 0) [IsPrimaryKey],
  [fk].[delete_referential_action] [CascadeAction]
from 
sys.foreign_key_columns [fkc]
join sys.foreign_keys [fk]
on fkc.constraint_object_id = fk.object_id
join sys.columns cc
on fkc.parent_column_id = cc.column_id
left join INFORMATION_SCHEMA.KEY_COLUMN_USAGE [kcu]
on kcu.COLUMN_NAME = cc.Name and kcu.TABLE_NAME = OBJECT_NAME([fk].[parent_object_id]) and OBJECTPROPERTY(OBJECT_ID('[' + kcu.constraint_name + ']'), 'IsPrimaryKey') = 1
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
                bool isPrimaryKey = (int)reader["IsPrimaryKey"] == 1;
                bool ignoreCanDelete = false;

                Type parentEntityType = Type.GetType( string.Format( "Rock.Model.{0}, {1}", parentTable, type.Assembly.FullName ) );
                if ( parentEntityType != null )
                {
                    PropertyInfo columnProp = parentEntityType.GetProperty( columnName );
                    if ( columnProp != null )
                    {
                        if ( columnProp.GetCustomAttribute<Rock.Data.IgnoreCanDelete>() != null )
                        {
                            ignoreCanDelete = true;
                        }
                    }
                }

                parentTableColumnNameList.Add( new TableColumnInfo { Table = parentTable, Column = columnName, IsPartOfPrimaryKey = isPrimaryKey, Ignore = ignoreCanDelete } );
            }

            parentTableColumnNameList = parentTableColumnNameList.OrderBy( a => a.Table ).ThenBy( a => a.Column ).ToList();

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

                // detect associative table where the foreign key column is also part of the primary key.  EF will automatically take care of it on the DELETE
                if ( item.IsPartOfPrimaryKey || item.Ignore )
                {
                    canDeleteMiddle += string.Format(
@"            
            // ignoring {0},{1} 
", item.Table, item.Column );
                    continue;
                }

                string parentTable = item.Table;
                string columnName = item.Column;
                string relationShipText;
                string pluralizeCode;

                if ( columnName.StartsWith( "Parent" + type.Name ) )
                {
                    relationShipText = "contains one or more child";
                    pluralizeCode = ".Pluralize().ToLower()";
                }
                else
                {
                    relationShipText = "is assigned to a";
                    pluralizeCode = "";
                }


                canDeleteMiddle += string.Format(
@" 
            if ( new Service<{0}>( Context ).Queryable().Any( a => a.{1} == item.Id ) )
            {{
                errorMessage = string.Format( ""This {{0}} {3} {{1}}."", {2}.FriendlyTypeName, {0}.FriendlyTypeName{4} );
                return false;
            }}  
",
                    parentTable,
                    columnName,
                    type.Name,
                    relationShipText,
                    pluralizeCode
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
            string restNamespace = type.Assembly.GetName().Name + ".Rest.Controllers";
            string dbContextFullName = Rock.Reflection.GetDbContextForEntityType( type ).GetType().FullName;

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
            sb.AppendFormat( "        public {0}Controller() : base( new {1}.{2}Service( new {3}() ) ) {{ }} " + Environment.NewLine, pluralizedName, type.Namespace, type.Name, dbContextFullName );
            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );

            var filePath1 = Path.Combine( rootFolder, "Controllers" );
            var file = new FileInfo( Path.Combine( filePath1, "CodeGenerated", pluralizedName + "Controller.cs" ) );
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
            var dirInfo = new DirectoryInfo( Path.GetDirectoryName( lblAssemblyPath.Text ) );
            while ( dirInfo != null && !dirInfo.GetDirectories().Any( a => a.Name == "RockWeb" ) )
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
                if ( type.Namespace == "Rock.Model" )
                {
                    return "Rock.Client.Enums." + type.Name;
                }
                else
                {
                    return GetKeyName( "Int32" ) + " /* " + type.Name + "*/";
                }
            }
            else
            {
                return GetKeyName( type.Name );
            }
        }

        private Dictionary<string, PropertyInfo> GetEntityProperties( Type type, bool includeRockClientIncludes )
        {
            var properties = new Dictionary<string, PropertyInfo>();

            var interfaces = type.GetInterfaces();

            foreach ( var property in type.GetProperties().SortByStandardOrder() )
            {
                bool include = false;
                if (includeRockClientIncludes && property.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>() != null)
                {
                    include = true;
                }
                
                var getMethod = property.GetGetMethod();
                if (getMethod == null)
                {
                    continue;
                }

                if ( getMethod.IsVirtual )
                {
                    if ( !include && !getMethod.IsFinal )
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
                    if ( !include && !interfaceProperty )
                    {
                        continue;
                    }
                }

                if ( !property.GetCustomAttributes( typeof( DatabaseGeneratedAttribute ) ).Any() )
                {
                    if ( (property.GetCustomAttribute<ObsoleteAttribute>() == null) )
                    {
                        if ( property.SetMethod != null && property.SetMethod.IsPublic && property.GetMethod.IsPublic )
                        {
                            properties.Add( property.Name, property );
                        }
                    }
                }
            }

            return properties;
        }

        /// <summary>
        /// Writes the rock client model enums file.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        private void WriteRockClientEnumsFile( string rootFolder )
        {
            rockAssembly = typeof( Rock.Data.IEntity ).Assembly;
            StringBuilder sb = new StringBuilder();
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
            sb.AppendLine( "using System.Collections.Generic;" );
            sb.AppendLine( "" );

            sb.AppendLine( "namespace Rock.Client.Enums" );
            sb.AppendLine( "{" );

            foreach ( var enumType in rockAssembly.GetTypes().Where( a => a.IsEnum ).OrderBy( a => a.Name ) )
            {
                if ( enumType.Namespace == "Rock.Model" )
                {
                    sb.AppendLine( "    /// <summary>" );
                    sb.AppendLine( "    /// </summary>" );
                    if (enumType.GetCustomAttribute<FlagsAttribute>() != null)
                    {
                        sb.AppendLine( "    [Flags]");
                    }
                    sb.AppendFormat( "    public enum {0}" + Environment.NewLine, enumType.Name );
                    sb.AppendLine( "    {" );
                    var enumValues = Enum.GetValues( enumType);
                    foreach ( var enumValueName in Enum.GetNames( enumType ) )
                    {
                        int enumValue = (int)Convert.ChangeType( Enum.Parse( enumType, enumValueName ), typeof( int ) );
                        string enumValueParam = enumValue >= 0 ? " = 0x" + enumValue.ToString( "x" ) : " = " + enumValue.ToString();
                        sb.AppendFormat( "        {0}{1},", enumValueName, enumValueParam );
                        sb.AppendLine( "" );
                    }

                    sb.AppendLine( "    }" );
                    sb.AppendLine( "" );
                }
            }


            sb.AppendLine( "}" );

            var file = new FileInfo( Path.Combine( rootFolder, "CodeGenerated\\Enums", "RockEnums.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Writes the rock client system unique identifier files.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        private void WriteRockClientSystemGuidFiles( string rootFolder )
        {
            rockAssembly = typeof( Rock.Data.IEntity ).Assembly;
            StringBuilder sb = new StringBuilder();
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
            sb.AppendLine( "using System.Collections.Generic;" );
            sb.AppendLine( "" );

            sb.AppendLine( "namespace Rock.Client.SystemGuid" );
            sb.AppendLine( "{" );

            foreach ( var systemGuidType in rockAssembly.GetTypes().Where( a => a.Namespace == "Rock.SystemGuid" ).OrderBy( a => a.Name ) )
            {
                sb.AppendLine( "    /// <summary>" );
                sb.AppendLine( "    /// </summary>" );
                sb.AppendFormat( "    public class {0}" + Environment.NewLine, systemGuidType.Name );
                sb.AppendLine( "    {" );
                foreach ( var field in systemGuidType.GetFields().OrderBy( a => a.Name ) )
                {
                    sb.AppendFormat( "        public const string {0} = \"{1}\";", field.Name, field.GetValue( systemGuidType ) );
                    sb.AppendLine( "" );
                }

                sb.AppendLine( "    }" );
                sb.AppendLine( "" );
            }


            sb.AppendLine( "}");

            var file = new FileInfo( Path.Combine( rootFolder, "CodeGenerated\\SystemGuid", "RockSystemGuids.cs" ) );
            WriteFile( file, sb );
        }

        /// <summary>
        /// Writes the rock client include client files.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="alreadyIncludedTypes">The already included types.</param>
        private void WriteRockClientIncludeClientFiles( string rootFolder, IEnumerable<Type> alreadyIncludedTypes)
        {
            foreach ( var rockClientIncludeType in rockAssembly.GetTypes().Where( a => a.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>() != null ).OrderBy( a => a.Name ) )
            {
                if ( !alreadyIncludedTypes.Any( a => a == rockClientIncludeType ) )
                {
                    WriteRockClientFile( rootFolder, rockClientIncludeType );
                }
            }
        }

        /// <summary>
        /// Writes the DTO file for a given type
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="type"></param>
        private void WriteRockClientFile( string rootFolder, Type type )
        {
            // make a copy of the EntityProperties since we are deleting some for this method
            var entityProperties = GetEntityProperties( type, true ).ToDictionary( k => k.Key, v => v.Value);
            entityProperties.Remove( "CreatedDateTime" );
            entityProperties.Remove( "ModifiedDateTime" );
            entityProperties.Remove( "CreatedByPersonAliasId" );
            entityProperties.Remove( "ModifiedByPersonAliasId" );

            var dataMembers = type.GetProperties().SortByStandardOrder()
                .Where( a => a.GetCustomAttribute<DataMemberAttribute>() != null )
                .Where( a => a.GetCustomAttribute<ObsoleteAttribute>() == null )
                .Where( a => (a.GetCustomAttribute<NotMappedAttribute>() == null || a.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>() != null) )
                .Where( a => !entityProperties.Keys.Contains( a.Name ) );

            var rockClientIncludeAttribute = type.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>();
            string comments = null;

            if ( rockClientIncludeAttribute != null)
            {
                comments = rockClientIncludeAttribute.DocumentationMessage;
            }
            
            if ( !entityProperties.Any() && !dataMembers.Any() )
            {
                return;
            }

            string lcName = type.Name.Substring( 0, 1 ).ToLower() + type.Name.Substring( 1 );

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
            sb.AppendLine( "using System.Collections.Generic;" );
            sb.AppendLine( "" );
            sb.AppendLine( "" );

            sb.AppendFormat( "namespace Rock.Client" + Environment.NewLine, type.Namespace );
            sb.AppendLine( "{" );

            sb.AppendLine( "    /// <summary>" );
            
            
            if ( !string.IsNullOrWhiteSpace( comments ) )
            {
                sb.AppendFormat( "    /// {0}" + Environment.NewLine, comments );
            }
            else
            {
                sb.AppendFormat( "    /// Base client model for {0} that only includes the non-virtual fields. Use this for PUT/POSTs" + Environment.NewLine, type.Name );
            }

            sb.AppendLine( "    /// </summary>" );

            sb.AppendFormat( "    public partial class {0}Entity" + Environment.NewLine, type.Name );
            sb.AppendLine( "    {" );

            foreach ( var keyVal in entityProperties )
            {
                var propertyRockClientIncludeAttribute = keyVal.Value.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>();
                string propertyComments = null;

                if ( propertyRockClientIncludeAttribute != null )
                {
                    propertyComments = propertyRockClientIncludeAttribute.DocumentationMessage;
                }

                if ( !string.IsNullOrWhiteSpace( propertyComments ) )
                {
                    sb.AppendLine( "        /// <summary>" );
                    sb.AppendFormat( "        /// {0}" + Environment.NewLine, propertyComments );
                    sb.AppendLine( "        /// </summary>" );
                }
                else
                {
                    sb.AppendLine( "        /// <summary />" );
                }
                sb.AppendFormat( "        public {0} {1} {{ get; set; }}" + Environment.NewLine, this.PropertyTypeName( keyVal.Value.PropertyType ), keyVal.Key );
                sb.AppendLine( "" );
            }

            sb.AppendLine( "    }" );

            sb.AppendLine( "" );
            
            sb.AppendLine( "    /// <summary>" );

            if ( !string.IsNullOrWhiteSpace( comments ) )
            {
                sb.AppendFormat( "    /// {0}" + Environment.NewLine, comments );
            }
            else
            {
                sb.AppendFormat( "    /// Client model for {0} that includes all the fields that are available for GETs. Use this for GETs (use {0}Entity for POST/PUTs)" + Environment.NewLine, type.Name );
            }

            sb.AppendLine( "    /// </summary>" );

            sb.AppendFormat( "    public partial class {0} : {0}Entity" + Environment.NewLine, type.Name );
            sb.AppendLine( "    {" );

            foreach ( var dataMember in dataMembers )
            {
                var dataMemberRockClientIncludeAttribute = dataMember.GetCustomAttribute<Rock.Data.RockClientIncludeAttribute>();
                string dataMemberComments = null;

                if ( dataMemberRockClientIncludeAttribute != null )
                {
                    dataMemberComments = dataMemberRockClientIncludeAttribute.DocumentationMessage;
                }
                
                if (!string.IsNullOrWhiteSpace(dataMemberComments))
                {
                    sb.AppendLine( "        /// <summary>" );
                    sb.AppendFormat( "        /// {0}" + Environment.NewLine, dataMemberComments );
                    sb.AppendLine( "        /// </summary>" );
                }
                else
                {
                    sb.AppendLine( "        /// <summary />" );    
                }
                
                sb.AppendFormat( "        public {0} {1} {{ get; set; }}" + Environment.NewLine, PropertyTypeName( dataMember.PropertyType ), dataMember.Name );
                sb.AppendLine( "" );
            }

            // if this is a IHasAttributes type, generate Attribute/AttributeValues since they can be fetched thru REST when ?loadAttributes is specified
            if ( typeof( Rock.Attribute.IHasAttributes ).IsAssignableFrom( type ) )
            {
                sb.AppendLine( "        /// <summary>" );
                sb.AppendLine( "        /// NOTE: Attributes are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' " );
                sb.AppendLine( "        /// </summary>" );
                sb.AppendLine( "        public Dictionary<string, Rock.Client.Attribute> Attributes { get; set; }" );
                sb.AppendLine( "" );
                sb.AppendLine( "        /// <summary>" );
                sb.AppendLine( "        /// NOTE: AttributeValues are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' " );
                sb.AppendLine( "        /// </summary>" );
                sb.AppendLine( "        public Dictionary<string, Rock.Client.AttributeValue> AttributeValues { get; set; }" );
            }

            sb.AppendLine( "    }" );
            sb.AppendLine( "}" );

            //var file = new FileInfo( Path.Combine( NamespaceFolder( rootFolder, type.Namespace ).FullName, "CodeGenerated", type.Name + "Dto.cs" ) );
            var file = new FileInfo( Path.Combine( rootFolder, "CodeGenerated", type.Name + ".cs" ) );
            WriteFile( file, sb );
        }

        private void tbServiceFolder_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            fbdServiceOutput.SelectedPath = tbServiceFolder.Text;
            if ( fbdServiceOutput.ShowDialog() == DialogResult.OK )
            {
                tbServiceFolder.Text = fbdServiceOutput.SelectedPath;
            }
        }

        private void tbRestFolder_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            fbdRestOutput.SelectedPath = tbRestFolder.Text;
            if ( fbdRestOutput.ShowDialog() == DialogResult.OK )
            {
                tbRestFolder.Text = fbdRestOutput.SelectedPath;
            }
        }

        private void tbClientFolder_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            fdbRockClient.SelectedPath = tbClientFolder.Text;
            if ( fdbRockClient.ShowDialog() == DialogResult.OK )
            {
                tbClientFolder.Text = fdbRockClient.SelectedPath;
            }
        }
    }

    public static class HelperExtensions
    {
        public static PropertyInfo[] SortByStandardOrder( this PropertyInfo[] properties )
        {
            string[] baseModelPropertyTypeNames = new string[] { "Id", "CreatedDateTime", "ModifiedDateTime", "CreatedByPersonAliasId", "ModifiedByPersonAliasId", "Guid", "ForeignId" };
            var result = new List<PropertyInfo>();

            // Have Standard Order by "Id", <alphabetic list of other fields>, "CreatedDateTime", "ModifiedDateTime", "CreatedByPersonAliasId", "ModifiedByPersonAliasId", "Guid", "ForeignId"
            result.AddRange( properties.Where( a => !baseModelPropertyTypeNames.Contains( a.Name ) ).OrderBy( a => a.Name ) );

            foreach ( var name in baseModelPropertyTypeNames )
            {
                var property = properties.FirstOrDefault( a => a.Name == name );
                if ( property != null )
                {
                    if ( property.Name == "Id" )
                    {
                        result.Insert( 0, property );
                    }
                    else
                    {
                        result.Add( property );
                    }
                }
            }

            return result.ToArray();
        }
    }
}
