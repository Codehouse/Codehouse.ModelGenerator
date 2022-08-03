using ModelGenerator.Framework.CodeGeneration.FileTypes;

namespace ModelGenerator.Framework.CodeGeneration
{
    /// <summary>
    ///     Defines a process for generating types within a file of type
    ///     <typeparamref name="TFile"/>.
    ///     <para>
    ///         Types have a defined <see cref="Tag"/>, which can be used
    ///         to control file ordering, and belong to a specified namespace.
    ///     </para>
    /// </summary>
    /// <typeparam name="TFile">The file type</typeparam>
    public interface ITypeGenerator<TFile>
        where TFile : IFileType
    {
        string Tag { get; }

        NamespacedType? GenerateType(TFile file);
    }
}