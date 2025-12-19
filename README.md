
# Dead Pigeons 🐦

## Overview

Dead Pigeons is a web-based system developed for **Jerne IF** to manage the traditional *Dead Pigeons* lottery game digitally. The system tracks digital players and boards while allowing physical players to participate outside the app.

The solution consists of:

* **.NET Web API** backend
* **React + TypeScript** frontend

### Workflow / Business Logic Diagram
This diagram illustrates the overall workflow and business logic of the system:
- Player registration and activation by admin
- Balance deposits and admin approval
- Board purchasing with balance validation
- Weekly game lifecycle
- Entering winning numbers and closing games

![Workflow Diagram](docs/workflow-diagram.jpg)

---

## Tech Stack

**Backend**

* .NET 9 Web API
* Entity Framework Core + PostgreSQL (Neon)
* JWT Authentication
* Swagger / OpenAPI
* XUnit + XUnit.DependencyInjection
* TestContainers

**Frontend**

* React
* TypeScript
* React Router
* Vite

**DevOps**

* Docker & Docker Compose
* GitHub Actions (CI)
* Fly.io (deployment)

---

## Security & Authorization

### Roles

* **Admin**

  * Full CRUD on players
  * Approve balance deposits
  * Manage games and enter winning numbers
  * View all data

* **Player**

  * View own data
  * Deposit money (pending admin approval)
  * Purchase boards (only if active and with sufficient balance)

### Policies

* Only admins can manage players, games, and approvals
* Only active players can buy boards
* Players can only access their own data
* Passwords are hashed and stored securely

---

## Balance & Transactions

* No balance column is stored
* Balance is calculated as:

```
Approved deposits – board purchases
```

* Balance cannot be negative
* Deposits require admin approval
* MobilePay transaction numbers are stored
* Winnings are handled manually (not added to balance)

---

## Game Rules (Summary)

* Weekly games
* Numbers range: **1–16**
* Boards contain **5–8 numbers**
* Price depends on number count
* Number order does not matter when checking wins
* Joining closes **Saturday 17:00 (DK time)**
* New game starts when winning numbers are entered

---

## Environment & Configuration

* All secrets are stored as **environment variables**
* No secrets are committed to Git

Examples:

* `ConnectionStrings__Default`
* `JWT__Secret`
* `VITE_API_BASE_URL`

---

## Testing

* Unit and integration tests with **XUnit**
* All service methods tested (happy & unhappy paths)
* **TestContainers** used for isolated database testing
* Tests run automatically via GitHub Actions

---
## Database Context

The database is designed to model the Dead Pigeons domain and support weekly games, player participation, and full financial traceability.

All entities use **GUIDs** as primary keys and follow a **soft-delete** strategy to preserve historical data.

Core entities include:
- **Player** – stores player information and active/inactive status.
- **Game** – represents a weekly game and its lifecycle.
- **Board** – represents a board played by a player in a specific game, containing 5–8 selected numbers.
- **Transaction** – represents financial actions such as deposits and board purchases.

The system does **not store a balance field**.  
A player’s balance is calculated as:

Sum of approved deposit transactions minus the total cost of purchased boards.

![Database Diagram](docs/database-diagram.jpg)


---

## Current Status

### Implemented

* Authentication & authorization
* Player management
* Balance deposit & approval flow
* Board purchasing
* Game lifecycle
* Swagger documentation
* CI & cloud deployment

### Known Limitations

* UI/UX can be improved
* Some admin overviews are basic

---

## Development

```bash
# Backend
cd server
dotnet run

# Frontend
cd client
npm install
npm run dev
```

---

###Test Credentials

##Admin
*Email: admin@example.com
*Password: Admin123

##Player
*Email: janejackson@example.com
*Password: JaneJackson123

##For development and demo purposes only.

---

## Possible Improvements

- Advanced filtering and searching for admins (players, games, transactions)
- Better UI/UX structure and visual feedback
- Improved frontend component structure and reuse
- More detailed validation and error handling
- Extended logging and monitoring for production usage
- Role-based UI separation for admin and player views

---

## Notes

The system follows course recommendations:

* Stateless API
* Soft-delete strategy
* Transaction-based balance calculation
