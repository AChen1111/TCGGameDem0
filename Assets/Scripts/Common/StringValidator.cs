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

    public static bool IsWeakPassword(string value)
    {
        bool hasLetter = false;
        bool hasNumberOrSymbol = false;

        foreach (char character in value)
        {
            if (char.IsLetter(character))
            {
                hasLetter = true;
            }
            else
            {
                hasNumberOrSymbol = true;
            }
        }

        return !hasLetter || !hasNumberOrSymbol;
    }
}
