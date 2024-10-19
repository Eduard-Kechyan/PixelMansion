// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("E6R5FAxqyW/JHt03kFHYU8xEeBb6+DXa/CmHexC3JDL2g+dt1WcaUR3MS/Rs+N+pXeDNJo82FLmChXvBgQIMAzOBAgkBgQICA4bDmz+Jq2ugWIQOukRetgC/QyBCMX1k8h8/uBIDu8gdH/GqhqJAENCOVVZW3XKmK/q2S4Q2ZIQ+86o80IvRW54XGEwzgQIhMw4FCimFS4X0DgICAgYDAPXAL7gXPx2EmMZWXY3gB7s+fMM/TjFkC52GihqmqpfvdRuvAAe4eGxkKXhcuzV/WdOnFKyRZAtlln7pRXk+aIiOYKOJav+fj6S/seJHkC/av+fMsUsmi50grjH9vu/hcDU0ZiN3Us0RpYSmxADvh3AMET8lPOqaJkQ45+1ZN6tJCAEAAgMC");
        private static int[] order = new int[] { 6,5,4,5,13,9,8,8,8,10,11,12,13,13,14 };
        private static int key = 3;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
