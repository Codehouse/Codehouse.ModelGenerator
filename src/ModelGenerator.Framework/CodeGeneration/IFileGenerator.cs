using System.IO;
using ModelGenerator.Framework.CodeGeneration.FileTypes;

namespace ModelGenerator.Framework.CodeGeneration
{
    /// <summary>
    /// Generates a file for a given <see cref="IFileType"/>.  This
    /// interface primarily exists to facilitate non-generic to generic
    /// dispatch.
    /// </summary>
    /// <remarks>A generic version of this interface exists
    /// and should be used for implementation, but the non-generic
    /// interface must be used for registration.</remarks>
    public interface IFileGenerator
    {
        public bool CanGenerate(IFileType file);

        public FileInfo? GenerateFile(IFileType file);
    }

    /// <summary>
    /// Generates a file for a <see cref="IFileType"/> specified by
    /// <typeparamref name="TFile"/>.
    /// <para>Implementors are recommended to use
    /// <see cref="FileGeneratorBase{TFile}"/>.</para>
    /// </summary>
    public interface IFileGenerator<TFile> : IFileGenerator
        where TFile : IFileType
    {
        public FileInfo? GenerateFile(TFile file);
    }
}