namespace Menees.Diffs.Tests
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	#endregion

	internal sealed class TempFile : IDisposable
	{
		#region Private Methods

		private bool disposed;

		#endregion

		#region Constructors

		public TempFile(string content)
		{
			this.FileName = Path.GetTempFileName();
			if (content != null)
			{
				File.WriteAllBytes(this.FileName, Encoding.UTF8.GetBytes(content));
			}
		}

		#endregion

		#region Public Properties

		public string FileName { get; }

		public FileInfo Info => new(this.FileName);

		#endregion

		#region Public Methods

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}

		#endregion

		#region Private Methods

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					File.Delete(this.FileName);
				}

				disposed = true;
			}
		}

		#endregion
	}
}
