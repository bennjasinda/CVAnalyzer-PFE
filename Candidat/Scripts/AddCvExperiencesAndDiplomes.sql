-- Migration: Add structured CV experience and diploma tables
-- This script creates tables for storing detailed CV experiences and diplomas

-- Create CvExperiences table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CvExperiences')
BEGIN
    CREATE TABLE CvExperiences (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CvId INT NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        Company NVARCHAR(500) NULL,
        Position NVARCHAR(500) NULL,
        StartDate DATETIME2 NULL,
        EndDate DATETIME2 NULL,
        IsCurrent BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_CvExperiences_Cv FOREIGN KEY (CvId) REFERENCES Cv(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_CvExperiences_CvId ON CvExperiences(CvId);
    
    PRINT 'Table CvExperiences created successfully.';
END
ELSE
BEGIN
    PRINT 'Table CvExperiences already exists.';
END

-- Create CvDiplomes table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CvDiplomes')
BEGIN
    CREATE TABLE CvDiplomes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CvId INT NOT NULL,
        Designation NVARCHAR(500) NOT NULL,
        Institution NVARCHAR(500) NULL,
        Field NVARCHAR(500) NULL,
        YearObtained INT NULL,
        Mention NVARCHAR(200) NULL,
        CONSTRAINT FK_CvDiplomes_Cv FOREIGN KEY (CvId) REFERENCES Cv(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_CvDiplomes_CvId ON CvDiplomes(CvId);
    
    PRINT 'Table CvDiplomes created successfully.';
END
ELSE
BEGIN
    PRINT 'Table CvDiplomes already exists.';
END

-- Add bonus score columns to Match table if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'BonusScore')
BEGIN
    ALTER TABLE Match ADD BonusScore REAL NOT NULL DEFAULT 0;
    PRINT 'Column BonusScore added to Match table.';
END
ELSE
BEGIN
    PRINT 'Column BonusScore already exists in Match table.';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'SkillsBonusScore')
BEGIN
    ALTER TABLE Match ADD SkillsBonusScore REAL NOT NULL DEFAULT 0;
    PRINT 'Column SkillsBonusScore added to Match table.';
END
ELSE
BEGIN
    PRINT 'Column SkillsBonusScore already exists in Match table.';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'EducationBonusScore')
BEGIN
    ALTER TABLE Match ADD EducationBonusScore REAL NOT NULL DEFAULT 0;
    PRINT 'Column EducationBonusScore added to Match table.';
END
ELSE
BEGIN
    PRINT 'Column EducationBonusScore already exists in Match table.';
END

PRINT 'Migration completed successfully: Enhanced CV structure and bonus scoring added.';
