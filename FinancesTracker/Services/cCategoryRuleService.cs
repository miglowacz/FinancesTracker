using FinancesTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Services {
  public class cCategoryRuleService {

    private readonly FinancesTrackerDbContext mDBContext;

    public cCategoryRuleService(FinancesTrackerDbContext xDBContext) {
      
      mDBContext = xDBContext;
    
    }

    public async Task<(int? categoryId, int? subcategoryId)> MatchCategoryAsync(string xTransactionDscr) {
      
      var pCln_Rules = await mDBContext.CategoryRules.ToListAsync();

      foreach (var pRule in pCln_Rules) {
        if (!string.IsNullOrWhiteSpace(pRule.Keyword) && xTransactionDscr.Contains(pRule.Keyword, StringComparison.OrdinalIgnoreCase)) {
          return (pRule.CategoryId, pRule.SubcategoryId);
        }
      
      }
      
      return (null, null);
    
    }

  }
}
