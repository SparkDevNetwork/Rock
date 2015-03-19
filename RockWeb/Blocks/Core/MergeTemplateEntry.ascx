<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MergeTemplateEntry.ascx.cs" Inherits="RockWeb.Blocks.Core.MergeTemplateEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="btnMerge" />
    </Triggers>
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlEntry" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfEntitySetId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-files-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" Text="Create Merge Document" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblInfo" runat="server" LabelType="Info" Text="" />
                </div>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />


                <fieldset>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NotificationBox ID="nbNumberOfRecords" runat="server" NotificationBoxType="Info" />
                            <Rock:RockCheckBox ID="cbCombineFamilyMembers" runat="server" Text="Combine Family Members" Help="Set this to true to include the family members of the selected people" />
                            <Rock:RockDropDownList ID="ddlMergeTemplate" runat="server" Label="Merge Template" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:HelpBlock ID="hbShowDataRows" runat="server" Text="TODO" />
                            <Rock:HelpBlock ID="hbShowMergeFields" runat="server" Text="TODO" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnMerge" runat="server" Text="Merge" CssClass="btn btn-primary" OnClick="btnMerge_Click" />
                    </div>
                </fieldset>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
