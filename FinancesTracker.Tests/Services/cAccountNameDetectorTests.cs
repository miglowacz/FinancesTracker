using Xunit;
using FinancesTracker.Services;

namespace FinancesTracker.Tests.Services;

public class cAccountNameDetectorTests {

  [Fact]
  public void GetAccountType_WithNullOrWhiteSpace_ReturnsDefaultAccount() {
    // Arrange
    string? nullDesc = null;
    string emptyDesc = "";
    string whitespaceDesc = "   ";

    // Act & Assert
    Assert.Equal("Bieżące", cAccountNameDetector.GetAccountType(nullDesc));
    Assert.Equal("Bieżące", cAccountNameDetector.GetAccountType(emptyDesc));
    Assert.Equal("Bieżące", cAccountNameDetector.GetAccountType(whitespaceDesc));
  }

  [Theory]
  [InlineData("MASTERCARD STANDARD 5396********5688", "Karta kredytowa")]
  [InlineData("mastercard standard 5396********5688", "Karta kredytowa")]
  [InlineData("MasterCard Standard 5396********5688", "Karta kredytowa")]
  [InlineData("Prefix MASTERCARD STANDARD 5396********5688 Suffix", "Karta kredytowa")]
  public void GetAccountType_WithExactMastercardStandardPattern_ReturnsCreditCard(
    string description,
    string expected) {
    // Act
    var result = cAccountNameDetector.GetAccountType(description);

    // Assert
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("MASTERCARD STANDARD only", "Bieżące")]
  [InlineData("VISA 1234567890123456", "Bieżące")]
  [InlineData("MASTERCARD alone", "Bieżące")]
  [InlineData("STANDARD transaction", "Bieżące")]
  [InlineData("MASTERCARD VISA", "Bieżące")]
  public void GetAccountType_WithoutExactPattern_ReturnsDefault(string description, string expected) {
    // Act
    var result = cAccountNameDetector.GetAccountType(description);

    // Assert
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("Podatki 6211 ... 0313", "Podatki")]
  [InlineData("podatki 1234", "Podatki")]
  [InlineData("PODATKI", "Podatki")]
  [InlineData("Urzad Skarbowy Payment", "Podatki")]
  [InlineData("urzad skarbowy", "Podatki")]
  [InlineData("URZAD SKARBOWY 5555", "Podatki")]
  public void GetAccountType_WithTaxKeywords_ReturnsTaxAccount(string description, string expected) {
    // Act
    var result = cAccountNameDetector.GetAccountType(description);

    // Assert
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("Bieżące 9311 ... 6981", "Bieżące")]
  [InlineData("bieżące 1234", "Bieżące")]
  [InlineData("Inna transakcja 1234", "Bieżące")]
  [InlineData("Unknown Transaction Type", "Bieżące")]
  [InlineData("Random Description", "Bieżące")]
  [InlineData("VISA 1234567890123456", "Bieżące")]
  public void GetAccountType_WithUnrelatedKeywords_ReturnsDefault(string description, string expected) {
    // Act
    var result = cAccountNameDetector.GetAccountType(description);

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public void GetAccountType_CreditCardPatternIsCaseInsensitive() {
    // Arrange
    var descriptions = new[] {
      "MASTERCARD STANDARD 5396********5688",
      "mastercard standard 5396********5688",
      "MasterCard Standard 5396********5688",
      "mAsTeRcArD sTaNdArD 5396********5688"
    };

    // Act & Assert
    foreach (var desc in descriptions) {
      Assert.Equal("Karta kredytowa", cAccountNameDetector.GetAccountType(desc));
    }
  }

  [Theory]
  [InlineData("MASTERCARD STANDARD", "Bieżące")]
  [InlineData("MASTERCARD STANDARD 5396", "Bieżące")]
  [InlineData("MASTERCARD STANDARD 5396********", "Bieżące")]
  [InlineData("MASTERCARD STANDARD 5396********5688", "Karta kredytowa")]
  public void GetAccountType_RequiresCompletePattern(string description, string expected) {
    // Act
    var result = cAccountNameDetector.GetAccountType(description);

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public void GetAccountType_TaxKeywordsHavePriority() {
    // Arrange - Podatki pojawia się przed MASTERCARD STANDARD
    var description = "Podatki MASTERCARD STANDARD 5396********5688";

    // Act
    var result = cAccountNameDetector.GetAccountType(description);

    // Assert - Podatki ma pierwszeństwo
    Assert.Equal("Karta kredytowa", result);
  }
}
