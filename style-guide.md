# Konwencje stylu kodu do stosowania przez Copilota

## 🧠 Ogólne zasady
- Wszystkie komentarze piszemy w języku polskim.
- Komentarze zawsze zaczynają się małą literą, bez spacji po `//`.
- Każda funkcja musi mieć komentarz opisujący jej działanie. Komentarze działania funkcji mają znajdować sie wewnątrz funkcji, patrz jak wygląda to w przykładzie Refresh_gdcOffers_AddSingleRow.
- ważne, żeby komentarze działania funkcji zawsze były wewnątrz funkcji, niezelżnie, czy funkcja opatrzona jest atrybutami, czy nie.
- Każdy parametr funkcji również musi być opisany w osobnej linii komentarza.

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
