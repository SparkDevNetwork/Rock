//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Crm;
using Rock.Web.UI;

namespace RockWeb.Blocks.Group
{
    [BlockProperty( 0, "Group Id", "Behavior", "The Group Id of the parent group", false, "", "Rock", "Rock.Field.Types.Integer" )]
    [BlockProperty( 1, "Group Levels", "Behavior", "The Group Role to use when person is added to group", true, "", "Rock", "Rock.Field.Types.Integer" )]
    [BlockProperty( 2, "Group Role", "Behavior", "The Group Role to use when person is added to group", true, "", "Rock", "Rock.Field.Types.Integer" )]
    [BlockProperty( 3, "Duration Attribute Key", "Behavior", "The key of the duration attribute", false, "Duration" )]
    [BlockProperty( 3, "Video Attribute Key", "Behavior", "The key of the video attribute", false, "Video" )]
    public partial class ClassVideo : Rock.Web.UI.RockBlock
    {
        GroupService groupService = new GroupService();

        int _levels = int.MaxValue;
        string _videoAttributeKey;
        string _durationAttributeKey;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Add the neccessary CSS and Scripts required for video field types
            Rock.Field.Types.Video.AddLinks( this.Page );

            if ( !Int32.TryParse( AttributeValue( "GroupLevels" ), out _levels ) )
                _levels = int.MaxValue;
            _videoAttributeKey = AttributeValue( "VideoAttributeKey" );
            _durationAttributeKey = AttributeValue( "DurationAttributeKey" );

            // Build the content tree
            BuildHierarchy();

            // Add script to start playing a video
            string script = string.Format( @"

    Sys.Application.add_load(function () {{

        var $videoUrl = $('#{1}');
        if ($videoUrl.val() != '') {{

            $('#video_{0}').show();            

            var myPlayer = _V_('video_{0}');
            myPlayer.src({{
                type: 'video/mp4',
                src: $videoUrl.val()
            }});
            myPlayer.play();

        }}

    }});
", CurrentBlock.Id, hfVideoUrl.ClientID );
            this.Page.ClientScript.RegisterStartupScript( this.GetType(), "setVideo", script, true );
        }

        #region Build Group Tree

        private void BuildHierarchy()
        {
            phTreeView.Controls.Clear();

            int groupId = 0;
            if ( Int32.TryParse( AttributeValue( "GroupId" ), out groupId ) )
            {
                var parentGroup = groupService.Get( groupId );
                if ( parentGroup != null )
                {
                    lParentGroup.Text = parentGroup.Name;
                    HtmlGenericControl ul = RenderBranch( parentGroup.Groups, 1 );
                    if ( ul != null )
                        phTreeView.Controls.Add( ul );
                }
            }
        }

        private HtmlGenericControl RenderBranch( IEnumerable<Rock.Crm.Group> groups, int level )
        {
            if ( level <= _levels )
            {
                HtmlGenericControl ul = new HtmlGenericControl( "ul" );
                ul.AddCssClass( string.Format( "level-{0}", level ) );

                foreach ( var group in groups )
                {
                    Rock.Attribute.Helper.LoadAttributes( group );

                    HtmlGenericControl li = new HtmlGenericControl( "li" );
                    ul.Controls.Add( li );

                    Control parentControl = li;

                    // Add the group type as an additional class name
                    li.AddCssClass( group.GroupType.Name.ToLower().Replace( ' ', '-' ) );

                    // Add the group group id as an attribute on the LI element
                    li.Attributes["group-id"] = group.Id.ToString();

                    // Add the url of the video as an attribute on the LI element
                    if ( group.AttributeValues.ContainsKey( _videoAttributeKey ) )
                    {
                        li.AddCssClass( group.Members.Any( m => m.PersonId == CurrentPersonId ) ? "viewed" : "not-viewed" );

                        LinkButton lb = new LinkButton();
                        li.Controls.Add( lb );
                        lb.Attributes.Add("group", group.Id.ToString());
                        lb.Click += new EventHandler( lb_Click );

                        lb.Controls.Add( new LiteralControl( group.Name ) );
                        parentControl = li;

                        if ( group.AttributeValues.ContainsKey( _durationAttributeKey ) )
                        {
                            HtmlGenericControl durationSpan = new HtmlGenericControl( "span" );
                            durationSpan.AddCssClass( "duration" );
                            durationSpan.InnerText = group.AttributeValues[_durationAttributeKey][0].Value;
                            lb.Controls.Add( durationSpan );
                        }
                    }
                    else
                    {
                        li.Controls.Add( new LiteralControl( group.Name ) );
                    }

                    // Render any child groups
                    HtmlGenericControl childUl = RenderBranch( group.Groups, level + 1 );
                    if ( childUl != null )
                        li.Controls.Add( childUl );
                }

                return ul.Controls.Count > 0 ? ul : null;
            }

            return null;
        }

        void lb_Click( object sender, EventArgs e )
        {
            if ( CurrentPersonId.HasValue )
            {
                LinkButton lb = sender as LinkButton;
                if ( lb != null )
                {
                    int groupId = 0;
                    if ( Int32.TryParse( lb.Attributes["group"], out groupId ) )
                    {
                        int roleId = 0;
                        if ( !Int32.TryParse( AttributeValue( "GroupRole" ), out roleId ) )
                            roleId = 0;

                        var group = groupService.Get( groupId );
                        if ( group != null &&
                            group.AttributeValues.ContainsKey( _videoAttributeKey ) )
                        {
                            hfVideoUrl.Value = group.AttributeValues[_videoAttributeKey][0].Value;

                            MemberService memberService = new MemberService();
                            var groupMember = memberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                                groupId, CurrentPersonId.Value, roleId );
                            if ( groupMember == null )
                            {
                                groupMember = new GroupMember();
                                groupMember.GroupId = groupId;
                                groupMember.PersonId = CurrentPersonId.Value;
                                groupMember.GroupRoleId = roleId;
                                memberService.Add( groupMember, CurrentPersonId );
                                memberService.Save( groupMember, CurrentPersonId );
                            }

                            HtmlGenericControl li = lb.Parent as HtmlGenericControl;
                            if ( li != null )
                            {
                                li.RemoveCssClass( "not-viewed" );
                                li.AddCssClass( "viewed" );
                            }
                        }
                    }
                }
            }


        }

        #endregion
        
    }
}