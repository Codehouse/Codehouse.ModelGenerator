# Codehouse.ModelGenerator

## Solution Structure

The solution is split up into a few main projects:

1. `ModelGenerator` - holds the console application entry point
2. `ModelGenerator.Framework` - holds the “common” and/or “base” functionality
  - Common functionality is considered to be anything that is not specific to a provider, for example, converting `Item` objects into `Template` objects.
  - Base functionality is generally incomplete and is expected to be supplemented with or implemented by a provider.
3. Provider projects including:
  - `ModelGenerator.Tds` - an input provider for consuming TDS files.
  - `ModelGenerator.Scs` - an input provider for consuming SCS files.
  - `ModelGenerator.Fortis` - an output provider for outputting Fortis models.
  - `ModelGenerator.Ids` - an output provider for outputting template and field ID classes.

## Application Start-up

Application start-up operations are fairly simple and include:

1. Clearing any previous log file
2. Loading all the configuration layers
3. Loading the appropriate input/output providers

## Generation Process

The entire process of code generation is represented in the application as a sequence of activities. 
Each activity requires the output of one (or more) activities that come before it. 
Some of the activites are fully implemented within the Framework project as “common functionality” (see above), 
while others are provided as “base functionality” with the expectation that a provider will provide the full implementation.

Each activity is listed below by order of execution.

## File Scan Activity

The “file scan” activity requires no input from a prior activity, as there are none. 
However, it does consume a list of sources from the `ISourceProvider` implementation of the configured input provider.

The file scan activity iterates over the provided sources and scans the associated directories for serialised item files. 
For each located source, the activity returns a `FileSet` object containing a list of all of the files that it contains, 
as well as any pertinent metadata from the source (e.g. metadata from a TDS project file).

The file scan activity provides a partial base implementation, but expects the input provider to supplement it with a complete 
implementation as by itself a base implementation would be unable to identify any metadata that may be relevant.

## File Parse Activity

The “file parse” activity requires a collection of FileSet objects as an input, which is obtained from the previous activity.

The file parse activity iterates over the provided file sets and parses each file into an Item object (comprising the item, its versions, and its field values).  These are then returned in ItemSet objects (mapped 1-to-1 with the input file sets).

The file parse activity provides a minimal base implementation, and expects the input provider to fully implement it.

## Database Activity

The database activity is responsible for converting a disparate set of Item objects into a unified IDatabase object.  This is necessary for performing de-duplication, lookups, and also for the ability to perform hierarchical operations (e.g. getting a parent or getting children).

Obviously, the database can only contain items which are present in serialised form, and depending on the input provider this may not necessarily be all items.  There is a small facility for ensuring that “well-known” items (e.g. some of the top-most items) are pre-populated.

The output IDatabase object will then be used in further activities.

## Template Activity

The template activity is responsible for converting an IDatabase object into a TemplateCollection object; that is, a collection of Template objects.  These will reflect the template structure within Sitecore (based on the serialised items), and will show things such as base templates, inherited fields, and so on.

## Type Activity

The type activity is responsible for converting the products from the previous activities and converting those into models representing types and files.

The activity typically should not require modification.

## Generation Activity

The generation activity contains most of the output provider specific logic.  It takes in type models (which also contain template models) and converts these into C# file outputs.

This phase has a fully implemented engine, but obviously requires provider-specific implementations.

### File Types

The generation activity has the concept of file types (based on `IFileType`).  It comes with one by default (`DefaultFile`), which represents the “normal” file that gets created named based on the template.

If your provider requires different or multiple files (e.g. if you wanted to put your interface and class into separate files), then you can create your own custom file types (implementing `IFileType`) and the create & register an `IFileFactory<TFile>` that emits objects of that type.

If you are registering your own file type, then you must also provide an `IFileGenerator<TFile>` for that file type.  A `FileGeneratorBase<TFile>` exists, which may be useful for providing some base functionality, but the interface exists if not.

### Custom Types

If you are using the DefaultFile, or a file type based on `DefaultFileGenerator<TFile>`, then implementing custom types is straight-forward.  If you have implemented your own `FileGeneratorBase<TFile>`, then this process is per your implementation.

If you expect that you will need additional using statements, then you can implement and register an `IUsingsGenerator<DefaultFile>`, which will add using statements to files of the `DefaultFile` type.

If you wish to add additional types to the a `DefaultFile`, then you simply implement and register an `ITypeGenerator<DefaultFile>`, this is responsible for generating the necessary syntax tree and indicating the namespace that this type belongs to.

### Syntax Rewriting

The default syntax tree that is built by the `Microsoft.CodeAnalysis` library occasionally leaves something to be desired when it comes to formatting.

Formatting in general is fairly finnicky, but the tool requests any registered `IRewriter` implementations.

It is strongly recommended that you make these as simple as possible, only addressing a single point of formatting in each.

These rewriters are run (in no specific order) upon the output tree, and so allow for any final modifications or tweaks to be made.
