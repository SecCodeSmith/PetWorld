# PetWorld 🐾

Sklep zoologiczny z **doradcą AI** (architektura Writer–Critic) zbudowany na **.NET 10**,
**Blazor Server**, **MySQL** i **Microsoft Agent Framework 1.0**. Aplikacja jest napisana
zgodnie z architekturą **Onion / Clean** ze ścisłą regułą zależności „do wewnątrz".

---

## Wymagania wstępne

- **Docker** i **Docker Compose** (jedyne, czego potrzeba, aby uruchomić całość).
- **Klucz OpenAI API** (`OPENAI_API_KEY`) — używany przez doradcę.
- (Opcjonalnie, do uruchomienia lokalnego bez Dockera: .NET 10 SDK i serwer MySQL.)

---

## Szybki start (Docker)

```bash
# 1. Skonfiguruj zmienne środowiskowe
cp .env.example .env
#    następnie uzupełnij w .env: OPENAI_API_KEY oraz hasła MySQL

# 2. Uruchom całość
docker compose up --build

# 3. Otwórz aplikację
#    http://localhost:5000
```

Przy starcie aplikacja **automatycznie**:

- czeka aż MySQL będzie gotowy (pętla ponawiania — brak „crash-loopu" przy pierwszym uruchomieniu),
- stosuje migracje EF Core,
- zasiewa **10 produktów** katalogu oraz **użytkownika demo** (jeśli ustawiono `SEED_USER_*`).

> Zatrzymanie i pełne wyczyszczenie danych: `docker compose down -v`.

---

## Konfiguracja (`.env`)

| Zmienna | Opis | Domyślnie |
| --- | --- | --- |
| `OPENAI_API_KEY` | Klucz OpenAI dla doradcy (przy lokalnym serwerze dowolny placeholder) | — (wymagane) |
| `AI__MODEL` | Model czatu (mapuje na `Ai:Model`) | `gpt-4o-mini` |
| `AI__BASEURL` | Opcjonalny endpoint zgodny z OpenAI (lokalny LLM) — z sufiksem `/v1` | — (puste = OpenAI) |
| `MYSQL_ROOT_PASSWORD` | Hasło root MySQL | — |
| `MYSQL_DATABASE` | Nazwa bazy | `petworld` |
| `MYSQL_USER` / `MYSQL_PASSWORD` | Konto aplikacyjne MySQL | — |
| `SEED_USER_EMAIL` / `SEED_USER_PASSWORD` | Konto demo zakładane przy starcie | — |

Model AI jest konfigurowany **wyłącznie** w jednym miejscu (`Ai:Model` / `AI__MODEL`); nigdzie
indziej nie jest „zaszyty" na sztywno (jedyna wartość domyślna `gpt-4o-mini` znajduje się w
warstwie kompozycji DI).

---

## Testowanie z lokalnym modelem LLM (zgodnym z OpenAI)

Doradcę można skierować na **lokalny serwer LLM**, który wystawia API zgodne z OpenAI — bez
żadnej zmiany w kodzie, wyłącznie konfiguracją. Wystarczy ustawić `AI__BASEURL` (z sufiksem `/v1`).
Gdy `AI__BASEURL` jest ustawione, `OPENAI_API_KEY` może być dowolnym placeholderem.

| Serwer | Uruchomienie | `AI__BASEURL` (lokalnie) | `AI__BASEURL` (z Dockera) |
| --- | --- | --- | --- |
| **LM Studio** | start serwera w aplikacji | `http://localhost:1234/v1` | `http://host.docker.internal:1234/v1` |
| **Ollama** | `ollama serve` | `http://localhost:11434/v1` | `http://host.docker.internal:11434/v1` |
| **llama.cpp** | `llama-server` | `http://localhost:8080/v1` | `http://host.docker.internal:8080/v1` |
| **vLLM** | `vllm serve <model>` | `http://localhost:8000/v1` | `http://host.docker.internal:8000/v1` |

Przykład w `.env` (LM Studio na hoście, aplikacja w Dockerze):

```dotenv
AI__BASEURL=http://host.docker.internal:1234/v1
AI__MODEL=local-model-name
OPENAI_API_KEY=local
```

`docker-compose.yml` dodaje `extra_hosts: host.docker.internal:host-gateway`, więc kontener
widzi serwer LLM działający na hoście również na Linuksie.

### Gotowy stack z lokalnym LLM (Ollama) — jedna komenda

Nakładka `docker-compose.ollama.yml` dokłada serwer **Ollama** (API zgodne z OpenAI),
automatycznie pobiera model i kieruje aplikację na niego — **bez klucza OpenAI**:

```bash
docker compose -f docker-compose.yml -f docker-compose.ollama.yml up --build
# aplikacja: http://localhost:5000   |   Ollama API: http://localhost:11434/v1
```

- Pierwszy start pobiera model do wolumenu `ollama-data` (jednorazowo).
- Domyślny model: `qwen2.5:3b`. Zmień przez `AI__MODEL` w `.env`, np. `AI__MODEL=qwen2.5:7b`
  lub `AI__MODEL=llama3.1:8b` — większe modele instrukcyjne dają pewniejszy structured output.
