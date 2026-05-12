/*
  RBAC: Roles, Permissions, RolePermissions, Users.RoleId, password column widen.
  Idempotent-ish: safe to review before running against MicroLoanDB.
*/

SET NOCOUNT ON;

BEGIN TRANSACTION;

--- 1. Core tables ---
IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Permissions (
        Id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100)   NOT NULL
    );
    CREATE UNIQUE INDEX UX_Permissions_Name ON dbo.Permissions ([Name]);
END;

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        Id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100)   NOT NULL
    );
    CREATE UNIQUE INDEX UX_Roles_Name ON dbo.Roles ([Name]);
END;

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions (
        RoleId       INT NOT NULL,
        PermissionId INT NOT NULL,
        CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
        CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId)
            REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId)
            REFERENCES dbo.Permissions(Id) ON DELETE CASCADE
    );
END;

--- 2. Seed Roles (consistent with EF HasData) ---
SET IDENTITY_INSERT dbo.Roles ON;
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Id = 1)
    INSERT INTO dbo.Roles (Id, [Name]) VALUES (1, N'Admin');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Id = 2)
    INSERT INTO dbo.Roles (Id, [Name]) VALUES (2, N'Staff');
SET IDENTITY_INSERT dbo.Roles OFF;

--- 3. Seed Permissions ---
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions)
BEGIN
    SET IDENTITY_INSERT dbo.Permissions ON;
    INSERT INTO dbo.Permissions (Id, [Name]) VALUES
    (1,N'Loan_Read'),(2,N'Loan_Create'),(3,N'Loan_Approve'),(4,N'Loan_Repay'),
    (5,N'Loan_Update'),(6,N'Loan_Delete'),(7,N'User_Read'),(8,N'User_Create'),
    (9,N'User_Update'),(10,N'User_Delete'),(11,N'User_AssignRole'),
    (12,N'Role_Read'),(13,N'Role_Create'),(14,N'Role_Update'),(15,N'Role_Delete'),
    (16,N'Role_AssignPermissions'),(17,N'Permission_Read'),(18,N'Permission_Create'),
    (19,N'Permission_Update'),(20,N'Permission_Delete');
    SET IDENTITY_INSERT dbo.Permissions OFF;
END;

--- 4. Seed RolePermissions (Admin=all, Staff=Loan_Read) ---
IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 1)
BEGIN
    ;WITH n AS (
        SELECT v FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),
                          (11),(12),(13),(14),(15),(16),(17),(18),(19),(20)) AS x(v))
    INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
    SELECT 1, v FROM n;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 2 AND PermissionId = 1)
    INSERT INTO dbo.RolePermissions (RoleId, PermissionId) VALUES (2, 1);

--- 5. Users: RoleId ---
IF COL_LENGTH(N'dbo.Users', N'RoleId') IS NULL
    ALTER TABLE dbo.Users ADD RoleId INT NULL;

IF COL_LENGTH(N'dbo.Users', N'Role') IS NOT NULL
BEGIN
    UPDATE dbo.Users
    SET RoleId =
        CASE
            WHEN Role = N'Admin' THEN 1
            WHEN Role = N'Staff' THEN 2
            ELSE 2
        END;

    ALTER TABLE dbo.Users DROP COLUMN Role;
END;

UPDATE dbo.Users SET RoleId = 2 WHERE RoleId IS NULL;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE RoleId NOT IN (SELECT Id FROM dbo.Roles))
BEGIN
    THROW 50001, N'Cannot assign Roles: dbo.Users.RoleId references missing role.', 1;
END;

DECLARE @FkUsersRolesExists BIT = CASE WHEN EXISTS (
    SELECT 1
    FROM sys.foreign_keys AS fk
    WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Users')
      AND fk.name = N'FK_Users_Roles')
    THEN 1 ELSE 0 END;

IF @FkUsersRolesExists = 0
BEGIN
    ALTER TABLE dbo.Users WITH NOCHECK
    ADD CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId)
        REFERENCES dbo.Roles(Id);
END;

ALTER TABLE dbo.Users WITH CHECK CHECK CONSTRAINT FK_Users_Roles;

ALTER TABLE dbo.Users ALTER COLUMN RoleId INT NOT NULL;

ALTER TABLE dbo.Users ALTER COLUMN [Password] NVARCHAR(512) NOT NULL;

COMMIT TRANSACTION;
