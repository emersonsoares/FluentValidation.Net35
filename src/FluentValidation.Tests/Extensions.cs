﻿namespace FluentValidation.Tests
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return value == null || string.IsNullOrEmpty(value.Trim());
        }
    }
}
