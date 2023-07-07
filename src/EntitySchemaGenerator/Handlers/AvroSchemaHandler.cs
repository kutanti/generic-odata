using Chr.Avro.Abstract;
using Chr.Avro.Codegen;
using Chr.Avro.Representation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EntitySchemaGenerator.Handlers
{
    /// <summary>
    /// Handles the Avro schema code gen
    /// </summary>
    public class AvroSchemaHandler
    {
        private readonly static CSharpCodeGenerator CodeGenerator = new CSharpCodeGenerator();
        private readonly static JsonSchemaReader SchemaReader = new JsonSchemaReader();
        static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        private readonly IConfiguration _configuration;
        public AvroSchemaHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates the C# class from Avro Schema.
        /// </summary>
        /// <param name="schemaStr"></param>
        /// <param name="apiPath"></param>
        public void GenerateClass(string schemaStr, string apiPath)
        {
            var schemaDict = JsonConvert.DeserializeObject<dynamic>(schemaStr);

            var schema = (RecordSchema)SchemaReader.Read(schemaStr);

            string nameSpaceToAppend = string.Join(".",
                apiPath.Split("/", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => textInfo.ToTitleCase(x))
                .SkipLast(1)
                );

            string className = textInfo.ToTitleCase(apiPath.Split('/').Last());

            var cSharpSchema = SchemaForCsharp(schema, nameSpaceToAppend, className);

            GenerateModelClass(cSharpSchema, apiPath);
        }

        private static NamedSchema SchemaForCsharp(NamedSchema schema, string nameSpaceToAppend, string className)
        {
            AlterSchemaForCSharp(schema, nameSpaceToAppend, className: className);
            return schema;
        }

        private static void AlterSchemaForCSharp(
            Schema schema,
            string nameSpaceToAppend,
            ISet<Schema>? seen = null,
            bool childClass = false,
            string className = "")
        {
            seen ??= new HashSet<Schema>();
            if (schema is NamedSchema namedSchema)
            {
                namedSchema.Name = childClass ? textInfo.ToTitleCase(namedSchema.Name) : className;
                namedSchema.Namespace = childClass ? $"Entities.DomainObjects.{nameSpaceToAppend}.InternalClass"
                    : $"Entities.DomainObjects.{nameSpaceToAppend}";
            }

            if (seen.Add(schema))
            {
                switch (schema)
                {
                    case ArraySchema a:
                        AlterSchemaForCSharp(a.Item, nameSpaceToAppend, seen, true);
                        break;

                    case MapSchema m:
                        AlterSchemaForCSharp(m.Value, nameSpaceToAppend, seen, true);
                        break;

                    case RecordSchema r:
                        foreach (var field in r.Fields)
                        {
                            field.Name = field.Name;
                            AlterSchemaForCSharp(field.Type, nameSpaceToAppend, seen, true);
                        }

                        break;

                    case UnionSchema u:
                        foreach (var child in u.Schemas)
                        {
                            AlterSchemaForCSharp(child, nameSpaceToAppend, seen, true);
                        }
                        break;
                }
            }
        }

        private void GenerateModelClass(NamedSchema schema, string fileRelativePath)
        {
            var generatedCodeStr = CodeGenerator.WriteCompilationUnit(schema);
            generatedCodeStr = generatedCodeStr.Replace("global::", "");
            WriteToFile(fileRelativePath, generatedCodeStr);
        }

        private void WriteToFile(string fileRelativePath,  string content)
        {
            string fullFilePath = Path.GetFullPath($@"{_configuration.GetValue<string>("OutputDirectory")}{fileRelativePath}");
            FileInfo fi = new FileInfo($@"{fullFilePath}.cs");
            fi.Directory.Create();

            try
            {
                // Check if file already exists. If yes, delete it.
                if (fi.Exists)
                {
                    fi.Delete();
                }

                File.WriteAllText(fi.FullName, content);
                Console.WriteLine($"Generated class {fi.FullName} successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error Occured while generating {fi.Name}.cs: {e.Message}");
                throw;
            }
        }
    }
}
