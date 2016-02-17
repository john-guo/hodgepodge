using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace My.ExpressionParser
{
    /// <summary>
    /// A class representing an anonymous type.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class AnonymousMetaType
    {
        #region Fields

        private readonly List<AnonymousMetaProperty> _metaProperties;
        private readonly string _name;
        
        [NonSerialized]
        private volatile Type _clrType;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets all the properties of the type.
        /// </summary>
        public IEnumerable<AnonymousMetaProperty> MetaProperties
        {
            get { return _metaProperties; }
        }

        /// <summary>
        /// Gets the names of all the properties of the type.
        /// </summary>
        private IEnumerable<String> PropertyNames
        {
            get { return _metaProperties.Select(p => p.Name); }
        }

        /// <summary>
        /// Gets the types of all the properties of the type.
        /// </summary>
        private IEnumerable<Type> PropertyTypes
        {
            get { return _metaProperties.Select(p => p.PropertyType); }
        }

        /// <summary>
        /// Returns a <see cref="IEnumerable{T}"/> containing the names of all parameters.
        /// </summary>
        private IEnumerable<String> GenericTypeParameterNames
        {
            get { return _metaProperties.Select(p => string.Format("<{0}>j__TPar", p.Name)); }
        }

        /// <summary>
        /// Returns a <see cref="IEnumerable{T}"/> containing the names of all fields.
        /// </summary>
        private IEnumerable<String> FieldNames
        {
            get { return _metaProperties.Select(p => string.Format("<{0}>i__Field", p.Name)); }
        }

        #endregion

        #region Constructors / Initialization

        /// <summary>
        /// Creates a new <see cref="AnonymousMetaType"/> with the specified <paramref name="properties"/>.
        /// </summary>
        public AnonymousMetaType(params AnonymousMetaProperty[] properties)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            _name = DynamicAssemblyHolder.Instance.GenerateAnonymousTypeName();
            _metaProperties = new List<AnonymousMetaProperty>(properties);
        }

        /// <summary>
        /// Creates a new <see cref="AnonymousMetaType"/> with the specified <paramref name="properties"/>.
        /// </summary>
        public AnonymousMetaType(IEnumerable<AnonymousMetaProperty> properties)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            _name = DynamicAssemblyHolder.Instance.GenerateAnonymousTypeName();
            _metaProperties = new List<AnonymousMetaProperty>(properties.ToArray());
        }

        /// <summary>
        /// Instance an instance of the class <see cref="AnonymousMetaType"/>
        /// </summary>
        /// <param name="representedType">The <see cref="Type"/> to generate the <see cref="AnonymousMetaType"/> from.</param>
        public AnonymousMetaType(Type representedType)
        {
            if (representedType == null)
                throw new ArgumentNullException("representedType");

            var properties = representedType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            var metaProperties = properties.Select(o => new AnonymousMetaProperty(o)).ToArray();

            _name = representedType.Name;
            _metaProperties = new List<AnonymousMetaProperty>(metaProperties);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate the a clr type at runtime.
        /// </summary>
        /// <returns>Returns the generated <see cref="Type"/>.</returns>
        public Type GetClrType()
        {
            if (_clrType == null)
            {
                lock (this)
                {
                    if (_clrType == null)
                        _clrType = CreateClrType(DynamicAssemblyHolder.Instance.ModuleBuilder);
                }
            }
            return _clrType;
        }

        /// <summary>
        /// Overrides the equality comparision.
        /// </summary>
        /// <param name="obj">Other object to compare with.</param>
        /// <returns>True, if the other <see langword="object"/> is equal to this. False, if not.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as AnonymousMetaType;
            
            if (other == null ||
                other._metaProperties.Count != _metaProperties.Count)
            {
                return false;
            }

            return other._metaProperties.SequenceEqual(_metaProperties);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see langword="object"/>.</returns>
        public override int GetHashCode()
        {
            var hashCode = _metaProperties.Aggregate(
                51463360,
                (value, property) => value ^ EqualityComparer<AnonymousMetaProperty>.Default.GetHashCode(property));
            
            return hashCode;
        }

        #region IL Generation

#pragma warning disable 168

        /// <summary>
        /// Generate the a clr type at runtime.
        /// </summary>
        /// <returns>Returns the generated <see cref="Type"/>.</returns>
        private Type CreateClrType(ModuleBuilder dynamicTypeModule)
        {
            var dynamicType = dynamicTypeModule.DefineType(
                this.Name,
                TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class | TypeAttributes.BeforeFieldInit);

            var fieldNames = this.FieldNames.ToArray();
            var propertyNames = this.PropertyNames;
            var createdFields = new List<FieldBuilder>();
            var typeParameters = dynamicType.DefineGenericParameters(this.GenericTypeParameterNames.ToArray());

            for (var i = 0; i < fieldNames.Length; i++)
            {
                var field = dynamicType.DefineField(fieldNames[i], typeParameters[i], FieldAttributes.Private | FieldAttributes.InitOnly);
                var attributeType = typeof(DebuggerBrowsableAttribute);
                var attribute = new CustomAttributeBuilder(attributeType.GetConstructor(new[] { typeof(DebuggerBrowsableState) }), new object[] { DebuggerBrowsableState.Never });

                field.SetCustomAttribute(attribute);
                createdFields.Add(field);
            }

            var createdProperties = propertyNames.Select((t, i) => GenerateProperty(dynamicType, t, createdFields[i])).ToList();

            GenerateClassAttributes(dynamicType);

            GenerateConstructor(dynamicType, propertyNames.ToArray(), createdFields);
            GenerateEqualsMethod(dynamicType, createdFields.ToArray());
            GenerateGetHashCodeMethod(dynamicType, createdFields.ToArray());
            GenerateToStringMethod(dynamicType, propertyNames.ToArray(), createdFields.ToArray());

            var createdType = dynamicType.CreateType();

            return createdType.MakeGenericType(this.PropertyTypes.ToArray());
        }

        /// <summary>
        /// Generate a constructor with for a type.
        /// </summary>
        /// <param name="dynamicType">A <see cref="TypeBuilder"/> generate the constructor for.</param>
        /// <param name="propertyNames"><see langword="string">strings</see> to create a constructor for.</param>
        /// <param name="fields">Fields to fill in the constructor.</param>
        private static void GenerateConstructor(TypeBuilder dynamicType, string[] propertyNames, IList<FieldBuilder> fields)
        {
            var dynamicConstuctor = dynamicType.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                fields.Select(f => f.FieldType).ToArray());

            var ilGen = dynamicConstuctor.GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[0]));

            for (var i = 0; i < propertyNames.Length; i++)
            {
                var propertyName = propertyNames[i];
                var field = fields[i];
                var parameter = dynamicConstuctor.DefineParameter(i + 1, ParameterAttributes.None, propertyName);

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg, parameter.Position);
                ilGen.Emit(OpCodes.Stfld, field);
            }

            ilGen.Emit(OpCodes.Ret);

            AddDebuggerHiddenAttribute(dynamicConstuctor);
        }

        /// <summary>
        /// Generate a ToString method.
        /// </summary>
        /// <param name="dynamicType">A <see cref="TypeBuilder"/> to generate a ToString method for.</param>
        /// <param name="propertyNames">The names of the properties of the type.</param>
        /// <param name="fields">Fields to read in the ToString method.</param>
        private static void GenerateToStringMethod(TypeBuilder dynamicType, string[] propertyNames, FieldBuilder[] fields)
        {
            var method = dynamicType.DefineMethod(
                "ToString",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.Standard,
                typeof(string),
                new Type[0]);

            var ilGen = method.GetILGenerator();

            var stringBuilderLocal = ilGen.DeclareLocal(typeof(StringBuilder));

            var appendMethod = typeof(StringBuilder).GetMethod("Append", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);
            var appendFormatMethod = typeof(StringBuilder).GetMethod("Append", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(object) }, null);
            var toStringMethod = typeof(object).GetMethod("ToString", BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);

            ilGen.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(new Type[0]));
            ilGen.Emit(OpCodes.Stloc, stringBuilderLocal);
            ilGen.Emit(OpCodes.Ldloc, stringBuilderLocal);
            ilGen.Emit(OpCodes.Ldstr, "{ ");
            ilGen.EmitCall(OpCodes.Callvirt, appendMethod, null);
            ilGen.Emit(OpCodes.Pop);

            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                ilGen.Emit(OpCodes.Ldloc, stringBuilderLocal);
                ilGen.Emit(OpCodes.Ldstr, string.Concat((i == 0) ? "" : ", ", propertyNames[i], " = "));
                ilGen.EmitCall(OpCodes.Callvirt, appendMethod, null);
                ilGen.Emit(OpCodes.Pop);

                ilGen.Emit(OpCodes.Ldloc, stringBuilderLocal);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, field);
                ilGen.Emit(OpCodes.Box, field.FieldType);
                ilGen.EmitCall(OpCodes.Callvirt, appendFormatMethod, null);
                ilGen.Emit(OpCodes.Pop);
            }

            ilGen.Emit(OpCodes.Ldloc, stringBuilderLocal);
            ilGen.Emit(OpCodes.Ldstr, " }");
            ilGen.EmitCall(OpCodes.Callvirt, appendMethod, null);
            ilGen.Emit(OpCodes.Pop);

            ilGen.Emit(OpCodes.Ldloc, stringBuilderLocal);
            ilGen.EmitCall(OpCodes.Callvirt, toStringMethod, null);
            ilGen.Emit(OpCodes.Ret);

            dynamicType.DefineMethodOverride(
                method,
                typeof(object).GetMethod(
                    "ToString",
                    BindingFlags.Public | BindingFlags.Instance));

            AddDebuggerHiddenAttribute(method);
        }

        /// <summary>
        /// Generates a GetHashCode method.
        /// </summary>
        /// <param name="dynamicType">A <see cref="TypeBuilder"/> to generate a GetHashCode method for.</param>
        /// <param name="fields">Fields to read in the GetHashCode method.</param>
        private static void GenerateGetHashCodeMethod(TypeBuilder dynamicType, IEnumerable<FieldBuilder> fields)
        {
            var method = dynamicType.DefineMethod(
                "GetHashCode",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.Standard,
                typeof(int),
                new Type[0]);
            
            var ilGen = method.GetILGenerator();
            var openComparerType = typeof(EqualityComparer<>);
            var resultLocal = ilGen.DeclareLocal(typeof(int));

            ilGen.Emit(OpCodes.Ldc_I4, new Random(unchecked((int)DateTime.Now.Ticks)).Next(int.MaxValue));
            ilGen.Emit(OpCodes.Stloc, resultLocal);

            foreach (var field in fields)
            {
                ilGen.Emit(OpCodes.Ldc_I4, 0xa5555529);
                ilGen.Emit(OpCodes.Ldloc, resultLocal);
                ilGen.Emit(OpCodes.Mul);

                var comparerType = openComparerType.MakeGenericType(field.FieldType);
                var defaultComparerAccessor = openComparerType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
                var defaultComparerAccessorHandle = TypeBuilder.GetMethod(comparerType, defaultComparerAccessor);

                ilGen.EmitCall(OpCodes.Call, defaultComparerAccessorHandle, null);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, field);

                var instanceType = openComparerType.GetGenericArguments()[0];
                var equalsMethod = openComparerType.GetMethod("GetHashCode", BindingFlags.Public | BindingFlags.Instance, null, new[] { instanceType }, null);
                var equalsMethodHandle = TypeBuilder.GetMethod(comparerType, equalsMethod);
                
                ilGen.EmitCall(OpCodes.Callvirt, equalsMethodHandle, null);
                ilGen.Emit(OpCodes.Add);
                ilGen.Emit(OpCodes.Stloc, resultLocal);
            }

            ilGen.Emit(OpCodes.Ldloc, resultLocal);
            ilGen.Emit(OpCodes.Ret);

            dynamicType.DefineMethodOverride(method, typeof(object).GetMethod("GetHashCode",
                BindingFlags.Public | BindingFlags.Instance));

            AddDebuggerHiddenAttribute(method);
        }

        /// <summary>
        /// Generates a Equals method.
        /// </summary>
        /// <param name="dynamicType">A <see cref="TypeBuilder"/> to generate a Equals method for.</param>
        /// <param name="fields">Fields to read in the GetHashCode method.</param>
        private static void GenerateEqualsMethod(TypeBuilder dynamicType, IEnumerable<FieldBuilder> fields)
        {
            var method = dynamicType.DefineMethod(
                "Equals",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.Standard,
                typeof(bool),
                new[] { typeof(object) });

            method.DefineParameter(0, ParameterAttributes.None, "value");

            var ilGen = method.GetILGenerator();

            var localType = ilGen.DeclareLocal(dynamicType);

            var falseLabel = ilGen.DefineLabel(); // IL_003a
            var endLabel = ilGen.DefineLabel();   // IL_003e

            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Isinst, dynamicType);
            ilGen.Emit(OpCodes.Stloc, localType);
            ilGen.Emit(OpCodes.Ldloc, localType);

            var openComparerType = typeof(EqualityComparer<>);

            foreach (var field in fields)
            {
                var comparerType = openComparerType.MakeGenericType(field.FieldType);
                var defaultComparerAccessor = openComparerType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
                var defaultComparerAccessorHandle = TypeBuilder.GetMethod(comparerType, defaultComparerAccessor);

                ilGen.Emit(OpCodes.Brfalse, falseLabel);
                ilGen.EmitCall(OpCodes.Call, defaultComparerAccessorHandle, null);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, field);
                ilGen.Emit(OpCodes.Ldloc, localType);
                ilGen.Emit(OpCodes.Ldfld, field);

                var instanceType = openComparerType.GetGenericArguments()[0];
                var equalsMethod = openComparerType.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance, null, new[] { instanceType, instanceType }, null);
                var equalsMethodHandle = TypeBuilder.GetMethod(comparerType, equalsMethod);

                ilGen.EmitCall(OpCodes.Callvirt, equalsMethodHandle, null);
            }

            ilGen.Emit(OpCodes.Br_S, endLabel);
            ilGen.MarkLabel(falseLabel);
            ilGen.Emit(OpCodes.Ldc_I4_0);
            ilGen.MarkLabel(endLabel);
            ilGen.Emit(OpCodes.Ret);

            dynamicType.DefineMethodOverride(method, typeof(object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance));

            AddDebuggerHiddenAttribute(method);
        }

        /// <summary>
        /// Generates a property.
        /// </summary>
        /// <param name="dynamicType">A <see cref="TypeBuilder"/> to generate a property for.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="field">Field to access in the property.</param>
        /// <returns>Returns the created property.</returns>
        private static PropertyBuilder GenerateProperty(TypeBuilder dynamicType, string propertyName, FieldBuilder field)
        {
            var property = dynamicType.DefineProperty(propertyName, PropertyAttributes.None, field.FieldType, null);
            var getMethod = GenerateGetMethod(dynamicType, property, field);

            property.SetGetMethod(getMethod);

            return property;
        }

        /// <summary>
        /// Generates a Get method for a property.
        /// </summary>
        /// <param name="dynamicType">A <see cref="TypeBuilder"/> to generate a Get method for.</param>
        /// <param name="property">Property to create a get method for.</param>
        /// <param name="field">Field to access in the method.</param>
        /// <returns>Returns the created method.</returns>
        private static MethodBuilder GenerateGetMethod(TypeBuilder dynamicType, PropertyBuilder property, FieldBuilder field)
        {
            var method = dynamicType.DefineMethod(
                string.Format("get_{0}", property.Name),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName);

            method.SetReturnType(field.FieldType);

            var ilGen = method.GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, field);
            ilGen.Emit(OpCodes.Ret);

            return method;
        }

        /// <summary>
        /// Generates attributes for a type.
        /// </summary>
        /// <param name="dynamicType">A <see cref="TypeBuilder"/> to generate the attributes for.</param>
        private static void GenerateClassAttributes(TypeBuilder dynamicType)
        {
            var attributeType = typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute);
            var compilerGeneratedAttribute = new CustomAttributeBuilder(attributeType.GetConstructor(new Type[0]), new object[0]);

            dynamicType.SetCustomAttribute(compilerGeneratedAttribute);

            attributeType = typeof(DebuggerDisplayAttribute);

            var typeProperty = attributeType.GetProperty("Type");

            var debuggerDisplayAttribute = new CustomAttributeBuilder(
                attributeType.GetConstructor(new[] { typeof(string) }),
                new object[] { "{ToString()}" },
                new[] { typeProperty },
                new object[] { "<Anonymous Type>" });

            dynamicType.SetCustomAttribute(debuggerDisplayAttribute);
        }

        /// <summary>
        /// Adds a <see cref="DebuggerHiddenAttribute"/> to a method.
        /// </summary>
        /// <param name="method">The method to add the attribute.</param>
        private static void AddDebuggerHiddenAttribute(MethodBuilder method)
        {
            var attributeType = typeof(DebuggerHiddenAttribute);
            var attribute = new CustomAttributeBuilder(attributeType.GetConstructor(new Type[0]), new object[0]);

            method.SetCustomAttribute(attribute);
        }

        /// <summary>
        /// Adds a <see cref="DebuggerHiddenAttribute"/> to a constructor.
        /// </summary>
        /// <param name="constructor">The constructor to add the attribute.</param>
        private static void AddDebuggerHiddenAttribute(ConstructorBuilder constructor)
        {
            var attributeType = typeof(DebuggerHiddenAttribute);
            var attribute = new CustomAttributeBuilder(attributeType.GetConstructor(new Type[0]), new object[0]);

            constructor.SetCustomAttribute(attribute);
        }

#pragma warning restore 168

        #endregion

        #endregion
    }
}
