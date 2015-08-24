if EXISTS (SELECT * FROM [dbo].[Idempotence] WHERE MessageId = @MessageId and AssignedThreadId is not null)
	select 1
else
	select 0
