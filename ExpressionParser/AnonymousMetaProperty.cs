using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace My.ExpressionParser
{
    /// <summary>
    /// Class that represents a property of an anonymous type.
    /// </summary>
    [Serializable]
    public sealed class AnonymousMetaProperty
    {
        private readonly string _name;
        private readonly Type _propertyType;

        #region Properties

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The <see cref="Type"/> of the property.
        /// </summary>
        public Type PropertyType
        {
            get { return _propertyType; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public AnonymousMetaProperty(string name, Type propertyType)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");

            _name = name;
            _propertyType = propertyType;
        }

        /// <summary>
        /// Instance an instance of the class <see cref="AnonymousMetaProperty"/> with a <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="property"><see cref="PropertyInfo"/> to create a <see cref="AnonymousMetaProperty"/> from.</param>
        public AnonymousMetaProperty(PropertyInfo property)
            : this(property.Name, property.PropertyType) { }

        #endregion

        #region Methods

        /// <summary>
        /// Overrides the equality check.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>True, if the other <see langword="object"/> is equal to this. False, if not.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as AnonymousMetaProperty;

            if (ReferenceEquals(other, null))
                return false;

            return Equals(other._name, _name) &&
                   Equals(other._propertyType, _propertyType);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see langword="object"/>.</returns>
        public override int GetHashCode()
        {
            var hashCode = -871466652;

            hashCode ^= EqualityComparer<Type>.Default.GetHashCode(_propertyType);
            hashCode ^= EqualityComparer<string>.Default.GetHashCode(_name);

            return hashCode;
        }

        #endregion

    }
}
