# Vodič: Add i Edit forma za Dostavljače (Angular)

Detaljan korak-po-korak vodič — odakle kopirati obrazac, kako povezati rute, i kompletan kod za Add i Edit komponente.

---

## Sadržaj

1. [Odakle uzeti obrazac](#1-odakle-uzeti-obrazac)
2. [Pregled koraka (redoslijed rada)](#2-pregled-koraka-redoslijed-rada)
3. [Korak 1 — Generiši komponente](#3-korak-1--generiši-komponente)
4. [Korak 2 — API model i servis](#4-korak-2--api-model-i-servis)
5. [Korak 3 — Registracija u modul i rute](#5-korak-3--registracija-u-modul-i-rute)
6. [Korak 4 — BaseFormComponent (šta već dobiješ besplatno)](#6-korak-4--baseformcomponent-šta-već-dobiješ-besplatno)
7. [Korak 5 — Add komponenta (TS + HTML)](#7-korak-5--add-komponenta-ts--html)
8. [Korak 6 — Edit komponenta (TS + HTML)](#8-korak-6--edit-komponenta-ts--html)
9. [Korak 7 — SCSS stilovi](#9-korak-7--scss-stilovi)
10. [Korak 8 — Navigacija sa liste](#10-korak-8--navigacija-sa-liste)
11. [Razlika Add vs Edit (sažetak)](#11-razlika-add-vs-edit-sažetak)
12. [Checklist prije testiranja](#12-checklist-prije-testiranja)

---

## 1. Odakle uzeti obrazac

U projektu već postoje gotovi CRUD obrasci koje kopiraš i prilagodiš poljima za dostavljača.

### Glavni uzor — Products modul (preporučeno)

| Šta tražiš | Gdje u projektu |
|---|---|
| Add forma (TS) | `src/app/modules/admin/catalogs/products/products-add/products-add.component.ts` |
| Add forma (HTML) | `src/app/modules/admin/catalogs/products/products-add/products-add.component.html` |
| Add forma (SCSS) | `src/app/modules/admin/catalogs/products/products-add/products-add.component.scss` |
| Edit forma (TS) | `src/app/modules/admin/catalogs/products/products-edit/products-edit.component.ts` |
| Edit ruta | `admin-routing-module.ts` → `products/:id/edit` |
| Navigacija na edit | `products.component.ts` → `onEdit()` |

### Bazna klasa za sve forme

| Šta | Gdje |
|---|---|
| `BaseFormComponent` | `src/app/core/components/base-classes/base-form-component.ts` |
| `BaseComponent` (loading, errorMessage) | `src/app/core/components/base-classes/base-component.ts` |

`BaseFormComponent` ti daje:
- `form`, `isEditMode`, `model`
- `onSubmit()` — validacija + poziv `save()`
- `hasError()` — prikaz grešaka u HTML-u
- `initForm(isEdit)` — u edit modu automatski poziva `loadData()`

### API sloj (mora postojati prije forme)

| Šta | Gdje |
|---|---|
| Modeli (enum, command, dto) | `src/app/api-services/dostavljaci/dostavljac-api.model.ts` |
| HTTP servis | `src/app/api-services/dostavljaci/dostavljac-api.services.ts` |

### Toast poruke

| Šta | Gdje |
|---|---|
| `ToasterService` | `src/app/core/services/toaster.service.ts` |

---

## 2. Pregled koraka (redoslijed rada)

```
Backend (entitet, enum, CQRS, controller)
        ↓
API model + DostavljaciApiService (frontend)
        ↓
ng g c ... → Add i Edit komponente
        ↓
Registracija u admin-module.ts + admin-routing-module.ts
        ↓
Add TS/HTML/SCSS
        ↓
Edit TS/HTML/SCSS (HTML skoro identičan Add-u)
        ↓
Lista → dugme "Novi" + ikona "Uredi" (navigacija)
        ↓
Test: dodaj → uredi → sačuvaj → toast → povratak na listu
```

---

## 3. Korak 1 — Generiši komponente

U terminalu (CMD, ne PowerShell — vidi upute ispita):

```bash
cd rs1-frontend-2025-26
npx ng g c modules/admin/catalogs/dostavljaci/dostavljaci-add --skip-tests
npx ng g c modules/admin/catalogs/dostavljaci/dostavljaci-edit --skip-tests
```

**Napomena o imenima:** Angular CLI generiše `DostavljaciAddComponent` / `DostavljaciEditComponent`. U primjerima ispod koristimo logička imena `DostavljacAddComponent` — bitno je da **selector, class name i fajlovi budu konzistentni** unutar tvog projekta.

**Napomena o import putanjama:** Komponente su u folderu:
```
src/app/modules/admin/catalogs/dostavljaci/dostavljaci-add/
```
Import do `core` ide sa **5** nivoa gore:
```typescript
import { BaseFormComponent } from '../../../../../core/components/base-classes/base-form-component';
```
(Ako si u starter folderu `modules/catalog/dostavljaci/`, tada su **4** nivoa: `../../../../`)

---

## 4. Korak 2 — API model i servis

Prije pisanja forme provjeri da postoje ovi tipovi u `dostavljac-api.model.ts`:

```typescript
export enum DostavljacTip {
  Ekstern = 1,
  Intern = 2,
  Freelancer = 3,
}

export interface CreateDostavljacCommand {
  naziv: string;
  kod: string;
  tip: DostavljacTip;
  aktivan: boolean;
}

export interface UpdateDostavljacCommand {
  naziv: string;
  kod: string;
  tip: DostavljacTip;
  aktivan: boolean;
}

export interface GetDostavljacByIdQueryDto {
  id: number;
  naziv: string;
  kod: string;
  tip: DostavljacTip;
  aktivan: boolean;
}
```

Servis (`DostavljaciApiService`) mora imati metode:
- `create(payload)` → POST
- `getById(id)` → GET (za edit)
- `update(id, payload)` → PUT

**Uzor:** kopiraj strukturu iz `src/app/api-services/products/products-api.service.ts`.

---

## 5. Korak 3 — Registracija u modul i rute

### 5.1 `admin-module.ts`

Dodaj u `declarations`:

```typescript
import { DostavljaciAddComponent } from './catalogs/dostavljaci/dostavljaci-add/dostavljaci-add.component';
import { DostavljaciEditComponent } from './catalogs/dostavljaci/dostavljaci-edit/dostavljaci-edit.component';

// u @NgModule declarations:
DostavljaciAddComponent,
DostavljaciEditComponent,
```

### 5.2 `admin-routing-module.ts`

**Važno:** Edit ruta **mora imati `:id`**, kao kod products:

```typescript
import { DostavljaciAddComponent } from './catalogs/dostavljaci/dostavljaci-add/dostavljaci-add.component';
import { DostavljaciEditComponent } from './catalogs/dostavljaci/dostavljaci-edit/dostavljaci-edit.component';

// u routes children:
{
  path: 'dostavljaci/add',
  component: DostavljaciAddComponent,
},
{
  path: 'dostavljaci/:id/edit',   // ← :id je obavezan!
  component: DostavljaciEditComponent,
},
```

---

## 6. Korak 4 — BaseFormComponent (šta već dobiješ besplatno)

Tvoja komponenta **extends BaseFormComponent** i mora implementirati:

| Metoda | Add | Edit |
|---|---|---|
| `loadData()` | prazna (komentar) | učitava podatke sa API-ja |
| `save()` | `api.create()` | `api.update()` |
| `ngOnInit()` | `initForm(false)` + kreiraj formu | pročitaj `id` iz rute + `initForm(true)` |

HTML koristi:
- `[formGroup]="form"` — reactive form
- `(ngSubmit)="onSubmit()"` — submit iz base klase
- `hasError('naziv')` + `getErrorMessage('naziv')` — validacija
- `[disabled]="form.invalid \|\| isLoading"` — Save disabled dok forma nije validna

---

## 7. Korak 5 — Add komponenta (TS + HTML)

### 7.1 `dostavljac-add.component.ts`

```typescript
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BaseFormComponent } from '../../../../../core/components/base-classes/base-form-component';
import { DostavljaciApiService } from '../../../../../api-services/dostavljaci/dostavljac-api.services';
import {
  CreateDostavljacCommand,
  DostavljacTip,
  GetDostavljacByIdQueryDto,
} from '../../../../../api-services/dostavljaci/dostavljac-api.model';
import { ToasterService } from '../../../../../core/services/toaster.service';

@Component({
  selector: 'app-dostavljac-add',
  standalone: false,
  templateUrl: './dostavljac-add.component.html',
  styleUrl: './dostavljac-add.component.scss',
})
export class DostavljacAddComponent
  extends BaseFormComponent<GetDostavljacByIdQueryDto>
  implements OnInit
{
  private api = inject(DostavljaciApiService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private fb = inject(FormBuilder);

  DostavljacTip = DostavljacTip;
  tipOptions = [
    { value: DostavljacTip.Ekstern, label: 'Ekstern' },
    { value: DostavljacTip.Intern, label: 'Intern' },
    { value: DostavljacTip.Freelancer, label: 'Freelancer' },
  ];

  ngOnInit(): void {
    this.form = this.fb.group({
      naziv: ['', [Validators.required]],
      tip: [null, [Validators.required]],
      kod: ['', [Validators.required, Validators.maxLength(3)]],
      aktivan: [true],
    });

    this.initForm(false); // Add mode — ne poziva loadData()
  }

  protected loadData(): void {
    // Nije potrebno u add modu
  }

  protected save(): void {
    if (this.form.invalid || this.isLoading) {
      return;
    }

    this.startLoading();

    const command: CreateDostavljacCommand = {
      naziv: this.form.value.naziv?.trim(),
      kod: this.form.value.kod?.trim(),
      tip: this.form.value.tip,
      aktivan: this.form.value.aktivan ?? true,
    };

    this.api.create(command).subscribe({
      next: () => {
        this.stopLoading();
        this.toaster.success('Dostavljač uspješno dodan.');
        this.router.navigate(['/admin/dostavljaci']);
      },
      error: (err) => {
        this.stopLoading(err?.message ?? 'Greška pri dodavanju.');
        this.toaster.error(err?.message ?? 'Greška pri dodavanju.');
        console.error('Create dostavljac error:', err);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/dostavljaci']);
  }

  getErrorMessage(controlName: string): string {
    const control = this.form.get(controlName);
    if (!control || !control.errors) return '';

    if (control.errors['required']) return 'Polje je obavezno.';
    if (control.errors['maxlength']) return 'Kod može imati najviše 3 karaktera.';
    return 'Neispravna vrijednost.';
  }
}
```

**Objašnjenje ključnih dijelova:**

| Dio koda | Zašto |
|---|---|
| `extends BaseFormComponent<GetDostavljacByIdQueryDto>` | Dobijaš `onSubmit()`, `hasError()`, loading |
| `aktivan: [true]` | Ispit traži default `true` |
| `Validators.maxLength(3)` | Kod max 3 karaktera |
| `this.form.invalid \|\| isLoading` u save | Dupla zaštita |
| `trim()` na naziv/kod | Čisti razmake prije slanja |
| `initForm(false)` | Add mod — **ne** učitava podatke |

### 7.2 `dostavljac-add.component.html`

```html
<div class="container">
  <div class="header-card">
    <h1>Novi dostavljač</h1>
  </div>

  <div class="form-card">
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <div *ngIf="errorMessage" class="error-banner">
        <mat-icon>error</mat-icon>
        <span>{{ errorMessage }}</span>
      </div>

      <div *ngIf="isLoading" class="loading-overlay">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Spremanje...</p>
      </div>

      <!-- Naziv -->
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Naziv</mat-label>
        <input matInput formControlName="naziv" placeholder="Unesite naziv" />
        <mat-error *ngIf="hasError('naziv')">{{ getErrorMessage('naziv') }}</mat-error>
      </mat-form-field>

      <!-- Kod -->
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Kod</mat-label>
        <input matInput formControlName="kod" placeholder="Max 3 karaktera" maxlength="3" />
        <mat-error *ngIf="hasError('kod')">{{ getErrorMessage('kod') }}</mat-error>
      </mat-form-field>

      <!-- Tip (enum) -->
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Tip dostavljača</mat-label>
        <mat-select formControlName="tip">
          <mat-option *ngFor="let opt of tipOptions" [value]="opt.value">
            {{ opt.label }}
          </mat-option>
        </mat-select>
        <mat-error *ngIf="hasError('tip')">{{ getErrorMessage('tip') }}</mat-error>
      </mat-form-field>

      <!-- Aktivan -->
      <mat-checkbox formControlName="aktivan">Aktivan</mat-checkbox>

      <!-- Akcije -->
      <div class="form-actions">
        <button type="button" mat-stroked-button (click)="onCancel()" [disabled]="isLoading">
          <mat-icon>close</mat-icon>
          Odustani
        </button>

        <button
          type="submit"
          mat-raised-button
          color="primary"
          [disabled]="form.invalid || isLoading"
        >
          <mat-icon>save</mat-icon>
          Sačuvaj
        </button>
      </div>
    </form>
  </div>
</div>
```

---

## 8. Korak 6 — Edit komponenta (TS + HTML)

Edit je **95% isti kao Add**, sa ovim razlikama:

1. U `ngOnInit` — pročitaš `id` iz rute
2. `initForm(true)` — automatski poziva `loadData()`
3. `loadData()` — `api.getById()` + `form.patchValue()`
4. `save()` — `api.update(id, command)` umjesto `create`

### 8.1 `dostavljac-edit.component.ts`

```typescript
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseFormComponent } from '../../../../../core/components/base-classes/base-form-component';
import { DostavljaciApiService } from '../../../../../api-services/dostavljaci/dostavljac-api.services';
import {
  DostavljacTip,
  GetDostavljacByIdQueryDto,
  UpdateDostavljacCommand,
} from '../../../../../api-services/dostavljaci/dostavljac-api.model';
import { ToasterService } from '../../../../../core/services/toaster.service';

@Component({
  selector: 'app-dostavljac-edit',
  standalone: false,
  templateUrl: './dostavljac-edit.component.html',
  styleUrl: './dostavljac-edit.component.scss',
})
export class DostavljacEditComponent
  extends BaseFormComponent<GetDostavljacByIdQueryDto>
  implements OnInit
{
  private api = inject(DostavljaciApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private fb = inject(FormBuilder);

  dostavljacId!: number;

  tipOptions = [
    { value: DostavljacTip.Ekstern, label: 'Ekstern' },
    { value: DostavljacTip.Intern, label: 'Intern' },
    { value: DostavljacTip.Freelancer, label: 'Freelancer' },
  ];

  ngOnInit(): void {
    this.dostavljacId = +this.route.snapshot.params['id'];

    if (!this.dostavljacId || this.dostavljacId <= 0) {
      this.toaster.error('Neispravan ID dostavljača.');
      this.router.navigate(['/admin/dostavljaci']);
      return;
    }

    // Ista forma kao Add — MORA biti prije initForm(true)
    this.form = this.fb.group({
      naziv: ['', [Validators.required]],
      tip: [null, [Validators.required]],
      kod: ['', [Validators.required, Validators.maxLength(3)]],
      aktivan: [true],
    });

    this.initForm(true); // Edit mode → automatski poziva loadData()
  }

  protected loadData(): void {
    this.startLoading();

    this.api.getById(this.dostavljacId).subscribe({
      next: (response) => {
        this.model = response;
        this.form.patchValue({
          naziv: response.naziv,
          kod: response.kod,
          tip: response.tip,
          aktivan: response.aktivan,
        });
        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading(err?.message ?? 'Dostavljač nije pronađen.');
        this.toaster.error(err?.message ?? 'Dostavljač nije pronađen.');
        console.error('Load dostavljac error:', err);
        this.router.navigate(['/admin/dostavljaci']);
      },
    });
  }

  protected save(): void {
    if (this.form.invalid || this.isLoading) {
      return;
    }

    this.startLoading();

    const command: UpdateDostavljacCommand = {
      naziv: this.form.value.naziv?.trim(),
      kod: this.form.value.kod?.trim(),
      tip: this.form.value.tip,
      aktivan: this.form.value.aktivan ?? true,
    };

    this.api.update(this.dostavljacId, command).subscribe({
      next: () => {
        this.stopLoading();
        this.toaster.success('Dostavljač uspješno ažuriran.');
        this.router.navigate(['/admin/dostavljaci']);
      },
      error: (err) => {
        this.stopLoading(err?.message ?? 'Greška pri ažuriranju.');
        this.toaster.error(err?.message ?? 'Greška pri ažuriranju.');
        console.error('Update dostavljac error:', err);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/dostavljaci']);
  }

  getErrorMessage(controlName: string): string {
    const control = this.form.get(controlName);
    if (!control || !control.errors) return '';

    if (control.errors['required']) return 'Polje je obavezno.';
    if (control.errors['maxlength']) return 'Kod može imati najviše 3 karaktera.';
    return 'Neispravna vrijednost.';
  }
}
```

### 8.2 `dostavljac-edit.component.html` — CIJELI KOD

Ovo je **kompletan** HTML (nije skraćen). Razlike u odnosu na Add:
- Naslov: "Uredi dostavljača"
- Prikaz ID-a (opciono)
- Spinner dok se učitavaju podaci (`isLoading && !model`)
- Forma se prikazuje tek kad su podaci učitani

```html
<div class="container">
  <div class="header-card">
    <h1>Uredi dostavljača</h1>
    <p *ngIf="model">ID: {{ model.id }}</p>
  </div>

  <!-- Spinner dok API učitava podatke -->
  <div *ngIf="isLoading && !model" class="loading-container">
    <mat-spinner diameter="50"></mat-spinner>
    <p>Učitavanje...</p>
  </div>

  <!-- Forma — prikazuje se kad su podaci učitani -->
  <div class="form-card" *ngIf="model">
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <div *ngIf="errorMessage" class="error-banner">
        <mat-icon>error</mat-icon>
        <span>{{ errorMessage }}</span>
      </div>

      <div *ngIf="isLoading" class="loading-overlay">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Spremanje...</p>
      </div>

      <!-- Naziv -->
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Naziv</mat-label>
        <input matInput formControlName="naziv" placeholder="Unesite naziv" />
        <mat-error *ngIf="hasError('naziv')">{{ getErrorMessage('naziv') }}</mat-error>
      </mat-form-field>

      <!-- Kod -->
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Kod</mat-label>
        <input matInput formControlName="kod" placeholder="Max 3 karaktera" maxlength="3" />
        <mat-error *ngIf="hasError('kod')">{{ getErrorMessage('kod') }}</mat-error>
      </mat-form-field>

      <!-- Tip (enum) -->
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Tip dostavljača</mat-label>
        <mat-select formControlName="tip">
          <mat-option *ngFor="let opt of tipOptions" [value]="opt.value">
            {{ opt.label }}
          </mat-option>
        </mat-select>
        <mat-error *ngIf="hasError('tip')">{{ getErrorMessage('tip') }}</mat-error>
      </mat-form-field>

      <!-- Aktivan -->
      <mat-checkbox formControlName="aktivan">Aktivan</mat-checkbox>

      <!-- Akcije -->
      <div class="form-actions">
        <button type="button" mat-stroked-button (click)="onCancel()" [disabled]="isLoading">
          <mat-icon>close</mat-icon>
          Odustani
        </button>

        <button
          type="submit"
          mat-raised-button
          color="primary"
          [disabled]="form.invalid || isLoading"
        >
          <mat-icon>save</mat-icon>
          Sačuvaj
        </button>
      </div>
    </form>
  </div>
</div>
```

**Zašto dva loading stanja u Edit HTML-u?**

| Stanje | Kada | Šta prikazuje |
|---|---|---|
| `isLoading && !model` | Prvo učitavanje sa API-ja | Spinner cijele stranice |
| `isLoading` unutar forme | Klik na Sačuvaj | Overlay preko forme |

---

## 9. Korak 7 — SCSS stilovi

Kopiraj sadržaj iz:
```
src/app/modules/admin/catalogs/products/products-add/products-add.component.scss
```

u oba fajla:
```
dostavljaci-add/dostavljaci-add.component.scss
dostavljaci-edit/dostavljaci-edit.component.scss
```

Za Edit možeš promijeniti ikonu u `h1::after` (npr. `edit` umjesto `add_shopping_cart`).

Dodaj i `.loading-container` za Edit (spinner pri učitavanju):

```scss
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 48px;
  background: white;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(73, 118, 181, 0.08);
}
```

---

## 10. Korak 8 — Navigacija sa liste

U `dostavljaci.component.ts`:

```typescript
import { ActivatedRoute, Router } from '@angular/router';

// inject:
private router = inject(Router);
private route = inject(ActivatedRoute);

// Novi dostavljač:
onCreate(): void {
  this.router.navigate(['/admin/dostavljaci/add']);
}

// Uredi — ISTI pattern kao Products:
editAction(item: ListDostavljacQueryDto): void {
  this.router.navigate(['/admin/dostavljaci', item.id, 'edit']);
}
```

**Ne koristi** `['edit', item.id]` sa relativnom rutom — to ne matcha rutu `dostavljaci/:id/edit`.

U HTML-u liste:

```html
<button mat-icon-button color="primary" (click)="editAction(item)" matTooltip="Uredi">
  <mat-icon>edit</mat-icon>
</button>
```

---

## 11. Razlika Add vs Edit (sažetak)

| | Add | Edit |
|---|---|---|
| Ruta | `/admin/dostavljaci/add` | `/admin/dostavljaci/5/edit` |
| `initForm()` | `initForm(false)` | `initForm(true)` |
| `loadData()` | prazna | `getById` + `patchValue` |
| `save()` | `api.create()` | `api.update(id, ...)` |
| Naslov | Novi dostavljač | Uredi dostavljača |
| ID iz rute | ne treba | `+this.route.snapshot.params['id']` |
| HTML | forma odmah vidljiva | spinner pa forma |

---

## 12. Checklist prije testiranja

- [ ] `DostavljaciApiService` ima `create`, `getById`, `update`
- [ ] Komponente registrovane u `admin-module.ts`
- [ ] Rute: `dostavljaci/add` i `dostavljaci/:id/edit`
- [ ] Edit navigacija: `['/admin/dostavljaci', item.id, 'edit']`
- [ ] Backend `UpdateDostavljacCommandHandler` **implementiran** (bez toga Save na edit ne radi)
- [ ] Save disabled: `[disabled]="form.invalid || isLoading"`
- [ ] Toast na uspjeh i grešku
- [ ] Povratak na listu nakon save/cancel
- [ ] `getErrorMessage` koristi `'maxlength'` (malo slovo!)

### Brzi test

1. Lista → "+ Novi dostavljač" → popuni formu → Sačuvaj → toast → lista
2. Lista → ikona olovke → forma popunjena → promijeni naziv → Sačuvaj → toast → lista
3. Ostavi Naziv prazan → Sačuvaj disabled
4. Unesi 4 karaktera u Kod → greška validacije

---

*Vodič pripadajući projektu RS1 Market — Modul 1, CRUD Dostavljači.*
