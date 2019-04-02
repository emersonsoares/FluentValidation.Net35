using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace System
{
#if NET35
	static class TypeCompatibilityExtension
	{
		readonly static object[] EmptyArray = new object[0];

		public static TypeInfo GetTypeInfo(this Type source)
		{
			return new TypeInfo(source);
		}

		public static PropertyInfo[] GetRuntimeProperties(this Type type)
		{
			return type.GetProperties();
		}



		public static PropertyInfo GetRuntimeProperty(this Type type, string name)
		{
			return type.GetProperty(name);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo member, bool inherit = false)
			where T : Attribute
		{
			return (member.GetCustomAttributes(typeof(T), false) ?? EmptyArray)
				.OfType<T>();
		}

		public static T GetCustomAttribute<T>(this MemberInfo member, bool inherit = false)
			where T : Attribute
		{
			return (member.GetCustomAttributes(typeof(T), false) ?? EmptyArray).OfType<T>().SingleOrDefault();
		}

		public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
		{
			return type.GetMethods();
		}
	}
#endif
}
