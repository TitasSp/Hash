
using System.Text;

/// <summary>
/// StormHash - A custom cryptographic hash function implementation
/// Produces 256-bit (64 hex character) hashes with strong mixing.
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
    private static readonly ulong[] INITIAL_HASH = {
        0x6A09E667F3BCC908UL,
        0xBB67AE8584CAA73BUL,
        0x3C6EF372FE94F82BUL,
        0xA54FF53A5F1D36F1UL,
        0x510E527FADE682D1UL,
        0x9B05688C2B3E6C1FUL,
        0x1F83D9ABFB41BD6BUL,
        0x5BE0CD19137E2179UL
    };
    
    /// <summary>
    /// Computes a 256-bit hash of the input string (returns 64 hex characters)
    /// </summary>
    /// <param name="input">Input string to hash (null treated as empty string)</param>
    /// <returns>64-character hexadecimal hash string</returns>
    public static string ComputeHash(string input)
    {
        if (input == null) input = "";
        
        byte[] data = Encoding.UTF8.GetBytes(input);
        
        ulong[] hash = new ulong[8];
        Array.Copy(INITIAL_HASH, hash, 8);
        
        ulong inputLength = (ulong)data.Length;
        hash[0] ^= inputLength;
        hash[7] ^= inputLength << 32;
        
        int fullChunks = data.Length / 64;
        for (int chunk = 0; chunk < fullChunks; chunk++)
        {
            ProcessChunk(data, chunk * 64, hash);
        }
        
        int remainingBytes = data.Length % 64;
        if (remainingBytes > 0 || data.Length == 0)
        {
            ProcessFinalChunk(data, fullChunks * 64, remainingBytes, hash);
        }
        
        FinalMix(hash);
        
        ulong[] compressed = CompressTo256(hash);
        return HashToHexString(compressed);
    }
    
    private static void ProcessChunk(byte[] data, int offset, ulong[] hash)
    {
        ulong[] words = new ulong[8];
        for (int i = 0; i < 8; i++)
        {
            words[i] = BytesToULong(data, offset + i * 8);
        }
        
        for (int round = 0; round < 4; round++)
        {
            MixingRound(hash, words, round);
        }
    }
    
    private static void ProcessFinalChunk(byte[] data, int offset, int remainingBytes, ulong[] hash)
    {
        byte[] finalChunk = new byte[64];
        if (remainingBytes > 0)
        {
            Array.Copy(data, offset, finalChunk, 0, remainingBytes);
        }
        finalChunk[remainingBytes] = 0x80;
        ulong bitLength = (ulong)data.Length * 8;
        for (int i = 0; i < 8; i++)
        {
            finalChunk[56 + i] = (byte)(bitLength >> (i * 8));
        }
        ProcessChunk(finalChunk, 0, hash);
    }
    
    private static void MixingRound(ulong[] hash, ulong[] words, int round)
    {
        ulong[] temp = new ulong[8];
        Array.Copy(hash, temp, 8);
        
        for (int i = 0; i < 8; i++)
        {
            int next = (i + 1) % 8;
            int prev = (i + 7) % 8;
            
            ulong mixed = words[i];
            
            switch (round)
            {
                case 0:
                    mixed = RotateLeft(mixed ^ PRIME1, 31) * PRIME2;
                    break;
                case 1:
                    mixed = RotateLeft(mixed + PRIME3, 17) ^ PRIME4;
                    break;
                case 2:
                    mixed = (mixed * PRIME5) ^ RotateLeft(mixed, 23);
                    break;
                case 3:
                    mixed = RotateLeft(mixed ^ PRIME1, 13) + PRIME2;
                    break;
            }
            
            temp[i] = hash[i] ^ mixed ^ RotateLeft(hash[next], 7) ^ RotateLeft(hash[prev], 25);
            temp[i] = RotateLeft(temp[i], 11) * PRIME1;
        }
        
        for (int i = 0; i < 8; i++)
        {
            hash[i] = temp[i] ^ temp[(i + 3) % 8] ^ temp[(i + 5) % 8];
        }
    }
    
    private static void FinalMix(ulong[] hash)
    {
        for (int round = 0; round < 5; round++)
        {
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
    
    private static ulong RotateLeft(ulong value, int bits)
    {
        return (value << bits) | (value >> (64 - bits));
    }
    
    /// <summary>
    /// Compress 512-bit internal state to 256-bit output (4 ulongs).
    /// </summary>
    private static ulong[] CompressTo256(ulong[] state)
    {
        var outArr = new ulong[4];
        for (int i = 0; i < 4; i++)
        {
            // combine mirrored state words with rotation and a small salt to ensure diffusion
            outArr[i] = state[i] ^ RotateLeft(state[i + 4], (i * 13) % 64) ^ (PRIME3 + (ulong)(i * 0x9E));
            // final small mixing per word
            outArr[i] ^= (outArr[i] >> 23);
            outArr[i] *= PRIME2;
            outArr[i] ^= RotateLeft(outArr[i], 41);
        }
        return outArr;
    }
    
    /// <summary>
    /// Converts hash state array to hexadecimal string representation (variable length)
    /// </summary>
    private static string HashToHexString(ulong[] hash)
    {
        StringBuilder sb = new StringBuilder(hash.Length * 16);
        foreach (ulong value in hash)
        {
            sb.Append(value.ToString("x16"));
        }
        return sb.ToString();
    }
}

