-- =============================================================
-- FICHIER : seed.sql
-- Projet   : Application complémentaire Odoo — Horeca-info.com SRL
-- Rôle     : Jeu de données de test pour la démo / l'évaluation.
--            - 12 utilisateurs (5 clients, 5 employés, 1 cuisine, 1 admin),
--              tous avec le mot de passe « Test123 ».
--            - 2 terrains de padel (10:00→22:00), grille TARIF différenciée
--              (3 plages × semaine/week-end, extérieur moins cher) et
--              5 réservations (semaine du 29/06/2026).
--
-- ORDRE D'EXÉCUTION : APRÈS backend/Infrastructure/horeca_db.sql
--   1) mysql -u root -p < backend/Infrastructure/horeca_db.sql
--   2) mysql -u root -p < backend/Infrastructure/seed.sql
--
-- RE-EXÉCUTABLE : le script supprime d'abord les comptes de test
--   (email LIKE '%@test.com') en respectant l'ordre des FK, puis réinsère.
--
-- SÉCURITÉ MOT DE PASSE :
--   La colonne mot_de_passe contient un VRAI hash BCrypt (jamais « Test123 »
--   en clair). Hash généré avec BCrypt.Net-Next 4.0.3 (même lib que le backend),
--   coût par défaut = 11 (préfixe « $2a$11$ »). Le sel étant intégré au hash,
--   un seul hash est réutilisable pour tous les comptes.
--   Génération + vérification (BCrypt.Verify("Test123", hash) == True) faites
--   avant insertion. Pour régénérer un hash :
--     var h = BCrypt.Net.BCrypt.HashPassword("Test123");      // coût 11
--     BCrypt.Net.BCrypt.Verify("Test123", h);                 // doit valoir True
-- =============================================================

USE horeca_info;

-- Désactive le safe update mode (MySQL Workbench) : les DELETE de nettoyage
-- ci-dessous ciblent via sous-requête (pas de colonne KEY directe), ce qui
-- déclenche l'erreur 1175 en safe mode. Rétabli en fin de script.
SET SQL_SAFE_UPDATES = 0;

-- Hash BCrypt de « Test123 » (coût 11) — vérifié BCrypt.Verify == True.
SET @pwd_hash = '$2a$11$srlMPGlNjGWXLWvO1eh4KOZB2CEmkBXh0AlB8RnKaIXpMHGFUVGu6';

-- -------------------------------------------------------------
-- Nettoyage idempotent — ordre FK : enfants d'abord, parent ensuite.
-- On cible UNIQUEMENT les comptes de test (email '%@test.com').
-- -------------------------------------------------------------
DELETE FROM TRANSACTION_FIDELITE
 WHERE id_utilisateur IN (SELECT id_utilisateur FROM UTILISATEUR WHERE email LIKE '%@test.com');

DELETE FROM RESERVATION
 WHERE id_utilisateur IN (SELECT id_utilisateur FROM UTILISATEUR WHERE email LIKE '%@test.com');

DELETE FROM EMPLOYE
 WHERE id_utilisateur IN (SELECT id_utilisateur FROM UTILISATEUR WHERE email LIKE '%@test.com');

DELETE FROM UTILISATEUR WHERE email LIKE '%@test.com';

-- Données padel de démo (terrains marqués « (seed) »). Ordre FK : RESERVATION → TARIF → TERRAIN.
DELETE FROM RESERVATION
 WHERE id_terrain IN (SELECT id_terrain FROM TERRAIN WHERE nom LIKE '%(seed)');
DELETE FROM TARIF
 WHERE id_terrain IN (SELECT id_terrain FROM TERRAIN WHERE nom LIKE '%(seed)');
DELETE FROM TERRAIN WHERE nom LIKE '%(seed)';

