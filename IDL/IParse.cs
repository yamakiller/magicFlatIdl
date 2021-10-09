namespace IDL
{
    public interface IBParse
    {
        bool Parse(string filename, string name, string bodys);
        string GetName();

        string CreateCodeForLanguage(ELanguage language);
    }
}