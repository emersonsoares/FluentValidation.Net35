#if NET35
namespace System.Linq.Expressions
{
	public static class ExpressionEx
	{
		public static BinaryExpression Assign(this Expression left, Expression right)
		{
			var assign = typeof(Assigner<>).MakeGenericType(left.Type).GetMethod("Assign");

			var assignExpr = Expression.Add(left, right, assign);

			return assignExpr;
		}

		private static class Assigner<T>
		{
			public static T Assign(ref T left, T right)
			{
				return (left = right);
			}
		}
	}

}
#endif