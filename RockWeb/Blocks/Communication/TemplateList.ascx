<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateList.ascx.cs" Inherits="RockWeb.Blocks.Communication.TemplateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment-o"></i> Communication Template List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:PersonPicker ID="ppCreatedBy" runat="server" Label="Created By" Help="The person who created the template." />
                        <Rock:ComponentPicker ID="cpMedium" runat="server" ContainerType="Rock.Communication.MediumContainer, Rock" Label="Medium" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gCommunication" runat="server" AllowSorting="true" TooltipField="Description" OnRowSelected="gCommunication_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" SortExpression="Subject" HeaderText="Name" />
                            <Rock:RockBoundField DataField="Subject" SortExpression="Subject" HeaderText="Subject" />
                            <Rock:RockBoundField DataField="MediumEntityType.FriendlyName" SortExpression="MediumEntityType.FriendlyName" HeaderText="Medium" />
                            <Rock:RockBoundField DataField="CreatedByPersonAlias.Person.FullName" SortExpression="CreatedByPersonAlias.Person.FullName" HeaderText="Created By" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gCommunication_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        

    </ContentTemplate>
</asp:UpdatePanel>
