# RS1 — Vodič za testiranje backenda na Swaggeru

> **Cilj ovog dokumenta:** Korak po korak objašnjava kako pokrenuti Market API i testirati endpointe — posebno modul **Dostavljači** — direktno u Swagger UI-ju.  
> **Projekat:** `rs1_backend-2025-26/`  
> **Swagger URL (Development):** `http://localhost:7001/swagger`

---

## Sadržaj

1. [Šta ti treba prije pokretanja](#1-šta-ti-treba-prije-pokretanja)
2. [Pokretanje API-ja](#2-pokretanje-api-ja)
3. [Upoznavanje sa Swagger UI-jem](#3-upoznavanje-sa-swagger-ui-jem)
4. [Prijava i JWT token (obavezno za admin endpointe)](#4-prijava-i-jwt-token-obavezno-za-admin-endpointe)
5. [Testiranje Dostavljači modula — korak po korak](#5-testiranje-dostavljači-modula--korak-po-korak)
6. [Testiranje validacije (namjerno pogrešni podaci)](#6-testiranje-validacije-namjerno-pogrešni-podaci)
7. [Brzi pregled ostalih endpointa](#7-brzi-pregled-ostalih-endpointa)
8. [Očekivani HTTP status kodovi](#8-očekivani-http-status-kodovi)
9. [Najčešći problemi i rješenja](#9-najčešći-problemi-i-rješenja)
10. [Checklista za testiranje](#10-checklista-za-testiranje)

---

## 1. Šta ti treba prije pokretanja

### Obavezno

| Alat | Verzija | Zašto |
|------|---------|-------|
| **.NET SDK** | 8.0+ | Backend je na .NET 8 |
| **SQL Server** | Express ili Developer | Baza podataka |
| **Visual Studio 2022** ili **VS Code + terminal** | — | Pokretanje projekta |

### Provjera .NET SDK-a

Otvori terminal (PowerShell ili CMD) i ukucaj:

```bash
dotnet --version
```

Očekuješ nešto poput `8.0.xxx`.

### SQL Server i connection string

U fajlu `rs1_backend-2025-26/Market.API/appsettings.json` nalazi se connection string:

```json
"ConnectionStrings": {
  "Main": "Server=localhost\\SQLEXPRESS;Database=Pokusaj4;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

**Šta ovo znači:**
- `localhost\SQLEXPRESS` — SQL Server Express na tvom računaru
- `Database=Pokusaj4` — ime baze (kreira se automatski pri prvom pokretanju)
- `Trusted_Connection=True` — koristi Windows autentifikaciju

**Ako ti SQL Server nije na `SQLEXPRESS` instanci**, promijeni `Server=` dio u `appsettings.json` (npr. `Server=localhost;` ili ime tvoje instance).

> **Napomena:** Redis **nije potreban** za testiranje — keširanje je isključeno radi jednostavnosti na ispitu.

---

## 2. Pokretanje API-ja

### Opcija A — Visual Studio (preporučeno)

1. Otvori solution: `rs1_backend-2025-26/rs1_backend-2025-26.sln`
2. Postavi **Market.API** kao *Startup Project* (desni klik → *Set as Startup Project*)
3. U toolbaru odaberi profil **http**
4. Pritisni **F5** (ili zeleno dugme *Start*)

Aplikacija će se pokrenuti na `http://localhost:7001` i browser bi trebao automatski otvoriti Swagger.

### Opcija B — Terminal

```bash
cd rs1_backend-2025-26/Market.API
dotnet run
```

Zatim u browseru otvori: **http://localhost:7001/swagger**

### Šta se dešava pri prvom pokretanju?

1. **Migracije** — EF Core automatski primjenjuje migracije na bazu (`Database.MigrateAsync`)
2. **Seed podaci** — u Development okruženju ubacuju se demo korisnici, kategorije, proizvodi, narudžbe i fakture
3. U konzoli vidiš poruke poput:
   - `✅ Dynamic seed: demo users added.`
   - `✅ Dynamic seed: product categories added.`
   - itd.

Ako vidiš `Market API started successfully.` — sve je u redu.

---

## 3. Upoznavanje sa Swagger UI-jem

Swagger je web sučelje koje prikazuje sve API endpointe. Svaki endpoint možeš testirati direktno iz browsera.

### Osnovni elementi

```
┌─────────────────────────────────────────────────────────┐
│  Market API v1                              [Authorize] │  ← dugme za JWT token
├─────────────────────────────────────────────────────────┤
│  ▼ Auth                                                  │
│      POST /api/auth/login                                │
│      POST /api/auth/refresh                              │
│      POST /api/auth/logout                               │
│  ▼ Dostavljaci                                           │
│      GET    /Dostavljaci                                 │
│      POST   /Dostavljaci                                 │
│      GET    /Dostavljaci/{id}                            │
│      PUT    /Dostavljaci/{id}                            │
│      DELETE /Dostavljaci/{id}                            │
│  ▼ ProductCategories                                     │
│  ...                                                     │
└─────────────────────────────────────────────────────────┘
```

### Kako testirati jedan endpoint

1. Klikni na endpoint (npr. `GET /Dostavljaci`) da se proširi
2. Klikni **Try it out**
3. Popuni parametre (ako ih ima)
4. Klikni **Execute**
5. Ispod ćeš vidjeti **Response** — status kod i tijelo odgovora

---

## 4. Prijava i JWT token (obavezno za admin endpointe)

Većina endpointa za **pisanje** (POST, PUT, DELETE) zahtijeva **Admin** ulogu.  
Za **čitanje** Dostavljača (GET) token **nije** potreban.

### Korak 4.1 — Login

1. U Swaggeru pronađi sekciju **Auth**
2. Otvori `POST /api/auth/login`
3. Klikni **Try it out**
4. U polje **Request body** unesi:

```json
{
  "email": "admin@market.local",
  "password": "Admin123!"
}
```

5. Klikni **Execute**

### Korak 4.2 — Kopiraj access token

U **Response body** dobiješ nešto poput:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123...",
  "accessExpiresAtUtc": "2026-06-25T18:30:00Z",
  "refreshExpiresAtUtc": "2026-07-09T18:15:00Z"
}
```

Kopiraj **cijeli** sadržaj polja `accessToken` (dugačak string).

> **Važno:** Access token traje **15 minuta** (`AccessTokenMinutes` u appsettings). Poslije toga moraš ponovo login ili koristiti `POST /api/auth/refresh`.

### Korak 4.3 — Postavi token u Swagger (Authorize)

1. Klikni zeleno dugme **Authorize** (gore desno)
2. U polje **Value** unesi:

```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

> Format je: riječ `Bearer`, razmak, pa token. **Ne** stavljaj navodnike oko cijelog stringa.

3. Klikni **Authorize**, zatim **Close**

Katanac pored endpointa koji zahtijevaju autentifikaciju sada treba biti zatvoren (zaključan).

### Demo korisnici (seed podaci)

| Email | Lozinka | Uloga | Može admin endpointe? |
|-------|---------|-------|----------------------|
| `admin@market.local` | `Admin123!` | Admin | ✅ Da |
| `manager@market.local` | `Manager123!` | Manager | ❌ Ne (samo Staff) |
| `employee@market.local` | `Employee123!` | Employee | ❌ Ne |
| `string` | `string` | Employee | ❌ Ne |

Za testiranje Dostavljača (Create/Update/Delete) **uvijek koristi admin** korisnika.

---

## 5. Testiranje Dostavljači modula — korak po korak

Modul Dostavljači ima 5 endpointa. Testiraj ih **ovim redoslijedom** — svaki korak gradi na prethodnom.

### Enum `DostavljacTip` — vrijednosti za JSON

| Vrijednost | Značenje |
|------------|----------|
| `1` | Intern |
| `2` | Ekstern |
| `3` | Freelancer |

---

### Korak 5.1 — Lista dostavljača (GET, bez tokena)

**Endpoint:** `GET /Dostavljaci`

1. **Ne treba** Authorize (endpoint je `[AllowAnonymous]`)
2. Klikni **Try it out**
3. Parametri (svi opcionalni):

| Parametar | Primjer | Opis |
|-----------|---------|------|
| `Page` | `1` | Broj stranice |
| `PageSize` | `10` | Broj zapisa po stranici |
| `Search` | `dhl` | Pretraga po nazivu |

4. Klikni **Execute**

**Očekivani odgovor (200 OK):**

```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 10
}
```

Ako baza još nema dostavljača, `items` je prazan niz — to je normalno.

---

### Korak 5.2 — Kreiranje dostavljača (POST, admin token)

**Endpoint:** `POST /Dostavljaci`

1. Provjeri da si **Authorize**-ana s admin tokenom (vidi korak 4.3)
2. Klikni **Try it out**
3. U **Request body** unesi:

```json
{
  "naziv": "DHL Express",
  "kod": "DHL",
  "tip": 2,
  "aktivan": true
}
```

4. Klikni **Execute**

**Očekivani odgovor (201 Created):**

```json
{
  "id": 1
}
```

**Zapamti `id`** — koristićeš ga u sljedećim koracima.

> Ako dobiješ **409 Conflict** s porukom *"Ovaj kod vec postoji."* — kod `DHL` već postoji u bazi. Promijeni `kod` u nešto drugo (npr. `"UPS"`).

---

### Korak 5.3 — Dohvat po ID-u (GET, bez tokena)

**Endpoint:** `GET /Dostavljaci/{id}`

1. Klikni **Try it out**
2. U polje `id` unesi ID iz koraka 5.2 (npr. `1`)
3. Klikni **Execute**

**Očekivani odgovor (200 OK):**

```json
{
  "id": 1,
  "naziv": "DHL Express",
  "kod": "DHL",
  "tip": 2,
  "aktivan": true
}
```

**Test za 404:** unesi `id` koji ne postoji (npr. `9999`) → očekuješ **404 Not Found**.

---

### Korak 5.4 — Lista s pretragom (GET, bez tokena)

**Endpoint:** `GET /Dostavljaci`

1. Klikni **Try it out**
2. Postavi `Search` na `dhl` (ili dio naziva koji si unijela)
3. Klikni **Execute**

**Očekivani odgovor (200 OK):** `items` sada sadrži kreiranog dostavljača, `totalCount` je najmanje `1`.

---

### Korak 5.5 — Izmjena dostavljača (PUT, admin token)

**Endpoint:** `PUT /Dostavljaci/{id}`

1. Provjeri admin token (Authorize)
2. Klikni **Try it out**
3. U polje `id` unesi ID dostavljača (npr. `1`)
4. U **Request body** unesi:

```json
{
  "naziv": "DHL Express BiH",
  "kod": "DHL",
  "tip": 2,
  "aktivan": false
}
```

5. Klikni **Execute**

**Očekivani odgovor (204 No Content)** — prazno tijelo, samo status kod 204.

6. Provjeri izmjenu: ponovo pokreni `GET /Dostavljaci/1` i vidi da je `naziv` promijenjen i `aktivan` je `false`.

> **Poznati problem u trenutnoj verziji:** Ako PUT vrati grešku tipa *"No handler registered"* ili 500 — nedostaje `UpdateDostavljacCommandHandler`. U tom slučaju Create, Get i Delete rade, ali Update treba još implementirati.

---

### Korak 5.6 — Brisanje dostavljača (DELETE, admin token)

**Endpoint:** `DELETE /Dostavljaci/{id}`

1. Provjeri admin token (Authorize)
2. Klikni **Try it out**
3. U polje `id` unesi ID (npr. `1`)
4. Klikni **Execute**

**Očekivani odgovor (204 No Content).**

5. Provjeri brisanje: `GET /Dostavljaci/1` → **404 Not Found**

> **Napomena:** Brisanje je **soft delete** — zapis se ne briše fizički iz baze, već se postavlja `IsDeleted = true`. Zato ga GET više ne vraća.

---

### Korak 5.7 — Test bez admin tokena (401 Unauthorized)

1. Klikni **Authorize** → **Logout** (ili obriši token iz polja)
2. Pokušaj `POST /Dostavljaci` s bilo kojim body-jem
3. Očekuješ **401 Unauthorized**

Ovo potvrđuje da zaštita `[Authorize(Policy = "AdminOnly")]` radi.

---

## 6. Testiranje validacije (namjerno pogrešni podaci)

Swagger je odličan za testiranje validacije — šalješ loše podatke i gledaš poruke grešaka.

### Test 6.1 — Prazan naziv

`POST /Dostavljaci` (s admin tokenom):

```json
{
  "naziv": "",
  "kod": "ABC",
  "tip": 1,
  "aktivan": true
}
```

**Očekivano:** **400 Bad Request** s porukom o obaveznom polju naziv.

### Test 6.2 — Kod predugačak (max 3 karaktera)

```json
{
  "naziv": "Test Dostavljac",
  "kod": "ABCD",
  "tip": 1,
  "aktivan": true
}
```

**Očekivano:** **400 Bad Request** — kod može imati maksimalno 3 karaktera.

### Test 6.3 — Nevažeći tip

```json
{
  "naziv": "Test",
  "kod": "TST",
  "tip": 99,
  "aktivan": true
}
```

**Očekivano:** **400 Bad Request** — *"Ovaj tip nije validan."*

### Test 6.4 — Duplikat koda

1. Kreiraj dostavljača s kodom `"DHL"`
2. Pokušaj kreirati drugog s istim kodom `"DHL"`

**Očekivano:** **409 Conflict** — *"Ovaj kod vec postoji."*

---

## 7. Brzi pregled ostalih endpointa

Ovi endpointi su dio istog API-ja i također se testiraju na Swaggeru.

### Auth

| Endpoint | Token | Opis |
|----------|-------|------|
| `POST /api/auth/login` | Ne | Prijava |
| `POST /api/auth/refresh` | Ne | Obnova access tokena |
| `POST /api/auth/logout` | Ne | Odjava (invalidira refresh token) |

### Katalog (većina GET je javna)

| Endpoint | Admin? | Opis |
|----------|--------|------|
| `GET /Catalog/home` | Ne | Početna stranica kataloga |
| `GET /ProductCategories` | Ne | Lista kategorija |
| `POST /ProductCategories` | Da | Nova kategorija |
| `GET /Products` | Ne | Lista proizvoda |
| `POST /Products` | Da | Novi proizvod |

### Narudžbe i fakture

| Endpoint | Admin? | Opis |
|----------|--------|------|
| `GET /Orders` | Da | Lista narudžbi |
| `GET /Fakture` | Ne | Lista faktura (paginacija) |

---

## 8. Očekivani HTTP status kodovi

| Kod | Značenje | Kada ga vidiš |
|-----|----------|---------------|
| **200** | OK | Uspješan GET |
| **201** | Created | Uspješan POST (kreiran zapis) |
| **204** | No Content | Uspješan PUT ili DELETE (nema tijela odgovora) |
| **400** | Bad Request | Validacija nije prošla |
| **401** | Unauthorized | Nema tokena ili je token nevažeći |
| **403** | Forbidden | Imaš token, ali nemaš Admin ulogu |
| **404** | Not Found | Zapis s tim ID-em ne postoji |
| **409** | Conflict | Duplikat koda ili poslovno pravilo |

---

## 9. Najčešći problemi i rješenja

### Problem: Swagger se ne otvara

- Provjeri da API radi (konzola pokazuje `Market API started successfully.`)
- Otvori ručno: `http://localhost:7001/swagger`
- Swagger radi **samo u Development** okruženju

### Problem: Greška pri spajanju na bazu

```
A network-related or instance-specific error occurred...
```

**Rješenje:**
1. Provjeri da SQL Server radi (Services → *SQL Server (SQLEXPRESS)* → Running)
2. Provjeri connection string u `appsettings.json`
3. U SQL Server Management Studio (SSMS) pokušaj ručno spojiti se na isti server

### Problem: 401 na POST /Dostavljaci iako si se ulogovala

**Rješenje:**
1. Ponovo uradi login i kopiraj **novi** `accessToken`
2. U Authorize polje unesi: `Bearer <token>` (s riječju Bearer!)
3. Provjeri da koristiš **admin@market.local**, ne manager/employee

### Problem: 403 Forbidden

Ulogovana si, ali korisnik **nije Admin**. Koristi `admin@market.local` / `Admin123!`.

### Problem: Token istekao

Access token traje 15 minuta. Ponovi login ili koristi:

`POST /api/auth/refresh` s body:

```json
{
  "refreshToken": "<tvoj_refresh_token_iz_logina>"
}
```

### Problem: PUT /Dostavljaci vraća 500

Vjerovatno nedostaje `UpdateDostavljacCommandHandler` u Application sloju. Ostali endpointi rade — fokusiraj se na Create, Get, List i Delete dok se Update ne popravi.

### Problem: Port 7001 je zauzet

U `launchSettings.json` promijeni `applicationUrl` ili zaustavi drugu aplikaciju koja koristi isti port.

---

## 10. Checklista za testiranje

Prije predaje ili demonstracije, prođi kroz ovu listu:

- [ ] API se pokreće bez greške u konzoli
- [ ] Swagger se otvara na `http://localhost:7001/swagger`
- [ ] `POST /api/auth/login` s admin korisnikom vraća `accessToken`
- [ ] Authorize u Swaggeru postavljen s `Bearer <token>`
- [ ] `GET /Dostavljaci` vraća 200 (prazna ili puna lista)
- [ ] `POST /Dostavljaci` kreira novog dostavljača (201 + id)
- [ ] `GET /Dostavljaci/{id}` vraća kreiranog dostavljača
- [ ] `GET /Dostavljaci?Search=...` filtrira po nazivu
- [ ] `PUT /Dostavljaci/{id}` mijenja zapis (204) — ili si svjesna da handler još nedostaje
- [ ] `DELETE /Dostavljaci/{id}` briše zapis (204)
- [ ] `GET /Dostavljaci/{id}` nakon brisanja vraća 404
- [ ] Validacija vraća 400 za prazan naziv / predugačak kod
- [ ] Duplikat koda vraća 409
- [ ] POST bez tokena vraća 401

---

## Kratki „copy-paste" scenarij za brzi test

```
1. Pokreni API (F5)
2. Swagger → POST /api/auth/login
   Body: { "email": "admin@market.local", "password": "Admin123!" }
3. Kopiraj accessToken
4. Authorize → Bearer <token>
5. POST /Dostavljaci
   Body: { "naziv": "FedEx", "kod": "FDX", "tip": 2, "aktivan": true }
6. GET /Dostavljaci/1
7. GET /Dostavljaci?Search=fed
8. PUT /Dostavljaci/1
   Body: { "naziv": "FedEx BiH", "kod": "FDX", "tip": 2, "aktivan": true }
9. DELETE /Dostavljaci/1
10. GET /Dostavljaci/1 → 404
```

---

*Dokument kreiran za repozitorij [RS1-januar](https://github.com/lana-mustafic/RS1-januar).*
