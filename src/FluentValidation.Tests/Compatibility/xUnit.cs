#if NET35
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xunit
{
	class XunitException : Exception
	{
		readonly string stackTrace;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitException"/> class.
		/// </summary>
		public XunitException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitException"/> class.
		/// </summary>
		/// <param name="userMessage">The user message to be displayed</param>
		public XunitException(string userMessage)
			: this(userMessage, (Exception)null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitException"/> class.
		/// </summary>
		/// <param name="userMessage">The user message to be displayed</param>
		/// <param name="innerException">The inner exception</param>
		protected XunitException(string userMessage, Exception innerException)
			: base(userMessage, innerException)
		{
			UserMessage = userMessage;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitException"/> class.
		/// </summary>
		/// <param name="userMessage">The user message to be displayed</param>
		/// <param name="stackTrace">The stack trace to be displayed</param>
		protected XunitException(string userMessage, string stackTrace)
			: this(userMessage)
		{
			this.stackTrace = stackTrace;
		}

		/// <summary>
		/// Gets a string representation of the frames on the call stack at the time the current exception was thrown.
		/// </summary>
		/// <returns>A string that describes the contents of the call stack, with the most recent method call appearing first.</returns>
		public override string StackTrace
		{
			get { return stackTrace ?? base.StackTrace; }
		}

		/// <summary>
		/// Gets the user message
		/// </summary>
		public string UserMessage { get; protected set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			string className = GetType().ToString();
			string message = Message;
			string result;

			if (message == null || message.Length <= 0)
				result = className;
			else
				result = string.Format("{0}: {1}", className, message);

			string stackTrace = StackTrace;
			if (stackTrace != null)
				result = result + Environment.NewLine + stackTrace;

			return result;
		}
	}

	class AllException : XunitException
	{
		readonly IReadOnlyList<Tuple<int, object, Exception>> errors;
		readonly int totalItems;

		/// <summary>
		/// Creates a new instance of the <see cref="AllException"/> class.
		/// </summary>
		/// <param name="totalItems">The total number of items that were in the collection.</param>
		/// <param name="errors">The list of errors that occurred during the test pass.</param>
		public AllException(int totalItems, Tuple<int, object, Exception>[] errors)
			: base("Assert.All() Failure")
		{
			this.errors = errors.ToList().AsReadOnlyEx();
			this.totalItems = totalItems;
		}

		/// <summary>
		/// The errors that occurred during execution of the test.
		/// </summary>
		public IReadOnlyList<Exception> Failures { get { return errors.Select(t => t.Item3).ToList().AsReadOnlyEx(); } }

		/// <inheritdoc/>
		public override string Message
		{
			get
			{
				var formattedErrors = errors.Select(error =>
				{
					var indexString = string.Format(CultureInfo.CurrentCulture, "[{0}]: ", error.Item1);
					var spaces = Environment.NewLine + "".PadRight(indexString.Length);

					return string.Format(CultureInfo.CurrentCulture,
										 "{0}Item: {1}{2}{3}",
										 indexString,
										 error.Item2?.ToString()?.Replace(Environment.NewLine, spaces),
										 spaces,
										 error.Item3.ToString().Replace(Environment.NewLine, spaces));
				})
#if NET35
					.ToArray()
#endif
				;

				return string.Format(CultureInfo.CurrentCulture,
									 "{0}: {1} out of {2} items in the collection did not pass.{3}{4}",
									 base.Message,
									 errors.Count,
									 totalItems,
									 Environment.NewLine,
									 string.Join(Environment.NewLine, formattedErrors));
			}
		}
	}

	// <summary>
	/// Default implementation of <see cref="ITestOutputHelper"/>.
	/// </summary>
	public class DebugWindowOutputHelper : Abstractions.ITestOutputHelper
	{
		public void WriteLine(string message)
		{
			System.Diagnostics.Debug.WriteLine(message);
		}

		public void WriteLine(string format, params object[] args)
		{
			System.Diagnostics.Debug.Print(format, args);
			System.Diagnostics.Debug.WriteLine(string.Empty);
		}
	}

	static class AssertEx
	{
		/// <summary>
		/// Verifies that all items in the collection pass when executed against
		/// action.
		/// </summary>
		/// <typeparam name="T">The type of the object to be verified</typeparam>
		/// <param name="collection">The collection</param>
		/// <param name="action">The action to test each item against</param>
		/// <exception cref="AllException">Thrown when the collection contains at least one non-matching element</exception>
		public static void All<T>(IEnumerable<T> collection, Action<T> action)
		{
			Assert.NotNull(collection);
			Assert.NotNull(action);

			var errors = new Stack<Tuple<int, object, Exception>>();
			var array = collection.ToArray();

			for (var idx = 0; idx < array.Length; ++idx)
			{
				try
				{
					action(array[idx]);
				}
				catch (Exception ex)
				{
					errors.Push(new Tuple<int, object, Exception>(idx, array[idx], ex));
				}
			}

			if (errors.Count > 0)
				throw new AllException(array.Length, errors.ToArray());
		}


		/// <summary>
		/// Records any exception which is thrown by the given code.
		/// </summary>
		/// <param name="testCode">The code which may thrown an exception.</param>
		/// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception is resurfaced to the user.")]
		static Exception RecordException(Action testCode)
		{
			Assert.NotNull(testCode);

			try
			{
				testCode();
				return null;
			}
			catch (Exception ex)
			{
				return ex;
			}
		}

		/// <summary>
		/// Verifies that the exact exception is thrown (and not a derived exception type).
		/// </summary>
		/// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
		/// <param name="testCode">A delegate to the code to be tested</param>
		/// <returns>The exception that was thrown, when successful</returns>
		/// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
		public static T Throws<T>(Action testCode)
			where T : Exception
		{
			return (T)Throws(typeof(T), RecordException(testCode));
		}

		/// <summary>
		/// Verifies that the exact exception is thrown (and not a derived exception type).
		/// </summary>
		/// <param name="exceptionType">The type of the exception expected to be thrown</param>
		/// <param name="testCode">A delegate to the code to be tested</param>
		/// <returns>The exception that was thrown, when successful</returns>
		/// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
		public static Exception Throws(Type exceptionType, Action testCode)
		{
			return Throws(exceptionType, RecordException(testCode));
		}

		static Exception Throws(Type exceptionType, Exception exception)
		{
			Assert.NotNull(exceptionType);

			if (exception == null)
				throw new Sdk.ThrowsException(exceptionType);

			if (!exceptionType.Equals(exception.GetType()))
				throw new Sdk.ThrowsException(exceptionType, exception);

			return exception;
		}
	}

	public class InlineDataAttribute : Xunit.Extensions.InlineDataAttribute
	{
		public InlineDataAttribute(params object[] dataValues)
			: base(dataValues)
		{

		}
	}

	public class TheoryAttribute : Xunit.Extensions.TheoryAttribute
	{

	}

	/// <summary>
	/// Provides a data source for a data theory, with the data coming from one of the following sources:
	/// 1. A static property
	/// 2. A static field
	/// 3. A static method (with parameters)
	/// The member must return something compatible with IEnumerable&lt;object[]&gt; with the test data.
	/// Caution: the property is completely enumerated by .ToList() before any test is run. Hence it should return independent object sets.
	/// </summary>
	[CLSCompliant(false)]
	//[DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class MemberDataAttribute : MemberDataAttributeBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemberDataAttribute"/> class.
		/// </summary>
		/// <param name="memberName">The name of the public static member on the test class that will provide the test data</param>
		/// <param name="parameters">The parameters for the member (only supported for methods; ignored for everything else)</param>
		public MemberDataAttribute(string memberName, params object[] parameters)
			: base(memberName, parameters) { }

		/// <inheritdoc/>
		protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
		{
			if (item == null)
				return null;

			var array = item as object[];
			if (array == null)
				throw new ArgumentException($"Property {MemberName} on {MemberType ?? testMethod.DeclaringType} yielded an item that is not an object[]");

			return array;
		}
	}

	/// <summary>
	/// Provides a base class for attributes that will provide member data. The member data must return
	/// something compatible with <see cref="IEnumerable"/>.
	/// Caution: the property is completely enumerated by .ToList() before any test is run. Hence it should return independent object sets.
	/// </summary>
	[CLSCompliant(false)]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public abstract class MemberDataAttributeBase : Extensions.DataAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemberDataAttributeBase"/> class.
		/// </summary>
		/// <param name="memberName">The name of the public static member on the test class that will provide the test data</param>
		/// <param name="parameters">The parameters for the member (only supported for methods; ignored for everything else)</param>
		protected MemberDataAttributeBase(string memberName, object[] parameters)
		{
			MemberName = memberName;
			Parameters = parameters;
		}

		/// <summary>
		/// Returns <c>true</c> if the data attribute wants to skip enumerating data during discovery.
		/// This will cause the theory to yield a single test case for all data, and the data discovery
		/// will be during test execution instead of discovery.
		/// </summary>
		public bool DisableDiscoveryEnumeration { get; set; }

		/// <summary>
		/// Gets the member name.
		/// </summary>
		public string MemberName { get; private set; }

		/// <summary>
		/// Gets or sets the type to retrieve the member from. If not set, then the property will be
		/// retrieved from the unit test class.
		/// </summary>
		public Type MemberType { get; set; }

		/// <summary>
		/// Gets or sets the parameters passed to the member. Only supported for static methods.
		/// </summary>
		public object[] Parameters { get; private set; }

		/// <inheritdoc/>
		public override IEnumerable<object[]> GetData(MethodInfo testMethod, Type[] parameterTypes)
		{
			//Guard.ArgumentNotNull("testMethod", testMethod);

			var type = MemberType ?? testMethod.DeclaringType;
			var accessor = GetPropertyAccessor(type) ?? GetFieldAccessor(type) ?? GetMethodAccessor(type);
			if (accessor == null)
			{
				var parameterText = Parameters?.Length > 0 ? $" with parameter types: {string.Join(", ", Parameters.Select(p => p?.GetType().FullName ?? "(null)").ToArray())}" : "";
				throw new ArgumentException($"Could not find public static member (property, field, or method) named '{MemberName}' on {type.FullName}{parameterText}");
			}

			var obj = accessor();
			if (obj == null)
				return null;

			var dataItems = obj as IEnumerable;
			if (dataItems == null)
				throw new ArgumentException($"Property {MemberName} on {type.FullName} did not return IEnumerable");

			return dataItems.Cast<object>().Select(item => ConvertDataItem(testMethod, item));
		}

		/// <summary>
		/// Converts an item yielded by the data member to an object array, for return from <see cref="GetData"/>.
		/// </summary>
		/// <param name="testMethod">The method that is being tested.</param>
		/// <param name="item">An item yielded from the data member.</param>
		/// <returns>An <see cref="T:object[]"/> suitable for return from <see cref="GetData"/>.</returns>
		protected abstract object[] ConvertDataItem(MethodInfo testMethod, object item);

		Func<object> GetFieldAccessor(Type type)
		{
			FieldInfo fieldInfo = null;
			for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.GetTypeInfo().BaseType)
			{
				fieldInfo = reflectionType.GetField(MemberName);
				if (fieldInfo != null)
					break;
			}

			if (fieldInfo == null || !fieldInfo.IsStatic)
				return null;

			return () => fieldInfo.GetValue(null);
		}

		Func<object> GetMethodAccessor(Type type)
		{
			MethodInfo methodInfo = null;
			var parameterTypes = Parameters == null ? new Type[0] : Parameters.Select(p => p?.GetType()).ToArray();
			for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.GetTypeInfo().BaseType)
			{
				methodInfo = reflectionType.GetMethods()
										   .FirstOrDefault(m => m.Name == MemberName && ParameterTypesCompatible(m.GetParameters(), parameterTypes));
				if (methodInfo != null)
					break;
			}

			if (methodInfo == null || !methodInfo.IsStatic)
				return null;

			return () => methodInfo.Invoke(null, Parameters);
		}

		Func<object> GetPropertyAccessor(Type type)
		{
			PropertyInfo propInfo = null;
			for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.GetTypeInfo().BaseType)
			{
				propInfo = reflectionType.GetProperty(MemberName);
				if (propInfo != null)
					break;
			}

			if (propInfo == null || propInfo.GetGetMethod() == null || !propInfo.GetGetMethod().IsStatic)
				return null;

			return () => propInfo.GetValue(null, null);
		}

		static bool ParameterTypesCompatible(ParameterInfo[] parameters, Type[] parameterTypes)
		{
			if (parameters?.Length != parameterTypes.Length)
				return false;

			for (int idx = 0; idx < parameters.Length; ++idx)
				if (parameterTypes[idx] != null && !parameters[idx].ParameterType.GetTypeInfo().IsAssignableFrom(parameterTypes[idx].GetTypeInfo()))
					return false;

			return true;
		}
	}

	/// <summary>
	/// Provides data for theories based on collection initialization syntax.
	/// </summary>
	public abstract class TheoryData : IEnumerable<object[]>
	{
		readonly List<object[]> data = new List<object[]>();

		/// <summary>
		/// Adds a row to the theory.
		/// </summary>
		/// <param name="values">The values to be added.</param>
		protected void AddRow(params object[] values)
		{
			data.Add(values);
		}

		/// <inheritdoc/>
		public IEnumerator<object[]> GetEnumerator()
		{
			return data.GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with a single parameter. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T">The parameter type.</typeparam>
	public class TheoryData<T> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p">The data value.</param>
		public void Add(T p)
		{
			AddRow(p);
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with 2 parameters. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T1">The first parameter type.</typeparam>
	/// <typeparam name="T2">The second parameter type.</typeparam>
	public class TheoryData<T1, T2> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p1">The first data value.</param>
		/// <param name="p2">The second data value.</param>
		public void Add(T1 p1, T2 p2)
		{
			AddRow(p1, p2);
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with 3 parameters. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T1">The first parameter type.</typeparam>
	/// <typeparam name="T2">The second parameter type.</typeparam>
	/// <typeparam name="T3">The third parameter type.</typeparam>
	public class TheoryData<T1, T2, T3> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p1">The first data value.</param>
		/// <param name="p2">The second data value.</param>
		/// <param name="p3">The third data value.</param>
		public void Add(T1 p1, T2 p2, T3 p3)
		{
			AddRow(p1, p2, p3);
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with 4 parameters. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T1">The first parameter type.</typeparam>
	/// <typeparam name="T2">The second parameter type.</typeparam>
	/// <typeparam name="T3">The third parameter type.</typeparam>
	/// <typeparam name="T4">The fourth parameter type.</typeparam>
	public class TheoryData<T1, T2, T3, T4> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p1">The first data value.</param>
		/// <param name="p2">The second data value.</param>
		/// <param name="p3">The third data value.</param>
		/// <param name="p4">The fourth data value.</param>
		public void Add(T1 p1, T2 p2, T3 p3, T4 p4)
		{
			AddRow(p1, p2, p3, p4);
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with 5 parameters. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T1">The first parameter type.</typeparam>
	/// <typeparam name="T2">The second parameter type.</typeparam>
	/// <typeparam name="T3">The third parameter type.</typeparam>
	/// <typeparam name="T4">The fourth parameter type.</typeparam>
	/// <typeparam name="T5">The fifth parameter type.</typeparam>
	public class TheoryData<T1, T2, T3, T4, T5> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p1">The first data value.</param>
		/// <param name="p2">The second data value.</param>
		/// <param name="p3">The third data value.</param>
		/// <param name="p4">The fourth data value.</param>
		/// <param name="p5">The fifth data value.</param>
		public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
		{
			AddRow(p1, p2, p3, p4, p5);
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with 5 parameters. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T1">The first parameter type.</typeparam>
	/// <typeparam name="T2">The second parameter type.</typeparam>
	/// <typeparam name="T3">The third parameter type.</typeparam>
	/// <typeparam name="T4">The fourth parameter type.</typeparam>
	/// <typeparam name="T5">The fifth parameter type.</typeparam>
	/// <typeparam name="T6">The sixth parameter type.</typeparam>
	public class TheoryData<T1, T2, T3, T4, T5, T6> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p1">The first data value.</param>
		/// <param name="p2">The second data value.</param>
		/// <param name="p3">The third data value.</param>
		/// <param name="p4">The fourth data value.</param>
		/// <param name="p5">The fifth data value.</param>
		/// <param name="p6">The sixth data value.</param>
		public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
		{
			AddRow(p1, p2, p3, p4, p5, p6);
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with 5 parameters. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T1">The first parameter type.</typeparam>
	/// <typeparam name="T2">The second parameter type.</typeparam>
	/// <typeparam name="T3">The third parameter type.</typeparam>
	/// <typeparam name="T4">The fourth parameter type.</typeparam>
	/// <typeparam name="T5">The fifth parameter type.</typeparam>
	/// <typeparam name="T6">The sixth parameter type.</typeparam>
	/// <typeparam name="T7">The seventh parameter type.</typeparam>
	public class TheoryData<T1, T2, T3, T4, T5, T6, T7> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p1">The first data value.</param>
		/// <param name="p2">The second data value.</param>
		/// <param name="p3">The third data value.</param>
		/// <param name="p4">The fourth data value.</param>
		/// <param name="p5">The fifth data value.</param>
		/// <param name="p6">The sixth data value.</param>
		/// <param name="p7">The seventh data value.</param>
		public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
		{
			AddRow(p1, p2, p3, p4, p5, p6, p7);
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with 5 parameters. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T1">The first parameter type.</typeparam>
	/// <typeparam name="T2">The second parameter type.</typeparam>
	/// <typeparam name="T3">The third parameter type.</typeparam>
	/// <typeparam name="T4">The fourth parameter type.</typeparam>
	/// <typeparam name="T5">The fifth parameter type.</typeparam>
	/// <typeparam name="T6">The sixth parameter type.</typeparam>
	/// <typeparam name="T7">The seventh parameter type.</typeparam>
	/// <typeparam name="T8">The eigth parameter type.</typeparam>
	public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p1">The first data value.</param>
		/// <param name="p2">The second data value.</param>
		/// <param name="p3">The third data value.</param>
		/// <param name="p4">The fourth data value.</param>
		/// <param name="p5">The fifth data value.</param>
		/// <param name="p6">The sixth data value.</param>
		/// <param name="p7">The seventh data value.</param>
		/// <param name="p8">The eigth data value.</param>
		public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
		{
			AddRow(p1, p2, p3, p4, p5, p6, p7, p8);
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with 5 parameters. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T1">The first parameter type.</typeparam>
	/// <typeparam name="T2">The second parameter type.</typeparam>
	/// <typeparam name="T3">The third parameter type.</typeparam>
	/// <typeparam name="T4">The fourth parameter type.</typeparam>
	/// <typeparam name="T5">The fifth parameter type.</typeparam>
	/// <typeparam name="T6">The sixth parameter type.</typeparam>
	/// <typeparam name="T7">The seventh parameter type.</typeparam>
	/// <typeparam name="T8">The eigth parameter type.</typeparam>
	/// <typeparam name="T9">The nineth parameter type.</typeparam>
	public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p1">The first data value.</param>
		/// <param name="p2">The second data value.</param>
		/// <param name="p3">The third data value.</param>
		/// <param name="p4">The fourth data value.</param>
		/// <param name="p5">The fifth data value.</param>
		/// <param name="p6">The sixth data value.</param>
		/// <param name="p7">The seventh data value.</param>
		/// <param name="p8">The eigth data value.</param>
		/// <param name="p9">The nineth data value.</param>
		public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
		{
			AddRow(p1, p2, p3, p4, p5, p6, p7, p8, p9);
		}
	}

	/// <summary>
	/// Represents a set of data for a theory with 5 parameters. Data can
	/// be added to the data set using the collection initializer syntax.
	/// </summary>
	/// <typeparam name="T1">The first parameter type.</typeparam>
	/// <typeparam name="T2">The second parameter type.</typeparam>
	/// <typeparam name="T3">The third parameter type.</typeparam>
	/// <typeparam name="T4">The fourth parameter type.</typeparam>
	/// <typeparam name="T5">The fifth parameter type.</typeparam>
	/// <typeparam name="T6">The sixth parameter type.</typeparam>
	/// <typeparam name="T7">The seventh parameter type.</typeparam>
	/// <typeparam name="T8">The eigth parameter type.</typeparam>
	/// <typeparam name="T9">The nineth parameter type.</typeparam>
	/// <typeparam name="T10">The tenth parameter type.</typeparam>
	public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : TheoryData
	{
		/// <summary>
		/// Adds data to the theory data set.
		/// </summary>
		/// <param name="p1">The first data value.</param>
		/// <param name="p2">The second data value.</param>
		/// <param name="p3">The third data value.</param>
		/// <param name="p4">The fourth data value.</param>
		/// <param name="p5">The fifth data value.</param>
		/// <param name="p6">The sixth data value.</param>
		/// <param name="p7">The seventh data value.</param>
		/// <param name="p8">The eigth data value.</param>
		/// <param name="p9">The nineth data value.</param>
		/// <param name="p10">The tenth data value.</param>
		public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
		{
			AddRow(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
		}
	}
}

namespace Xunit.Abstractions
{
	/// <summary>
	/// Represents a class which can be used to provide test output.
	/// </summary>
	public interface ITestOutputHelper
	{
		/// <summary>
		/// Adds a line of text to the output.
		/// </summary>
		/// <param name="message">The message</param>
		void WriteLine(string message);

		/// <summary>
		/// Formats a line of text and adds it to the output.
		/// </summary>
		/// <param name="format">The message format</param>
		/// <param name="args">The format arguments</param>
		void WriteLine(string format, params object[] args);
	}
}
#endif
