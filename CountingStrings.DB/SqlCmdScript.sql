CREATE DATABASE CountingStrings;
GO
USE CountingStrings;
CREATE TABLE Sessions (Id uniqueidentifier Primary Key Not Null, DateCreated DateTime Not Null);
CREATE TABLE SessionCounts (Id uniqueidentifier Primary Key Not Null, NumOpen int, NumClose int);
GO
INSERT INTO SessionCounts (Id, NumOpen, NumClose) VALUES ('A9EF096F-9FB5-4408-B3FB-FF7F577D7C80', 0, 0);
GO

