-- ========================================
-- CREATE DATABASE USER FOR E-COMMERCE APP
-- ========================================
-- Run this script as SYSDBA in SQL*Plus or SQL Developer
-- ========================================

-- Create the user DB_PROJECT with password 1234
CREATE USER DB_PROJECT IDENTIFIED BY 1234
  DEFAULT TABLESPACE USERS
  TEMPORARY TABLESPACE TEMP
  QUOTA UNLIMITED ON USERS;

-- Grant necessary privileges
GRANT CONNECT, RESOURCE, CREATE VIEW, CREATE SEQUENCE TO DB_PROJECT;
GRANT CREATE SESSION TO DB_PROJECT;
GRANT CREATE TABLE TO DB_PROJECT;
GRANT CREATE PROCEDURE TO DB_PROJECT;
GRANT CREATE TRIGGER TO DB_PROJECT;

-- Grant DBA privileges (for development only - remove in production)
GRANT DBA TO DB_PROJECT;

COMMIT;

-- Verify user creation
SELECT username FROM dba_users WHERE username = 'DB_PROJECT';
