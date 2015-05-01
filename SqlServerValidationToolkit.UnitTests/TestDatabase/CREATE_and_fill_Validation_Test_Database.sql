/*============================================================================
  File:     CREATE_and_fill_Validation_Test_Database.sql

  Summary:  Creates and fills the Database used for testing the
			Validation Toolkit

  Date:     August 17, 2011
------------------------------------------------------------------------------
  
  Copyright (c) 2011, Simon Gubler
  All rights reserved.

  Redistribution and use, with or without modification, are permitted provided that the following conditions are met:

  Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
  
  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.
============================================================================*/


---------------------------------------------------------------ViewsToCheck------------------------------------------------------------------


--USE master;
--GO
------------------------------------------------------------------------------------------------------------------------------------------------
---- Database TestDbValidation
------------------------------------------------------------------------------------------------------------------------------------------------
--USE [master]
--GO

--IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TestDbValidation')
--BEGIN
--CREATE DATABASE [TestDbValidation] 
--COLLATE SQL_Latin1_General_CP1_CI_AS
--END
--GO

--ALTER DATABASE [TestDbValidation] SET COMPATIBILITY_LEVEL = 100
--GO

--USE [TestDbValidation]
--GO

----------------------------------------------------------------------------------------------------------------------------------------------
-- Babies
----------------------------------------------------------------------------------------------------------------------------------------------
CREATE TABLE [dbo].[Babies](
	BabyID [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
	[Hospital_entry] datetime,
	[Birth_date] datetime,
	[Length] numeric (10,2),
	[Weight] int,
	[Email] varchar(MAX)
) ON [PRIMARY]


GO

CREATE VIEW ViewBabies
AS SELECT * FROM dbo.Babies

GO

INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight], [Email]) VALUES ('08.24.2011 15:34','08.25.2011 16:45',30.4,3500,'a@mail.com');
INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight], [Email]) VALUES ('08.24.2011 15:34',NULL,-10,3700,'b@mail.com');
INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight], [Email]) VALUES (NULL,'08.25.2011 16:45',35.0,0,'wrong value');

--Hospital-entry after birth
INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight], [Email]) VALUES ('08.26.2011 15:34','08.25.2011 16:45',0,-100,'d@mail.com');

--birth two days after Hospital-entry
INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight], [Email]) VALUES ('08.24.2011 15:34','08.26.2011 16:45',30.4,3500,'a@mail.com');

--Additional values
--INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight]) VALUES ('08.24.2011 15:34','08.25.2011 16:45',30.4,3500);
--INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight]) VALUES ('08.24.2011 15:34','08.25.2011 16:45',-10,3700);
--INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight]) VALUES ('08.24.2011 15:34','08.25.2011 16:45',35.0,0);
--INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight]) VALUES ('08.24.2011 15:34','08.25.2011 16:45',0,-100);
--INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight]) VALUES ('08.24.2011 15:34','08.25.2011 16:45',NULL,6000);
--INSERT INTO Babies ([Hospital_entry], [Birth_date], [Length], [Weight]) VALUES ('08.24.2011 15:34','08.25.2011 16:45',70,NULL);

GO

CREATE TABLE [dbo].[TestColumns](
	[CBigInt] [bigint] NULL,
	[CBinary50] [binary](50) NULL,
	[CBit] [bit] NULL,
	[CDate] [date] NULL,
	[CDateTime2] [datetime2](7) NULL,
	[CDateTimeOffset] [datetimeoffset](7) NULL,
	[CGeography] [geography] NULL,
	[CGeometry] [geometry] NULL,
	[CHieraryId] [hierarchyid] NULL,
	[CImage] [image] NULL,
	[CXML] [xml] NULL,
	[CUniqueId] [uniqueidentifier] NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[varchar] [varchar](max) NULL,
	[nchar] [nchar](10) NULL,
	[nvarchar] [nvarchar](max) NULL,
	[char] [char](10) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
