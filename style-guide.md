# Konwencje stylu kodu do stosowania przez Copilota

## 🧠 Ogólne zasady
- Wszystkie komentarze piszemy w języku polskim.
- Komentarze zawsze zaczynają się małą literą, bez spacji po `//`.
- Każda funkcja musi mieć komentarz opisujący jej działanie. Komentarze działania funkcji mają znajdować się wewnątrz funkcji, patrz jak wygląda to w przykładzie `Refresh_gdcOffers_AddSingleRow`.
- Ważne, żeby komentarze działania funkcji zawsze były wewnątrz funkcji, niezależnie, czy funkcja opatrzona jest atrybutami, czy nie.
- Każdy parametr funkcji również musi być opisany w osobnej linii komentarza.

---

## 🧾 Nazewnictwo zmiennych

### 🧩 Zmienne lokalne
- Zaczynają się od `p` i dalej wielką literą.
- Przykład: `int pSum`, `string pName`

### 📦 Zmienne na poziomie klasy (modułu)
- Zaczynają się od `m` i dalej wielką literą.
- Przykład: `private int mSum`, `protected string mLabel`

### 🎯 Parametry funkcji
- Zaczynają się od `x` i dalej wielką literą.
- Przykład: `int xSum`, `string xDescription`

---

## 🧱 Nawiasy klamrowe (style bracketingu)

- Nawias otwierający bloku (`{`) **musi znajdować się w tej samej linii**, co definicja funkcji.
- **Przed nawiasem zamykającym funkcji (`}`) musi znajdować się pusta linia.**
- Przykład poprawnego użycia:
```csharp
private void Refresh_gdcOffers_AddSingleRow(int xIdxTradeDoc) {
	//funkcja odświeżająca pojedynczy wiersz grida
	//xIdxTradeDoc - indeks odświeżanego TD

    int pRowIndex = FindRowIndex(xIdxTradeDoc);
    if (pRowIndex >= 0)
        mSum = pRowIndex;

}
```

## 🧾 Bloki if oraz foreach w plikach Razor

- W plikach `.razor` oraz sekcjach Razor w plikach `.cshtml` **wszystkie instrukcje `@if` oraz `@foreach` muszą mieć nawias otwierający `{` w tej samej linii co warunek**.
- **Cała zawartość bloku powinna być wcięta o jeden poziom tabulacji względem instrukcji**.
- Nawias zamykający `}` powinien być w tej samej kolumnie co `@if` lub `@foreach`.
- Niedozwolone jest przenoszenie nawiasu otwierającego `{` do nowej linii.
- Niedozwolone jest pomijanie nawiasów klamrowych nawet dla pojedynczych instrukcji w bloku Razor.
- Przykład poprawnego użycia:
@if (categories != null) {
  @foreach (var category in categories) { 
   <option value="@category.Id">@category.Name</option> 
   } 
}
