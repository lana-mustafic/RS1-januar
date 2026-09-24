# Korak 3 — Komponenta liste (HTML)

Ovo je uputa za `dostavljaci.component.html`, plus male dopune u `.ts` i `.scss`.
Dizajn headera ne diraš. Dodaješ bindinge, tabelu, loading, praznu listu i paginator.

Radiš u tri fajla:

| Fajl | Šta radiš |
| --- | --- |
| `rs1-frontend-2025-26/src/app/modules/admin/catalogs/dostavljaci/dostavljaci.component.html` | search, dugme, tabela, loading, paginator |
| `rs1-frontend-2025-26/src/app/modules/admin/catalogs/dostavljaci/dostavljaci.component.ts` | badge metode (već su tu, samo uskladi imena) |
| `rs1-frontend-2025-26/src/app/modules/admin/catalogs/dostavljaci/dostavljaci.component.scss` | `.tip-badge` i ikone |

Uzorci koje otvaraš pored:

| Uzorak | Zašto |
| --- | --- |
| `product-categories-2.component.html` | `(keydown)` + `[(ngModel)]="searchValue"` |
| `products.component.html` | `mat-table`, `[dataSource]="items"`, akcije, paginator, ikona aktivno |
| `fakture.component.html` + `.scss` + `.ts` | tip kao obojeni badge |
| `product-categories.component.html` | ista kartica kao tvoja: loading, tabela, `displayedColumns`, paginator |

---

## Kako podaci putuju

`DostavljaciComponent` nasljeđuje `BaseListPagedComponent`. Lista nije tvoja varijabla `dostavljaci`. Bazna klasa već ima:

- `items` — redovi tabele (`base-list-component.ts`, linija 6)
- `totalItems` — broj zapisa (`base-list-paged-component.ts`, linija 13)
- `isLoading` — spinner
- `goToPage` — paginator

Tvoj TS već puni to u `loadPagedData()` preko `this.handlePageResult(response)`.
HTML samo čita ta imena.

Lanac:

```
searchValue + Enter
  → inputKeyDown()        (već u TS, linije 111–115)
  → searchAction()        (već u TS, linije 105–109)
  → API list()
  → items + totalItems
  → mat-table [dataSource]="items"
```

`displayedColumns` u TS (linija 30) odlučuje redoslijed kolona:

```ts
['naziv', 'kod', 'tip', 'aktivan', 'actions']
```

Ime u `matColumnDef="naziv"` mora biti isto slovo kao u tom nizu. Ako se ne poklope, kolona se ne pojavi.

Polja jednog reda dolaze iz modela `ListDostavljacQueryDto` (`dostavljaci-api.model.ts`, linije 14–20):

- `naziv`
- `kod`
- `tip` (enum: `Ekstern = 1`, `Interni = 2`, `Freelancer = 3`)
- `aktivan` (boolean)

Nema `isEnabled`. To je polje kod proizvoda, ne kod dostavljača.

---

## 1. Header — samo binding, dizajn ostaje

Otvori svoj HTML. Search input je linija 15, dugme je linija 19. Trenutno nemaju događaje.

### Search

Uzorak: `product-categories-2.component.html`, linija 3:

```html
<input [(ngModel)]="searchValue" (keydown)="inputKeyDown($event)" >
```

`searchValue` i `inputKeyDown` već postoje u tvom TS (linije 28 i 111).

Zamijeni liniju 15 ovim (ostavi `matInput` i placeholder):

```html
<input
  matInput
  placeholder="Pretraži dostavljače..."
  [(ngModel)]="searchValue"
  (keydown)="inputKeyDown($event)"
/>
```

Šta se dešava: Angular drži tekst u `searchValue`. Na tipku Enter `inputKeyDown` zove `searchAction()`, a ta metoda stavi tekst u `this.request.search` i ponovo učita prvu stranicu.

### Dugme „Novi dostavljač“

Uzorak: `products.component.html`, linija 21:

```html
<button mat-raised-button color="primary" (click)="onCreate()">
```

Tvoj `onCreate()` je već u TS (linije 77–79) i vodi na rutu `add`.

Na liniji 19 dodaj samo `(click)`:

