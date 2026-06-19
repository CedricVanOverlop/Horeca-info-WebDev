# Horeca-info.com — Application web

Application web complémentaire à **Odoo** pour **Horeca-info.com SRL** (Lobbes, Wallonie).
Trois commerces sur un même site : **Friterie.net**, **Baraque à Glaces**, **Centre Padel**.
Elle gère ce qu'Odoo ne couvre pas (comptes clients/staff, fidélité, planning, réservations padel)
et ne duplique aucune donnée gérée par Odoo (stocks, factures, catalogue).

Architecture **Clean Architecture** en 3 projets C# — `Api` → `Core` (logique métier, interfaces) ←
`Infrastructure` (Dapper, gateways) — avec un frontend Angular 21 standalone.

---

## 1. Stack technique

| Couche | Technologie | Version |
|---|---|---|
| Backend | ASP.NET Core C# Minimal API | .NET 10 |
| Base de données | MySQL — SQL brut via **Dapper** (pas d'Entity Framework) | MySQL 8.0 |
| Frontend | Angular standalone, zoneless | Angular 21 |
| Auth | JWT Bearer + BCrypt (BCrypt.Net-Next, coût 11) | — |

---

## 2. Prérequis

Installer ces 4 outils, puis vérifier chaque version avec la commande indiquée.

| Outil | Version min. | Vérifier | Installer |
|---|---|---|---|
| .NET SDK | **10.0** | `dotnet --version` → `10.0.x` | https://dotnet.microsoft.com/download/dotnet/10.0 |
| Node.js | **20 LTS+** | `node --version` → `v20.x`+ | https://nodejs.org |
| Angular CLI | **21** | `ng version` | `npm install -g @angular/cli@21` |
| MySQL Server | **8.0** | `mysql --version` → `8.0.x` | https://dev.mysql.com/downloads/mysql/ |

> Sous Windows, `dotnet`, `node`, `npm`, `ng` et `mysql` doivent être dans le `PATH`.
> Si `mysql` est introuvable, ajouter `C:\Program Files\MySQL\MySQL Server 8.0\bin` au `PATH`
> (ou utiliser MySQL Workbench pour exécuter les scripts SQL).

---

## 3. Installation pas à pas

### 3.a — Cloner le dépôt

```bash
git clone https://github.com/CedricVanOverlop/Horeca-info-WebDev.git
cd Horeca-info-WebDev
```

### 3.b — Créer la base de données

**Deux scripts, dans cet ordre strict** (le second dépend du premier) :

```bash
# 1) Schéma : crée la base horeca_info, toutes les tables, contraintes et la table COMMERCE.
mysql -u root -p < backend/Infrastructure/horeca_db.sql

# 2) Comptes de test : 12 utilisateurs prêts à l'emploi (voir section 7). APRÈS le schéma.
mysql -u root -p < backend/Infrastructure/seed.sql
```

> `seed.sql` est **ré-exécutable** : il supprime d'abord les comptes `%@test.com`
> existants (dans le bon ordre FK) avant de les réinsérer. Aucune erreur si relancé.
> Les mots de passe y sont stockés en **hash BCrypt réel** (jamais en clair).

### 3.c — Configurer les secrets

Les secrets ne sont **jamais** dans `appsettings.json` (versionné). Deux options au choix.

**Option A — fichier de dev local (recommandé)**
Créer `backend/Api/appsettings.Development.json` (ignoré par Git) :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=horeca_info;Uid=root;Pwd=root;"
  },
  "Jwt": {
    "Key": "ma-cle-jwt-de-demo-32-caracteres-minimum-0123456789"
  }
}
```

**Option B — variables d'environnement** (prioritaires sur le fichier ci-dessus) :

```powershell
# Windows PowerShell (session courante)
$env:DB_CONNECTION = "Server=localhost;Database=horeca_info;Uid=root;Pwd=root;"
$env:JWT_KEY       = "ma-cle-jwt-de-demo-32-caracteres-minimum-0123456789"
```

```bash
# Linux / macOS (bash)
export DB_CONNECTION="Server=localhost;Database=horeca_info;Uid=root;Pwd=root;"
export JWT_KEY="ma-cle-jwt-de-demo-32-caracteres-minimum-0123456789"
```

> ⚠️ **Erreur n°1 au premier lancement** : sans `JWT_KEY` (ou `Jwt:Key`), **l'API refuse de
> démarrer** avec `Variable d'environnement JWT_KEY manquante.`. La clé doit faire **au moins
> 32 caractères** (sinon erreur de signature au login : *IDX10720 / key size too small*).
> Adapter `Uid`/`Pwd` à votre installation MySQL (souvent `root` / le mot de passe choisi à l'install).

### 3.d — Restaurer les dépendances frontend

```bash
cd frontend
npm install
cd ..
```

---

## 4. Lancer le backend

```bash
cd backend/Api
dotnet run --launch-profile https
```

- API : **https://localhost:7160** (et http://localhost:5287).
- Swagger (dev uniquement) : **https://localhost:7160/swagger**.

> Le frontend de dev appelle `https://localhost:7160` → lancer impérativement le profil `https`.

## 5. Lancer le frontend

Dans un **second terminal** (le backend doit rester lancé en parallèle) :

```bash
cd frontend
npm start
```

- Application : **http://localhost:4200**.

> Back **et** front tournent simultanément, dans 2 terminaux distincts.

---

## 6. Comptes de test

Tous les comptes ci-dessous (créés par `seed.sql`) ont le mot de passe **`Test123`**.

> 🔑 **Compte de démo principal : `admin1@test.com` / `Test123`** (accès Administrateur complet).

| Email | Mot de passe | Rôle |
|---|---|---|
| **admin1@test.com** | **Test123** | **Administrateur** |
| cuisine1@test.com | Test123 | Cuisine |
| employe1@test.com | Test123 | Employe (Friterie) |
| employe2@test.com | Test123 | Employe (Glaces) |
| employe3@test.com | Test123 | Employe (Padel) |
| employe4@test.com | Test123 | Employe (polyvalent) |
| employe5@test.com | Test123 | Employe (Friterie) |
| user1@test.com | Test123 | Client |
| user2@test.com | Test123 | Client (50 pts) |
| user3@test.com | Test123 | Client (12,5 pts) |
| user4@test.com | Test123 | Client |
| user5@test.com | Test123 | Client (120 pts) |

> Besoin d'un nouveau client ? La page `http://localhost:4200/register` permet aussi de s'inscrire.

### Résolution des rôles

| Rôle | Condition |
|---|---|
| **Client** | aucune ligne `EMPLOYE` active |
| **Employe** | `EMPLOYE.acces = 'Employe'` et `actif = TRUE` |
| **Cuisine** | `EMPLOYE.acces = 'Cuisine'` et `actif = TRUE` |
| **Administrateur** | `EMPLOYE.acces = 'Administrateur'` et `actif = TRUE` |

Sécurité serveur via policies ASP.NET Core (`AdminOnly`, `CuisineOrAdmin`, `PersonnelOnly`) ;
guards Angular (`auth.guard`, `role.guard`) complémentaires côté client.

---

## 7. Dépannage

| Symptôme | Cause probable | Solution |
|---|---|---|
| `Variable d'environnement JWT_KEY manquante.` au démarrage de l'API | Aucun secret JWT défini | Définir `JWT_KEY` (≥ 32 caractères) ou `Jwt:Key` dans `appsettings.Development.json` (section 3.c) |
| Login échoue avec *IDX10720 / key too small* | Clé JWT trop courte | Utiliser une clé d'**au moins 32 caractères** |
| `Variable d'environnement DB_CONNECTION manquante.` | Pas de chaîne de connexion | Renseigner `ConnectionStrings:DefaultConnection` ou `DB_CONNECTION` (section 3.c) |
| `Access denied for user 'root'@'localhost'` | Mauvais identifiants MySQL | Corriger `Uid`/`Pwd` dans la chaîne de connexion |
| `Unknown database 'horeca_info'` | Schéma non créé | Exécuter `horeca_db.sql` (section 3.b) |
| Le login renvoie 401 alors que le compte existe | Comptes de test non chargés | Exécuter `seed.sql` **après** `horeca_db.sql` |
| `Address already in use` / port 7160, 5287 ou 4200 occupé | Une instance tourne déjà | Fermer l'autre process, ou changer le port (`launchSettings.json` pour l'API, `ng serve --port 4300` pour le front) |
| Front : erreurs CORS / appels bloqués | Backend non lancé en `https`, ou mauvaise URL | Lancer l'API avec `--launch-profile https` (CORS autorise `http://localhost:4200`) |
| `Your connection is not private` / certificat HTTPS dev | Certificat de dev non approuvé | `dotnet dev-certs https --trust` puis relancer |
| `ng` introuvable | Angular CLI non installé globalement | `npm install -g @angular/cli@21` |

