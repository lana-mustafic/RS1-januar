# Izmjene na tvoj kod — red po red

Tvoj kod je uglavnom kopija `product-categories-2.component.ts`.  
Ispod je **tačno šta mijenjaš**, linija po linija. Radi redom od vrha fajla.

**Fajl koji edituješ:** `dostavljaci.component.ts`

---

## PREGLED — šta je pogrešno u tvom kodu

| Problem | Zašto |
|---------|-------|
| `DostavljaciApiService` import iz `.model` | Servis je u fajlu `dostavljaci-api.services.ts` |
| Tipovi iz `product-categories-api.model` | Trebaju iz `dostavljaci-api.model` |
| `onlyEnabled`, `pageSize = 100` | To je za kategorije — `ListDostavljacRequest` nema `onlyEnabled` |
| `changeStatus` cijela metoda | Dostavljači nemaju enable/disable API |
| `ListProductCategoriesQueryDto`, `x.name`, `x.isEnabled` | Dostavljač ima `naziv`, `aktivan` — drugi tip |
| `dialogHelper.productCategory` | To je za kategorije — koristi `dialogHelper.confirmDelete` |
| `searchAction` pravi novi objekat | Mora koristiti `this.request` |
| Nema `onCreate()` | Treba za dugme "Novi dostavljač" |
| Nema `displayedColumns` | Treba za mat-table (Korak 3) |
| `selector: 'dostavljaci'` | U projektu je `app-dostavljaci` |

---

## KORAK 1 — Importi (linije 1–12)

### Linija 2 — OBRISI i zamijeni

**Tvoja (pogrešno):**
```ts
import {DostavljaciApiService} from '../../../../api-services/dostavljaci/dostavljaci-api.model';
```

**Zamijeni sa (kopiraj putanju — servis je u `.services`, ne `.model`):**
```ts
import { DostavljaciApiService } from '../../../../api-services/dostavljaci/dostavljaci-api.services';
```

**Odakle:** otvori `dostavljaci-api.services.ts` — tamo je klasa `DostavljaciApiService`.

---

### Linije 3–6 — OBRISI i zamijeni

**Tvoja (pogrešno):**
```ts
import {
  ListDostavljacQueryDto,
  ListDostavljacRequest
} from '../../../../api-services/product-categories/product-categories-api.model';
```

**Zamijeni sa:**
```ts
import {
  ListDostavljacQueryDto,
  ListDostavljacRequest,
} from '../../../../api-services/dostavljaci/dostavljaci-api.model';
```

**Odakle:** `dostavljaci-api.model.ts` — tipovi su već tamo definisani.

---

### Linija 11 — OBRISI cijelu liniju

```ts
import {BaseComponent} from '../../../../core/components/base-classes/base-component';
```

**Zašto:** ne koristiš `BaseComponent` direktno — nasljeđuješ `BaseListPagedComponent`.

---

### Ostali importi (linije 7–10, 12) — OSTAVI

Ovi su OK:
- `DialogHelperService`
- `DialogButton`
- `ToasterService`
- `ActivatedRoute`, `Router`
- `BaseListPagedComponent`

---

## KORAK 2 — @Component dekorator (linije 14–19)

### Linija 15 — promijeni selector

**Tvoja:**
```ts
selector: 'dostavljaci',
```

**Zamijeni sa:**
```ts
selector: 'app-dostavljaci',
```

**Odakle:** originalni starter i `admin-module.ts` koriste `app-dostavljaci`.

---

## KORAK 3 — Svojstva u klasi (poslije inject linija)

### Dodaj POSLIJE `searchValue=""` (oko linije 28)

```ts
displayedColumns: string[] = ['naziv', 'kod', 'tip', 'aktivan', 'actions'];
```

**Zašto:** kolone za tabelu u Koraku 3. Nema u product-categories — dodaješ samo za dostavljače.

---

## KORAK 4 — Konstruktor (linije 30–35)

### OBRISI linije 33 i 34

**Obriši ovo:**
```ts
  this.request.onlyEnabled = true;
  this.request.paging.pageSize = 100;
```

**Zašto:** `ListDostavljacRequest` nema polje `onlyEnabled` — to je samo za kategorije.  
Default `pageSize = 10` dolazi iz `BasePagedQuery` — ne moraš mijenjati.

### Konstruktor treba da izgleda ovako:

```ts
constructor() {
  super();
  this.request = new ListDostavljacRequest();
}
```

**Uzorak:** `products.component.ts` linije 39–42 (isti obrazac, samo drugi Request tip).

---

## KORAK 5 — ngOnInit (linije 37–41)

**Ostavi kako jest** — OK je:

```ts
ngOnInit(): void {
  this.initList();
}
```

---

## KORAK 6 — loadPagedData (linije 43–55)

### U `next` — obriši debug toast (linija 48)

**Obriši:**
```ts
this.toaster.success("ok su podaci: " + this.totalItems);
```

**Zašto:** debug poruka iz product-categories — u `products.component.ts` nema tog toasta.

### `error` — možeš ostaviti ili malo poboljšati

Tvoja verzija radi. Opcionalno kao u products:

```ts
error: (err) => {
  this.stopLoading();
  this.toaster.error(err?.message ?? 'Greška pri učitavanju dostavljača.');
  console.error('Load dostavljaci error:', err);
},
```

**Uzorak za strukturu:** `products.component.ts` linije 48–60.

---

## KORAK 7 — OBRISI cijelu metodu changeStatus (linije 57–86)

**Obriši SVE od:**
```ts
changeStatus(x: ListProductCategoriesQueryDto, $event: Event) {
```
**do zatvarajuće `}`** (uključujući enable/disable logiku).

**Zašto:**
- Dostavljači nemaju `enable`/`disable` na API-ju
- Koristi `ListProductCategoriesQueryDto` i `isEnabled` — to nije tvoj model
- U Koraku 3 ćeš `aktivan` prikazati kao tekst/badge, ne kao switch (osim ako zadatak ne traži drugačije)

---

## KORAK 8 — deleteAction (linije 88–109)

