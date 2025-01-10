-- Create the database
CREATE DATABASE UWPMessagesDB;
GO

-- Use the database
USE UWPMessagesDB;
GO

-- Table to store messages
CREATE TABLE Message (
    Id INT IDENTITY(1,1) PRIMARY KEY,          -- Primary key
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(), -- Date and time the message was created
    ToField NVARCHAR(255) NOT NULL,            -- Phone Number
    Message NVARCHAR(MAX) NOT NULL             -- Content of the message
);

-- Table to log message sending events
CREATE TABLE MessageSendingLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,          -- Primary key
    MessageId INT NOT NULL,                    -- Foreign key referencing the Message table
    SentAt DATETIME NOT NULL DEFAULT GETDATE(), -- Date and time the message was sent
    TwilioConfirmationCode NVARCHAR(255) NULL, -- Confirmation code from Twilio
    CONSTRAINT FK_MessageSendingLog_Message FOREIGN KEY (MessageId) REFERENCES Message(Id) -- Foreign key constraint to message table
);

-- Table to store Twilio credentials
CREATE TABLE TwilioCredentials (
    Id INT IDENTITY(1,1) PRIMARY KEY,          -- Primary key
    Email NVARCHAR(255) NOT NULL,              -- Email associated with the Twilio account
    Password NVARCHAR(255) NOT NULL,           -- Password for the Twilio account (hashed and secured)
);



--Creating DB User

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'UserUWPMessages')
BEGIN
    CREATE LOGIN UserUWPMessages WITH PASSWORD = 'Wr12azqo+';
END

USE UWPMessagesDB;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'UserUWPMessages')
BEGIN
    CREATE USER UserUWPMessages FOR LOGIN UserUWPMessages;
    
    ALTER ROLE db_datareader ADD MEMBER UserUWPMessages;
    
    ALTER ROLE db_datawriter ADD MEMBER UserUWPMessages;
    
END

--Grant permision to execute SPs to DB User

USE UWPMessagesDB;
GO

GRANT EXECUTE ON SCHEMA::dbo TO UserUWPMessages;

