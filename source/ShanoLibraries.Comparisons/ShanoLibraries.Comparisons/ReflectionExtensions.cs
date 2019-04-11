using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ShanoLibraries.Comparisons
{
    internal static class ReflectionExtensions
    {
        public static bool IsCompilerGenerated(this MemberInfo memberInfo) =>
            memberInfo.GetCustomAttribute<CompilerGeneratedAttribute>() is null;

        public static bool IsNullable(this Type type, out Type underlyingValueType)
        {
            if (
                !type.IsConstructedGenericType ||
                type.GetGenericTypeDefinition() != typeof(Nullable<>)
            ) return (underlyingValueType = null) != null;

            underlyingValueType = type.GenericTypeArguments.Single();
            return true;
        }

        public static bool ImplementsGenericEnumerable(this Type type, out Type itemType)
        {
            bool success = type.ImplementsInterface(typeof(IEnumerable<>), out Type interfaceType);
            if (!success) return (itemType = null) != null;

            itemType = interfaceType.GenericTypeArguments.Single();
            return success;
        }

        public static bool ImplementsGenericComparable(this Type type) =>
            type.ImplementsInterface(typeof(IComparable<>).MakeGenericType(type));

        public static bool ImplementsEnumerable(this Type type) =>
            type.ImplementsInterface(typeof(IEnumerable));

        public static bool ImplementsComparable(this Type type) =>
            ImplementsInterface(type, typeof(IComparable));

        public static bool ImplementsInterface(this Type type, Type interfaceType) =>
            type.ImplementsInterface(interfaceType, out _);

        public static bool ImplementsInterface(this Type type, Type interfaceType, out Type actualInterfaceType)
        {
            Type[] interfaces = type.GetInterfaces();

            if (interfaces.Contains(interfaceType))
            {
                actualInterfaceType = interfaceType;
                return true;
            }

            actualInterfaceType =
                interfaces
                .Where(x => x.IsConstructedGenericType)
                .Where(x => x.GetGenericTypeDefinition() == interfaceType)
                .FirstOrDefault();

            return actualInterfaceType != null;
        }
    }
}
