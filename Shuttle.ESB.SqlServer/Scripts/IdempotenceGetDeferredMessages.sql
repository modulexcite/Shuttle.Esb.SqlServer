select MessageBody from [dbo].[IdempotenceDeferredMessage] where MessageIdReceived = @MessageIdReceived