### Zamijeni CIJELU metodu ovim:

Kombinacija uzorka iz `products.component.ts` (linije 73–99), prilagođena dostavljačima:

```ts
deleteAction(item: ListDostavljacQueryDto): void {
  this.dialogHelper.confirmDelete(item.naziv).subscribe((result) => {
    if (result && result.button === DialogButton.DELETE) {
      this.performDelete(item);
    }
  });
}

private performDelete(item: ListDostavljacQueryDto): void {
  this.startLoading();

  this.api.delete(item.id).subscribe({
    next: () => {
      this.toaster.success(`Dostavljač "${item.naziv}" uspješno obrisan.`);
      this.loadPagedData();
    },
    error: (err) => {
      this.stopLoading();
      this.toaster.error(err?.message ?? 'Greška pri brisanju.');
      console.error('Delete dostavljac error:', err);
    },
  });
}
```

### Šta si mijenjala u odnosu na tvoj kod:

| Tvoja verzija | Ispravno |
|---------------|----------|
| `x: ListProductCategoriesQueryDto` | `item: ListDostavljacQueryDto` |
| `x.name` | `item.naziv` |
| `dialogHelper.productCategory.confirmDelete` | `dialogHelper.confirmDelete` |
| `this.ngOnInit()` poslije brisanja | `this.loadPagedData()` |
| sve u jednoj metodi | `deleteAction` + `performDelete` (čistije, kao products) |

**Odakle kopiraš obrazac:** `products.component.ts` → `onDelete` + `performDelete`, samo zamijeniš imena.

---

## KORAK 9 — searchAction (linije 111–129)

### OBRISI cijelu staru metodu i zamijeni ovom:

```ts
searchAction(): void {
  this.request.search = this.searchValue?.trim() || null;
  this.request.paging.page = 1;
  this.loadPagedData();
}
```

### Zašto stara ne valja:

```ts
// ❌ NE — pravi novi objekat, gubi se veza sa this.request
this.api.list({
  paging: { page: 1, pageSize: 1000 },
  search: this.searchValue
})
```

- Ne koristi `this.request` → paginator u HTML-u neće raditi ispravno
- `pageSize: 1000` — hack iz kategorija, ne za dostavljače

---

## KORAK 10 — inputKeyDown (linije 131–135)

### Možeš ostaviti ili zamijeni (oba rade):

**Tvoja (radi):**
```ts
inputKeyDown($event: KeyboardEvent) {
  if ($event.keyCode == 13) {
    this.searchAction();
  }
}
```

**Modernija verzija (preporuka):**
```ts
inputKeyDown(event: KeyboardEvent): void {
  if (event.key === 'Enter') {
    this.searchAction();
  }
}
```

---

## KORAK 11 — editAction (linije 137–139)

### Zamijeni tip parametra:

**Tvoja:**
```ts
editAction(x: ListProductCategoriesQueryDto) {
  this.router.navigate(['edit', x.id], {relativeTo: this.route});
}
```

**Ispravno:**
```ts
editAction(item: ListDostavljacQueryDto): void {
  this.router.navigate(['edit', item.id], { relativeTo: this.route });
}
```

**Tijelo metode** — isto kao u `product-categories-2.component.ts` linija 137–139, samo drugi tip.

---

## KORAK 12 — DODAJ metodu onCreate (nema je u tvom kodu)

Dodaj **poslije** `loadPagedData`, **prije** `deleteAction`:

```ts
onCreate(): void {
  this.router.navigate(['add'], { relativeTo: this.route });
}
```

**Uzorak:** `product-categories-2` nema `onCreate` — ali u zadatku traži.  
Relativna ruta kao `editAction` — samo `['add']`.

Za products je apsolutna ruta (`/admin/products/add`) — ti koristiš **relativnu** jer si već na `/admin/dostavljaci`.

---

## KORAK 13 — Konačni fajl (referenca)

Kad sve uradiš, fajl treba da izgleda ovako (bez changeStatus, sa ispravnim importima):

```ts
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DostavljaciApiService } from '../../../../api-services/dostavljaci/dostavljaci-api.services';
import {
  ListDostavljacQueryDto,
  ListDostavljacRequest,
} from '../../../../api-services/dostavljaci/dostavljaci-api.model';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { DialogButton } from '../../../shared/models/dialog-config.model';

@Component({
  selector: 'app-dostavljaci',
  standalone: false,
  templateUrl: './dostavljaci.component.html',
  styleUrl: './dostavljaci.component.scss',
})
export class DostavljaciComponent
  extends BaseListPagedComponent<ListDostavljacQueryDto, ListDostavljacRequest>
  implements OnInit
{
  private api = inject(DostavljaciApiService);
  private dialogHelper = inject(DialogHelperService);
  private toaster = inject(ToasterService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  searchValue = '';
  displayedColumns: string[] = ['naziv', 'kod', 'tip', 'aktivan', 'actions'];

  constructor() {
    super();
    this.request = new ListDostavljacRequest();
  }

  ngOnInit(): void {
    this.initList();
  }

  protected override loadPagedData(): void {
    this.startLoading();
    this.api.list(this.request).subscribe({
      next: (response) => {
        this.handlePageResult(response);
        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading();
        this.toaster.error(err?.message ?? 'Greška pri učitavanju dostavljača.');
        console.error('Load dostavljaci error:', err);
      },
    });
  }

  onCreate(): void {
    this.router.navigate(['add'], { relativeTo: this.route });
  }

  editAction(item: ListDostavljacQueryDto): void {
    this.router.navigate(['edit', item.id], { relativeTo: this.route });
  }

  deleteAction(item: ListDostavljacQueryDto): void {
    this.dialogHelper.confirmDelete(item.naziv).subscribe((result) => {
      if (result && result.button === DialogButton.DELETE) {
        this.performDelete(item);
      }
    });
  }

  private performDelete(item: ListDostavljacQueryDto): void {
    this.startLoading();

    this.api.delete(item.id).subscribe({
      next: () => {
        this.toaster.success(`Dostavljač "${item.naziv}" uspješno obrisan.`);
        this.loadPagedData();
      },
      error: (err) => {
        this.stopLoading();
        this.toaster.error(err?.message ?? 'Greška pri brisanju.');
        console.error('Delete dostavljac error:', err);
      },
    });
  }

  searchAction(): void {
    this.request.search = this.searchValue?.trim() || null;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  inputKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.searchAction();
    }
  }
}
```

