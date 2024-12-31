namespace LXGaming.Hosting.Generators.Utilities {

    public static class StringUtils {

        public static string Format(string format, params string[] args) {
            for (var index = 0; index < args.Length; index++) {
                format = format.Replace($"{{{index}}}", args[index]);
            }

            return format;
        }
    }
}