#if NET35
using System.Reflection;

namespace System.Collections.Generic
{
	//
	// Summary:
	//     Represents a strongly-typed, read-only collection of elements.
	//
	// Type parameters:
	//   T:
	//     The type of the elements.This type parameter is covariant. That is, you can use
	//     either the type you specified or any type that is more derived. For more information
	//     about covariance and contravariance, see Covariance and Contravariance in Generics.
	//[TypeDependencyAttribute("System.SZArrayHelper")]
	public interface IReadOnlyCollection<T> : IEnumerable<T>, IEnumerable
	{
		//
		// Summary:
		//     Gets the number of elements in the collection.
		//
		// Returns:
		//     The number of elements in the collection.
		int Count { get; }
	}

	//
	// Summary:
	//     Represents a read-only collection of elements that can be accessed by index.
	//
	// Type parameters:
	//   T:
	//     The type of elements in the read-only list. This type parameter is covariant.
	//     That is, you can use either the type you specified or any type that is more derived.
	//     For more information about covariance and contravariance, see Covariance and
	//     Contravariance in Generics.
	//[DefaultMember("Item")]
	//[TypeDependencyAttribute("System.SZArrayHelper")]
	public interface IReadOnlyList<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
	{
		//
		// Summary:
		//     Gets the element at the specified index in the read-only list.
		//
		// Parameters:
		//   index:
		//     The zero-based index of the element to get.
		//
		// Returns:
		//     The element at the specified index in the read-only list.
		T this[int index] { get; }
	}

	static class RealdOnlyExtension
	{
		class RL<T>: ObjectModel.ReadOnlyCollection<T>, IReadOnlyList<T>			
		{
			public RL(IList<T> source):base(source)
			{

			}
		}

		public static IReadOnlyList<T> AsReadOnlyEx<T>(this IList<T> source)
		{
			return new RL<T>(source);
		}
	}
}
#endif