using System.Reflection;
using Inedo.Extensibility;

[assembly: AssemblyTitle("Gitea")]
[assembly: AssemblyDescription("Provides integration with Gitea.")]
[assembly: AssemblyCompany("Inedo, LLC")]
[assembly: AssemblyCopyright("Copyright © Inedo 2026")]
[assembly: AssemblyProduct("any")]

// Not for ProGet
[assembly: AppliesTo(InedoProduct.BuildMaster | InedoProduct.Otter)]

[assembly: ScriptNamespace("Gitea")]

[assembly: AssemblyVersion("4.0.0")]
[assembly: AssemblyFileVersion("4.0.0")]
