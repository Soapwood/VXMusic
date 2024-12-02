namespace VXMusic.Connections.Tidal;

public class TidalUtil
{
    internal static class Ensure
    {
        /// <summary>Checks an argument to ensure it isn't null.</summary>
        /// <param name="value">The argument value to check</param>
        /// <param name="name">The name of the argument</param>
        public static void ArgumentNotNull(object value, string name)
        {
            if (value == null)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        ///   Checks an argument to ensure it isn't null or an empty string
        /// </summary>
        /// <param name="value">The argument value to check</param>
        /// <param name="name">The name of the argument</param>
        public static void ArgumentNotNullOrEmptyString(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("String is empty or null", name);
        }

        public static void ArgumentNotNullOrEmptyList<T>(IEnumerable<T> value, string name)
        {
            if (value == null || !value.Any<T>())
                throw new ArgumentException("List is empty or null", name);
        }
    }
}