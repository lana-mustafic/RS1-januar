# Pregled implementacije: Dostavljači (Zadatak 1)

**Datum pregleda:** 29.06.2026  
**Projekat:** `rs1_backend-2025-26` + `rs1-frontend-2025-26`  
**Napomena:** Kod nije mijenjan — ovo je samo analiza u odnosu na zahtjeve ispita sa slika.

---

## Ukupna procjena

Implementacija je **solidno započeta** i pokriva većinu zahtjeva (CQRS struktura, entitet, enum, migracija, lista, dodavanje, brisanje s modalom, reactive forms, paginacija, toast poruke). Međutim, postoje **nekoliko kritičnih problema** koji bi na ispitu vjerovatno koštali bodova — posebno **uređivanje (Update)**, koje trenutno **ne radi** ni na backendu ni na frontend rutiranju.

| Kategorija | Procjena |
|---|---|
| Backend – struktura (CQRS, entitet, enum) | ✅ Uglavnom OK |
| Backend – CRUD funkcionalnost | ⚠️ Create/Read/Delete OK, **Update ne radi** |
| Frontend – lista, pretraga, paginacija | ✅ Uglavnom OK (sitne greške) |
| Frontend – dodavanje | ✅ Uglavnom OK |
| Frontend – uređivanje | ❌ Rutiranje + backend handler |
| Frontend – brisanje | ✅ OK |
| Validacija (FE + BE) | ⚠️ Djelimično (nedostaje alfanumerički kod) |
| UX (toast, modal, disabled Save) | ✅ Uglavnom OK |

---

## Checklist prema zahtjevima ispita

### Opći zahtjevi

| Zahtjev | Status | Napomena |
|---|---|---|
| Backend Web API | ✅ | `DostavljaciController` postoji |
| Frontend Angular | ✅ | Modul u `admin/catalogs/dostavljaci` |
| CQRS pattern na backendu | ⚠️ | Struktura postoji, ali **Update handler je prazan** |
| Reactive Forms (create/update) | ✅ | `FormBuilder` + `FormGroup` |
| Toast poruke za uspjeh/grešku | ⚠️ | Postoje, ali ima bugova (vidi ispod) |
| Paginacija na listi | ✅ | `BaseListPagedComponent` + `app-fit-paginator-bar` |
| Confirmation modal pri brisanju | ✅ | `dialogHelper.confirmDelete()` |

### Lista (Dostavljači)

| Zahtjev | Status | Napomena |
|---|---|---|
| Pristup preko sidebara "Dostavljači" | ✅ | `admin-layout.component.html` |
| Kolone: Naziv, Tip, Kod, Aktivan, Akcije | ✅ | Sve kolone prisutne |
| Pretraga po nazivu | ⚠️ | Enter + reset stranice OK; **backend pretraga nije case-insensitive** |
| Pretraga samo na Enter | ✅ | `inputKeyDown` |
| Prazan search → svi zapisi | ✅ | `search = null` |
| Paginacija uz filter | ✅ | `page = 1` pri pretrazi |
| Dugme "Novi dostavljač" | ✅ | Navigacija na add |
| Edit / Delete akcije | ⚠️ | Delete OK; **Edit ne radi** (ruta) |
| Badge boje za tip | ⚠️ | HTML postoji, ali **CSS za badge nedostaje** i prikazuje se pogrešan tekst |

### Dodavanje

| Zahtjev | Status | Napomena |
|---|---|---|
| Polja: Naziv, Tip, Kod, Aktivan | ✅ | |
| Tip kao enum | ✅ | `DostavljacTip` |
| Kod max 3 karaktera | ✅ | `maxLength(3)` + `maxlength="3"` |
| Kod alfanumerički | ❌ | Nema validacije (samo max dužina) |
| Kod jedinstven | ✅ | Backend provjera + unique index u bazi |
| Aktivan default `true` | ✅ | `[true]` u formi + default u command |
| Validacija FE + BE | ⚠️ | Postoji, ali **bug u poruci za maxlength** na Add |
| Save disabled ako forma invalid | ✅ | `[disabled]="form.invalid \|\| isLoading"` |
| Povratak na listu + toast | ✅ | |
| Ažurirana lista nakon save | ✅ | Navigacija na listu koja ponovo učitava |

