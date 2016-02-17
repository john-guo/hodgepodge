using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace My.ExpressionParser
{
    /// <summary>
    /// A class that holds a <see cref="AssemblyBuilder">dynamic assembly</see>.
    /// </summary>
    internal class DynamicAssemblyHolder
    {
        #region Singleton Instance

        public static DynamicAssemblyHolder Instance
        {
            get { return _instance.Value; }
        }

        #endregion

        #region Fields

        private static readonly Lazy<DynamicAssemblyHolder> _instance = new Lazy<DynamicAssemblyHolder>(() => new DynamicAssemblyHolder());
        private static int _nextInstanceId = -1;

        private readonly AssemblyBuilder _assembly;
        private readonly ModuleBuilder _moduleBuilder;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a <see cref="ModuleBuilder"/> to create types in it. 
        /// </summary>
        public ModuleBuilder ModuleBuilder
        {
            get { return _moduleBuilder; }
        }

        #endregion

        #region Constuctors and Finalizers

        /// <summary>
        /// Private constructor to avoid external instantiation.
        /// </summary>
        private DynamicAssemblyHolder()
        {
            // Get the current AppDomain
            var appDomain = AppDomain.CurrentDomain;

            // Create a new dynamic assembly
            var assemblyName = new AssemblyName
            {
                Name = "CdcSoftware.Shared.IdeaBlade.DynamicTypes",
                Version = new Version("1.0.0.0")
            };

            _assembly = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            // create a new module to hold code in the assembly
            _moduleBuilder = _assembly.GetDynamicModule("CdcSoftware.Shared.IdeaBlade.DynamicTypes") ??
                             _assembly.DefineDynamicModule("CdcSoftware.Shared.IdeaBlade.DynamicTypes");
        }

        #endregion

        #region Methods

        public string GenerateAnonymousTypeName()
        {
            var instanceId = Interlocked.Increment(ref _nextInstanceId);
            return AnonymousTypeHelper.AnonymousTypeNamePrefix + instanceId.ToString("x");
        }

        #endregion
    }
}
