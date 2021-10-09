using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Libs;

namespace IDL
{
    public class Builder
    {

        string m_inFilePath = ".";
        string m_outFilePath = ".";
        string m_fileName;
        ELanguage m_language;

        public string ParseFile { set { m_inFilePath = value; } }
        public string OutputFilePath { set { m_outFilePath = value; } }
        public string FileName { set { m_fileName = value; } }

        public Builder(string language)
        {
            switch (language.ToLower().Trim())
            {
                case "go": m_language = ELanguage.CL_GOLANG; golangVariableInit(); break;
                case "cpp": m_language = ELanguage.CL_CPP; cppVariableInit(); break;
                case "csharp": m_language = ELanguage.CL_SHARP; csharpVariableInit(); break;
                case "java": m_language = ELanguage.CL_JAVA; javaVariableInit(); break;
                default: m_language = ELanguage.CL_GOLANG; golangVariableInit(); break;
            }
        }

        public void StartParse()
        {
            string text = "";
            try
            {
                text = System.IO.File.ReadAllText(m_inFilePath);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("read file fail:" + e.Message);
                return;
            }
            this.parse(text);
        }

        bool parse(string data)
        {
            data = Regex.Replace(data, @"\/\/[^\n]*", "");
            data = Regex.Replace(data, @"[\n\r\t]", "");
            data = Regex.Replace(data, @"\s{2,}", "");

            string[] classes = Regex.Split(data, @"[@\}]");

            if (classes.Length == 0)
            {
                throw new System.Exception("parse classes is failed, no class struct!!");
            }


            foreach (string c in classes)
            {
                string[] symbolFlags = c.Split('{');
                if (symbolFlags.Length != 2)
                {
                    continue;
                }
                string[] symbolAttrs = symbolFlags[0].Trim().Split(":");
                if (symbolAttrs.Length != 2)
                {
                    throw new Exception("parse symbol attr is failed, symbol missing:" + symbolFlags[0].Trim());
                }

                IBParse idlParse;
                switch (symbolAttrs[0].Trim())
                {
                    case Symbol.Struct:
                        idlParse = new ParseStruct();
                        break;
                    case Symbol.Namespace:
                        idlParse = new ParseNamespace();
                        break;
                    default:
                        throw new Exception("parse symbol attr is error, symbol: " + symbolAttrs[0].Trim());
                }

                if (idlParse.Parse(m_fileName, symbolAttrs[1].Trim(), symbolFlags[1].Trim()))
                {
                    switch (symbolAttrs[0].Trim())
                    {
                        case Symbol.Struct:
                            Vars.RegisterStruct(idlParse.GetName(), idlParse);
                            break;
                        case Symbol.Namespace:
                            Vars.RegisterNamespace(idlParse);
                            break;
                    }
                }
            }

            createCode();
            return true;
        }


        void createCode()
        {
            if (!Directory.Exists(m_outFilePath))
                Directory.CreateDirectory(m_outFilePath);
            if (!Directory.Exists(m_outFilePath + "/" + Vars.GetNamespace().GetName()))
                Directory.CreateDirectory(m_outFilePath + "/" + Vars.GetNamespace().GetName());


            string flatbufferCode;
            flatbufferCode = "namespace " + Vars.GetNamespace().GetName() + ";\n\n";
            flatbufferCode += "//magic flat idl Automatic generation fbs" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "\n\n";
            flatbufferCode += "attribute \"priority\";\n\n";
            foreach (KeyValuePair<string, IBParse> pair in Vars.GetStructs())
            {
                flatbufferCode += FlatbufferCode.CreateFlatbufferCode((ParseStruct)pair.Value);
            }


            switch (m_language)
            {
                case ELanguage.CL_SHARP:
                    CodeCSharp.CreateCode(m_outFilePath);
                    break;
                case ELanguage.CL_GOLANG:
                    CodeGolang.CreateCode(m_outFilePath);
                    break;
                case ELanguage.CL_JAVA:
                    CodeJava.CreateCode(m_outFilePath);
                    break;
                case ELanguage.CL_CPP:
                    CodeCPP.CreateCode(m_outFilePath);
                    break;
            }

            FileSave.Save(m_outFilePath + "/" + m_fileName + ".fbs", flatbufferCode);
            Proc.RunExe("flatc.exe", "--go -o " + m_outFilePath + "/ " + m_outFilePath + "/" + m_fileName + ".fbs");
        }


        void golangVariableInit()
        {
            Vars.RegisterVariable("bool", "bool");
            Vars.RegisterVariable("byte", "int8");
            Vars.RegisterVariable("ubyte", "uint8");
            Vars.RegisterVariable("short", "int16");
            Vars.RegisterVariable("ushort", "uint16");
            Vars.RegisterVariable("int", "int32");
            Vars.RegisterVariable("uint", "uint32");
            Vars.RegisterVariable("int32", "int32");
            Vars.RegisterVariable("uint32", "uint32");
            Vars.RegisterVariable("long", "int64");
            Vars.RegisterVariable("ulong", "uint64");
            Vars.RegisterVariable("float", "float32");
            Vars.RegisterVariable("double", "float64");
            Vars.RegisterVariable("string", "string");
        }

        void cppVariableInit()
        {
            //Vars.RegisterVariable("bool", "bool");
            //Vars.RegisterVariable("byte", "int8_t");
        }

        void csharpVariableInit()
        {

        }

        void javaVariableInit()
        {

        }


        string extension()
        {
            switch (m_language)
            {
                case ELanguage.CL_GOLANG:
                    return ".go";
                case ELanguage.CL_SHARP:
                    return ".cs";
                case ELanguage.CL_CPP:
                    return ".cpp";
                case ELanguage.CL_JAVA:
                    return ".java";
                default:
                    return ".go";
            }
        }
    }
}