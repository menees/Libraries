namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;

	#endregion

	#region Public EditType

	public enum EditType
	{
		None,
		Delete,
		Insert,
		Change,
	}

	#endregion

	#region Public HashType

	public enum HashType
	{
		HashCode,

		[SuppressMessage(
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Crc",
			Justification = "CRC is for 'cyclic redundancy check'.")]
		Crc32,

		Unique,
	}

	#endregion
}
