using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;

public class XMLUtil {
    public static XmlDocument LoadAndValidateXml(string xmlFilename, TextAsset schemaFile)
    {
        return LoadAndValidateXml(xmlFilename, schemaFile, new ValidationEventHandler(BasicValidationHandler));
    }

    public static XmlDocument LoadAndValidateXml(string xmlFilename, TextAsset schemaFile, ValidationEventHandler validationEventHandler)
    {
        XmlTextReader textReader = null;
        XmlValidatingReader validatingReader = null;

        MemoryStream fs = null;
        try
        {
            textReader = new XmlTextReader(xmlFilename);
            validatingReader = new XmlValidatingReader(textReader);

            validatingReader.ValidationType = ValidationType.Schema;
            validatingReader.ValidationEventHandler += validationEventHandler;

            fs = new MemoryStream(Encoding.ASCII.GetBytes(schemaFile.text));
            XmlSchema schema = XmlSchema.Read(fs, validationEventHandler);

            validatingReader.Schemas.Add(schema);

            XmlDocument result = new XmlDocument();
            result.Load(validatingReader);

            Debug.Log("XML validation finished for " + xmlFilename + "!");

            fs.Close();
            validatingReader.Close();
            textReader.Close();

            return result;
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError("Could not find file: " + e.FileName);
            if(fs != null) fs.Close();
            if(validatingReader != null) validatingReader.Close();
            if(textReader != null) textReader.Close();
            return null;
        }
    }

    private static void BasicValidationHandler(object sender, ValidationEventArgs args)
    {
        Debug.Log("VALIDATION ERROR!");
        Debug.Log(string.Format("\tSeverity:{0}", args.Severity));
        Debug.Log(string.Format("\tMessage:{0}", args.Message));
    }
}
