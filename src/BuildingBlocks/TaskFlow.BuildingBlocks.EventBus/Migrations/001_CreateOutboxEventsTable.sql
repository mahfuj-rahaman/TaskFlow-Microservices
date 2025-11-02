-- =====================================================================
-- Outbox Events Table Migration
-- Supports: PostgreSQL, SQL Server, MySQL
-- =====================================================================
-- Purpose: Creates the OutboxEvents table for the Outbox pattern
-- Features:
--   - Stores domain events persistently
--   - Supports at-least-once delivery guarantee
--   - Tracks publishing status and retry attempts
--   - Enables event sourcing and audit trail
-- =====================================================================

-- =====================================================================
-- POSTGRESQL
-- =====================================================================

-- PostgreSQL version
CREATE TABLE IF NOT EXISTS "OutboxEvents" (
    "Id" UUID PRIMARY KEY,
    "EventType" VARCHAR(500) NOT NULL,
    "EventData" TEXT NOT NULL,
    "AggregateId" UUID NULL,
    "AggregateType" VARCHAR(255) NULL,
    "OccurredAt" TIMESTAMP NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    "IsPublished" BOOLEAN NOT NULL DEFAULT FALSE,
    "PublishedAt" TIMESTAMP NULL,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "ErrorMessage" TEXT NULL,
    "IsFailed" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Indexes for PostgreSQL
CREATE INDEX IF NOT EXISTS "IX_OutboxEvents_IsPublished_IsFailed_CreatedAt"
    ON "OutboxEvents" ("IsPublished", "IsFailed", "CreatedAt");

CREATE INDEX IF NOT EXISTS "IX_OutboxEvents_AggregateId"
    ON "OutboxEvents" ("AggregateId")
    WHERE "AggregateId" IS NOT NULL;

CREATE INDEX IF NOT EXISTS "IX_OutboxEvents_EventType"
    ON "OutboxEvents" ("EventType");

CREATE INDEX IF NOT EXISTS "IX_OutboxEvents_OccurredAt"
    ON "OutboxEvents" ("OccurredAt");

-- =====================================================================
-- SQL SERVER
-- =====================================================================

-- SQL Server version (uncomment if using SQL Server)
/*
CREATE TABLE [dbo].[OutboxEvents] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EventType] NVARCHAR(500) NOT NULL,
    [EventData] NVARCHAR(MAX) NOT NULL,
    [AggregateId] UNIQUEIDENTIFIER NULL,
    [AggregateType] NVARCHAR(255) NULL,
    [OccurredAt] DATETIME2 NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsPublished] BIT NOT NULL DEFAULT 0,
    [PublishedAt] DATETIME2 NULL,
    [RetryCount] INT NOT NULL DEFAULT 0,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [IsFailed] BIT NOT NULL DEFAULT 0
);

-- Indexes for SQL Server
CREATE NONCLUSTERED INDEX [IX_OutboxEvents_IsPublished_IsFailed_CreatedAt]
    ON [dbo].[OutboxEvents] ([IsPublished], [IsFailed], [CreatedAt]);

CREATE NONCLUSTERED INDEX [IX_OutboxEvents_AggregateId]
    ON [dbo].[OutboxEvents] ([AggregateId])
    WHERE [AggregateId] IS NOT NULL;

CREATE NONCLUSTERED INDEX [IX_OutboxEvents_EventType]
    ON [dbo].[OutboxEvents] ([EventType]);

CREATE NONCLUSTERED INDEX [IX_OutboxEvents_OccurredAt]
    ON [dbo].[OutboxEvents] ([OccurredAt]);
*/

-- =====================================================================
-- MYSQL
-- =====================================================================

-- MySQL version (uncomment if using MySQL)
/*
CREATE TABLE IF NOT EXISTS `OutboxEvents` (
    `Id` CHAR(36) PRIMARY KEY,
    `EventType` VARCHAR(500) NOT NULL,
    `EventData` LONGTEXT NOT NULL,
    `AggregateId` CHAR(36) NULL,
    `AggregateType` VARCHAR(255) NULL,
    `OccurredAt` DATETIME NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    `IsPublished` BOOLEAN NOT NULL DEFAULT FALSE,
    `PublishedAt` DATETIME NULL,
    `RetryCount` INT NOT NULL DEFAULT 0,
    `ErrorMessage` TEXT NULL,
    `IsFailed` BOOLEAN NOT NULL DEFAULT FALSE,
    INDEX `IX_OutboxEvents_IsPublished_IsFailed_CreatedAt` (`IsPublished`, `IsFailed`, `CreatedAt`),
    INDEX `IX_OutboxEvents_AggregateId` (`AggregateId`),
    INDEX `IX_OutboxEvents_EventType` (`EventType`),
    INDEX `IX_OutboxEvents_OccurredAt` (`OccurredAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
*/

-- =====================================================================
-- VERIFICATION QUERIES
-- =====================================================================

-- Check if table was created successfully (PostgreSQL)
SELECT COUNT(*) as OutboxEventsTableExists
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name = 'OutboxEvents';

-- Check indexes (PostgreSQL)
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'OutboxEvents'
ORDER BY indexname;

-- =====================================================================
-- MAINTENANCE QUERIES
-- =====================================================================

-- Clean up old published events (run periodically, e.g., daily)
-- Keep events for 30 days for audit trail, then delete
/*
DELETE FROM "OutboxEvents"
WHERE "IsPublished" = TRUE
  AND "PublishedAt" < (NOW() - INTERVAL '30 days');
*/

-- Archive failed events older than 90 days
-- (Move to archive table before deleting if needed)
/*
DELETE FROM "OutboxEvents"
WHERE "IsFailed" = TRUE
  AND "CreatedAt" < (NOW() - INTERVAL '90 days');
*/

-- Reset failed events for retry (use with caution)
/*
UPDATE "OutboxEvents"
SET "IsFailed" = FALSE,
    "RetryCount" = 0,
    "ErrorMessage" = NULL
WHERE "IsFailed" = TRUE
  AND "RetryCount" < 5;
*/
