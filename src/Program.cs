using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace FhirResourceExamples
{
    class Program
    {
        const string URGENCY_PRIORTY_EXTENSION = "http://helsenorge.no/fhir/StructureDefinition/urgency-priority";
        const string URGENCY_PRIORITY_CODESYSTEM = "http://helsenorge.no/fhir/CodeSystem/urgency-priority";

        const string NATIONAL_IDENTITY_OID = "2.16.578.1.12.4.1.4.1";
        const string D_NUMBER_OID = "2.16.578.1.12.4.1.4.2";
        const string COMMON_HELP_NUMBER_OID = "2.16.578.1.12.4.1.4.3";
        const string HPR_NUMBER_OID = "2.16.578.1.12.4.1.4.4";
        const string DUF_NUMBER_OID = "2.16.578.1.12.4.1.4.5";

        static void Main(string[] args)
        {
            var documentReferenceGuid = Guid.NewGuid().ToString("N");
            var buffer = ReadFile("example.pdf");

            var documentReference = new DocumentReference
            {
                Status = DocumentReferenceStatus.Current,
                DocStatus = CompositionStatus.Final,
                Description = "Some Descriptive Text",
                Author = new List<ResourceReference> { new ResourceReference { Identifier = new Identifier { System = $"urn:oid:{NATIONAL_IDENTITY_OID}", Value = "031094XXXXX", }, Display = "Ilyas Jens Kristiansen", } },
                Content = new List<DocumentReference.ContentComponent> { new DocumentReference.ContentComponent { Attachment = new Attachment { Data = buffer, ContentType = "application/pdf" } } },
            };

            var serviceRequestGuid = Guid.NewGuid().ToString("N");
            var serviceRequest = new ServiceRequest
            {
                Extension = new List<Extension> { new Extension(URGENCY_PRIORTY_EXTENSION, new CodeableConcept(URGENCY_PRIORITY_CODESYSTEM, "urgency2", "Hastegrad 2")) },
                Priority = RequestPriority.Urgent,
                SupportingInfo = new List<ResourceReference> { new ResourceReference { Reference = $"urn:uuid:{documentReferenceGuid}" } }
            };

            var bundle = new Bundle
            {
                Id = "bundle-transaction",
                Type = Bundle.BundleType.Transaction,
                Entry = new List<Bundle.EntryComponent>
                {
                    new Bundle.EntryComponent
                    {
                        FullUrl = $"urn:uuid:{serviceRequestGuid}",
                        Resource = serviceRequest,
                        Request = new Bundle.RequestComponent
                        {
                            Method = Bundle.HTTPVerb.POST,
                            Url = "ServiceRequest",
                        },
                    },
                    new Bundle.EntryComponent
                    {
                        FullUrl = $"urn:uuid:{documentReferenceGuid}",
                        Resource = documentReference,
                        Request = new Bundle.RequestComponent
                        {
                            Method = Bundle.HTTPVerb.POST,
                            Url = "DocumentReference",
                        },
                    },
                },
            };

            SerializeResourceToDiskAsJson(bundle, "bundle-transaction.json");
            SerializeResourceToDiskAsXml(bundle, "bundle-transaction.xml");
        }

        static byte[] ReadFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new ArgumentException("", nameof(path));

            var fileStream = File.OpenRead(path);
            var buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, (int)fileStream.Length);
            return buffer;
        }

        static void SerializeResourceToDiskAsJson(Resource resource, string path, bool pretty = true)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            using var writer = new JsonTextWriter(new StreamWriter(path));
            var serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = pretty });
            serializer.Serialize(resource, writer);
        }

        static void SerializeResourceToDiskAsXml(Resource resource, string path, bool pretty = true)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            using var writer = new XmlTextWriter(new StreamWriter(path));
            var serializer = new FhirXmlSerializer(new SerializerSettings { Pretty = pretty });
            serializer.Serialize(resource, writer);
        }
    }
}
