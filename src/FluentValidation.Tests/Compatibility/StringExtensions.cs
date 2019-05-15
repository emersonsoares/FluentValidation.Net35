namespace System
{
	static class StringExtensions
	{
		public static bool IsNullOrWhiteSpace(this string source)
		{
#if NET35
			return string.IsNullOrEmpty(source) || source.Trim().Length > 0;
#else
			return string.IsNullOrWhiteSpace(source);
#endif
		}
	}
}
