using System.Linq;
using System.Reflection;

namespace System
{
#if NET35
	static class TypeExpensions
	{
		public static Type GetTypeInfo(this Type source) => source;

		public static T GetCustomAttribute<T>(this Type typeInfo)
		{
			return typeInfo.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
		}

		public static T GetCustomAttribute<T>(this ParameterInfo parameterInfo)
		{
			return parameterInfo.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
		}
	}
#endif
}
