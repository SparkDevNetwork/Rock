using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Constants;

namespace com.centralaz.Prayerbook.Migrations
{
    [MigrationNumber( 1, "1.1.0" )]
    public class AddSystemData1 : Migration
    {
        public override void Up()
        {
            // Add Pages
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.ROCK_INTRANET_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_FULLWIDTH_LAYOUT, "Applications", "", com.centralaz.Prayerbook.SystemGuid.Page.APPLICATIONS_PAGE, "fa fa-angle-double-right" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.APPLICATIONS_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_RIGHTSIDEBAR_LAYOUT, "UP Team Prayerbook", "", com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, "fa fa-book" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_FULLWIDTH_LAYOUT, "Edit Entry", "", com.centralaz.Prayerbook.SystemGuid.Page.EDIT_ENTRY_PAGE, "fa fa-book" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_FULLWIDTH_LAYOUT, "App Management", "", com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, "fa fa-book" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_FULLWIDTH_LAYOUT, "Book Management", "", com.centralaz.Prayerbook.SystemGuid.Page.BOOK_MANAGEMENT_PAGE, "fa fa-book" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_FULLWIDTH_LAYOUT, "Administrators Group Management", "", com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MANAGEMENT_PAGE, "fa fa-book" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_FULLWIDTH_LAYOUT, "Contributors Group Management", "", com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MANAGEMENT_PAGE, "fa fa-book" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_LEFTSIDEBAR_LAYOUT, "Ministry List Management", "", com.centralaz.Prayerbook.SystemGuid.Page.MINISTRY_LIST_MANAGEMENT_PAGE, "fa fa-book" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_LEFTSIDEBAR_LAYOUT, "Subministry List Management", "", com.centralaz.Prayerbook.SystemGuid.Page.SUBMINISTRY_LIST_MANAGEMENT_PAGE, "fa fa-book" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MANAGEMENT_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_FULLWIDTH_LAYOUT, "Administrators Group Member Detail", "", com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MEMBER_DETAIL_PAGE, "fa fa-book" );
            RockMigrationHelper.AddPage( com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MANAGEMENT_PAGE, com.centralaz.Prayerbook.SystemGuid.Layout.ROCK_SITE1_FULLWIDTH_LAYOUT, "Contributors Group Member Detail", "", com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MEMBER_DETAIL_PAGE, "fa fa-book" );

            //Update Block Types
            RockMigrationHelper.UpdateBlockType( "Up Team Prayerbook Homepage Sidebar", "Up Team Prayebook Homepage Sidebar", "~/Plugins/com_centralaz/Prayerbook/PrayerBookHomePageSidebar.ascx", "com_centralaz > Prayerbook", com.centralaz.Prayerbook.SystemGuid.BlockType.HOMEPAGE_SIDEBAR_BLOCKTYPE );
            RockMigrationHelper.UpdateBlockType( "UP Team Prayerbook Edit Entry", "Edit and Add Prayerbook Entries", "~/Plugins/com_centralaz/Prayerbook/EditEntry.ascx", "com_centralaz > Prayerbook", com.centralaz.Prayerbook.SystemGuid.BlockType.EDIT_ENTRY_BLOCKTYPE );
            RockMigrationHelper.UpdateBlockType( "UP Team Prayerbook Book Management", "Manage Books; Open, Close, Publish, etc.", "~/Plugins/com_centralaz/Prayerbook/BookManagement.ascx", "com_centralaz > Prayerbook", com.centralaz.Prayerbook.SystemGuid.BlockType.BOOK_MANAGEMENT_BLOCKTYPE );

            //Add Blocks to Pages
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.HOMEPAGE_SIDEBAR_BLOCKTYPE, "UP Team Prayerbook Homepage Sidebar", "Sidebar1", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_SIDEBAR_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, "", Rock.SystemGuid.BlockType.HTML_CONTENT, "UP Team Prayerbook Welcome", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_WELCOME_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.EDIT_ENTRY_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.EDIT_ENTRY_BLOCKTYPE, "UP Team Prayerbook Edit Entry", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.EDIT_ENTRY_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.BOOK_MANAGEMENT_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.BOOK_MANAGEMENT_BLOCKTYPE, "UP Team Prayerbook Book Management", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.BOOK_MANAGEMENT_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.ROCK_PAGEMENU_BLOCKTYPE, "Management Page Navigation", "Feature", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.ROCK_PAGEMENU_BLOCKTYPE, "App Management Page Menu", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.APP_MANAGEMNT_PAGE_MENU_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MANAGEMENT_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.ROCK_GROUPMEMBERLIST_BLOCKTYPE, "Administrators Group Member List", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.ADMINISTRATORS_GROUP_MEMBER_LIST_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MEMBER_DETAIL_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.ROCK_GROUPMEMBERDETAIL_BLOCKTYPE, "Administrators Group Member Detail", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.ADMINISTRATORS_GROUP_MEMBER_DETAIL_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MANAGEMENT_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.ROCK_GROUPMEMBERLIST_BLOCKTYPE, "Contributors Group Member List", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.CONTRIBUTORS_GROUP_MEMBER_LIST_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MEMBER_DETAIL_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.ROCK_GROUPMEMBERDETAIL_BLOCKTYPE, "Contributors Group Member Detail", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.CONTRIBUTORS_GROUP_MEMBER_DETAIL_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.MINISTRY_LIST_MANAGEMENT_PAGE, "", Rock.SystemGuid.BlockType.HTML_CONTENT, "Note", "Sidebar1", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.MINISTRY_LIST_MANAGEMENT_NOTE_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.MINISTRY_LIST_MANAGEMENT_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.ROCK_DEFINEDVALUELIST_BLOCKTYPE, "Ministry List", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.MINISTRY_LIST_DEFINED_VALUE_LIST_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.SUBMINISTRY_LIST_MANAGEMENT_PAGE, "", Rock.SystemGuid.BlockType.HTML_CONTENT, "Note", "Sidebar1", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.SUBMINISTRY_LIST_MANAGEMENT_NOTE_BLOCK );
            RockMigrationHelper.AddBlock( com.centralaz.Prayerbook.SystemGuid.Page.SUBMINISTRY_LIST_MANAGEMENT_PAGE, "", com.centralaz.Prayerbook.SystemGuid.BlockType.ROCK_DEFINEDVALUELIST_BLOCKTYPE, "Subministry List", "Main", "", "", 0, com.centralaz.Prayerbook.SystemGuid.Block.SUBMINISTRY_LIST_DEFINED_VALUE_LIST_BLOCK );