### Uređivanje

| Zahtjev | Status | Napomena |
|---|---|---|
| Edit forma (ista polja kao Add) | ✅ | Komponenta postoji |
| Reactive form + validacija | ✅ | |
| Save + toast + povratak | ⚠️ | Logika postoji, ali **ne može raditi** zbog rute i handlera |
| Backend Update handler | ❌ | **Prazna klasa — nema implementacije** |

### Brisanje

| Zahtjev | Status | Napomena |
|---|---|---|
| Confirmation modal | ✅ | `dialogHelper.confirmDelete(item.naziv)` |
| Tekst "Da li ste sigurni da želite obrisati {{naziv}}?" | ✅ | Preko i18n (`bs.json`) |
| OTKAŽI / OBRIŠI dugmad | ✅ | `DialogButton.CANCEL` / `DELETE` |
| Nakon potvrde: delete + refresh + toast | ✅ | |
| Nakon otkazivanja: zatvori modal | ✅ | |

---

## Referentni kod — šta je dobro urađeno

### Backend: Entitet i enum

`Market.Domain/Entities/Dostavljaci/DostavljacEntity.cs`

```csharp
public class DostavljacEntity : BaseEntity
{
    public required string Naziv { get; set; }
    public required DostavljacTip Tip { get; set; }
    public required string Kod { get; set; }
    public bool Aktivan { get; set; }

    public static class Constraints
    {
        public const int NazivMaxLength = 100;
        public const int KodMaxLength = 3;
    }
}
```

`Market.Domain/Entities/Dostavljaci/DostavljacTip.cs`

```csharp
public enum DostavljacTip
{
    Ekstern = 1,
    Intern = 2,
    Freelancer = 3
}
```

### Backend: Controller (CQRS)

`Market.API/Controllers/DostavljaciController.cs`

```csharp
[ApiController]
[Route("[controller]")]
[Authorize(Policy = "AdminOnly")]
public class DostavljaciController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateDostavljacCommand command, CancellationToken ct)
    {
        int id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdateDostavljacCommand command, CancellationToken ct)
    {
        command.Id = id;
        await sender.Send(command, ct);
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteDostavljacCommand { Id = id }, ct);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<GetDostavljacByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        return await sender.Send(new GetDostavljacByIdQuery { Id = id }, ct);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<PageResult<ListDostavljacQueryDto>> List(
        [FromQuery] ListDostavljacQuery query, CancellationToken ct)
    {
        return await sender.Send(query, ct);
    }
}
```

### Backend: Create handler (radi ispravno)

`CreateDostavljacCommandHandler.cs`

```csharp
public async Task<int> Handle(CreateDostavljacCommand request, CancellationToken cancellationToken)
{
    var naziv = request.Naziv?.Trim();
    var kod = request.Kod?.Trim();

    if (string.IsNullOrWhiteSpace(naziv))
        throw new ValidationException("Naziv je obavezno polje.");

    if (string.IsNullOrWhiteSpace(kod))
        throw new ValidationException("Kod je obavezno polje.");

    bool exists = await context.Dostavljaci
        .AnyAsync(x => x.Kod.ToLower() == kod.ToLower(), cancellationToken);

    if (exists)
        throw new MarketConflictException("Kod vec postoji.");

    var dto = new DostavljacEntity
    {
        Naziv = naziv,
        Kod = kod,
        Tip = request.Tip,
        Aktivan = request.Aktivan
    };

    context.Dostavljaci.Add(dto);
    await context.SaveChangesAsync(cancellationToken);
    return dto.Id;
}
```

### Frontend: Pretraga na Enter (radi ispravno)

`dostavljaci.component.ts`

