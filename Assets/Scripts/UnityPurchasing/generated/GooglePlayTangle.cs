// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("l4Y+TZiadC8DJ8WVVQvQ09NY9yP8u+0NC+UmDO96GgohOjRnwhWqX7YEh6S2i4CPrADOAHGLh4eHg4aFy7ThjhgDD58jLxJq8J4qhYI9/emufzPOAbPhAbt2L7lVDlTeG5KdyTpiSTTOow4YpSu0eDtqZPWwseOmJd0Biz/B2zOFOsalx7T44Xeauj1/fbBfeawC/pUyobdzBmLoUOKf1JYh/JGJ70zqTJtYshXUXdZJwf2T4az92T6w+txWIpEpFOGO4BP7bMBwRao9krqYAR1D09gIZYI+u/lGuvLXSJQgASNBhWoC9YmUuqC5bx+jBIeJhrYEh4yEBIeHhgNGHroMLu6YSc5x6X1aLNhlSKMKs5E8BwD+RMG9Ymjcsi7MjYSFh4aH");
        private static int[] order = new int[] { 9,12,9,10,8,9,13,9,13,11,11,11,12,13,14 };
        private static int key = 134;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
