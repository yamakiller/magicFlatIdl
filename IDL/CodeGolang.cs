using System;
using System.Collections.Generic;
using Libs;

namespace IDL
{
    public class CodeGolang
    {
        public static void CreateCode(string outPath)
        {
            string structCode = "";
            foreach (KeyValuePair<string, IBParse> pair in Vars.GetStructs())
            {
                ParseStruct structInterface = (ParseStruct)pair.Value;
                structCode = "//magic flat idl Automatic generation struct[golang]-" + structInterface.GetName() + ":" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "\n\n";
                structCode += Vars.GetNamespace().CreateCodeForLanguage(ELanguage.CL_GOLANG);

                structCode += "import (\n";
                structCode += "\tflatbuffers \"github.com/google/flatbuffers/go\"\n";
                structCode += ")\n\n\n";

                structCode += structInterface.CreateCodeForLanguage(ELanguage.CL_GOLANG);
                structCode += "\n";
                //解码
                string flatVarNameD = "ret" + structInterface.GetName() + "FB";
                string structVarNameD = "ret" + structInterface.GetName();
                structCode += "func " + StringTo.ToUpper(structInterface.GetName(), 0) + "Deserialize(data []byte) " + structInterface.GetName() + "{\n";
                structCode += "\t" + flatVarNameD + " := GetRootAs" + structInterface.GetName() + "FB(data,0)\n";
                structCode += "\t" + structVarNameD + " := " + structInterface.GetName() + "{}\n";
                structCode += ParseStruct.CreateDeserializeCodeForFlatbuffer(structInterface, structVarNameD, flatVarNameD);
                structCode += "\treturn " + structVarNameD + "\n";
                structCode += "}\n";
                //编码
                structCode += "func " + StringTo.ToUpper(structInterface.GetName(), 0) +
                            "Serialize(builder *flatbuffers.Builder, data " + structInterface.GetName() + ") ([]byte, error) {\n";
                structCode += ParseStruct.CreateSerializeCodeForFlatbuffer(structInterface, "data");
                structCode += "\n";
                structCode += "\tbuilder.Finish(dataPos)\n";
                structCode += "\treturn builder.FinishedBytes(), nil\n";
                structCode += "}\n";


                FileSave.Save(outPath + "/" + Vars.GetNamespace().GetName() + "/" + structInterface.GetName() + ".go", structCode);
            }
        }
    }
}