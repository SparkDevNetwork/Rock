//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;

namespace Rock.Services.NuGet
{
	public class WebProjectSystem : PhysicalFileSystem, IProjectSystem, IFileSystem
	{
		private bool _isBindingRedirectSupported = false;

		public bool IsBindingRedirectSupported { get { return _isBindingRedirectSupported; } }

		public WebProjectSystem( string siteRoot ) : base( siteRoot ) {	}

		public void AddFrameworkReference( string name )
		{
			throw new NotImplementedException();
		}

		public void AddReference( string referencePath, Stream stream )
		{
			string fileName = Path.GetFileName( referencePath );
			string fullPath = this.GetFullPath( GetReferencePath( fileName ) );
			this.AddFile( fullPath, stream );
		}

		protected virtual string GetReferencePath( string name )
		{
			return Path.Combine( "bin", name );
		}

		public bool IsSupportedFile( string path )
		{
		    return ( !path.StartsWith( "tools", StringComparison.OrdinalIgnoreCase ) && !Path.GetFileName( path ).Equals( "app.config", StringComparison.OrdinalIgnoreCase ) );
		}

		public string ProjectName
		{
			get { return Root; }
		}

		public bool ReferenceExists( string name )
		{
			string referencePath = GetReferencePath( name );
			return FileExists( referencePath );
		}

		public void RemoveReference( string name )
		{
			DeleteFile( GetReferencePath( name ) );
			if ( !this.GetFiles( "bin", false ).Any<string>() )
			{
				DeleteDirectory( "bin" );
			}
		}

		public string ResolvePath( string path )
		{
			return path;
		}

		public FrameworkName TargetFramework
		{
			get { return VersionUtility.DefaultTargetFramework; }
		}

		public dynamic GetPropertyValue( string propertyName )
		{
			if ( ( propertyName != null ) && propertyName.Equals( "RootNamespace", StringComparison.OrdinalIgnoreCase ) )
			{
				return string.Empty;
			}
			return null;
		}
	}
}
