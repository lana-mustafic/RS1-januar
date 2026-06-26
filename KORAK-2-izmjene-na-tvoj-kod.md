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