```html
<button mat-raised-button color="primary" (click)="onCreate()">
```

---

## 2. Loading i prazna lista

Uzorak strukture: `product-categories.component.html`, linije 47–50 (spinner) i uslov `*ngIf="!isLoading"`.

Tvoj spinner (linije 28–31) je dobar. Uvjet za praznu listu (linija 33) nije: prikazuje se i dok se podaci još učitavaju, jer ne gleda `isLoading`.

Zamijeni blok od linije 27 do 35 ovim:

```html
<!-- Loading -->
<div *ngIf="isLoading" class="loading-container">
  <mat-spinner diameter="40"></mat-spinner>
  <p>Učitavanje...</p>
</div>

<!-- Nema podataka -->
<div *ngIf="!isLoading && totalItems === 0" class="no-data">
  <img src="images/no-data.png" alt="Nema podataka" />
  <p>Nema dostavljača za prikaz.</p>
</div>
```

Slika `images/no-data.png` je ista kao u `product-categories-2.component.html`, linije 5–7.

Tabela ide u sljedeći `*ngIf`, da se ne vidi dok spinner radi i da se ne vidi kad nema redova.

---

## 3. Tabela — kopiraj kostur iz Products, sadržaj prilagodi

Otvori `products.component.html`.

Kopiraj kostur:

| Šta | Linije u products |
| --- | --- |
| omotač + `<table mat-table [dataSource]="items">` | 39–40 |
| jedan `ng-container matColumnDef` (naziv) | 42–47 |
| ikona aktivno / neaktivno | 80–87 |
| dugmad edit i delete | 91–110 |
| header red i data red | 113–130 |
| `<app-fit-paginator-bar [vm]="this" />` | 145 |

`[dataSource]="items"` je obavezno. U tvom HTML-u sada piše `[dataSource]="dostavljaci"` (linija 39). Takvo polje ne postoji u TS-u, pa tabela ostaje prazna i u konzoli dobiješ grešku.

Cijeli blok tabele (zamijeni sve od `<div class="table-card">` do kraja fajla — fajl ti je i odrezan na liniji 100, nema zatvarajućih tagova):

```html
<div class="table-card" *ngIf="!isLoading && totalItems > 0">
  <table mat-table [dataSource]="items">

    <ng-container matColumnDef="naziv">
      <th mat-header-cell *matHeaderCellDef>NAZIV</th>
      <td mat-cell *matCellDef="let item">
        <span style="font-weight: 500">{{ item.naziv }}</span>
      </td>
    </ng-container>

    <ng-container matColumnDef="kod">
      <th mat-header-cell *matHeaderCellDef>KOD</th>
      <td mat-cell *matCellDef="let item">
        <span style="font-weight: 600">{{ item.kod }}</span>
      </td>
    </ng-container>

    <ng-container matColumnDef="tip">
      <th mat-header-cell *matHeaderCellDef>TIP</th>
      <td mat-cell *matCellDef="let item">
        <span class="tip-badge" [ngClass]="getTipClass(item.tip)">
          {{ getTipLabel(item.tip) }}
        </span>
      </td>
    </ng-container>

    <ng-container matColumnDef="aktivan">
      <th mat-header-cell *matHeaderCellDef>AKTIVAN</th>
      <td mat-cell *matCellDef="let item">
        <mat-icon [ngClass]="item.aktivan ? 'icon-enabled' : 'icon-disabled'">
          {{ item.aktivan ? 'check_circle' : 'cancel' }}
        </mat-icon>
      </td>
    </ng-container>

    <ng-container matColumnDef="actions">
      <th mat-header-cell *matHeaderCellDef>AKCIJE</th>
      <td mat-cell *matCellDef="let item">
        <button mat-icon-button color="primary" (click)="editAction(item)" matTooltip="Uredi">
          <mat-icon>edit</mat-icon>
        </button>
        <button mat-icon-button color="warn" (click)="onDelete(item)" matTooltip="Obriši">
          <mat-icon>delete</mat-icon>
        </button>
      </td>
    </ng-container>

    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
    <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
  </table>

  <app-fit-paginator-bar [vm]="this" />
</div>
</div>
```

Zadnji `</div>` zatvara `<div class="container">` s vrha fajla.

