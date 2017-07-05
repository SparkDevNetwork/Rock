<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LavaShortcodeDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.LavaShortcodeDetail" %>

<asp:UpdatePanel ID="upLavaShortcodeDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfLavaShortcodeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-cube"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <div id="pnlEditDetails" runat="server">
                    <fieldset>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbLavaShortcodeName" runat="server" SourceTypeName="Rock.Model.LavaShortcode, Rock" PropertyName="Name" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbTagName" runat="server" Help="This will be the name of the shortcode when used in Lava." SourceTypeName="Rock.Model.LavaShortcode, Rock" PropertyName="TagName" />
                                <asp:HiddenField ID="hfOriginalTagName" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockRadioButtonList ID="rblTagType" Required="true" runat="server" Help="Block tags require an end tag while inline do not." Label="TagType" RepeatDirection="Horizontal" />
                            </div>
                        </div>

                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.LavaShortcode, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                        <Rock:HtmlEditor ID="htmlDocumentation" runat="server" Label="Documentation" Help="Technical description of the internals of the shortcode" Height="250" />

                        <Rock:CodeEditor ID="ceMarkup" Label="Shortcode Markup" runat="server" EditorHeight="350" Required="true" RequiredErrorMessage="Please provide Lava markup for this shortcode." />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:KeyValueList ID="kvlParameters" runat="server" Label="Parameters" Help="List the parameters for your shortcode. If you provide a value here it will become the default value if none is provided." />
                            </div>
                            <div class="col-md-6">
                                <Rock:LavaCommandsPicker id="lcpLavaCommands" runat="server" Label="Enabled Lava Commands" />
                            </div>
                        </div>

                    </fieldset>
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>

                <fieldset id="fieldsetViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lLayoutDescription" runat="server"></asp:Literal>
                    </p>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                </fieldset>
            </div>


        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
