# Horeca-info.com — Application web

Application web complémentaire à **Odoo** pour **Horeca-info.com SRL** (Lobbes, Wallonie).
Trois commerces sur un même site : **Friterie.net**, **Baraque à Glaces**, **Centre Padel**.
Elle gère ce qu'Odoo ne couvre pas (comptes clients/staff, fidélité, planning, réservations padel)
et ne duplique aucune donnée gérée par Odoo (stocks, factures, catalogue).

> 📘 **Ce README est un manuel d'installation pas à pas.** Suis les sections **1 → 7 dans l'ordre**,
> même si tu n'as pas l'habitude : chaque étape t'explique quoi faire et comment vérifier que ça marche.

| Couche | Technologie | Version |
|---|---|---|
| Backend | ASP.NET Core C# Minimal API | .NET 10 |
| Base de données | MySQL — SQL brut via **Dapper** (pas d'Entity Framework) | MySQL 8.0 |
| Frontend | Angular standalone, zoneless | Angular 21 |
| Auth | JWT Bearer + BCrypt (BCrypt.Net-Next, coût 11) | — |

---

## Fonctionnalités (état au rendu)

> 🎯 **Pour la démo** : le module **Authentification / Comptes** est complet de bout en bout
> (inscription → connexion → rôles → administration). Le module **Padel** est complet côté client
> et admin. Les autres modules sont à l'état de maquette (voir « Reste à faire »).

### ✅ Implémenté et fonctionnel

**Authentification & comptes** (complet, front + back)
- Inscription (`/register`) avec validation, email unique (409), réactivation d'un compte supprimé
- Connexion JWT (`/login`) + déconnexion
- Mon compte : voir/modifier son profil, changer son mot de passe, supprimer son compte (soft-delete)
- Rôles (Client / Employé / Cuisine / Administrateur) avec **guards Angular** + **policies serveur**
- Sidebar filtrée dynamiquement selon le rôle connecté

**Administration des utilisateurs** (Administrateur)
- Liste + recherche des comptes
- Changer le rôle d'un utilisateur
- Ajuster son solde de points de fidélité (avec motif)
- Bloquer / débloquer un compte
- Supprimer un compte (soft-delete)
- Voir le détail de ses réservations et de ses horaires

**Padel** (complet, front + back)
- Client : consulter les terrains, voir les tarifs (calcul auto jour/plage), créneaux occupés, réserver, voir/annuler ses réservations
- Admin : créer/modifier/(dés)activer des terrains, gérer la grille tarifaire (ajout/modif/suppression)
- Staff (Cuisine/Admin) : rechercher un client, créer une réservation manuelle (= blocage de créneau)

### 🚧 Reste à faire (maquettes / non branché)

| Module | État actuel | À faire |
|---|---|---|
| Planning employé | Pages `Mes disponibilités` / `Mon horaire` vides ; back partiel (seule la vue admin des horaires existe) | Déclaration des dispos + consultation horaire côté employé, endpoints associés |
| Création d'horaires (admin) | Page `Créer des horaires` vide | Génération/attribution des horaires |
| Gestion Cuisine | Page vide | Fonctions liées à la cuisine |
| Gestion Stocks | Page vide | Lecture/écriture des stocks **via l'API Odoo** (Phase 3) |
| Fidélité (client) | Solde visible dans le profil + ajustement admin ; pas de page client dédiée | Page gain/dépense de points côté client |
| Intégration Odoo | Non démarré | Proxy backend vers Odoo (credentials jamais exposés au front) |

> ℹ️ **Pourquoi des pages blanches dans le menu ?** Les pages ci-dessus (Mes disponibilités,
> Mon horaire, Créer des horaires, Gestion Cuisine, Gestion Stocks) sont **laissées
> volontairement** comme maquettes : elles servent à démontrer que l'**authentification et le
> filtrage par rôle fonctionnent jusqu'au bout** — chaque rôle ne voit dans la sidebar que les
> entrées qui le concernent, et les guards bloquent l'accès direct par URL. Le contenu métier de
> ces pages fait partie des évolutions futures.

---

## 1. Ce qu'il faut installer avant de commencer

Installe ces 3 logiciels. Pour chacun : clique le lien, prends la version indiquée (ou plus récente),
puis ouvre un terminal et tape la commande de vérification — si elle affiche un numéro de version, c'est bon.

| Logiciel | Version min. | Lien de téléchargement | Vérifier (dans un terminal) |
|---|---|---|---|
| **SDK .NET 10** | 10.0 | https://dotnet.microsoft.com/download/dotnet/10.0 | `dotnet --version` → `10.0.x` |
| **Node.js** (inclut npm) | 20 LTS+ | https://nodejs.org | `node --version` puis `npm --version` |
| **MySQL Server** | 8.0 | https://dev.mysql.com/downloads/installer/ | `mysql --version` → `8.0.x` |

> 💡 **Conseil débutant** : pendant l'installation de MySQL, coche aussi **MySQL Workbench**
> (proposé dans le même installeur). C'est une fenêtre graphique qui rend la création de la base
> beaucoup plus simple que la ligne de commande.
>
> Tout est testé sous **Windows 11** avec **PowerShell**. Les commandes ci-dessous sont en PowerShell.

---

## 2. Récupérer le projet

```powershell
git clone https://github.com/CedricVanOverlop/Horeca-info-WebDev.git
cd Horeca-info-WebDev
```

Le projet a deux dossiers principaux :

```
Horeca-info-WebDev/
├── backend/     ← l'API .NET (le serveur)
└── frontend/    ← l'application Angular (le site web)
```

---

## 3. Créer la base de données

Dans Services (Win+R, services.mcs), ne pas oublier de lancer MySQL80 si ce n'est pas automatique.

Deux fichiers SQL à exécuter **dans cet ordre** (le second a besoin du premier) :

1. `backend/Infrastructure/horeca_db.sql` → crée la base `horeca_info` et toutes les tables.
2. `backend/Infrastructure/seed.sql` → remplit la base avec des données de test (comptes, terrains, réservations).

### Option A — avec MySQL Workbench (recommandé pour débuter)

1. Ouvre MySQL Workbench, connecte-toi à ton serveur local (utilisateur `root`).
2. Menu **File → Open SQL Script…**, choisis `horeca_db.sql`, puis clique sur l'éclair ⚡ pour l'exécuter.
3. Recommence avec `seed.sql`.

### Option B — en ligne de commande

```powershell
mysql -u root -p < backend/Infrastructure/horeca_db.sql
mysql -u root -p < backend/Infrastructure/seed.sql
```

> `seed.sql` est **ré-exécutable** : il supprime d'abord les comptes de test (`%@test.com`)
> dans le bon ordre, puis les recrée. Aucune erreur si tu le relances pour repartir propre.
> Les mots de passe y sont stockés en **hash BCrypt réel** (jamais en clair).

---

## 4. Configurer les secrets du backend

Pour des raisons de sécurité, la **clé JWT** et la **connexion MySQL** ne sont **jamais** écrites
dans le code versionné. Tu dois créer un fichier local (ignoré par Git) qui les contient.

Crée le fichier **`backend/Api/appsettings.Development.json`** avec ce contenu :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=horeca_info;Uid=root;Pwd=root;"
  },
  "Jwt": {
    "Key": "remplace-ceci-par-une-longue-phrase-secrete-de-32-caracteres-minimum"
  }
}
```

Adapte :
- `Uid` / `Pwd` à ton installation MySQL (souvent `root` + le mot de passe choisi à l'install).
- `Jwt:Key` : une longue chaîne aléatoire d'**au moins 32 caractères** (elle signe les jetons de connexion).

> ✅ Ce fichier est déjà listé dans `.gitignore` : il ne partira **jamais** sur Git.
> En production, on utilise plutôt les variables d'environnement `DB_CONNECTION` et `JWT_KEY`
> (elles sont prioritaires sur le fichier ci-dessus).

---

## 5. Lancer le backend (le serveur)

Dans un **premier terminal**, à la racine du projet :

```powershell
dotnet run --project backend/Api
```

Au premier lancement, .NET télécharge ses dépendances — c'est normal que ce soit un peu long.

Quand c'est prêt, le serveur écoute sur **http://localhost:5287**.
Pour vérifier, ouvre dans ton navigateur **http://localhost:5287/swagger** : tu verras la liste de
toutes les routes de l'API.

> ⚠️ Laisse ce terminal ouvert : si tu le fermes, le serveur s'arrête.

---

## 6. Lancer le frontend (le site web)

Dans un **deuxième terminal** (sans fermer le premier) :

```powershell
cd frontend
npm install      # à faire une seule fois — installe les dépendances Angular (long la 1re fois)
npm start        # lance le site
```

Quand c'est prêt, ouvre **http://localhost:4200** dans ton navigateur. 🎉
(Les fois suivantes, tu peux sauter `npm install` et faire directement `npm start`.)

---

## 7. Se connecter — comptes de test

Tous les comptes créés par `seed.sql` ont le **même mot de passe : `Test123`**.

> 🔑 **Pour tout voir, connecte-toi d'abord avec le compte admin : `admin1@test.com` / `Test123`.**

| Email | Mot de passe | Rôle |
|---|---|---|
| **admin1@test.com** | **Test123** | **Administrateur** (accès complet) |
| cuisine1@test.com | Test123 | Cuisine |
| employe1@test.com … employe5@test.com | Test123 | Employé |
| user1@test.com … user5@test.com | Test123 | Client (user2/3/5 ont des points de fidélité) |

> Besoin d'un nouveau client ? La page `http://localhost:4200/register` permet aussi de s'inscrire.