```typescript
searchAction(): void {
  this.request.search = this.searchValue?.trim() || null;
  this.request.paging.page = 1;
  this.loadPagedData();
}

inputKeyDown(event: KeyboardEvent) {
  if (event.key === 'Enter') {
    this.searchAction();
  }
}
```

### Frontend: Brisanje s modalom (radi ispravno)

`dostavljaci.component.ts`

```typescript
deleteAction(item: ListDostavljacQueryDto) {
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
      this.toaster.success(`Dostavljac "${item.naziv}" je uspjesno obrisan.`);
      this.loadPagedData();
    },
    error: (err) => {
      this.stopLoading();
      this.toaster.error(err?.message ?? 'Greska pri brisanju dostavljaca');
    }
  });
}
```

### Frontend: Add forma — reactive form (uglavnom OK)

`dostavljaci-add.component.ts`

```typescript
ngOnInit(): void {
  this.initForm(false);
  this.form = this.fb.group({
    naziv: ['', [Validators.required]],
    tip: [null, [Validators.required]],
    kod: ['', [Validators.required, Validators.maxLength(3)]],
    aktivan: [true]
  });
}

protected save(): void {
  if (this.form.invalid || this.isLoading) return;

  this.startLoading();
  const command: CreateDostavljacCommand = {
    naziv: this.form.value.naziv?.trim(),
    kod: this.form.value.kod?.trim(),
    tip: this.form.value.tip,
    aktivan: this.form.value.aktivan ?? true
  };

  this.api.create(command).subscribe({
    next: () => {
      this.stopLoading();
      this.toaster.success('Dostavljac dodan uspjesno.');
      this.router.navigate(['/admin/dostavljaci']);
    },
    error: (err) => {
      this.toaster.error(err?.message ?? 'Greska pri dodavanju novog dostavljaca');
    }
  });
}
```

`dostavljaci-add.component.html` — Save disabled:

```html
<button
  type="submit"
  mat-raised-button
  color="primary"
  [disabled]="form.invalid || isLoading"
>
  <mat-icon>save</mat-icon>
  Sačuvaj
</button>
```

---

## Kritične greške (prioritet: visok)

Ove stvari treba popraviti prije ispita — vjerovatno direktno utiču na bodovanje.

### 1. `UpdateDostavljacCommandHandler` je prazan

**Lokacija:** `Market.Application/Modules/Catalog/Dostavljaci/Commands/Update/UpdateDostavljacCommandHandler.cs`

**Trenutni kod (ne radi):**

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Update
{
    internal class UpdateDostavljacCommandHandler
    {
    }
}
```

**Posljedica:** PUT `/Dostavljaci/{id}` neće raditi — nema registriranog `IRequestHandler`.

**Predloženi ispravak:**

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Update;

public sealed class UpdateDostavljacCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateDostavljacCommand, Unit>
{
    public async Task<Unit> Handle(UpdateDostavljacCommand request, CancellationToken ct)
    {
        var entity = await context.Dostavljaci
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MarketNotFoundException("Dostavljac nije pronađen.");

        var naziv = request.Naziv?.Trim();
        var kod = request.Kod?.Trim();

        bool kodExists = await context.Dostavljaci
            .AnyAsync(x => x.Id != request.Id && x.Kod.ToLower() == kod!.ToLower(), ct);

        if (kodExists)
            throw new MarketConflictException("Kod vec postoji.");

        entity.Naziv = naziv!;
        entity.Kod = kod!;
        entity.Tip = request.Tip;
        entity.Aktivan = request.Aktivan;

        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
```

---

### 2. Pogrešna ruta za uređivanje (frontend)

**Lokacija:** `admin-routing-module.ts` + `dostavljaci.component.ts`

**Trenutni kod — ruta (pogrešno):**

```typescript
{
  path: 'dostavljaci/edit',   // nema :id
  component: DostavljaciEditComponent,
},
```

**Trenutni kod — navigacija (pogrešno):**

```typescript
editAction(item: ListDostavljacQueryDto): void {
  this.router.navigate(['edit', item.id], { relativeTo: this.route });
  // pokušava ići na /admin/dostavljaci/edit/5 — ruta ne postoji!
}
```