-- -------------------------------------------------------------
-- UTILISATEUR — 5 clients + 5 employés + 1 cuisine + 1 admin.
-- points_solde > 0 sur quelques clients pour une démo fidélité parlante.
-- -------------------------------------------------------------
INSERT INTO UTILISATEUR (nom, prenom, email, mot_de_passe, telephone, points_solde, actif) VALUES
  -- Clients
  ('Dubois',    'Camille', 'user1@test.com',     @pwd_hash, '+32 471 10 20 30',  0.00, TRUE),
  ('Lambert',   'Hugo',    'user2@test.com',     @pwd_hash, '+32 472 11 21 31', 50.00, TRUE),
  ('Moreau',    'Sophie',  'user3@test.com',     @pwd_hash, '+32 473 12 22 32', 12.50, TRUE),
  ('Petit',     'Lucas',   'user4@test.com',     @pwd_hash, '+32 474 13 23 33',  0.00, TRUE),
  ('Renard',    'Emma',    'user5@test.com',     @pwd_hash, '+32 475 14 24 34', 120.00, TRUE),
  -- Employés (acces='Employe')
  ('Martin',    'Nathan',  'employe1@test.com',  @pwd_hash, '+32 476 15 25 35',  0.00, TRUE),
  ('Leroy',     'Chloé',   'employe2@test.com',  @pwd_hash, '+32 477 16 26 36',  0.00, TRUE),
  ('Garcia',    'Maxime',  'employe3@test.com',  @pwd_hash, '+32 478 17 27 37',  0.00, TRUE),
  ('Simon',     'Julie',   'employe4@test.com',  @pwd_hash, '+32 479 18 28 38',  0.00, TRUE),
  ('Roux',      'Tom',     'employe5@test.com',  @pwd_hash, '+32 470 19 29 39',  0.00, TRUE),
  -- Cuisine (acces='Cuisine')
  ('Fontaine',  'Léa',     'cuisine1@test.com',  @pwd_hash, '+32 471 30 40 50',  0.00, TRUE),
  -- Administrateur (acces='Administrateur') — compte de démo principal
  ('Vandamme',  'Pierre',  'admin1@test.com',    @pwd_hash, '+32 472 31 41 51',  0.00, TRUE);

-- -------------------------------------------------------------
-- EMPLOYE — relie certains UTILISATEUR au rôle staff.
-- id_commerce_preference : 1=Friterie.net, 2=Baraque à Glaces, 3=Padel Center,
--                          NULL = employé polyvalent (3 commerces).
-- Les sous-requêtes par email rendent le script indépendant des AUTO_INCREMENT.
-- -------------------------------------------------------------
INSERT INTO EMPLOYE (id_utilisateur, acces, actif, id_commerce_preference) VALUES
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'employe1@test.com'), 'Employe',        TRUE, 1),
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'employe2@test.com'), 'Employe',        TRUE, 2),
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'employe3@test.com'), 'Employe',        TRUE, 3),
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'employe4@test.com'), 'Employe',        TRUE, NULL),
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'employe5@test.com'), 'Employe',        TRUE, 1),
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'cuisine1@test.com'), 'Cuisine',        TRUE, 1),
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'admin1@test.com'),   'Administrateur', TRUE, 3);

-- -------------------------------------------------------------
-- Cohérence du solde de points : toute valeur points_solde > 0 doit avoir
-- sa trace dans TRANSACTION_FIDELITE (RG-03). On insère un GAIN initial
-- équivalent pour les clients crédités ci-dessus (id_commerce NULL = global).
-- -------------------------------------------------------------
INSERT INTO TRANSACTION_FIDELITE (id_utilisateur, id_commerce, points, type_transaction, description) VALUES
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'user2@test.com'), NULL,  50.00, 'GAIN', 'Crédit initial de démonstration'),
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'user3@test.com'), NULL,  12.50, 'GAIN', 'Crédit initial de démonstration'),
  ((SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'user5@test.com'), NULL, 120.00, 'GAIN', 'Crédit initial de démonstration');

-- -------------------------------------------------------------
-- TERRAIN — 2 terrains de padel (commerce 3 = Padel Center),
-- ouverts de 10:00 à 22:00 tous les jours. Suffixe « (seed) »
-- pour le nettoyage idempotent ci-dessus.
-- -------------------------------------------------------------
INSERT INTO TERRAIN (nom, actif, heure_ouverture, heure_fermeture, id_commerce) VALUES
  ('Terrain 1 — Couvert (seed)',  TRUE, '10:00:00', '22:00:00', 3),
  ('Terrain 2 — Extérieur (seed)', TRUE, '10:00:00', '22:00:00', 3);

-- -------------------------------------------------------------
-- TARIF — grille différenciée par terrain, jour et plage horaire.
-- 3 plages quotidiennes : Matin 10→14, Après-midi 14→18, Soirée 18→22.
-- 2 groupes de jours : SEMAINE (ISO 1→5), WEEKEND (ISO 6→7).
-- Terrain 1 — Couvert : prix « pleins » ; Terrain 2 — Extérieur : -4 €/h.
-- Le week-end et la soirée sont plus chers (heures pleines).
-- Total : 2 terrains × 7 jours × 3 plages = 42 lignes.
-- -------------------------------------------------------------
INSERT INTO TARIF (type, prix_heure, heure_debut, heure_fin, jour_semaine, id_terrain)
SELECT s.type,
       CASE WHEN t.nom LIKE 'Terrain 1%' THEN s.prix_couvert ELSE s.prix_exterieur END,
       s.heure_debut, s.heure_fin, d.jour, t.id_terrain
FROM   TERRAIN t
JOIN   (SELECT 1 jour, 'SEMAINE' grp UNION SELECT 2, 'SEMAINE' UNION SELECT 3, 'SEMAINE'
        UNION SELECT 4, 'SEMAINE' UNION SELECT 5, 'SEMAINE'
        UNION SELECT 6, 'WEEKEND' UNION SELECT 7, 'WEEKEND') d
