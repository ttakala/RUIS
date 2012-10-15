using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;

public class XMLUtil {
    public static XmlDocument LoadAndValidateXml(string xmlFilename, string schemaFilename)
    {
        return LoadAndValidateXml(xmlFilename, schemaFilename, new ValidationEventHandler(BasicValidationHandler));
    }

    public static XmlDocument LoadAndValidateXml(string xmlFilename, string schemaFilename, ValidationEventHandler validationEventHandler)
    {
        XmlTextReader textReader = new XmlTextReader(xmlFilename);
        XmlValidatingReader validatingReader = new XmlValidatingReader(textReader);

        validatingReader.ValidationType = ValidationType.Schema;
        validatingReader.ValidationEventHandler += validationEventHandler;

        FileStream fs = new FileStream(schemaFilename, FileMode.Open);
        XmlSchema schema = XmlSchema.Read(fs, validationEventHandler);

        validatingReader.Schemas.Add(schema);

        XmlDocument result = new XmlDocument();
        result.Load(validatingReader);

        Debug.Log("XML validation finished for " + xmlFilename + "!");

        return result;
    }

    private static void BasicValidationHandler(object sender, ValidationEventArgs args)
    {
        Debug.Log("VALIDATION ERROR!");
        Debug.Log(string.Format("\tSeverity:{0}", args.Severity));
        Debug.Log(string.Format("\tMessage:{0}", args.Message));
    }
}
