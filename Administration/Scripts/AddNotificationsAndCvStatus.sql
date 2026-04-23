-- Script d'ajout des tables/colonnes nécessaires pour les notifications et le statut des candidatures

-- 1. Ajout de la colonne Statut à la table Cv (si elle n'existe pas déjà)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'Statut'
)
BEGIN
    ALTER TABLE Cv ADD Statut NVARCHAR(50) NOT NULL DEFAULT 'En attente';
END
GO

-- 2. Creation de la table Notification (si elle n'existe pas déjà)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'Notification'
)
BEGIN
    CREATE TABLE Notification (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UtilisateurId INT NOT NULL,
        Titre NVARCHAR(255) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        Type NVARCHAR(50) NOT NULL DEFAULT 'Info',
        IsRead BIT NOT NULL DEFAULT 0,
        DateCreation DATETIME2 NOT NULL DEFAULT GETDATE(),
        RelatedCvId INT NULL,
        RelatedOffreId INT NULL
    );
END
GO
