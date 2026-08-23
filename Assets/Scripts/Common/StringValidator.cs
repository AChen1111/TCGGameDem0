public static class StringValidator
{
    public static bool IsEnglishOrNumber(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (char c in value)
        {
            if ((c < 'a' || c > 'z') &&
                (c < 'A' || c > 'Z') &&
                (c < '0' || c > '9'))
            {
                return false;
            }
        }

        return true;
    }
}