            //Add Block Type Attributes
            RockMigrationHelper.AddBlockTypeAttribute( com.centralaz.Prayerbook.SystemGuid.BlockType.HOMEPAGE_SIDEBAR_BLOCKTYPE, Rock.SystemGuid.FieldType.PAGE_REFERENCE, "Edit Entry Page", "EditEntryPage", "", "", 0, @"", com.centralaz.Prayerbook.SystemGuid.Attribute.EDIT_ENTRY_PAGE_ATTRIBUTE );
            RockMigrationHelper.AddBlockTypeAttribute( com.centralaz.Prayerbook.SystemGuid.BlockType.BOOK_MANAGEMENT_BLOCKTYPE, Rock.SystemGuid.FieldType.PAGE_REFERENCE, "UP Team Prayerbook Homepage", "UPTeamPrayerbookHomepage", "", "", 0, @"", com.centralaz.Prayerbook.SystemGuid.Attribute.HOMEPAGE_PAGE_ATTRIBUTE );

            //Add Block Attribute Values
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_SIDEBAR_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.EDIT_ENTRY_PAGE_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.Page.EDIT_ENTRY_PAGE );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_WELCOME_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_HTMLCONTENT_CACHEDURATION_ATTRIBUTE, "0" );

            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.ADMINISTRATORS_GROUP_MEMBER_LIST_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_GROUPMEMBERLIST_DETAILPAGE_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.Page.ADMINISTRATORS_GROUP_MEMBER_DETAIL_PAGE );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.ADMINISTRATORS_GROUP_MEMBER_LIST_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_GROUPMEMBERLIST_GROUP_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.CONTRIBUTORS_GROUP_MEMBER_LIST_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_GROUPMEMBERLIST_DETAILPAGE_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.Page.CONTRIBUTORS_GROUP_MEMBER_DETAIL_PAGE );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.CONTRIBUTORS_GROUP_MEMBER_LIST_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_GROUPMEMBERLIST_GROUP_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP );

            StringBuilder s = new StringBuilder();
            s.Append( @"{% for subpage in Page.Pages %}{% if forloop.last == false %}<a href={{ subpage.Url }}>{{ subpage.Title }}</a> | {% else %}<a href={{ subpage.Url }}>{{ subpage.Title }}</a>{% endif %}{% endfor %}" );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_PAGEMENU_TEMPLATE_ATTRIBUTE, s.ToString() );

            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_PAGEMENU_ROOTPAGE_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_PAGEMENU_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_PAGEMENU_NUMBEROFLEVELS_ATTRIBUTE, "1" );

            s.Clear();
            s.Append( "<ul>{% for subpage in Page.Pages %}<li><a href={{ subpage.Url }}>{{ subpage.Title }}</a></li>{% endfor %}</ul>" );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.APP_MANAGEMNT_PAGE_MENU_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_PAGEMENU_TEMPLATE_ATTRIBUTE, s.ToString() );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.APP_MANAGEMNT_PAGE_MENU_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_PAGEMENU_ROOTPAGE_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.Page.APP_MANAGEMENT_PAGE );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.APP_MANAGEMNT_PAGE_MENU_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_PAGEMENU_NUMBEROFLEVELS_ATTRIBUTE, "1" );

            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.BOOK_MANAGEMENT_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.HOMEPAGE_PAGE_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE ); 
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.MINISTRY_LIST_DEFINED_VALUE_LIST_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_DEFINEDVALUELIST_DEFINEDTYPE_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.DefinedType.MINISTRY_DEFINED_TYPE );
            RockMigrationHelper.AddBlockAttributeValue( com.centralaz.Prayerbook.SystemGuid.Block.SUBMINISTRY_LIST_DEFINED_VALUE_LIST_BLOCK, com.centralaz.Prayerbook.SystemGuid.Attribute.ROCK_DEFINEDVALUELIST_DEFINEDTYPE_ATTRIBUTE, com.centralaz.Prayerbook.SystemGuid.DefinedType.SUBMINISTRY_DEFINED_TYPE );

            //Add HtmlContent to Welcome Block
            s.Clear();
            s.Append( @"<h2>Welcome, {{ Person.NickName }}</h2>
                <p>Please submit your requests for the UP Team Prayerbook by no later than noon 
                on the fifteenth day of the month. You will receive one courtesy reminder.</p>
                <h3>Writing Tips:</h3>
                <ul>
                <li>All fields are limited to 255 characters.</li>
                <li>Make at least one entry for each section in <i>Praise</i>, <i>Ministry Needs</i>, and <i>Personal Requests</i>.
                <li>Select the proper Name, Ministry, and Subministry where appropriate.</li>
                <li>Use the writing style as presented in the CentralStyle guide, including the usage of proper grammar, full sentences, and punctuation.</li>
                <li>Remember to use proper tense in your writing. Think ahead to the date this entry will be published. 
                You are writing approximately three weeks in advance of the publication.</li>
                <li>Avoid the use of the following in your writing:</li>
                <ul>
                <li>& (ampersand)</li>
                <li>"" (quotes)</li>
                <li>Tabs</li>
                <li>New lines (the Enter key)</li>
                </ul>
                <li>Be brief and succinct.</li>
                <li>Check your work for correctness.</li>
                </ul>
                <p>Thank you.</p>" );
            AddHtmlContent( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_WELCOME_BLOCK, "", 1, s.ToString(), true, null, DateTime.Now, null, null, com.centralaz.Prayerbook.SystemGuid.HtmlContent.WELCOME_BLOCK_HTMLCONTENT );

            s.Clear();
            s.Append( @"<h2>NOTE:</h2><b>Do not delete Values from this list.</b><p>If an entry is no longer used, click to edit it and uncheck 'isActive'.</p>" );
            AddHtmlContent( com.centralaz.Prayerbook.SystemGuid.Block.MINISTRY_LIST_MANAGEMENT_NOTE_BLOCK, "", 1, s.ToString(), true, null, DateTime.Now, null, null, com.centralaz.Prayerbook.SystemGuid.HtmlContent.MINISTRY_MANAGEMENT_NOTE_HTMLCONTENT );
            AddHtmlContent( com.centralaz.Prayerbook.SystemGuid.Block.SUBMINISTRY_LIST_MANAGEMENT_NOTE_BLOCK, "", 1, s.ToString(), true, null, DateTime.Now, null, null, com.centralaz.Prayerbook.SystemGuid.HtmlContent.SUBMINISTRY_MANAGEMENT_NOTE_HTMLCONTENT );

            //Defined Types
            RockMigrationHelper.AddDefinedType( "Global", "Prayerbook Ministry", "List of Prayerbook Ministries", com.centralaz.Prayerbook.SystemGuid.DefinedType.MINISTRY_DEFINED_TYPE );
            RockMigrationHelper.AddDefinedType( "Global", "Prayerbook Subministry", "List of Prayerbook Subministries", com.centralaz.Prayerbook.SystemGuid.DefinedType.SUBMINISTRY_DEFINED_TYPE );

            //Defined Type Attributes
            RockMigrationHelper.AddDefinedTypeAttribute( com.centralaz.Prayerbook.SystemGuid.DefinedType.MINISTRY_DEFINED_TYPE, Rock.SystemGuid.FieldType.BOOLEAN, "isActive", "isActive", "Is the ministry active?", 0, "true", com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRY_ISACTIVE_ATTRIBTUTE );
            RockMigrationHelper.AddDefinedTypeAttribute( com.centralaz.Prayerbook.SystemGuid.DefinedType.SUBMINISTRY_DEFINED_TYPE, Rock.SystemGuid.FieldType.BOOLEAN, "isActive", "isActive", "Is the subministry active?", 0, "true", com.centralaz.Prayerbook.SystemGuid.Attribute.SUBMINISTRY_ISACTIVE_ATTRIBTUTE );

            //set isGridColumn for these to TRUE
            Sql( String.Format( @"UPDATE [Attribute] SET [IsGridColumn] = 1 WHERE [Guid] IN ('{0}', '{1}')", com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRY_ISACTIVE_ATTRIBTUTE, com.centralaz.Prayerbook.SystemGuid.Attribute.SUBMINISTRY_ISACTIVE_ATTRIBTUTE ) );

            //Defined Values
            RockMigrationHelper.AddDefinedValue( com.centralaz.Prayerbook.SystemGuid.DefinedType.MINISTRY_DEFINED_TYPE, "Sample Ministry 01", "", com.centralaz.Prayerbook.SystemGuid.DefinedValue.SAMPLE_MINISTRY_DEFINEDVALUE, false );
            RockMigrationHelper.AddDefinedValue( com.centralaz.Prayerbook.SystemGuid.DefinedType.SUBMINISTRY_DEFINED_TYPE, "Sample Subministry 01", "", com.centralaz.Prayerbook.SystemGuid.DefinedValue.SAMPLE_SUBMINISTRY_DEFINEDVALUE, false );

            //Group Type and Group Type Association
            RockMigrationHelper.UpdateGroupType( "UP Team Prayerbook Admin Groups", "Group Type for UP Team Prayerbook Admin Groups", "Group", "Member", null, false, true, false, "fa fa-book", 0, null, 0, null, com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE, false );
            RockMigrationHelper.UpdateGroupType( "UP Team Prayerbook Contributors Group", "Group Type for UP Team Prayerbook Contributors Group", "Group", "Member", null, false, true, false, "fa fa-book", 0, null, 0, null, com.centralaz.Prayerbook.SystemGuid.GroupType.CONTRIBUTORS_GROUPTYPE, false );
            UpdateGroupTypeAssociation( com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE, com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE );
            UpdateGroupTypeAssociation( com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE, com.centralaz.Prayerbook.SystemGuid.GroupType.CONTRIBUTORS_GROUPTYPE );
            RockMigrationHelper.UpdateGroupType( "UP Team Prayerbook Groups", "GroupType for the Prayerbook Books Groups", "Group", "Member", null, false, true, false, "fa fa-book", 0, com.centralaz.Prayerbook.SystemGuid.GroupType.CONTRIBUTORS_GROUPTYPE, 0, null, com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, false );
            UpdateGroupTypeAssociation( com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE, com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE );

            //Group Type Role, and Groups
            RockMigrationHelper.UpdateGroupTypeRole( com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE, "Member", "", 0, null, null, com.centralaz.Prayerbook.SystemGuid.GroupTypeRole.CONTAINER_GROUPTYPE_MEMBER_GROUPTYPEROLE, false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, "Member", "", 0, null, null, com.centralaz.Prayerbook.SystemGuid.GroupTypeRole.BOOKS_GROUPTYPE_MEMBER_GROUPTYPEROLE, false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( com.centralaz.Prayerbook.SystemGuid.GroupType.CONTRIBUTORS_GROUPTYPE, "Member", "", 0, null, null, com.centralaz.Prayerbook.SystemGuid.GroupTypeRole.CONTRIBUTORS_GROUPTYPE_MEMBER_GROUPTYPEROLE, false, false, true );
            RockMigrationHelper.UpdateGroup( null, com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE, "UP Team Prayerbook", "Section head for UP Team Prayerbook groups", null, 0, com.centralaz.Prayerbook.SystemGuid.Group.SECTION_GROUP, false, false, true );
            RockMigrationHelper.UpdateGroup( com.centralaz.Prayerbook.SystemGuid.Group.SECTION_GROUP, com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE, "UP Team Prayerbook Administrators", "UP Team Prayerbook Administrators", null, 0, com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP, false, true, true );
            RockMigrationHelper.UpdateGroup( com.centralaz.Prayerbook.SystemGuid.Group.SECTION_GROUP, com.centralaz.Prayerbook.SystemGuid.GroupType.CONTRIBUTORS_GROUPTYPE, "UP Team Prayerbook Contributors", "UP Team Prayerbook Contributors", null, 0, com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP, false, true, true );
            RockMigrationHelper.UpdateGroup( com.centralaz.Prayerbook.SystemGuid.Group.SECTION_GROUP, com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE, "UP Team Prayerbook Monthly Books", "UP Team Prayerbook Monthly Books", null, 0, com.centralaz.Prayerbook.SystemGuid.Group.BOOKS_GROUP, false, true, true );

            //Add GroupType Group Attributes
            RockMigrationHelper.AddGroupTypeGroupAttribute( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, Rock.SystemGuid.FieldType.BOOLEAN, "isOpen", "is Open", 0, "True", "F438BA68-FE87-41CA-BFD1-D6912866407F" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, Rock.SystemGuid.FieldType.BOOLEAN, "isPublished", "Is published", 1, "False", "2277DA7E-3B5B-44CF-9664-29B2578AA368" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, Rock.SystemGuid.FieldType.DATE, "openDate", "Open Date", 2, null, "6D5C6677-FE19-4417-BE24-4DF81D6EBE45" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, Rock.SystemGuid.FieldType.DATE, "closeDate", "Close Date", 3, null, "D1E1FC2A-D8F1-4874-BE1B-F408A8134ED8" );

            //Add GroupType GroupMember Attributes
            UpdateGroupTypeGroupMemberAttributeDefinedValue( com.centralaz.Prayerbook.SystemGuid.GroupType.CONTRIBUTORS_GROUPTYPE, "Ministry", "The Ministry of the contributor.", 1, "", true, false, false, com.centralaz.Prayerbook.SystemGuid.DefinedType.MINISTRY_DEFINED_TYPE, "483615D4-C500-48E5-9D61-F0FA1F6753EC", isSystem: false );
            UpdateGroupTypeGroupMemberAttributeDefinedValue( com.centralaz.Prayerbook.SystemGuid.GroupType.CONTRIBUTORS_GROUPTYPE, "Subministry", "The Subministry of the contributor.", 2, "", true, false, false, com.centralaz.Prayerbook.SystemGuid.DefinedType.SUBMINISTRY_DEFINED_TYPE, "6991419A-28D5-4EA2-9911-4F59AC75AC50", isSystem: false );
            UpdateGroupTypeGroupMemberAttributeMemo( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, "Praise 1", "Praise 1", 3, "", false, false, true, com.centralaz.Prayerbook.SystemGuid.Attribute.PRAISE1_ATTRIBTUTE, false );
            UpdateGroupTypeGroupMemberAttributeMemo( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, "Personal Request 1", "Personal Request 1", 4, "", false, false, true, com.centralaz.Prayerbook.SystemGuid.Attribute.PERSONALREQUEST1_ATTRIBTUTE, false );
            UpdateGroupTypeGroupMemberAttributeMemo( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, "Personal Request 2", "Personal Request 2", 5, "", false, false, false, com.centralaz.Prayerbook.SystemGuid.Attribute.PERSONALREQUEST2_ATTRIBTUTE, false );
            UpdateGroupTypeGroupMemberAttributeMemo( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, "Ministry Need 1", "Ministry Need 1", 6, "", false, false, true, com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRYNEED1_ATTRIBTUTE, false );
            UpdateGroupTypeGroupMemberAttributeMemo( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, "Ministry Need 2", "Ministry Need 2", 7, "", false, false, false, com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRYNEED2_ATTRIBTUTE, false );
            UpdateGroupTypeGroupMemberAttributeMemo( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE, "Ministry Need 3", "Ministry Need 3", 8, "", false, false, false, com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRYNEED3_ATTRIBTUTE, false );
        }

        //TODO: Complete this
        public override void Down()
        {
            //Remove GroupType Group and GroupMemeber attributes
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.BOOK_MANAGEMENT_PAGE_ATTRIBUTE );
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.EDIT_ENTRY_PAGE_ATTRIBUTE );
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRY_ISACTIVE_ATTRIBTUTE );
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRYNEED1_ATTRIBTUTE );
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRYNEED2_ATTRIBTUTE );
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRYNEED3_ATTRIBTUTE );
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.PERSONALREQUEST1_ATTRIBTUTE );
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.PERSONALREQUEST2_ATTRIBTUTE );
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.PRAISE1_ATTRIBTUTE );
            RockMigrationHelper.DeleteAttribute( com.centralaz.Prayerbook.SystemGuid.Attribute.SUBMINISTRY_ISACTIVE_ATTRIBTUTE );

            RockMigrationHelper.DeletePage( com.centralaz.Prayerbook.SystemGuid.Page.APPLICATIONS_PAGE ); //  Page: Applications

            RockMigrationHelper.DeleteBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_SIDEBAR_BLOCK );
            RockMigrationHelper.DeleteBlock( com.centralaz.Prayerbook.SystemGuid.Block.HOMEPAGE_WELCOME_BLOCK );
            RockMigrationHelper.DeleteBlockType( com.centralaz.Prayerbook.SystemGuid.BlockType.HOMEPAGE_SIDEBAR_BLOCKTYPE );
            RockMigrationHelper.DeletePage( com.centralaz.Prayerbook.SystemGuid.Page.HOME_PAGE ); //  Page: Up Team Prayer Book

            RockMigrationHelper.DeleteBlock( com.centralaz.Prayerbook.SystemGuid.Block.EDIT_ENTRY_BLOCK );
            RockMigrationHelper.DeleteBlockType( com.centralaz.Prayerbook.SystemGuid.BlockType.EDIT_ENTRY_BLOCKTYPE );
            RockMigrationHelper.DeletePage( com.centralaz.Prayerbook.SystemGuid.Page.EDIT_ENTRY_PAGE ); //  Page: Edit Entry

            RockMigrationHelper.DeleteBlock( com.centralaz.Prayerbook.SystemGuid.Block.BOOK_MANAGEMENT_BLOCK );
            RockMigrationHelper.DeleteBlockType( com.centralaz.Prayerbook.SystemGuid.BlockType.BOOK_MANAGEMENT_BLOCKTYPE );
            RockMigrationHelper.DeletePage( com.centralaz.Prayerbook.SystemGuid.Page.BOOK_MANAGEMENT_PAGE ); //  Page: Book Management

            RockMigrationHelper.DeleteDefinedValue( com.centralaz.Prayerbook.SystemGuid.DefinedValue.SAMPLE_MINISTRY_DEFINEDVALUE );
            RockMigrationHelper.DeleteDefinedValue( com.centralaz.Prayerbook.SystemGuid.DefinedValue.SAMPLE_SUBMINISTRY_DEFINEDVALUE );

            RockMigrationHelper.DeleteDefinedType( com.centralaz.Prayerbook.SystemGuid.DefinedType.MINISTRY_DEFINED_TYPE );
            RockMigrationHelper.DeleteDefinedType( com.centralaz.Prayerbook.SystemGuid.DefinedType.SUBMINISTRY_DEFINED_TYPE );

            //Remove Group Type, Group Type Role, and groups
            RockMigrationHelper.DeleteGroup( com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP );
            RockMigrationHelper.DeleteGroup( com.centralaz.Prayerbook.SystemGuid.Group.ADMINISTRATORS_GROUP );
            RockMigrationHelper.DeleteGroup( com.centralaz.Prayerbook.SystemGuid.Group.SECTION_GROUP );
            RockMigrationHelper.DeleteGroup( com.centralaz.Prayerbook.SystemGuid.Group.BOOKS_GROUP );
            RockMigrationHelper.DeleteGroupTypeRole( com.centralaz.Prayerbook.SystemGuid.GroupTypeRole.BOOKS_GROUPTYPE_MEMBER_GROUPTYPEROLE );
            RockMigrationHelper.DeleteGroupTypeRole( com.centralaz.Prayerbook.SystemGuid.GroupTypeRole.CONTAINER_GROUPTYPE_MEMBER_GROUPTYPEROLE );
            RockMigrationHelper.DeleteGroupType( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE );
            RockMigrationHelper.DeleteGroupType( com.centralaz.Prayerbook.SystemGuid.GroupType.CONTAINER_GROUPTYPE );
        }

        #region HTMLContent

        /// <summary>
        /// Add HTML Content to an HtmlContent Block on a Page
        /// </summary>
        /// <param name="blockGuid">The Guid of the Block Instance to whick you want to add Html Content</param>
        /// <param name="entityValue">The Entity Value</param>
        /// <param name="version">The content version</param>
        /// <param name="content">The Content to add, as a string</param>
        /// <param name="isApproved">Bool isApproved content?</param>
        /// <param name="approvedByPersonAliasId">Who approved by</param>
        /// <param name="approvedDateTime">When approved</param>
        /// <param name="startDateTime">Start time to display content</param>
        /// <param name="expireDateTime">When content expires</param>
        /// <param name="guid">Guid for this entry</param>
        public void AddHtmlContent( string blockGuid, string entityValue, int version, string content, bool isApproved, int? approvedByPersonAliasId, DateTime? approvedDateTime, DateTime? startDateTime, DateTime? expireDateTime, string guid )
        {
            Sql( String.Format( @"
                        DECLARE @blockId int = (SELECT Top 1 [Id] FROM [Block] WHERE [Guid] = '{0}' )
        
                        INSERT INTO [dbo].[HtmlContent]
                        ([BlockId],
                        [EntityValue],
                        [Version],
                        [Content],
                        [IsApproved],
                        [ApprovedByPersonAliasId],
                        [ApprovedDateTime],
                        [StartDateTime],
                        [ExpireDateTime],
                        [Guid])
                            VALUES
                        (@blockId,
                        '{1}',
                        {2},
                        '{3}',
                        {4},
                        {5},
                        {6},
                        {7},
                        {8},
                        '{9}')",
                    blockGuid,
                    entityValue,
                    version,
                    content.Replace( "'", "''" ),
                    ( isApproved ? "1" : "0" ),
                    approvedByPersonAliasId.HasValue ? approvedByPersonAliasId.Value.ToString() : "NULL",
                    approvedDateTime.HasValue ? String.Format( "'{0}'", approvedDateTime.Value ) : "NULL",
                    startDateTime.HasValue ? String.Format( "'{0}'", startDateTime.Value ) : "NULL",
                    expireDateTime.HasValue ? String.Format( "'{0}'", expireDateTime.Value ) : "NULL",
                    guid
                )
            );
        }

        #endregion

        #region Group Type

        /// <summary>
        /// Adds GroupTypeAssociations so that Groups of GroupType and can child Groups of ChildGroupType
        /// </summary>
        /// <param name="groupTypeGuid">The Guid (as a string) of the parent GroupType</param>
        /// <param name="childGroupTypeGuid">The Guid (as a string) of the child GroupType</param>
        public void UpdateGroupTypeAssociation( string groupTypeGuid, string childGroupTypeGuid )
        {
            Sql( string.Format( @"
                DECLARE @GroupTypeId int = ( SELECT Top 1 [Id] FROM [GroupType] WHERE [Guid] = '{0}' )
                DECLARE @ChildGroupTypeId int = (SELECT Top 1 [Id] FROM [GroupType] WHERE [Guid] = '{1}' )
                
                INSERT INTO [dbo].[GroupTypeAssociation]
                ([GroupTypeId],
                [ChildGroupTypeId])
                    VALUES
                (@GroupTypeId,
                @ChildGroupTypeId)",
                groupTypeGuid,
                childGroupTypeGuid
                )
            );
        }

        #endregion

        #region Group Member Attributes

        /// <summary>
        /// Add or updates a group member Attribute for the given GroupType to hold a particular text field
        /// </summary>
        public void UpdateGroupTypeGroupMemberAttributeMemo( string groupTypeGuid, string name, string description, int order, string defaultValue, bool isGridColumn, bool isMultiValue, bool isRequired, string guid, bool isSystem = true )
        {
            Sql( string.Format( @"
                -- Add group member attribute for a group type that holds text.

                DECLARE @GroupTypeId int = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{0}')
                DECLARE @GroupMemberEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{9}')

                IF EXISTS (
                    SELECT [Id] 
                    FROM [Attribute] 
                    WHERE [EntityTypeId] = @GroupMemberEntityTypeId
                    AND [EntityTypeQualifierColumn] = '{8}'
                    AND [EntityTypeQualifierValue] = CONVERT(NVARCHAR, @GroupTypeId)
                    AND [Key] = '{2}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [IsGridColumn] = '{14}',
                        [DefaultValue] = '{6}',
                        [IsMultiValue] = '{10}',
                        [IsRequired] = '{11}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @GroupMemberEntityTypeId
                    AND [EntityTypeQualifierColumn] = '{8}'
                    AND [EntityTypeQualifierValue] = CONVERT(NVARCHAR, @GroupTypeId)
                    AND [Key] = '{2}'
                END
                ELSE
                BEGIN                
                    INSERT INTO [Attribute] (
                        [IsSystem],
                        [FieldTypeId],
                        [EntityTypeId],
                        [EntityTypeQualifierColumn],
                        [EntityTypeQualifierValue],
                        [Key],
                        [Name],
                        [Description],
                        [Order],
                        [IsGridColumn],
                        [DefaultValue],
                        [IsMultiValue],
                        [IsRequired],
                        [Guid],
                        [CreatedDateTime])
                    VALUES(
                        {12},
                        21,
                        @GroupMemberEntityTypeId,
                        '{8}',
                        CONVERT(NVARCHAR, @GroupTypeId),
                        '{2}',
                        '{3}',
                        '{4}',
                        {5},
                        {14},
                        '{6}',
                        {10},
                        {11},
                        '{7}',
                        GETDATE() )
                END
",
                    groupTypeGuid,
                    Rock.SystemGuid.FieldType.DEFINED_VALUE,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid,
                    "GroupTypeId",
                    "49668B95-FEDC-43DD-8085-D2B0D6343C48",
                    ( isMultiValue ? "1" : "0" ),
                    ( isRequired ? "1" : "0" ),
                    ( isSystem ? "1" : "0" ),
                    ( isMultiValue ? "True" : "False" ),
                    ( isGridColumn ? "1" : "0" )
                    )
            );
        }


        /// <summary>
        /// Add or updates a group member Attribute for the given group type to hold a particular defined value
        /// that is constrained by the given defined type.
        /// </summary>
        public void UpdateGroupTypeGroupMemberAttributeDefinedValue( string groupTypeGuid, string name, string description, int order, string defaultValue, bool isGridColumn, bool isMultiValue, bool isRequired, string definedTypeGuid, string guid, bool isSystem = true )
        {
            Sql( string.Format( @"
                -- Add group member attribute for a group that holds a particular defined value (constrained by a defined type).

                DECLARE @GroupTypeId int = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{0}')
                DECLARE @GroupMemberEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{9}')
                DECLARE @DefinedValueFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                IF EXISTS (
                    SELECT [Id] 
                    FROM [Attribute] 
                    WHERE [EntityTypeId] = @GroupMemberEntityTypeId
                    AND [EntityTypeQualifierColumn] = '{8}'
                    AND [EntityTypeQualifierValue] = CONVERT(NVARCHAR, @GroupTypeId)
                    AND [Key] = '{2}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [IsGridColumn] = '{15}',
                        [DefaultValue] = '{6}',
                        [IsMultiValue] = '{10}',
                        [IsRequired] = '{11}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @GroupMemberEntityTypeId
                    AND [EntityTypeQualifierColumn] = '{8}'
                    AND [EntityTypeQualifierValue] = CONVERT(NVARCHAR, @GroupTypeId)
                    AND [Key] = '{2}'
                END
                ELSE
                BEGIN                
                    INSERT INTO [Attribute] (
                        [IsSystem],
                        [FieldTypeId],
                        [EntityTypeId],
                        [EntityTypeQualifierColumn],
                        [EntityTypeQualifierValue],
                        [Key],
                        [Name],
                        [Description],
                        [Order],
                        [IsGridColumn],
                        [DefaultValue],
                        [IsMultiValue],
                        [IsRequired],
                        [Guid],
                        [CreatedDateTime])
                    VALUES(
                        {12},
                        @DefinedValueFieldTypeId,
                        @GroupMemberEntityTypeId,
                        '{8}',
                        CONVERT(NVARCHAR, @GroupTypeId),
                        '{2}',
                        '{3}',
                        '{4}',
                        {5},
                        {15},
                        '{6}',
                        {10},
                        {11},
                        '{7}',
                        GETDATE() )
                END

                -- Add/Update the 'allowmultiple' and 'definedtype' attribute qualifiers

                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{7}')
                DECLARE @DefinedTypeId int = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{13}')
                
                IF NOT EXISTS( SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @AttributeId AND [Key] = 'allowmultiple' )
                BEGIN
                    INSERT INTO [AttributeQualifier] (
                        [IsSystem],
                        [AttributeId],
                        [Key],
                        [Value],
                        [Guid])
                    VALUES(
                        {12},
                        @AttributeId,
                        'allowmultiple',
                        '{14}',
                        NEWID() )
                END
                ELSE
                BEGIN
                    UPDATE [AttributeQualifier] SET
                        [Key] = 'allowmultiple',
                        [Value] = '{14}'
                    WHERE [AttributeId] = @AttributeId AND [Key] = 'allowmultiple'
                END

                IF NOT EXISTS( SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @AttributeId AND [Key] = 'definedtype' )
                BEGIN
                    INSERT INTO [AttributeQualifier] (
                        [IsSystem],
                        [AttributeId],
                        [Key],
                        [Value],
                        [Guid])
                    VALUES(
                        {12},
                        @AttributeId,
                        'definedtype',
                        CONVERT(NVARCHAR, @DefinedTypeId),
                        NEWID() )
                END
                ELSE
                BEGIN
                    UPDATE [AttributeQualifier] SET
                        [Key] = 'definedtype',
                        [Value] = CONVERT(NVARCHAR, @DefinedTypeId)
                    WHERE [AttributeId] = @AttributeId AND [Key] = 'definedtype'
                END
",
                    groupTypeGuid,
                    Rock.SystemGuid.FieldType.DEFINED_VALUE,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid,
                    "GroupTypeId",
                    "49668B95-FEDC-43DD-8085-D2B0D6343C48",
                    ( isMultiValue ? "1" : "0" ),
                    ( isRequired ? "1" : "0" ),
                    ( isSystem ? "1" : "0" ),
                    definedTypeGuid,
                    ( isMultiValue ? "True" : "False" ),
                    ( isGridColumn ? "1" : "0" )
                    )
            );
        }

        #endregion

        #region Unused Methods

        /// <summary>
        /// Add or updates a group member Attribute for the given group to hold a particular defined value
        /// that is constrained by the given defined type.
        /// Keeping this one around for Nick, in case it has value as a future Helper Method
        /// </summary>
        //        public void UpdateGroupMemberAttributeDefinedValue( string groupGuid, string name, string description, int order, string defaultValue, bool isGridColumn, bool isMultiValue, bool isRequired, string definedTypeGuid, string guid, bool isSystem = true )
        //        {
        //            Sql( string.Format( @"
        //                -- Add group member attribute for a group that holds a particular defined value (constrained by a defined type).
        //
        //                DECLARE @GroupId int = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')
        //                DECLARE @GroupMemberEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{9}')
        //                DECLARE @DefinedValueFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')
        //
        //                IF EXISTS (
        //                    SELECT [Id] 
        //                    FROM [Attribute] 
        //                    WHERE [EntityTypeId] = @GroupMemberEntityTypeId
        //                    AND [EntityTypeQualifierColumn] = '{8}'
        //                    AND [EntityTypeQualifierValue] = CONVERT(NVARCHAR, @GroupId)
        //                    AND [Key] = '{2}' )
        //                BEGIN
        //                    UPDATE [Attribute] SET
        //                        [Name] = '{3}',
        //                        [Description] = '{4}',
        //                        [Order] = {5},
        //                        [IsGridColumn] = '{15}',
        //                        [DefaultValue] = '{6}',
        //                        [IsMultiValue] = '{10}',
        //                        [IsRequired] = '{11}',
        //                        [Guid] = '{7}'
        //                    WHERE [EntityTypeId] = @GroupMemberEntityTypeId
        //                    AND [EntityTypeQualifierColumn] = '{8}'
        //                    AND [EntityTypeQualifierValue] = CONVERT(NVARCHAR, @GroupId)
        //                    AND [Key] = '{2}'
        //                END
        //                ELSE
        //                BEGIN                
        //                    INSERT INTO [Attribute] (
        //                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
        //                        [Key],[Name],[Description],
        //                        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
        //                        [Guid],[CreatedDateTime])
        //                    VALUES(
        //                        {12},@DefinedValueFieldTypeId,@GroupMemberEntityTypeId,'{8}',CONVERT(NVARCHAR, @GroupId),
        //                        '{2}','{3}','{4}',
        //                        {5},{15},'{6}',{10},{11},
        //                        '{7}', GETDATE() )
        //                END
        //
        //                -- Add/Update the 'allowmultiple' and 'definedtype' attribute qualifiers
        //
        //                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{7}')
        //                DECLARE @DefinedTypeId int = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{13}')
        //                
        //                IF NOT EXISTS( SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @AttributeId AND [Key] = 'allowmultiple' )
        //                BEGIN
        //                    INSERT INTO [AttributeQualifier] (
        //                        [IsSystem],[AttributeId],[Key],[Value],[Guid])
        //                    VALUES(
        //                       {12},@AttributeId,'allowmultiple','{14}',NEWID() )
        //                END
        //                ELSE
        //                BEGIN
        //                    UPDATE [AttributeQualifier] SET
        //                        [Key] = 'allowmultiple',
        //                        [Value] = '{14}'
        //                    WHERE [AttributeId] = @AttributeId AND [Key] = 'allowmultiple'
        //                END
        //
        //                IF NOT EXISTS( SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @AttributeId AND [Key] = 'definedtype' )
        //                BEGIN
        //                    INSERT INTO [AttributeQualifier] (
        //                        [IsSystem],[AttributeId],[Key],[Value],[Guid])
        //                    VALUES(
        //                       {12},@AttributeId,'definedtype',CONVERT(NVARCHAR, @DefinedTypeId),NEWID() )
        //                END
        //                ELSE
        //                BEGIN
        //                    UPDATE [AttributeQualifier] SET
        //                        [Key] = 'definedtype',
        //                        [Value] = CONVERT(NVARCHAR, @DefinedTypeId)
        //                    WHERE [AttributeId] = @AttributeId AND [Key] = 'definedtype'
        //                END
        //",
        //                    groupGuid,
        //                    Rock.SystemGuid.FieldType.DEFINED_VALUE,
        //                    name.Replace( " ", string.Empty ),
        //                    name,
        //                    description.Replace( "'", "''" ),
        //                    order,
        //                    defaultValue,
        //                    guid,
        //                    "GroupId",
        //                    "49668B95-FEDC-43DD-8085-D2B0D6343C48",
        //                    ( isMultiValue ? "1" : "0" ),
        //                    ( isRequired ? "1" : "0" ),
        //                    ( isSystem ? "1" : "0" ),
        //                    definedTypeGuid,
        //                    ( isMultiValue ? "True" : "False" ),
        //                    ( isGridColumn ? "1" : "0" )
        //                )
        //            );
        //        }

        #endregion
    }
}
