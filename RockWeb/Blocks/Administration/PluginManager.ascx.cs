﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Services.NuGet;
using NuGet;

namespace RockWeb.Blocks.Administration
{
	public enum ViewMode
	{
		Installed,
		Available
	}

	public partial class PluginManager : Rock.Web.UI.Block
	{
		#region Fields
		WebProjectManager nuGetService = null;
		ViewMode viewing;

		protected WebProjectManager NuGetService
		{
			get
			{
				if ( nuGetService == null )
				{
					string packageSource = Rock.Web.Cache.GlobalAttributes.Value( "PackageSourceUrl" );
					string siteRoot = Request.MapPath( "~/" );

					nuGetService = new WebProjectManager( packageSource, siteRoot );
				}
				return nuGetService;
			}
		}

		IEnumerable<IPackage> availablePackages = null;
		protected IEnumerable<IPackage> AvailablePackages
		{
			get
			{
				if ( availablePackages == null )
				{
					availablePackages = NuGetService.GetLatestRemotePackages( "", includeAllVersions: false );
				}
				return availablePackages;
			}
		}

		IEnumerable<IPackage> installedPackages = null;
		protected IEnumerable<IPackage> InstalledPackages
		{
			get
			{
				if ( installedPackages == null )
				{
					installedPackages = NuGetService.GetInstalledPackages( "" );
				}
				return installedPackages;
			}
		}

		#endregion

		#region Control Methods

		protected override void OnInit( EventArgs e )
		{
			base.OnInit( e );
			gPackageList.RowUpdating += gPackageList_RowUpdating;
			gvPackageVersions.RowUpdating += gvPackageVersions_RowUpdating;
		}

		protected override void OnLoad( EventArgs e )
		{
			nbMessage.Visible = false;

			this.viewing = ( hfViewing.Value == "available" ) ? ViewMode.Available : ViewMode.Installed;

			if ( PageInstance.IsAuthorized( "Configure", CurrentUser ) )
			{
				if ( !Page.IsPostBack )
				{
					BindPackageListGrid();
				}
			}
			else
			{
				nbMessage.Text = "You are not authorized to edit.";
				nbMessage.Visible = true;
			}

			base.OnLoad( e );
		}

		protected override void OnPreRender( EventArgs e )
		{
			base.OnPreRender( e );
		}
		#endregion

		#region Nav Button Events
		
		protected void btnInstalled_Click( object sender, EventArgs e )
		{
			this.viewing = ViewMode.Installed;
			BindPackageListGrid();
		}

		protected void btnAvailable_Click( object sender, EventArgs e )
		{
			this.viewing = ViewMode.Available;
			BindPackageListGrid();
		}

		protected void bSearch_Click( object sender, EventArgs e )
		{
			liInstalled.RemoveCssClass( "active" );
			liAvailable.RemoveCssClass( "active" );

			gPackageList.DataSource = NuGetService.GetLatestRemotePackages( txtSearch.Text, includeAllVersions: false );
			gPackageList.DataBind();
		}

		protected void lbBack_Click( object sender, EventArgs e )
		{
			pnlPackageList.Visible = true;
			pnlPackage.Visible = false;
			BindPackageListGrid();
		}

		#endregion

		protected void BindPackageListGrid()
		{
			switch ( this.viewing )
			{
				case ViewMode.Installed:
					liInstalled.AddCssClass( "active" );
					liAvailable.RemoveCssClass( "active" );
					gPackageList.DataSource = InstalledPackages;
					break;
				case ViewMode.Available:
					liInstalled.RemoveCssClass( "active" );
					liAvailable.AddCssClass( "active" );
					gPackageList.DataSource = AvailablePackages;
					break;
				default:
					break;
			}

			gPackageList.DataBind();
		}

		#region Package List Grid Events

