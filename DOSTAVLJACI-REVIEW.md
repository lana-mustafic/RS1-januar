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

## Kritične greške (prioritet: visok)

Ove stvari treba popraviti prije ispita — vjerovatno direktno utiču na bodovanje.

### 1. `UpdateDostavljacCommandHandler` je prazan

**Lokacija:** `Market.Application/Modules/Catalog/Dostavljaci/Commands/Update/UpdateDostavljacCommandHandler.cs`

Handler je prazna `internal` klasa koja **ne implementira** `IRequestHandler<UpdateDostavljacCommand, Unit>`. Validator i controller postoje, ali nema logike za ažuriranje entiteta u bazi.

**Posljedica:** PUT `/Dostavljaci/{id}` neće raditi (runtime greška — nema registriranog handlera).

**Šta treba:** Implementirati handler po uzoru na `UpdateProductCommandHandler` — učitaj entitet, provjeri postojanje, provjeri jedinstvenost koda (osim trenutnog ID-a), ažuriraj polja, `SaveChangesAsync`.

---

### 2. Pogrešna ruta za uređivanje (frontend)

**Lokacija:** `admin-routing-module.ts` + `dostavljaci.component.ts`

| Šta je sada | Šta bi trebalo (kao kod Products) |
|---|---|
| Ruta: `dostavljaci/edit` (bez `:id`) | `dostavljaci/:id/edit` ili `dostavljaci/edit/:id` |
| Navigacija: `['edit', item.id]` → `/admin/dostavljaci/edit/5` | `['/admin/dostavljaci', item.id, 'edit']` |

Edit komponenta čita `this.route.snapshot.params['id']`, ali ruta **nema parametar `id`**.

**Posljedica:** Klik na "Uredi" ne otvara ispravnu stranicu / ID je `NaN` → redirect ili greška.

---

### 3. Backend pretraga nije case-insensitive

**Lokacija:** `ListDostavljacQueryHandler.cs`, linija ~14

```csharp
q = q.Where(x => x.Naziv.ToLower().Contains(s));
```

`Naziv` se pretvara u lowercase, ali **`s` (search string) nije**. Ispit eksplicitno traži pretragu **bez obzira na velika/mala slova**.

**Ispravak:** `s.ToLower()` ili `EF.Functions.Like` / `Contains` s `StringComparison`.

---

## Srednje greške (prioritet: srednji)

### 4. U koloni TIP prikazuje se CSS klasa umjesto labela

**Lokacija:** `dostavljaci.component.html`

```html
{{ getTipClass(item.tip) }}
```

Prikazuje se `ekstern`, `intern`, `freelancer` umjesto `Ekstern`, `Intern`, `Freelancer`. Metoda `getTipLabel()` postoji u TS-u, ali se **ne koristi u HTML-u**.

---

### 5. Nedostaju stilovi za `tip-badge`

**Lokacija:** `dostavljaci.component.scss` (prazan za badge)

U `fakture.component.scss` postoji `.tip-badge` sa bojama, ali u dostavljačima **nema** odgovarajućih stilova za `.ekstern`, `.intern`, `.freelancer`. Badge-ovi neće izgledati kao na šablonu ispita.

---

### 6. Greška pri učitavanju liste prikazuje `success` toast

**Lokacija:** `dostavljaci.component.ts`, error callback u `loadPagedData()`

```typescript
this.toaster.success(err?.message ?? 'Greska pri ucitavanju dostavljaca');
```

Za grešku treba `toaster.error(...)`, ne `success`.

---

### 7. Bug u poruci validacije za Kod (Add komponenta)

**Lokacija:** `dostavljaci-add.component.ts` → `getErrorMessage()`

```typescript
if (control.errors['maxLength']) return 'Broj karaktera neispravan.';
```

Angular validator vraća ključ **`maxlength`** (malo slovo), ne `maxLength`. U Edit komponenti je ispravno (`maxlength`), u Add nije — poruka se neće prikazati.

---

### 8. `stopLoading()` se ne poziva nakon greške pri kreiranju

