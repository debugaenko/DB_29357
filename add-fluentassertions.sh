#!/bin/bash
# Add FluentAssertions package to the project

echo "Adding FluentAssertions package..."
dotnet add package FluentAssertions

echo "FluentAssertions package added successfully!"
echo "You can now use FluentAssertions in your tests:"
echo ""
echo "using FluentAssertions;"
echo ""
echo "// Examples:"
echo "_mainPage.IsOpened().Should().BeTrue(\"Main page should be accessible\");"
echo "apartmentCount.Should().BeGreaterOrEqualTo(1, \"Should find apartments\");"
echo "browser.Driver.Url.Should().Contain(\"otodom.pl\", \"Should be on Otodom domain\");"