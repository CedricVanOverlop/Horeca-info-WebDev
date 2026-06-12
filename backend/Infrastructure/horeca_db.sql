-- =============================================================
-- BASE DE DONNÉES : horeca_info
-- Projet       : Application complémentaire Odoo — Horeca-info.com SRL
-- Version      : 1.0 (officielle)
-- Description  : Base locale uniquement. Aucune donnée Odoo n'est
--                dupliquée ici. Les stocks et la facturation sont
--                lus/écrits en temps réel via l'API JSON-RPC Odoo.
-- =============================================================

CREATE DATABASE IF NOT EXISTS horeca_info
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE horeca_info;


-- =============================================================
-- TABLE : COMMERCE
-- Rôle  : Table pivot immuable — référence les 3 commerces.
--         Utilisée comme FK dans la quasi-totalité des tables.
-- =============================================================
CREATE TABLE COMMERCE (
    id_commerce INT          NOT NULL AUTO_INCREMENT,
    nom         VARCHAR(100) NOT NULL,

    CONSTRAINT pk_commerce PRIMARY KEY (id_commerce)
);

-- Données fixes — ne jamais modifier ni supprimer en production.
INSERT INTO COMMERCE (nom) VALUES
    ('Friterie.net'),
    ('La Baraque à Glaces'),
    ('Padel Center Lobbes');


-- =============================================================
-- TABLE : UTILISATEUR
-- Rôle  : Tous les comptes de l'application (clients ET employés).
--         Authentification exclusive par cette table (email + hash).
--         Le rôle se déduit : absent de EMPLOYE → Client ;
--         présent dans EMPLOYE → valeur de EMPLOYE.acces.
-- =============================================================
CREATE TABLE UTILISATEUR (
    id_utilisateur INT           NOT NULL AUTO_INCREMENT,
    nom            VARCHAR(100)  NOT NULL,
    prenom         VARCHAR(100)  NOT NULL,
    email          VARCHAR(255)  NOT NULL,
    mot_de_passe   VARCHAR(255)  NOT NULL,   -- Hash BCrypt, jamais en clair
    telephone      VARCHAR(20)   NULL,        -- Format libre : +32 471 12 34 56
    points_solde   DECIMAL(10,2) NOT NULL DEFAULT 0.00,

    CONSTRAINT pk_utilisateur    PRIMARY KEY (id_utilisateur),
    CONSTRAINT uq_email          UNIQUE      (email),
    CONSTRAINT chk_points_solde  CHECK       (points_solde >= 0),

    INDEX idx_email (email)
);


-- =============================================================
-- TABLE : EMPLOYE
-- Rôle  : Étend UTILISATEUR pour les profils professionnels.
--         Relation 1-1 stricte (UNIQUE sur id_utilisateur).
--         id_commerce_preference = NULL → employé polyvalent
--         sur les trois commerces.
-- =============================================================
CREATE TABLE EMPLOYE (
    id_employe             INT         NOT NULL AUTO_INCREMENT,
    id_utilisateur         INT         NOT NULL,
    acces                  VARCHAR(50) NOT NULL,
    actif                  BOOLEAN     NOT NULL DEFAULT TRUE,  -- FALSE = a quitté la société
    id_commerce_preference INT         NULL,

    CONSTRAINT pk_employe         PRIMARY KEY (id_employe),
    CONSTRAINT uq_id_utilisateur  UNIQUE      (id_utilisateur),  -- Garantit la relation 1-1
    CONSTRAINT fk_employe_utilisateur
        FOREIGN KEY (id_utilisateur)
        REFERENCES UTILISATEUR(id_utilisateur),
    CONSTRAINT fk_employe_commerce
        FOREIGN KEY (id_commerce_preference)
        REFERENCES COMMERCE(id_commerce),
    CONSTRAINT chk_acces
        CHECK (acces IN ('Employe', 'Cuisine', 'Administrateur'))
);


