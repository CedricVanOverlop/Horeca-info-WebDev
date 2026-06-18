# Horeca-info.com — Application web

Application web complémentaire à Odoo pour **Horeca-info.com SRL** (Lobbes, Wallonie).
Trois commerces sur un même site : **Friterie.net**, **Baraque à Glaces**, **Centre Padel**.

Le projet ne duplique aucune donnée gérée par Odoo (stocks, factures, catalogue).

---

## Stack technique

| Couche | Technologie | Version |
|---|---|---|
| Backend | ASP.NET Core C# Minimal API | .NET 10 |
| Base de données | MySQL — SQL brut via **Dapper** (pas d'Entity Framework) | MySQL 8.0 |
| Frontend | Angular (standalone, zoneless, signals) | Angular 21 |
| Auth | JWT Bearer + BCrypt | — |

Architecture backend : **Clean Architecture** en 3 projets — `Api` → `Core` (logique métier, interfaces) → `Infrastructure` (Dapper, gateways).

---

## Prérequis

| Outil | Version minimale |
|---|---|
| .NET SDK | **10.0** |
| Node.js | **20 LTS** ou supérieur |
| Angular CLI | **21** (`npm i -g @angular/cli`) |
| MySQL Server | **8.0** |

Vérifier : `dotnet --version`, `node --version`, `ng version`, `mysql --version`.

---

## Installation

### 1. Cloner le dépôt

```bash
git clone https://github.com/CedricVanOverlop/Horeca-info-WebDev.git
cd Horeca-info-WebDev
```

### 2. Base de données

Créer la base et le schéma à partir du script fourni :

```bash
mysql -u root -p < backend/Infrastructure/horeca_db.sql
```

Le script crée la base **`horeca_info`**, toutes les tables, les contraintes et les
données de référence (table `COMMERCE`).

### 3. Secrets (jamais versionnés)

Les secrets ne sont **pas** dans `appsettings.json`. Deux options au choix.

**Option A — fichier de dev (recommandé en local)**
Créer `backend/Api/appsettings.Development.json` (ignoré par Git) :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=horeca_info;Uid=root;Pwd=root;"
  },
  "Jwt": {
    "Key": "REMPLACER_PAR_UNE_CLE_SECRETE_DE_32_CARACTERES_MINIMUM"
  }
}
```

**Option B — variables d'environnement (prod)**

```bash
# Windows PowerShell
$env:DB_CONNECTION = "Server=localhost;Database=horeca_info;Uid=root;Pwd=root;"
$env:JWT_KEY       = "REMPLACER_PAR_UNE_CLE_SECRETE_DE_32_CARACTERES_MINIMUM"
```

Les variables d'environnement `DB_CONNECTION` et `JWT_KEY` sont prioritaires sur
`appsettings.Development.json`.

### 4. Dépendances frontend

```bash
cd frontend
npm install
```

---

## Lancer le projet (2 terminaux)

### Backend

```bash
cd backend/Api
dotnet run --launch-profile https
```

API disponible sur **https://localhost:7160** (Swagger en dev : `https://localhost:7160/swagger`).

> Le frontend de dev appelle `https://localhost:7160`. Lancer avec le profil `https`.

### Frontend

```bash
cd frontend
npm start
```

Application sur **http://localhost:4200**.

---

## Comptes de test

Le script SQL ne contient **pas** de comptes utilisateurs (mots de passe hashés BCrypt).
Créer les comptes ainsi :

