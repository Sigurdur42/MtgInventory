namespace MkmApi
{
    public static class MkmAuthenticationDataExtension
    {
        public static bool IsValid(this MkmAuthenticationData data) => data?.AppToken != null && data?.AppSecret != null;
    }
}