**Usporedba s Products (ispravno):**

```typescript
// admin-routing-module.ts
{ path: 'products/:id/edit', component: ProductsEditComponent }

// products.component.ts
onEdit(product: ListProductsQueryDto): void {
  this.router.navigate(['/admin/products', product.id, 'edit']);
}
```

**Predloženi ispravak:**

```typescript
// admin-routing-module.ts
{
  path: 'dostavljaci/:id/edit',
  component: DostavljaciEditComponent,
},

// dostavljaci.component.ts
editAction(item: ListDostavljacQueryDto): void {
  this.router.navigate(['/admin/dostavljaci', item.id, 'edit']);
}
```

Edit komponenta već čita ID ispravno — samo ruta i navigacija trebaju popravku:

```typescript
// dostavljaci-edit.component.ts
this.dostavljacId = +this.route.snapshot.params['id'];
```

---

### 3. Backend pretraga nije case-insensitive

**Lokacija:** `ListDostavljacQueryHandler.cs`

**Trenutni kod (pogrešno):**

```csharp
if (!string.IsNullOrWhiteSpace(request.Search))
{
    var s = request.Search.Trim();
    q = q.Where(x => x.Naziv.ToLower().Contains(s));  // s nije lowercase!
}
```

**Predloženi ispravak:**

```csharp
if (!string.IsNullOrWhiteSpace(request.Search))
{
    var s = request.Search.Trim().ToLower();
    q = q.Where(x => x.Naziv.ToLower().Contains(s));
}
```

---

## Srednje greške (prioritet: srednji)

### 4. U koloni TIP prikazuje se CSS klasa umjesto labela

**Lokacija:** `dostavljaci.component.html`

**Trenutni kod (pogrešno):**

```html
<span class="tip-badge" [ngClass]="getTipClass(item.tip)">
  {{ getTipClass(item.tip) }}
</span>
```

Prikazuje `ekstern`, `intern`, `freelancer` umjesto `Ekstern`, `Intern`, `Freelancer`.

**Predloženi ispravak:**

```html
<span class="tip-badge" [ngClass]="getTipClass(item.tip)">
  {{ getTipLabel(item.tip) }}
</span>
```

Metoda `getTipLabel()` već postoji u `dostavljaci.component.ts`:

```typescript
getTipLabel(tip: DostavljacTip): string {
  switch (tip) {
    case DostavljacTip.Ekstern: return 'Ekstern';
    case DostavljacTip.Intern: return 'Intern';
    case DostavljacTip.Freelancer: return 'Freelancer';
  }
}
```

---

### 5. Nedostaju stilovi za `tip-badge`

**Lokacija:** `dostavljaci.component.scss` — nema `.tip-badge` stilova.

**Predloženi ispravak** — dodaj u `dostavljaci.component.scss`:

```scss
.tip-badge {
  display: inline-block;
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;

  &.ekstern {
    background-color: #fff3e0;
    color: #e65100;
  }

  &.intern {
    background-color: #e8f5e9;
    color: #2e7d32;
  }

  &.freelancer {
    background-color: #e3f2fd;
    color: #1565c0;
  }
}
```

**Uzor:** `fakture.component.scss` ima sličan `.tip-badge` blok.

---

### 6. Greška pri učitavanju liste prikazuje `success` toast

**Lokacija:** `dostavljaci.component.ts`

**Trenutni kod (pogrešno):**

```typescript
error: (err) => {
  this.stopLoading();
  this.toaster.success(err?.message ?? 'Greska pri ucitavanju dostavljaca');
  console.error('Load dostavljaci error:', err);
}
```

**Predloženi ispravak:**

```typescript
error: (err) => {
  this.stopLoading();
  this.toaster.error(err?.message ?? 'Greska pri ucitavanju dostavljaca');
  console.error('Load dostavljaci error:', err);
}
```

---

### 7. Bug u poruci validacije za Kod (Add komponenta)

