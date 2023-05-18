<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockProperties.ascx.cs" Inherits="RockWeb.Blocks.Core.BlockProperties" %>

<div class="admin-dialog">

    <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Danger" Dismissable="true" />

    <asp:UpdatePanel ID="upBlockProperties" runat="server">
        <ContentTemplate>

            <asp:PlaceHolder ID="phContent" runat="server">

                <ul class="nav nav-pills" >
                    <asp:Repeater ID="rptProperties" runat="server" >
                        <ItemTemplate >
                            <li class='<%# GetTabClass(Container.DataItem) %>'>
                                <asp:LinkButton ID="lbProperty" runat="server" Text='<%# Container.DataItem %>' OnClick="lbProperty_Click" CausesValidation="false">
                                </asp:LinkButton> 
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>

                <div class="margin-t-md" >

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <asp:Panel ID="pnlBasicProperty" runat="server" Visible="true" >
                        <Rock:DataTextBox ID="tbBlockName" runat="server" SourceTypeName="Rock.Model.Block, Rock" PropertyName="Name" Required="true" />
                        <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />
                        <asp:PlaceHolder ID="phCustomBasicSettings" runat="server" />
                    </asp:Panel>

                    <asp:Panel ID="pnlMobileSettings" runat="server" Visible="false">
                        <Rock:AttributeValuesContainer ID="avcMobileAttributes" runat="server" ShowCategoryLabel="false" />
                    </asp:Panel>

                    <asp:Panel ID="pnlAdvancedSettings" runat="server" Visible="false" >
                        <Rock:RockTextBox ID="tbCssClass" runat="server" Label="CSS Class" Help="An optional CSS class to include with this block's containing div" />
                        <Rock:CodeEditor ID="cePreHtml" runat="server" Label="Pre-HTML" Help="HTML Content to render before the block <span class='tip tip-lava'></span>." EditorMode="Lava" EditorTheme="Rock" EditorHeight="400" />
                        <Rock:CodeEditor ID="cePostHtml" runat="server" Label="Post-HTML" Help="HTML Content to render after the block <span class='tip tip-lava'></span>." EditorMode="Lava" EditorTheme="Rock" EditorHeight="400" />
                        <Rock:RockTextBox ID="tbCacheDuration" runat="server"  Label="Output Cache Duration (seconds)" Help="Number of seconds to cache the output of this block.  If a value is entered here, this block will only process data when the cache expires." />
                        <Rock:AttributeValuesContainer ID="avcAdvancedAttributes" runat="server" ShowCategoryLabel="false" />
                        <asp:PlaceHolder ID="phCustomAdvancedSettings" runat="server" />
                    </asp:Panel>

                    <asp:Panel ID="pnlCustomGridTab" runat="server" Visible="false">
                        <Rock:Toggle ID="tglEnableStickyHeader" runat="server" Label="Enable Sticky Header" OnText="Yes" OffText="No" Checked="false" Help="If set to yes, all the table headers will stay at the top of the window when scrolling." />

                        <Rock:PanelWidget ID="pwCustomActions" runat="server" Title="Custom Actions">
                            <Rock:NotificationBox runat="server" Text="This feature requires that the grid know the entity type of the items displayed. If this information is not available, then the action buttons will not be displayed. Action buttons are also not displayed if the person does not have permission to visit the destination route or the route does not exist." />
                            <Rock:Toggle ID="tglEnableDefaultWorkflowLauncher" runat="server" Label="Enable Workflow Launcher" OnText="Yes" OffText="No" Help="If set to yes, the workflow launcher button will be enabled for the grid." />
                            <Rock:RockControlWrapper ID="rcwCustomActions" runat="server" Label="Custom Actions">
                                <asp:LinkButton ID="lbAddAction" runat="server" CssClass="btn btn-default" Text="Add Actions" OnClick="lbAddCustomAction_Click" />
                                <asp:Repeater ID="rptCustomActions" runat="server" OnItemDataBound="rptCustomActions_ItemDataBound">
                                    <ItemTemplate>
                                        <hr />
                                        <div class="row">
                                            <div class="col-md-11">
                                                <div class="row">
                                                    <div class="col-md-6">
                                                        <Rock:RockTextBox ID="rtbName" runat="server" Label="Name" Help="The name of the action. This should be one or two words." />
                                                    </div>
                                                    <div class="col-md-6">
                                                        <Rock:RockTextBox ID="rtbRoute" runat="server" Label="Route" Help="The route that the user is directed to after clicking the action button. This will be formatted using an EntitySetId in position {0}. If position {0} is not included in this value, then the EntitySetId will be included as a query parameter. Example: /CustomLaunchRoute/{0}" />
                                                    </div>
                                                    <div class="col-md-6">
                                                        <Rock:RockTextBox ID="rtbIcon" runat="server" Label="Icon CSS Class" Help="The class of the icon to be used on the action button. Example: fa fa-cog" />
                                                    </div>
                                                    <div class="col-md-12">
                                                        <Rock:RockTextBox ID="rtbHelp" runat="server" Label="Help Text" Help="The help text shown on mouse-over of the action button icon. Example: Click here to go to the Meal-Train kickoff page." />
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-1">
                                                <asp:LinkButton ID="btnDeleteCustomAction" runat="server" CssClass="btn btn-danger btn-sm" OnClick="btnDeleteCustomAction_Click" >
                                                    <i class="fa fa-times"></i>
                                                </asp:LinkButton>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </Rock:RockControlWrapper>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="pwCustomGridColumns" runat="server" Title="Custom Columns">
                            <Rock:RockControlWrapper ID="rcwCustomGridColumns" runat="server" Label="Custom Columns">
                                <Rock:NotificationBox ID="nbGridWarning" runat="server" Text="With great power comes great responsibility! This feature allows you to easily display additional information but be aware that this could cause performance issues for grids with more than a few dozen rows." CssClass="alert alert-warning"></Rock:NotificationBox>
                                <asp:LinkButton ID="lbAddColumns" runat="server" CssClass="btn btn-default" Text="Add Column" OnClick="lbAddColumns_Click" />
                                <asp:Repeater ID="rptCustomGridColumns" runat="server" OnItemDataBound="rptCustomGridColumns_ItemDataBound">
                                    <ItemTemplate>
                                        <hr />
                                        <div class="row">
                                            <div class="col-md-4">
                                        
                                                    <div class="row">
                                                        <div class="col-md-8">
                                                            <Rock:RockDropDownList ID="ddlOffsetType" runat="server" Label="Column Position" Help="Enter the relative position of the custom column. For example, to make it the 2nd to the last column, select Last Column with an Offset of 1." AutoPostBack="true" OnSelectedIndexChanged="ddlOffsetType_SelectedIndexChanged">
                                                                <asp:ListItem Text="First Column" Value="0" />
                                                                <asp:ListItem Text="Last Column" Value="1" />
                                                            </Rock:RockDropDownList>
                                                        </div>
                                                        <div class="col-md-4">
                                                            <Rock:NumberBox ID="nbRelativeOffset" runat="server" Label="Offset" PrependText="-/+" Required="true" />
                                                        </div>
                                                    </div>
                                        
                                                <Rock:RockTextBox ID="tbHeaderText" runat="server" Label="Header Text" />
                                                <Rock:RockTextBox ID="tbHeaderClass" runat="server" Label="Header Class" />
                                                <Rock:RockTextBox ID="tbItemClass" runat="server" Label="Item Class" />
                                            </div>
                                            <div class="col-md-7">
                                                <Rock:CodeEditor ID="ceLavaTemplate" runat="server" Help="The properties of the item in each row can be accessed using the 'Row' merge field. <span class='tip tip-lava'></span>" Label="Lava Template" EditorMode="Lava" EditorHeight="275" />
                                            </div>
                                            <div class="col-md-1">
                                            <asp:LinkButton ID="btnDeleteColumn" runat="server" CssClass="btn btn-danger btn-sm" OnClick="btnDeleteColumn_Click" ><i class="fa fa-times"></i></asp:LinkButton>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </Rock:RockControlWrapper>
                        </Rock:PanelWidget>

                    </asp:Panel>

                    <asp:Placeholder ID="phCustomSettings" runat="server" />

                </div>

            </asp:PlaceHolder>

        </ContentTemplate>
    </asp:UpdatePanel>

</div>

