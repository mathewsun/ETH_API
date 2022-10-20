USE [ETH_API]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Address] [nvarchar](max) NOT NULL,
	[Label] [nvarchar](max) NOT NULL,
	[Value] [decimal](38, 20) NOT NULL,
	[State] [int] NOT NULL,
	[LastUpdateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TransactionETH](
	[Hash] [nvarchar](max) NOT NULL,
	[Nonce] [nvarchar](max) NULL,
	[TransactionIndex] [nvarchar](max) NULL,
	[FromAddress] [nvarchar](max) NOT NULL,
	[ToAddress] [nvarchar](max) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
	[Gas] [nvarchar](max) NOT NULL,
	[GasPrice] [nvarchar](max) NOT NULL,
	[Input] [nvarchar](max) NULL,
	[ReceiptCumulativeGasUsed] [nvarchar](max) NULL,
	[ReceiptGasUsed] [nvarchar](max) NULL,
	[ReceiptContractAddress] [nvarchar](max) NULL,
	[ReceiptRoot] [nvarchar](max) NULL,
	[ReceiptStatus] [nvarchar](max) NULL,
	[BlockTimestamp] [nvarchar](max) NULL,
	[BlockNumber] [nvarchar](max) NOT NULL,
	[BlockHash] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF_Accounts_LastUpdateDate]  DEFAULT (getdate()) FOR [LastUpdateDate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateAccount]
@address nvarchar(max),
@label nvarchar(max),
@value decimal(38,20) NULL,
@state int
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

END

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAccountByValue]
@value nvarchar(200)
AS

SELECT TOP(1) [Id]
			,[Address]
			,[Label]
			, CONVERT(decimal(38,12), [Value]) as Value
			,[State]
			,[LastUpdateDate]
FROM [Accounts]
WHERE [Value] >= @value
ORDER BY [Value]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLastTransaction]
@address nvarchar(200)
AS

SELECT TOP(1) * FROM TransactionETH
WHERE TransactionETH.ToAddress = @address
ORDER BY TransactionETH.BlockTimestamp desc
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertOrUpdateTransaction]
@hash nvarchar(200),
@nonce nvarchar(200),
@transactionIndex nvarchar(200),
@fromAddress nvarchar(200),
@toAddress nvarchar(200),
@value nvarchar(200),
@gas nvarchar(200),
@gasPrice nvarchar(200),
@input nvarchar(200),
@receiptCumulativeGasUsed nvarchar(200),
@receiptGasUsed nvarchar(200),
@receiptContractAddress nvarchar(200),
@receiptRoot nvarchar(200),
@receiptStatus nvarchar(200),
@blockTimestamp nvarchar(200),
@blockNumber nvarchar(200),
@blockHash nvarchar(200)
AS
BEGIN

BEGIN TRANSACTION; 
IF @@TRANCOUNT > 0

BEGIN
BEGIN TRY
	MERGE INTO [dbo].[TransactionETH] AS [Target]
	USING (
		VALUES 
		(@hash, @nonce, @transactionIndex, @fromAddress, @toAddress,
		@value, @gas, @gasPrice, @input, @receiptCumulativeGasUsed,
		@receiptGasUsed, @receiptContractAddress, @receiptRoot,
		@receiptStatus, @blockTimestamp, @blockNumber, @blockHash)
	) AS [Source] ([Hash], [Nonce], [TransactionIndex], [FromAddress], [ToAddress],
				[Value], [Gas], [GasPrice], [Input], [ReceiptCumulativeGasUsed],
				[ReceiptGasUsed], [ReceiptContractAddress], [ReceiptRoot],
				[ReceiptStatus], [BlockTimestamp], [BlockNumber], [BlockHash])
	ON ([Target].[Hash] = [Source].[Hash] AND [Target].[BlockNumber] = [Source].[BlockNumber] 
		AND [Target].[BlockHash] = [Source].[BlockHash])
	WHEN MATCHED THEN 
		UPDATE SET 
			[Target].[Nonce] = [Source].[Nonce],
			[Target].[TransactionIndex] = [Source].[TransactionIndex],
			[Target].[FromAddress] = [Source].[FromAddress],
			[Target].[Value] = [Source].[Value],
			[Target].[Gas] = [Source].[Gas],
			[Target].[Input] = [Source].[Input],
			[Target].[ReceiptCumulativeGasUsed] = [Source].[ReceiptCumulativeGasUsed],
			[Target].[ReceiptGasUsed] = [Source].[ReceiptGasUsed],
			[Target].[ReceiptContractAddress] = [Source].[ReceiptContractAddress],
			[Target].[ReceiptRoot] = [Source].[ReceiptRoot],
			[Target].[ReceiptStatus] = [Source].[ReceiptStatus],
			[Target].[BlockTimestamp] = [Source].[BlockTimestamp]
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([Hash], [Nonce], [TransactionIndex], [FromAddress], [ToAddress],
				[Value], [Gas], [GasPrice], [Input], [ReceiptCumulativeGasUsed],
				[ReceiptGasUsed], [ReceiptContractAddress], [ReceiptRoot],
				[ReceiptStatus], [BlockTimestamp], [BlockNumber], [BlockHash])

		Values([Source].[Hash], [Source].[Nonce], [Source].[TransactionIndex], [Source].[FromAddress], [Source].[ToAddress],
				[Source].[Value], [Source].[Gas], [Source].[GasPrice], [Source].[Input], [Source].[ReceiptCumulativeGasUsed],
				[Source].[ReceiptGasUsed], [Source].[ReceiptContractAddress], [Source].[ReceiptRoot],
				[Source].[ReceiptStatus], [Source].[BlockTimestamp], [Source].[BlockNumber], [Source].[BlockHash])
	OUTPUT $action AS ActionType;



END TRY
BEGIN CATCH
DECLARE @Error NVARCHAR(4000)
SET @Error = ERROR_MESSAGE()
RAISERROR(@Error, 11, 1)
ROLLBACK TRANSACTION
END CATCH
END

IF @@TRANCOUNT > 0
BEGIN
PRINT 'The database update succeeded.'
COMMIT TRANSACTION
END
ELSE
BEGIN
RAISERROR('The database update failed.', 11, 1)
END

END





GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateValueAccount]
@address nvarchar(max),
@value decimal(38,20) NULL
AS
BEGIN
	UPDATE Accounts
	SET 
		Value = @value,
		LastUpdateDate = GETDATE()
	WHERE Address = @address;

END

GO