-- =============================================================
-- TABLE : DISPONIBILITE
-- Rôle  : Créneaux déclarés par un employé pour indiquer
--         ses disponibilités. Plusieurs lignes possibles
--         pour un même employé sur une même journée.
-- =============================================================
CREATE TABLE DISPONIBILITE (
    id_disponibilite INT  NOT NULL AUTO_INCREMENT,
    id_employe       INT  NOT NULL,
    date             DATE NOT NULL,
    heure_debut      TIME NOT NULL,
    heure_fin        TIME NOT NULL,
    commentaire      TEXT NULL,

    CONSTRAINT pk_disponibilite PRIMARY KEY (id_disponibilite),
    CONSTRAINT fk_dispo_employe
        FOREIGN KEY (id_employe)
        REFERENCES EMPLOYE(id_employe),
    CONSTRAINT chk_dispo_heures
        CHECK (heure_fin > heure_debut),

    INDEX idx_dispo_employe (id_employe),
    INDEX idx_dispo_date    (date)
);


-- =============================================================
-- TABLE : HORAIRE
-- Rôle  : Planning effectif validé par l'administrateur.
--         Croise un employé, un commerce et une plage horaire
--         pour une date donnée.
-- =============================================================
CREATE TABLE HORAIRE (
    id_horaire  INT           NOT NULL AUTO_INCREMENT,
    id_employe  INT           NOT NULL,
    id_commerce INT           NOT NULL,
    date        DATE          NOT NULL,
    heure_debut TIME          NOT NULL,
    heure_fin   TIME          NOT NULL,
    heure_payee DECIMAL(5,2)  NOT NULL DEFAULT 0.00,  -- Heures à rémunérer
    statut      VARCHAR(50)   NOT NULL DEFAULT 'Planifie',

    CONSTRAINT pk_horaire PRIMARY KEY (id_horaire),
    CONSTRAINT fk_horaire_employe
        FOREIGN KEY (id_employe)
        REFERENCES EMPLOYE(id_employe),
    CONSTRAINT fk_horaire_commerce
        FOREIGN KEY (id_commerce)
        REFERENCES COMMERCE(id_commerce),
    CONSTRAINT chk_horaire_heures
        CHECK (heure_fin > heure_debut),
    CONSTRAINT chk_horaire_heure_payee
        CHECK (heure_payee >= 0),
    CONSTRAINT chk_statut
        CHECK (statut IN ('Planifie', 'Confirme', 'Annule')),

    INDEX idx_horaire_employe (id_employe),
    INDEX idx_horaire_date    (date)
);


-- =============================================================
-- TABLE : TERRAIN
-- Rôle  : Terrains de padel réservables. Un terrain inactif
--         (travaux, maintenance) n'est plus proposé à la
--         réservation mais conserve son historique.
-- =============================================================
CREATE TABLE TERRAIN (
    id_terrain  INT          NOT NULL AUTO_INCREMENT,
    nom         VARCHAR(100) NOT NULL,  -- Inclut le type : "Terrain 1 — Couvert"
    actif       BOOLEAN      NOT NULL DEFAULT TRUE,
    id_commerce INT          NOT NULL,

    CONSTRAINT pk_terrain PRIMARY KEY (id_terrain),
    CONSTRAINT fk_terrain_commerce
        FOREIGN KEY (id_commerce)
        REFERENCES COMMERCE(id_commerce)
);


-- =============================================================
-- TABLE : TARIF
-- Rôle  : Grille tarifaire par terrain, jour ISO et plage
--         horaire. Permet des prix différents selon le jour
--         (semaine, week-end, dimanche) et l'heure (creuse/pleine).
-- =============================================================
CREATE TABLE TARIF (
    id_tarif     INT           NOT NULL AUTO_INCREMENT,
    type         VARCHAR(100)  NOT NULL,       -- Libellé lisible ex: "Soirée semaine"
    prix_heure   DECIMAL(10,2) NOT NULL,
    heure_debut  TIME          NOT NULL,
    heure_fin    TIME          NOT NULL,
    jour_semaine INT           NOT NULL,       -- ISO 8601 : 1=lundi … 7=dimanche
    id_terrain   INT           NOT NULL,

    CONSTRAINT pk_tarif PRIMARY KEY (id_tarif),
    CONSTRAINT fk_tarif_terrain
        FOREIGN KEY (id_terrain)
        REFERENCES TERRAIN(id_terrain),
    CONSTRAINT chk_tarif_heures
        CHECK (heure_fin > heure_debut),
    CONSTRAINT chk_tarif_jour
        CHECK (jour_semaine BETWEEN 1 AND 7),
    CONSTRAINT chk_tarif_prix
        CHECK (prix_heure > 0),

    INDEX idx_tarif_terrain (id_terrain)
);


