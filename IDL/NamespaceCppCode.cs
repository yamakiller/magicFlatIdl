
namespace IDL
{
    public class NamespaceCppCode
    {
        public static string CreateSpaceCode(ParseNamespace namespaceInterface)
        {
            return "namespace " + namespaceInterface.GetName();
        }
    }
}