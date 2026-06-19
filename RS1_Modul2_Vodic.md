# RS1 — Vodič za Drugi Modul (26.01.2026.)

> **Cilj ovog dokumenta:** Da razumiješ *logiku* rješavanja drugog modula, a ne da kopiraš gotov kod.  
> **Pravilo:** Prvo čitaj, razumij, zatim sama piši. Koristi postojeće primjere u projektu kao „udžbenik", ne kao rješenje.  
> **Pretpostavka:** Pročitala si ili bar znaš da postoji vodič za prvi modul (`RS1_Modul1_Vodic.md`). Drugi modul gradi se na istoj arhitekturi, ali je **teži** jer uključuje veze između entiteta i poslovnu logiku.

---

## Sadržaj

1. [Uvod — šta je drugi modul?](#1-uvod--šta-je-drugi-modul)
2. [Razlika između modula 1 i 2](#2-razlika-između-modula-1-i-2)
3. [Analiza template projekta](#3-analiza-template-projekta)
4. [Kako pravilno pročitati zadatak](#4-kako-pravilno-pročitati-zadatak)
5. [Zadatak 2 — Upravljanje fakturama (pregled)](#5-zadatak-2--upravljanje-fakturama-pregled)
6. [Dio A — Lista faktura (Read + paginacija)](#6-dio-a--lista-faktura-read--paginacija)
7. [Dio B — Backend: entiteti, veze, Create](#7-dio-b--backend-entiteti-veze-create)
8. [Dio C — Poslovna logika (ulazna / izlazna)](#8-dio-c--poslovna-logika-ulazna--izlazna)
9. [Dio D — Frontend: forma sa stavkama (FormArray)](#9-dio-d--frontend-forma-sa-stavkama-formarray)
10. [Rječnik pojmova](#10-rječnik-pojmova)
11. [Vizuelni tok izvršavanja](#11-vizuelni-tok-izvršavanja)
12. [Najčešće greške studenata](#12-najčešće-greške-studenata)
13. [Kako razmišljati na ispitu](#13-kako-razmišljati-na-ispitu)
14. [Checklista prije predaje](#14-checklista-prije-predaje)

---

## 1. Uvod — šta je drugi modul?

Na ispitu iz **Razvoja softvera I** od **26.01.2026.** drugi modul pokriva **napredne funkcionalnosti — upravljanje fakturama** u aplikaciji „Market".

Za razliku od prvog modula (Dostavljač = klasičan CRUD), drugi modul je **master-detail** zadatak:

- **Master (roditelj):** Faktura — jedan dokument
- **Detail (djeca):** Stavke fakture — više redova unutar jedne fakture

Fakture utiču na **stanje zaliha proizvoda** u bazi:
- **Ulazna faktura** → roba ulazi → zaliha **raste**
- **Izlazna faktura** → roba izlazi → zaliha **opada**

U template projektu drugi modul je označen u fajlu:  
`rs1-frontend-2025-26/src/app/modules/admin/catalogs/fakture/fakture.component.html`  
(poruka: *„Ovdje raditi ispitni zadatak — drugi modul"*)

### Šta drugi modul NIJE

- **Nema** edit i delete faktura u tabeli (zadatak to eksplicitno kaže)
- **Nema** klasičan CRUD za svaku stavku posebno — stavke se kreiraju **zajedno** sa fakturom
- **Nije** dovoljno samo prikazati podatke — moraš implementirati **poslovnu logiku** nad proizvodima

---

## 2. Razlika između modula 1 i 2

| Aspekt | Modul 1 (Dostavljač) | Modul 2 (Fakture) |
|--------|----------------------|-------------------|
| Tip zadatka | Klasičan CRUD | Master-detail + poslovna logika |
| Entiteti | 1 entitet + enum | 2 entiteta (Faktura + Stavka) + veza 1:N |
| Operacije | Create, Read, Update, Delete | Read (lista) + Create (samo dodavanje) |
| Uticaj na druge tabele | Ne | Da — mijenja `Products` (zalihe) |
| Frontend forma | Običan FormGroup | FormGroup + **FormArray** (dinamičke stavke) |
| Validacija | Polja forme | Polja + **poslovna pravila** (npr. „nema dovoljno na stanju") |
| Transakcija | Jednostavno spremanje | Sve ili ništa — greška = ništa se ne sprema |

**Zaključak:** Modul 2 traži da razumiješ **veze između tabela** i da u jednom handleru koordinišeš više entiteta odjednom.

---

## 3. Analiza template projekta

### 3.1. Dva projekta — isto kao modul 1

```
2026-01-26/
├── rs1_backend-2025-26/     ← Visual Studio (C# Web API)
└── rs1-frontend-2025-26/    ← VS Code (Angular)
```

### 3.2. Šta je VEĆ urađeno u templateu (ne radi od nule!)

Profesor ti **nije dao prazan projekat**. Za Fakture već postoji dosta koda. Tvoj posao je da **dopuniš** ono što nedostaje.

#### Backend — već postoji:

| Fajl | Stanje |
|------|--------|
| `Market.Domain/Entities/Fakture/FakturaEntity.cs` | Entitet postoji, ali **nema kolekciju stavki** |
| `Market.Domain/Entities/Fakture/FakturaTip.cs` | Enum Ulazna/Izlazna — gotov |
| `Market.Infrastructure/.../FakturaConfiguration.cs` | EF konfiguracija za Faktura — gotova |
| `Market.Application/Modules/Fakture/Queries/List/` | List query + handler — gotov |
| `Market.API/Controllers/FaktureController.cs` | Samo `GET` (lista) — **nema POST** |
| Migracija `faktura-ispit` | Tabela `Fakture` postoji u bazi |
| `DynamicDataSeeder.cs` | Seed podaci za 4 fakture (bez stavki) |

#### Backend — moraš TI dodati:

| Šta | Zašto |
|-----|-------|
| `FakturaStavkaEntity` | Stavke fakture ne postoje kao entitet |
| Veza 1:N Faktura ↔ Stavka | Zadatak to eksplicitno traži |
| `DbSet` za stavke | Da EF zna za novu tabelu |
| EF konfiguracija stavki | Foreign key, cascade delete |
| Nova migracija | Nova tabela u bazi |
| `CreateFakturaCommand` + Handler + Validator | Backend trenutno nema Create |
| `POST` endpoint u kontroleru | Frontend ne može spremiti fakturu |

#### Frontend — već postoji:

| Fajl | Stanje |
|------|--------|
| `fakture.component.ts/html` | Lista sa tabelom — UI gotov |
| `faktura-add.component.ts/html` | Forma sa FormArray — UI gotov |
| `fakture-api.service.ts` | Samo `list()` — **nema create** |
| `fakture-api.models.ts` | DTO za listu — **nema create modele** |
| Routing | `/admin/fakture` i `/admin/fakture/add` — registrovano |

#### Frontend — moraš TI popraviti/dodati:

| Šta | Problem u templateu |
|-----|---------------------|
| Paginacija | Hardkodirano `list(1, 10)` — ne radi prava paginacija |
| Kategorije | Hardkodirane u komponenti — treba API |
| Validacija forme | Polja nemaju `Validators` |
| `onSubmit()` | Samo `console.log` — nema API poziva |
| Toast poruke | Nema feedback korisniku |
| API servis | Nema `create()` metodu |

### 3.3. Mapa fajlova — gdje je šta

| Pojam | Lokacija u projektu |
|-------|---------------------|
| **Model (entitet)** | `Market.Domain/Entities/Fakture/` |
| **DbContext** | `Market.Infrastructure/Database/DatabaseContext.cs` |
| **Interfejs baze** | `Market.Application/Abstractions/IAppDbContext.cs` |
| **EF konfiguracija** | `Market.Infrastructure/Database/Configurations/Fakture/` |
| **CQRS handleri** | `Market.Application/Modules/Fakture/` |
| **Controller** | `Market.API/Controllers/FaktureController.cs` |
| **Forma (UI)** | `catalogs/fakture/faktura-add/` |
| **Lista (UI)** | `catalogs/fakture/fakture.component.ts` |
| **API servis** | `api-services/fakture/` |
| **Event handleri** | Metode u `.component.ts` vezane sa `(click)`, `(keydown)`, `(ngSubmit)` u HTML-u |
| **Proizvod (zalihe)** | `Market.Domain/Entities/Catalog/ProductEntity.cs` |

### 3.4. Referentni primjeri — KOPIRAJ OBRASAC

Za drugi modul najvažniji uzorak u projektu je **Order + OrderItem** (narudžba sa stavkama):

| Šta tražiš | Gdje gledaš |
|-----------|------------|
| Master-detail entiteti | `OrderEntity.cs` + `OrderItemEntity.cs` |
| Veza 1:N u EF | `OrderItemConfiguration.cs` — `HasOne().WithMany().HasForeignKey()` |
| Create sa stavkama | `CreateOrderCommand.cs` + `CreateOrderCommandHandler.cs` |
| Validacija liste stavki | `CreateOrderCommandValidator.cs` — `RuleForEach` |
| FormArray na frontendu | `faktura-add.component.ts` (već započeto) |

**Pravilo:** Faktura + Stavka = isti pattern kao Order + OrderItem, samo sa drugačijom poslovnom logikom.

---

## 4. Kako pravilno pročitati zadatak

### 4.1. Pročitaj zadatak u tri prolaza

**Prolaz 1 — Šta korisnik vidi (UI):**
- Lista faktura sa kolonama: broj računa, tip, datum, broj stavki
- Dugme „Nova faktura"
- Forma sa poljima fakture + dinamičke stavke
- Paginacija, toast poruke

**Prolaz 2 — Šta se dešava u bazi:**
- Nova tabela `FakturaStavke` (ili slično)
- Veza sa `Fakture` i `ProductCategories`
- Izmjene u tabeli `Products` (StockQuantity, IsEnabled)

**Prolaz 3 — Poslovna pravila (najvažnije!):**
- Ulazna vs izlazna — potpuno različita logika
- Greška = ništa se ne smije spremiti
- Faktura mora imati bar jednu stavku

### 4.2. Ključne riječi u tekstu zadatka

| Riječ / fraza | Šta znači za tebe |
|---------------|-------------------|
| „master-detail forma" | Roditeljski FormGroup + FormArray za djecu |
| „1:N relacija" | Jedna faktura — više stavki; FK `FakturaId` na stavci |
| „FakturaStavkaEntity" | Novi entitet koji moraš kreirati |
| „BrojStavki" | Brojač stavki — postavi pri kreiranju (npr. `items.Count`) |
| „ulazna faktura" | Logika povećanja zaliha / kreiranja proizvoda |
| „izlazna faktura" | Logika smanjenja zaliha + stroga validacija |
| „case-insensitive" | `ToLower()` pri poređenju imena proizvoda |
| „transakcija" / „greška = ne kreirati" | Sve u jednom `SaveChangesAsync` ili explicit transaction |
| „hardkodirano zamijeniti API-jem" | Kategorije učitati iz `ProductCategories` endpointa |
| „hardkodirano paging" | Koristiti `BaseListPagedComponent` ili paginator |
| „toast poruka" | `ToasterService.success()` / `.error()` |
| „bez edit/delete kolona" | Ne dodaji akcije u tabelu — samo prikaz |

### 4.3. Redoslijed rada (preporučen)

1. Razumij postojeći kod (otvori sve Fakture fajlove)
2. Backend: FakturaStavkaEntity + veza + migracija
3. Backend: Create command + handler sa poslovnom logikom
4. Backend: test u Swaggeru
5. Frontend: popravi paginaciju na listi
6. Frontend: učitaj kategorije iz API-ja
7. Frontend: validacija forme + create API poziv
8. Frontend: toast + navigacija nazad na listu
9. End-to-end test: ulazna i izlazna faktura

### 4.4. Na šta posebno obratiti pažnju

- **Izlazna faktura je stroža** — tu studenti najviše gube bodove
- **Ime proizvoda na stavci** je tekst koje korisnik unosi — ne bira postojeći proizvod iz dropdowna (u zadatku piše „Ime proizvoda" kao input)
- **Matching proizvoda** ide po imenu (case-insensitive) **i** kategoriji
- **Nakon greške** korisnik ipak ide na listu — ali sa toast error porukom
- **Paginacija** — parametri moraju odgovarati backendu (`Paging.Page`, ne `PageNumber`)

---

## 5. Zadatak 2 — Upravljanje fakturama (pregled)

### 5.1. Analiza zadatka — šta profesor traži?

**Cilj:** Implementirati evidenciju faktura koje utiču na stanje zaliha proizvoda.

**Dva tipa fakture (enum `FakturaTip`):**

| Tip | Značenje | Efekat na zalihe |
|-----|----------|------------------|
| **Ulazna** | Ulaz robe | Zaliha **raste** (ili se kreira novi proizvod) |
| **Izlazna** | Izlos robe | Zaliha **opada** |

**Funkcionalnosti:**

1. **Lista faktura** — tabela, paginacija, bez edit/delete
2. **Dodavanje fakture** — master-detail forma
3. **Poslovna logika** — ulazna/izlazna (detalji u sekciji 8)
4. **Feedback** — toast + povratak na listu

### 5.2. Polja forme

**Faktura (master):**

| Polje | Obavezno | Napomena |
|-------|----------|----------|
| Broj računa | Da | npr. FAK-2026-0001 |
| Tip | Da | enum: Ulazna / Izlazna |
| Napomena | Ne | opciono |

**Stavka (detail) — može ih biti više:**

| Polje | Obavezno | Napomena |
|-------|----------|----------|
| Kategorija proizvoda | Da | iz baze (API) |
| Ime proizvoda | Da | tekstualni unos |
| Količina | Da | broj, minimum 1 |

**Pravilo:** Faktura **mora** imati bar jednu stavku. Stavka **ne može** postojati bez fakture.

### 5.3. Na šta studenti najčešće pogriješe

- Prave stavke kao JSON u polju fakture umjesto posebne tabele
- Zaborave poslovnu logiku — samo spreme fakturu u bazu
- Na izlaznoj fakturi ne provjere da li proizvod postoji
- Na izlaznoj fakturi ne provjere da li ima dovoljno zaliha
- Promijene proizvod iako je faktura odbijena (nema transakcije)
- Hardkodirane kategorije ostave
- Paginacija ne radi jer su pogrešni query parametri
- FormArray ostane prazan ili bez validacije

---

## 6. Dio A — Lista faktura (Read + paginacija)

### 6.1. Analiza

Lista **već radi** djelimično. Backend `ListFaktureQueryHandler` vraća paginirane podatke. Frontend učitava podatke, ali:
- Paging je **hardkodiran** na stranicu 1, veličina 10
- Footer prikazuje statički „Stranica 1 od 1"
- Nema `BaseListPagedComponent`
- API servis koristi pogrešan naziv parametra (`PageNumber` umjesto `Page`)

### 6.2. Koraci rješavanja

#### Korak 1: Prouči backend list handler

- **Otvori:** `ListFaktureQueryHandler.cs`
- **Pogledaj:** koristi `PageResult.FromQueryableAsync` — ista paginacija kao modul 1
- **Razumij:** Sortira po `CreatedAtUtc` opadajuće — ne mijenjaj osim ako zadatak traži drugačije

#### Korak 2: Prouči frontend listu

- **Otvori:** `fakture.component.ts` i `fakture.component.html`
- **Pogledaj:** `loadFakture()` poziva `list(1, 10)` — to je ono što treba zamijeniti
- **Pogledaj HTML:** tabela koristi `mat-table` — to je tvoj „DataGridView"

#### Korak 3: Uporedi sa radnom paginacijom

- **Otvori:** `products.component.ts` ili `product-categories-2.component.ts`
- **Pogledaj:** nasljeđuju `BaseListPagedComponent`, koriste `buildHttpParams`, imaju `<app-fit-paginator-bar>`
- **Razmisli:** Isti pattern primijeni na Fakture

#### Korak 4: Popravi API servis

- **Otvori:** `fakture-api.service.ts`
- **Problem:** parametri `Paging.PageNumber` — backend očekuje `Paging.Page` (vidi `PageRequest.cs`)
- **Rješenje:** koristi `buildHttpParams` sa objektom `{ paging: { page: 1, pageSize: 10 } }` kao u drugim servisima
- **Zašto:** Pogrešan naziv parametra = backend uvijek vraća stranicu 1

#### Korak 5: Refaktoriši FaktureComponent

- **Naslijedi** `BaseListPagedComponent` (opciono ali preporučeno)
- **Implementiraj** `loadPagedData()` umjesto `loadFakture()`
- **Dodaj** `<app-fit-paginator-bar [vm]="this" />` u HTML umjesto statičkog footera
- **Zašto:** Jedan izvor istine za paginaciju kroz cijeli projekat

#### Korak 6: Provjeri prikaz kolona

Kolone koje zadatak traži (već postoje u HTML-u):
- Broj računa
- Tip (badge — metode `getTipString`, `getTipClass` već postoje)
- Datum kreiranja (`datumKreiranja | date`)
- Broj stavki

**Ne dodaji** kolonu Akcije (edit/delete).

---

## 7. Dio B — Backend: entiteti, veze, Create

### 7.1. Analiza

Ovo je srž backend dijela. Moraš:
1. Kreirati `FakturaStavkaEntity`
2. Povezati sa `FakturaEntity` (1:N)
3. Dodati u DbContext
4. Napraviti migraciju
5. Implementirati Create CQRS

### 7.2. Koraci rješavanja

#### Korak 1: Prouči uzorak Order + OrderItem

- **Otvori:** `OrderEntity.cs` — vidi `Items` kolekciju
- **Otvori:** `OrderItemEntity.cs` — vidi `OrderId` (FK) i `Order` (navigation property)
- **Otvori:** `OrderItemConfiguration.cs` — vidi `HasOne().WithMany().HasForeignKey()` i `OnDelete(DeleteBehavior.Cascade)`
- **Razumij:** Cascade = kad obrišeš fakturu, stavke se automatski brišu

#### Korak 2: Kreiraj FakturaStavkaEntity

- **Gdje:** `Market.Domain/Entities/Fakture/FakturaStavkaEntity.cs`
- **Svojstva koja trebaš razmisliti:**
  - `FakturaId` — foreign key ka fakturi
  - `Faktura` — navigation property (opciono ali korisno)
  - `KategorijaId` — FK ka kategoriji proizvoda (ili samo ID bez navigacije)
  - `ImeProizvoda` — string koji korisnik unosi
  - `Kolicina` — int, obavezno
- **Naslijedi** `BaseEntity` (dobija Id, CreatedAtUtc, itd.)
- **Dodaj** `Constraints` klasu sa max dužinama

#### Korak 3: Ažuriraj FakturaEntity

- **Otvori:** `FakturaEntity.cs`
- **Dodaj:** kolekciju stavki — po uzoru na `OrderEntity.Items`
- **Razmisli za `BrojStavki`:** Zadatak kaže „dinamički ili ažurirati pri dodavanju". Najjednostavnije: u handleru postavi `BrojStavki = request.Items.Count` pri kreiranju

#### Korak 4: EF konfiguracija stavki

- **Gdje:** `Market.Infrastructure/Database/Configurations/Fakture/FakturaStavkaConfiguration.cs`
- **Postavi:**
  - Ime tabele
  - Max dužina za ImeProizvoda
  - Veza ka Faktura (Cascade delete)
  - Veza ka ProductCategory (Restrict delete — ne briši kategoriju ako ima stavki)
- **Uzorak:** `OrderItemConfiguration.cs`

#### Korak 5: DbContext + IAppDbContext

- **Dodaj** `DbSet<FakturaStavkaEntity>` u oba fajla
- **Provjeri** build

#### Korak 6: Migracija

- Package Manager Console, default project: `Market.Infrastructure`
- `Add-Migration faktura-stavke` (ili slično ime)
- `Update-Database`
- **Provjeri u SSMS:** tabele `Fakture` i nova tabela stavki

#### Korak 7: CreateFakturaCommand

- **Gdje:** `Market.Application/Modules/Fakture/Commands/Create/`
- **Struktura po uzoru na** `CreateOrderCommand.cs`:

```
CreateFakturaCommand
├── BrojRacuna (string)
├── Tip (FakturaTip enum)
├── Napomena (string?, opciono)
└── Items (lista CreateFakturaCommandItem)
    ├── KategorijaId (int)
    ├── ImeProizvoda (string)
    └── Kolicina (int)
```

- **Zašto odvojeni Item tip:** Validator može validirati svaku stavku posebno (`RuleForEach`)

#### Korak 8: CreateFakturaCommandValidator

- **Uzorak:** `CreateOrderCommandValidator.cs`
- **Pravila:**
  - BrojRacuna — obavezan, max dužina iz Constraints
  - Tip — obavezan (enum)
  - Items — **NotEmpty** (bar jedna stavka!)
  - RuleForEach za stavke: KategorijaId > 0, ImeProizvoda not empty, Kolicina >= 1

#### Korak 9: CreateFakturaCommandHandler — struktura

Handler je **najduži dio**. Razbij ga na regione (kao CreateOrderCommandHandler):

```
1. Validacija na početku (poslovna — vidi sekciju 8)
2. Kreiraj FakturaEntity
3. Za svaku stavku — kreiraj FakturaStavkaEntity
4. Primijeni poslovnu logiku na Products (ulazna/izlazna)
5. Postavi BrojStavki
6. SaveChangesAsync — JEDNOM na kraju
7. Vrati Id fakture
```

**Važno:** Ne zovi `SaveChangesAsync` više puta unutar petlje — sve pripremi, pa jednom spremi.

#### Korak 10: Controller — POST endpoint

- **Otvori:** `FaktureController.cs`
- **Dodaj:** `[HttpPost]` metodu po uzoru na `ProductCategoriesController.Create`
- **Testiraj u Swaggeru** prije nego diraš frontend

---

## 8. Dio C — Poslovna logika (ulazna / izlazna)

Ovo je **najteži dio modula 2**. Pročitaj pažljivo i razumij prije pisanja koda.

### 8.1. Zajedničko za oba tipa

Prije bilo kakve logike, za svaku stavku moraš:
1. Provjeriti da kategorija postoji u bazi
2. Pripremiti podatke za matching proizvoda

**Matching proizvoda** = pronađi proizvod gdje:
- Ime se poklapa (**case-insensitive**)
- **I** CategoryId odgovara kategoriji sa stavke

**Kako case-insensitive:**  
U C#: uporedi `.ToLower()` na oba stringa, ili koristi `EF.Functions.Collate` / `string.Equals(..., StringComparison.OrdinalIgnoreCase)` — bitno je da radi i u LINQ upitu prema bazi.

---

### 8.2. Ulazna faktura — logika

```
Za svaku stavku:
│
├─ Proizvod POSTOJI (ime + kategorija)?
│   └─ DA → StockQuantity += stavka.Kolicina
│
└─ Proizvod NE POSTOJI?
    └─ Kreiraj novi ProductEntity:
        - Name = stavka.ImeProizvoda
        - CategoryId = stavka.KategorijaId
        - StockQuantity = stavka.Kolicina
        - Description = "kreirano putem ulazne fakture"
        - IsEnabled = false
        - Price = ? (zadatak ne spominje cijenu — razmisli šta staviti, npr. 0)
```

**Zašto IsEnabled = false:** Novi proizvod iz ulazne fakture nije odmah spreman za prodaju — profesor to eksplicitno traži.

**Napomena:** Ulazna faktura je „blaga" — ne odbija se lako. Uvijek može proći ako su osnovna polja validna.

---

### 8.3. Izlazna faktura — logika

```
Prije spremanja — provjeri SVE stavke:
│
Za svaku stavku:
├─ Proizvod POSTOJI?
│   └─ NE → ODBIJ (baci exception) — faktura se NE kreira
│
└─ DA → StockQuantity >= stavka.Kolicina?
        └─ NE → ODBIJ — nema dovoljno na stanju
        └─ DA → OK, može se nastaviti

Ako SVE stavke prođu provjeru:
Za svaku stavku:
├─ StockQuantity -= stavka.Kolicina
│
└─ Ako je StockQuantity == 0 nakon oduzimanja:
    ├─ StockQuantity = 0
    └─ IsEnabled = false
```

**Redoslijed je ključan:**
1. **Prvo** provjeri SVE stavke (read-only)
2. **Tek ako sve prođe** — primijeni promjene
3. Ako ijedna stavka padne — **ništa** se ne smije promijeniti

---

### 8.4. Transakcija — „sve ili nišno"

Zadatak kaže: *„U slučaju greške faktura se ne kreira i ne vrši se nikakva promjena proizvoda."*

**Kako to postići:**

**Opcija A (jednostavnija, dovoljna na ispitu):**
- Sve provjere obavi **prije** bilo kakvog `Add()` ili izmjene
- Ako provjera padne → baci exception (`MarketBusinessRuleException` ili `ValidationException`)
- Handler se prekine — ništa nije spremljeno jer `SaveChangesAsync` nije pozvan

**Opcija B (naprednija):**
- Explicit database transaction: `BeginTransactionAsync` → commit/rollback

Za ispit je **Opcija A** dovoljna ako redoslijed u handleru bude ispravan.

**Uzorak bacanja greške u projektu:**  
Pogledaj kako `CreateOrderCommandHandler` baca `ValidationException` kad proizvod ne postoji.

---

### 8.5. Vizuelni tok — odluka ulazna vs izlazna

```
CreateFakturaCommand stiže u Handler
        ↓
Validator provjerava polja (FluentValidation)
        ↓
Handler kreira FakturaEntity objekt (još nije u bazi)
        ↓
Handler prolazi kroz Items
        ↓
    ┌─────────────────────────────────┐
    │  Tip == Ulazna?                 │
    └──────────┬──────────────────────┘
               │
      ┌────────┴────────┐
      │ DA              │ NE (Izlazna)
      ↓                 ↓
 Za svaku stavku:    Faza 1: Provjeri SVE stavke
 traži proizvod      (postoji? dovoljno zaliha?)
      │                 │
 ┌────┴────┐       ┌────┴────┐
 │Postoji? │       │Sve OK?  │
 └────┬────┘       └────┬────┘
  DA  │ NE          NE  │ DA
  ↓   ↓               ↓  ↓
 +qty  kreiraj      throw  Faza 2:
       novi         exception  -qty
       proizvod              (možda IsEnabled=false)
      │                 │
      └────────┬────────┘
               ↓
 Kreiraj FakturaStavkaEntity za svaku stavku
               ↓
 Postavi BrojStavki
               ↓
 SaveChangesAsync() — JEDNOM
               ↓
 Vrati faktura.Id
```

---

## 9. Dio D — Frontend: forma sa stavkama (FormArray)

### 9.1. Analiza

Forma **već postoji** u `faktura-add.component.ts`. Koristi:
- `FormGroup` za fakturu
- `FormArray` za stavke (`items`)
- Metode `addItem()` i `removeItem()`

Tvoj posao: dopuniti validaciju, API pozive, kategorije, toast.

### 9.2. Šta je FormArray?

Običan `FormGroup` = jedan objekat sa fiksnim poljima.  
`FormArray` = **lista** formi — svaki element je zaseban `FormGroup`.

```
form (FormGroup)
├── brojRacuna
├── tip
├── napomena
└── items (FormArray)
    ├── [0] FormGroup { kategorijaId, proizvod, kolicina }
    ├── [1] FormGroup { kategorijaId, proizvod, kolicina }
    └── ...
```

**WinForms analogija:** BindingSource sa listom redova — FormArray je Angular način da vežeš dinamičku tabelu stavki.

### 9.3. Koraci rješavanja

#### Korak 1: Prouči postojeću formu

- **Otvori:** `faktura-add.component.ts` i `.html`
- **Pogledaj:** `formArrayName="items"`, `*ngFor="let itemGroup of items.controls"`
- **Razumij:** HTML već podržava dodavanje/brisanje stavki — ne moraš dizajnirati od nule

#### Korak 2: Dodaj validaciju u FormGroup

Trenutno polja nemaju `Validators`. Trebaš:

| Polje | Validator |
|-------|-----------|
| brojRacuna | `Validators.required` |
| tip | `Validators.required` |
| napomena | nema (opciono) |
| kategorijaId | `Validators.required` |
| proizvod | `Validators.required` |
| kolicina | `Validators.required`, `Validators.min(1)` |

**Dugme Sačuvaj** u HTML-u već ima `[disabled]="form.invalid || isSaving"` — super, samo treba validacija da `invalid` radi ispravno.

#### Korak 3: Zamijeni hardkodirane kategorije

- **Otvori:** `products-add.component.ts` → metoda `loadCategories()`
- **Uzorak:** poziva `ProductCategoriesApiService.list()`
- **U FakturaAddComponent:**
  1. Inject `ProductCategoriesApiService`
  2. U `ngOnInit` učitaj kategorije
  3. Obriši hardkodirani niz `kategorije = [{ id: 1, name: 'Laptopi' }, ...]`
- **Zašto:** Zadatak eksplicitno kaže da se kategorije učitavaju iz baze

#### Korak 4: Dodaj create u API servis

- **Otvori:** `fakture-api.service.ts`
- **Dodaj modele** u `fakture-api.models.ts`:
  - `CreateFakturaCommand`
  - `CreateFakturaCommandItem`
- **Dodaj metodu** `create(payload)` → HTTP POST na `/Fakture`
- **Uzorak:** `product-categories-api.service.ts` → metoda `create`

#### Korak 5: Implementiraj onSubmit()

Trenutno:
```
onSubmit() {
  if (this.form.valid) {
    console.log(...);
    this.router.navigate(...);
  }
}
```

Treba postati (logika, ne kod):
1. Provjeri `form.invalid` → return
2. Provjeri da `items.length >= 1` → inače toast error
3. `isSaving = true`
4. Mapiraj form value u `CreateFakturaCommand` objekat
5. Pozovi `faktureApiService.create(...).subscribe(...)`
6. **U next:** toast success, navigiraj na `/admin/fakture`
7. **U error:** toast error sa porukom, navigiraj na `/admin/fakture` (zadatak kaže: uvijek nazad na listu!)
8. `isSaving = false`

**Važno iz zadatka:** Bez obzira uspjeh ili greška → korisnik ide na listu + toast.

#### Korak 6: Osvježi listu nakon povratka

Kad se vratiš na listu, `ngOnInit` ponovo učitava podatke — nova faktura bi trebala biti vidljiva (sortirana po datumu).

Alternativa: koristi servis/subject za refresh — nije obavezno na ispitu.

#### Korak 7: Minimalno jedna stavka

- Pri kreiranju forme već se dodaju 2 stavke — OK
- Ako korisnik obriše sve → onSubmit treba odbiti (validator: Items not empty)
- Razmisli: da li dozvoliti 0 stavki? **Ne** — zadatak zabranjuje

---

## 10. Rječnik pojmova

> Za svaki pojam: šta je, kada se koristi, zašto, kako prepoznati u zadatku, i jednostavan primjer **nevezan za ispit**.

---

### LINQ

**Šta je:** Jezik upita u C# za rad sa kolekcijama i bazom.

**Kada se koristi:** U handlerima — filtriranje, sortiranje, projekcija.

**Zašto:** Čitljivije od SQL stringova; EF prevodi u SQL.

**U zadatku prepoznaj:** „Filtriraj", „Pronađi proizvod", „Sortiraj po datumu".

**Primjer (općenito):**  
„Daj mi sve studente starije od 20" → `studenti.Where(s => s.Godine > 20)`

---

### Lambda izrazi

**Šta je:** Kratka anonimna funkcija: `x => x.Ime`

**Kada se koristi:** Unutar LINQ metoda.

**Primjer:** `.Where(x => x.Tip == FakturaTip.Ulazna)`

---

### Include

**Šta je:** EF naredba za učitavanje povezanih entiteta u istom upitu.

**Kada se koristi:** Kad trebaš podatke iz dvije tabele odjednom.

**U modulu 2:** Vjerovatno **ne treba** Include za Create — samo tražiš proizvod po imenu i kategoriji sa `Where`.

**Primjer (općenito):** `.Include(f => f.Stavke)` — učitaj stavke uz fakturu.

---

### Where

**Šta je:** LINQ filter — zadrži samo redove koji zadovolje uslov.

**Kada se koristi:** Traženje proizvoda, filtriranje.

**Primjer (općenito):** `context.Products.Where(p => p.CategoryId == 3)`

**U modulu 2:** Pronađi proizvod po imenu i kategoriji.

---

### Select

**Šta je:** LINQ projekcija — pretvori entitet u DTO ili anoniman objekat.

**Kada se koristi:** List handleri — ne vraćaj cijeli entitet.

**Primjer (općenito):** `.Select(p => new { p.Id, p.Name })`

---

### OrderBy / OrderByDescending

**Šta je:** Sortiranje rezultata.

**U modulu 2:** Lista faktura sortirana po datumu — `OrderByDescending(x => x.CreatedAtUtc)`.

**Primjer (općenito):** `.OrderBy(x => x.Prezime)` — A-Z

---

### Entity Framework (EF Core)

**Šta je:** ORM — mapira C# klase na tabele u bazi.

**Kada se koristi:** Cijeli backend projekat.

**Zašto:** Ne pišeš SQL ručno — pišeš LINQ, EF generiše SQL.

**U zadatku prepoznaj:** Entiteti, DbContext, migracije, DbSet.

---

### DbSet

**Šta je:** „Tabela" u kodu — `context.Fakture`, `context.Products`.

**Primjer u projektu:** `ctx.Fakture.AsNoTracking()` u List handleru.

---

### Foreign Key (FK)

**Šta je:** Kolona koja referencira Id u drugoj tabeli.

**U modulu 2:**
- `FakturaStavkaEntity.FakturaId` → FK ka `Fakture`
- `FakturaStavkaEntity.KategorijaId` → FK ka `ProductCategories`

**Kako prepoznati u zadatku:** „1:N relacija", „povezano sa", „pripada fakturi".

---

### Navigation Property

**Šta je:** Property u entitetu koji predstavlja povezani objekat.

**Primjer:** `OrderItemEntity.Order` — navigacija ka narudžbi.

**Zašto:** EF koristi za Include i konfiguraciju veza.

**U modulu 2:** `FakturaStavkaEntity.Faktura` i kolekcija `FakturaEntity.Stavke`.

---

### DTO (Data Transfer Object)

**Šta je:** Klasa samo za prenos podataka između slojeva.

**U modulu 2:**
- `ListFaktureQueryDto` — za tabelu
- `CreateFakturaCommand` — za POST body

**Zašto:** Ne izlažeš internu strukturu entiteta API-ju.

---

### Validacija

**Backend:** FluentValidation — `*Validator.cs`  
**Frontend:** Angular `Validators` u FormGroup

**U modulu 2 imaš DVI vrste:**
1. **Tehnička** — polje prazno, količina < 1
2. **Poslovna** — nema proizvoda na stanju (samo u handleru, ne u validatoru)

---

### Event Handler

**Šta je:** Metoda koja reaguje na događaj (klik, submit).

**U Angularu:**
- `(click)="onNovaFaktura()"` — klik na dugme
- `(click)="onSubmit()"` — submit forme
- `(click)="addItem()"` — dodaj stavku
- `(click)="removeItem(i)"` — obriši stavku

**WinForms analogija:** `button1_Click` — isto, druga sintaksa.

---

### ComboBox → mat-select

**WinForms:** Padajući izbor.

**Angular:** `<mat-select formControlName="tip">` sa `<mat-option>`.

**U modulu 2:** Tip fakture i Kategorija proizvoda.

---

### DataGridView → mat-table

**WinForms:** Tabela sa redovima.

**Angular:** `<table mat-table [dataSource]="fakture">`.

**U modulu 2:** Lista faktura.

---

### BindingSource → FormArray / dataSource

**WinForms:** BindingSource povezuje listu objekata sa gridom.

**Angular ekvivalent:**
- `[dataSource]="fakture"` na tabeli
- `FormArray` za dinamičke stavke u formi

---

### MessageBox → ToasterService / DialogHelper

**WinForms:** `MessageBox.Show("Uspješno!")`

**Angular u ovom projektu:**
- **Toast** (mala poruka u uglu) — `ToasterService.success()` / `.error()`
- **Modal** (popup) — `DialogHelperService` — u modulu 2 se koristi **toast**, ne modal za create

---

### Async metode

**Backend (C#):** `async Task<T> Handle(...)` + `await ctx.SaveChangesAsync()`

**Frontend (Angular):** HTTP pozivi vraćaju `Observable` — koristiš `.subscribe({ next, error })`

**Zašto async:** Ne blokira UI dok čekaš odgovor servera.

**Kako prepoznati:** Svaki API poziv, svaki handler sa `await`.

**Primjer (općenito):**  
Umjesto da cijeli ekran „smrzne" dok se čeka baza, aplikacija prikaže spinner (`isSaving = true`) i nastavi reagovati.

---

## 11. Vizuelni tok izvršavanja

### 11.1. Otvaranje liste faktura

```
Korisnik klikne „Fakture" u sidebaru
        ↓
Router navigira na /admin/fakture
        ↓
FaktureComponent.ngOnInit()
        ↓
loadPagedData() → FaktureApiService.list(request)
        ↓
HTTP GET /Fakture?Paging.Page=1&Paging.PageSize=10
        ↓
FaktureController.List() → MediatR → ListFaktureQueryHandler
        ↓
Handler: ctx.Fakture.AsNoTracking() → OrderByDescending → PageResult
        ↓
JSON odgovor: { items: [...], totalItems: 4, totalPages: 1, ... }
        ↓
handlePageResult() → this.fakture = response.items
        ↓
Angular renderuje mat-table
        ↓
Korisnik vidi tabelu + paginator
```

**U pozadini:** `AsNoTracking()` znači EF ne prati promjene — brže za read-only listu.

---

### 11.2. Otvaranje forme za novu fakturu

```
Korisnik klikne „Nova faktura"
        ↓
onNovaFaktura() → router.navigate(['/admin/fakture/add'])
        ↓
FakturaAddComponent se kreira
        ↓
constructor: FormBuilder kreira formu + 2 prazne stavke u FormArray
        ↓
ngOnInit: učitaj kategorije iz ProductCategoriesApiService
        ↓
mat-select za kategorije se puni podacima iz baze
        ↓
Korisnik vidi praznu formu spremnu za unos
```

---

### 11.3. Dodavanje / uklanjanje stavke

```
Korisnik klikne „Dodaj stavku"
        ↓
addItem() → fb.group({...}) → items.push(itemGroup)
        ↓
FormArray raste za 1 → Angular *ngFor prikaže novi red
        ↓
Korisnik klikne ikonicu kante na stavci #2
        ↓
removeItem(1) → items.removeAt(1)
        ↓
FormArray se smanji → red nestaje sa ekrana
```

---

### 11.4. Spremanje fakture (cijeli tok)

```
Korisnik popuni fakturu + stavke
        ↓
Klikne „Sačuvaj"
        ↓
onSubmit() → form.markAllAsTouched() (ako koristiš BaseFormComponent)
        ↓
form.invalid? → DA → return (dugme je već disabled)
        ↓ NE
isSaving = true (prikaže se spinner na dugmetu)
        ↓
Mapiranje: form.value → CreateFakturaCommand JSON
        ↓
HTTP POST /Fakture → Backend
        ↓
FaktureController.Create() → MediatR
        ↓
CreateFakturaCommandValidator → provjera polja
        ↓
CreateFakturaCommandHandler:
    ├─ Tip == Ulazna? → logika ulazne
    └─ Tip == Izlazna? → provjera zaliha → logika izlazne
        ↓
SaveChangesAsync() — faktura + stavke + promjene proizvoda
        ↓
┌─────────────────────────────────────────┐
│ USPJEH                                  │
├─────────────────────────────────────────┤
│ Backend vraća 201 Created + id            │
│ Frontend: toast.success(...)              │
│ router.navigate(['/admin/fakture'])       │
│ Lista se učita → nova faktura vidljiva    │
└─────────────────────────────────────────┘
        ↓
┌─────────────────────────────────────────┐
│ GREŠKA (npr. nema zaliha)               │
├─────────────────────────────────────────┤
│ Backend baca exception → 400/409          │
│ Middleware vraća poruku greške            │
│ Frontend: toast.error(poruka)             │
│ router.navigate(['/admin/fakture'])       │
│ Lista se učita → nema promjena            │
└─────────────────────────────────────────┘
        ↓
isSaving = false
```

---

## 12. Najčešće greške studenata

### 12.1. Backend greške

| Greška | Simptom | Kako izbjeći |
|--------|---------|--------------|
| Nema FakturaStavkaEntity | Stavke se ne spremaju | Kreiraj entitet + migraciju |
| Nema FK veze | Build/migracija greška | Pogledaj OrderItemConfiguration |
| SaveChanges u petlji | Polu-spremljeni podaci | Jedan SaveChanges na kraju |
| Izlazna ne provjerava zalihe | Može ići u minus | Provjeri PRIJE izmjene |
| Greška ali proizvod se promijeni | Nekonzistentni podaci | Provjere prije Add/Modify |
| Case-sensitive match imena | „Asus" ≠ „asus" | ToLower na oba stringa |
| Zaboravi BrojStavki | U listi uvijek 0 | Postavi u handleru |
| Nema POST endpoint | 404 na frontendu | Dodaj u FaktureController |

### 12.2. Frontend greške

| Greška | Simptom | Kako izbjeći |
|--------|---------|--------------|
| Pogrešan paging param | Uvijek ista stranica | `Paging.Page`, ne `PageNumber` |
| Hardkodirane kategorije | Profesor vidi u kodu | Učitaj iz API-ja |
| console.log umjesto API | Ništa se ne sprema | Implementiraj create u servisu |
| Nema toast | Korisnik ne zna ishod | ToasterService u subscribe |
| Ne vrati na listu pri grešci | Korisnik ostane na formi | Zadatak traži uvijek nazad |
| FormArray bez validatora | Prazne stavke se šalju | Validators.required |
| items.length == 0 | Prazna faktura | NotEmpty u backend validatoru + frontend provjera |

### 12.3. Kako pronaći grešku

**1. Browser F12 → Network**
- Vidi da li POST stiže do servera
- Status kod: 200/201 = OK, 400 = validacija, 500 = server greška
- Response body — poruka greške

**2. Browser F12 → Console**
- Crvene Angular greške
- `console.error` iz subscribe error bloka

**3. Visual Studio Output / Swagger**
- Testiraj POST direktno u Swaggeru sa JSON body-jem
- Izoluješ da li je problem backend ili frontend

**4. SSMS**
```sql
SELECT * FROM Fakture
SELECT * FROM FakturaStavke  -- ili kako se zove tvoja tabela
SELECT Id, Name, StockQuantity, IsEnabled, CategoryId FROM Products
```
Provjeri da li se podaci stvarno mijenjaju.

### 12.4. Kako čitati exception poruke

| Poruka | Značenje | Akcija |
|--------|----------|--------|
| `ValidationException` | FluentValidation ili poslovna provjera | Pročitaj `message`, popravi podatke |
| `MarketNotFoundException` | Entitet nije pronađen | Provjeri Id / matching logiku |
| `MarketBusinessRuleException` | Poslovno pravilo prekršeno | Npr. nema zaliha — očekivano ponašanje |
| `500 Internal Server Error` | Neočekivana greška u handleru | Pogledaj VS Output — stack trace |
| CORS error | Backend ne radi ili CORS | Pokreni API |
| `401 Unauthorized` | Nisi ulogovana | Login u aplikaciju |

### 12.5. Debug strategija na ispitu

1. **Swagger prvo** — backend mora raditi izolirano
2. **Network tab** — frontend-backend komunikacija
3. **SSMS** — da li su podaci u bazi
4. **Breakpoint** u handleru — ako stigneš do te razine

---

## 13. Kako razmišljati na ispitu

### Kada dobiješ zadatak, uradi ovo:

---

#### Korak 1: Pročitaj zahtjeve (5–10 minuta)

**Ne otvaraj odmah Visual Studio.**

1. Pročitaj cijeli tekst **dva puta**
2. Podvuci:
   - Entitete (Faktura, Stavka, Product)
   - Tipove (Ulazna/Izlazna)
   - Poslovna pravila (zalihe, matching, greške)
   - UI zahtjeve (paginacija, toast, FormArray)
3. Zapiši u bilježnicu:
   - „Backend: šta nedostaje?"
   - „Frontend: šta je hardkodirano?"
   - „Poslovna logika: ulazna vs izlazna"

**Zašto:** 10 minuta planiranja štedi 30 minuta lutanja.

---

#### Korak 2: Pronađi odgovarajuće klase (5 minuta)

1. U Solution Exploreru pretraži: `Faktura`
2. Otvori **sve** fajlove koji se pojave
3. Označi šta je gotovo, šta nije
4. Pronađi uzorak: pretraži `OrderItem` — to je tvoj kalup

**Zašto:** Template već ima pola posla — ne radi od nule.

---

#### Korak 3: Analiziraj veze između modela (5 minuta)

Nacrtaj na papiru:

```
FakturaEntity (1) ──────< FakturaStavkaEntity (N)
                                    │
                                    └──> ProductCategoryEntity

ProductEntity ←── matching po Name + CategoryId
```

**Pitanja koja si postavi:**
- Šta je FK na stavci?
- Cascade ili Restrict delete?
- Koji entitet se mijenja pri ulaznoj/izlaznoj?

**Zašto:** Modul 2 je o **vezama** — ako ih ne razumiješ, handler neće raditi.

---

#### Korak 4: Isplaniraj rješenje (5 minuta)

Napiši redoslijed na papir:

```
1. FakturaStavkaEntity
2. Konfiguracija + DbContext + migracija
3. CreateCommand + Validator
4. Handler — prvo ulazna logika, pa izlazna
5. POST endpoint
6. Swagger test
7. Frontend: paginacija
8. Frontend: kategorije iz API
9. Frontend: create + toast
10. End-to-end test
```

**Zašto:** Imaš mapu — ne gubiš se.

---

#### Korak 5: Tek onda počni pisati kod

**Redoslijed pisanja:**

1. **Entiteti i baza** — bez baze ništa drugo ne možeš testirati
2. **Handler sa poslovnom logikom** — srž zadatka
3. **Swagger test** — potvrdi backend PRIJE frontenda
4. **Frontend** — spaja se na gotov API

**Pravilo ispita:** Ako ne stigneš sve — **backend sa ispravnom logikom** vrijedi više od lijepog UI-a bez logike.

---

### Prioriteti ako ne stigneš

| Prioritet | Šta | Zašto |
|-----------|-----|-------|
| 1 | FakturaStavkaEntity + migracija | Bez ovoga ništa ne radi |
| 2 | Create handler sa izlaznom logikom | Najviše bodova |
| 3 | POST endpoint + Swagger test | Dokaz da backend radi |
| 4 | Frontend create + toast | Cijeli tok |
| 5 | Paginacija | Lako se doda na kraju |
| 6 | Kategorije iz API | Zamjena hardkoda |

---

## 14. Checklista prije predaje

### Backend

- [ ] `FakturaStavkaEntity` kreiran sa FK na Faktura i Kategorija
- [ ] Kolekcija stavki na `FakturaEntity`
- [ ] EF konfiguracija stavki (Cascade delete sa fakture)
- [ ] `DbSet` u DbContext i IAppDbContext
- [ ] Migracija primijenjena — tabela stavki postoji u SSMS
- [ ] `CreateFakturaCommand` sa listom stavki
- [ ] Validator: bar jedna stavka, obavezna polja
- [ ] Handler: ulazna logika (povećaj/kreiraj proizvod)
- [ ] Handler: izlazna logika (provjera + smanji zalihe)
- [ ] Handler: greška = ništa se ne sprema
- [ ] `BrojStavki` se postavlja pri kreiranju
- [ ] POST endpoint u FaktureController
- [ ] Swagger test: ulazna faktura — proizvod se kreira/poveća
- [ ] Swagger test: izlazna faktura — zaliha opada
- [ ] Swagger test: izlazna bez zaliha — greška, ništa se ne mijenja

### Frontend

- [ ] Paginacija radi (nije hardkodirana)
- [ ] Kategorije se učitavaju iz API-ja (nema hardkoda)
- [ ] FormArray sa validacijom
- [ ] Dugme Sačuvaj disabled kad forma invalid
- [ ] `create()` metoda u API servisu
- [ ] `onSubmit()` poziva API
- [ ] Toast success nakon uspjeha
- [ ] Toast error nakon greške
- [ ] Uvijek navigacija nazad na listu
- [ ] Lista prikazuje novu fakturu nakon dodavanja
- [ ] Nema edit/delete kolona u tabeli

### Poslovna logika (ručno testiraj!)

- [ ] **Ulazna:** postojeći proizvod → zaliha se poveća
- [ ] **Ulazna:** novi proizvod → kreira se sa IsEnabled=false
- [ ] **Izlazna:** dovoljno zaliha → zaliha se smanji
- [ ] **Izlazna:** zaliha postane 0 → IsEnabled=false
- [ ] **Izlazna:** proizvod ne postoji → greška, ništa se ne sprema
- [ ] **Izlazna:** nema dovoljno zaliha → greška, ništa se ne sprema

---

## Završna napomena

Drugi modul je **prvi pravi „sistemski" zadatak** — ne radiš samo CRUD, već povezuješ više dijelova aplikacije (fakture → proizvodi → zalihe).

**Tri stvari koje moraš savladati:**

1. **Master-detail** — Faktura + Stavke (FormArray + 1:N relacija)
2. **Poslovna logika** — različito ponašanje za ulaznu i izlaznu
3. **Integritet podataka** — greška znači da se ništa ne smije promijeniti

Koristi **Order + OrderItem** kao backend uzorak i **faktura-add.component** kao frontend polaznu tačku — većina koda je tamo, tvoj posao je dopuniti logiku.

Ako zapneš na konkretnom koraku, pitaj sa tačnim dijelom (npr. „ne razumijem kako provjeriti zalihe prije spremanja") — objasnit ću logiku bez gotovog koda.

**Sretno na ispitu!**
