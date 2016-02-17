using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace My.ExpressionParser
{
    /// <summary>
    /// Provides methods for identifying anonymous types and closure ("<c>DisplayClass</c>") types.
    /// </summary>
    public static class AnonymousTypeHelper
    {
        public const string AnonymousTypeNamePrefix = "<>f__AnonymousType";
        public const string ClosureTypeNamePrefix = "<>c__DisplayClass";

        private static readonly Dictionary<AnonymousMetaType, AnonymousMetaType> _anonymousTypes = new Dictionary<AnonymousMetaType, AnonymousMetaType>();

        /// <summary>
        /// Returns true if the given <see cref="Type"/> is an anonymous type.
        /// </summary>
        /// <param name="t"><see cref="Type"/> to test.</param>
        /// <returns>True, if the type is an anonymous type. False, if not.</returns>
        /// <remarks>
        /// A anonymous type is not really marked as anonymous.
        /// The only way to recognize it is it's name.
        /// Maybe in future versions they will be marked.
        /// </remarks>
        public static bool IsAnonymous(this Type t)
        {
            if (t == null)
                throw new ArgumentNullException("t");

            return Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) &&
                   t.Name.StartsWith(AnonymousTypeNamePrefix);
        }

        /// <summary>
        /// Returns true if the given <see cref="Type"/> is a display class.
        /// </summary>
        /// <param name="t"><see cref="Type"/> to test.</param>
        /// <returns>True, if the type is a display class. False, if not.</returns>
        /// <remarks>
        /// A display class is not really marked as display class.
        /// The only way to recognize it is it's name.
        /// Maybe in future versions they will be marked.
        /// </remarks>
        public static bool IsClosure(this Type t)
        {
            if (t == null)
                throw new ArgumentNullException("t");

            return Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) &&
                   t.Name.StartsWith(ClosureTypeNamePrefix);
        }

        /// <summary>
        /// Returns true if the given <see cref="Type"/> is a <see cref="IEnumerator{t}"/> class.
        /// </summary>
        /// <param name="t"><see cref="Type"/> to test.</param>
        /// <returns>Returns true if the given <see cref="Type"/> is a <see cref="IEnumerator{t}"/> class.</returns>
        public static bool IsEnumerator(this Type t)
        {
            if (!t.IsGenericType)
                return false;

            if (t.GetGenericTypeDefinition() == typeof(IEnumerator<>))
                return true;

            return t.GetInterfaces().Any(
                interfaceType => interfaceType.IsGenericType &&
                                 interfaceType.GetGenericTypeDefinition() == typeof(IEnumerator<>));
        }


        public static AnonymousMetaType GetAnonymousType(params AnonymousMetaProperty[] properties)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            AnonymousMetaType metaType;

            var key = new AnonymousMetaType(properties);

            if (!_anonymousTypes.TryGetValue(key, out metaType))
            {
                _anonymousTypes.Add(key, key);
                metaType = key;
            }

            return metaType;
        }
    }
}