- Używa **GPU NVIDIA** (wymaga zainstalowanego *NVIDIA Container Toolkit* na hoście). Aby
  uruchomić na CPU, usuń blok `deploy:` z usługi `ollama` w nakładce.

**Wydajność:** na GPU pojedyncza porada (2–6 wywołań modelu) zwykle zajmuje kilka–kilkanaście
sekund; na CPU może to być kilka minut — to normalne dla lokalnego LLM.

> Uwaga: pętla Writer–Critic wykonuje 2–6 wywołań modelu i wymaga **structured output**
> (JSON wg schematu dla Critica). Wybierz lokalny model, który dobrze radzi sobie z trybem
> JSON/grammar (np. większe modele instrukcyjne), inaczej Critic może częściej nie akceptować.

---

## Architektura

```text
                 ┌───────────────────────────┐
                 │        PetWorld.Web        │  Blazor Server (interactive server)
                 │  .razor, Program.cs (DI)   │
                 └─────────────┬──────────────┘
            referuje Application │ (Infrastructure tylko w Program.cs)
                 ┌─────────────▼──────────────┐
                 │    PetWorld.Application     │  interfejsy (abstrakcje) + DTO + use-case'y
                 └─────────────┬──────────────┘
                               │ referuje Domain
                 ┌─────────────▼──────────────┐
                 │      PetWorld.Domain        │  encje: Product, ChatInteraction (BEZ zależności)
                 └─────────────▲──────────────┘
                               │ implementuje abstrakcje
                 ┌─────────────┴──────────────┐
                 │    PetWorld.Infrastructure  │  EF Core + Pomelo MySQL, Identity, Agent Framework, koszyk
                 └────────────────────────────┘
```

**Reguła zależności (do wewnątrz):** `Web → Application → Domain` oraz
`Infrastructure → Application → Domain`. Warstwa wewnętrzna nigdy nie zna zewnętrznej.

**Gdzie żyją frameworki (kluczowe dla architektury):** pakiety **Microsoft Agent Framework**,
**EF Core / Pomelo MySQL** oraz **ASP.NET Core Identity** występują **wyłącznie** w
`PetWorld.Infrastructure.csproj`. `Application` wystawia jedynie interfejsy
(`IProductAdvisorService`, `IProductRepository`, `IChatHistoryRepository`, `ICartService`,
`IAuthService`) i DTO; ich implementacje są w `Infrastructure`. `Web` korzysta z `Infrastructure`
tylko w `Program.cs` (composition root) — żaden komponent `.razor` nie dotyka typu z Infrastructure.

### Wersje i decyzje technologiczne

- **.NET 10 (LTS)** — bieżąca wersja LTS (.NET 9 to STS, już poza wsparciem).
- **Microsoft.Agents.AI / .OpenAI 1.11.1** — stabilna linia GA 1.x (bez pakietów `--prerelease`).
- **EF Core 9 + Pomelo 9.0.0** — najnowszy stabilny Pomelo wspiera EF Core 9, więc cały stos
  EF/Identity przypięto do `9.0.x`. Działa bez zarzutu na środowisku uruchomieniowym .NET 10
  (kompatybilność wstecz).
- **Identity w jednym `PetWorldDbContext : IdentityDbContext<ApplicationUser>`** — jedno
  połączenie, jedna historia migracji, jeden magazyn transakcyjny. `ApplicationUser` znajduje się
  w `Infrastructure`, więc żaden typ Identity nie przecieka do Domain/Application.

---

## Doradca Writer–Critic

Dwa agenty Microsoft Agent Framework (`AIAgent`) działają na **jednym** kliencie OpenAI i są
sterowane **ręcznie napisaną pętlą** w `ProductAdvisorService` (Infrastructure) — celowo **nie**
silnikiem workflow, aby przepływ był w pełni wyjaśnialny:

1. **Writer** pisze odpowiedź po polsku i poleca produkty **wyłącznie z katalogu** (dokładne nazwy
   i ceny). Katalog jest wstrzyknięty do jego instrukcji.
2. **Critic** zwraca **strukturalny** werdykt `CriticVerdict { Approved, Feedback }`
   (przez `RunAsync<CriticVerdict>` → typowany `.Result`): sprawdza trafność, istnienie produktów
   w katalogu, poprawność cen i ton.
3. **Pętla:** `iter = 1`, Writer → A; Critic(A); jeśli `Approved` → zwróć `(A, iter, true)`;
   w przeciwnym razie, jeśli `iter == 3` → zwróć `(A, 3, false)`; w przeciwnym razie `iter++`,
   Writer poprawia odpowiedź na podstawie feedbacku i pętla się powtarza (maks. 3 iteracje).

Każda interakcja jest zapisywana w tabeli `chat_interactions` i widoczna na `/historia`
(najnowsze na górze). Polecane produkty dla kart na czacie są dopasowywane po nazwie do katalogu.

