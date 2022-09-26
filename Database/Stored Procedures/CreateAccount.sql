USE [ETH_API]
GO
/****** Object:  StoredProcedure [dbo].[CreateEvent]    Script Date: 9/26/2022 19:57:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateAccount]
@address nvarchar(max),
@label nvarchar(max),
@value decimal(38,20) NULL,
@state int,
@new_identity INT OUTPUT
AS
BEGIN
INSERT INTO [dbo].[Accounts]
           ([Address]
           ,[Label]
           ,[Value]
           ,[State])
VALUES (@address
		,@label
		,@value
		,@state)

SET @new_identity = SCOPE_IDENTITY()

END

