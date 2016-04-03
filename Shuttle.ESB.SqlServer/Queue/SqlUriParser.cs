﻿using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.SqlServer
{
	public class SqlUriParser
	{
		internal const string SCHEME = "sql";

		public SqlUriParser(Uri uri)
		{
			Guard.AgainstNull(uri, "uri");

			if (!uri.Scheme.Equals(SCHEME, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new InvalidSchemeException(SCHEME, uri.ToString());
			}

			if (uri.LocalPath == "/" || uri.Segments.Length != 2)
			{
				throw new UriFormatException(string.Format(EsbResources.UriFormatException,
					"sql://{{connection-name}}/{{table-name}}",
					uri));
			}

			Uri = uri;

			ConnectionName = Uri.Host;
			TableName = Uri.Segments[1];
		}

		public Uri Uri { get; private set; }
		public string ConnectionName { get; private set; }
		public string TableName { get; private set; }
	}
}