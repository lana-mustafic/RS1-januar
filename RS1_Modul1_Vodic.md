# RS1 — Vodič za Prvi Modul (26.01.2026.)

> **Cilj ovog dokumenta:** Da razumiješ *logiku* rješavanja, a ne da kopiraš gotov kod.  
> **Pravilo:** Prvo čitaj, razumij, zatim sama piši. Koristi postojeće primjere u projektu kao „udžbenik", ne kao rješenje.

---

## Sadržaj

1. [Uvod — šta je prvi modul?](#1-uvod--šta-je-prvi-modul)
2. [Priprema okruženja na ispitu](#2-priprema-okruženja-na-ispitu)
3. [Pregled arhitekture projekta](#3-pregled-arhitekture-projekta)
4. [Kako čitati bilo koji RS1 zadatak](#4-kako-čitati-bilo-koji-rs1-zadatak)
5. [Zadatak 1 — Backend (Dostavljač)](#5-zadatak-1--backend-dostavljač)
6. [Zadatak 2 — Frontend (Dostavljač)](#6-zadatak-2--frontend-dostavljač)
7. [Funkcionalnosti detaljno (pretraga, dodavanje, brisanje)](#7-funkcionalnosti-detaljno)
8. [Rječnik pojmova](#8-rječnik-pojmova)
9. [Vizuelni tok cijelog rješenja](#9-vizuelni-tok-cijelog-rješenja)
10. [Najčešće greške i debug](#10-najčešće-greške-i-debug)
11. [Kako razmišljati na ispitu — korak po korak](#11-kako-razmišljati-na-ispitu--korak-po-korak)
12. [Checklista prije predaje](#12-checklista-prije-predaje)

---

## 1. Uvod — šta je prvi modul?

Na ispitu iz **Razvoja softvera I** od **26.01.2026.** prvi modul pokriva **CRUD za entitet Dostavljač** u aplikaciji „Market".

**CRUD** znači:
- **C**reate — dodavanje novog zapisa
- **R**ead — prikaz liste i pojedinačnog zapisa
- **U**pdate — izmjena postojećeg zapisa
- **D**elete — brisanje zapisa

U template projektu prvi modul je označen u fajlu:
`rs1-frontend-2025-26/src/app/modules/admin/catalogs/dostavljaci/dostavljaci.component.html`  
(poruka: *„Ovdje raditi ispitni zadatak — prvi modul"*)

**Napomena o „dva zadatka":** Ispitni zadatak se logički dijeli na **dva dijela** koje radiš u istom projektu:
1. **Backend** — baza, entitet, CQRS, API (Visual Studio)
2. **Frontend** — Angular komponente, forme, tabela (VS Code)

Komponenta **Fakture** pripada **drugom modulu** — ne ulazi u ovaj vodič.

### Polja entiteta Dostavljač (iz zadatka)

| Polje | Tip | Pravila |
|-------|-----|---------|
| **Naziv** | tekst | obavezno |
| **Tip dostavljača** | enum | Ekstern / Intern / Freelancer — **ne pravi novu tabelu** |
| **Kod** | tekst | obavezno, max 3 slova, **jedinstven** |
| **Aktivan** | boolean | default = `true` |

### Obavezna pravila cijelog modula (crveni okvir na ispitu)

- Backend mora koristiti **CQRS** obrazac
- Frontend mora koristiti **Reactive Forms** za dodavanje i izmjenu
- **Toast poruke** za svaku uspješnu akciju i grešku
- **Paginacija** na svim listama
- **Modal potvrde** prije brisanja
- Ne troši vrijeme na dizajn — HTML/CSS već postoji u templateu

---

## 2. Priprema okruženja na ispitu

### 2.1. Koji alati trebaju biti otvoreni?

| Alat | Za šta |
|------|--------|
| **Visual Studio 2022** | Backend (C# Web API) |
| **VS Code** (ili WebStorm) | Frontend (Angular) |
| **SSMS** | Baza podataka |
| **Browser** | Testiranje aplikacije |

### 2.2. Baza podataka (SSMS)

1. Otvori **SQL Server Management Studio**
2. Poveži se na server: `10.10.10.18`
3. Autentifikacija: **SQL Server Authentication**
   - Korisnik: `sa`
   - Lozinka: `test`
4. Kreiraj **novu bazu** imenom tvog **broja indeksa** (npr. `IB210001`)

### 2.3. Connection string (backend)

Otvori: `rs1_backend-2025-26/Market.API/appsettings.json`

U sekciji `ConnectionStrings` → `Main` promijeni `Database=` na **ime tvoje baze**.

Primjer formata (ne kopiraj ime baze — stavi svoje):
```
Server=10.10.10.18,1433;Database=TVOJ_INDEKS;User ID=sa;Password=test;...
```

### 2.4. Preuzimanje projekta

- FTP: `ftp.fit.ba`
- Korisnik: `student_aj`
- Fajl: `rs1-2026-01-26-stari-silabus-projekat.zip`

### 2.5. NPM u učionici (PowerShell problem)

U PowerShellu `npm` često ne radi zbog blokiranih skripti.

**Rješenje:**
1. U terminalu ukucaj: `cmd` i pritisni Enter
2. Sada si u CMD modu
3. Pokreni: `npm install`, zatim `npm start`

### 2.6. Angular CLI

Ako `ng` nije prepoznat:
```
npx ng g c naziv-komponente
```
`npx` pokreće Angular CLI iz lokalnog `node_modules` foldera.

### 2.7. Redoslijed pokretanja

1. **Prvo backend** — u Visual Studiju pokreni `Market.API` (F5 ili zeleno dugme)
2. **Zatim frontend** — u folderu `rs1-frontend-2025-26`: `npm install` → `npm start`
3. Frontend se otvara na `http://localhost:4200`
4. Backend API je na `http://localhost:7001` (vidi `environment.ts`)

---

## 3. Pregled arhitekture projekta

### 3.1. Dva odvojena projekta

```
2026-01-26/
├── rs1_backend-2025-26/     ← Visual Studio solution (.sln)
└── rs1-frontend-2025-26/    ← Angular aplikacija
```

### 3.2. Backend — slojevi (layeri)

| Projekt | Uloga | Gdje tražiš stvari |
|---------|-------|-------------------|
| **Market.Domain** | Entiteti, enumi | `Entities/` folder |
| **Market.Application** | CQRS (Commands, Queries, Handlers, Validators) | `Modules/` folder |
| **Market.Infrastructure** | Baza, DbContext, EF konfiguracije, migracije | `Database/` folder |
| **Market.API** | REST kontroleri (HTTP endpointi) | `Controllers/` folder |
| **Market.Shared** | Zajedničke konstante i opcije | rijetko diraš na ispitu |

**Važno:** U ovom projektu **NEMA klasičnog Repository patterna**. Umjesto toga, handleri koriste **`IAppDbContext`** — to je „most" ka bazi.

### 3.3. Frontend — struktura

| Folder | Uloga |
|--------|-------|
| `src/app/api-services/` | Servisi koji zovu backend API |
| `src/app/modules/admin/catalogs/` | Admin CRUD komponente |
| `src/app/modules/shared/` | Dijeljene komponente (dialog, paginator) |
| `src/app/core/` | Bazne klase, toaster, paging modeli |

### 3.4. Gdje je šta — brza mapa

| Pojam iz starijih predmeta | Gdje je u OVOM projektu |
|---------------------------|-------------------------|
| Model (entitet) | `Market.Domain/Entities/...` |
| DbContext | `Market.Infrastructure/Database/DatabaseContext.cs` |
| Repository | **Ne postoji odvojeno** — koristi se `IAppDbContext` u handlerima |
| Forma (UI) | Angular `.component.html` + `.component.ts` |
| DataGridView (tabela) | `mat-table` u HTML-u |
| ComboBox (padajući izbor) | `mat-select` u HTML-u |
| Validacija | Backend: `*Validator.cs` / Frontend: `Validators` u Reactive Form |

### 3.5. Referentni primjer u projektu — KOPIRAJ OBRASAC, NE KOD

Prije nego pišeš bilo šta za Dostavljača, **otvori i prouči** kako je urađeno za **ProductCategories** ili **Products**:

**Backend:**
- Entitet: `Market.Domain/Entities/Catalog/ProductCategoryEntity.cs`
- List query: `Market.Application/Modules/Catalog/ProductCategories/Queries/List/`
- Create command: `Market.Application/Modules/Catalog/ProductCategories/Commands/Create/`
- Controller: `Market.API/Controllers/ProductCategoriesController.cs`

**Frontend:**
- Lista: `product-categories-2.component.ts`
- API servis: `api-services/product-categories/product-categories-api.service.ts`
- Dodavanje (primjer forme): `products-add.component.ts`

**Pravilo:** Novi entitet = isti folder pattern, ista imena fajlova, samo drugačiji naziv entiteta.

---

## 4. Kako čitati bilo koji RS1 zadatak

### 4.1. Prva tri pitanja koja si postavi

1. **Koji entitet?** → Dostavljač
2. **Koje operacije?** → CRUD + pretraga + paginacija
3. **Koja polja?** → Naziv, Tip, Kod, Aktivan

### 4.2. Ključne riječi u tekstu zadatka

| Riječ u zadatku | Šta znači za tebe |
|-----------------|-------------------|
| „entitet" | Nova klasa u `Market.Domain` |
| „enum" | Nova enumeracija (npr. TipDostavljaca) — **bez nove tabele** |
| „CQRS modul" | Folder u `Market.Application/Modules/` sa Commands i Queries |
| „Controller" | Novi fajl u `Market.API/Controllers/` |
| „API servis" | Novi folder u `src/app/api-services/` |
| „Reactive Forms" | `FormGroup`, `FormControl`, `formControlName` |
| „paginacija" | `PageRequest` / `PageResult` na backendu, `BaseListPagedComponent` na frontendu |
| „toast poruka" | `ToasterService` — `.success()` i `.error()` |
| „dialogHelper" | `DialogHelperService` — modal prije brisanja |
| „validacija backend i frontend" | Validator klasa + Angular `Validators` |
| „case-insensitive pretraga" | Ignoriši velika/mala slova u filteru |
| „jedinstven kod" | Provjeri u bazi da kod već ne postoji |

### 4.3. Na šta studenti najčešće pogriješe (općenito)

- Kreću pisati frontend **prije** nego backend radi
- Zaborave **migraciju** baze nakon novog entiteta
- Ne dodaju entitet u **DbContext** i **IAppDbContext**
- Pretraga filtrira podatke **samo na frontendu** umjesto na backendu
- Paginacija se ne ažurira nakon pretrage
- Zaborave toast poruku nakon akcije
- Brisanje ide direktno bez **modala za potvrdu**
- Validacija samo na frontendu (profesor traži i backend)

---

## 5. Zadatak 1 — Backend (Dostavljač)

---

### 5.1. Analiza zadatka

**Šta profesor traži?**

Kompletan backend za entitet Dostavljač:
- Model (entitet + enum)
- Povezivanje sa bazom (DbContext, konfiguracija, migracija)
- CQRS operacije: List, GetById, Create, Update, Delete
- Validacija poslovnih pravila
- REST API controller

**Kako prepoznati šta treba uraditi?**

U zadatku piše: *„U starter VS projektu postoji samo prazna komponenta za dostavljače. Potrebno je kreirati kompletnu implementaciju uključujući entitet, enum, CQRS modul i Controller."*

To znači: **sve na backend strani moraš ti napraviti od nule**, koristeći postojeće entitete kao uzor.

**Najčešće greške na backendu:**
- Enum se pravi kao posebna tabela (pogrešno — enum je C# tip)
- Kod se ne provjerava na jedinstvenost
- Pretraga po nazivu ne ignoriše velika/mala slova
- Zaboravi se `DbSet` u DbContext-u
- Migracija se ne pokrene

---

### 5.2. Pronalazak odgovarajućih fajlova

| Šta tražiš | Gdje ide |
|-----------|----------|
| Solution za otvaranje | `rs1_backend-2025-26/rs1_backend-2025-26.sln` |
| Novi entitet | `Market.Domain/Entities/` — napravi novi folder npr. `Catalog/` ili `Dostavljaci/` |
| Novi enum | `Market.Domain/Entities/` — u istom folderu kao entitet |
| DbContext | `Market.Infrastructure/Database/DatabaseContext.cs` |
| Interfejs baze | `Market.Application/Abstractions/IAppDbContext.cs` |
| EF konfiguracija | `Market.Infrastructure/Database/Configurations/` |
| CQRS modul | `Market.Application/Modules/` — novi folder npr. `Catalog/Dostavljaci/` |
| Controller | `Market.API/Controllers/` |
| Connection string | `Market.API/appsettings.json` |

**Kako pronaći mjesto gdje pisati kod?**

1. U Solution Exploreru pronađi entitet sličan tvojem (npr. `ProductCategoryEntity`)
2. Desni klik → **Go to References** ili pretraži ime entiteta (`Ctrl+Shift+F`)
3. Vidi sve fajlove koji ga koriste — to je tvoja „mapa" šta sve treba napraviti

---

### 5.3. Koraci rješavanja — Backend checklist

#### Korak 1: Enum za tip dostavljača

- **Otvori:** `Market.Domain/Entities/Fakture/FakturaTip.cs` kao uzorak
- **Pogledaj:** kako je definisan enum sa vrijednostima i brojevima
- **Razmisli:** Tvoj enum treba tri vrijednosti: Ekstern, Intern, Freelancer
- **Zašto:** Zadatak kaže „enum, ne treba kreirati entitet" — enum se čuva kao broj u bazi

**Detaljno — šta tačno uraditi:**

1. **Gdje kreirati fajl**
   - U Visual Studiju, u projektu `Market.Domain`, otvori folder `Entities/`
   - Napravi **novi folder** npr. `Dostavljaci/` (može i `Catalog/`, ali `Dostavljaci/` je jasnije)
   - U tom folderu kreiraj novi C# fajl, npr. `DostavljacTip.cs`

2. **Šta kopiraš kao obrazac (NE kopiraj sadržaj 1:1)**
   - Otvori `FakturaTip.cs` — vidi strukturu:
     - `namespace Market.Domain.Entities.Fakture;`
     - XML komentar iznad enuma (`/// <summary>...`)
     - `public enum ImeEnuma { ... }`
     - Svaka vrijednost ima **eksplicitni broj**: `Ulazna = 1`, `Izlazna = 2`
   - Možeš pogledati i `OrderStatusType.cs` — isti princip, samo više vrijednosti

3. **Šta ti pišeš u fajlu (logika, ne gotov kod)**
   - `namespace` → `Market.Domain.Entities.Dostavljaci` (ili folder koji si odabrala)
   - Ime enuma → npr. `DostavljacTip` (ili `TipDostavljaca` — bitno da bude jasno i konzistentno kroz cijeli projekt)
   - Tri člana enuma, **svaki sa brojem od 1 nadalje**:
     - `Ekstern = 1`
     - `Intern = 2`
     - `Freelancer = 3`
   - Opcionalno: kratki `/// <summary>` komentar iznad enuma i iznad svake vrijednosti (kao u uzorku)

4. **Zašto brojevi (= 1, = 2, = 3)?**
   - U bazi se enum **ne čuva kao tekst** „Ekstern", nego kao **cijeli broj** u koloni (npr. `1`, `2`, `3`)
   - Eksplicitni brojevi su konvencija u ovom projektu — vidi `FakturaTip`, `OrderStatusType`
   - **Ne kreiraj** poseban entitet `TipDostavljacaEntity` i **ne kreiraj** posebnu tabelu `TipoviDostavljaca` — to bi bilo pogrešno

5. **Šta NE radiš u ovom koraku**
   - Ne diraš DbContext, migraciju, handler, controller
   - Ne praviš frontend enum još — to dolazi kasnije
   - Enum je samo **tip podatka** koji ćeš koristiti u entitetu u Koraku 2

6. **Provjera da je korak gotov**
   - Solution se **builda bez greške** (Ctrl+Shift+B)
   - U Solution Exploreru vidiš novi fajl u `Market.Domain/Entities/Dostavljaci/`
   - Enum ima tačno 3 vrijednosti sa brojevima 1, 2, 3

**Primjer koda (ispravno) — `DostavljacTip.cs`:**

```csharp
namespace Market.Domain.Entities.Dostavljaci;

/// <summary>
/// Definiše tipove dostavljača.
/// </summary>
public enum DostavljacTip
{
    /// <summary>
    /// Eksterni dostavljač.
    /// </summary>
    Ekstern = 1,

    /// <summary>
    /// Interni dostavljač.
    /// </summary>
    Intern = 2,

    /// <summary>
    /// Freelancer dostavljač.
    /// </summary>
    Freelancer = 3
}
```

---

#### Korak 2: Entitet Dostavljač

- **Otvori:** `ProductCategoryEntity.cs` i `BaseEntity.cs`
- **Pogledaj:** entitet nasljeđuje `BaseEntity` (dobija `Id`, `IsDeleted`, `CreatedAtUtc`...)
- **Dodaj svojstva:** Naziv, Tip (enum), Kod, Aktivan
- **Razmisli:** U entitetu možeš imati `Constraints` klasu sa konstantama (npr. max dužina koda = 3) — isto kao kod ProductCategory
- **Zašto:** Konstante koristiš i u validatoru i u EF konfiguraciji — jedan izvor istine

**Detaljno — šta tačno uraditi:**

1. **Gdje kreirati fajl**
   - U **istom folderu** gdje si napravila enum: `Market.Domain/Entities/Dostavljaci/`
   - Novi fajl, npr. `DostavljacEntity.cs`
   - Konvencija u projektu: entiteti se zovu `*Entity` (vidi `ProductCategoryEntity`, `FakturaEntity`)

2. **Šta nasljeđuješ — otvori `BaseEntity.cs`**
   - `BaseEntity` već daje ova polja — **NE pišeš ih ponovo** u svom entitetu:
     - `int Id` — primarni ključ
     - `bool IsDeleted` — soft delete (koristi se kasnije pri brisanju)
     - `DateTime CreatedAtUtc` — automatski se puni pri kreiranju
     - `DateTime? ModifiedAtUtc` — automatski pri izmjeni
   - Tvoja klasa: `public class DostavljacEntity : BaseEntity`

3. **Koja svojstva TI dodaješ (iz zadatka)**

   | Svojstvo u entitetu | C# tip | Napomena |
   |---------------------|--------|----------|
   | `Naziv` | `string` | Obavezno polje |
   | `Tip` | tvoj enum (`DostavljacTip`) | Vidi kako `FakturaEntity` ima `public FakturaTip Tip` |
   | `Kod` | `string` | Obavezno, max 3 karaktera, jedinstven (jedinstvenost ide u EF config + handler kasnije) |
   | `Aktivan` | `bool` | Default `true` — postavljaš u **Create handleru**, ne mora u samom entitetu |

4. **Kako izgleda property u praksi (uzorci iz projekta)**
   - Otvori `ProductCategoryEntity.cs` → vidi `Name`, `IsEnabled` — običan `get; set;`
   - Otvori `FakturaEntity.cs` → vidi `Tip` — enum property; `BrojRacuna` koristi `required string` jer je obavezno
   - Za Dostavljača možeš koristiti `required` na obaveznim stringovima (`Naziv`, `Kod`) i na `Tip` — kao u `FakturaEntity`
   - `Aktivan` je običan `bool` — vrijednost `true` postaviš kad kreiraš zapis u handleru (Korak 8)

5. **`Constraints` klasa — jedan izvor istine**
   - Otvori `ProductCategoryEntity.cs` → na dnu klase vidi:
     ```
     public static class Constraints
     {
         public const int NameMaxLength = 100;
     }
     ```
   - U tvom entitetu napravi isto, npr.:
     - `KodMaxLength = 3` — **iz zadatka** (max 3 slova)
     - `NazivMaxLength = 100` — zadatak ne kaže eksplicitno, ali projekat uvijek stavlja max dužinu; uzmi 100 kao kod kategorija (ili 150 kao kod proizvoda)
   - Ove konstante **kasnije** koristiš u:
     - EF konfiguraciji (`HasMaxLength(DostavljacEntity.Constraints.KodMaxLength)`)
     - Validatoru (`MaximumLength(DostavljacEntity.Constraints.KodMaxLength)`)

6. **Šta NE dodaješ u entitet**
   - **Navigation property** — nema povezane tabele za Tip (to je enum, ne FK)
   - **Include** ti ne treba za Dostavljača
   - Ne pišeš logiku (validacija, jedinstvenost koda) u entitetu — to ide u validator i handler

7. **Using direktive na vrhu fajla**
   - `using Market.Domain.Common;` — zbog `BaseEntity`
   - Enum je u istom namespaceu/folderu — obično ne treba dodatni using

8. **Provjera da je korak gotov**
   - `DostavljacEntity` nasljeđuje `BaseEntity`
   - Ima 4 svojstva: `Naziv`, `Tip`, `Kod`, `Aktivan`
   - Ima `Constraints` sa `KodMaxLength = 3` (i `NazivMaxLength`)
   - Solution se **builda bez greške**
   - Još **nema** tabele u bazi — to dolazi u Koracima 3–5 (EF config, DbContext, migracija)

**Primjer koda (ispravno) — `DostavljacEntity.cs`:**

```csharp
using Market.Domain.Common;

namespace Market.Domain.Entities.Dostavljaci;

/// <summary>
/// Predstavlja dostavljača u sistemu.
/// </summary>
public class DostavljacEntity : BaseEntity
{
    /// <summary>
    /// Naziv dostavljača.
    /// </summary>
    public required string Naziv { get; set; }

    /// <summary>
    /// Tip dostavljača (Ekstern / Intern / Freelancer).
    /// </summary>
    public required DostavljacTip Tip { get; set; }

    /// <summary>
    /// Kod dostavljača (max 3 karaktera, jedinstven).
    /// </summary>
    public required string Kod { get; set; }

    /// <summary>
    /// Da li je dostavljač aktivan.
    /// </summary>
    public bool Aktivan { get; set; }

    /// <summary>
    /// Single source of truth for technical/business constraints.
    /// Used in validators and EF configuration.
    /// </summary>
    public static class Constraints
    {
        public const int NazivMaxLength = 100;
        public const int KodMaxLength = 3;
    }
}
```

**Vizuelno — šta imaš nakon Koraka 1 i 2:**

```
Market.Domain/
└── Entities/
    └── Dostavljaci/
        ├── DostavljacTip.cs      ← enum (Ekstern=1, Intern=2, Freelancer=3)
        └── DostavljacEntity.cs   ← entitet (Naziv, Tip, Kod, Aktivan + Constraints)
```

**Sljedeći korak:** Tek kad ovo builda, prelaziš na Korak 3 (EF konfiguracija) — bez nje baza ne zna max dužinu koda ni unique index.

#### Korak 3: EF konfiguracija

- **Otvori:** `ProductCategoryConfiguration.cs`
- **Napravi:** novi fajl `DostavljacConfiguration.cs` (ili slično) u `Configurations/`
- **Postavi:** ime tabele, max dužina za Kod i Naziv, obavezna polja, **unique index na Kod**
- **Zašto:** Bez konfiguracije EF ne zna pravila baze (npr. da je Kod max 3 karaktera)

**Detaljno — šta tačno uraditi:**

1. **Gdje kreirati fajl**
   - Projekt: `Market.Infrastructure` (NE `Market.Domain` — konfiguracija baze ide u Infrastructure sloj)
   - Putanja: `Database/Configurations/`
   - Napravi podfolder npr. `Dostavljaci/` (isto kao što `Fakture/` ima `FakturaConfiguration.cs`)
   - Novi fajl: `DostavljacConfiguration.cs`

2. **Šta kopiraš kao obrazac — otvori dva fajla**
   - **`ProductCategoryConfiguration.cs`** — osnovna struktura (ime tabele, `IsRequired`, `HasMaxLength`):
     - Klasa implementira `IEntityTypeConfiguration<ProductCategoryEntity>`
     - Metoda `Configure(EntityTypeBuilder<...> builder)`
     - `builder.ToTable("ProductCategories")`
     - `builder.Property(x => x.Name).IsRequired().HasMaxLength(...)`
   - **`UserEntityConfiguration.cs`** — za **unique index** (jedinstven email):
     - `b.HasIndex(x => x.Email).IsUnique();`
   - **`FakturaConfiguration.cs`** — za **enum property** `Tip`:
     - `builder.Property(x => x.Tip).IsRequired();`

3. **Struktura klase (logika, ne gotov kod)**
   - `public class DostavljacConfiguration : IEntityTypeConfiguration<DostavljacEntity>`
   - Jedna metoda: `public void Configure(EntityTypeBuilder<DostavljacEntity> builder)`
   - Na vrhu fajla: `using` za entitet i EF (`Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Metadata.Builders`)

4. **Šta postavljaš unutar `Configure` — redom**

   | Šta konfigurišeš | EF metoda | Zašto |
   |------------------|-----------|-------|
   | Ime tabele u bazi | `.ToTable("Dostavljaci")` | Bez ovoga EF može dati drugačije ime tabele |
   | `Naziv` | `.IsRequired()` + `.HasMaxLength(DostavljacEntity.Constraints.NazivMaxLength)` | Obavezno polje, max dužina iz Constraints |
   | `Kod` | `.IsRequired()` + `.HasMaxLength(DostavljacEntity.Constraints.KodMaxLength)` | Obavezno, max 3 karaktera |
   | `Tip` | `.IsRequired()` | Enum mora imati vrijednost (kao kod Fakture) |
   | `Aktivan` | `.IsRequired()` | Boolean kolona — uvijek mora imati vrijednost |
   | Jedinstvenost koda | `builder.HasIndex(x => x.Kod).IsUnique()` | Baza ne dozvoljava dva ista koda |

5. **Zašto koristiš `Constraints` iz entiteta?**
   - U Koraku 2 si stavila `KodMaxLength = 3` i `NazivMaxLength = 100`
   - Ovdje pišeš: `HasMaxLength(DostavljacEntity.Constraints.KodMaxLength)` — **ne pišeš broj 3 direktno**
   - Isti broj kasnije ide u validator — jedan izvor istine

6. **Da li moraš ručno registrovati konfiguraciju?**
   - **NE.** U `DatabaseConfiguration.cs` već postoji:
     - `modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);`
   - To znači: svaka klasa koja implementira `IEntityTypeConfiguration<T>` u `Market.Infrastructure` se **automatski** učita
   - Dovoljno je napraviti fajl i buildati — ne dodaješ ništa u `Program.cs`

7. **Šta NE radiš u ovom koraku**
   - Ne dodaješ `DbSet` — to je Korak 4
   - Ne pokrećeš migraciju — to je Korak 5
   - Ne pišeš handler ni validator

8. **Provjera da je korak gotov**
   - Fajl je u `Market.Infrastructure/Database/Configurations/Dostavljaci/`
   - Klasa implementira `IEntityTypeConfiguration<DostavljacEntity>`
   - Ima: `ToTable`, `IsRequired` na svim poljima, `HasMaxLength` na Naziv i Kod, `HasIndex(...).IsUnique()` na Kod
   - Solution se **builda bez greške**

**Primjer koda (ispravno) — `DostavljacConfiguration.cs`:**

```csharp
using Market.Domain.Entities.Dostavljaci;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Database.Configurations.Dostavljaci;

public sealed class DostavljacConfiguration : IEntityTypeConfiguration<DostavljacEntity>
{
    public void Configure(EntityTypeBuilder<DostavljacEntity> builder)
    {
        builder.ToTable("Dostavljaci");

        builder.Property(x => x.Naziv)
            .IsRequired()
            .HasMaxLength(DostavljacEntity.Constraints.NazivMaxLength);

        builder.Property(x => x.Kod)
            .IsRequired()
            .HasMaxLength(DostavljacEntity.Constraints.KodMaxLength);

        builder.Property(x => x.Tip)
            .IsRequired();

        builder.Property(x => x.Aktivan)
            .IsRequired();

        builder.HasIndex(x => x.Kod)
            .IsUnique();
    }
}
```

**Vizuelno — šta dodaješ u Koraku 3:**

```
Market.Infrastructure/
└── Database/
    └── Configurations/
        └── Dostavljaci/
            └── DostavljacConfiguration.cs   ← mapiranje entiteta → tabela u bazi
```

---

#### Korak 4: DbContext i IAppDbContext

- **Otvori:** `DatabaseContext.cs` i `IAppDbContext.cs`
- **Dodaj:** `DbSet` za tvoj entitet u **oba** fajla
- **Provjeri:** da su imena konzistentna
- **Zašto:** Handleri pristupaju bazi preko `IAppDbContext` — ako nema `DbSet`, ne možeš ništa spremiti

**Detaljno — šta tačno uraditi:**

1. **Zašto DVA fajla?**
   - `DatabaseContext.cs` — **stvarna** implementacija baze (EF Core klasa)
   - `IAppDbContext.cs` — **interfejs** koji handleri koriste (Application sloj ne smije direktno zavistiti od Infrastructure)
   - Handler prima `IAppDbContext` u konstruktoru → zove `context.Dostavljaci.Add(...)` itd.
   - Ako dodaš `DbSet` samo u `DatabaseContext` a zaboraviš interfejs → **build error**

2. **Fajl 1 — `DatabaseContext.cs`**
   - Putanja: `Market.Infrastructure/Database/DatabaseContext.cs`
   - Otvori i pogledaj postojeće linije, npr.:
     ```
     public DbSet<ProductCategoryEntity> ProductCategories => Set<ProductCategoryEntity>();
     public DbSet<FakturaEntity> Fakture => Set<FakturaEntity>();
     ```
   - Dodaj **jednu novu liniju** u istom stilu:
     - Tip: `DostavljacEntity`
     - Ime property-ja: `Dostavljaci` (množina — kao `ProductCategories`, `Fakture`)
     - Sintaksa: `public DbSet<DostavljacEntity> Dostavljaci => Set<DostavljacEntity>();`
   - Na vrhu fajla dodaj `using`:
     - `using Market.Domain.Entities.Dostavljaci;` (ili namespace koji si koristila u Koraku 1/2)

3. **Fajl 2 — `IAppDbContext.cs`**
   - Putanja: `Market.Application/Abstractions/IAppDbContext.cs`
   - Otvori i pogledaj postojeće linije, npr.:
     ```
     DbSet<ProductCategoryEntity> ProductCategories { get; }
     DbSet<FakturaEntity> Fakture { get; }
     ```
   - Dodaj **jednu novu liniju** — **isto ime** property-ja kao u DatabaseContext:
     - `DbSet<DostavljacEntity> Dostavljaci { get; }`
   - Dodaj isti `using` za entitet

**Primjer koda — ispravan `DatabaseContext.cs` isječak:**

```csharp
using Market.Application.Abstractions;
using Market.Domain.Entities.Dostavljaci;
using Market.Domain.Entities.Fakture;
using Market.Domain.Entities.Sales;

namespace Market.Infrastructure.Database;

public partial class DatabaseContext : DbContext, IAppDbContext
{
    public DbSet<ProductCategoryEntity> ProductCategories => Set<ProductCategoryEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<PromotionEntity> Promotions => Set<PromotionEntity>();
    public DbSet<MarketUserEntity> Users => Set<MarketUserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();

    public DbSet<FakturaEntity> Fakture => Set<FakturaEntity>();

    // NOVO — za Dostavljača:
    public DbSet<DostavljacEntity> Dostavljaci => Set<DostavljacEntity>();

    private readonly TimeProvider _clock;
    public DatabaseContext(DbContextOptions<DatabaseContext> options, TimeProvider clock) : base(options)
    {
        _clock = clock;
    }
}
```

**Primjer koda — ispravan `IAppDbContext.cs` isječak:**

```csharp
using Market.Domain.Entities.Dostavljaci;
using Market.Domain.Entities.Fakture;
using Market.Domain.Entities.Sales;

namespace Market.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<ProductEntity> Products { get; }
    DbSet<ProductCategoryEntity> ProductCategories { get; }
    DbSet<PromotionEntity> Promotions { get; }
    DbSet<MarketUserEntity> Users { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }

    DbSet<OrderEntity> Orders { get; }
    DbSet<OrderItemEntity> OrderItems { get; }

    DbSet<FakturaEntity> Fakture { get; }

    // NOVO — za Dostavljača:
    DbSet<DostavljacEntity> Dostavljaci { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

4. **Pravilo konzistentnosti imena**

   | Mjesto | Ime koje koristiš | Primjer pristupa u handleru |
   |--------|-------------------|----------------------------|
   | `DatabaseContext` | `Dostavljaci` | — |
   | `IAppDbContext` | `Dostavljaci` | — |
   | List handler | `ctx.Dostavljaci` | `.AsNoTracking()` |
   | Create handler | `context.Dostavljaci` | `.Add(entity)` |
   | Update/Delete | `context.Dostavljaci` | `.FirstOrDefaultAsync(x => x.Id == id)` |

   - Ime property-ja (`Dostavljaci`) mora biti **identično** u oba fajla
   - Ime tabele u bazi (`"Dostavljaci"` u Koraku 3) i ime `DbSet`-a **mogu** biti ista — to je uobičajeno u ovom projektu

5. **Šta NE radiš u ovom koraku**
   - Ne mijenjaš `SaveChangesAsync` — već postoji u interfejsu
   - Ne registruješ DbContext u `Program.cs` — već je registrovan
   - Još nema tabele u bazi dok ne napraviš migraciju (Korak 5)

6. **Provjera da je korak gotov**
   - `DatabaseContext.cs` ima `DbSet<DostavljacEntity> Dostavljaci`
   - `IAppDbContext.cs` ima `DbSet<DostavljacEntity> Dostavljaci { get; }`
   - Oba fajla imaju `using` za tvoj entitet
   - Solution se **builda bez greške**
   - U handlerima (kasnije) možeš pisati `context.Dostavljaci` bez greške

**Vizuelno — šta imaš nakon Koraka 3 i 4:**

```
Market.Infrastructure/
└── Database/
    ├── DatabaseContext.cs          ← + DbSet<DostavljacEntity> Dostavljaci
    └── Configurations/
        └── Dostavljaci/
            └── DostavljacConfiguration.cs

Market.Application/
└── Abstractions/
    └── IAppDbContext.cs            ← + DbSet<DostavljacEntity> Dostavljaci { get; }
```

**Sljedeći korak:** Korak 5 — migracija. Tek nakon `Add-Migration` + `Update-Database` (ili pokretanja API-ja) u SSMS-u ćeš vidjeti tabelu `Dostavljaci` sa kolonama i unique indexom na `Kod`.

#### Korak 5: Migracija baze

- **Razmisli:** Migracija = „instrukcija bazi da napravi novu tabelu"
- **Kako:** U Package Manager Console (Visual Studio):
  - Default project: `Market.Infrastructure`
  - Komanda: `Add-Migration ime-migracije`
  - Zatim: `Update-Database`
- **Alternativa:** Pokreni API — u `Program.cs` se automatski poziva `MigrateAsync()` pri startu
- **Provjeri u SSMS-u:** da li postoji nova tabela u tvojoj bazi

**Detaljno — šta tačno uraditi (i kojim redom):**

1. **Preuslovi (prije migracije)**
   - Završeni Koraci 1–4:
     - Imaš `DostavljacTip` enum
     - Imaš `DostavljacEntity`
     - Imaš `DostavljacConfiguration` (EF konfiguracija) sa unique index na `Kod`
     - Imaš `DbSet<DostavljacEntity> Dostavljaci` u `DatabaseContext` i u `IAppDbContext`
   - Provjeri `Market.API/appsettings.json` → connection string pokazuje na **tvoju bazu** (npr. `IB2xxxxx`)

2. **Varijanta A (preporučeno na ispitu): Package Manager Console u Visual Studiju**
   - U Visual Studiju otvori: **Tools → NuGet Package Manager → Package Manager Console**
   - U PMC prozoru obavezno podesi:
     - **Default project**: `Market.Infrastructure`
   - Komande (primjer imena migracije — ime je proizvoljno, ali neka bude smisleno):

```
Add-Migration add-dostavljaci
Update-Database
```

   - Šta očekuješ:
     - `Add-Migration ...` kreira novi folder/fajl u `Market.Infrastructure/Migrations/` (C# migracioni fajl)
     - `Update-Database` primijeni migraciju na tvoju bazu (kreira tabelu + indexe)

3. **Varijanta B (ako PMC pravi probleme): dotnet-ef iz terminala**
   - Ovo radi samo ako imaš instalirane EF Core tools i ako znaš putanje do projekata.
   - U root folderu solution-a pokreni (primjer, prilagodi putanje ako su drugačije):

```bash
dotnet ef migrations add add-dostavljaci --project rs1_backend-2025-26/Market.Infrastructure --startup-project rs1_backend-2025-26/Market.API
dotnet ef database update --project rs1_backend-2025-26/Market.Infrastructure --startup-project rs1_backend-2025-26/Market.API
```

   - Ako ti `dotnet ef` nije prepoznat, na ispitu se nemoj zadržavati na ovome — vrati se na PMC (Varijanta A).

4. **Alternativa (auto-migracija pri startu API-ja) — kada i kako**
   - U ovom template-u postoji automatsko pozivanje migracija pri pokretanju API-ja (u `Program.cs` se poziva `MigrateAsync()`).
   - Šta to znači praktično:
     - Ako pokreneš `Market.API` (F5) i sve je ispravno podešeno, aplikacija će pokušati sama primijeniti pending migracije.
   - Bitno:
     - Ovo **ne zamjenjuje** `Add-Migration` — i dalje moraš *napraviti* migraciju kad dodaš novi entitet.
     - Auto-migracija samo pomaže da se migracije **primijene** na bazu pri startu.

5. **Provjera u SSMS-u (obavezno)**
   - Otvori SSMS i konektuj se na server (onaj iz uputa).
   - Otvori svoju bazu → **Tables**:
     - Očekuješ tabelu: `Dostavljaci`
     - Očekuješ i EF tabelu: `__EFMigrationsHistory`
   - Brza provjera preko SQL upita:

```sql
SELECT TOP (50) *
FROM Dostavljaci;
```

   - Provjera unique indexa na `Kod`:
     - U SSMS: `Dostavljaci` → **Indexes** → vidi da postoji unique index na `Kod`
     - Ili test: probaj unijeti dva reda sa istim `Kod` (kasnije kroz API) → baza/handler treba to blokirati

6. **Najčešće greške (i kako ih prepoznaš brzo)**
   - **Migracija se kreira, ali tabela nije u tvojoj bazi**: connection string pokazuje na pogrešnu bazu.
   - **Update-Database puca**: nedostaje `DbSet`/konfiguracija ili je build već crven — prvo popravi build.
   - **Ne vidiš unique na Kod**: nisi dodala `HasIndex(x => x.Kod).IsUnique()` ili migracija nije rerun-ovana (napravi novu migraciju).

#### Korak 6: CQRS — List (Query)

- **Otvori folder:** `ProductCategories/Queries/List/`
- **Vidi strukturu:** 4 fajla — Query, QueryDto, QueryHandler (+ eventualno validator)
- **Napravi isto** za Dostavljače u `Modules/Catalog/Dostavljaci/Queries/List/`
- **Query klasa treba:** parametar za pretragu (`Search`), paging parametre
- **Handler treba:**
  1. Uzeti `IAppDbContext`
  2. Početi sa `ctx.Dostavljaci.AsNoTracking()` (ime tvog DbSet-a)
  3. Ako Search nije prazan → filtriraj po Nazivu (**case-insensitive**)
  4. Projektuj u DTO (ne vraćaj cijeli entitet)
  5. Vrati `PageResult.FromQueryableAsync(...)`
- **Zašto AsNoTracking:** Za čitanje liste ne treba EF pratiti promjene — brže je

**Case-insensitive pretraga — razmisli ovako:**
- U SQL Serveru možeš koristiti `EF.Functions.Like` ili `.ToLower()` na oba stringa
- Pogledaj kako ProductCategories radi pretragu i prilagodi

**Primjer koda (Dostavljači) — 3 fajla u `Modules/Catalog/Dostavljaci/Queries/List/`:**

`ListDostavljaciQuery.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.List;

public sealed class ListDostavljaciQuery : BasePagedQuery<ListDostavljaciQueryDto>
{
    public string? Search { get; init; }
}
```

`ListDostavljaciQueryDto.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.List;

public sealed class ListDostavljaciQueryDto
{
    public required int Id { get; init; }
    public required string Naziv { get; init; }
    public required string Kod { get; init; }
    public required DostavljacTip Tip { get; init; }
    public required bool Aktivan { get; init; }
}
```

`ListDostavljaciQueryHandler.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.List;

public sealed class ListDostavljaciQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListDostavljaciQuery, PageResult<ListDostavljaciQueryDto>>
{
    public async Task<PageResult<ListDostavljaciQueryDto>> Handle(ListDostavljaciQuery request, CancellationToken ct)
    {
        var q = ctx.Dostavljaci.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            q = q.Where(x => x.Naziv.ToLower().Contains(s));
        }

        var projectedQuery = q
            .OrderBy(x => x.Naziv)
            .Select(x => new ListDostavljaciQueryDto
            {
                Id = x.Id,
                Naziv = x.Naziv,
                Kod = x.Kod,
                Tip = x.Tip,
                Aktivan = x.Aktivan
            });

        return await PageResult<ListDostavljaciQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
```

#### Korak 7: CQRS — GetById (Query)

- **Uzorak:** `ProductCategories/Queries/GetById/`
- **Handler:** pronađi entitet po Id, ako ne postoji → baci `MarketNotFoundException`
- **Vrati DTO** sa svim poljima potrebnim za edit formu

**Primjer koda (Dostavljači) — 4 fajla u `Modules/Catalog/Dostavljaci/Queries/GetById/`:**

`GetDostavljacByIdQuery.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.GetById;

public sealed class GetDostavljacByIdQuery : IRequest<GetDostavljacByIdQueryDto>
{
    public int Id { get; set; }
}
```

`GetDostavljacByIdQueryDto.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.GetById;

public sealed class GetDostavljacByIdQueryDto
{
    public required int Id { get; init; }
    public required string Naziv { get; init; }
    public required string Kod { get; init; }
    public required DostavljacTip Tip { get; init; }
    public required bool Aktivan { get; init; }
}
```

`GetDostavljacByIdQueryHandler.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.GetById;

public sealed class GetDostavljacByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetDostavljacByIdQuery, GetDostavljacByIdQueryDto>
{
    public async Task<GetDostavljacByIdQueryDto> Handle(GetDostavljacByIdQuery request, CancellationToken ct)
    {
        var dto = await context.Dostavljaci
            .Where(x => x.Id == request.Id)
            .Select(x => new GetDostavljacByIdQueryDto
            {
                Id = x.Id,
                Naziv = x.Naziv,
                Kod = x.Kod,
                Tip = x.Tip,
                Aktivan = x.Aktivan
            })
            .FirstOrDefaultAsync(ct);

        if (dto is null)
            throw new MarketNotFoundException($"Dostavljač (ID={request.Id}) nije pronađen.");

        return dto;
    }
}
```

`GetDostavljacByIdQueryValidator.cs`

```csharp
public sealed class GetDostavljacByIdQueryValidator : AbstractValidator<GetDostavljacByIdQuery>
{
    public GetDostavljacByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be a positive value.");
    }
}
```

#### Korak 8: CQRS — Create (Command)

- **Uzorak:** `ProductCategories/Commands/Create/`
- **Fajlovi:** Command, CommandHandler, CommandValidator
- **Handler logika:**
  1. Normalizuj Naziv (trim)
  2. Provjeri da Kod ne postoji (jedinstvenost!)
  3. Kreiraj novi entitet — Aktivan default `true`
  4. `context.Dostavljaci.Add(...)` → `SaveChangesAsync`
  5. Vrati novi Id
- **Validator:** Naziv obavezan, Kod obavezan i max 3, Tip obavezan

**Primjer koda (Dostavljači) — 3 fajla u `Modules/Catalog/Dostavljaci/Commands/Create/`:**

`CreateDostavljacCommand.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Create;

public sealed class CreateDostavljacCommand : IRequest<int>
{
    public required string Naziv { get; set; }
    public required string Kod { get; set; }
    public required DostavljacTip Tip { get; set; }
    public bool Aktivan { get; set; } = true;
}
```

`CreateDostavljacCommandHandler.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Create;

public sealed class CreateDostavljacCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateDostavljacCommand, int>
{
    public async Task<int> Handle(CreateDostavljacCommand request, CancellationToken ct)
    {
        var naziv = request.Naziv?.Trim();
        var kod = request.Kod?.Trim();

        if (string.IsNullOrWhiteSpace(naziv))
            throw new ValidationException("Naziv je obavezan.");

        if (string.IsNullOrWhiteSpace(kod))
            throw new ValidationException("Kod je obavezan.");

        // Jedinstvenost koda (case-insensitive)
        var exists = await context.Dostavljaci
            .AnyAsync(x => x.Kod.ToLower() == kod.ToLower(), ct);

        if (exists)
            throw new MarketConflictException("Kod već postoji.");

        var entity = new DostavljacEntity
        {
            Naziv = naziv,
            Kod = kod,
            Tip = request.Tip,
            Aktivan = request.Aktivan
        };

        context.Dostavljaci.Add(entity);
        await context.SaveChangesAsync(ct);

        return entity.Id;
    }
}
```

`CreateDostavljacCommandValidator.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Create;

public sealed class CreateDostavljacCommandValidator : AbstractValidator<CreateDostavljacCommand>
{
    public CreateDostavljacCommandValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(DostavljacEntity.Constraints.NazivMaxLength);

        RuleFor(x => x.Kod)
            .NotEmpty().WithMessage("Kod je obavezan.")
            .MaximumLength(DostavljacEntity.Constraints.KodMaxLength);

        RuleFor(x => x.Tip)
            .IsInEnum().WithMessage("Tip nije validan.");
    }
}
```

#### Korak 9: CQRS — Update (Command)

- **Uzorak:** `ProductCategories/Commands/Update/`
- **Handler:** pronađi po Id, ažuriraj polja, provjeri jedinstvenost Koda (ali dozvoli isti kod za isti zapis)
- **Controller:** Id iz URL-a prepisuje Id u command objektu

**Primjer koda (Dostavljači) — 3 fajla u `Modules/Catalog/Dostavljaci/Commands/Update/`:**

`UpdateDostavljacCommand.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Update;

public sealed class UpdateDostavljacCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }

    public required string Naziv { get; set; }
    public required string Kod { get; set; }
    public required DostavljacTip Tip { get; set; }
    public required bool Aktivan { get; set; }
}
```

`UpdateDostavljacCommandHandler.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Update;

public sealed class UpdateDostavljacCommandHandler(IAppDbContext ctx)
    : IRequestHandler<UpdateDostavljacCommand, Unit>
{
    public async Task<Unit> Handle(UpdateDostavljacCommand request, CancellationToken ct)
    {
        var entity = await ctx.Dostavljaci
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MarketNotFoundException($"Dostavljač (ID={request.Id}) nije pronađen.");

        var kod = request.Kod.Trim();

        // Jedinstvenost koda (case-insensitive), osim za isti ID
        var exists = await ctx.Dostavljaci
            .AnyAsync(x => x.Id != request.Id && x.Kod.ToLower() == kod.ToLower(), ct);

        if (exists)
            throw new MarketConflictException("Kod već postoji.");

        entity.Naziv = request.Naziv.Trim();
        entity.Kod = kod;
        entity.Tip = request.Tip;
        entity.Aktivan = request.Aktivan;

        await ctx.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
```

`UpdateDostavljacCommandValidator.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Update;

public sealed class UpdateDostavljacCommandValidator : AbstractValidator<UpdateDostavljacCommand>
{
    public UpdateDostavljacCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Naziv)
            .NotEmpty()
            .MaximumLength(DostavljacEntity.Constraints.NazivMaxLength);

        RuleFor(x => x.Kod)
            .NotEmpty()
            .MaximumLength(DostavljacEntity.Constraints.KodMaxLength);

        RuleFor(x => x.Tip).IsInEnum();
    }
}
```

#### Korak 10: CQRS — Delete (Command)

- **Uzorak:** `DeleteProductCategoryCommandHandler.cs`
- **Handler:** pronađi entitet, pozovi `Remove()`, `SaveChangesAsync`
- **Napomena:** Projekt koristi **soft delete** — `Remove()` u DbContext-u zapravo postavlja `IsDeleted = true` (vidi `DatabaseConfiguration.cs`)

**Primjer koda (Dostavljači) — 2 fajla u `Modules/Catalog/Dostavljaci/Commands/Delete/`:**

`DeleteDostavljacCommand.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Delete;

public sealed class DeleteDostavljacCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
```

`DeleteDostavljacCommandHandler.cs`

```csharp
namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Delete;

public sealed class DeleteDostavljacCommandHandler(IAppDbContext context, IAppCurrentUser appCurrentUser)
    : IRequestHandler<DeleteDostavljacCommand, Unit>
{
    public async Task<Unit> Handle(DeleteDostavljacCommand request, CancellationToken ct)
    {
        if (appCurrentUser.UserId is null)
            throw new MarketBusinessRuleException("123", "Korisnik nije autentifikovan.");

        var entity = await context.Dostavljaci
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MarketNotFoundException("Dostavljač nije pronađen.");

        context.Dostavljaci.Remove(entity); // soft delete u ovom projektu
        await context.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
```

#### Korak 11: Controller

- **Otvori:** `ProductCategoriesController.cs`
- **Napravi:** `DostavljaciController.cs`
- **Endpointi:**
  - `GET /Dostavljaci` → List query
  - `GET /Dostavljaci/{id}` → GetById query
  - `POST /Dostavljaci` → Create command
  - `PUT /Dostavljaci/{id}` → Update command
  - `DELETE /Dostavljaci/{id}` → Delete command
- **MediatR:** Controller ne radi posao sam — šalje command/query preko `ISender sender`
- **Testiraj:** Pokreni API, otvori Swagger (`https://localhost:7001/swagger` ili port iz launchSettings)

**Detaljno — šta tačno uraditi:**

1. **Gdje kreirati fajl**
   - Projekt: `Market.API`
   - Putanja: `Controllers/DostavljaciController.cs`
   - Uzorak: `Controllers/ProductCategoriesController.cs`

2. **Šta controller radi (i šta NE radi)**
   - Controller je **tanka** HTTP „prosljeđivačica":
     - prima HTTP zahtjev (route, body, query string)
     - pozove MediatR: `await sender.Send(...)`
     - vrati HTTP odgovor (200/201/204)
   - Controller **ne smije**:
     - direktno koristiti `DatabaseContext`
     - pisati LINQ upite
     - raditi validaciju poslovnih pravila (to je u handleru/validatoru)

3. **Obavezni `using`-i na vrhu fajla**
   - Importuj sve command/query tipove koje koristiš:
     - `...Commands.Create`
     - `...Commands.Update`
     - `...Commands.Delete`
     - `...Queries.GetById`
     - `...Queries.List`

4. **Atributi klase (kopiraj obrazac)**
   - `[ApiController]` — automatska validacija modela, binding
   - `[Route("[controller]")]` — ruta postaje `/Dostavljaci` (ime kontrolera bez „Controller")
   - `[Authorize(Policy = "AdminOnly")]` — samo admin može Create/Update/Delete
   - Primary constructor: `public class DostavljaciController(ISender sender) : ControllerBase`

5. **Mapiranje endpointa — tačno šta ide gdje**

   | HTTP | Ruta | Metoda u controlleru | MediatR poziv |
   |------|------|----------------------|---------------|
   | GET | `/Dostavljaci` | `List(...)` | `sender.Send(query, ct)` |
   | GET | `/Dostavljaci/{id}` | `GetById(id, ...)` | `sender.Send(new GetDostavljacByIdQuery { Id = id }, ct)` |
   | POST | `/Dostavljaci` | `Create(...)` | `sender.Send(command, ct)` → vrati `CreatedAtAction` |
   | PUT | `/Dostavljaci/{id}` | `Update(id, command, ...)` | `command.Id = id; sender.Send(command, ct)` |
   | DELETE | `/Dostavljaci/{id}` | `Delete(id, ...)` | `sender.Send(new DeleteDostavljacCommand { Id = id }, ct)` |

6. **Bitne sitnice koje često pobjegnu**
   - **Update:** Id iz URL-a **prepisuje** Id iz body-ja:
     - `command.Id = id;` (zbog `[JsonIgnore]` na command Id property-ju)
   - **List:** koristi `[FromQuery] ListDostavljaciQuery query` — paging + search dolaze iz query stringa
   - **Create:** vrati `201 Created` preko `CreatedAtAction(nameof(GetById), new { id }, new { id })`
   - **Delete/Update:** bez return → automatski `204 No Content`
   - **GetById + List:** u uzorku imaju `[AllowAnonymous]` — za Dostavljača možeš isto (ili ostavi samo admin, zavisi od zadatka)

7. **Swagger test — redoslijed na ispitu**
   1. Pokreni `Market.API` (F5)
   2. Otvori Swagger (port iz `launchSettings.json`, često `7001`)
   3. Uloguj se / authorize (admin token) za POST/PUT/DELETE
   4. Testiraj redom:
      - `POST /Dostavljaci` — kreiraj zapis
      - `GET /Dostavljaci` — vidi listu (+ `?Search=...&Paging.Page=1&Paging.PageSize=10`)
      - `GET /Dostavljaci/{id}` — vidi jedan zapis
      - `PUT /Dostavljaci/{id}` — izmijeni
      - `DELETE /Dostavljaci/{id}` — obriši (soft delete)

8. **Provjera da je korak gotov**
   - U Swaggeru vidiš svih 5 endpointa pod `Dostavljaci`
   - Create vraća `201` sa novim Id
   - List vraća `PageResult` sa paginacijom
   - GetById za nepostojeći Id → `404`
   - Dupli Kod pri Create → `409 Conflict` (iz handlera)

**Primjer koda (ispravno) — `DostavljaciController.cs`:**

```csharp
using Market.Application.Modules.Catalog.Dostavljaci.Commands.Create;
using Market.Application.Modules.Catalog.Dostavljaci.Commands.Delete;
using Market.Application.Modules.Catalog.Dostavljaci.Commands.Update;
using Market.Application.Modules.Catalog.Dostavljaci.Queries.GetById;
using Market.Application.Modules.Catalog.Dostavljaci.Queries.List;

namespace Market.API.Controllers;

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
        // ID from the route takes precedence
        command.Id = id;
        await sender.Send(command, ct);
        // no return -> 204 No Content
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteDostavljacCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<GetDostavljacByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        var dto = await sender.Send(new GetDostavljacByIdQuery { Id = id }, ct);
        return dto; // if NotFoundException -> 404 via middleware
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<PageResult<ListDostavljaciQueryDto>> List([FromQuery] ListDostavljaciQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result;
    }
}
```

**Napomena:** Za Dostavljača **nema** `/enable` i `/disable` endpointa (to ima ProductCategories) — ti endpointi ti ne trebaju.

---

### 5.4. Vizuelni tok — Backend

```
HTTP zahtjev (npr. POST /Dostavljaci)
        ↓
Controller prima JSON body
        ↓
Controller poziva: sender.Send(createCommand)
        ↓
MediatR pronalazi CreateDostavljacCommandHandler
        ↓
ValidationBehavior pokreće CreateDostavljacCommandValidator
        ↓ (ako validacija padne → 400 Bad Request)
Handler koristi IAppDbContext
        ↓
Provjera poslovnih pravila (jedinstven kod, itd.)
        ↓
context.Dostavljaci.Add(...) → SaveChangesAsync()
        ↓
DbContext.ApplyAuditAndSoftDelete() automatski postavlja CreatedAtUtc
        ↓
Vraća se Id novog zapisa → Controller vraća 201 Created
```

---

### 5.5. Najčešće greške — Backend

| Greška | Simptom | Kako izbjeći |
|--------|---------|--------------|
| Nema DbSet | Build error: IAppDbContext nema property | Dodaj u oba fajla |
| Nema migracije | Tabela ne postoji u SSMS | Add-Migration + Update-Database |
| Jedinstven kod | Dva ista koda u bazi | Unique index + provjera u handleru |
| 404 na API | Controller nije registrovan | Provjeri ime kontrolera i route |
| 500 error | Exception u handleru | Pogledaj Output u VS i Swagger response |
| Pretraga ne radi | Uvijek svi rezultati | Provjeri da li Search parametar stiže iz query stringa |

**Debug backend:**
1. Swagger — testiraj svaki endpoint ručno
2. Visual Studio **Output** prozor
3. SSMS — provjeri da li se podaci stvarno upisuju u tabelu
4. Breakpoint u handleru (F9) → F5 debug → pošalji zahtjev

---

## 6. Zadatak 2 — Frontend (Dostavljač)

---

### 6.1. Analiza zadatka

**Šta profesor traži?**

- API servis koji komunicira sa backendom
- Komponenta liste sa tabelom, paginacijom, pretragom
- Komponenta za dodavanje (i izmjenu) sa Reactive Form
- Modal potvrde pri brisanju
- Toast poruke

**Starter stanje:** `DostavljaciComponent` je **prazan** — HTML ima izgled, ali nema logike.

**Najčešće greške na frontendu:**
- Ne registruje se nova ruta za add/edit
- Komponenta se ne doda u `admin-module.ts`
- Zaboravi se `(keydown.enter)` na pretrazi
- Forma se ne disable-uje kad je invalid
- API URL pogrešan

---

### 6.2. Pronalazak odgovarajućih fajlova

| Šta | Gdje |
|-----|------|
| Prazna komponenta | `catalogs/dostavljaci/dostavljaci.component.ts/html` |
| Routing | `admin-routing-module.ts` |
| Registracija komponente | `admin-module.ts` |
| API servis (novi) | `src/app/api-services/dostavljaci/` |
| Uzorak liste | `product-categories-2.component.ts` |
| Uzorak dodavanja | `products-add.component.ts` |
| Uzorak brisanja | `products.component.ts` → metoda `onDelete` |
| Dialog helper | `shared/services/dialog-helper.service.ts` |
| Toaster | `core/services/toaster.service.ts` |
| Paginator | `shared/components/fit-paginator-bar/` |
| Bazna klasa liste | `core/components/base-classes/base-list-paged-component.ts` |
| Bazna klasa forme | `core/components/base-classes/base-form-component.ts` |

---

### 6.3. Koraci rješavanja — Frontend checklist

#### Korak 1: API modeli i servis

- **Otvori:** `api-services/product-categories/product-categories-api.model.ts`
- **Napravi:** folder `api-services/dostavljaci/` sa `.model.ts` i `.service.ts`
- **Model treba sadržavati:**
  - Request za listu (sa paging + search)
  - Response (PageResult sa listom DTO-ova)
  - Command objekti za create/update
  - GetById DTO
- **Servis treba metode:** `list()`, `getById()`, `create()`, `update()`, `delete()`
- **Zašto:** Komponente ne zovu HTTP direktno — sve ide preko servisa (čist kod, lakše održavanje)

**Detaljno — šta tačno uraditi:**

1. **Gdje kreirati fajlove**
   - U VS Code-u otvori frontend projekat: `rs1-frontend-2025-26/`
   - Napravi folder: `src/app/api-services/dostavljaci/`
   - U njemu dva fajla:
     - `dostavljaci-api.model.ts`
     - `dostavljaci-api.service.ts`

2. **Uzorak koji kopiraš obrazac (NE kopiraj 1:1)**
   - Otvori `product-categories-api.model.ts` — vidi strukturu:
     - `List...Request extends BasePagedQuery` (paging + search)
     - `List...QueryDto` (jedan red u tabeli)
     - `Get...ByIdQueryDto` (podaci za edit formu)
     - `List...Response = PageResult<...>`
     - `Create...Command`, `Update...Command`
   - Otvori `product-categories-api.service.ts` — vidi:
     - `baseUrl = environment.apiUrl + '/ProductCategories'`
     - metode: `list`, `getById`, `create`, `update`, `delete`
     - `buildHttpParams(request)` za query string

3. **Mapiranje backend → frontend (bitno!)**

   | Backend (C#) | Frontend (TS) | JSON u HTTP |
   |--------------|---------------|-------------|
   | `ListDostavljaciQuery` | `ListDostavljaciRequest` | query string |
   | `ListDostavljaciQueryDto` | `ListDostavljaciQueryDto` | camelCase: `id`, `naziv`, `kod`, `tip`, `aktivan` |
   | `GetDostavljacByIdQueryDto` | `GetDostavljacByIdQueryDto` | isto |
   | `CreateDostavljacCommand` | `CreateDostavljacCommand` | body POST |
   | `UpdateDostavljacCommand` | `UpdateDostavljacCommand` | body PUT |
   | `DostavljacTip` enum | `DostavljacTip` enum | broj: 1, 2, 3 |

4. **Enum na frontendu**
   - Backend šalje `tip` kao **broj** (1, 2, 3)
   - Na frontendu definiši enum sa istim brojevima:
     - `Ekstern = 1`, `Intern = 2`, `Freelancer = 3`
   - Koristiš ga u modelima i u `mat-select` (Korak 8)

5. **Servis — pravila**
   - `@Injectable({ providedIn: 'root' })` — servis dostupan cijeloj app
   - `baseUrl = \`${environment.apiUrl}/Dostavljaci\`` — **mora** odgovarati controlleru
   - `list(request)` koristi `buildHttpParams` — automatski šalje `paging.page`, `paging.pageSize`, `search`
   - `create` vraća `Observable<number>` (novi Id)
   - `update` i `delete` vraćaju `Observable<void>`

6. **Šta NE radiš u ovom koraku**
   - Ne diraš komponente (`dostavljaci.component.ts`)
   - Ne diraš routing
   - Ne pišeš HTML
   - Servis se **ne registruje** u modulu — `providedIn: 'root'` je dovoljno

7. **Provjera da je korak gotov**
   - Folder `api-services/dostavljaci/` postoji sa 2 fajla
   - Model ima sve tipove (list request, list dto, getById dto, create/update command, enum)
   - Servis ima 5 metoda: `list`, `getById`, `create`, `update`, `delete`
   - Frontend se builda bez greške (`npm start` ili `ng build`)
   - U Network tabu (kasnije) vidiš pozive na `/Dostavljaci`

**Primjer koda (ispravno) — `dostavljaci-api.model.ts`:**

```typescript
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

// === ENUM (odgovara DostavljacTip.cs na backendu) ===

export enum DostavljacTip {
  Ekstern = 1,
  Intern = 2,
  Freelancer = 3,
}

// === QUERIES (READ) ===

/**
 * Query parameters for GET /Dostavljaci
 * Corresponds to: ListDostavljaciQuery.cs
 */
export class ListDostavljaciRequest extends BasePagedQuery {
  search?: string | null;
}

/**
 * Response item for GET /Dostavljaci
 * Corresponds to: ListDostavljaciQueryDto.cs
 */
export interface ListDostavljaciQueryDto {
  id: number;
  naziv: string;
  kod: string;
  tip: DostavljacTip;
  aktivan: boolean;
}

/**
 * Response for GET /Dostavljaci/{id}
 * Corresponds to: GetDostavljacByIdQueryDto.cs
 */
export interface GetDostavljacByIdQueryDto {
  id: number;
  naziv: string;
  kod: string;
  tip: DostavljacTip;
  aktivan: boolean;
}

/**
 * Paged response for GET /Dostavljaci
 */
export type ListDostavljaciResponse = PageResult<ListDostavljaciQueryDto>;

// === COMMANDS (WRITE) ===

/**
 * Command for POST /Dostavljaci
 * Corresponds to: CreateDostavljacCommand.cs
 */
export interface CreateDostavljacCommand {
  naziv: string;
  kod: string;
  tip: DostavljacTip;
  aktivan: boolean;
}

/**
 * Command for PUT /Dostavljaci/{id}
 * Corresponds to: UpdateDostavljacCommand.cs
 */
export interface UpdateDostavljacCommand {
  naziv: string;
  kod: string;
  tip: DostavljacTip;
  aktivan: boolean;
}
```

**Primjer koda (ispravno) — `dostavljaci-api.service.ts`:**

```typescript
import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListDostavljaciRequest,
  ListDostavljaciResponse,
  GetDostavljacByIdQueryDto,
  CreateDostavljacCommand,
  UpdateDostavljacCommand,
} from './dostavljaci-api.model';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({
  providedIn: 'root',
})
export class DostavljaciApiService {
  private readonly baseUrl = `${environment.apiUrl}/Dostavljaci`;
  private http = inject(HttpClient);

  /**
   * GET /Dostavljaci
   * List dostavljaci with optional query parameters (paging + search).
   */
  list(request?: ListDostavljaciRequest): Observable<ListDostavljaciResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;

    return this.http.get<ListDostavljaciResponse>(this.baseUrl, {
      params,
    });
  }

  /**
   * GET /Dostavljaci/{id}
   * Get a single dostavljac by ID.
   */
  getById(id: number): Observable<GetDostavljacByIdQueryDto> {
    return this.http.get<GetDostavljacByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * POST /Dostavljaci
   * Create a new dostavljac.
   * @returns ID of the newly created record
   */
  create(payload: CreateDostavljacCommand): Observable<number> {
    return this.http.post<number>(this.baseUrl, payload);
  }

  /**
   * PUT /Dostavljaci/{id}
   * Update an existing dostavljac.
   */
  update(id: number, payload: UpdateDostavljacCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  /**
   * DELETE /Dostavljaci/{id}
   * Delete a dostavljac.
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
```

**Vizuelno — šta dodaješ u Koraku 1:**

```
src/app/api-services/
└── dostavljaci/
    ├── dostavljaci-api.model.ts    ← tipovi + enum
    └── dostavljaci-api.service.ts  ← HTTP pozivi ka backendu
```

**Sljedeći korak:** Korak 2 — komponenta liste (`dostavljaci.component.ts`) koristi `DostavljaciApiService.list()` i nasljeđuje `BaseListPagedComponent`.

---

#### Korak 2: Komponenta liste — TypeScript

- **Otvori:** `product-categories-2.component.ts` i `products.component.ts`
- **Naslijedi:** `BaseListPagedComponent<TItem, TRequest>`
- **Implementiraj:** `loadPagedData()` — poziva API i puni `this.items`
- **Dodaj metode:**
  - `onCreate()` → navigacija na add stranicu
  - `editAction(x)` → navigacija na edit stranicu
  - `deleteAction(x)` → otvara dialog, pa briše
  - `inputKeyDown(event)` → ako Enter, pokreni pretragu
  - `searchAction()` → postavi search u request, resetuj page na 1, učitaj podatke
- **Zašto BaseListPagedComponent:** Već ima `goToPage`, `totalItems`, `totalPages` — ne pišeš paginaciju od nule

**Detaljno — šta tačno uraditi:**

1. **Gdje pišeš kod**
   - Fajl: `src/app/modules/admin/catalogs/dostavljaci/dostavljaci.component.ts`
   - Starter je **prazan** — samo `@Component` dekorator, nema logike
   - Komponenta je **već registrovana** u `admin-module.ts` i ruti `dostavljaci`

2. **Uzorci koje otvoriš**
   - **`product-categories-2.component.ts`** — lista sa pretragom, `BaseListPagedComponent`, `searchValue`, `inputKeyDown`, `editAction` sa relativnom rutom
   - **`products.component.ts`** — čistiji `loadPagedData`, `onDelete` sa dialogom, `onCreate` navigacija

3. **Naslijeđivanje — generici**
   ```typescript
   extends BaseListPagedComponent<ListDostavljaciQueryDto, ListDostavljaciRequest>
   ```
   - `TItem` = jedan red u tabeli (`ListDostavljaciQueryDto`)
   - `TRequest` = request za listu (`ListDostavljaciRequest`)

4. **Dependency injection — šta ti treba**
   - `DostavljaciApiService` — API pozivi
   - `Router` + `ActivatedRoute` — navigacija na add/edit
   - `ToasterService` — toast poruke
   - `DialogHelperService` — modal prije brisanja

5. **Constructor — inicijalizacija requesta**
   ```typescript
   constructor() {
     super();
     this.request = new ListDostavljaciRequest();
   }
   ```
   - `this.request` dolazi iz `BaseListPagedComponent`
   - Paging default (`page=1`, `pageSize=10`) postavlja `PageRequest` unutra

6. **`ngOnInit` — učitaj listu**
   ```typescript
   ngOnInit(): void {
     this.initList(); // poziva loadPagedData()
   }
   ```

7. **`loadPagedData()` — srce liste**
   - Pozovi `this.api.list(this.request)`
   - U `next`: `this.handlePageResult(response)` — puni `items`, `totalItems`, `totalPages`
   - U `error`: `toaster.error(...)` + `stopLoading()`
   - **Uvijek** koristi `this.request` — ne pravi novi objekat u `searchAction` (to je greška u product-categories-2 primjeru)

8. **`searchAction()` — ispravan obrazac**
   ```typescript
   this.request.search = this.searchValue?.trim() || null;
   this.request.paging.page = 1;  // reset stranice!
   this.loadPagedData();
   ```
   - Prazan search → `null` → backend vraća sve zapise
   - Page mora na 1 jer se broj rezultata mijenja

9. **`inputKeyDown(event)` — pretraga na Enter**
   ```typescript
   if (event.key === 'Enter') {
     this.searchAction();
   }
   ```

10. **Navigacija**
    - `onCreate()` → `this.router.navigate(['add'], { relativeTo: this.route })`
    - `editAction(x)` → `this.router.navigate(['edit', x.id], { relativeTo: this.route })`
    - Rute `add` i `edit/:id` dodaješ u Koraku 10 — ali navigacija ide već ovdje

11. **`deleteAction(x)` — modal + brisanje**
    - `dialogHelper.confirmDelete(x.naziv)` — generički dialog
    - Provjeri `result.button === DialogButton.DELETE`
    - Pozovi `api.delete(x.id)`
    - Toast success/error
    - Ponovo učitaj listu: `this.loadPagedData()`

12. **`displayedColumns` — za mat-table (HTML korak)**
    ```typescript
    displayedColumns = ['naziv', 'kod', 'tip', 'aktivan', 'actions'];
    ```

13. **Provjera da je korak gotov**
    - Komponenta nasljeđuje `BaseListPagedComponent`
    - `loadPagedData()` poziva `DostavljaciApiService.list(this.request)`
    - Pretraga radi na Enter
    - Dugmad za create/edit/delete imaju metode
    - Nema crvenih grešaka u TS fajlu

**Primjer koda (ispravno) — `dostavljaci.component.ts`:**

```typescript
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { DostavljaciApiService } from '../../../../api-services/dostavljaci/dostavljaci-api.service';
import {
  ListDostavljaciQueryDto,
  ListDostavljaciRequest,
} from '../../../../api-services/dostavljaci/dostavljaci-api.model';
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
  extends BaseListPagedComponent<ListDostavljaciQueryDto, ListDostavljaciRequest>
  implements OnInit
{
  private api = inject(DostavljaciApiService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);

  searchValue = '';

  displayedColumns: string[] = ['naziv', 'kod', 'tip', 'aktivan', 'actions'];

  constructor() {
    super();
    this.request = new ListDostavljaciRequest();
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

  editAction(item: ListDostavljaciQueryDto): void {
    this.router.navigate(['edit', item.id], { relativeTo: this.route });
  }

  deleteAction(item: ListDostavljaciQueryDto): void {
    this.dialogHelper.confirmDelete(item.naziv).subscribe((result) => {
      if (result && result.button === DialogButton.DELETE) {
        this.performDelete(item);
      }
    });
  }

  private performDelete(item: ListDostavljaciQueryDto): void {
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

**Vizuelno — tok podataka u Koraku 2:**

```
ngOnInit()
   ↓
initList() → loadPagedData()
   ↓
DostavljaciApiService.list(this.request)
   ↓
HTTP GET /Dostavljaci?paging.page=1&paging.pageSize=10&search=...
   ↓
handlePageResult(response) → this.items = response.items
   ↓
HTML (Korak 3) prikazuje this.items u mat-table
```

**Sljedeći korak:** Korak 3 — HTML (`dostavljaci.component.html`) povezuje search input, tabelu i paginator sa ovim metodama.

---

#### Korak 3: Komponenta liste — HTML

- **Otvori:** `dostavljaci.component.html` — dizajn već postoji!
- **Dopuni:**
  - `(keydown)` na search input
  - `mat-table` sa kolonama: Naziv, Kod, Tip, Aktivan, Akcije
  - Dugmad za edit i delete u koloni Akcije
  - `<app-fit-paginator-bar [vm]="this" />` na dnu
  - `*ngFor` ili `[dataSource]` za prikaz podataka
- **Tip prikaži** kao badge (boje su u CSS-u templatea)

**Detaljno — šta tačno uraditi:**

1. **Starter stanje**
   - HTML već ima **header karticu** (naslov, search polje, dugme „Novi dostavljač")
   - Ima **info karticu** („Ovdje raditi ispitni zadatak...")
   - Nema tabele, nema paginacije, nema bindinga — to dodaješ ti

2. **Uzorci koje otvoriš**
   - **`products.component.html`** — `mat-table`, `[dataSource]="items"`, `matColumnDef`, akcije, paginator
   - **`fakture.component.html`** — prikaz **Tip** kao badge sa `[ngClass]`
   - **`product-categories-2.component.html`** — `(keydown)="inputKeyDown($event)"`, `[(ngModel)]="searchValue"`

3. **Header — poveži sa TS metodama (ne diraj dizajn, samo dodaj binding)**

   | Element | Binding |
   |---------|---------|
   | Search input | `[(ngModel)]="searchValue"` + `(keydown)="inputKeyDown($event)"` |
   | Dugme „Novi dostavljač" | `(click)="onCreate()"` |

4. **Tabela — `mat-table` struktura**
   - `[dataSource]="items"` — podaci iz `BaseListPagedComponent`
   - Za svaku kolonu: `<ng-container matColumnDef="naziv">` (i kod, tip, aktivan, actions)
   - Header red: `<tr mat-header-row *matHeaderRowDef="displayedColumns">`
   - Data red: `<tr mat-row *matRowDef="let row; columns: displayedColumns">`
   - `displayedColumns` definisan u TS (Korak 2)

5. **Kolone — šta prikazuješ**

   | Kolona | Sadržaj |
   |--------|---------|
   | `naziv` | `{{ item.naziv }}` |
   | `kod` | `{{ item.kod }}` (bold) |
   | `tip` | badge sa `getTipLabel(item.tip)` i `[ngClass]="getTipClass(item.tip)"` |
   | `aktivan` | ikonica `check_circle` / `cancel` ili „Da"/„Ne" |
   | `actions` | dugmad edit + delete |

6. **Tip kao badge — treba 2 helper metode u TS**
   - U `dostavljaci.component.ts` dodaj `getTipLabel(tip)` i `getTipClass(tip)` (vidi primjer ispod)
   - U `dostavljaci.component.scss` dodaj `.tip-badge` klase (kao kod Faktura)

7. **Loading i prazna lista**
   - `*ngIf="isLoading"` → spinner ili tekst
   - `*ngIf="!isLoading && totalItems === 0"` → slika `images/no-data.png` ili poruka
   - Tabelu prikaži kad `!isLoading && totalItems > 0`

8. **Paginator**
   - Na dnu tabele: `<app-fit-paginator-bar [vm]="this" />`
   - `[vm]="this"` — komponenta nasljeđuje paging (`goToPage`, `totalItems`...)

9. **Info karticu**
   - Možeš **ukloniti** kad lista radi, ili ostaviti dok ne završiš cijeli modul

10. **Provjera da je korak gotov**
    - Search reaguje na Enter
    - Tabela prikazuje podatke iz API-ja
    - Edit/delete dugmad rade
    - Paginator pri dnu
    - Tip se vidi kao obojeni badge

**Dopuna u TS — helper metode za badge (dodaj u `dostavljaci.component.ts`):**

```typescript
import { DostavljacTip } from '../../../../api-services/dostavljaci/dostavljaci-api.model';

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

**Dopuna u SCSS — badge stilovi (dodaj u `dostavljaci.component.scss`):**

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

**Primjer koda (ispravno) — `dostavljaci.component.html`:**

```html
<div class="container">
  <!-- Header Card (starter dizajn + binding) -->
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

  <!-- Loading -->
  <div *ngIf="isLoading" class="loading-container">
    <mat-spinner diameter="40"></mat-spinner>
    <p>Učitavanje...</p>
  </div>

  <!-- No data -->
  <div *ngIf="!isLoading && totalItems === 0" class="no-data">
    <img src="images/no-data.png" alt="Nema podataka" />
    <p>Nema dostavljača za prikaz.</p>
  </div>

  <!-- Table -->
  <div class="table-card" *ngIf="!isLoading && totalItems > 0">
    <table mat-table [dataSource]="items">
      <!-- Naziv -->
      <ng-container matColumnDef="naziv">
        <th mat-header-cell *matHeaderCellDef>NAZIV</th>
        <td mat-cell *matCellDef="let item">
          <span style="font-weight: 500">{{ item.naziv }}</span>
        </td>
      </ng-container>

      <!-- Kod -->
      <ng-container matColumnDef="kod">
        <th mat-header-cell *matHeaderCellDef>KOD</th>
        <td mat-cell *matCellDef="let item">
          <span style="font-weight: 600">{{ item.kod }}</span>
        </td>
      </ng-container>

      <!-- Tip (badge) -->
      <ng-container matColumnDef="tip">
        <th mat-header-cell *matHeaderCellDef>TIP</th>
        <td mat-cell *matCellDef="let item">
          <span class="tip-badge" [ngClass]="getTipClass(item.tip)">
            {{ getTipLabel(item.tip) }}
          </span>
        </td>
      </ng-container>

      <!-- Aktivan -->
      <ng-container matColumnDef="aktivan">
        <th mat-header-cell *matHeaderCellDef>AKTIVAN</th>
        <td mat-cell *matCellDef="let item">
          <mat-icon [ngClass]="item.aktivan ? 'icon-enabled' : 'icon-disabled'">
            {{ item.aktivan ? 'check_circle' : 'cancel' }}
          </mat-icon>
        </td>
      </ng-container>

      <!-- Akcije -->
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

**Vizuelno — šta povezuješ:**

```
searchValue + (keydown Enter)  →  searchAction()  →  API  →  items
items  →  mat-table [dataSource]
displayedColumns  →  matColumnDef kolone
[vm]="this"  →  paginator (page, totalItems, goToPage)
edit/delete dugmad  →  editAction(item) / deleteAction(item)
```

**Sljedeći korak:** Korak 4 — generiši Add komponentu (`dostavljac-add`).

---

#### Korak 4: Generisanje Add komponente

- **U CMD terminalu** (u folderu frontend projekta):
  ```
  npx ng g c modules/admin/catalogs/dostavljaci/dostavljac-add --skip-tests
  ```
- **Registruj** komponentu u `admin-module.ts`
- **Dodaj rutu** u `admin-routing-module.ts`: `dostavljaci/add`

**Detaljno — šta tačno uraditi:**

1. **Terminal — koristi CMD, ne PowerShell**
   - U PowerShellu `npm`/`ng` često ne rade
   - U terminalu ukucaj `cmd` pa Enter
   - Idi u folder projekta:
     ```
     cd rs1-frontend-2025-26
     ```
   - Pokreni Angular CLI:
     ```
     npx ng g c modules/admin/catalogs/dostavljaci/dostavljac-add --skip-tests
     ```

2. **Šta komanda kreira**
   - Folder: `src/app/modules/admin/catalogs/dostavljaci/dostavljac-add/`
   - Fajlovi:
     - `dostavljac-add.component.ts`
     - `dostavljac-add.component.html`
     - `dostavljac-add.component.scss`
   - Selector: `app-dostavljac-add`
   - Klasa: `DostavljacAddComponent`

3. **Registracija u `admin-module.ts`**
   - Otvori: `src/app/modules/admin/admin-module.ts`
   - Dodaj import na vrh:
     ```typescript
     import { DostavljacAddComponent } from './catalogs/dostavljaci/dostavljac-add/dostavljac-add.component';
     ```
   - Dodaj u `declarations` niz:
     ```typescript
     DostavljacAddComponent,
     ```
   - **Napomena:** Angular CLI ponekad automatski doda u modul — provjeri da li već postoji prije duplog dodavanja

4. **Ruta u `admin-routing-module.ts`**
   - Otvori: `src/app/modules/admin/admin-routing-module.ts`
   - Dodaj import:
     ```typescript
     import { DostavljacAddComponent } from './catalogs/dostavljaci/dostavljac-add/dostavljac-add.component';
     ```
   - Dodaj rutu **ispod** postojeće `dostavljaci` rute (uzorak: `product-categories-2/add`):
     ```typescript
     // DOSTAVLJACI
     {
       path: 'dostavljaci',
       component: DostavljaciComponent,
     },
     {
       path: 'dostavljaci/add',
       component: DostavljacAddComponent,
     },
     ```

5. **Zašto ruta mora biti `dostavljaci/add`**
   - U listi (Korak 2) imaš: `this.router.navigate(['add'], { relativeTo: this.route })`
   - Kad si na `/admin/dostavljaci`, relativna navigacija `add` vodi na `/admin/dostavljaci/add`
   - Ruta u routing modulu **mora** postojati da Angular zna koju komponentu učitati

6. **Starter komponenta — šta dobiješ odmah**
   - Prazna komponenta (samo `@Component` dekorator)
   - Logiku forme dodaješ u **Koraku 6** (Reactive Form)
   - U ovom koraku samo: generiši, registruj, rutiraj, provjeri navigaciju

7. **Provjera da je korak gotov**
   - Folder `dostavljac-add/` postoji sa 3 fajla
   - `DostavljacAddComponent` je u `admin-module.ts` → `declarations`
   - Ruta `dostavljaci/add` postoji u `admin-routing-module.ts`
   - Frontend se builda bez greške
   - Klik na „Novi dostavljač" otvara praznu add stranicu (forma dolazi u Koraku 6)

**Primjer koda — isječak `admin-module.ts` (dodaci):**

```typescript
import { DostavljacAddComponent } from './catalogs/dostavljaci/dostavljac-add/dostavljac-add.component';

@NgModule({
  declarations: [
    // ... postojeće komponente ...
    DostavljaciComponent,
    DostavljacAddComponent,  // NOVO
    FaktureComponent,
    // ...
  ],
  imports: [
    AdminRoutingModule,
    SharedModule,
  ]
})
export class AdminModule { }
```

**Primjer koda — isječak `admin-routing-module.ts` (dodaci):**

```typescript
import { DostavljacAddComponent } from './catalogs/dostavljaci/dostavljac-add/dostavljac-add.component';

const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      // ... ostale rute ...

      // DOSTAVLJACI
      {
        path: 'dostavljaci',
        component: DostavljaciComponent,
      },
      {
        path: 'dostavljaci/add',
        component: DostavljacAddComponent,
      },

      // ...
    ],
  },
];
```

**Primjer koda — starter `dostavljac-add.component.ts` (odmah nakon generisanja):**

```typescript
import { Component } from '@angular/core';

@Component({
  selector: 'app-dostavljac-add',
  standalone: false,
  templateUrl: './dostavljac-add.component.html',
  styleUrl: './dostavljac-add.component.scss',
})
export class DostavljacAddComponent {

}
```

**Primjer koda — starter `dostavljac-add.component.html` (privremeno, dok ne dođe forma):**

```html
<div class="container">
  <h1>Novi dostavljač</h1>
  <p>Forma za dodavanje ide ovdje (Korak 6).</p>
</div>
```

**Vizuelno — šta dodaješ u Koraku 4:**

```
src/app/modules/admin/
├── admin-module.ts              ← + DostavljacAddComponent u declarations
├── admin-routing-module.ts      ← + ruta dostavljaci/add
└── catalogs/dostavljaci/
    ├── dostavljaci.component.ts ← lista (Korak 2)
    └── dostavljac-add/          ← NOVO (Korak 4)
        ├── dostavljac-add.component.ts
        ├── dostavljac-add.component.html
        └── dostavljac-add.component.scss
```

**Tok navigacije:**

```
Lista (/admin/dostavljaci)
   ↓ klik "Novi dostavljač" → onCreate()
Router → /admin/dostavljaci/add
   ↓
DostavljacAddComponent se učita
```

**Sljedeći korak:** Korak 5 — generiši Edit komponentu (`dostavljac-edit`) i rutu `dostavljaci/edit/:id`.

---

#### Korak 5: Generisanje Edit komponente

- Isto kao Add, ali ruta: `dostavljaci/edit/:id`
- Edit komponenta u `ngOnInit` učitava podatke po Id-u

**Detaljno — šta tačno uraditi:**

1. **Terminal — CMD (isto kao Korak 4)**
   ```
   cd rs1-frontend-2025-26
   npx ng g c modules/admin/catalogs/dostavljaci/dostavljac-edit --skip-tests
   ```

2. **Šta komanda kreira**
   - Folder: `src/app/modules/admin/catalogs/dostavljaci/dostavljac-edit/`
   - Klasa: `DostavljacEditComponent`
   - Selector: `app-dostavljac-edit`

3. **Registracija u `admin-module.ts`**
   - Import:
     ```typescript
     import { DostavljacEditComponent } from './catalogs/dostavljaci/dostavljac-edit/dostavljac-edit.component';
     ```
   - U `declarations`:
     ```typescript
     DostavljacEditComponent,
     ```

4. **Ruta u `admin-routing-module.ts`**
   - Import `DostavljacEditComponent`
   - Dodaj rutu:
     ```typescript
     {
       path: 'dostavljaci/edit/:id',
       component: DostavljacEditComponent,
     },
     ```
   - **Bitno:** parametar se zove `:id` — mora se poklapati sa listom:
     - Lista (Korak 2): `navigate(['edit', item.id], { relativeTo: this.route })`
     - URL postaje: `/admin/dostavljaci/edit/5`
     - U edit komponenti čitaš: `this.route.snapshot.params['id']`

5. **`ngOnInit` — učitavanje podataka po Id-u**
   - Inject: `ActivatedRoute`, `DostavljaciApiService`, `ToasterService`, `Router`
   - U `ngOnInit`:
     1. Pročitaj Id iz rute: `this.dostavljacId = +this.route.snapshot.params['id'];`
     2. Pozovi `api.getById(this.dostavljacId)`
     3. U `next`: spremi DTO u property (npr. `this.model = response`)
     4. U `error`: toast + navigacija nazad na listu
   - **Formu** (`FormGroup`, `patchValue`) dodaješ u **Koraku 7** — ovdje je dovoljno da podaci stignu sa API-ja

6. **Razlika Add vs Edit**

   | | Add (Korak 4) | Edit (Korak 5) |
   |---|---------------|----------------|
   | Ruta | `dostavljaci/add` | `dostavljaci/edit/:id` |
   | Id u URL | nema | ima (`:id`) |
   | ngOnInit | prazno (forma u Koraku 6) | `getById(id)` |
   | API poziv | `create()` (Korak 6) | `getById()` sada, `update()` (Korak 7) |

7. **Provjera da je korak gotov**
   - Folder `dostavljac-edit/` postoji
   - Komponenta registrovana u `admin-module.ts`
   - Ruta `dostavljaci/edit/:id` u routing modulu
   - Klik „Uredi" u listi otvara edit stranicu
   - U konzoli/network vidiš `GET /Dostavljaci/{id}`
   - Podaci se učitavaju (barem u TS property, ili privremeno prikaži u HTML)

**Primjer koda — isječak `admin-routing-module.ts` (dodaci):**

```typescript
import { DostavljacEditComponent } from './catalogs/dostavljaci/dostavljac-edit/dostavljac-edit.component';

// DOSTAVLJACI
{
  path: 'dostavljaci',
  component: DostavljaciComponent,
},
{
  path: 'dostavljaci/add',
  component: DostavljacAddComponent,
},
{
  path: 'dostavljaci/edit/:id',
  component: DostavljacEditComponent,
},
```

**Primjer koda — `dostavljac-edit.component.ts` (učitavanje po Id-u):**

```typescript
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DostavljaciApiService } from '../../../../api-services/dostavljaci/dostavljaci-api.service';
import { GetDostavljacByIdQueryDto } from '../../../../api-services/dostavljaci/dostavljaci-api.model';
import { BaseComponent } from '../../../../core/components/base-classes/base-component';
import { ToasterService } from '../../../../core/services/toaster.service';

@Component({
  selector: 'app-dostavljac-edit',
  standalone: false,
  templateUrl: './dostavljac-edit.component.html',
  styleUrl: './dostavljac-edit.component.scss',
})
export class DostavljacEditComponent extends BaseComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private api = inject(DostavljaciApiService);
  private toaster = inject(ToasterService);

  dostavljacId!: number;
  model: GetDostavljacByIdQueryDto | null = null;

  ngOnInit(): void {
    this.dostavljacId = +this.route.snapshot.params['id'];

    if (!this.dostavljacId || this.dostavljacId <= 0) {
      this.toaster.error('Neispravan ID dostavljača.');
      this.router.navigate(['/admin/dostavljaci']);
      return;
    }

    this.loadDostavljac();
  }

  private loadDostavljac(): void {
    this.startLoading();

    this.api.getById(this.dostavljacId).subscribe({
      next: (response) => {
        this.model = response;
        this.stopLoading();
        // Korak 7: ovdje ide form.patchValue(response)
      },
      error: (err) => {
        this.stopLoading();
        this.toaster.error(err?.message ?? 'Dostavljač nije pronađen.');
        console.error('Load dostavljac error:', err);
        this.router.navigate(['/admin/dostavljaci']);
      },
    });
  }
}
```

**Primjer koda — privremeni `dostavljac-edit.component.html` (dok ne dođe forma u Koraku 7):**

```html
<div class="container">
  <h1>Uredi dostavljača</h1>

  <div *ngIf="isLoading">Učitavanje...</div>

  <div *ngIf="!isLoading && model">
    <p><strong>ID:</strong> {{ model.id }}</p>
    <p><strong>Naziv:</strong> {{ model.naziv }}</p>
    <p><strong>Kod:</strong> {{ model.kod }}</p>
    <p><strong>Tip:</strong> {{ model.tip }}</p>
    <p><strong>Aktivan:</strong> {{ model.aktivan ? 'Da' : 'Ne' }}</p>
    <p><em>Reactive forma ide ovdje (Korak 7).</em></p>
  </div>
</div>
```

**Vizuelno — tok edit navigacije:**

```
Lista → klik "Uredi" na redu
   ↓
editAction(item) → navigate(['edit', item.id])
   ↓
URL: /admin/dostavljaci/edit/5
   ↓
DostavljacEditComponent.ngOnInit()
   ↓
route.params['id'] → 5
   ↓
api.getById(5) → GetDostavljacByIdQueryDto
   ↓
model = response (Korak 7: patchValue u formu)
```

**Sljedeći korak:** Korak 6 — Reactive Form za **dodavanje** (`dostavljac-add`).

---

#### Korak 6: Reactive Form — dodavanje

- **Otvori:** `products-add.component.ts` kao uzorak
- **Naslijedi:** `BaseFormComponent`
- **Kreiraj FormGroup** sa poljima: naziv, tip, kod, aktivan
- **Validatori na frontendu:**
  - naziv → required
  - tip → required
  - kod → required, maxLength(3)
  - aktivan → default true
- **HTML:** `[formGroup]="form"`, `formControlName="naziv"`, itd.
- **Dugme Sačuvaj:** `[disabled]="form.invalid || isSaving"`
- **onSubmit:** pozovi API create, toast success, navigiraj nazad na listu

**Detaljno — šta tačno uraditi:**

1. **Gdje pišeš kod**
   - `src/app/modules/admin/catalogs/dostavljaci/dostavljac-add/dostavljac-add.component.ts`
   - `src/app/modules/admin/catalogs/dostavljaci/dostavljac-add/dostavljac-add.component.html`

2. **Uzorak — otvori `products-add.component.ts`**
   - Nasljeđuje `BaseFormComponent<TModel>`
   - `ngOnInit` → `this.initForm(false)` — **false** = add mode
   - `loadData()` — prazno u add modu (abstract metoda, mora postojati)
   - `save()` — poziva `api.create(...)`, toast, navigacija
   - HTML: `[formGroup]="form"` + `(ngSubmit)="onSubmit()"`

3. **`BaseFormComponent` — šta dobijaš besplatno**
   - `form: FormGroup`
   - `onSubmit()` — već radi `markAllAsTouched()` + poziva tvoj `save()`
   - `hasError(controlName)` — za prikaz grešaka u HTML-u
   - **Napomena:** U projektu se koristi `isLoading` (ne `isSaving`) za disabled dugme

4. **FormGroup — polja i validatori**

   | Control | Validators | Default |
   |---------|------------|---------|
   | `naziv` | `Validators.required` | `''` |
   | `tip` | `Validators.required` | `null` |
   | `kod` | `Validators.required`, `Validators.maxLength(3)` | `''` |
   | `aktivan` | — | `true` |

5. **`save()` — logika**
   - Provjeri `form.invalid` → return
   - `startLoading()`
   - Napravi `CreateDostavljacCommand` iz `form.value`
   - `api.create(command).subscribe(...)`
   - Success: `toaster.success(...)`, `router.navigate(['/admin/dostavljaci'])`
   - Error: `toaster.error(...)`, `stopLoading()`

6. **HTML — Reactive Forms binding**
   - `<form [formGroup]="form" (ngSubmit)="onSubmit()">`
   - Svako polje: `formControlName="naziv"` (i tip, kod, aktivan)
   - Tip: `mat-select` sa opcijama enuma (Ekstern, Intern, Freelancer)
   - Aktivan: `mat-checkbox` ili `mat-slide-toggle`
   - Dugme: `[disabled]="form.invalid || isLoading"`

7. **Provjera da je korak gotov**
   - Forma se prikaže na `/admin/dostavljaci/add`
   - Prazna forma → dugme Sačuvaj disabled
   - Bez naziva/koda/tipa → validacija crvena
   - Kod duži od 3 → greška
   - Uspješan submit → toast + nazad na listu + novi zapis vidljiv

**Primjer koda — `dostavljac-add.component.ts`:**

```typescript
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BaseFormComponent } from '../../../../core/components/base-classes/base-form-component';
import { DostavljaciApiService } from '../../../../api-services/dostavljaci/dostavljaci-api.service';
import {
  CreateDostavljacCommand,
  DostavljacTip,
  GetDostavljacByIdQueryDto,
} from '../../../../api-services/dostavljaci/dostavljaci-api.model';
import { ToasterService } from '../../../../core/services/toaster.service';

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
    this.initForm(false); // Add mode
    this.form = this.fb.group({
      naziv: ['', [Validators.required]],
      tip: [null, [Validators.required]],
      kod: ['', [Validators.required, Validators.maxLength(3)]],
      aktivan: [true],
    });
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

**Primjer koda — `dostavljac-add.component.html`:**

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

**Vizuelni tok — dodavanje:**

```
/admin/dostavljaci/add
   ↓
ngOnInit → initForm(false) → prazna forma (aktivan=true)
   ↓
Korisnik popuni polja
   ↓
Klik "Sačuvaj" → onSubmit() → save()
   ↓
api.create(command) → POST /Dostavljaci
   ↓
toast success → router.navigate(['/admin/dostavljaci'])
   ↓
Lista se učita → novi zapis vidljiv
```

**Sljedeći korak:** Korak 7 — ista forma za **edit** (`dostavljac-edit`), ali `initForm(true)` + `getById` + `update`.

---

#### Korak 7: Reactive Form — izmjena

- Ista forma kao Add, ali:
  - `initForm(true)` — edit mode
  - `loadData()` učitava postojeći zapis preko `getById`
  - `save()` poziva `update` umjesto `create`

#### Korak 8: Enum na frontendu — padajući izbor (ComboBox)

- **Za tip dostavljača** koristi `mat-select`
- **Opcije:** možeš hardkodirati niz `{ id: 1, name: 'Ekstern' }` ili učitati iz backend enum vrijednosti
- **Povezivanje:** `formControlName="tip"` i `[value]="tip.id"`

#### Korak 9: Brisanje sa dialogHelper

- **Otvori:** `products.component.ts` → metoda `onDelete`
- **Logika:**
  1. Pozovi `dialogHelper.confirmDelete(naziv)` — ili dodaj sekciju u dialogHelper za dostavljače
  2. U subscribe provjeri da li je kliknuto DELETE dugme
  3. Pozovi API delete
  4. Toast success/error
  5. Ponovo učitaj listu
- **Poruka modala:** „Da li ste sigurni da želite obrisati {{naziv}}?"

#### Korak 10: Routing i navigacija

- **Provjeri** da u `admin-routing-module.ts` postoje rute:
  - `dostavljaci` → lista
  - `dostavljaci/add` → dodavanje
  - `dostavljaci/edit/:id` → izmjena
- **Dugme „Novi dostavljač"** → `routerLink="add"` ili `router.navigate`

#### Korak 11: Testiranje cijelog toka

1. Login u aplikaciju (admin nalog)
2. Klikni „Dostavljači" u sidebaru
3. Vidi listu (praznu ili sa seed podacima)
4. Dodaj novog → provjeri toast → vidi ga u listi
5. Pretraži po nazivu → Enter
6. Izmijeni zapis
7. Obriši → modal → potvrdi → toast

---

### 6.4. Vizuelni tok — Frontend (dodavanje)

```
Korisnik klikne „Novi dostavljač"
        ↓
Router navigira na /admin/dostavljaci/add
        ↓
DostavljacAddComponent se učita
        ↓
ngOnInit → initForm(false) → FormGroup sa praznim poljima
        ↓
Korisnik popuni polja
        ↓
Klikne „Sačuvaj" → onSubmit()
        ↓
form.markAllAsTouched() → provjera form.invalid
        ↓ (ako invalid → dugme ostaje disabled, prikaži greške)
save() → DostavljaciApiService.create(payload)
        ↓
HTTP POST → Backend API
        ↓
Backend vrati Id → subscribe next
        ↓
ToasterService.success("Uspješno sačuvano")
        ↓
Router.navigate(['/admin/dostavljaci'])
        ↓
Lista se ponovo učita → novi zapis vidljiv
```

---

## 7. Funkcionalnosti detaljno

### 7.1. Pretraga (Funkcionalnost pretrage)

**Zahtjev:**
- Filtriraj po **nazivu**
- Pokreće se na **Enter**
- **Case-insensitive**
- Prazan input → svi zapisi
- **Paginacija se mora ažurirati** (manje rezultata = manje stranica)

**Gdje implementirati:**
- **Backend:** u List Query Handler-u — filter prije paginacije
- **Frontend:** u searchAction — postavi `request.search = searchValue`, `request.paging.page = 1`, pozovi `loadPagedData()`

**Zašto page = 1:** Ako si bila na stranici 3, a pretraga vrati 2 rezultata, stranica 3 ne postoji.

**Test:**
- Unesi „express" → treba naći „Express Dostava"
- Unesi „EXPRESS" → isti rezultat
- Obriši tekst, Enter → svi zapisi

---

### 7.2. Dodavanje (Funkcionalnost dodavanja)

**Zahtjev:**
- Obavezna polja: Naziv, Tip, Kod
- Kod max 3 slova
- Aktivan default true
- Validacija backend + frontend
- Dugme Sačuvaj disabled ako validacija ne prolazi
- Nakon uspjeha → nazad na listu + toast

**Checklist:**
- [ ] Backend validator odbija prazan naziv
- [ ] Backend validator odbija kod duži od 3
- [ ] Backend handler provjerava jedinstvenost koda
- [ ] Frontend forma ima iste validatore
- [ ] Dugme `[disabled]="form.invalid"`
- [ ] Toast na success i error
- [ ] Navigacija nazad na listu

---

### 7.3. Brisanje (Funkcionalnost brisanja)

**Zahtjev:**
- Klik na delete → modal: „Da li ste sigurni da želite obrisati {{naziv}}?"
- Potvrdi → obriši → osvježi listu → toast
- Otkaži → samo zatvori modal

**Checklist:**
- [ ] Koristi se `DialogHelperService`
- [ ] Provjera `result.button === DialogButton.DELETE`
- [ ] Nakon brisanja pozovi `loadPagedData()` ili `initList()`
- [ ] Toast success/error

---

## 8. Rječnik pojmova

> Pojmovi iz tvog spiska, objašnjeni za **ovaj** projekt (Web API + Angular).  
> Ako si ranije učila WinForms, poredim i sa tim.

---

### LINQ

**Šta je:** Način da pišeš upite nad kolekcijama/bazom na C# na „engleskom" načinu.

**Zašto se koristi:** U handlerima filtriraš, sortiraš i projektuješ podatke prije nego ih vratiš.

**Kada prepoznati:** Kad u zadatku piše „filtriraj", „pretraži", „sortiraj" — u handleru koristiš LINQ metode nad `IQueryable`.

**Primjer (općenito, ne rješenje):**
```
var rezultat = lista.Where(x => x.Ime.StartsWith("A")).OrderBy(x => x.Ime);
```
`Where` = filter, `OrderBy` = sortiranje.

---

### Lambda izrazi

**Šta je:** Kratka funkcija bez imena: `x => x.Naziv`

**Zašto se koristi:** LINQ metode primaju lambda kao pravilo filtriranja.

**Kada prepoznati:** Uvijek pored `Where`, `Select`, `OrderBy`.

**Primjer:** `.Where(x => x.Aktivan == true)` — „uzmi samo aktivne".

---

### DbSet

**Šta je:** „Tabela" u kodu — predstavlja kolekciju entiteta u bazi.

**Zašto se koristi:** Preko njega dodaješ, čitaš, brišeš zapise: `context.Dostavljaci.Add(...)`.

**Kada prepoznati:** Kad praviš novi entitet — moraš dodati `DbSet<TvojEntitet>` u DbContext.

**Primjer:** U `DatabaseContext.cs` vidiš `DbSet<ProductCategoryEntity> ProductCategories`.

---

### Include

**Šta je:** EF Core naredba koja u jednom upitu učitava i povezane entitete.

**Zašto se koristi:** Kad trebaš podatke iz dvije tabele (npr. proizvod + kategorija).

**Kada prepoznati:** Kad u listi prikazuješ podatke iz **druge tabele** (npr. ime kategorije pored proizvoda).

**Za Dostavljača:** Vjerovatno **NE treba** Include — Tip je enum, ne foreign key na drugu tabelu.

**Primjer (općenito):** `.Include(x => x.Kategorija)` — učitaj i kategoriju uz proizvod.

---

### Foreign Key

**Šta je:** Kolona koja pokazuje na Id u drugoj tabeli (veza između tabela).

**Zašto se koristi:** Povezuje entitete (npr. Order → User).

**Kada prepoznati:** Kad zadatak kaže „povezano sa...", „pripada...", „referenca na...".

**Za Dostavljača:** Tip je **enum**, ne FK — nema posebne tabele Tipovi.

---

### Navigation Property

**Šta je:** Property u entitetu koji predstavlja povezani entitet (npr. `Product.Category`).

**Zašto se koristi:** Omogućava EF Include i pristup povezanim podacima.

**Primjer u projektu:** `ProductCategoryEntity.Products` — lista proizvoda u kategoriji.

---

### DTO (Data Transfer Object)

**Šta je:** Klasa samo za prenos podataka — nema poslovnu logiku.

**Zašto se koristi:**
- Ne šalješ cijeli entitet na frontend (sigurnost, manje podataka)
- List DTO ima samo kolone za tabelu
- GetById DTO ima sve za edit formu

**Kada prepoznati:** Fajlovi koji se zovu `*QueryDto`, `*Command`, `*Response`.

**Primjer:** `ListProductCategoriesQueryDto` ima `Id`, `Name`, `IsEnabled` — samo ono što treba tabeli.

---

### Validacija

**Šta je:** Provjera da li su podaci ispravni prije spremanja.

**Zašto se koristi:** Spriječava loše podatke u bazi (prazan naziv, predugačak kod).

**Gdje u projektu:**
- **Backend:** `*Validator.cs` (FluentValidation) + provjere u handleru (jedinstvenost)
- **Frontend:** Angular `Validators.required`, `Validators.maxLength(3)` u FormGroup

**Kada prepoznati:** Zadatak eksplicitno kaže „validacija backend i frontend".

---

### Event Handler

**Šta je:** Funkcija koja reaguje na događaj (klik, Enter, promjena vrijednosti).

**Zašto se koristi:** Povezuje UI sa logikom.

**U Angularu:**
- `(click)="onDelete(item)"` — klik
- `(keydown)="inputKeyDown($event)"` — pritisak tipke
- `(change)="..."` — promjena vrijednosti

**WinForms analogija:** `button1_Click` — isto, samo druga sintaksa.

---

### Data Binding

**Šta je:** Automatsko povezivanje podataka između koda i ekrana.

**Zašto se koristi:** Kad se podaci promijene u kodu, ekran se ažurira.

**U Angularu:**
- **Interpolation:** `{{ item.naziv }}` — prikaži vrijednost
- **Property binding:** `[disabled]="form.invalid"` — poveži property
- **Event binding:** `(click)="save()"` — događaj
- **Two-way (forme):** `formControlName="naziv"` u Reactive Forms

**WinForms analogija:** `textBox1.DataBindings.Add(...)` — Reactive Forms rade slično, ali deklarativno u HTML-u.

---

### ComboBox → mat-select

**Šta je (WinForms):** Padajući izbor.

**U Angularu:** `<mat-select formControlName="tip">` sa `<mat-option>` elementima.

**Kada prepoznati:** Kad birate iz fiksnog skupa vrijednosti (enum Tip).

---

### DataGridView → mat-table

**Šta je (WinForms):** Tabela sa redovima i kolonama.

**U Angularu:** `<table mat-table [dataSource]="items">` sa `matColumnDef`.

**Kada prepoznati:** Kad zadatak traži „listu", „tabelu", „prikaz svih zapisa".

**Uzorak u projektu:** `fakture.component.html` ili `products.component.html`.

---

## 9. Vizuelni tok cijelog rješenja

### 9.1. Prikaz liste (Read)

```
Korisnik klikne „Dostavljači" u sidebaru
        ↓
Angular Router učitava DostavljaciComponent
        ↓
ngOnInit() → initList() → loadPagedData()
        ↓
DostavljaciApiService.list(request) → HTTP GET /Dostavljaci?page=1&pageSize=10
        ↓
Backend: DostavljaciController.List() → MediatR → ListQueryHandler
        ↓
Handler: ctx.Dostavljaci → filter → paginacija → PageResult
        ↓
JSON odgovor stiže u frontend subscribe
        ↓
handlePageResult(response) → this.items = response.items
        ↓
Angular renderuje mat-table sa podacima
        ↓
Korisnik vidi tabelu + „Stranica 1 od X · Ukupno: Y zapisa"
```

### 9.2. Pretraga

```
Korisnik ukuca tekst u search polje
        ↓
Pritisne Enter → inputKeyDown(event) → event.key === 'Enter'
        ↓
searchAction() → request.search = tekst, request.paging.page = 1
        ↓
loadPagedData() → API sa parametrom search
        ↓
Backend filtrira po nazivu (case-insensitive)
        ↓
Vraća manji broj zapisa → totalItems se smanji → paginacija se prilagodi
        ↓
Tabela prikazuje filtrirane rezultate
```

### 9.3. Brisanje

```
Korisnik klikne ikonicu kante
        ↓
deleteAction(dostavljac) se pozove
        ↓
dialogHelper.confirmDelete(dostavljac.naziv)
        ↓
Modal: „Da li ste sigurni da želite obrisati 'Express Dostava'?"
        ↓
┌─ OTKAŽI → modal se zatvara, ništa se ne dešava
└─ OBRIŠI → api.delete(id)
              ↓
              Backend: soft delete (IsDeleted = true)
              ↓
              Toast: „Uspješno obrisano"
              ↓
              loadPagedData() → tabela bez obrisanog zapisa
```

---

## 10. Najčešće greške i debug

### 10.1. Greške po kategorijama

| # | Greška | Rješenje |
|---|--------|----------|
| 1 | CORS error u browser konzoli | Provjeri da backend radi; CORS je već podešen u Program.cs |
| 2 | 401 Unauthorized | Uloguj se u aplikaciju prije testiranja admin dijela |
| 3 | Prazna tabela ali nema greške | Provjeri Network tab — da li API vraća podatke? |
| 4 | 404 na API poziv | Provjeri `environment.apiUrl` i ime kontrolera u servisu |
| 5 | Forma se ne submituje | `form.invalid` — pogledaj koja polja fale |
| 6 | Migracija ne napravi tabelu | Pogrešan connection string (pogrešna baza) |
| 7 | Dupli kod u bazi | Nisi stavila unique constraint ili provjeru u handleru |
| 8 | Pretraga ne reaguje na Enter | Nisi vezala `(keydown)` event na input |
| 9 | Paginacija pokazuje krivo | Nisi resetovala `page` na 1 pri pretrazi |
| 10 | Modal se ne pojavi | Nisi inject-ovala DialogHelperService |
| 11 | Komponenta se ne vidi | Nije u `admin-module.ts` declarations |
| 12 | Ruta ne radi | Nisi dodala rutu u `admin-routing-module.ts` |
| 13 | npm ne radi | Prebaci se u CMD (`cmd` u terminalu) |
| 14 | Backend build error | Čitaj Error List u Visual Studiju — nedostaje using ili DbSet |

### 10.2. Kako debugovati — praktično

**Frontend (F12 u browseru):**
1. **Console** — crvene greške
2. **Network** — vidi HTTP zahtjeve, status kodove, response body
3. **Elements** — provjeri da li se HTML renderuje

**Backend (Visual Studio):**
1. **Error List** (View → Error List)
2. **Output** prozor pri pokretanju
3. **Swagger** — testiraj API izolirano od frontenda
4. **Breakpoint** u handleru — F5 debug

**Baza (SSMS):**
```sql
SELECT * FROM Dostavljaci
```
Provjeri da li se podaci upisuju, brišu (soft delete: `IsDeleted = 1`).

---

## 11. Kako razmišljati na ispitu — korak po korak

### Faza 0: Priprema (5 minuta)

1. Otvori backend solution u Visual Studiju
2. Otvori frontend u VS Code
3. Podesi connection string na svoju bazu
4. Pokreni backend → provjeri Swagger
5. U CMD: `npm install` → `npm start`
6. Uloguj se u aplikaciju

### Faza 1: Pročitaj cijeli zadatak (5 minuta)

1. Podvuci **entitet** i **polja**
2. Podvuci **operacije** (CRUD, pretraga, paginacija...)
3. Podvuci **posebna pravila** (max 3 slova, jedinstven kod, enum...)
4. Podvuci **UI zahtjeve** (toast, modal, reactive forms)

**Ne piši kod još!**

### Faza 2: Pronađi uzorak (5 minuta)

1. U backendu otvori ProductCategories ili Products modul
2. Na frontendu otvori odgovarajuću komponentu
3. Razumij strukturu foldera — to je tvoj „kalup"

### Faza 3: Backend prvo (45–60 minuta)

Redoslijed:
1. Enum
2. Entitet
3. EF Configuration
4. DbContext + IAppDbContext
5. Migracija
6. CQRS: List → GetById → Create → Update → Delete
7. Controller
8. **Test u Swaggeru** — NE idi na frontend dok ovo ne radi!

### Faza 4: Frontend (45–60 minuta)

Redoslijed:
1. API model + servis
2. Lista (tabela + paginacija)
3. Pretraga
4. Add komponenta + ruta
5. Edit komponenta + ruta
6. Brisanje sa modalom
7. Toast poruke svugdje

### Faza 5: Testiranje (15 minuta)

Prođi kroz checklist u sekciji 12.

### Faza 6: Ako ne stigneš

Prioritet (od najvažnijeg):
1. Backend CRUD radi (Swagger)
2. Frontend lista + dodavanje
3. Pretraga + paginacija
4. Edit
5. Delete sa modalom
6. Toast poruke
7. Validacija
8. Dizajn/dotjerivanje

---

## 12. Checklista prije predaje

### Backend

- [ ] Entitet nasljeđuje `BaseEntity`
- [ ] Enum za Tip (Ekstern, Intern, Freelancer)
- [ ] `DbSet` u `DatabaseContext` i `IAppDbContext`
- [ ] EF konfiguracija (max dužina koda, unique kod)
- [ ] Migracija primijenjena — tabela postoji u SSMS
- [ ] List sa paginacijom i pretragom (case-insensitive)
- [ ] GetById
- [ ] Create sa validacijom i jedinstvenim kodom
- [ ] Update
- [ ] Delete (soft delete)
- [ ] Controller sa svim endpointima
- [ ] Swagger test prošao za svaku operaciju

### Frontend

- [ ] API servis sa svim metodama
- [ ] Lista sa tabelom i paginacijom
- [ ] Pretraga na Enter, prazna = svi
- [ ] Add forma — Reactive Forms, validacija, disabled dugme
- [ ] Edit forma — učitava postojeći zapis
- [ ] Delete — modal potvrde, toast
- [ ] Toast na svaku akciju (success i error)
- [ ] Rute registrovane u routing modulu
- [ ] Komponente registrovane u admin modulu

### Općenito

- [ ] Connection string na tvoju bazu
- [ ] Backend i frontend rade istovremeno
- [ ] Nema crvenih grešaka u konzoli
- [ ] Testirano sa barem 2–3 zapisa (da paginacija i pretraga imaju smisla)

---

## Završna napomena

Ovaj vodič ti daje **mapu**, ne **rješenje**. Na ispitu otvori odgovarajući postojeći modul (ProductCategories, Products), razumij šta svaki fajl radi, i **istim redoslijedom** napravi isto za Dostavljača.

Ako zapneš na konkretnom koraku (npr. „ne razumijem šta ide u Handler"), pitaj sa tačnim korakom — objasnit ću logiku, ali ne i gotov kod.

**Sretno na ispitu!**
