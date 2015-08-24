if EXISTS (SELECT * FROM [dbo].[Idempotence] WHERE MessageId = @MessageId and AssignedThreadId is null)
	update [dbo].[Idempotence] set 
		InboxWorkQueueUri = @InboxWorkQueueUri, 
		AssignedThreadId = @AssignedThreadId, 
		DateThreadIdAssigned = getdate()
	where
		MessageId = @MessageId;
else
	if NOT EXISTS (SELECT * FROM [dbo].[IdempotenceHistory] WHERE MessageId = @MessageId)
		insert into [dbo].[Idempotence] (MessageId, InboxWorkQueueUri, AssignedThreadId, DateThreadIdAssigned) values (@MessageId, @InboxWorkQueueUri, @AssignedThreadId, getdate());
	else
		raiserror ('IGNORE MESSAGE PROCESSING', 16, 0) WITH SETERROR

	