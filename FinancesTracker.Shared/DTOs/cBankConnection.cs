using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancesTracker.Shared.DTOs {
  public class cBankConnection {
    public int Id { get; set; }
    public string BankId { get; set; } // np. asz_pl_ip (mBank)
    public string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int AccountId { get; set; } // Powiązanie z Twoim cAccount
  }
}
