using System;
using System.Collections.Generic;
using System.Text;

namespace System.Reflection
{
#if NET35
	static class ReflectionExtensions
	{

		public static MethodInfo GetMethod(this PropertyInfo property)
		{
			return property.GetGetMethod();
		}

		public static Delegate CreateDelegate(this MethodInfo method, Type delegateType)
		{
			return Delegate.CreateDelegate(delegateType, method);
		}
	}
#endif
}