**Lokacija:** `dostavljaci-add.component.ts`

**Trenutni kod (pogrešno):**

```typescript
getErrorMessage(controlName: string): string {
  const control = this.form.get(controlName);
  if (!control || !control.errors) return '';

  if (control.errors['required']) return 'Polje je obavezno.';
  if (control.errors['maxLength']) return 'Broj karaktera neispravan.';  // pogrešan ključ!
  return 'Neispravna vrijednost.';
}
```

Angular validator vraća ključ **`maxlength`** (malo slovo), ne `maxLength`.

**Predloženi ispravak:**

```typescript
if (control.errors['maxlength']) return 'Kod može imati najviše 3 karaktera.';
```

---

### 8. `stopLoading()` se ne poziva nakon greške pri kreiranju

**Lokacija:** `dostavljaci-add.component.ts`

**Trenutni kod (pogrešno):**

```typescript
error: (err) => {
  this.toaster.error(err?.message ?? 'Greska pri dodavanju novog dostavljaca');
  console.error('Create dostavljac error:', err);
  // nedostaje stopLoading()!
}
```

**Predloženi ispravak:**

```typescript
error: (err) => {
  this.stopLoading(err?.message ?? 'Greska pri dodavanju novog dostavljaca');
  this.toaster.error(err?.message ?? 'Greska pri dodavanju novog dostavljaca');
  console.error('Create dostavljac error:', err);
}
```

---

### 9. Nedostaje validacija da je Kod alfanumerički

Ispit traži: *"maksimalno 3 alfanumerička karaktera"*.

**Trenutni kod — Frontend:**

```typescript
kod: ['', [Validators.required, Validators.maxLength(3)]],
```

**Trenutni kod — Backend validator:**

```csharp
RuleFor(x => x.Kod)
    .NotEmpty().WithMessage("Kod je obavezno polje.")
    .MaximumLength(DostavljacEntity.Constraints.KodMaxLength);
```

**Predloženi ispravak — Frontend:**

```typescript
import { Validators, FormBuilder, AbstractControl, ValidationErrors } from '@angular/forms';

function alphanumericValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value?.trim();
  if (!value) return null;
  return /^[a-zA-Z0-9]{1,3}$/.test(value) ? null : { alphanumeric: true };
}

// u formi:
kod: ['', [Validators.required, Validators.maxLength(3), alphanumericValidator]],
```

**Predloženi ispravak — Backend:**

```csharp
RuleFor(x => x.Kod)
    .NotEmpty()
    .MaximumLength(DostavljacEntity.Constraints.KodMaxLength)
    .Matches("^[a-zA-Z0-9]{1,3}$").WithMessage("Kod mora biti alfanumerički (max 3 karaktera).");
```

---

### 10. Update nema provjeru jedinstvenosti Koda

Create handler to radi:

```csharp
bool exists = await context.Dostavljaci
    .AnyAsync(x => x.Kod.ToLower() == kod.ToLower(), cancellationToken);
```

Update handler (kad se implementira) mora imati:

```csharp
bool kodExists = await context.Dostavljaci
    .AnyAsync(x => x.Id != request.Id && x.Kod.ToLower() == kod!.ToLower(), ct);
```

---

## Manje greške / kozmetika (prioritet: nizak)

| # | Problem | Lokacija | Kod / napomena |
|---|---|---|---|
| 11 | Starter komponenta još u `app-module.ts` | `app-module.ts` | `DostavljacEditComponent` iz `catalog/dostavljaci/` — prazna, nije u rutama |
| 12 | Nekorišteni importi u Add | `dostavljaci-add.component.ts` | `ProductsApiService`, `CreateProductCommand`, `largePaging`... |
| 13 | "Intern" umjesto "Interni" | FE + BE enum | `label: 'Intern'` → `label: 'Interni'` |
| 14 | Checkbox umjesto toggle | Add/Edit HTML | `<mat-checkbox>` → `<mat-slide-toggle>` |
| 15 | "Odustani" umjesto "OTKAŽI" | Add/Edit HTML | Tekst na dugmetu |
| 16 | Naslov "Novi dostavljač" | Add HTML | Ispit: "Dodaj dostavljača" |
| 17 | Prazan SCSS | `dostavljaci-add.component.scss` | Kopiraj iz `products-add.component.scss` |
| 18 | Hardkodiran error kod | `DeleteDostavljacCommandHandler.cs` | `throw new MarketBusinessRuleException("123", ...)` |
| 19 | Nekonzistentno pisanje | više fajlova | Greska/Greška, uspjesno/uspješno |