		/// <summary>
		/// Used to populate each item in the PackageList
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void gPackageList_RowDataBound( object sender, GridViewRowEventArgs e )
		{
			IPackage package = e.Row.DataItem as IPackage;
			if ( package != null )
			{
				Boolean isPackageInstalled = NuGetService.IsPackageInstalled( package, anyVersion: true );
				
				LinkButton lbCommand = e.Row.FindControl( "lbCommand" ) as LinkButton;
				LinkButton lbUpdate = e.Row.FindControl( "lbUpdate" ) as LinkButton;
				LinkButton lbView = e.Row.FindControl( "lbView" ) as LinkButton;
				HtmlAnchor link = e.Row.FindControl( "lProjectUrl" ) as HtmlAnchor;
				HtmlImage imgIcon = e.Row.FindControl( "imgIcon" ) as HtmlImage;
				Literal lblAuthors = e.Row.FindControl( "lblAuthors" ) as Literal;
				Literal lblVersion = e.Row.FindControl( "lblVersion" ) as Literal;
				Literal lblLatestVersion = e.Row.FindControl( "lblLatestVersion" ) as Literal;
				Literal lblInstalledVersion = e.Row.FindControl( "lblInstalledVersion" ) as Literal;

				lblAuthors.Text = string.Join( ",", package.Authors );

				if ( package.IconUrl == null )
				{
					// TODO: change to "http://quarry.rockchms.com/Content/Images/Quarry/packageDefaultIcon-80x80.png";
					imgIcon.Src = "http://quarry.rockchms.com/Content/Images/packageDefaultIcon1.png";
				}

				if ( package.ProjectUrl != null )
				{
					link.Visible = true;
					link.HRef = package.ProjectUrl.ToString();
				}
				else
				{
					link.Visible = false;
				}
				
				lbUpdate.Visible = false;

				// If this package (not necessarily this version) is installed
				// show an uninstall button and/or an update button if a later version exists
				if ( isPackageInstalled )
				{
					IPackage theInstalledPackage = NuGetService.GetInstalledPackage( package.Id );
					if ( theInstalledPackage != null )
					{
						lblInstalledVersion.Visible = true;
						lblInstalledVersion.Text += theInstalledPackage.Version;

						// Checking "IsLatestVersion" does not work because of what's discussed here:
						// http://nuget.codeplex.com/discussions/279837
						// if ( !installedPackage.IsLatestVersion )...
						var latestPackage = NuGetService.GetUpdate( package );
						if ( latestPackage != null )
						{
							lbUpdate.Visible = true;
							lblLatestVersion.Visible = true;
							lblLatestVersion.Text += latestPackage.Version;
						}
					}

					lbCommand.CommandName = "uninstall";
					lbCommand.Text = "<i class='icon-remove'></i> &nbsp; Uninstall";
					lbCommand.AddCssClass( "btn-warning" );
				}
				else
				{
					lblVersion.Visible = true;
					lblVersion.Text += package.Version;
					lbCommand.CommandName = "Install";
					lbCommand.Text = "<i class='icon-download-alt'></i> &nbsp; Install";
				}

				lbCommand.CommandArgument = lbUpdate.CommandArgument = lbView.CommandArgument = e.Row.RowIndex.ToString();
			}
		}

		protected void gPackageList_RowCommand( object sender, GridViewCommandEventArgs e )
		{
			int index = Int32.Parse( e.CommandArgument.ToString() );
			string packageId = gPackageList.DataKeys[index]["Id"].ToString();
			string version = gPackageList.DataKeys[index]["Version"].ToString();

			if ( e.CommandName.ToLower() == "view" )
			{
				ViewPackage( packageId );
			}
			else
			{
				ChangePackage( e, packageId, version );
				BindPackageListGrid();
			}
		}

		protected void gPackageList_RowUpdating( object sender, GridViewUpdateEventArgs e )
		{
			// Not sure why this is getting called, but it is and will throw an exception 
			// if I don't handle it.
		}

		#endregion

		#region Package Versions Grid Events

		private void ViewPackage( string packageId )
		{
			pnlPackage.Visible = true;
			pnlPackageList.Visible = false;

			IPackage package = NuGetService.SourceRepository.FindPackage( packageId, version: null, allowPrereleaseVersions: false, allowUnlisted: false );

			// TODO: change to "http://quarry.rockchms.com/Content/Images/Quarry/packageDefaultIcon-128x128.png";
			imgIcon.ImageUrl = ( package.IconUrl == null ) ? "http://quarry.rockchms.com/Content/Images/packageDefaultIcon1.png" : package.IconUrl.ToString();
			lTitle.Text = package.Title;
			lAuthors.Text = string.Join( ",", package.Authors );
			lDescription.Text = package.Description;
			lTags.Text = package.Tags;
			lDependencies.Text = ( package.DependencySets.Count() == 0 ) ? "This plugin has no dependencies." : 
				package.DependencySets.Aggregate( new StringBuilder( "<ul>" ), ( sb, s ) => sb.AppendFormat( "<li>{0}</li>", s ) ).Append( "</ul>" ).ToString();

			lbPackageUninstall.CommandArgument = packageId;
			lbPackageUninstall.Visible = NuGetService.IsPackageInstalled( package, anyVersion: true );
			gvPackageVersions.DataSource = NuGetService.SourceRepository.FindPackagesById( package.Id ).OrderByDescending( p => p.Version );
			gvPackageVersions.DataBind();
		}

