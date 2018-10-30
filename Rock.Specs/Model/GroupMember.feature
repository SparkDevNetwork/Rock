Feature: GroupMember
	I want to ensure that the helper methods in the 
	GroupMember service class are working correctly

@mytag
Scenario Outline: Create Known Relationships
	When I call CreateKnownRelationship with <personId> and <relatedPersonId> and <knownRelationshipRoleId>
	Then the relationship and an inverse relationship of <inverseRelationshipRoleId> should be created for both people
Examples: 
| personId | relatedPersonId | knownRelationshipRoleId | inverseRelationshipRoleId |
| 4        | 21              | 12                      | 12                       |