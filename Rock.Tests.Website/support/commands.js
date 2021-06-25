// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
//
// -- This is a parent command --
// Cypress.Commands.add('login', (email, password) => { ... })
Cypress.Commands.add('login', () => {
    cy.visit('/page/3')
    cy.get('input[id$=_tbUserName]').type('admin')
    cy.get('input[id$=_tbPassword]').type('admin')
    cy.get('input[type=submit][id$=_btnLogin]').click()
    Cypress.Cookies.preserveOnce('ASP.NET_SessionId', '.ROCK')
})
