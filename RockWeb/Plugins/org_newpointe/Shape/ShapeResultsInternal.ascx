<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShapeResultsInternal.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Shape.ShapeResultsInternal" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox runat="server" NotificationBoxType="Danger" Text="You must be logged in or access this page from a vaild link." Visible="False" ID="nbNoPerson" Title="No Valid Person"></Rock:NotificationBox>
        
 
        <h1 class="text-center">SHAPE Profile</h1>
        <h2 class="text-center" style="margin-top: -10px;">
            <asp:Label runat="server" ID="lbPersonName"></asp:Label><br />
                <small>Assessment Taken: <asp:Label runat="server" ID="lbAssessmentDate"></asp:Label></small></h2>
        <p class="text-center"><a href="https://rock.newpointe.org/Person/<%= SelectedPerson.Id %>" class="btn btn-primary np-button">Return to Person Profile</a></p>

        
    <div class="panel panel-block"> 
    <div class="panel-heading">
        <h4 class="panel-title"><i class="fa fa-share-square-o"></i> Spiritual Gifts</h4>
    </div>
        <div class="panel panel-body">
            <div class="row">
                <div class="col-md-6">
                    <h5 class="text-center">Spiritual Gift 1: </h5>
                    <h3 class="text-center" style="margin-top: -10px;">
                        <asp:Label runat="server" ID="lbGift1Title"></asp:Label></h3>
                    <asp:Label runat="server" ID="lbGift1BodyHTML"></asp:Label>
                    <hr/>
                </div>

                <div class="col-md-6">
                    <h5 class="text-center">Spiritual Gift 2: </h5>
                    <h3 class="text-center" style="margin-top: -10px;">
                        <asp:Label runat="server" ID="lbGift2Title"></asp:Label></h3>
                    <asp:Label runat="server" ID="lbGift2BodyHTML"></asp:Label>
                    <hr/>
                </div>
            </div>
            
            <div class="row">
                <div class="col-md-6">
                    <h5 class="text-center">Spiritual Gift 3: </h5>
                    <h3 class="text-center" style="margin-top: -10px;">
                        <asp:Label runat="server" ID="lbGift3Title"></asp:Label></h3>
                    <asp:Label runat="server" ID="lbGift3BodyHTML"></asp:Label>
                </div>

                <div class="col-md-6">
                    <h5 class="text-center">Spiritual Gift 4: </h5>
                    <h3 class="text-center" style="margin-top: -10px;">
                        <asp:Label runat="server" ID="lbGift4Title"></asp:Label></h3>
                    <asp:Label runat="server" ID="lbGift4BodyHTML"></asp:Label>
                </div>
                
                <div class="col-md-12 text-center">
                    <a href="/SpiritualGiftDescriptions" class="btn btn-primary np-button">View All Spiritual Gifts</a>
                <br />
            </div>

            </div>
        </div>
        </div>


        <div class="panel panel-block"> 
    <div class="panel-heading">
        <h4 class="panel-title"><i class="fa fa-share-square-o"></i>Heart</h4>
    </div>
        <div class="panel panel-body">
            
            <div class="col-md-4">
                <h3 class="text-center">My Interests: </h3>
                <p><asp:Label runat="server" ID="lbHeartCategories"></asp:Label></p>
            </div>

            <div class="col-md-4">
                <h3 class="text-center">My Causes: </h3>
                <p><asp:Label runat="server" ID="lbHeartCauses"></asp:Label></p>
            </div>
            
            <div class="col-md-4">
                <h3 class="text-center">My Passions: </h3>
                <p><asp:Label runat="server" ID="lbHeartPassion"></asp:Label></p>
            </div>


            <div class="col-md-12">
                <br />
            </div>

        </div>
        </div>
        
        
        

    <div class="panel panel-block"> 
    <div class="panel-heading">
        <h4 class="panel-title"><i class="fa fa-share-square-o"></i>Abilities</h4>
    </div>
        <div class="panel panel-body">
            <div class="col-md-6">
                <h5 class="text-center">Ability 1: </h5>
                <h3 class="text-center" style="margin-top: -10px;">
                    <asp:Label runat="server" ID="lbAbility1Title"></asp:Label></h3>
                <asp:Label runat="server" ID="lbAbility1BodyHTML"></asp:Label>
            </div>

            <div class="col-md-6">
                <h5 class="text-center">Ability 2: </h5>
                <h3 class="text-center" style="margin-top: -10px;">
                    <asp:Label runat="server" ID="lbAbility2Title"></asp:Label></h3>
                <asp:Label runat="server" ID="lbAbility2BodyHTML"></asp:Label>
            </div>


            <div class="col-md-12">
                <br />
            </div>

        </div>
        </div>



    <div class="panel panel-block"> 
    <div class="panel-heading">
        <h4 class="panel-title"><i class="fa fa-share-square-o"></i> Personality (DISC Assessment)</h4>
    </div>
        <div class="panel panel-body">
            
            
            <asp:Panel runat="server" ID="DISCResults" Visible="False">

            <div class="col-md-12" >
                
                                    <ul class="discchart">
                        <li class="discchart-midpoint"></li>
                        <li style="height: 100%; width:0px;"></li>
                        <li id="discNaturalScore_D" runat="server" class="discbar discbar-d">
                            <div class="discbar-label">D</div>
                        </li>
                        <li id="discNaturalScore_I" runat="server" class="discbar discbar-i">
                            <div class="discbar-label">I</div>
                        </li>
                        <li id="discNaturalScore_S" runat="server" class="discbar discbar-s">
                            <div class="discbar-label">S</div>
                        </li>
                        <li id="discNaturalScore_C" runat="server" class="discbar discbar-c">
                            <div class="discbar-label">C</div>
                        </li>
                    </ul>
                

                
                <h3>Description</h3>
                    <asp:Literal ID="lDescription" runat="server"></asp:Literal>

                    <h3>Strengths</h3>
                    <asp:Literal ID="lStrengths" runat="server"></asp:Literal>

                    <h3>Challenges</h3>
                    <asp:Literal ID="lChallenges" runat="server"></asp:Literal>

                    <h3>Under Pressure</h3>
                    <asp:Literal ID="lUnderPressure" runat="server"></asp:Literal>

                    <h3>Motivation</h3>
                    <asp:Literal ID="lMotivation" runat="server"></asp:Literal>

                    <h3>Team Contribution</h3>
                    <asp:Literal ID="lTeamContribution" runat="server"></asp:Literal>

                    <h3>Leadership Style</h3>
                    <asp:Literal ID="lLeadershipStyle" runat="server"></asp:Literal>

                    <h3>Follower Style</h3>
                    <asp:Literal ID="lFollowerStyle" runat="server"></asp:Literal>
                
                
                <br />
            </div>
                
                </asp:Panel>
            
            <asp:Panel runat="server" ID="NoDISCResults" Visible="True">

            <div class="col-md-12 text-center" >
                
                <p>This person has not yet taken the DISC Assessment.</p>

                
                
                <br />
            </div>
                
                </asp:Panel>


        </div>
        </div>



            <div class="panel panel-block"> 
    <div class="panel-heading">
        <h4 class="panel-title"><i class="fa fa-share-square-o"></i> Experiences</h4>
    </div>
        <div class="panel panel-body">
            <div class="col-md-12">
                <h4 class="text-center">People </h4>
                <p><asp:Label runat="server" ID="lbPeople"></asp:Label><br /></p>
                
                <h4 class="text-center">Places </h4>
                <p><asp:Label runat="server" ID="lbPlaces"></asp:Label><br /></p>
                
                <h4 class="text-center">Events </h4>
                <p><asp:Label runat="server" ID="lbEvents"></asp:Label><br /></p>
            </div>

            <div class="col-md-12">
                <br />
            </div>

        </div>
        </div>




            <div class="panel panel-block"> 
    <div class="panel-heading">
        <h4 class="panel-title"><i class="fa fa-share-square-o"></i> Volunteer Opportunities</h4>
    </div>
        <div class="panel panel-body">

            <div class="col-md-12">
                <h2 class="text-center">Volunteer Opportunities</h2>
                <p>Based on their SHAPE Profile, here are the volunteer opportunities that we recommended to <%= SelectedPerson.NickName %>:</p>

                <asp:Repeater runat="server" ID="rpVolunteerOpportunities">
                    <ItemTemplate>

                        <div class="panel panel-default margin-t-md">
                            <div class="panel-heading clearfix">
                                <h1 class="panel-title pull-left">
                                    <i class='<%# Eval("IconCssClass") %>'></i> <%# Eval("Name") %>
                                </h1>
                            </div>
                            <div class="panel-body">
                                <div class="col-md-12">
                                    <p><%# Eval("Summary") %></p>
                                    <a class="btn btn-default" href="https://newpointe.org/VolunteerOpportunities/<%# Eval("Id") %>" role="button">More Info</a>
                                </div>
                            </div>
                        </div>

                    </ItemTemplate>
                </asp:Repeater>

            </div>


            <div class="col-md-12">
                <br />
            </div>
            
            

        </div>
                </div>
        
        <div class="col-md-12">
                <br />
            </div>


    </ContentTemplate>
</asp:UpdatePanel>
