-- Simple migration to fix FLOAT columns to REAL
-- This script uses ALTER COLUMN which doesn't require dropping constraints

-- Fix BonusScore
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'BonusScore' 
           AND DATA_TYPE = 'float' AND NUMERIC_PRECISION = 53)
BEGIN
    ALTER TABLE Match ALTER COLUMN BonusScore REAL NOT NULL;
    PRINT 'BonusScore changed to REAL';
END
ELSE
BEGIN
    PRINT 'BonusScore is already correct or does not exist';
END

-- Fix SkillsBonusScore
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'SkillsBonusScore' 
           AND DATA_TYPE = 'float' AND NUMERIC_PRECISION = 53)
BEGIN
    ALTER TABLE Match ALTER COLUMN SkillsBonusScore REAL NOT NULL;
    PRINT 'SkillsBonusScore changed to REAL';
END
ELSE
BEGIN
    PRINT 'SkillsBonusScore is already correct or does not exist';
END

-- Fix EducationBonusScore
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'EducationBonusScore' 
           AND DATA_TYPE = 'float' AND NUMERIC_PRECISION = 53)
BEGIN
    ALTER TABLE Match ALTER COLUMN EducationBonusScore REAL NOT NULL;
    PRINT 'EducationBonusScore changed to REAL';
END
ELSE
BEGIN
    PRINT 'EducationBonusScore is already correct or does not exist';
END

PRINT 'Done!';