> Pętla potrafi trwać **30–60 s**. Czat obsługuje to asynchronicznie i pokazuje stan ładowania
> (spinner na przycisku + „skeleton"), dzięki czemu obwód SignalR nie wygląda na zawieszony.

Jeden **wspólny katalog** (`Infrastructure/Persistence/Catalogue.cs`) jest źródłem prawdy zarówno
dla zasiewu tabeli `products`, jak i dla instrukcji Writera — ceny nie mogą się więc rozjechać
między sklepem a AI.

---

## Uwierzytelnianie — głębokość: **Real**

Zaimplementowano wariant **Real**: **ASP.NET Core Identity** na MySQL z **własnymi, minimalnymi**
komponentami Blazor logowania i rejestracji (bez scaffoldingu Identity UI).

- Strony `/logowanie` i `/rejestracja` renderują się jako **statyczny SSR** — formularz wykonuje
  realny POST, więc `SignInManager`/`UserManager` mogą ustawić ciasteczko uwierzytelnienia.
- Pozostałe strony są **interaktywne** (`InteractiveServer`).
- `/profil` wymaga zalogowania — anonimowy użytkownik jest przekierowywany na `/logowanie`.
- Wylogowanie to realny POST na `/account/logout`, przeprowadzony przez `IAuthService`
  (żaden typ Identity nie pojawia się w warstwie Web).

---

## Strony

| Ścieżka | Strona |
| --- | --- |
| `/` | Czat z doradcą (empty / loading / answered) |
| `/sklep` | Katalog: filtr kategorii, wyszukiwarka, sortowanie |
| `/produkt/{id}` | Szczegóły produktu + stepper ilości |
| `/koszyk` | Koszyk + podsumowanie + zaślepka kasy |
| `/historia` | QuickGrid interakcji (Data · Pytanie · Odpowiedź · Liczba iteracji) |
| `/logowanie`, `/rejestracja` | Uwierzytelnianie |
| `/profil` | Konto (wymaga zalogowania) |

---

## Frontend (Tailwind CSS, self-hosted)

UI korzysta z **Tailwind CSS** budowanego lokalnie do statycznego, zminifikowanego pliku
`wwwroot/app.css` — **bez CDN w czasie działania**. Tokeny z `design-reference/DESIGN.md`
(kolory, odstępy `md`/`gutter`, `fontSize` `headline-md`, promienie, cienie) są odwzorowane
w `tailwind.config.js`, więc klasy typu `bg-surface`, `p-md`, `text-headline-md`,
`rounded-card` mapują się 1:1. Czcionka Plus Jakarta Sans jest hostowana lokalnie.

```bash
cd src/PetWorld.Web
npm install
npm run build:css     # -> wwwroot/app.css (purged + minified)
npm run watch:css     # tryb obserwacji podczas pracy nad UI
```

- Źródło: `Styles/app.css` (dyrektywy `@tailwind`, `@font-face`, atomy designu przez `@apply`).
- Konfiguracja: `tailwind.config.js` (tokeny + ścieżki content do plików `.razor`).
- Wynik: `wwwroot/app.css` (commitowany, by `dotnet run` działał bez Node; w Dockerze
  generowany na nowo w osobnym etapie `node` — patrz `Dockerfile`).

## Ograniczenia (świadome uproszczenia)

- **Koszyk** jest **w pamięci, per obwód Blazor** (rejestracja `Scoped`). Żyje tak długo jak
  połączenie użytkownika; nie jest współdzielony między kartami przeglądarki ani zapisywany w bazie.
- **Kasa** to **zaślepka** — brak płatności. „Przejdź do kasy" czyści koszyk i pokazuje ekran
  potwierdzenia.
- **Obrazy produktów** — katalog nie ma zdjęć; karty używają placeholdera (ikona łapki). Brak
  pipeline'u uploadu obrazów.

---

## Rozwój lokalny bez Dockera (opcjonalnie)

Wymaga **.NET 10 SDK** i działającego **MySQL**.

```bash
# zmienne
export OPENAI_API_KEY=sk-...
# domyślny connection string celuje w localhost (User=root, Password=root) — patrz appsettings.json

# styl (Tailwind) — wygeneruj wwwroot/app.css, jeśli edytujesz UI
cd src/PetWorld.Web && npm install && npm run build:css && cd ../..

# migracje (narzędzie EF Core 9)
dotnet tool install --global dotnet-ef --version 9.0.0
dotnet ef database update --project src/PetWorld.Infrastructure --startup-project src/PetWorld.Web

# uruchomienie
dotnet run --project src/PetWorld.Web   # http://localhost:5000
```

(W Dockerze migracje stosują się same przy starcie — powyższe kroki są tylko dla trybu lokalnego.)

---

## Bezpieczeństwo / sekrety

Repozytorium jest publiczne i **nie zawiera żadnych sekretów**. `OPENAI_API_KEY` oraz hasła są
przekazywane **wyłącznie** przez `.env` (w `.gitignore`). Plik `.env.example` zawiera jedynie
oczywiste placeholdery. `appsettings.Development.json`, `.env`, `bin/`, `obj/` są ignorowane.
