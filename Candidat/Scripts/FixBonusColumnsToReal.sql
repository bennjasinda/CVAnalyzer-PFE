-- Migration: Fix Bonus Score Columns Type from FLOAT to REAL
-- This script changes the bonus columns from FLOAT (double) to REAL (float) to match existing score columns

-- Fix BonusScore column
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'BonusScore'
           AND DATA_TYPE = 'float' AND NUMERIC_PRECISION = 53)
BEGIN
    -- Drop default constraint if exists
    DECLARE @BonusScoreConstraint NVARCHAR(200);
    SELECT @BonusScoreConstraint = d.name 
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_column_id = c.column_id
    WHERE d.parent_object_id = OBJECT_ID('Match') 
    AND c.name = 'BonusScore';
    
    IF @BonusScoreConstraint IS NOT NULL
        EXEC('ALTER TABLE Match DROP CONSTRAINT ' + @BonusScoreConstraint);
    
    -- Drop and recreate with REAL type
    ALTER TABLE Match DROP COLUMN BonusScore;
    ALTER TABLE Match ADD BonusScore REAL NOT NULL DEFAULT 0;
    PRINT 'Column BonusScore changed from FLOAT to REAL.';
END
ELSE IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'BonusScore')
BEGIN
    ALTER TABLE Match ADD BonusScore REAL NOT NULL DEFAULT 0;
    PRINT 'Column BonusScore created as REAL.';
END
ELSE
BEGIN
    PRINT 'Column BonusScore is already REAL type.';
END

-- Fix SkillsBonusScore column
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'SkillsBonusScore')
BEGIN
    -- Check if it's currently FLOAT (53) and needs to be changed to REAL
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Match' 
               AND COLUMN_NAME = 'SkillsBonusScore' 
               AND DATA_TYPE = 'float' 
               AND NUMERIC_PRECISION = 53)
    BEGIN
        -- Drop and recreate with REAL type
        ALTER TABLE Match DROP COLUMN SkillsBonusScore;
        ALTER TABLE Match ADD SkillsBonusScore REAL NOT NULL DEFAULT 0;
        PRINT 'Column SkillsBonusScore changed from FLOAT to REAL.';
    END
    ELSE
    BEGIN
        PRINT 'Column SkillsBonusScore is already REAL type.';
    END
END
ELSE
BEGIN
    ALTER TABLE Match ADD SkillsBonusScore REAL NOT NULL DEFAULT 0;
    PRINT 'Column SkillsBonusScore created as REAL.';
END

-- Fix EducationBonusScore column
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Match' AND COLUMN_NAME = 'EducationBonusScore')
BEGIN
    -- Check if it's currently FLOAT (53) and needs to be changed to REAL
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Match' 
               AND COLUMN_NAME = 'EducationBonusScore' 
               AND DATA_TYPE = 'float' 
               AND NUMERIC_PRECISION = 53)
    BEGIN
        -- Drop and recreate with REAL type
        ALTER TABLE Match DROP COLUMN EducationBonusScore;
        ALTER TABLE Match ADD EducationBonusScore REAL NOT NULL DEFAULT 0;
        PRINT 'Column EducationBonusScore changed from FLOAT to REAL.';
    END
    ELSE
    BEGIN
        PRINT 'Column EducationBonusScore is already REAL type.';
    END
END
ELSE
BEGIN
    ALTER TABLE Match ADD EducationBonusScore REAL NOT NULL DEFAULT 0;
    PRINT 'Column EducationBonusScore created as REAL.';
END

PRINT 'Migration completed: All bonus score columns are now REAL type (matching existing score columns).';
