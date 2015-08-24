delete 
from 
	idm
from
	[dbo].[IdempotenceDeferredMessage] idm
inner join
	[dbo].[Idempotence] i on
	(
		idm.MessageId = i.MessageId
		and
		i.InboxWorkQueueUri = @InboxWorkQueueUri
		and
		i.MessageHandled = 0
	)

delete from [dbo].[Idempotence] where InboxWorkQueueUri = @InboxWorkQueueUri and MessageHandled = 0

update [dbo].[Idempotence] set
	[AssignedThreadId] = null,
	[DateThreadIdAssigned] = null

