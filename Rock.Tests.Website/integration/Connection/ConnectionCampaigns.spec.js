describe('CRUD for Connection Campaign entity', () => {
    before(() => {
      // log in only once before any of the tests run.
      // your app will likely set some sort of session cookie.
      // you'll need to know the name of the cookie(s), which you can find
      // in your Resources -> Cookies panel in the Chrome Dev Tools.

      // This login is defined in the support/commands.js
      cy.login()
    })
  

    beforeEach(() => {
        // before each test, we can automatically preserve the
        // 'ASP.NET_SessionId' and '.ROCK' cookies. this means they
        // will not be cleared before the NEXT test starts.
        Cypress.Cookies.preserveOnce('ASP.NET_SessionId', '.ROCK')
    })
  
    it('Should find "People" > and then "Connections" in menu', () => {

        // Find the People option and mouseover
        cy.get('.nav').contains('li[class=title]', 'People').as('PeopleMenu')
            .parent().parent().invoke('mouseover')

        // Get the Connections item and click
        cy.get('.nav').contains('a[role=menu-item]', 'Connections').as('ConnectionsMenu')
            .click()
    })

    it('Should find Campaign Manager under Connections', () => {

        // Verify the My Active Opportunities is showing
        cy.contains('My Active Opportunities')

        // Click on the Connection Types configuration link
        cy.get('a[id$=_lbConnectionTypes]').click()

        // verify Connection Campaigns is available then click.
        cy.get('a[href="/CampaignConfiguration"]').click()

        // add a new Connection Campaign by clicking the first add button
        cy.get('a[id$=_gCampaigns_actionFooterRow_footerGridActions_lbAdd')
            .first().click()
    })

    const campaignName = "campaign " + cy.RockUtils.makeUniqueName();

    // Create
    it('Should be able to fill out new campaign', () => {
        cy.get('input[id$=_tbName]').type( campaignName );

        cy.get('select[id$=_ddlConnectionType]').select('Involvement')
        cy.get('select[id$=_ddlConnectionOpportunity]').select('Usher')

        // Select the Requestor Data View
        cy.get('div[id$=_dvRequestor] .treeview.treeview-items')
        .get('span[id$=_dvRequestor]').click( {force: true} )

        // TODO - this needs to be something other than C000 if possible.
        cy.get('[data-id="C131"] > .rocktree-name').click({force:true})

        cy.get('[data-id="C131"] > .rocktree-icon').click({force:true})

        // TODO - this needs to be something other than 1 if possible.
        cy.get('[data-id="1"] > .rocktree-name').dblclick();

        // select an Opt Out Group
        cy.get('[id$=_gpOptOutGroup] > .picker-label').click();
        cy.get('.rocktree > .rocktree-leaf > .rocktree-name').dblclick()

        // select Create option:
        cy.get('[for$="_rblCreateConnectionRequests_0"] > .label-text').click()

        cy.get('[id$=_nbDailyLimit]').type( 5 );

        // Set Recurrence Settings
        cy.get('[id$=_nbNumberOfDays]').type( 360 );

        cy.get('[id$=_cbPreferPreviousConnector]').click({force:true})
    })

    // Requirement checking
    it('Should error if a Family Limit has not been selected', () => {
        cy.get('[id$=_btnSave]').click()
        cy.contains('Family Limits is required.') 
    })

    // Save
    it('Should save correctly once a Family Limit has been selected', () => {
        // Click first option
        cy.get('[for$="_rblFamilyLimits_0"]').click();

        cy.get('[id$=_btnSave]').click()
        cy.wait(500);
        cy.contains( campaignName ) 
    })

    // Update
    it('Should be able to select, update, save and verify change', () => {
        cy.contains('td', campaignName).click()

        // Change the name
        cy.get('input[id$=_tbName]').type( "-updated" );
        cy.get('[id$=_btnSave]').click()
    })

    // Delete
    it('Should be able to delete the campaign', () => {

        cy.contains('td', campaignName + "-updated")    // gives you the cell 
            .siblings()                                 // gives you all the other cells in the row
            .find('.grid-delete-button')                // finds the delete button
            //.contains('a', 'DELETE')                  // THIS DID NOT WORK
            .click()

        // confirm modal OK
        cy.get('.btn-primary').contains('OK').click();
        
        cy.reload();
        cy.wait(100);
        cy.contains('td', campaignName).should('not.exist');
    })

})
  
