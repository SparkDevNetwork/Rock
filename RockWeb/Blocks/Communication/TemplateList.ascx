<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateList.ascx.cs" Inherits="RockWeb.Blocks.Communication.TemplateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment-o"></i>&nbsp;Communication Template List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:PersonPicker ID="ppCreatedBy" runat="server" Label="Created By" Help="The person who created the template." />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gCommunicationTemplates" runat="server" AllowSorting="true" TooltipField="Description" OnRowSelected="gCommunicationTemplates_RowSelected" OnRowDataBound="gCommunicationTemplates_RowDataBound" CssClass="js-grid-communicationtemplate-list">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" SortExpression="Subject" HeaderText="Name" />
                            <Rock:RockBoundField DataField="Subject" SortExpression="Subject" HeaderText="Subject" />
                            <Rock:RockLiteralField ID="lSupports" HeaderText="Supports" />
                            <Rock:RockBoundField DataField="CreatedByPersonAlias.Person.FullName" SortExpression="CreatedByPersonAlias.Person.FullName" HeaderText="Created By" />
                            <Rock:BoolField DataField="IsActive" SortExpression="IsActive" HeaderText="Active" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gCommunicationTemplates_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
