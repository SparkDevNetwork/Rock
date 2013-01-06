<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OneTimeGift.ascx.cs" Inherits="RockWeb.Blocks.Finance.OneTimeGift" %>

<asp:UpdatePanel ID="upOneTimeGift" runat="server">
 <ContentTemplate>

    <asp:Panel ID="pnlGift" runat="server">
        
        <div class="row-fluid">     
                      
            <div class="span6">
             
                <fieldset>
                    <Rock:DataDropDownList ID="ddlCampusList" runat="server" SourceTypeName="Rock.Model.Campus, Rock" LabelText="Select campus:"/>
                    <Rock:DataTextBox ID="tbGiftAmount"  runat="server" SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Amount" LabelText="Enter amount:" Text="0.00"/>
					<Rock:DataDropDownList ID="ddlFund" runat="server" SourceTypeName="Rock.Model.Fund, Rock" LabelText="Select fund:"/>
                
                </fieldset>
            </div>

            <div class="span3">
                <fieldset>
                    <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="GivenName" />
                    <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" />
                    <Rock:DataTextBox ID="tbAddress" runat="server" SourceTypeName="Rock.Model.Location" PropertyName="Street1" />
                    <Rock:DataTextBox ID="tbCity" runat="server" SourceTypeName="Rock.Model.Location" PropertyName="City" />
                    <Rock:DataDropDownList ID="ddlState" runat="server" SourceTypeName="Rock.Model.Location" PropertyName="State"/>
                    <Rock:DataTextBox ID="tbZip" runat="server" SourceTypeName="Rock.Model.Location" PropertyName="Zip"/>
                    <Rock:DataTextBox ID="tbEmail" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="Email" />

                </fieldset>
            </div>

        </div>

        <div class="actions">
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" />
        </div>

    </asp:Panel>       

    <asp:HiddenField ID="hfCampusId" runat="server" />

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

