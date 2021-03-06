﻿--
-- Script was generated by Devart dbForge Studio 2019 for MySQL, Version 8.2.23.0
-- Product home page: http://www.devart.com/dbforge/mysql/studio
-- Script date 1/02/2021 3:30:55 PM
-- Server version: 5.7.33
-- Client version: 4.1
--

-- 
-- Disable foreign keys
-- 
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;

-- 
-- Set SQL mode
-- 
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- 
-- Set character set the client will use to send SQL statements to the server
--
SET NAMES 'utf8';

--
-- Set default database
--
USE routinizedb;

--
-- Drop table `tokens`
--
DROP TABLE IF EXISTS tokens;

--
-- Drop table `userphotos`
--
DROP TABLE IF EXISTS userphotos;

--
-- Drop table `photos`
--
DROP TABLE IF EXISTS photos;

--
-- Set default database
--
USE routinizedb;

--
-- Create table `photos`
--
CREATE TABLE photos (
  Id INT(11) NOT NULL AUTO_INCREMENT,
  PhotoName VARCHAR(150) DEFAULT NULL,
  Location VARCHAR(150) DEFAULT NULL,
  PRIMARY KEY (Id)
)
ENGINE = INNODB,
AUTO_INCREMENT = 2,
AVG_ROW_LENGTH = 16384,
CHARACTER SET utf8mb4,
COLLATE utf8mb4_general_ci,
ROW_FORMAT = COMPACT;

--
-- Create table `userphotos`
--
CREATE TABLE userphotos (
  Id INT(11) NOT NULL AUTO_INCREMENT,
  PhotoId INT(11) DEFAULT NULL,
  HidrogenianId INT(11) DEFAULT NULL,
  IsAvatar TINYINT(1) DEFAULT 0,
  IsCover TINYINT(1) DEFAULT 0,
  PRIMARY KEY (Id)
)
ENGINE = INNODB,
AUTO_INCREMENT = 2,
CHARACTER SET utf8mb4,
COLLATE utf8mb4_general_ci,
ROW_FORMAT = COMPACT;

--
-- Create foreign key
--
ALTER TABLE userphotos 
  ADD CONSTRAINT userphotos_ibfk_1 FOREIGN KEY (PhotoId)
    REFERENCES photos(Id) ON DELETE CASCADE;

--
-- Create table `tokens`
--
CREATE TABLE tokens (
  TokenId INT(11) NOT NULL AUTO_INCREMENT,
  TokenString VARCHAR(100) NOT NULL,
  TimeStamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  Life TINYINT(4) DEFAULT 5,
  Target VARCHAR(70) DEFAULT NULL,
  PRIMARY KEY (TokenId)
)
ENGINE = INNODB,
AUTO_INCREMENT = 154,
AVG_ROW_LENGTH = 8192,
CHARACTER SET utf8mb4,
COLLATE utf8mb4_general_ci,
ROW_FORMAT = COMPACT;

-- 
-- Restore previous SQL mode
-- 
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;

-- 
-- Enable foreign keys
-- 
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;