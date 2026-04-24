-- Force fix: Drop constraints, then alter columns
SET NOCOUNT ON;

-- Drop ALL default constraints on Match table
DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql = @sql + 'ALTER TABLE Match DROP CONSTRAINT [' + d.name + '];' + CHAR(13)
FROM sys.default_constraints d
INNER JOIN sys.columns c ON d.parent_column_id = c.column_id
WHERE d.parent_object_id = OBJECT_ID('Match')
AND c.name IN ('BonusScore', 'SkillsBonusScore', 'EducationBonusScore');

-- Execute constraint drops
IF LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql;
    PRINT 'Dropped default constraints';
END

-- Now alter columns
ALTER TABLE Match ALTER COLUMN BonusScore REAL NOT NULL;
PRINT 'BonusScore -> REAL';

ALTER TABLE Match ALTER COLUMN SkillsBonusScore REAL NOT NULL;
PRINT 'SkillsBonusScore -> REAL';

ALTER TABLE Match ALTER COLUMN EducationBonusScore REAL NOT NULL;
PRINT 'EducationBonusScore -> REAL';

PRINT 'All bonus columns fixed!';
