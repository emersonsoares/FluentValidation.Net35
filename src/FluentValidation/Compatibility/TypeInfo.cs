using System.Collections.Generic;
using System.Globalization;

namespace System.Reflection
{
#if NET35
	public class TypeInfo : Type
	{
		readonly Type type;
		readonly static MethodInfo MethodGetAttributeFlagsImpl =
			typeof(Type).GetMethod(nameof(GetAttributeFlagsImpl), BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo MethodGetConstructorImpl =
			typeof(Type).GetMethod(nameof(GetConstructorImpl), BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo MethodGetMethodImpl =
			typeof(Type).GetMethod(nameof(GetMethodImpl), BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo MethodGetPropertyImpl =
			typeof(Type).GetMethod(nameof(GetPropertyImpl), BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo MethodHasElementTypeImpl =
			typeof(Type).GetMethod(nameof(HasElementTypeImpl), BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo MethodIsArrayImpl =
			typeof(Type).GetMethod(nameof(IsArrayImpl), BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo MethodIsByRefImpl =
			typeof(Type).GetMethod(nameof(IsByRefImpl), BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo MethodIsCOMObjectImpl =
			typeof(Type).GetMethod(nameof(IsCOMObjectImpl), BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo MethodIsPointerImpl =
			typeof(Type).GetMethod(nameof(IsPointerImpl), BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo MethodIsPrimitiveImpl =
			typeof(Type).GetMethod(nameof(IsPrimitiveImpl), BindingFlags.Instance | BindingFlags.NonPublic);

		internal TypeInfo(Type type) => this.type = type;

		public override Guid GUID => type.GUID;

		public override Module Module => type.Module;

		public override Assembly Assembly => type.Assembly;

		public override string FullName => type.FullName;

		public override string Namespace => type.Namespace;

		public override string AssemblyQualifiedName => type.AssemblyQualifiedName;

		public override Type BaseType => type.BaseType;

		public override Type UnderlyingSystemType => type.UnderlyingSystemType;

		public override string Name => type.Name;

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => type.GetConstructors(bindingAttr);

		public override object[] GetCustomAttributes(bool inherit) => type.GetCustomAttributes(inherit);

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) => type.GetCustomAttributes(attributeType, inherit);

		public override Type GetElementType() => type.GetElementType();

		public override EventInfo GetEvent(string name, BindingFlags bindingAttr) => type.GetEvent(name, bindingAttr);

		public override EventInfo[] GetEvents(BindingFlags bindingAttr) => type.GetEvents(bindingAttr);

		public override FieldInfo GetField(string name, BindingFlags bindingAttr) => GetField(name, bindingAttr);

		public override FieldInfo[] GetFields(BindingFlags bindingAttr) => GetFields(bindingAttr);

		public override Type GetInterface(string name, bool ignoreCase) => type.GetInterface(name, ignoreCase);

		public override Type[] GetInterfaces() => type.GetInterfaces();

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => type.GetMembers(bindingAttr);

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => type.GetMethods(bindingAttr);

		public override Type GetNestedType(string name, BindingFlags bindingAttr) => type.GetNestedType(name, bindingAttr);

		public override Type[] GetNestedTypes(BindingFlags bindingAttr) => type.GetNestedTypes(bindingAttr);

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => GetProperties(bindingAttr);

		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) => type.InvokeMember(name, invokeAttr, binder, target, args);

		public override bool IsDefined(Type attributeType, bool inherit) => type.IsDefined(attributeType, inherit);

		protected override TypeAttributes GetAttributeFlagsImpl() => (TypeAttributes)MethodGetAttributeFlagsImpl.Invoke(type, null);

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) => (ConstructorInfo)MethodGetConstructorImpl.Invoke(type, new object[] { bindingAttr, binder, callConvention, type, modifiers });

		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) => (MethodInfo)MethodGetMethodImpl.Invoke(type, new object[] { name, bindingAttr, binder, callConvention, type, modifiers });

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) => (PropertyInfo)MethodGetPropertyImpl.Invoke(type, new object[] { name, bindingAttr, binder, returnType, type, modifiers });

		protected override bool HasElementTypeImpl() => (bool)MethodHasElementTypeImpl.Invoke(type, null);

		protected override bool IsArrayImpl() => (bool)MethodIsArrayImpl.Invoke(type, null);

		protected override bool IsByRefImpl() => (bool)MethodIsByRefImpl.Invoke(type, null);

		protected override bool IsCOMObjectImpl() => (bool)MethodIsCOMObjectImpl.Invoke(type, null);

		protected override bool IsPointerImpl() => (bool)MethodIsPointerImpl.Invoke(type, null);

		protected override bool IsPrimitiveImpl() => (bool)MethodIsPrimitiveImpl.Invoke(type, null);

		public IEnumerable<Type> GenericTypeArguments => type.GetGenericArguments();
	}
#endif

}
