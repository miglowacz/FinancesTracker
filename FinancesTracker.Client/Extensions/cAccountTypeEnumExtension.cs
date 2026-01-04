using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;

using MudBlazor;

namespace FinancesTracker.Client.Extensions {
  public static class AccountTypeEnumExtensions {
    public static string GetIcon(this AccountTypeEnum type) => type switch {
      AccountTypeEnum.Personal => Icons.Material.Filled.AccountBalance,
      AccountTypeEnum.Savings => Icons.Material.Filled.Savings,
      AccountTypeEnum.Cash => Icons.Material.Filled.Payments,
      AccountTypeEnum.CreditCard => Icons.Material.Filled.CreditCard,
      AccountTypeEnum.Investment => Icons.Material.Filled.TrendingUp,
      _ => Icons.Material.Filled.Help
    };
  }
}