### Odakle je koja kolona

**Naziv** — oblik iz `products.component.html` linije 42–47. Tamo je `product.name`. Ti pišeš `item.naziv`, jer DTO ima `naziv`.

**Kod** — isti oblik, tekst `item.kod`, `font-weight: 600` da bude podebljan (kao cijena u products, linija 61).

**Tip (badge)** — kopiraj HTML iz `fakture.component.html`, linije 38–45:

```html
<span class="tip-badge" [ngClass]="getTipClass(faktura.tip)">
  {{ getTipString(faktura.tip) }}
</span>
```

Dvije izmjene:

- `faktura` → `item`
- `getTipString` → `getTipLabel` (to ime već imaš u svom TS-u)

`[ngClass]="getTipClass(item.tip)"` doda klasu `ekstern`, `interni` ili `freelancer`. SCSS onda oboji badge.

**Aktivan** — kopiraj iz `products.component.html`, linije 84–86, pa zamijeni `product.isEnabled` sa `item.aktivan`.

U tvom HTML-u (linije 70–71) još stoji `item.isEnabled`. To polje dostavljač nema, ikonica bi uvijek bila crvena `cancel`.

**Akcije** — kopiraj dugmad iz `products.component.html`, linije 94–109.

| Products zove | Ti zoveš | Gdje je metoda |
| --- | --- | --- |
| `onEdit(product)` | `editAction(item)` | tvoj TS, linija 117 |
| `onDelete(product)` | `onDelete(item)` | tvoj TS, linija 81 |

Vodič na GitHubu piše `deleteAction`. To ime ima `product-categories-2` (HTML linija 40, TS linija 88). U tvom TS-u metoda se zove `onDelete`, zato dugme mora zvati `onDelete(item)`. Ako preimenuješ metodu u `deleteAction`, onda i HTML mora zvati `deleteAction`.

`matTooltip` je isti obrazac kao u products (linije 98 i 106), samo bez `translate`.

**Redovi** — iz `product-categories.component.html`, linije 134–137 (čistije od products, jer products ima imena kolona upisana dva puta):

```html
<tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
<tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
```

`*matCellDef="let item"` je lokalno ime jednog reda unutar te kolone. Može se zvati `item`, `row` ili `product`. Bitno je da unutar tog `td` koristiš isto ime.

### Paginator

Jedna linija, iz `products.component.html` linija 145, ili iz `product-categories-2.component.html` linija 47:

```html
<app-fit-paginator-bar [vm]="this" />
```

`[vm]="this"` predaje cijelu komponentu paginatoru. On čita `totalItems`, `paging` i zove `goToPage` — to je već u `base-list-paged-component.ts`, linije 16–35. Ništa novo u TS ne pišeš.

Stavi je unutar `.table-card`, ispod `</table>`, da sjedi na dnu kartice.

---

## 4. TypeScript — badge metode

Metode su ti već u `dostavljaci.component.ts`, linije 33–49. Import `DostavljacTip` je već na linijama 3–7.

Enum u modelu (`dostavljaci-api.model.ts`, linije 4–8) se zove `Interni`, ne `Intern`. GitHub vodič piše `DostavljacTip.Intern` — to u ovom projektu ne postoji i TS će prijaviti grešku. Ostavi `Interni`.

CSS klasa i SCSS moraju imati isto ime. Tvoj TS vraća `'interni'`, a SCSS ima `&.interni`. To je usklađeno. Ako promijeniš jedno u `intern`, promijeni i drugo.

`getTipLabel` vraća tekst u badgeu. `getTipClass` vraća ime CSS klase.

Freelancer boja u tvom SCSS-u trenutno nije narandžasta (pozadina je zelena, tekst ljubičast). To popravljaš u sljedećem koraku, metode u TS-u ne moraš dirati ako već vraćaju:

- `Ekstern` → klasa `ekstern`
- `Interni` → klasa `interni`
- `Freelancer` → klasa `freelancer`

---

## 5. SCSS — badge i ikone

`.table-card` i `.tip-badge` već imaš (linije 16–46). Dvije stvari treba srediti.

### Boje badgea

Uzorak je `fakture.component.scss`, linije 143–160 (`.ulazna` plava, `.izlazna` zelena).