		protected void gvPackageVersions_RowUpdating( object sender, GridViewUpdateEventArgs e )
		{
			// Not sure why this is getting called, but it is and will throw an exception 
			// if I don't handle it.
		}

		protected void gvPackageVersions_RowDataBound( object sender, GridViewRowEventArgs e )
		{
			if ( e.Row.RowType == DataControlRowType.DataRow )
			{
				IPackage package = e.Row.DataItem as IPackage;
				if ( package != null )
				{
					Boolean isExactPackageInstalled = NuGetService.IsPackageInstalled( package );
					LinkButton lbInstall = e.Row.FindControl( "lbInstall" ) as LinkButton;
					LinkButton lbUpdate = e.Row.FindControl( "lbUpdate" ) as LinkButton;
					HtmlGenericControl iInstalledIcon = e.Row.FindControl( "iInstalledIcon" ) as HtmlGenericControl;

					lbInstall.CommandArgument = lbUpdate.CommandArgument = e.Row.RowIndex.ToString();

					// Don't show the install button if a version of this package is already installed
					lbInstall.Visible = ( ! lbPackageUninstall.Visible );

					// Show the update button if an older version of this package is already installed and the package is the latest version
					lbUpdate.Visible = ( ( lbPackageUninstall.Visible && ! isExactPackageInstalled ) && package.IsLatestVersion );

					iInstalledIcon.Visible = ( isExactPackageInstalled );
				}
			}
		}

		protected void gvPackageVersions_RowCommand( object sender, GridViewCommandEventArgs e )
		{
			int index = Int32.Parse( e.CommandArgument.ToString() );
			string packageId = gvPackageVersions.DataKeys[index]["Id"].ToString();
			string version = gvPackageVersions.DataKeys[index]["Version"].ToString();

			ChangePackage( e, packageId, version );
			ViewPackage( packageId );
		}

		protected void lbPackageUninstall_Click( object sender, CommandEventArgs e )
		{
			string packageId = e.CommandArgument.ToString();
			ChangePackage( e, packageId, null );
			ViewPackage( packageId );
		}

		#endregion

		#region Package Utility Method (used by both panels/grids)
		private void ChangePackage( CommandEventArgs e, string packageId, string version )
		{
			IEnumerable<string> errors = Enumerable.Empty<string>();

			switch ( e.CommandName.ToLower() )
			{
				case "uninstall":
					{
						var package = NuGetService.SourceRepository.FindPackage( packageId, ( version != null ) ? SemanticVersion.Parse( version ) : null, false, false );
						errors = NuGetService.UninstallPackage( package, true );
					}
					break;

				case "install":
					{
						var package = NuGetService.SourceRepository.FindPackage( packageId, ( version != null ) ? SemanticVersion.Parse( version ) : null, false, false );
						if ( package != null )
						{
							errors = NuGetService.InstallPackage( package );
							//IEnumerable<IPackage> packagesRequiringLicenseAcceptance = NuGetService.GetPackagesRequiringLicenseAcceptance(package);
						}
						break;
					}
				case "update":
					{
						var installed = NuGetService.GetInstalledPackage( packageId );
						var update = NuGetService.GetUpdate( installed );
						errors = NuGetService.UpdatePackage( update );
					}
					break;
			}

			if ( errors != null && errors.Count() > 0 )
			{
				nbMessage.Visible = true;
				nbMessage.Text = errors.Aggregate( new StringBuilder( "<ul>" ), ( sb, s ) => sb.AppendFormat( "<li>{0}</li>", s ) ).Append( "</ul>" ).ToString();
			}
		}

		#endregion
	}
}