-- =============================================================
-- TABLE : RESERVATION
-- Rôle  : Réservations de terrains de padel par les clients.
--         prix_paye est calculé et figé à l'insertion :
--         il dépend du tarif, du terrain et de la durée exacte.
--         Il reste exact même si les tarifs évoluent ensuite.
--         La vérification de chevauchement se fait dans une
--         transaction atomique côté backend (voir Use Cases).
-- =============================================================
CREATE TABLE RESERVATION (
    id_reservation   INT           NOT NULL AUTO_INCREMENT,
    date             DATE          NOT NULL,
    heure_debut      TIME          NOT NULL,
    heure_fin        TIME          NOT NULL,
    date_reservation DATETIME      NOT NULL DEFAULT NOW(),
    prix_paye        DECIMAL(10,2) NOT NULL,
    id_terrain       INT           NOT NULL,
    id_utilisateur   INT           NOT NULL,
    id_tarif         INT           NOT NULL,

    CONSTRAINT pk_reservation PRIMARY KEY (id_reservation),
    CONSTRAINT fk_resa_terrain
        FOREIGN KEY (id_terrain)
        REFERENCES TERRAIN(id_terrain),
    CONSTRAINT fk_resa_utilisateur
        FOREIGN KEY (id_utilisateur)
        REFERENCES UTILISATEUR(id_utilisateur),
    CONSTRAINT fk_resa_tarif
        FOREIGN KEY (id_tarif)
        REFERENCES TARIF(id_tarif),
    CONSTRAINT chk_resa_heures
        CHECK (heure_fin > heure_debut),
    CONSTRAINT chk_resa_prix
        CHECK (prix_paye >= 0),

    INDEX idx_resa_terrain       (id_terrain),
    INDEX idx_resa_utilisateur   (id_utilisateur),
    -- Index composite pour accélérer la détection de chevauchements
    INDEX idx_resa_terrain_date  (id_terrain, date)
);


-- =============================================================
-- TABLE : TRANSACTION_FIDELITE
-- Rôle  : Historique complet des mouvements de points.
--         Le champ points est TOUJOURS positif ; c'est
--         type_transaction qui indique le sens du mouvement.
--         Relation avec UTILISATEUR.points_solde :
--         points_solde = SUM(GAIN) - SUM(DEPENSE) - SUM(EXPIRATION)
--         id_commerce NULL = ajustement administratif global.
-- =============================================================
CREATE TABLE TRANSACTION_FIDELITE (
    id_transaction   INT           NOT NULL AUTO_INCREMENT,
    id_utilisateur   INT           NOT NULL,
    id_commerce      INT           NULL,       -- NULL = ajustement global
    points           DECIMAL(10,2) NOT NULL,   -- Toujours > 0
    type_transaction VARCHAR(50)   NOT NULL,
    description      VARCHAR(255)  NULL,
    date_transaction DATETIME      NOT NULL DEFAULT NOW(),

    CONSTRAINT pk_transaction PRIMARY KEY (id_transaction),
    CONSTRAINT fk_transac_utilisateur
        FOREIGN KEY (id_utilisateur)
        REFERENCES UTILISATEUR(id_utilisateur),
    CONSTRAINT fk_transac_commerce
        FOREIGN KEY (id_commerce)
        REFERENCES COMMERCE(id_commerce),
    CONSTRAINT chk_points_positifs
        CHECK (points > 0),
    CONSTRAINT chk_type_transaction
        CHECK (type_transaction IN ('GAIN', 'DEPENSE', 'EXPIRATION', 'AJUSTEMENT')),

    INDEX idx_transac_utilisateur (id_utilisateur),
    INDEX idx_transac_date        (date_transaction)
);