---

## CHECKLIST — provjeri prije nego što kažeš "gotovo"

- [ ] Nema importa iz `product-categories`
- [ ] `DostavljaciApiService` iz `dostavljaci-api.services.ts`
- [ ] Nema `ListProductCategoriesQueryDto` nigdje u fajlu
- [ ] Nema `changeStatus`
- [ ] Nema `onlyEnabled`
- [ ] `searchAction` koristi `this.request`
- [ ] Ima `onCreate`, `displayedColumns`
- [ ] `selector` je `app-dostavljaci`
- [ ] Nema crvenih linija u editoru

---

## BONUS — kad budeš testirala (nije Korak 2, ali bitno)

U `dostavljaci-api.services.ts` linija 13:

```ts
// ❌ sada
private readonly baseUrl = `${environment.apiUrl}/ProductCategories`;

// ✅ treba biti
private readonly baseUrl = `${environment.apiUrl}/Dostavljaci`;
```

Bez toga API zove pogrešan endpoint — lista neće raditi čak i sa ispravnom komponentom.

---

## Redoslijed rada (preporuka)

1. Ispravi importe (Korak 1)
2. Selector (Korak 2)
3. Konstruktor — obriši onlyEnabled (Korak 4)
4. Obriši changeStatus (Korak 7)
5. Zamijeni deleteAction (Korak 8)
6. Zamijeni searchAction (Korak 9)
7. Ispravi editAction (Korak 11)
8. Dodaj onCreate + displayedColumns (Korak 3, 12)
9. Očisti loadPagedData toast (Korak 6)
10. Provjeri checklist

Radi **jedan korak → spasi → pogledaj ima li crvenih grešaka** prije sljedećeg.

---
---
---

# KORAK 3 — HTML + SCSS + badge helperi u TS

Povezuješ UI sa metodama iz Koraka 2.  
Radiš u **3 fajla**:

| Fajl | Šta radiš |
|------|-----------|
| `dostavljaci.component.ts` | Dodaješ `getTipLabel` i `getTipClass` |
| `dostavljaci.component.html` | Cijela tabela, search, loading, paginator |
| `dostavljaci.component.scss` | Dodaješ `.table-card`, `.tip-badge`, ikone |

---

## MAPA — odakle šta uzimaš (najvažnije!)

| Dio koji pišeš | Primarni uzorak | Fajl u projektu |
|----------------|-----------------|-----------------|
| Header + search + dugme | Starter + binding | `dostavljaci.component.html` (već imaš dizajn) |
| `[(ngModel)]` + `(keydown)` | Pretraga | `product-categories-2.component.html` linija 3 |
| `(click)="onCreate()"` | Dugme novi zapis | `products.component.html` linija 21 |
| `*ngIf="isLoading"` + spinner | Loading stanje | `product-categories-2.component.html` linije 10–13 |
| `*ngIf="totalItems === 0"` + slika | Nema podataka | `product-categories-2.component.html` linije 5–7 |
| `mat-table` + kolone | Cijela tabela | `products.component.html` linije 39–146 |
| Kolona **aktivan** (ikone) | check_circle / cancel | `products.component.html` linije 81–87 |
| Kolona **akcije** (edit/delete) | mat-icon-button | `products.component.html` linije 91–110 |
| Paginator | `[vm]="this"` | `products.component.html` linija 145 |
| `displayedColumns` u tabeli | Tvoj TS | `dostavljaci.component.ts` — već imaš niz |
| Kolona **tip** (badge) | **Nema gotov uzorak** | Pišeš iz zadatka + helper metode u TS |
| SCSS ikone enabled/disabled | Boje ikona | `products.component.scss` linije 252–264 |
| SCSS `.table-card` | Kartica oko tabele | Iz zadatka (ispod) |
| SCSS `.tip-badge` | Badge za tip | Iz zadatka (ispod) — **nema u products** |

**Pravilo:** ne kopiraj `products` HTML 1:1 — tamo su `translate` pipe-ovi. Ti pišeš tekst direktno na bosanskom/hrvatskom.

> **Kako čitati sekciju ispod:** za svaki dio HTML-a i SCSS-a prvo vidiš **OBRAZAC** (kopiraj iz navedenog fajla), pa **TVOJA VERZIJA** (to lijepiš u svoj fajl). Ako piše "bez izmjene" — kopiraj doslovno.

---

## KORAK 3 — OBRAZAC → TVOJA VERZIJA (HTML i SCSS)

Za svaki blok: **gore = obrazac iz projekta**, **dolje = tvoj `dostavljaci` fajl**.

---

### BLOK 1 — Search input (pretraga)

**OBRAZAC iz:** `product-categories-2.component.html` (linija 3)  
*(jednostavan primjer ngModel + keydown)*

```html
<input [(ngModel)]="searchValue" (keydown)="inputKeyDown($event)" >
```

**OBRAZAC iz:** `products.component.html` (linije 12–17)  
*(isti princip, ali u mat-form-field — bliže tvom dizajnu)*

```html
<input
  matInput
  [placeholder]="'PRODUCTS.SEARCH_INPUT_PLACEHOLDER' | translate"
  (keyup.enter)="request.paging.page = 1; loadPagedData()"
  [(ngModel)]="request.search"
/>
```

**TVOJ STARTER** (`dostavljaci.component.html`, linija 15) — još nije povezan:

```html
<input matInput placeholder="Pretraži dostavljače..." />
```

**➡️ TVOJA VERZIJA** — zamijeni samo `<input>` u svom headeru:

```html
<input
  matInput
  placeholder="Pretraži dostavljače..."
  [(ngModel)]="searchValue"
  (keydown)="inputKeyDown($event)"
/>
```

