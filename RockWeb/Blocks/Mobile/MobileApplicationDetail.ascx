<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileApplicationDetail.ascx.cs" Inherits="RockWeb.Blocks.Mobile.MobileApplicationDetail" %>

<style>
    .mobile-app-preview {
        padding: 20px;
        border: 1px solid #bbb;
        border-radius: 18px;
    }
    .mobile-app-preview img {
        width: 100%;
        height: auto;
        display: block;
        min-height: 50px;
        background-color: #ddd;
    }
    .mobile-app-icon {
        width: 100%;
        height: auto;
        display: block;
    }
</style>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlOverview" runat="server" CssClass="panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <i class="fa fa-mobile"></i>
                    <asp:Literal ID="ltAppName" runat="server" />

                    <div class="panel-labels">
                        <span class="label label-default">Site Id: <asp:Literal ID="lSiteId" runat="server" /></span>
                    </div>
                </h3>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlContent" runat="server">
                    <asp:HiddenField ID="hfSiteId" runat="server" />

                    <div class="row">
                        <div class="col-md-6 col-lg-8">
                            <ul class="nav nav-tabs margin-b-lg">
                                <li id="liTabApplication" runat="server">
                                    <asp:LinkButton ID="lbTabApplication" runat="server" OnClick="lbTabApplication_Click">Application</asp:LinkButton>
                                </li>
                                <li id="liTabLayouts" runat="server">
                                    <asp:LinkButton ID="lbTabLayouts" runat="server" OnClick="lbTabLayouts_Click">Layouts</asp:LinkButton>
                                </li>
                                <li id="liTabPages" runat="server">
                                    <asp:LinkButton ID="lbTabPages" runat="server" OnClick="lbTabPages_Click">Pages</asp:LinkButton>
                                </li>
                            </ul>

                            <asp:Panel ID="pnlApplication" runat="server">
                                <fielset>
                                    <p>
                                        <asp:Literal ID="ltDescription" runat="server" />
                                    </p>

                                    <div>
                                        <asp:Literal ID="ltAppDetails" runat="server" />
                                    </div>
                                </fielset>
                            </asp:Panel>

                            <asp:Panel ID="pnlLayouts" runat="server">
                                <Rock:Grid ID="gLayouts" runat="server" RowItemText="Layout" DisplayType="Light" OnGridRebind="gLayouts_GridRebind" OnRowSelected="gLayouts_RowSelected">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" SortExpression="Name" HeaderText="Name" />
                                        <Rock:RockBoundField DataField="Description" SortExpression="Description" HeaderText="Description" />
                                        <Rock:DeleteField OnClick="gLayouts_DeleteClick" />
                                    </Columns>
                                </Rock:Grid>
                            </asp:Panel>

                            <asp:Panel ID="pnlPages" runat="server">
                                <Rock:Grid ID="gPages" runat="server" RowItemText="Page" DisplayType="Light" OnGridRebind="gPages_GridRebind" OnRowSelected="gPages_RowSelected" OnGridReorder="gPages_GridReorder">
                                    <Columns>
                                        <Rock:ReorderField />
                                        <Rock:RockBoundField DataField="InternalName" SortExpression="Name" HeaderText="Name" />
                                        <Rock:RockBoundField DataField="LayoutName" SortExpression="LayoutName" HeaderText="Layout" />
                                        <Rock:BoolField DataField="DisplayInNav" SortExpression="DisplayInNav" HeaderText="Display In Nav" />
                                        <Rock:DeleteField OnClick="gPages_DeleteClick" />
                                    </Columns>
                                </Rock:Grid>
                            </asp:Panel>
                        </div>

                        <div class="col-md-6 col-lg-4 hidden-sm hidden-xs">
                            <div class="row">
                                <div class="col-sm-4">
                                    <asp:Image ID="imgAppIcon" runat="server" CssClass="mobile-app-icon" />
                                </div>

                                <div class="col-sm-8">
                                    <asp:Panel ID="pnlPreviewImage" runat="server" CssClass="mobile-app-preview">
                                        <asp:Image ID="imgAppPreview" runat="server" />
                                    </asp:Panel>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="actions margin-t-md">
                        <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-primary" Text="Edit" OnClick="lbEdit_Click" AccessKey="m" ToolTip="Alt+m" />
                        <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbCancel_Click" CausesValidation="false" AccessKey="c" ToolTip="Alt+c" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEdit" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbEditName" runat="server" Label="Application Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbEditActive" runat="server" Label="Active" />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbEditDescription" runat="server" Label="Description" TextMode="MultiLine" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rblEditApplicationType" runat="server" Label="Application Type" RepeatDirection="Horizontal" OnSelectedIndexChanged="rblEditApplicationType_SelectedIndexChanged" AutoPostBack="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rblEditAndroidTabLocation" runat="server" Label="Android Tab Location" RepeatDirection="Horizontal" Help="Determines where the Tab bar should be displayed on Android. iOS will always display at the bottom." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PagePicker ID="ppEditLoginPage" runat="server" Label="Login Page" />
                        </div>

                        <div class="col-md-6">
                            <Rock:PagePicker ID="ppEditProfilePage" runat="server" Label="Profile Page" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbEditApiKey" runat="server" Label="API Key" Required="true" />
                        </div>

                        <div class="col-md-6">
                            <Rock:CategoryPicker ID="cpEditPersonAttributeCategories" runat="server" Label="Person Attribute Categories" Help="All attributes in selected categories will be sent to the client and made available remotely."  AllowMultiSelect="true" />
                        </div>
                    </div>

                    <Rock:CodeEditor ID="ceEditCssStyles" runat="server" Label="CSS Styles" Help="CSS Styles to apply to UI elements." EditorMode="Css" />

                    <Rock:CodeEditor ID="ceEditFlyoutXaml" runat="server" Label="Flyout Xaml" Help="The XAML template to use for the menu in the Flyout Shell." EditorMode="Xml" Required="true" />

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:ColorPicker ID="cpEditBarBackgroundColor" runat="server" Label="Bar Background Color" Help="Override the default title bar background color provided by the mobile OS." />
                        </div>
                        <div class="col-md-4">
                            <Rock:ColorPicker ID="cpEditMenuButtonColor" runat="server" Label="Menu Button Color" Help="The color of the menu button in the title bar." />
                        </div>
                        <div class="col-md-4">
                            <Rock:ColorPicker ID="cpEditActivityIndicatorColor" runat="server" Label="Activity Indicator Color" Help="Defines the color that will be used when displaying an activity indicator, these alert the user that something is happening in the background." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:ImageUploader ID="imgEditHeaderImage" runat="server" Label="Header Image" Help="The image that appears on the top header. While the size is dependent on design we recommend a height of 120px and minimum width of 560px." />
                        </div>

                        <div class="col-md-4">
                            <Rock:ImageUploader ID="imgEditPreviewThumbnail" runat="server" Label="Preview Thumbnail" Help="Preview thumbnail to be used by Rock to distinguish application." />
                        </div>
                    </div>

                    <div class="actions margin-t-md">
                        <asp:LinkButton ID="lbEditSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbEditSave_Click" AccessKey="s" ToolTip="Alt+s" />
                        <asp:LinkButton ID="lbEditCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbEditCancel_Click" CausesValidation="false" AccessKey="c" ToolTip="Alt+c" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>