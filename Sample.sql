USE [master]
GO
/****** Object:  Database [gamedb]    Script Date: 26-05-2023 12:56:01 ******/
CREATE DATABASE [gamedb]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'gamedb', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\gamedb.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'gamedb_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\gamedb_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [gamedb] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [gamedb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [gamedb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [gamedb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [gamedb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [gamedb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [gamedb] SET ARITHABORT OFF 
GO
ALTER DATABASE [gamedb] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [gamedb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [gamedb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [gamedb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [gamedb] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [gamedb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [gamedb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [gamedb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [gamedb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [gamedb] SET  DISABLE_BROKER 
GO
ALTER DATABASE [gamedb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [gamedb] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [gamedb] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [gamedb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [gamedb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [gamedb] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [gamedb] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [gamedb] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [gamedb] SET  MULTI_USER 
GO
ALTER DATABASE [gamedb] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [gamedb] SET DB_CHAINING OFF 
GO
ALTER DATABASE [gamedb] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [gamedb] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [gamedb] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [gamedb] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [gamedb] SET QUERY_STORE = OFF
GO
USE [gamedb]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 26-05-2023 12:56:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Persons_]    Script Date: 26-05-2023 12:56:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Persons_](
	[PersonId] [uniqueidentifier] NOT NULL,
	[LastName] [nvarchar](max) NULL,
	[FirstName] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[City] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
 CONSTRAINT [PK_Persons_] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20230525164241_Initial', N'5.0.17')
GO
INSERT [dbo].[Persons_] ([PersonId], [LastName], [FirstName], [Address], [City], [Email]) VALUES (N'3fa85f64-5717-4562-b3fc-2c963f66afa9', N'Williams', N'Emily', N'321 Pine St', N'San Francisco', N'emilywilliams@example.com')
INSERT [dbo].[Persons_] ([PersonId], [LastName], [FirstName], [Address], [City], [Email]) VALUES (N'3fa85f64-5717-4562-b3fc-2c963f66afaa', N'Brown', N'David', N'654 Cedar St', N'Seattle', N'davidbrown@example.com')
INSERT [dbo].[Persons_] ([PersonId], [LastName], [FirstName], [Address], [City], [Email]) VALUES (N'3fa85f64-5717-4562-b3fc-2c963f66afab', N'Miller', N'Olivia', N'987 Maple St', N'Boston', N'oliviamiller@example.com')
INSERT [dbo].[Persons_] ([PersonId], [LastName], [FirstName], [Address], [City], [Email]) VALUES (N'3fa85f64-5717-4562-b3fc-2c963f66afac', N'Jones', N'Daniel', N'135 Walnut St', N'Houston', N'danieljones@example.com')
INSERT [dbo].[Persons_] ([PersonId], [LastName], [FirstName], [Address], [City], [Email]) VALUES (N'3fa85f64-5717-4562-b3fc-2c963f66afad', N'Taylor', N'Sophia', N'246 Birch St', N'Miami', N'sophiataylor@example.com')
INSERT [dbo].[Persons_] ([PersonId], [LastName], [FirstName], [Address], [City], [Email]) VALUES (N'3fa85f64-5717-4562-b3fc-2c963f66afae', N'Davis', N'James', N'753 Oak St', N'Denver', N'jamesdavis@example.com')
GO
/****** Object:  StoredProcedure [dbo].[SelectAllPersons]    Script Date: 26-05-2023 12:56:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SelectAllPersons]
	
AS
BEGIN
SELECT * FROM Persons_;
END
GO
/****** Object:  StoredProcedure [dbo].[SelectSpecificPerson]    Script Date: 26-05-2023 12:56:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SelectSpecificPerson]
@personId uniqueidentifier 
	
AS
BEGIN
SELECT * FROM Persons_ WHERE PersonId = @personId;
END
GO
USE [master]
GO
ALTER DATABASE [gamedb] SET  READ_WRITE 
GO
