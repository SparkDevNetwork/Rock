﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockProperties.ascx.cs" Inherits="RockWeb.Blocks.Core.BlockProperties" %>

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
                        <Rock:NotificationBox ID="nbGridWarning" runat="server" Text="With great power comes great responsibility! This feature allows you to easily display additional information but be aware for large grids and complex lava that will come with a performance penalty."></Rock:NotificationBox>
                        <Rock:Toggle ID="tglEnableStickyHeader" runat="server" Label="Enable Sticky Header" OnText="Yes" OffText="No" Checked="false" Help="If set to yes, all the table headers will stay at the top of the window when scrolling." />
                        <Rock:RockControlWrapper ID="rcwCustomGridColumns" runat="server" Label="Custom Columns">
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
                    </asp:Panel>

                    <asp:Placeholder ID="phCustomSettings" runat="server" />

                </div>

            </asp:PlaceHolder>

        </ContentTemplate>
    </asp:UpdatePanel>

</div>

