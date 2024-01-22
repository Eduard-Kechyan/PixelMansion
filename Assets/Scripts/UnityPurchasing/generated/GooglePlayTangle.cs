// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("fSUOc4nkSV/ibPM/fC0jsvf2pOGm67qeefe9mxFl1m5TpsmnVLwrh/FDwOPxzMfI60eJRzbMwMDAxMHCjPOmyV9ESNhkaFUtt9ltwsV6uq67/KpKTKJhS6g9XU1mfXMghVLtGEPAzsHxQ8DLw0PAwMFEAVn9S2mpYppGzHiGnHTCfYHigPO/pjDd/XrRZrvWzqgLrQvcH/VSkxqRDoa61Dg69xg+60W50nXm8DRBJa8XpdiT6Th0iUb0pkb8MWj+EkkTmVzV2o61kA/TZ0ZkBsItRbLO0/3n/ihY5DcC7XrV/d9GWgSUn08ixXn8vgH90MF5Ct/dM2hEYILSEkyXlJQfsGTfDok2rjoda58iD+RN9NZ7QEe5A4b6JS+b9WmLysPCwMHA");
        private static int[] order = new int[] { 2,11,2,10,12,11,13,13,11,11,10,13,13,13,14 };
        private static int key = 193;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
