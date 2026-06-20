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

#### Korak 7: CQRS — GetById (Query)

- **Uzorak:** `ProductCategories/Queries/GetById/`
- **Handler:** pronađi entitet po Id, ako ne postoji → baci `MarketNotFoundException`
- **Vrati DTO** sa svim poljima potrebnim za edit formu

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

#### Korak 9: CQRS — Update (Command)

- **Uzorak:** `ProductCategories/Commands/Update/`
- **Handler:** pronađi po Id, ažuriraj polja, provjeri jedinstvenost Koda (ali dozvoli isti kod za isti zapis)
- **Controller:** Id iz URL-a prepisuje Id u command objektu

#### Korak 10: CQRS — Delete (Command)

- **Uzorak:** `DeleteProductCategoryCommandHandler.cs`
- **Handler:** pronađi entitet, pozovi `Remove()`, `SaveChangesAsync`
- **Napomena:** Projekt koristi **soft delete** — `Remove()` u DbContext-u zapravo postavlja `IsDeleted = true` (vidi `DatabaseConfiguration.cs`)

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

#### Korak 3: Komponenta liste — HTML

- **Otvori:** `dostavljaci.component.html` — dizajn već postoji!
- **Dopuni:**
  - `(keydown)` na search input
  - `mat-table` sa kolonama: Naziv, Kod, Tip, Aktivan, Akcije
  - Dugmad za edit i delete u koloni Akcije
  - `<app-fit-paginator-bar [vm]="this" />` na dnu
  - `*ngFor` ili `[dataSource]` za prikaz podataka
- **Tip prikaži** kao badge (boje su u CSS-u templatea)

#### Korak 4: Generisanje Add komponente

- **U CMD terminalu** (u folderu frontend projekta):
  ```
  npx ng g c modules/admin/catalogs/dostavljaci/dostavljac-add --skip-tests
  ```
- **Registruj** komponentu u `admin-module.ts`
- **Dodaj rutu** u `admin-routing-module.ts`: `dostavljaci/add`

#### Korak 5: Generisanje Edit komponente

- Isto kao Add, ali ruta: `dostavljaci/edit/:id`
- Edit komponenta u `ngOnInit` učitava podatke po Id-u

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
