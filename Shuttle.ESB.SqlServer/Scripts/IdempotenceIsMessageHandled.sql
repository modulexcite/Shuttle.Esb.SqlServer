if EXISTS (SELECT * FROM [dbo].[Idempotence] WHERE MessageId = @MessageId and MessageHandled = 1)
	select 1
else
	select 0
