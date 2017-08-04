using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RazorEngine.Configuration;

namespace RazorEngine.Compilation.ReferenceResolver
{
    /// <summary>
    /// Resolves the assemblies by using collection of types required by default and currently executing assembly. See <see cref="IReferenceResolver"/>
    /// </summary>
    public class MinimumReferenceResolver : IReferenceResolver
    {
        /// <summary>
        /// See <see cref="IReferenceResolver.GetReferences"/>
        /// </summary>
        /// <param name="context">gives context about the compilation process.</param>
        /// <param name="includeAssemblies">The references that should be included (requested by the compiler itself)</param>
        /// <returns>Default set of references which will be used in the compilation process.</returns>
        public IEnumerable<CompilerReference> GetReferences(TypeContext context,
            IEnumerable<CompilerReference> includeAssemblies = null)
        {
            var defaultAssemblies = TemplateServiceConfiguration.RequiredTypes.Select(dn => dn.Assembly).ToList();
            defaultAssemblies.Add(Assembly.GetExecutingAssembly());

            return defaultAssemblies
                .Where(a => !a.IsDynamic && File.Exists(a.Location) && !a.Location.Contains(CompilerServiceBase.DynamicTemplateNamespace))
                .GroupBy(a => a.GetName().Name)
                .Select(grp => grp.First(y => y.GetName().Version == grp.Max(x => x.GetName().Version))) // only select distinct assemblies based on FullName to avoid loading duplicate assemblies
                .Select(a => CompilerReference.From(a))
                .Concat(includeAssemblies ?? Enumerable.Empty<CompilerReference>());
        }
    }
}