JOIN   (
        SELECT 'Matin semaine'      type, 'SEMAINE' grp, '10:00:00' heure_debut, '14:00:00' heure_fin, 22.00 prix_couvert, 18.00 prix_exterieur
        UNION SELECT 'Après-midi semaine', 'SEMAINE', '14:00:00', '18:00:00', 26.00, 22.00
        UNION SELECT 'Soirée semaine',     'SEMAINE', '18:00:00', '22:00:00', 30.00, 26.00
        UNION SELECT 'Matin week-end',     'WEEKEND', '10:00:00', '14:00:00', 28.00, 24.00
        UNION SELECT 'Après-midi week-end','WEEKEND', '14:00:00', '18:00:00', 32.00, 28.00
        UNION SELECT 'Soirée week-end',    'WEEKEND', '18:00:00', '22:00:00', 34.00, 30.00
       ) s ON s.grp = d.grp
WHERE  t.nom LIKE '%(seed)';

-- -------------------------------------------------------------
-- RESERVATION — 5 réservations dans la semaine du 29 juin 2026
-- (lundi 29/06 → dimanche 05/07). prix_paye = prix_heure × durée.
-- Le tarif est résolu par (terrain, jour ISO, plage horaire) : il existe
-- désormais 3 TARIF par jour, donc la sous-requête filtre aussi sur
-- heure_debut de la plage (Matin 10:00 / Après-midi 14:00 / Soirée 18:00)
-- pour rester unique. Chaque réservation tient dans une seule plage.
-- Pas de chevauchement sur un même terrain (dates/heures disjointes).
-- -------------------------------------------------------------
INSERT INTO RESERVATION
  (date, heure_debut, heure_fin, prix_paye, moyen_paiement, remarques, id_terrain, id_utilisateur, id_tarif)
VALUES
  -- T1 · lundi 29/06 · 10:00-11:00 (1h) · matin semaine 22 €/h → 22,00 · client user1
  ('2026-06-29', '10:00:00', '11:00:00', 22.00, 'EnLigne', NULL,
     (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 1 — Couvert (seed)'),
     (SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'user1@test.com'),
     (SELECT id_tarif FROM TARIF WHERE jour_semaine = 1 AND heure_debut = '10:00:00'
        AND id_terrain = (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 1 — Couvert (seed)'))),
  -- T1 · mardi 30/06 · 18:00-19:30 (1,5h) · soirée semaine 30 €/h → 45,00 · client user2
  ('2026-06-30', '18:00:00', '19:30:00', 45.00, 'SurPlace', NULL,
     (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 1 — Couvert (seed)'),
     (SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'user2@test.com'),
     (SELECT id_tarif FROM TARIF WHERE jour_semaine = 2 AND heure_debut = '18:00:00'
        AND id_terrain = (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 1 — Couvert (seed)'))),
  -- T2 · mercredi 01/07 · 14:00-15:00 (1h) · après-midi semaine 22 €/h → 22,00 · client user3
  ('2026-07-01', '14:00:00', '15:00:00', 22.00, 'EnLigne', NULL,
     (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 2 — Extérieur (seed)'),
     (SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'user3@test.com'),
     (SELECT id_tarif FROM TARIF WHERE jour_semaine = 3 AND heure_debut = '14:00:00'
        AND id_terrain = (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 2 — Extérieur (seed)'))),
  -- T2 · vendredi 03/07 · 20:00-21:00 (1h) · soirée semaine 26 €/h → 26,00 · client user5
  ('2026-07-03', '20:00:00', '21:00:00', 26.00, 'SurPlace', NULL,
     (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 2 — Extérieur (seed)'),
     (SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'user5@test.com'),
     (SELECT id_tarif FROM TARIF WHERE jour_semaine = 5 AND heure_debut = '18:00:00'
        AND id_terrain = (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 2 — Extérieur (seed)'))),
  -- T1 · samedi 04/07 · 11:00-13:00 (2h) · matin week-end 28 €/h → 56,00 · réservation staff (admin1)
  ('2026-07-04', '11:00:00', '13:00:00', 56.00, 'SurPlace', 'Réservation de démonstration (staff)',
     (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 1 — Couvert (seed)'),
     (SELECT id_utilisateur FROM UTILISATEUR WHERE email = 'admin1@test.com'),
     (SELECT id_tarif FROM TARIF WHERE jour_semaine = 6 AND heure_debut = '10:00:00'
        AND id_terrain = (SELECT id_terrain FROM TERRAIN WHERE nom = 'Terrain 1 — Couvert (seed)')));

-- Rétablit le safe update mode.
SET SQL_SAFE_UPDATES = 1;
