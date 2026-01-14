using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Services;

public class cInsignificantTransactionDetector {

  private static readonly string[] mInternalTransferKeywords = {
        "przelew wewnętrzny", "przelew własny", "przelew między rachunkami", "transfer wewnętrzny"
    };

  private static readonly string[] mSignificantKeywords = {
        "fv/", "faktura", "fs/", "zapłata za", "paragon"
    };

  private static readonly string[] mCreditCardPaymentKeywords = {
        "spłata karty", "opłacenie karty", "karta kredytowa", "credit card payment"
    };

  private static readonly string[] mTechnicalOperationKeywords = {
        "korekta", "adjustment", "storno", "anulowanie", "zwrot prowizji"
    };

  public bool IsInsignificant(cTransaction_DTO xTransaction, List<string> myAccountIdentifiers) {
    if (xTransaction == null || string.IsNullOrWhiteSpace(xTransaction.Description))
      return false;

    string pDesc = xTransaction.Description.ToLowerInvariant();

    // 1. PRIORYTET: Twoje własne konta (Rozwiązuje paradoks Michał vs Artur)
    // Jeśli wykryjemy numer Twojego konta, to jest to transfer własny (czyli nieistotny statystycznie jako koszt)
    if (myAccountIdentifiers != null && myAccountIdentifiers.Any(id => !string.IsNullOrEmpty(id) && pDesc.Contains(id.ToLower()))) {
      return true;
    }

    // 2. PRIORYTET: Wykluczenia handlowe (Faktury)
    // Jeśli nie znaleźliśmy Twojego konta, a jest FS/ lub FV/, to na 100% jest to istotna transakcja
    if (mSignificantKeywords.Any(k => pDesc.Contains(k))) {
      return false;
    }

    // 3. PRIORYTET: Pozostałe operacje techniczne i karty
    if (mInternalTransferKeywords.Any(k => pDesc.Contains(k))) return true;
    if (mCreditCardPaymentKeywords.Any(k => pDesc.Contains(k))) return true;
    if (mTechnicalOperationKeywords.Any(k => pDesc.Contains(k))) return true;

    return false;
  }
}