1. **Client** — via la page d'inscription `http://localhost:4200/register`.
2. **Promouvoir un compte en staff** (Employé / Cuisine / Administrateur) — après
   inscription, exécuter en SQL (en adaptant l'email) :

```sql
-- Retrouver l'id de l'utilisateur inscrit
SELECT id_utilisateur, email FROM UTILISATEUR WHERE email = 'mon.email@test.be';

-- Lui attribuer un rôle staff (acces ∈ 'Employe' | 'Cuisine' | 'Administrateur')
INSERT INTO EMPLOYE (id_utilisateur, acces, actif, id_commerce_preference)
VALUES (<id_utilisateur>, 'Administrateur', TRUE, 3);
```

`id_commerce_preference = 3` correspond au Centre Padel (voir table `COMMERCE`).

---

## Rôles & accès

| Rôle | Résolution |
|---|---|
| **Client** | aucune ligne `EMPLOYE` active |
| **Employe** | `EMPLOYE.acces = 'Employe'` et `actif = TRUE` |
| **Cuisine** | `EMPLOYE.acces = 'Cuisine'` et `actif = TRUE` |
| **Administrateur** | `EMPLOYE.acces = 'Administrateur'` et `actif = TRUE` |

Sécurité serveur via policies ASP.NET Core (`AdminOnly`, `CuisineOrAdmin`,
`PersonnelOnly`) ; guards Angular complémentaires côté client.

---

## Structure

### Backend — Clean Architecture (`Api` → `Core` ← `Infrastructure`)

```
backend/
├── Api/                              # Couche présentation (dépend de Core + Infrastructure)
│   ├── Program.cs                    # Composition root : DI, JWT, CORS, pipeline, mapping des routes
│   ├── appsettings.json              # Config non sensible (pas de secrets)
│   ├── EndPoints/                    # Minimal API — un fichier de routes par module
│   │   ├── UtilisateurRoutes.cs
│   │   ├── FideliteRoutes.cs
│   │   ├── PersonnelRoutes.cs
│   │   ├── PlanningRoutes.cs
│   │   └── PadelRoutes.cs
│   ├── Middleware/
│   │   ├── GlobalExceptionHandlerMiddleware.cs   # Exceptions → codes HTTP + message FR
│   │   └── ActiveUserMiddleware.cs               # Bloque les comptes soft-deleted
│   ├── Models/                       # DTO de requête HTTP (CreerTerrainRequest, TarifRequest…)
│   └── Services/
│       └── JwtTokenService.cs        # Génération du token JWT
│
├── Core/                             # Logique métier pure — AUCUNE dépendance infra (ni Dapper, ni MySQL)
│   ├── Models/                       # Entités métier + requêtes (User, Reservation, Tarif, Terrain…)
│   ├── IGateways/                    # Interfaces de passerelle (IUserGateway, IReservationGateway…)
│   ├── UseCases/                     # Logique applicative + validation
│   │   ├── Abstractions/             # Interfaces des use cases (IPadelUseCases…)
│   │   ├── PadelUseCases.cs
│   │   ├── UserUseCases.cs
│   │   ├── FideliteUseCases.cs
│   │   ├── PersonnelUseCases.cs
│   │   └── PlanningUseCases.cs
│   ├── Exceptions/                   # ValidationException(400)/Conflict(409)/NotFound(404)/Forbidden(403)
│   └── ServiceCollectionExtension.cs # AddCoreServices() — enregistre les use cases
│
└── Infrastructure/                   # Accès données (dépend de Core uniquement)
    ├── Models/                       # Modèles DB (mapping colonnes SQL)
    ├── Repositories/                 # SQL brut via Dapper
    │   └── Abstractions/             # Interfaces repository
    ├── Gateways/                     # Mapping Infrastructure.Models ↔ Core.Models + BCrypt
    ├── ServiceCollectionExtension.cs # AddInfrastructureServices() — DB, repos, gateways
    └── horeca_db.sql                 # Schéma MySQL + données de référence
```

### Frontend — Angular 21 standalone (zoneless, signals)

```
frontend/src/app/
├── app.ts / app.html / app.css      # Composant racine (shell : header + side-menu + router-outlet)
├── app.config.ts                    # Providers (router, HttpClient…)
├── app.routes.ts                    # Routes générées depuis nav.config (+ guards)
├── nav.config.ts                    # SOURCE UNIQUE des pages (path, composant, rôles, sidebar)
├── roles.ts                         # Rôles : Client / Employe / Cuisine / Administrateur
│
├── guards/
│   ├── auth.guard.ts                # Exige un token valide
│   └── role.guard.ts                # Vérifie les rôles autorisés (data.roles)
│
├── components/                      # Composants réutilisables
│   ├── header/                      # En-tête
│   ├── navbar/                      # Barre du haut (projette le contenu via ng-content)
│   ├── side-menu/                   # Menu latéral filtré par rôle
│   ├── friterie-card/ glaces-card/ padel-card/   # Cartes commerces (home)
│   ├── reserver-card/               # Onglet « Réserver » (réservation manuelle)
│   ├── terrains-card/               # Onglet « Terrains » (toggle / édition / création)
│   ├── reservations-card/           # Onglet « Réservations » (vue staff + annulation)
│   └── tarifs-card/                 # Onglet « Tarifs » (planning peignable)
│
├── pages/                           # Une page par route
│   ├── home-page/  login-page/  register-page/  mon-compte-page/
│   ├── fidelite-page/  padel-page/
│   ├── gestion-terrains-page/       # Coquille : navbar + onglets → cards ci-dessus
│   ├── gestion-utilisateurs-page/
│   ├── disponibilites-page/  mon-horaire-page/   # (à venir)
│   └── gestion-cuisine-page/  gestion-stocks-page/  creer-horaires-page/   # (à venir)
│
├── services/
│   ├── api/                         # Un service HTTP par domaine
│   │   ├── auth.service.ts  users.service.ts  fidelite.service.ts
│   │   ├── padel.service.ts  utilisateur-admin.service.ts
│   │   └── models/              # DTO/interfaces ([entité].model.ts)
│   ├── auth-state.service.ts        # État d'auth (token, rôle) — signals
│   ├── menu-state.service.ts        # Ouverture du menu latéral
│   └── tab-state.service.ts
│
└── shared/
    └── format.util.ts               # Helpers de formatage partagés (formatHeure, formatDate)
```

> Les pages marquées *(à venir)* sont volontairement présentes dans la navigation
> (choix de scope assumé) ; elles n'ont pas encore de logique branchée.

---

## Modules

| Module | État |
|---|---|
| Authentification & rôles (JWT, BCrypt, soft-delete) | ✅ |
| Fidélité (solde, transactions) | ✅ |
| Planning / Personnel (disponibilités, horaires) | ✅ |
| Padel (terrains, tarifs, réservations) | ✅ |
