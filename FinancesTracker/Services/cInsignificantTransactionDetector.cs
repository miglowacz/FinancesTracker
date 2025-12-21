using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Services;

public class cInsignificantTransactionDetector {
  //lista słów kluczowych dla przelewów własnych
  private static readonly string[] mInternalTransferKeywords = {
    "przelew wewnętrzny",
    "przelew własny",
    "przelew między rachunkami",
    "przelew na rachunek",
    "z rachunku",
    "na rachunek",
    "transfer wewnętrzny",
    "internal transfer",
    "transfer środków"
  };

  //lista słów kluczowych dla spłat kart kredytowych
  private static readonly string[] mCreditCardPaymentKeywords = {
    "spłata karty",
    "opłacenie karty",
    "rata karty",
    "karta kredytowa",
    "spłata kredytu kartowego",
    "credit card payment",
    "card payment",
    "opłata karty",
    "zapłata karty"
  };

  //lista słów kluczowych dla operacji technicznych
  private static readonly string[] mTechnicalOperationKeywords = {
    "korekta",
    "adjustment",
    "reversal",
    "storno",
    "anulowanie",
    "zwrot prowizji",
    "korekta opłat"
  };

  public bool IsInsignificant(cTransaction_DTO xTransaction) {
    //funkcja sprawdza czy transakcja powinna być oznaczona jako nieistotna
    //xTransaction - dane transakcji do sprawdzenia

    if (xTransaction == null || string.IsNullOrWhiteSpace(xTransaction.Description))
      return false;

    string pDescription = xTransaction.Description.ToLowerInvariant();

    //sprawdź przelewy własne
    if (IsInternalTransfer(pDescription))
      return true;

    //sprawdź spłaty kart kredytowych
    if (IsCreditCardPayment(pDescription))
      return true;

    //sprawdź operacje techniczne
    if (IsTechnicalOperation(pDescription))
      return true;

    return false;

  }

  private static bool IsInternalTransfer(string xDescription) {
    //funkcja sprawdza czy opis transakcji wskazuje na przelew własny
    //xDescription - opis transakcji (lowercase)

    foreach (string pKeyword in mInternalTransferKeywords) {
      if (xDescription.Contains(pKeyword))
        return true;
    }

    return false;

  }

  private static bool IsCreditCardPayment(string xDescription) {
    //funkcja sprawdza czy opis transakcji wskazuje na spłatę karty kredytowej
    //xDescription - opis transakcji (lowercase)

    foreach (string pKeyword in mCreditCardPaymentKeywords) {
      if (xDescription.Contains(pKeyword))
        return true;
    }

    return false;

  }

  private static bool IsTechnicalOperation(string xDescription) {
    //funkcja sprawdza czy opis transakcji wskazuje na operację techniczną
    //xDescription - opis transakcji (lowercase)

    foreach (string pKeyword in mTechnicalOperationKeywords) {
      if (xDescription.Contains(pKeyword))
        return true;
    }

    return false;

  }

  public string GetInsignificanceReason(cTransaction_DTO xTransaction) {
    //funkcja zwraca powód oznaczenia transakcji jako nieistotna
    //xTransaction - dane transakcji

    if (xTransaction == null || string.IsNullOrWhiteSpace(xTransaction.Description))
      return string.Empty;

    string pDescription = xTransaction.Description.ToLowerInvariant();

    if (IsInternalTransfer(pDescription))
      return "Przelew własny między rachunkami";

    if (IsCreditCardPayment(pDescription))
      return "Spłata karty kredytowej";

    if (IsTechnicalOperation(pDescription))
      return "Operacja techniczna";

    return string.Empty;

  }
}
