-- Write Database Schema (Command Side)

-- Create schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'wirex')
BEGIN
    EXEC('CREATE SCHEMA wirex')
END
GO

-- Event Store Table
CREATE TABLE wirex.EventStore (
    EventId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AggregateId UNIQUEIDENTIFIER NOT NULL,
    AggregateType NVARCHAR(255) NOT NULL,
    EventType NVARCHAR(255) NOT NULL,
    EventData NVARCHAR(MAX) NOT NULL,
    EventVersion INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255) NULL,

    CONSTRAINT UQ_EventStore_AggregateVersion UNIQUE (AggregateId, EventVersion),
    INDEX IX_EventStore_AggregateId (AggregateId),
    INDEX IX_EventStore_AggregateType (AggregateType),
    INDEX IX_EventStore_CreatedAt (CreatedAt)
)
GO

-- Payment Write Model 
CREATE TABLE wirex.Payments (
    PaymentId UNIQUEIDENTIFIER PRIMARY KEY,
    UserAccountId UNIQUEIDENTIFIER NOT NULL,
    SourceCurrency NVARCHAR(10) NOT NULL,
    TargetCurrency NVARCHAR(10) NOT NULL,
    SourceValue DECIMAL(18, 2) NOT NULL,
    TargetValue DECIMAL(18, 2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreateDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsRemoved BIT NOT NULL DEFAULT 0,
    IsEmailNotificationSent BIT NOT NULL DEFAULT 0,
    Version INT NOT NULL DEFAULT 0,

    INDEX IX_Payments_UserAccountId (UserAccountId),
    INDEX IX_Payments_Status (Status),
    INDEX IX_Payments_CreateDate (CreateDate)
)
GO

-- User Accounts Write Model
CREATE TABLE wirex.UserAccounts (
    UserAccountId UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Balance DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Currency NVARCHAR(10) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastModifiedDate DATETIME2 NULL,
    Version INT NOT NULL DEFAULT 0,

    INDEX IX_UserAccounts_UserId (UserId),
    INDEX IX_UserAccounts_Currency (Currency)
)
GO

-- Bonus Accounts Write Model
CREATE TABLE wirex.BonusAccounts (
    BonusAccountId UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    BonusPoints DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    ExpiryDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastModifiedDate DATETIME2 NULL,
    Version INT NOT NULL DEFAULT 0,

    INDEX IX_BonusAccounts_UserId (UserId),
    INDEX IX_BonusAccounts_ExpiryDate (ExpiryDate)
)
GO

-- Users Write Model
CREATE TABLE wirex.Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Address NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastModifiedDate DATETIME2 NULL,
    Version INT NOT NULL DEFAULT 0,

    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    INDEX IX_Users_Email (Email)
)
GO

-- Outbox Pattern for reliable messaging
CREATE TABLE wirex.OutboxMessages (
    OutboxMessageId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AggregateId UNIQUEIDENTIFIER NOT NULL,
    EventType NVARCHAR(255) NOT NULL,
    EventData NVARCHAR(MAX) NOT NULL,
    KafkaTopic NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ProcessedAt DATETIME2 NULL,
    ProcessedStatus NVARCHAR(50) NULL, -- Pending, Published, Failed
    RetryCount INT NOT NULL DEFAULT 0,
    LastError NVARCHAR(MAX) NULL,

    INDEX IX_OutboxMessages_ProcessedStatus (ProcessedStatus),
    INDEX IX_OutboxMessages_CreatedAt (CreatedAt)
)
GO

PRINT 'Write database schema created successfully'
GO