---

## 8. Structure du projet

### Backend — Clean Architecture (`Api` → `Core` ← `Infrastructure`)

```
backend/
├── Api/                              # Couche présentation (dépend de Core + Infrastructure)
│   ├── Program.cs                    # Composition root : DI, JWT, CORS, pipeline, mapping des routes
│   ├── appsettings.json              # Config non sensible (pas de secrets)
│   ├── EndPoints/                    # Minimal API — un fichier de routes par module
│   │   ├── UtilisateurRoutes.cs  FideliteRoutes.cs  PersonnelRoutes.cs
│   │   └── PlanningRoutes.cs  PadelRoutes.cs
│   ├── Middleware/
│   │   ├── GlobalExceptionHandlerMiddleware.cs   # Exceptions → codes HTTP + message FR
│   │   └── ActiveUserMiddleware.cs               # Bloque les comptes soft-deleted
│   ├── Models/                       # DTO de requête HTTP (CreerTerrainRequest, TarifRequest…)
│   └── Services/JwtTokenService.cs   # Génération du token JWT
│
├── Core/                             # Logique métier pure — AUCUNE dépendance infra
│   ├── Models/                       # Entités métier + requêtes (User, Reservation, Tarif…)
│   ├── IGateways/                    # Interfaces de passerelle (IUserGateway…)
│   ├── UseCases/  (+ Abstractions/)  # Logique applicative + validation
│   ├── Exceptions/                   # Validation(400)/Conflict(409)/NotFound(404)/Forbidden(403)
│   └── ServiceCollectionExtension.cs # AddCoreServices()
│
└── Infrastructure/                   # Accès données (dépend de Core uniquement)
    ├── Models/                       # Modèles DB (mapping colonnes SQL)
    ├── Repositories/  (+ Abstractions/)  # SQL brut via Dapper
    ├── Gateways/                     # Mapping Infrastructure ↔ Core + BCrypt
    ├── ServiceCollectionExtension.cs # AddInfrastructureServices()
    ├── horeca_db.sql                 # Schéma MySQL + données de référence
    └── seed.sql                      # Comptes de test (à exécuter après horeca_db.sql)
```