---

## 8. Récapitulatif rapide (une fois tout installé)

Deux terminaux, lancés à la racine du projet :

```powershell
# Terminal 1 — backend
dotnet run --project backend/Api

# Terminal 2 — frontend
cd frontend
npm start
```

Puis ouvre **http://localhost:4200**.

| Service | Adresse |
|---|---|
| Site web (frontend) | http://localhost:4200 |
| API (backend) | http://localhost:5287 |
| Documentation API (Swagger) | http://localhost:5287/swagger |

---

## 9. En cas de problème

| Symptôme | Cause probable | Solution |
|---|---|---|
| `Variable d'environnement JWT_KEY manquante.` au démarrage de l'API | Fichier `appsettings.Development.json` absent, mal nommé, ou clé `Jwt:Key` manquante | Refais l'**étape 4**, vérifie le nom exact du fichier et le bloc `Jwt:Key` |
| Login échoue avec *IDX10720 / key too small* | Clé JWT trop courte | Mets une clé d'**au moins 32 caractères** |
| `Variable d'environnement DB_CONNECTION manquante.` | Pas de chaîne de connexion | Renseigne `ConnectionStrings:DefaultConnection` (étape 4) |
| `Access denied for user 'root'@'localhost'` | Mauvais identifiants MySQL | Corrige `Uid`/`Pwd` dans la chaîne de connexion |
| `Unknown database 'horeca_info'` | Schéma non créé | Exécute `horeca_db.sql` (étape 3) |
| Login renvoie 401 alors que le compte existe | Comptes de test non chargés | Exécute `seed.sql` **après** `horeca_db.sql` |
| Le site se charge mais aucune donnée / erreurs réseau (CORS) | Backend non lancé, ou pas sur le port 5287 | Vérifie que le terminal 1 tourne sur http://localhost:5287 |
| Port 5287 ou 4200 déjà occupé | Une instance tourne déjà | Ferme l'autre process (ou `ng serve --port 4300` pour le front) |
| `MSB3027` / DLL verrouillée au build | Le backend tourne déjà dans un autre terminal | Arrête-le (`Ctrl+C`) avant de relancer |

