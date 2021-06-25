/** 
 * These are our custom Rock Utilities for any Cypress stuff we want to be
 * able to use from inside our Cypress tests.
 *
 * Example Usage:
 *    const campaignName = "campaign " + cy.getUniqueName();
 * 
 */
cy.RockUtils = {

    /**
     * This will create a random string in the format of:
     *
     *   yyyymmddhhmmss-0000000 (where 0 are random numbers)
     */
    makeUniqueName: () => {
        
        // Custom date formatter yyyymmddhhmmss
        function formatDate( date ) {

            var year = date.getFullYear(),
                month = date.getMonth() + 1, // month is zero indexed
                day = date.getDate().toString().padStart(2, '0'),
                hour = date.getHours().toString().padStart(2, '0'),
                minute = date.getMinutes().toString().padStart(2, '0'),
                second = date.getSeconds().toString().padStart(2, '0'),
                monthFormatted = month < 10 ? "0" + month : month

            return year + monthFormatted + day + + hour +  minute + second;
    
        }

        return formatDate(new Date()) + '-' + Cypress._.random(0, 1e6)
    },

}
