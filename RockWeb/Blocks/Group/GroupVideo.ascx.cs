using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Groups;

namespace RockWeb.Blocks.Group
{
    [Rock.Attribute.Property( 0, "Group Id", "Behavior", "The Group Id of the parent group", false, "", "Rock", "Rock.FieldTypes.Integer" )]
    [Rock.Attribute.Property( 1, "Group Levels", "Behavior", "The Group Role to use when person is added to group", true, "", "Rock", "Rock.FieldTypes.Integer" )]
    [Rock.Attribute.Property( 2, "Group Role", "Behavior", "The Group Role to use when person is added to group", true, "", "Rock", "Rock.FieldTypes.Integer" )]
    [Rock.Attribute.Property( 3, "Duration Attribute Key", "Behavior", "The key of the duration attribute", false, "Duration" )]
    [Rock.Attribute.Property( 3, "Video Attribute Key", "Behavior", "The key of the video attribute", false, "Video" )]
    public partial class GroupVideo : Rock.Web.UI.Block
    {
        int _levels = int.MaxValue;
        string _videoAttributeKey;
        string _durationAttributeKey;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            Rock.Web.UI.Page.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.core.min.js" );
            Rock.Web.UI.Page.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.fx.min.js" );
            Rock.Web.UI.Page.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.draganddrop.min.js" );
            Rock.Web.UI.Page.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.treeview.min.js" );

            Rock.Web.UI.Page.AddCSSLink( this.Page, "~/CSS/Kendo/kendo.common.min.css" );
            Rock.Web.UI.Page.AddCSSLink( this.Page, "~/CSS/Kendo/kendo.rock.min.css" );

            // Add the neccessary CSS and Scripts required for video field types
            Rock.FieldTypes.Video.AddLinks( this.Page );

            // Add script to start playing a video and add the person to a group whenever they
            // click on a group that has a video
            string script = string.Format( @"

    Sys.Application.add_load(function () {{

        $('li[video]').click(function () {{

            var $li = $(this);
            var videoId = $(this).closest('.group-tree').attr('video-id');
            
            $('#' + videoId).show();            

            var myPlayer = _V_(videoId);
            myPlayer.src({{
                type: 'video/mp4',
                src: $(this).attr('video')
            }});
            myPlayer.play();

            if ($(this).children('span:first').is('.not-viewed'))
            {{
                var groupMember = new Object();
                groupMember.Id = 0;
                groupMember.GroupId = $(this).attr('group-id');
                groupMember.PersonId = '{0}';
                groupMember.GroupRoleId = '{1}';
            
                $.ajax({{
                    type: 'POST',
                    contentType: 'application/json',
                    dataType: 'json',
                    data: JSON.stringify(groupMember),
                    url: rock.baseUrl + 'REST/Groups/Member/',
                    success: function (data, status, xhr) {{

                        $li.children('span:first').addClass('viewed');
                        $li.children('span:first').removeClass('not-viewed');

                    }},
                    error: function (xhr, status, error) {{

                        alert(status + ' [' + error + ']: ' + xhr.responseText);

                    }}
                }});

            }}

        }});

    }});
", CurrentPersonId, AttributeValue( "GroupRole" ) );
            this.Page.ClientScript.RegisterStartupScript( this.GetType(), "setVideo", script, true );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Int32.TryParse( AttributeValue( "GroupLevels" ), out _levels ) )
                _levels = int.MaxValue;

            _videoAttributeKey = AttributeValue( "VideoAttributeKey" );
            _durationAttributeKey = AttributeValue( "DurationAttributeKey" );

            BuildHierarchy();
        }

        #region Build Group Tree

        private void BuildHierarchy()
        {
            GroupRepository groupRepository = new GroupRepository();

            int groupId = 0;
            if ( Int32.TryParse( AttributeValue( "GroupId" ), out groupId ) )
            {
                var parentGroup = groupRepository.Get( groupId );
                if ( parentGroup != null )
                {
                    lParentGroup.Text = parentGroup.Name;
                    HtmlGenericControl ul = RenderBranch( parentGroup.Groups, 1 );
                    if ( ul != null )
                        phTreeView.Controls.Add( ul );
                }
            }
        }

        private HtmlGenericControl RenderBranch( IEnumerable<Rock.Groups.Group> groups, int level )
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

                    // Add the group type as an additional class name
                    li.AddCssClass( group.GroupType.Name.ToLower().Replace( ' ', '-' ) );

                    // Add the group group id as an attribute on the LI element
                    li.Attributes["group-id"] = group.Id.ToString();

                    // Add the url of the video as an attribute on the LI element
                    if (group.AttributeValues.ContainsKey(_videoAttributeKey))
                        li.Attributes["video"] = group.AttributeValues[_videoAttributeKey].Value;

                    // Add the span indicating if person has viewed the video
                    HtmlGenericControl viewedSpan = new HtmlGenericControl( "span" );
                    viewedSpan.AddCssClass( group.Members.Any( m => m.PersonId == CurrentPersonId ) ? "viewed" : "not-viewed" );
                    li.Controls.Add( viewedSpan );

                    // Add the name of the group
                    li.Controls.Add( new LiteralControl( group.Name ) );

                    // Add duration span
                    HtmlGenericControl durationSpan = new HtmlGenericControl( "span" );
                    durationSpan.AddCssClass( "duration" );
                    if (group.AttributeValues.ContainsKey(_durationAttributeKey))
                        durationSpan.InnerText = group.AttributeValues[_durationAttributeKey].Value;
                    li.Controls.Add( durationSpan );

                    // Render any child groups
                    HtmlGenericControl childUl = RenderBranch( group.Groups, level + 1 );
                    if ( childUl != null )
                        li.Controls.Add( childUl );
                }

                return ul.Controls.Count > 0 ? ul : null;
            }

            return null;
        }

        #endregion
        
    }
}