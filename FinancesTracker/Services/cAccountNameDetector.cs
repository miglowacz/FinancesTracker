using System;

public class cAccountNameDetector {

  public static string GetAccountType(string transactionDesc) {

    if (string.IsNullOrWhiteSpace(transactionDesc)) {
      return "Bieżące";
    }

    // Sprawdzanie karty kredytowej - dokładne dopasowanie "MASTERCARD STANDARD"
    if (transactionDesc.ToLower().Contains("MASTERCARD STANDARD 5396********5688".ToLower(), StringComparison.OrdinalIgnoreCase)) {
      return "Karta kredytowa";
    }

    // Sprawdzanie konta podatkowego
    if (transactionDesc.ToLower().Contains("Podatki".ToLower(), StringComparison.OrdinalIgnoreCase) ||
        transactionDesc.ToLower().Contains("Urzad Skarbowy".ToLower(), StringComparison.OrdinalIgnoreCase)) {
      return "Podatki";
    }

    // Domyślnie zwracamy bieżące (obsługuje też przypadek "Bieżące")
    return "Bieżące";

  }

  // Przykład użycia
  public static void Main() {

    string[] testCases = {
      "Bieżące 9311 ... 6981",
      "MASTERCARD STANDARD 5396********5688",
      "Podatki 6211 ... 0313",
      "Inna transakcja 1234"
    };

    foreach (var test in testCases) {
      Console.WriteLine($"{test} -> {GetAccountType(test)}");
    }
  }
}