U svom fajlu, unutar `.tip-badge`, freelancer blok (linije 42–45) zamijeni ovim:

```scss
&.freelancer {
  background-color: #fff3e0;
  color: #e65100;
}
```

Plava (`ekstern`) i zelena (`interni`) mogu ostati.

### Ikone aktivno / neaktivno

Uzorak: `products.component.scss`, linije 252–264. Te klase koriste varijable `$success-color` i `$text-disabled`.

U products su definisane na linijama 15–17:

```scss
$text-disabled: #90a4ae;
$success-color: #66bb6a;
```

U tvom `dostavljaci.component.scss` te dvije varijable ne postoje (vrh fajla ima samo primarnu paletu), a `.icon-enabled` / `.icon-disabled` (linije 49–61) ih ipak koriste. SCSS kompajler će puknuti.

Dodaj odmah ispod `$text-secondary` (linija 8):

```scss
$text-disabled: #90a4ae;
$success-color: #66bb6a;
$error-color: #ef5350;
```

Ikone ostavi kao što jesu, unutar `td` ako hoćeš isti scope kao products, ili na korijenu fajla — bitno je da klasa postoji i da varijabla postoji.

Ako ne želiš varijable, u vodiču su direktne boje i to isto radi:

```scss
.icon-enabled { color: #66bb6a; }
.icon-disabled { color: #ef5350; }
```

### Da tabela bude puna širina

Iz `fakture.component.scss`, linije 103–105, unutar `.table-card`:

```scss
table {
  width: 100%;
}
```

Bez ovoga kolone znaju ostati zbijene lijevo.

---

## 6. Info kartica

U SCSS-u imaš `.info-card` (linije 190–207), ali je u HTML-u više nema. To je u redu: vodič kaže da je možeš ukloniti kad lista radi. Ne moraš je vraćati.

---

## Šta u tvom HTML-u trenutno ne valja

Fajl je na pola. Ovo zamijeni, ne dodaji još jednu tabelu pored.

| Linija | Sada | Treba |
| --- | --- | --- |
| 15 | input bez bindinga | `[(ngModel)]="searchValue"` i `(keydown)="inputKeyDown($event)"` |
| 19 | dugme bez klika | `(click)="onCreate()"` |
| 33 | `*ngIf="totalItems ===0"` | `*ngIf="!isLoading && totalItems === 0"` |
| 38 | tabela uvijek vidljiva | `*ngIf="!isLoading && totalItems > 0"` na `.table-card` |
| 39 | `[dataSource]="dostavljaci"` | `[dataSource]="items"` |
| 70–71 | `item.isEnabled` | `item.aktivan` |
| 90 | `(click)="onDelete(item)"` | ostavi `onDelete` — tako se zove metoda u TS-u |
| kraj fajla | odrezan poslije `</table>` | zatvori `table-card`, dodaj paginator, zatvori `container` |

`editAction(item)` na liniji 83 je već dobro.

---

## Provjera

1. Otvori stranicu Dostavljači. Vidiš redove iz API-ja, ne praznu tabelu i ne grešku `dostavljaci is undefined`.
2. U search upiši dio naziva i pritisni Enter. Lista se suzi. `searchAction` šalje `request.search`.
3. Kolona Tip: EKSTERN plavo, INTERNI zeleno, FREELANCER narandžasto.
4. Kolona Aktivan: zelena `check_circle` kad je `aktivan === true`, crvena `cancel` kad je `false`.
5. Olovka otvara edit rutu (`editAction`). Kanta otvara confirm dijalog (`onDelete`).
6. Ispod tabele je paginator. Klik na sljedeću stranicu mijenja podatke.
7. Ako obrišeš sve ili filter ništa ne nađe, vidiš `no-data.png`, ne praznu tabelu.
8. Dok traje poziv, vidiš spinner, ne tabelu i ne „nema podataka“ u isto vrijeme.

Ako tabela i dalje ne izađe, u browser konzoli traži ime koje ne postoji (`dostavljaci`, `isEnabled`, `DostavljacTip.Intern`). HTML smije čitati samo `items`, `item.naziv`, `item.kod`, `item.tip`, `item.aktivan`.