### Frontend — Angular 21 standalone (zoneless)

```
frontend/src/app/
├── app.config.ts  app.routes.ts  nav.config.ts  roles.ts   # Shell, routes, navigation, rôles
├── guards/                         # auth.guard.ts (token valide) · role.guard.ts (rôles)
├── components/                     # header, navbar, side-menu, *-card (commerces + onglets padel)
├── pages/                          # home, login, register, mon-compte, fidelite, padel,
│                                   #   gestion-terrains, gestion-utilisateurs, …
├── services/
│   ├── api/                        # auth, users, utilisateur-admin, fidelite, padel (+ models/)
│   ├── auth-state.service.ts       # État d'auth : getters qui décodent le JWT (pas de signals)
│   ├── menu-state.service.ts       # Ouverture du menu latéral (signal)
│   └── tab-state.service.ts        # Onglet actif (signal)
└── shared/format.util.ts           # Helpers de formatage (formatHeure, formatDate)
```

---

## 9. État des modules

| Module | État |
|---|---|
| Authentification & rôles (JWT, BCrypt, soft-delete) | ✅ Complet |
| Fidélité (solde, transactions) | ✅ Complet |
| Padel (terrains, tarifs, réservations) | ✅ Complet |
| Planning / Personnel (disponibilités, horaires) | 🟡 **Partiel — backend uniquement** (use cases + routes présents ; pages `disponibilites-page`, `mon-horaire-page`, `creer-horaires-page` non branchées) |
| Interface Odoo | ⛔ Non démarré (hors périmètre actuel) |
