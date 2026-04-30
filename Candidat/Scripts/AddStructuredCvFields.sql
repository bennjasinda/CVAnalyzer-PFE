-- Migration: Add structured CV fields to Cv table
-- This script adds the new fields required for the structured CV form

-- Add all columns if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'NomCandidat')
BEGIN
    ALTER TABLE Cv ADD NomCandidat NVARCHAR(200) NULL;
    PRINT 'Added column: NomCandidat';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'Email')
BEGIN
    ALTER TABLE Cv ADD Email NVARCHAR(200) NULL;
    PRINT 'Added column: Email';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'Telephone')
BEGIN
    ALTER TABLE Cv ADD Telephone NVARCHAR(50) NULL;
    PRINT 'Added column: Telephone';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'Competences')
BEGIN
    ALTER TABLE Cv ADD Competences NVARCHAR(MAX) NULL;
    PRINT 'Added column: Competences';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'Experience')
BEGIN
    ALTER TABLE Cv ADD Experience NVARCHAR(MAX) NULL;
    PRINT 'Added column: Experience';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'NiveauEducation')
BEGIN
    ALTER TABLE Cv ADD NiveauEducation NVARCHAR(MAX) NULL;
    PRINT 'Added column: NiveauEducation';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'AutresInfos')
BEGIN
    ALTER TABLE Cv ADD AutresInfos NVARCHAR(MAX) NULL;
    PRINT 'Added column: AutresInfos';
END

-- Update existing NULL values to empty string for required fields (only if columns exist)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'NomCandidat')
BEGIN
    UPDATE Cv SET NomCandidat = '' WHERE NomCandidat IS NULL;
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cv' AND COLUMN_NAME = 'Email')
BEGIN
    UPDATE Cv SET Email = '' WHERE Email IS NULL;
END

PRINT 'Migration completed successfully: Structured CV fields added to Cv table.';
