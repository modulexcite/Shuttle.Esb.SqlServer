Shuttle.Esb.SqlServer
=====================

Microsoft SQL Server implementation for use with Shuttl.Esb:

- `SubscriptionManager` implements `ISubscriptionManager`
- `IdempotenceService` implements `iIdempotenceService`
- `SqlQueue` implements `IQueue`

# SqlQueue

There is a `IQueue` implementation for Sql Server that enables a table-based queue.  Since this a table-based queue is not a real queuing technology it is prudent to make use of a local outbox.

## Configuration

The queue configuration is part of the specified uri, e.g.:

~~~xml
    <inbox
      workQueueUri="sql://connectionstring-name/table-queue"
	  .
	  .
	  .
    />
~~~

In addition to this there is also a Sql Server specific section (defaults specified here):

~~~xml
<configuration>
  <configSections>
    <section name='sqlServer' type="Shuttle.Esb.SqlServer.SqlServerSection, Shuttle.Esb.SqlServer"/>
  </configSections>
  
  <sqlServer
	subscriptionManagerConnectionStringName="Subscription"
	idempotenceServiceConnectionStringName="Idempotence"
	scriptFolder=""
  />
  .
  .
  .
<configuration>
~~~

# SubscriptionManager

A Sql Server based `ISubscriptionManager` implementation is also provided.  The subscription manager caches all subscriptions forever so should a new subscriber be added be sure to restart the publisher endpoint service.

# IdempotenceService

A `IIdempotenceService` implementation is also available for Sql Server.