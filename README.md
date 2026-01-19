# 📦 Projekt BDwAI – Aplikacja do zarządzania zamówieniami i magazynem

---

## 📝 Opis projektu
Aplikacja webowa wykonana w technologii **ASP.NET Core MVC**, umożliwiająca zarządzanie
produktami, zamówieniami oraz stanem magazynowym.  
Projekt został zaprojektowany zgodnie z wzorcem **MVC (Model–View–Controller)** i spełnia
wymagania specyfikacji projektowej przedmiotu BDwAI.

---

## 🛠️ Technologie
- ASP.NET Core MVC
- Entity Framework Core
- ASP.NET Core Identity
- SQLite
- C#
- Razor Views (HTML)
- Visual Studio 2022

---

## 📋 Wymagania systemowe
- Visual Studio 2022
- .NET SDK 7.0 lub nowszy
- SQLite
- System operacyjny Windows / Linux / macOS

---

## 🚀 Instalacja i uruchomienie projektu

### 1️⃣ Otwarcie projektu
Projekt należy otworzyć w **Visual Studio 2022**:

File → Open → Project/Solution

### 2️⃣ Konfiguracja bazy danych

Aplikacja wykorzystuje bazę danych SQLite.

Łańcuch połączenia znajduje się w pliku appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Data Source=app.db"
}

Migracje należy wykonać w Konsoli Menedżera Pakietów wpisując polecenie:

Update-Database

Polecenie utworzy bazę danych oraz wszystkie wymagane tabele.

---

## 🧱 Architektura projektu (MVC)

### 📁 Models
- **Product** – produkt magazynowy
- **Order** – zamówienie użytkownika
- **OrderItem** – pozycja zamówienia
- **IdentityUser** – użytkownik systemu

**Relacje:**
- Order → OrderItems (1:N)
- OrderItem → Product (N:1)

---

### 📁 Controllers

**Kontrolery MVC:**
- **HomeController** – strony główne
- **ProductsController** – CRUD produktów (widoki HTML)
- **OrdersController** – obsługa zamówień

**Kontroler API:**
- **ProductsApiController** – REST API CRUD dla encji Product

---

### 📁 Views
Aplikacja zawiera widoki umożliwiające:
- przeglądanie produktów
- dodawanie, edycję i usuwanie produktów
- przeglądanie zamówień
- podgląd szczegółów zamówień

Każdy formularz posiada **walidację danych**.

---

## 🔐 Autoryzacja i role

Aplikacja wykorzystuje **ASP.NET Core Identity**.

### Role użytkowników:
- **Admin**
  - zarządzanie produktami
  - zarządzanie zamówieniami (anulowanie, realizacja)
  - Dane logowania do admina login: admin@local.test hasło: Admin123!

- **Użytkownik**
  - przeglądanie produktów
  - składanie zamówień
  - podgląd własnych zamówień

Dostęp do wybranych funkcjonalności jest ograniczony atrybutem `[Authorize]`.

## 📦 Funkcjonalności aplikacji

### Produkty
- dodawanie produktów
- edycja produktów
- usuwanie produktów
- kontrola stanów magazynowych

### Zamówienia
- składanie zamówień
- automatyczne odejmowanie produktów z magazynu
- filtrowanie zamówień po statusie
- anulowanie zamówień (zwracanie produktów do magazynu)
- oznaczanie zamówień jako zrealizowane

---

## 🔌 API CRUD (wymaganie projektowe)

Aplikacja udostępnia **REST API CRUD** dla głównej encji **Product**.

### Endpointy API:

| Operacja | Metoda HTTP | Endpoint |
|--------|------------|----------|
| Pobranie wszystkich produktów | GET | `/api/products` |
| Pobranie produktu po ID | GET | `/api/products/{id}` |
| Dodanie produktu | POST | `/api/products` |
| Aktualizacja produktu | PUT | `/api/products/{id}` |
| Usunięcie produktu | DELETE | `/api/products/{id}` |

- Endpointy **POST / PUT / DELETE** dostępne są wyłącznie dla roli **Admin**
- API zwraca dane w formacie **JSON**

### Przykładowa odpowiedź API

```json
{
  "id": 1,
  "name": "Laptop Dell",
  "sku": "LAP-DELL-001",
  "price": 4500,
  "quantityInStock": 10
}