**Šta si promijenila:**
| Obrazac (products) | Tvoja verzija |
|--------------------|---------------|
| `[(ngModel)]="request.search"` | `[(ngModel)]="searchValue"` — jer u TS koristiš `searchValue`, a `searchAction` puni `this.request.search` |
| `(keyup.enter)="request.paging.page = 1; loadPagedData()"` | `(keydown)="inputKeyDown($event)"` — logika je u TS metodi |
| `translate` pipe | običan tekst `placeholder="Pretraži..."` |

---

### BLOK 2 — Dugme "Novi dostavljač"

**OBRAZAC iz:** `products.component.html` (linije 21–24)

```html
<button mat-raised-button color="primary" (click)="onCreate()">
  <mat-icon>add</mat-icon>
  {{ 'PRODUCTS.NEW_PRODUCT' | translate }}
</button>
```

**TVOJ STARTER** (`dostavljaci.component.html`, linije 19–22) — nema `(click)`:

```html
<button mat-raised-button color="primary">
  <mat-icon>add</mat-icon>
  Novi dostavljač
</button>
```

**➡️ TVOJA VERZIJA** — dodaj samo `(click)="onCreate()"`:

```html
<button mat-raised-button color="primary" (click)="onCreate()">
  <mat-icon>add</mat-icon>
  Novi dostavljač
</button>
```

**Šta si promijenila:** samo `(click)="onCreate()"`. Tekst ostaje tvoj, bez `translate`.

---

### BLOK 3 — Loading (spinner)

**OBRAZAC iz:** `product-categories-2.component.html` (linije 10–13)

```html
<div *ngIf="isLoading" class="loading-container">
  <mat-spinner diameter="40"></mat-spinner>
  <p>{{ 'PRODUCT_CATEGORIES.FORM.LOADING' | translate }}</p>
</div>
```

**➡️ TVOJA VERZIJA** — kopiraj strukturu, zamijeni tekst (dodaj POSLIJE `header-card`, PRIJE tabele):

```html
<div *ngIf="isLoading" class="loading-container">
  <mat-spinner diameter="40"></mat-spinner>
  <p>Učitavanje...</p>
</div>
```

**Šta si promijenila:** `{{ '...' | translate }}` → `Učitavanje...`

---

### BLOK 4 — Nema podataka (no data)

**OBRAZAC iz:** `product-categories-2.component.html` (linije 5–7)

```html
<div *ngIf="totalItems ===0">
  <img src="images/no-data.png"/>
</div>
```

**➡️ TVOJA VERZIJA** — prošireno (dodaj POSLIJE loading bloka):

```html
<div *ngIf="!isLoading && totalItems === 0" class="no-data">
  <img src="images/no-data.png" alt="Nema podataka" />
  <p>Nema dostavljača za prikaz.</p>
</div>
```

**Šta si promijenila:**
- dodala `!isLoading &&` — da se ne prikaže dok se učitava
- dodala klasu `no-data` i paragraf sa porukom

---

### BLOK 5 — Obriši info-card (tvoj starter)

**TVOJ STARTER** (`dostavljaci.component.html`, linije 27–31) — **OBRIŠI CIJELI BLOK:**

```html
<div class="info-card">
  <mat-icon>info</mat-icon>
  <span>Ovdje raditi ispitni zadatak - prvi modul</span>
</div>
```

**➡️ TVOJA VERZIJA:** ništa — samo obriši. U finalnom fajlu ovoga nema.

---

### BLOK 6 — Omotač tabele (table + paginator)

**OBRAZAC iz:** `products.component.html` (linije 39–40 i 145–146)

```html
<div class="mat-elevation-z8" *ngIf="!isLoading && !errorMessage">
  <table mat-table [dataSource]="items">
    ...
  </table>

  <app-fit-paginator-bar [vm]="this" />
</div>
```

**➡️ TVOJA VERZIJA:**

```html
<div class="table-card" *ngIf="!isLoading && totalItems > 0">
  <table mat-table [dataSource]="items">
    <!-- kolone — blokovi 7–11 -->
    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
    <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
  </table>

  <app-fit-paginator-bar [vm]="this" />
</div>
```

**Šta si promijenila:**
| Obrazac (products) | Tvoja verzija |
|--------------------|---------------|
| `class="mat-elevation-z8"` | `class="table-card"` |
| `*ngIf="!isLoading && !errorMessage"` | `*ngIf="!isLoading && totalItems > 0"` |
| paginator | **isto** — `<app-fit-paginator-bar [vm]="this" />` |

**Paginator obrazac iz:** `product-categories-2.component.html` (linija 47) — identičan:
```html
<app-fit-paginator-bar [vm]="this" />
```

---

### BLOK 7 — Kolona NAZIV

**OBRAZAC iz:** `products.component.html` (linije 42–47)

```html
<ng-container matColumnDef="name">
  <th mat-header-cell *matHeaderCellDef>{{ 'PRODUCTS.TABLE.NAME' | translate }}</th>
  <td mat-cell *matCellDef="let product">
    <span style="font-weight: 500">{{ product.name }}</span>
  </td>
</ng-container>
```

**➡️ TVOJA VERZIJA:**

```html
<ng-container matColumnDef="naziv">
  <th mat-header-cell *matHeaderCellDef>NAZIV</th>
  <td mat-cell *matCellDef="let item">
    <span style="font-weight: 500">{{ item.naziv }}</span>
  </td>
</ng-container>
```

**Šta si promijenila:**
| products | dostavljaci |
|----------|-------------|
| `matColumnDef="name"` | `matColumnDef="naziv"` |
| `let product` | `let item` |
| `product.name` | `item.naziv` |
| translate u headeru | `NAZIV` direktno |

---

### BLOK 8 — Kolona KOD

**OBRAZAC iz:** `products.component.html` (linije 58–64) — kolona `price` (bold span)

