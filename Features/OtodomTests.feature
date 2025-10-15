Feature: Otodom Website Tests
    Scenario: Complete Otodom.pl automation for Warszawa
    Given I open the Otodom main page and verify it loads correctly
    When I pass through the authorization process with valid credentials
    Then I verify the user is authorized and main page is opened

    When I select location "<location>" and price filters <minPrice> to <maxPrice> and click Search
    Then I verify results page opened and apartment prices are in the selected range

    When I remove price range filters
    And I get max and min apartment surface values from the first page and apply surface filter
    Then I verify results page opened and apartment surfaces are in the selected range

    When I get a random offer and save price, rooms, surface to scenario context
    And I click on the selected offer
    Then I verify the offer page opened and price, rooms, surface match the saved context

    Examples:
    | location | minPrice | maxPrice |
    | Warszawa | 500000   | 800000   |