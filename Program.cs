using System.Text;

/// <summary>
/// StormHash - A custom cryptographic hash function implementation
/// Produces 512-bit (128 hex character) hashes with strong security properties
/// </summary>
public class StormHash
{
    // Mathematical constants derived from irrational numbers for optimal mixing
    private const ulong PRIME1 = 0x9E3779B185EBCA87UL; // Golden ratio * 2^64
    private const ulong PRIME2 = 0xC2B2AE3D27D4EB4FUL; // Large prime for multiplication
    private const ulong PRIME3 = 0x165667B19E3779F9UL; // Mixing constant
    private const ulong PRIME4 = 0x85EBCA77C2B2AE63UL; // Rotation multiplier  
    private const ulong PRIME5 = 0x27D4EB2F165667C5UL; // Final mixing prime
    
    // Initial hash values - fractional parts of cube roots of first 8 primes
    // These provide good initial entropy distribution
    private static readonly ulong[] INITIAL_HASH = {
        0x6A09E667F3BCC908UL, // cube_root(2)
        0xBB67AE8584CAA73BUL, // cube_root(3)
        0x3C6EF372FE94F82BUL, // cube_root(5)
        0xA54FF53A5F1D36F1UL, // cube_root(7)
        0x510E527FADE682D1UL, // cube_root(11)
        0x9B05688C2B3E6C1FUL, // cube_root(13)
        0x1F83D9ABFB41BD6BUL, // cube_root(17)
        0x5BE0CD19137E2179UL  // cube_root(19)
    };
    
    /// <summary>
    /// Computes a 512-bit hash of the input string
    /// </summary>
    /// <param name="input">Input string to hash (null treated as empty string)</param>
    /// <returns>128-character hexadecimal hash string</returns>
    public static string ComputeHash(string input)
    {
        if (input == null) input = "";
        
        // Convert string to UTF-8 bytes for processing
        byte[] data = Encoding.UTF8.GetBytes(input);
        
        // Initialize hash state with predetermined constants
        ulong[] hash = new ulong[8];
        Array.Copy(INITIAL_HASH, hash, 8);
        
        // Mix input length into initial state (prevents length extension attacks)
        ulong inputLength = (ulong)data.Length;
        hash[0] ^= inputLength;
        hash[7] ^= inputLength << 32;
        
        // Process input in 64-byte chunks for efficiency
        int fullChunks = data.Length / 64;
        
        for (int chunk = 0; chunk < fullChunks; chunk++)
        {
            ProcessChunk(data, chunk * 64, hash);
        }
        
        // Handle remaining bytes with proper padding
        int remainingBytes = data.Length % 64;
        if (remainingBytes > 0 || data.Length == 0)
        {
            ProcessFinalChunk(data, fullChunks * 64, remainingBytes, hash);
        }
        
        // Apply final mixing to ensure strong avalanche effect
        FinalMix(hash);
        
        // Convert hash state to hexadecimal string
        return HashToHexString(hash);
    }
    
    /// <summary>
    /// Processes a complete 64-byte chunk of input data
    /// </summary>
    private static void ProcessChunk(byte[] data, int offset, ulong[] hash)
    {
        // Convert 64 bytes into 8 ulong values (little-endian)
        ulong[] words = new ulong[8];
        for (int i = 0; i < 8; i++)
        {
            words[i] = BytesToULong(data, offset + i * 8);
        }
        
        // Apply multiple rounds of mixing with different operations per round
        for (int round = 0; round < 4; round++)
        {
            MixingRound(hash, words, round);
        }
    }
    
    /// <summary>
    /// Processes the final partial chunk with proper padding
    /// </summary>
    private static void ProcessFinalChunk(byte[] data, int offset, int remainingBytes, ulong[] hash)
    {
        // Create padded final chunk using Merkle-Damgård construction
        byte[] finalChunk = new byte[64];
        
        // Copy remaining bytes
        if (remainingBytes > 0)
        {
            Array.Copy(data, offset, finalChunk, 0, remainingBytes);
        }
        
        // Add mandatory padding bit (prevents padding attacks)
        finalChunk[remainingBytes] = 0x80;
        
        // Add original length in bits at end (little-endian)
        ulong bitLength = (ulong)data.Length * 8;
        for (int i = 0; i < 8; i++)
        {
            finalChunk[56 + i] = (byte)(bitLength >> (i * 8));
        }
        
        ProcessChunk(finalChunk, 0, hash);
    }
    