```html
<ng-container matColumnDef="price">
  <th mat-header-cell *matHeaderCellDef>{{ 'PRODUCTS.TABLE.PRICE' | translate }}</th>
  <td mat-cell *matCellDef="let product">
    <span style="font-weight: 600; color: #4976b5"
      >{{ product.price | number : '1.2-2' }} KM</span
    >
  </td>
</ng-container>
```

**➡️ TVOJA VERZIJA:**

```html
<ng-container matColumnDef="kod">
  <th mat-header-cell *matHeaderCellDef>KOD</th>
  <td mat-cell *matCellDef="let item">
    <span style="font-weight: 600">{{ item.kod }}</span>
  </td>
</ng-container>
```

**Šta si promijenila:** ime kolone, polje `kod`, uklonila `color` i `number` pipe (kod nije cijena).

---

### BLOK 9 — Kolona TIP (badge) — NEMA gotovog obrasca u projektu

**OBRAZAC:** nema u `products` ni `product-categories-2` — pišeš iz zadatka.

**➡️ TVOJA VERZIJA** (kopiraj ovo u tabelu):

```html
<ng-container matColumnDef="tip">
  <th mat-header-cell *matHeaderCellDef>TIP</th>
  <td mat-cell *matCellDef="let item">
    <span class="tip-badge" [ngClass]="getTipClass(item.tip)">
      {{ getTipLabel(item.tip) }}
    </span>
  </td>
</ng-container>
```

**Zahtijeva u TS:** `getTipLabel` i `getTipClass` (Korak 3.1) + SCSS `.tip-badge` (Korak 3.2).

---

### BLOK 10 — Kolona AKTIVAN (ikone)

**OBRAZAC iz:** `products.component.html` (linije 81–87)

```html
<ng-container matColumnDef="isEnabled">
  <th mat-header-cell *matHeaderCellDef>{{ 'PRODUCTS.TABLE.ACTIVE' | translate }}</th>
  <td mat-cell *matCellDef="let product">
    <mat-icon [ngClass]="product.isEnabled ? 'icon-enabled' : 'icon-disabled'">
      {{ product.isEnabled ? 'check_circle' : 'cancel' }}
    </mat-icon>
  </td>
</ng-container>
```

**➡️ TVOJA VERZIJA:**

```html
<ng-container matColumnDef="aktivan">
  <th mat-header-cell *matHeaderCellDef>AKTIVAN</th>
  <td mat-cell *matCellDef="let item">
    <mat-icon [ngClass]="item.aktivan ? 'icon-enabled' : 'icon-disabled'">
      {{ item.aktivan ? 'check_circle' : 'cancel' }}
    </mat-icon>
  </td>
</ng-container>
```

**Šta si promijenila:** `isEnabled` → `aktivan`, `product` → `item`. Struktura ikona **ista**.

---

### BLOK 11 — Kolona AKCIJE (edit / delete)

**OBRAZAC iz:** `products.component.html` (linije 91–110)

```html
<ng-container matColumnDef="actions">
  <th mat-header-cell *matHeaderCellDef>{{ 'PRODUCTS.TABLE.ACTIONS' | translate }}</th>
  <td mat-cell *matCellDef="let product">
    <button
      mat-icon-button
      color="primary"
      (click)="onEdit(product)"
      [matTooltip]="'PRODUCTS.ACTIONS.EDIT_TOOLTIP' | translate"
    >
      <mat-icon>edit</mat-icon>
    </button>
    <button
      mat-icon-button
      color="warn"
      (click)="onDelete(product)"
      [matTooltip]="'PRODUCTS.ACTIONS.DELETE_TOOLTIP' | translate"
    >
      <mat-icon>delete</mat-icon>
    </button>
  </td>
</ng-container>
```

**➡️ TVOJA VERZIJA:**

```html
<ng-container matColumnDef="actions">
  <th mat-header-cell *matHeaderCellDef>AKCIJE</th>
  <td mat-cell *matCellDef="let item">
    <button mat-icon-button color="primary" (click)="editAction(item)" matTooltip="Uredi">
      <mat-icon>edit</mat-icon>
    </button>
    <button mat-icon-button color="warn" (click)="deleteAction(item)" matTooltip="Obriši">
      <mat-icon>delete</mat-icon>
    </button>
  </td>
</ng-container>
```

**Šta si promijenila:**
| products | dostavljaci |
|----------|-------------|
| `onEdit(product)` | `editAction(item)` |
| `onDelete(product)` | `deleteAction(item)` |
| `[matTooltip]="'...' \| translate"` | `matTooltip="Uredi"` |

---

### BLOK 12 — Redovi tabele (header + data rows)

**OBRAZAC iz:** `products.component.html` (linije 113–130)

```html
<tr
  mat-header-row
  *matHeaderRowDef="[
    'name',
    'categoryName',
    'price',
    'stockQuantity',
    'isEnabled',
    'actions'
  ]"
></tr>
<tr
  mat-row
  *matRowDef="
    let row;
    columns: ['name', 'categoryName', 'price', 'stockQuantity', 'isEnabled', 'actions']
  "
></tr>
```

**➡️ TVOJA VERZIJA** — kraće, koristi niz iz TS:

```html
<tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
<tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
```

**Zašto kraće:** u `dostavljaci.component.ts` već imaš:
```ts
displayedColumns: string[] = ['naziv','kod','tip','aktivan','actions'];
```

**Napomena:** products ima i `*matNoDataRow` (linije 133–142) — ti to **ne moraš** jer već imaš poseban `no-data` div iznad tabele (Blok 4).

---

### BLOK 13 — SCSS: kartica oko tabele

**OBRAZAC iz:** `products.component.scss` (linije 224–229) — klasa `.mat-elevation-z8`

```scss
.mat-elevation-z8 {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(73, 118, 181, 0.08), 0 1px 3px rgba(0, 0, 0, 0.06);
  border: 1px solid $border-color;
```

**➡️ TVOJA VERZIJA** — dodaj na **kraj** `dostavljaci.component.scss` (koristiš klasu `table-card` iz HTML-a):

