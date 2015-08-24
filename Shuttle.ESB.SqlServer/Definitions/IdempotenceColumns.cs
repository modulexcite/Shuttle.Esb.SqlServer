using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.ESB.SqlServer
{
    public class IdempotenceColumns
    {
        public static MappedColumn<Guid> MessageId = new MappedColumn<Guid>("MessageId", DbType.Guid);
        public static MappedColumn<Guid> MessageIdReceived = new MappedColumn<Guid>("MessageIdReceived", DbType.Guid);
		public static MappedColumn<string> InboxWorkQueueUri = new MappedColumn<string>("InboxWorkQueueUri", DbType.AnsiString, 265);
		public static MappedColumn<byte[]> MessageBody = new MappedColumn<byte[]>("MessageBody", DbType.Binary);
		public static MappedColumn<int> AssignedThreadId = new MappedColumn<int>("AssignedThreadId", DbType.Int32);
	}
}