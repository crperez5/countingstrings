CREATE DATABASE CountingStrings;
GO
USE CountingStrings;
CREATE TABLE Sessions (
    Id uniqueidentifier Not Null Primary Key, 
    [Status] int Default 1, 
    DateCreated DateTime Not Null);

CREATE TABLE SessionCounts (
    Id uniqueidentifier Default NEWSEQUENTIALID() Primary Key, 
    NumOpen int, 
    NumClose int);
GO
IF NOT EXISTS (SELECT * FROM SessionCounts)
    INSERT INTO SessionCounts (NumOpen, NumClose) VALUES (0, 0);
GO

CREATE TABLE SessionWords (
    Id uniqueidentifier Default NEWSEQUENTIALID() Primary Key, 
    SessionId uniqueidentifier Not Null,
    Word nvarchar(450) Not Null,    
    DateCreated DateTime Default GETUTCDATE()
)
GO

CREATE NONCLUSTERED INDEX IX_SessionWords_SessionId ON SessionWords (SessionId);
GO

CREATE TABLE SessionWordCounts (
    Id uniqueidentifier Default NEWSEQUENTIALID() Primary Key, 
    SessionId uniqueidentifier Not Null,
    Word nvarchar(450) Not Null,
    Count int Not Null,
    DateCreated Datetime Default GETUTCDATE()
)
GO

CREATE NONCLUSTERED INDEX IX_SessionWordCounts_SessionWord
ON SessionWordCounts (SessionId, Word);
GO

CREATE NONCLUSTERED INDEX IX_SessionWordCounts_DateCreated
ON SessionWordCounts (DateCreated DESC);
GO

CREATE TABLE WordDateCounts (
    Id uniqueidentifier Default NEWSEQUENTIALID() Primary Key, 
    Word nvarchar(450) Not Null,
    [Date] Datetime Not Null,
    Count int Not Null,
    DateCreated Datetime Default GETUTCDATE()
)
GO

CREATE NONCLUSTERED INDEX IX_WordDateCounts_WordDate
ON WordDateCounts (Word, [Date]);
GO

CREATE TABLE WorkerJobs (
    Id uniqueidentifier Default NEWSEQUENTIALID() Primary Key, 
    ProcessId int Not Null,
    StartDate DateTime Not Null,
    EndDate DateTime Null
)
GO

CREATE TABLE RequestCount (
    Id uniqueidentifier Default NEWSEQUENTIALID() Primary Key, 
    [Count] int);
GO
IF NOT EXISTS (SELECT * FROM RequestCount)
    INSERT INTO RequestCount ([Count]) VALUES (0);
GO

