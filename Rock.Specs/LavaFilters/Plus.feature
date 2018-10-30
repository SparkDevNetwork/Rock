Feature: Lava Plus Filter

Scenario: Add two integer numbers
	Given I have a value of 50
	And I have entered 70 as a parameter
	When The lava resolves
	Then the result should be an integer 120 on the screen

Scenario: Add two decimal numbers
	Given I have a value of 12.5
	And I have entered 3.2 as a parameter
	When The lava resolves
	Then the result should be a decimal 15.7 on the screen 