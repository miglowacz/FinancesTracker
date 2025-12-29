namespace FinancesTracker.Shared.Constants;

public static class cAppConstants {

  public static class ApiEndpoints {
    public const string Transactions = "api/transactions";
    public const string Categories = "api/categories";
    public const string CategoryRules = "api/categoryrules";
    public const string Reports = "api/reports";
    public const string CsvImport = "api/csvimport";
  }

  public static class SupportedBanks {
    public const string Millennium = "Millennium";
    public const string MBank = "MBank";
    public const string Alior = "Alior";

    public static readonly List<string> All = new()
    {
            Millennium, MBank, Alior
        };
  }

  public static class Months {

    public static readonly Dictionary<int, string> Polish = new() {
            { 1, "Styczeń" },
            { 2, "Luty" },
            { 3, "Marzec" },
            { 4, "Kwiecień" },
            { 5, "Maj" },
            { 6, "Czerwiec" },
            { 7, "Lipiec" },
            { 8, "Sierpień" },
            { 9, "Wrzesień" },
            { 10, "Październik" },
            { 11, "Listopad" },
            { 12, "Grudzień" }
        };
  }
}
