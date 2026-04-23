-- =============================================
-- Script: AddCandidateProfileFields.sql
-- Description: Add new fields to Utilisateur table for candidate profile
-- Date: 2026-04-23
-- =============================================

USE [YourDatabaseName];
GO

-- Add new columns to Utilisateur table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Utilisateur') AND name = 'Telephone')
BEGIN
    ALTER TABLE Utilisateur ADD Telephone NVARCHAR(50) NULL;
    PRINT 'Column Telephone added';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Utilisateur') AND name = 'Competences')
BEGIN
    ALTER TABLE Utilisateur ADD Competences NVARCHAR(MAX) NULL;
    PRINT 'Column Competences added';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Utilisateur') AND name = 'Experiences')
BEGIN
    ALTER TABLE Utilisateur ADD Experiences NVARCHAR(MAX) NULL;
    PRINT 'Column Experiences added';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Utilisateur') AND name = 'Diplomes')
BEGIN
    ALTER TABLE Utilisateur ADD Diplomes NVARCHAR(MAX) NULL;
    PRINT 'Column Diplomes added';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Utilisateur') AND name = 'GitLink')
BEGIN
    ALTER TABLE Utilisateur ADD GitLink NVARCHAR(500) NULL;
    PRINT 'Column GitLink added';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Utilisateur') AND name = 'LinkedInLink')
BEGIN
    ALTER TABLE Utilisateur ADD LinkedInLink NVARCHAR(500) NULL;
    PRINT 'Column LinkedInLink added';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Utilisateur') AND name = 'Bio')
BEGIN
    ALTER TABLE Utilisateur ADD Bio NVARCHAR(MAX) NULL;
    PRINT 'Column Bio added';
END
GO

PRINT '=========================================';
PRINT 'All candidate profile fields added successfully!';
PRINT '=========================================';
GO