**Nekorišteni importi u Add (obriši):**

```typescript
import {CreateProductCommand, GetProductByIdQueryDto} from '../../../../../api-services/products/products-api.models';
import {ProductsApiService} from '../../../../../api-services/products/products-api.service';
import {ListDostavljacQueryDto} from '../../../../../api-services/dostavljaci/dostavljac-api.model';
import {largePaging} from '../../../../../core/models/paging/paging-utils';
```

---

## Šta je dobro urađeno (sažetak)

- **CQRS struktura** — odvojeni Commands/Queries, validatori, controller sa MediatR
- **Entitet i enum** — `DostavljacEntity`, `DostavljacTip` (Ekstern, Intern, Freelancer)
- **Baza** — migracija, unique index na `Kod`, EF konfiguracija
- **API servis** — kompletan CRUD na frontendu
- **Lista** — tabela sa svim kolonama, paginacija, pretraga na Enter
- **Dodavanje** — reactive forma, disabled Save, default aktivan, povratak + toast
- **Brisanje** — `dialogHelper`, refresh liste, success/error toast
- **Sidebar link** — "Dostavljači" u navigaciji
- **Create handler** — provjera jedinstvenog koda prije inserta
- **Base klase** — `BaseListPagedComponent` i `BaseFormComponent`

---

## Moguća unapređenja (nije obavezno za ispit, ali plus)

1. **Uskladiti rute s Products modulom** — `products/:id/edit` pattern
2. **Dodati `dostavljac` sekciju u `dialogHelper`** — kao `product` i `productCategory`
3. **Kopirati `.tip-badge` stilove** iz `fakture.component.scss`
4. **Koristiti `mat-slide-toggle`** umjesto checkboxa za "Aktivan"
5. **Ukloniti starter komponentu** iz `app-module.ts`
6. **Očistiti nekorištene importe** u Add komponenti
7. **Dodati seed podatke** za dostavljače
8. **Testirati auth flow** — Delete zahtijeva autentifikaciju u handleru

---

## Preporučeni redoslijed popravki prije ispita

```
1. Implementirati UpdateDostavljacCommandHandler          ← BE, kritično
2. Popraviti rutu: dostavljaci/:id/edit + navigaciju      ← FE, kritično
3. Ispraviti case-insensitive pretragu (s.ToLower())      ← BE
4. getTipLabel() u HTML + tip-badge CSS                   ← FE
5. toaster.error umjesto success na load error            ← FE
6. maxlength bug + stopLoading na create error            ← FE
7. Alfanumerička validacija za Kod (FE + BE)              ← oba
8. Očistiti starter komponentu i importe                  ← FE, čistoća
```

---

## Brzi test scenariji (ručno provjeri prije ispita)

- [ ] Otvori `/admin/dostavljaci` — lista se učitava
- [ ] Pretraga: unesi "express" + Enter — pronađe "Express Dostava" bez obzira na velika slova
- [ ] Dodaj novog dostavljača — toast uspjeh, pojavi se na listi
- [ ] Pokušaj dodati isti Kod — greška (toast)
- [ ] Uredi postojećeg — forma se popuni, save radi, lista se ažurira
- [ ] Obriši — modal se pojavi, OTKAŽI zatvara, OBRIŠI briše + toast
- [ ] Paginacija — promijeni stranicu / page size
- [ ] Save dugme disabled dok forma nije validna

---

*Generisano automatskom analizom koda — bez izmjena u source fajlovima.*
