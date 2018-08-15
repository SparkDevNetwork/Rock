<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PartnershipSignup.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Partnership.PartnershipSignup" %>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <Rock:ModalAlert ID="mdNotLoggedIn" runat="server" />
        
        <Rock:ModalDialog runat="server" ID="mdCampus" Title="Please Select Your Campus" OnSaveClick="mdCampus_OnSaveClick">
            <Content>
                <p>It looks like we don't have your Campus on record.  Please select the NewPointe Campus you attend:</p>
                <Rock:CampusPicker runat="server" ID="cpCampus"/>
            </Content>
        </Rock:ModalDialog>
        
        
        <Rock:ModalDialog runat="server" ID="mdAge" Title="Please Select Your Birthdate" OnSaveClick="mdAge_OnSaveClick">
            <Content>
                <p>It looks like we don't have your birthdate on record.  Please enter your Date of Birth:</p>
                <Rock:DatePicker runat="server" ID="dpBirthDate" Label="Birthdate"/>
            </Content>
        </Rock:ModalDialog>

 
        <asp:Panel ID="pnlSignup" runat="server" CssClass="panel panel-block" Visible="True">
            
            <div class="panel panel-green">
            <div class="panel-heading">
                <h4 class="panel-title">
                    <a data-toggle="collapse" data-target="#collapsePartnership" aria-expanded="true" aria-controls="collapseExample">
                        <i class="fa fa-pencil"></i> Partnership at NewPointe
                    </a>
                </h4>
            </div>
                
                <div class="panel-body collapse in" id="collapsePartnership">

                <h2><asp:Literal runat="server" ID="lYear"></asp:Literal> Partnership Covenant for <asp:Literal runat="server" ID="lPersonInfo"></asp:Literal></h2>
                    
                    
                <asp:Literal runat="server" ID="lPartnershipText" />

                
            </div>
                </div>
                
           <asp:Panel ID="pnlSignature" runat="server" CssClass="panel panel-block" Visible="True">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> NewPointe Partnership Agreement</h1>
            </div>
            <div class="panel-body">

                <br /><br />
                <p>By signing <small>(typing my name in the box)</small> below, I agree to uphold the values of the NewPointe Partnership Covenant.</p>

               <Rock:RockTextBox runat="server" ID="tbSignature" Label="Signature" Required="True" Help="Please Type Your Signature in the Box Below"  />
                
                <Rock:BootstrapButton runat="server" ID="btnSubmit" DataLoadingText="Submitting Partnership Covenant" CssClass="btn btn-newpointe" OnClick="btnSubmit_OnClick">Submit</Rock:BootstrapButton>
                
                
                <br />

            </div>
        </asp:Panel>
                
                
                
                <asp:Panel ID="pnlSuccess" runat="server" CssClass="panel panel-block" Visible="False">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> NewPointe Partnership Agreement</h1>
            </div>
            <div class="panel-body">
                
                <br /><br />
                
                <div class="panel-body">
                
                <h4>Thank you! Your Partnership Covenant has been signed and submitted.</h4>
                
                <div class="clearfix">
                    <div class="pull-left">
                    <a href="#" class="btn btn-primary hidden-print" onClick="window.print();"><i class="fa fa-print"></i> Print Partnership Covenant</a> 
                    </div>
                </div>
                
                <br />


            </div>
        </asp:Panel>

                
         <br/><br /><br/>
        
        
        <asp:Panel ID="pnlOpportunities" runat="server" CssClass="panel panel-block" Visible="False">
            <div class="panel panel-green">
            <div class="panel-heading">
                <h4 class="panel-title">
                    <a data-toggle="collapse" data-target="#collapseOpportunities" aria-expanded="true" aria-controls="collapseExample">
                        <i class="fa fa-pencil"></i> My Partnership Opportunities
                    </a>
                </h4>
            </div>

                <div class="panel-body collapse in" id="collapseOpportunities">
                    <div class="col-md-6" style="outline: 1px solid #4d4d4d; padding: 10px;">
                        <p style="font-family: 'Open Sans','Gotham',Helvetica,Arial,sans-serif;"><strong>Serving:</strong> <asp:Literal runat="server" ID="lServing"></asp:Literal> </p>
                    </div>
                    <div class="col-md-6" style="outline: 1px solid #4d4d4d; padding: 10px;">
                        <p style="font-family: 'Open Sans','Gotham',Helvetica,Arial,sans-serif;"><strong>Giving:</strong> <asp:Literal runat="server" ID="lGiving"></asp:Literal> </p>
                    </div>
                    <div class="col-md-12" style="outline: 1px solid #4d4d4d; padding: 10px;">
                        <p style="font-family: 'Open Sans','Gotham',Helvetica,Arial,sans-serif;"><strong>DISCOVER My Church:</strong> <asp:Literal runat="server" ID="lDiscover"></asp:Literal> </p> 
                    </div>
                </div>

            </div>
        </asp:Panel>
                
                

   
        </asp:Panel>
        
        
        
    

        
        <asp:Panel ID="pnlNotLoggedIn" runat="server" CssClass="panel panel-block" Visible="False">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> NewPointe Partnership Agreement</h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox runat="server" NotificationBoxType="Danger" Text="You must be logged to sign the Partnership Covenant." Visible="True" ID="nbNoPerson" Title="MyNewPointe Login Required"></Rock:NotificationBox>

            </div>
        </asp:Panel>
        
        
        
        <asp:Panel ID="pnlNotSixteen" runat="server" CssClass="panel panel-block" Visible="False">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> NewPointe Partnership Agreement</h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox runat="server" NotificationBoxType="Danger" Text="Sorry, you aren't old enough to sign the NewPointe Partnership Covenant" Visible="True" ID="nbAge" Title="Age Requirement"></Rock:NotificationBox>

            </div>
        </asp:Panel>
        
        
        <asp:Panel ID="pnlContact" runat="server" CssClass="panel panel-block" Visible="True">
            <div class="panel-heading">
            </div>
            <div class="panel-body">

                <p>Got Questions? <a href="https://newpointe.org/moreinfo?topic=Partnership" class="btn btn-newpointe">CONTACT US ABOUT PARTNERSHIP</a></p>

            </div>
        </asp:Panel>


    </ContentTemplate>
</asp:UpdatePanel>


