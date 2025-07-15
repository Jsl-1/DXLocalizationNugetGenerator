using DXLocalizationNugetGenerator.Abstractions;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static DXLocalizationNugetGenerator.Model.Data;

namespace DXLocalizationNugetGenerator.Command
{
    public class CreateNuspec : BaseCommand
    {
        public override string CommandDescription => "Creates DX nuspec localization files from DX nuget packages.";

        public CreateNuspec() : base()
        {
            HasRequiredOption("inputDXNuGetPath=", "The full path of the directory that contains DevExpress nuget packages.", t => NugetPackagesPath = t);
            HasRequiredOption("inputLocalizationNetFrameworkPath=", "The full path of DevExpress localization libraries.", t => LocalizationDllPathNetFramework = t);
            HasRequiredOption("inputLocalizationNetCorePath=", "The full path of DevExpress localization libraries.", t => LocalizationDllPathNetCore = t);
            HasRequiredOption("outputLanguageCode=", "The two-letter language code.", t => LanguageCode = t);
            HasRequiredOption("outputNuspecPath=", "The output nuspec path.", t => OutputNuspecPath = t);
            HasOption("r|revision=", "The revision version number of the package.", t => Revision = Int32.Parse(t));
        }

        #region CONSTANTS

        public const string DEFAULT_INPUT_LANGUAGE = "de";

        #endregion

        #region PARAMETERS

        /// <summary>
        /// Gets or sets the output nuspec path.
        /// </summary>
        /// <value>
        /// The output nuspec path.
        /// </value>
        public string OutputNuspecPath { get; set; }

        /// <summary>
        /// Gets or sets the localization DLL path.
        /// </summary>
        /// <value>
        /// The localization DLL path.
        /// </value>
        public string LocalizationDllPathNetFramework { get; set; }

        /// <summary>
        /// Gets or sets the localization DLL path.
        /// </summary>
        /// <value>
        /// The localization DLL path.
        /// </value>
        public string LocalizationDllPathNetCore { get; set; }

        /// <summary>
        /// Gets or sets the nuget packages path.
        /// </summary>
        /// <value>
        /// The nuget packages path.
        /// </value>
        public string NugetPackagesPath { get; set; }

        /// <summary>
        /// Gets or sets the language code.
        /// </summary>
        /// <value>
        /// The language code.
        /// </value>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Get or set the minor version added to the package.
        /// Allow to update translation without changing devexpress version.
        /// </summary>
        public Int32? Revision { get; set; }

        #endregion PARAMETERS

        public override int Execute(string[] remainingArguments)
        {
            ValidateParameters();

            var packages = FindNugetPackages(NugetPackagesPath);

            if (packages.Count() == 0) 
            { 
                ConsoleWrite("There are no nuget packages.");
                return 0;
            }

            CreateNuspecFiles(packages, LocalizationDllPathNetFramework, LocalizationDllPathNetCore, OutputNuspecPath, LanguageCode);

            return 0;
        }

        void ValidateParameters()
        {
            if (!Directory.Exists(OutputNuspecPath))
            {
                Directory.CreateDirectory(OutputNuspecPath);
            }

            if (!Directory.Exists(LocalizationDllPathNetFramework))
            {
                throw new DirectoryNotFoundException("Directory with localization .net framework libraries does not exist.");
            }

            if (!Directory.Exists(LocalizationDllPathNetCore))
            {
                throw new DirectoryNotFoundException("Directory with localization .net core libraries does not exist.");
            }

            if (!Directory.Exists(NugetPackagesPath))
            {
                throw new DirectoryNotFoundException("Directory with DevExpress Nuget packages does not exist.");
            }
        }

        string[] FindNugetPackages(string nugetPackagesPath)
        {
            string pattern = "*." + DEFAULT_INPUT_LANGUAGE + ".*" + ".nupkg";
            string[] files = Directory.GetFiles(nugetPackagesPath, pattern);

            return files;
        }