```scss
.table-card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(73, 118, 181, 0.08);
  border: 1px solid rgba(73, 118, 181, 0.15);
}
```

**Šta si promijenila:** ime klase `.mat-elevation-z8` → `.table-card`, malo jednostavniji `border` (bez SCSS varijable `$border-color` — radi i ovako).

---

### BLOK 14 — SCSS: ikone aktivan (zelena / crvena)

**OBRAZAC iz:** `products.component.scss` (linije 252–264) — unutar `td { }`

```scss
.icon-enabled {
  color: $success-color;
  font-size: 24px;
  width: 24px;
  height: 24px;
}

.icon-disabled {
  color: $text-disabled;
  font-size: 24px;
  width: 24px;
  height: 24px;
}
```

**➡️ TVOJA VERZIJA** — dodaj na kraj `dostavljaci.component.scss` (kraća verzija, iste boje):

```scss
.icon-enabled { color: #66bb6a; }
.icon-disabled { color: #ef5350; }
```

**Šta si promijenila:** direktne boje umjesto `$success-color` / `$text-disabled` — jednostavnije jer tvoj SCSS fajl već ima svoje varijable, ali nema `$success-color`.

---

### BLOK 15 — SCSS: badge za tip — NEMA obrasca u projektu

**OBRAZAC:** nema u repo-u — iz zadatka.

**➡️ TVOJA VERZIJA** — dodaj na kraj `dostavljaci.component.scss`:

```scss
.tip-badge {
  display: inline-block;
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;

  &.ekstern {
    background-color: #e3f2fd;
    color: #1565c0;
  }

  &.intern {
    background-color: #e8f5e9;
    color: #2e7d32;
  }

  &.freelancer {
    background-color: #fff3e0;
    color: #e65100;
  }
}
```

**Kopiraj doslovno** — povezuje se sa `getTipClass()` koji vraća `ekstern`, `intern`, `freelancer`.

---

### BLOK 16 — Cijeli HTML fajl (sve blokovi spojeni)

**Tvoj starter** (`dostavljaci.component.html`) — samo header + info-card.

**➡️ TVOJA VERZIJA** — zamijeni **cijeli sadržaj** fajla ovim (sve blokovi 1–12 spojeni):

```html
<div class="container">
  <div class="header-card">
    <div class="header-card-content">
      <div class="title-section">
        <div class="title-icon">
          <mat-icon>local_shipping</mat-icon>
        </div>
        <h1>Dostavljači</h1>
      </div>

      <div class="actions-container">
        <mat-form-field class="search-field" appearance="outline">
          <mat-label>Pretraga...</mat-label>
          <input
            matInput
            placeholder="Pretraži dostavljače..."
            [(ngModel)]="searchValue"
            (keydown)="inputKeyDown($event)"
          />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>

        <button mat-raised-button color="primary" (click)="onCreate()">
          <mat-icon>add</mat-icon>
          Novi dostavljač
        </button>
      </div>
    </div>
  </div>

  <div *ngIf="isLoading" class="loading-container">
    <mat-spinner diameter="40"></mat-spinner>
    <p>Učitavanje...</p>
  </div>

  <div *ngIf="!isLoading && totalItems === 0" class="no-data">
    <img src="images/no-data.png" alt="Nema podataka" />
    <p>Nema dostavljača za prikaz.</p>
  </div>

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
          <button mat-icon-button color="warn" (click)="deleteAction(item)" matTooltip="Obriši">
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

**Header** (`title-section`, `title-icon`) — **ostaje iz tvog startera**, samo si dodala bindinge na input i dugme.

---

### BLOK 17 — Cijeli SCSS dopuna (sve blokovi 13–15 spojeni)

**Tvoj fajl** `dostavljaci.component.scss` — već ima `.container`, `.header-card`, itd. **Ne briši to.**

**➡️ TVOJA VERZIJA** — na **kraj fajla** (poslije `@media` bloka) **zalijepi**:

```scss
.table-card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(73, 118, 181, 0.08);
  border: 1px solid rgba(73, 118, 181, 0.15);
}

.tip-badge {
  display: inline-block;
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;

  &.ekstern {
    background-color: #e3f2fd;
    color: #1565c0;
  }

  &.intern {
    background-color: #e8f5e9;
    color: #2e7d32;
  }

  &.freelancer {
    background-color: #fff3e0;
    color: #e65100;
  }
}

.icon-enabled { color: #66bb6a; }
.icon-disabled { color: #ef5350; }
```

---

## Redoslijed copy-paste (ako ne znaš odakle krenuti)

1. TS helper metode (sekcija 3.1 ispod)
2. SCSS — Blok 17 na kraj fajla
3. HTML — ili blok po blok (1→12), ili cijeli Blok 16 odjednom
4. Obriši info-card (Blok 5)
5. Pokreni app → `/admin/dostavljaci`

---

## KORAK 3.1 — TS: helper metode za badge

**Gdje:** `dostavljaci.component.ts` — na **kraj klase**, prije zatvarajuće `}`

### 3.1a — Dodaj import

Na vrh fajla, u postojeći import iz `dostavljaci-api.model`, dodaj `DostavljacTip`:

**Prije:**
```ts
import {
  ListDostavljacQueryDto,
  ListDostavljacRequest
} from '../../../../api-services/dostavljaci/dostavljaci-api.model';
```

**Poslije:**
```ts
import {
  DostavljacTip,
  ListDostavljacQueryDto,
  ListDostavljacRequest,
} from '../../../../api-services/dostavljaci/dostavljaci-api.model';
```

**Odakle:** enum `DostavljacTip` je u `dostavljaci-api.model.ts` (Ekstern=1, Intern=2, Freelancer=3).

### 3.1b — Dodaj dvije metode

```ts
getTipLabel(tip: DostavljacTip): string {
  switch (tip) {
    case DostavljacTip.Ekstern: return 'Ekstern';
    case DostavljacTip.Intern: return 'Intern';
    case DostavljacTip.Freelancer: return 'Freelancer';
    default: return '';
  }
}

getTipClass(tip: DostavljacTip): string {
  switch (tip) {
    case DostavljacTip.Ekstern: return 'ekstern';
    case DostavljacTip.Intern: return 'intern';
    case DostavljacTip.Freelancer: return 'freelancer';
    default: return '';
  }
}
```

**Zašto:** HTML ne može lako prikazati enum broj (1, 2, 3) — helper pretvara u tekst i CSS klasu.

**Obrazac:** slično kao switch u drugim projektima; ovdje nema gotovog primjera u repo-u — kopiraš iz zadatka.

**HTML će zvati:**
```html
<span class="tip-badge" [ngClass]="getTipClass(item.tip)">
  {{ getTipLabel(item.tip) }}
</span>
```

---

## KORAK 3.2 — SCSS: badge i tabela

**Gdje:** `dostavljaci.component.scss` — **na kraj fajla** (poslije postojećeg responsive bloka)

Starter već ima `.container`, `.header-card`, `.info-card` — **ne briši to još**.

### 3.2a — Dodaj na kraj SCSS fajla:

```scss
.table-card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(73, 118, 181, 0.08);
  border: 1px solid rgba(73, 118, 181, 0.15);
}

