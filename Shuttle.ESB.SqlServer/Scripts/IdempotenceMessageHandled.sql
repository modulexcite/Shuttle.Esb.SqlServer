update [dbo].[Idempotence] set MessageHandled = 1 where MessageId = @MessageId