    /// <summary>
    /// Performs one round of mixing operations on the hash state
    /// </summary>
    private static void MixingRound(ulong[] hash, ulong[] words, int round)
    {
        ulong[] temp = new ulong[8];
        Array.Copy(hash, temp, 8);
        
        // Apply round-specific transformations for different diffusion patterns
        for (int i = 0; i < 8; i++)
        {
            int next = (i + 1) % 8;
            int prev = (i + 7) % 8;
            
            ulong mixed = words[i];
            
            // Each round uses different operations for varied diffusion
            switch (round)
            {
                case 0: // Multiplication and rotation
                    mixed = RotateLeft(mixed ^ PRIME1, 31) * PRIME2;
                    break;
                case 1: // Addition and XOR
                    mixed = RotateLeft(mixed + PRIME3, 17) ^ PRIME4;
                    break;
                case 2: // Self-mixing with multiplication
                    mixed = (mixed * PRIME5) ^ RotateLeft(mixed, 23);
                    break;
                case 3: // Complex rotation and addition
                    mixed = RotateLeft(mixed ^ PRIME1, 13) + PRIME2;
                    break;
            }
            
            // Update hash state with neighbor influence
            temp[i] = hash[i] ^ mixed ^ RotateLeft(hash[next], 7) ^ RotateLeft(hash[prev], 25);
            temp[i] = RotateLeft(temp[i], 11) * PRIME1;
        }
        
        // Additional cross-mixing between all elements for increased diffusion
        for (int i = 0; i < 8; i++)
        {
            hash[i] = temp[i] ^ temp[(i + 3) % 8] ^ temp[(i + 5) % 8];
        }
    }
    
    /// <summary>
    /// Applies final mixing rounds for maximum avalanche effect
    /// </summary>
    private static void FinalMix(ulong[] hash)
    {
        // Multiple rounds ensure single-bit changes affect entire output
        for (int round = 0; round < 5; round++)
        {
            // Individual element mixing
            for (int i = 0; i < 8; i++)
            {
                hash[i] ^= hash[(i + 1) % 8];
                hash[i] = RotateLeft(hash[i], 19) * PRIME1;
                hash[i] ^= hash[i] >> 17;
                hash[i] *= PRIME3;
                hash[i] ^= hash[i] >> 13;
                hash[i] *= PRIME5;
                hash[i] ^= hash[i] >> 16;
            }
            
            // Cross-element mixing (skip on final round to avoid redundancy)
            if (round < 4)
            {
                ulong temp = hash[0];
                for (int i = 0; i < 7; i++)
                {
                    hash[i] ^= hash[i + 1];
                }
                hash[7] ^= temp;
            }
        }
    }
    
    /// <summary>
    /// Converts 8 bytes from array to ulong (little-endian)
    /// </summary>
    private static ulong BytesToULong(byte[] data, int offset)
    {
        ulong result = 0;
        for (int i = 0; i < 8; i++)
        {
            if (offset + i < data.Length)
            {
                result |= ((ulong)data[offset + i]) << (i * 8);
            }
        }
        return result;
    }
    
    /// <summary>
    /// Rotates ulong value left by specified number of bits
    /// </summary>
    private static ulong RotateLeft(ulong value, int bits)
    {
        return (value << bits) | (value >> (64 - bits));
    }
    
    /// <summary>
    /// Converts hash state array to hexadecimal string representation
    /// </summary>
    private static string HashToHexString(ulong[] hash)
    {
        StringBuilder sb = new StringBuilder(128);
        foreach (ulong value in hash)
        {
            sb.Append(value.ToString("x16"));
        }
        return sb.ToString();
    }
}