.tip-badge {
  display: inline-block;
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;

  &.ekstern {
    background-color: #e3f2fd;
    color: #1565c0;
  }

  &.intern {
    background-color: #e8f5e9;
    color: #2e7d32;
  }

  &.freelancer {
    background-color: #fff3e0;
    color: #e65100;
  }
}

.icon-enabled { color: #66bb6a; }
.icon-disabled { color: #ef5350; }
```

**Odakle:**
- `.table-card` — iz zadatka (u `products` se koristi klasa `.mat-elevation-z8`, ti koristiš `.table-card` kao u zadatku)
- `.icon-enabled` / `.icon-disabled` — iste boje kao u `products.component.scss` (linije 252–264), samo kraća verzija
- `.tip-badge` — **samo iz zadatka**, nema u products

---

## KORAK 3.3 — HTML: red po red

**Gdje:** `dostavljaci.component.html` — zamijeni **cijeli sadržaj** fajla.

Radi u dijelovima — ne moraš odjednom.

---

### 3.3a — Header (već imaš — samo poveži)

**Tvoj starter** već ima lijep header. Trebaš samo **2 bindinga**:

**U `<input>` dodaj** (obrazac: `product-categories-2.component.html` linija 3):
```html
[(ngModel)]="searchValue"
(keydown)="inputKeyDown($event)"
```

**U dugme dodaj** (obrazac: `products.component.html` linija 21):
```html
(click)="onCreate()"
```

**Input prije:**
```html
<input matInput placeholder="Pretraži dostavljače..." />
```

**Input poslije:**
```html
<input
  matInput
  placeholder="Pretraži dostavljače..."
  [(ngModel)]="searchValue"
  (keydown)="inputKeyDown($event)"
/>
```

**Napomena:** `FormsModule` je već u `SharedModule` — `ngModel` radi bez dodatnog importa.

---

### 3.3b — Obriši info-card

**Obriši cijeli blok:**
```html
<div class="info-card">
  <mat-icon>info</mat-icon>
  <span>Ovdje raditi ispitni zadatak - prvi modul</span>
</div>
```

To je bilo samo za starter — u finalnoj verziji nema ga.

---

### 3.3c — Loading (dodaj POSLIJE header-card)

**Obrazac:** `product-categories-2.component.html` linije 10–13

```html
<div *ngIf="isLoading" class="loading-container">
  <mat-spinner diameter="40"></mat-spinner>
  <p>Učitavanje...</p>
</div>
```

`isLoading` dolazi iz `BaseComponent` (bazna klasa) — ne moraš ništa dodavati u TS.

---

### 3.3d — No data (dodaj POSLIJE loading)

**Obrazac:** `product-categories-2.component.html` linije 5–7, prošireno:

```html
<div *ngIf="!isLoading && totalItems === 0" class="no-data">
  <img src="images/no-data.png" alt="Nema podataka" />
  <p>Nema dostavljača za prikaz.</p>
</div>
```

`totalItems` dolazi iz `BaseListPagedComponent`.

---

### 3.3e — Tabela (glavni dio)

**Obrazac:** `products.component.html` linije 39–146 — ista struktura, druga imena kolona.

**Omotač tabele:**
```html
<div class="table-card" *ngIf="!isLoading && totalItems > 0">
  <table mat-table [dataSource]="items">
    <!-- kolone ovdje -->
    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
    <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
  </table>

  <app-fit-paginator-bar [vm]="this" />
</div>
```

**Razlika od products:**
- products koristi `class="mat-elevation-z8"` — ti `class="table-card"`
- products u `*matHeaderRowDef` ima dugi inline niz — ti koristiš `displayedColumns` iz TS (čistije)

**Paginator obrazac:** `product-categories-2.component.html` linija 47 ili `products.component.html` linija 145 — identično: `<app-fit-paginator-bar [vm]="this" />`

---

### 3.3f — Kolona NAZIV

**Obrazac:** `products.component.html` kolona `name` (linije 42–47)

```html
<ng-container matColumnDef="naziv">
  <th mat-header-cell *matHeaderCellDef>NAZIV</th>
  <td mat-cell *matCellDef="let item">
    <span style="font-weight: 500">{{ item.naziv }}</span>
  </td>
</ng-container>
```

`matColumnDef="naziv"` **mora** biti isto kao string u `displayedColumns` u TS.

---

### 3.3g — Kolona KOD

```html
<ng-container matColumnDef="kod">
  <th mat-header-cell *matHeaderCellDef>KOD</th>
  <td mat-cell *matCellDef="let item">
    <span style="font-weight: 600">{{ item.kod }}</span>
  </td>
</ng-container>
```

**Obrazac:** kao `price` kolona u products (bold span).

---

### 3.3h — Kolona TIP (badge) — NOVO, nema u products

```html
<ng-container matColumnDef="tip">
  <th mat-header-cell *matHeaderCellDef>TIP</th>
  <td mat-cell *matCellDef="let item">
    <span class="tip-badge" [ngClass]="getTipClass(item.tip)">
      {{ getTipLabel(item.tip) }}
    </span>
  </td>
</ng-container>
```

**Odakle:** zadatak + helper metode iz Koraka 3.1.  
`[ngClass]` dodaje klasu `ekstern` / `intern` / `freelancer` → SCSS boji badge.

---

### 3.3i — Kolona AKTIVAN

**Obrazac:** `products.component.html` linije 81–87 — kopiraj strukturu, zamijeni `isEnabled` → `aktivan`

```html
<ng-container matColumnDef="aktivan">
  <th mat-header-cell *matHeaderCellDef>AKTIVAN</th>
  <td mat-cell *matCellDef="let item">
    <mat-icon [ngClass]="item.aktivan ? 'icon-enabled' : 'icon-disabled'">
      {{ item.aktivan ? 'check_circle' : 'cancel' }}
    </mat-icon>
  </td>
</ng-container>
```

---

### 3.3j — Kolona AKCIJE

**Obrazac:** `products.component.html` linije 91–110 — zamijeni `onEdit`/`onDelete` sa tvojim metodama

```html
<ng-container matColumnDef="actions">
  <th mat-header-cell *matHeaderCellDef>AKCIJE</th>
  <td mat-cell *matCellDef="let item">
    <button mat-icon-button color="primary" (click)="editAction(item)" matTooltip="Uredi">
      <mat-icon>edit</mat-icon>
    </button>
    <button mat-icon-button color="warn" (click)="deleteAction(item)" matTooltip="Obriši">
      <mat-icon>delete</mat-icon>
    </button>
  </td>
</ng-container>
```

| products | dostavljaci |
|----------|-------------|
| `onEdit(product)` | `editAction(item)` |
| `onDelete(product)` | `deleteAction(item)` |
| `[matTooltip]="'...' \| translate"` | `matTooltip="Uredi"` (direktan tekst) |

---

## KORAK 3.4 — Cijeli HTML (referenca)

Kad sve spojiš, fajl izgleda ovako:

```html
<div class="container">
  <div class="header-card">
    <div class="header-card-content">
      <div class="title-section">
        <div class="title-icon">
          <mat-icon>local_shipping</mat-icon>
        </div>
        <h1>Dostavljači</h1>
      </div>

      <div class="actions-container">
        <mat-form-field class="search-field" appearance="outline">
          <mat-label>Pretraga...</mat-label>
          <input
            matInput
            placeholder="Pretraži dostavljače..."
            [(ngModel)]="searchValue"
            (keydown)="inputKeyDown($event)"
          />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>

        <button mat-raised-button color="primary" (click)="onCreate()">
          <mat-icon>add</mat-icon>
          Novi dostavljač
        </button>
      </div>
    </div>
  </div>

  <div *ngIf="isLoading" class="loading-container">
    <mat-spinner diameter="40"></mat-spinner>
    <p>Učitavanje...</p>
  </div>

  <div *ngIf="!isLoading && totalItems === 0" class="no-data">
    <img src="images/no-data.png" alt="Nema podataka" />
    <p>Nema dostavljača za prikaz.</p>
  </div>

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
          <button mat-icon-button color="warn" (click)="deleteAction(item)" matTooltip="Obriši">
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

---

## KORAK 3.5 — Kako HTML zna za TS (veza)

| U HTML pišeš | Šta to zove u TS |
|--------------|------------------|
| `[(ngModel)]="searchValue"` | property `searchValue` |
| `(keydown)="inputKeyDown($event)"` | metoda `inputKeyDown` |
| `(click)="onCreate()"` | metoda `onCreate` |
| `*ngIf="isLoading"` | iz `BaseComponent` |
| `totalItems === 0` | iz `BaseListPagedComponent` |
| `[dataSource]="items"` | iz `BaseListComponent` |
| `displayedColumns` | tvoj niz u TS |
| `getTipLabel(item.tip)` | helper metoda |
| `(click)="editAction(item)"` | metoda `editAction` |
| `(click)="deleteAction(item)"` | metoda `deleteAction` |
| `[vm]="this"` | cijela komponenta — paginator zove `goToPage`, `totalPages`... |

---

## CHECKLIST — Korak 3

- [ ] TS: import `DostavljacTip` + `getTipLabel` + `getTipClass`
- [ ] SCSS: `.table-card`, `.tip-badge`, `.icon-enabled`, `.icon-disabled`
- [ ] HTML: search povezan sa `searchValue` i `inputKeyDown`
- [ ] HTML: dugme povezano sa `onCreate()`
- [ ] HTML: obrišan `info-card`
- [ ] HTML: loading, no-data, tabela sa 5 kolona
- [ ] `matColumnDef` imena = `displayedColumns` u TS
- [ ] Paginator `<app-fit-paginator-bar [vm]="this" />`
- [ ] Nema crvenih grešaka

---

## Redoslijed rada — Korak 3

1. TS helper metode (3.1)
2. SCSS na kraj fajla (3.2)
3. HTML: binding na header (3.3a)
4. HTML: obriši info-card (3.3b)
5. HTML: loading + no-data (3.3c, 3.3d)
6. HTML: tabela kolona po kolona (3.3e–3.3j)
7. Pokreni app, otvori `/admin/dostavljaci`, provjeri tabelu

---

## Česte greške — Korak 3

| Greška | Rješenje |
|--------|----------|
| Tabela prazna ali nema greške | Provjeri `baseUrl` u API servisu (`/Dostavljaci`) |
| `ngModel` ne radi | `SharedModule` mora biti u `admin-module` — već jest |
| Kolona se ne prikazuje | `matColumnDef="naziv"` mora biti u `displayedColumns` |
| Badge bez boje | Provjeri `getTipClass` + SCSS `.tip-badge.ekstern` itd. |
| Paginator ne radi | `[vm]="this"` — komponenta mora nasljeđivati `BaseListPagedComponent` |
| `getTipLabel` crveno | Dodaj import `DostavljacTip` |
