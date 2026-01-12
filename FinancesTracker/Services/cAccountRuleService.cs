using FinancesTracker.Data;
using FinancesTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Services;

public class cAccountRuleService {
  private readonly FinancesTrackerDbContext mDBContext;
  private List<cAccountRule>? mCachedRules;
  private DateTime mLastCacheUpdate = DateTime.MinValue;
  private readonly TimeSpan mCacheDuration = TimeSpan.FromMinutes(5);

  public cAccountRuleService(FinancesTrackerDbContext xDBContext) {
    mDBContext = xDBContext;
  }

  public async Task<int?> MatchAccountAsync(string xTransactionDescription) {
    var pCln_Rules = await GetCachedRulesAsync();

    foreach (var pRule in pCln_Rules) {
      if (!string.IsNullOrWhiteSpace(pRule.Keyword) && 
          xTransactionDescription.Contains(pRule.Keyword, StringComparison.OrdinalIgnoreCase)) {
        return pRule.AccountId;
      }
    }

    return null;
  }

  private async Task<List<cAccountRule>> GetCachedRulesAsync() {
    if (mCachedRules == null || DateTime.UtcNow - mLastCacheUpdate > mCacheDuration) {
      mCachedRules = await mDBContext.AccountRules
        .Where(ar => ar.IsActive)
        .OrderBy(ar => ar.Keyword)
        .ToListAsync();
      mLastCacheUpdate = DateTime.UtcNow;
    }

    return mCachedRules;
  }

  public void InvalidateCache() {
    mCachedRules = null;
  }
}
