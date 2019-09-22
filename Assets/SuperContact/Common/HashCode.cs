
public static class HashCode {
    
    public static int Combine(params object[] values) {
        unchecked {
            int hash = 1009;
            foreach (object value in values) {
                hash = hash * 9176 + value.GetHashCode();
            }
            return hash;
        }
    }
}
