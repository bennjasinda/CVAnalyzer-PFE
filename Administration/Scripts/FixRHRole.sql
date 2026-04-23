-- =============================================
-- Script: FixRHRole.sql
-- Description: Diagnostiquer et corriger le rôle RH
-- Date: 2026-04-23
-- =============================================

USE [YourDatabaseName];
GO

-- =============================================
-- ÉTAPE 1: DIAGNOSTIC
-- =============================================

PRINT '========================================';
PRINT 'DIAGNOSTIC DES RÔLES UTILISATEURS';
PRINT '========================================';
PRINT '';

-- Voir tous les rôles distincts
PRINT '1. Tous les rôles distincts dans la base:';
SELECT DISTINCT Role, COUNT(*) as NombreUtilisateurs
FROM Utilisateur
GROUP BY Role
ORDER BY Role;
PRINT '';

-- Voir les utilisateurs qui devraient être RH
PRINT '2. Utilisateurs avec rôle contenant "rh" (insensible à la casse):';
SELECT Id, NomUtilisateur, Email, Role, IsActive, DateCreation
FROM Utilisateur
WHERE LOWER(Role) LIKE '%rh%'
ORDER BY DateCreation DESC;
PRINT '';

-- Voir TOUS les utilisateurs (pour vérification)
PRINT '3. Tous les utilisateurs:';
SELECT Id, NomUtilisateur, Email, Role, IsActive, DateCreation
FROM Utilisateur
ORDER BY Role, DateCreation DESC;
PRINT '';

-- =============================================
-- ÉTAPE 2: CORRECTIONS
-- =============================================

PRINT '========================================';
PRINT 'CORRECTIONS';
PRINT '========================================';
PRINT '';

-- Correction 1: Standardiser "RH" en majuscules
PRINT '4. Correction des rôles "rh", "Rh", "rH" vers "RH":';
UPDATE Utilisateur 
SET Role = 'RH'
WHERE LOWER(Role) = 'rh' AND Role != 'RH';

DECLARE @rowsUpdated INT = @@ROWCOUNT;
PRINT CAST(@rowsUpdated AS VARCHAR) + ' ligne(s) corrigée(s) vers "RH"';
PRINT '';

-- Correction 2: Vérifier après correction
PRINT '5. Vérification après correction:';
SELECT Role, COUNT(*) as NombreUtilisateurs
FROM Utilisateur
GROUP BY Role
ORDER BY Role;
PRINT '';

-- =============================================
-- ÉTAPE 3: CRÉER UN COMPTE RH DE TEST (Optionnel)
-- =============================================

/*
-- DÉCOMMENTER POUR CRÉER UN COMPTE RH DE TEST
-- Mot de passe: Password123

PRINT '6. Création d''un compte RH de test:';

-- Vérifier si le compte existe déjà
IF NOT EXISTS (SELECT 1 FROM Utilisateur WHERE NomUtilisateur = 'RH_Test')
BEGIN
    INSERT INTO Utilisateur (
        NomUtilisateur, 
        Email, 
        MotPasse, 
        Role, 
        IsActive, 
        DateCreation
    )
    VALUES (
        'RH_Test',
        'rh.test@cvanalyzer.com',
        '$2a$11$tQZ8qQvQxQZ8qQvQxQZ8qOQxQZ8qQvQxQZ8qQvQxQZ8qQvQxQ', -- Hash BCrypt de 'Password123'
        'RH',
        1,
        GETDATE()
    );
    PRINT 'Compte RH_Test créé avec succès!';
    PRINT 'Email: rh.test@cvanalyzer.com';
    PRINT 'Mot de passe: Password123';
END
ELSE
BEGIN
    PRINT 'Le compte RH_Test existe déjà.';
END
PRINT '';
*/

-- =============================================
-- ÉTAPE 4: RÉSUMÉ FINAL
-- =============================================

PRINT '========================================';
PRINT 'RÉSUMÉ FINAL';
PRINT '========================================';
PRINT '';

PRINT 'Statistiques actuelles:';
SELECT 
    Role,
    COUNT(*) as Total,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as Actifs,
    SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) as Inactifs
FROM Utilisateur
GROUP BY Role
ORDER BY Role;

PRINT '';
PRINT '========================================';
PRINT 'TERMINÉ!';
PRINT '========================================';
PRINT '';
PRINT 'INSTRUCTIONS:';
PRINT '1. Vérifiez les résultats ci-dessus';
PRINT '2. Si vous avez corrigé des rôles, déconnectez-vous et reconnectez-vous';
PRINT '3. La sidebar RH devrait maintenant apparaître';
PRINT '';

GO