**Lokacija:** `dostavljaci-add.component.ts`, error callback u `save()`

Na uspjeh se poziva `stopLoading()`, na grešku ne — forma može ostati u loading stanju sa disabled dugmetom.

---

### 9. Nedostaje validacija da je Kod alfanumerički

Ispit traži: *"maksimalno 3 alfanumerička karaktera"*.

Trenutno:
- **Frontend:** samo `Validators.required` + `Validators.maxLength(3)`
- **Backend:** samo `NotEmpty` + `MaximumLength(3)`

Nema provjere da su samo slova/brojevi (npr. regex `^[a-zA-Z0-9]{1,3}$`).

---

### 10. Update nema provjeru jedinstvenosti Koda

Create handler provjerava duplikat koda, ali Update handler (kada se implementira) mora imati istu logiku — dozvoliti isti kod za isti ID, odbiti ako drugi zapis već koristi taj kod.

---

## Manje greške / kozmetika (prioritet: nizak)

| # | Problem | Lokacija |
|---|---|---|
| 11 | Starter komponenta `catalog/dostavljaci/dostavljac-edit` još u `app-module.ts` — prazna, nije u rutama | `app-module.ts` |
| 12 | Nekorišteni importi u Add komponenti (`ProductsApiService`, `CreateProductCommand`, `largePaging`...) | `dostavljaci-add.component.ts` |
| 13 | Enum label "Intern" umjesto "Interni" (ispit koristi "Interni") | FE + BE enum |
| 14 | `mat-checkbox` umjesto toggle switcha (ispit pokazuje switch) | Add/Edit HTML |
| 15 | Dugme "Odustani" umjesto "OTKAŽI" | Add/Edit HTML |
| 16 | Naslov "Novi dostavljač" umjesto "Dodaj dostavljača" | Add HTML |
| 17 | `dostavljaci-add.component.scss` je prazan fajl | — |
| 18 | Hardkodiran error kod `"123"` u Delete handleru | `DeleteDostavljacCommandHandler.cs` |
| 19 | Nekonzistentno pisanje (Greska/Greška, uspjesno/uspješno) | više fajlova |

---

## Šta je dobro urađeno

- **CQRS struktura** — odvojeni Commands/Queries, validatori, controller sa MediatR
- **Entitet i enum** — `DostavljacEntity`, `DostavljacTip` (Ekstern, Intern, Freelancer)
- **Baza** — migracija, unique index na `Kod`, EF konfiguracija
- **API servis** — kompletan CRUD na frontendu
- **Lista** — tabela sa svim kolonama, paginacija, pretraga na Enter
- **Dodavanje** — reactive forma, disabled Save, default aktivan, povratak + toast
- **Brisanje** — `dialogHelper`, refresh liste, success/error toast
- **Sidebar link** — "Dostavljači" u navigaciji
- **Create handler** — provjera jedinstvenog koda prije inserta
- **Base klase** — konzistentno korištenje `BaseListPagedComponent` i `BaseFormComponent`

---

## Moguća unapređenja (nije obavezno za ispit, ali plus)

1. **Uskladiti rute s Products modulom** — `products/:id/edit` pattern kroz cijeli projekat
2. **Dodati `dostavljac` sekciju u `dialogHelper`** — kao `product` i `productCategory` (konzistentnost)
3. **Kopirati `.tip-badge` stilove** iz `fakture.component.scss` i prilagoditi boje za Ekstern/Intern/Freelancer
4. **Koristiti `mat-slide-toggle`** umjesto checkboxa za "Aktivan" (bliže šablonu)
5. **Ukloniti starter komponentu** iz `app-module.ts` i `catalog/dostavljaci/` foldera
6. **Očistiti nekorištene importe** u Add komponenti
7. **Dodati seed podatke** za dostavljače (olakšava demonstraciju na ispitu)
8. **Testirati auth flow** — Delete zahtijeva autentifikaciju u handleru; provjeriti da login radi prije demo brisanja

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