        void CreateNuspecFiles(string[] nugetPackages, string localizationLibrariesNetFrameworkPath, string localizationLibrariesNetCorePath, string outputDirectory, string languageCode)
        {
            foreach (string nugetPackage in nugetPackages)
            {
                using ZipArchive zip = ZipFile.OpenRead(nugetPackage);

                var dlllibEntryList = zip.Entries.Where(w => w.FullName.StartsWith("lib/") && w.Name.EndsWith(".dll"));

                var nuspecFile = zip.Entries.FirstOrDefault(f => f.Name.EndsWith(".nuspec"));

                if (nuspecFile == null)
                {
                    /*
                     * nuspec not found in nupkg; skip
                     */

                    continue;
                }

                /*
                 * Prepare new nuspec filename and path.
                 */
                string nuspecFileLocalizedName = _ReplaceLanguage(nuspecFile.Name, languageCode);
                string nuspecFileLocalizedPath = Path.Combine(OutputNuspecPath, nuspecFileLocalizedName);

                /*
                 * Extract nuspec.
                 */
                nuspecFile.ExtractToFile(nuspecFileLocalizedPath, true);

                FilesRoot root = new FilesRoot();

                /*
                 * Create files node.
                 */
                foreach (var dlllibEntry in dlllibEntryList)
                {

                    var dllDirectory = dlllibEntry.FullName.StartsWith("lib/net462") ?
                        localizationLibrariesNetFrameworkPath :
                        localizationLibrariesNetCorePath;

                    XmlFile xmlFile = new XmlFile()
                    {
                        Src = Path.Combine(Path.GetRelativePath(OutputNuspecPath, dllDirectory), dlllibEntry.Name),
                        Target = dlllibEntry.FullName,
                    };
                    root.XmlFiles.Add(xmlFile);
                }

                /*
                 * Convert data to xml.
                 */
                string filesElementToXml = string.Empty;

                using (var stringwriter = new System.IO.StringWriter())
                {
                    StringBuilder sb = new StringBuilder();
                    using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings() { OmitXmlDeclaration = true }))
                    {
                        new XmlSerializer(root.GetType()).Serialize(writer, root);
                    }
                    filesElementToXml = sb.ToString();
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(nuspecFileLocalizedPath);

                // Gérer l'espace de noms par défaut
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd");

                if (dlllibEntryList.Any())
                {
                    doc["package"].InnerXml = doc["package"].InnerXml + filesElementToXml;
                }

                //Gère la version du package
                var versionElement = doc.SelectSingleNode("//ns:metadata/ns:version", nsmgr) as XmlElement;
                var devExpressVersion = new Version(versionElement.InnerText);
                var localizationPackageVersion = devExpressVersion;
                
              
                if (Revision.HasValue)
                {
                   
                    var newVersion = new Version(localizationPackageVersion.Major, localizationPackageVersion.Minor, localizationPackageVersion.Build, Revision.Value);
                    versionElement.InnerText = newVersion.ToString();
                    localizationPackageVersion = newVersion;
                }

                _ReplaceLanguage(doc, nsmgr, LanguageCode);
                _FixNet50(doc);
                _ReplaceLocalizationPackageVersions(doc, nsmgr, LanguageCode, localizationPackageVersion);
                _ReplaceDevExpressDependencyVersions(doc, nsmgr, LanguageCode, devExpressVersion);


                doc.Save(nuspecFileLocalizedPath);
            }
        }

        private void _FixNet50(XmlDocument doc)
        {
            var text = doc.InnerXml;

            text = text.Replace("/net5.0-windows/", "/net5.0-windows7.0/");
            text = text.Replace("\"net5.0-windows\"", "\"net5.0-windows7.0\"");

            text = text.Replace("/net6.0-windows/", "/net6.0-windows7.0/");
            text = text.Replace("\"net6.0-windows\"", "\"net6.0-windows7.0\"");

            text = text.Replace("/net8.0-windows/", "/net8.0-windows7.0/");
            text = text.Replace("\"net8.0-windows\"", "\"net8.0-windows7.0\"");

            text = text.Replace("/net9.0-windows/", "/net9.0-windows7.0/");
            text = text.Replace("\"net9.0-windows\"", "\"net9.0-windows7.0\"");

            doc.InnerXml = text;
        }

        private void _ReplaceLocalizationPackageVersions(XmlDocument doc, XmlNamespaceManager nsmgr, string language, Version packageVersion)
        {
            if (Revision.HasValue)
            {
                //Find all dependency nodes in the nuspec file with id ending with the language code
                XmlElement root = doc.DocumentElement;
                var dependencies = root.SelectNodes($"//ns:dependency", nsmgr);
                foreach (var node in dependencies.OfType<XmlElement>())
                {
                    var id = node.GetAttribute("id");
                    if (!String.IsNullOrWhiteSpace(id) && id.EndsWith($".{language}"))
                    {
                        node.SetAttribute("version", $"[{packageVersion}]");
                    }
                }
            }
        }

        private void _ReplaceDevExpressDependencyVersions(XmlDocument doc, XmlNamespaceManager nsmgr, string language, Version devExpressVersion)
        {
            if (Revision.HasValue)
            {
                //Find all dependency nodes in the nuspec file with id ending with the language code
                XmlElement root = doc.DocumentElement;
                var dependencies = root.SelectNodes($"//ns:dependency", nsmgr);
                foreach (var node in dependencies.OfType<XmlElement>())
                {
                    var id = node.GetAttribute("id");
                    if (!String.IsNullOrWhiteSpace(id) && !id.EndsWith($".{language}"))
                    {
                        //On rend le package compatiblie avec toutes les hot fix et mise à jour mineures
                        var nextDevExpressVersion = new Version(devExpressVersion.Major, devExpressVersion.Minor + 1);
                        var newVersionString = $"[{devExpressVersion},{nextDevExpressVersion})";
                        node.SetAttribute ("version", newVersionString );
                    }
                }
            }
        }

        private String _ReplaceLanguage(string text, string language)
        {
            // .de.
            text = text.Replace("." + DEFAULT_INPUT_LANGUAGE + ".", "." + language + ".");
            // .de<
            text = text.Replace("." + DEFAULT_INPUT_LANGUAGE + "<", "." + language + "<");
            // .de"
            text = text.Replace("." + DEFAULT_INPUT_LANGUAGE + "\"", "." + language + "\"");
            // /de/
            text = text.Replace("/" + DEFAULT_INPUT_LANGUAGE + "/", "/" + language + "/");
            // >de<
            text = text.Replace(">" + DEFAULT_INPUT_LANGUAGE + "<", ">" + language + "<");

            return text;
        }

        void _ReplaceLanguage(XmlDocument doc, XmlNamespaceManager nsmgr, string language)
        {
            doc.InnerXml = _ReplaceLanguage(doc.InnerXml, language);
        }

    }
}
