-- TaskFlow Microservices Database Initialization Script
-- This script creates all necessary databases for the microservices architecture

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create Task Service Database
SELECT 'CREATE DATABASE taskflow_task'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'taskflow_task')\gexec

-- Create User Service Database (for future implementation)
SELECT 'CREATE DATABASE taskflow_user'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'taskflow_user')\gexec

-- Create Notification Service Database (for future implementation)
SELECT 'CREATE DATABASE taskflow_notification'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'taskflow_notification')\gexec

-- Create Audit Service Database (for future implementation)
SELECT 'CREATE DATABASE taskflow_audit'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'taskflow_audit')\gexec

-- Connect to Task database and create schema
\c taskflow_task

-- Create schema for task service
CREATE SCHEMA IF NOT EXISTS task_schema;

-- Grant privileges
GRANT ALL PRIVILEGES ON SCHEMA task_schema TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA task_schema TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA task_schema TO postgres;

-- Create indexes for better performance (will be created by EF Core migrations, but listed here for reference)
-- CREATE INDEX IF NOT EXISTS idx_tasks_user_id ON task_schema.tasks(user_id);
-- CREATE INDEX IF NOT EXISTS idx_tasks_status ON task_schema.tasks(status);
-- CREATE INDEX IF NOT EXISTS idx_tasks_due_date ON task_schema.tasks(due_date);
-- CREATE INDEX IF NOT EXISTS idx_tasks_created_at ON task_schema.tasks(created_at);

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'TaskFlow databases initialized successfully at %', NOW();
END $$;
