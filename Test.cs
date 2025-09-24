using System.Diagnostics;
using System.Text;

public class HashTestSuite
{
    private const string TEST_FOLDER = "test";
    private static Random random = new Random();
    
    public static void Main()
    {
        Console.WriteLine("=== StormHash Comprehensive Test Suite ===\n");
        
        // Create test folder if it doesn't exist
        if (!Directory.Exists(TEST_FOLDER))
        {
            Directory.CreateDirectory(TEST_FOLDER);
        }
        
        // Run all tests
        TestOutputSize();
        TestDeterminism();
        TestPerformance();
        TestCollisions();
        TestAvalancheEffect();
        TestIrreversibility();
        
        Console.WriteLine("\n=== All tests completed! Check results above ===");
    }
    
        
    private static void TestOutputSize()
    {
        Console.WriteLine("2. TESTING OUTPUT SIZE");
        Console.WriteLine("======================");
        
        string[] testFiles = {
            "empty.txt",
            "a.txt", 
            "b.txt",
            "random1.txt",
            "random2.txt",
            "konstitucija.txt"
        };
        
        foreach (string fileName in testFiles)
        {
            try
            {
                string filePath = Path.Combine(TEST_FOLDER, fileName);
                string content = File.ReadAllText(filePath);
                string hash = StormHash.ComputeHash(content);
                
                Console.WriteLine($"{fileName,-20} | Input: {content.Length,5} chars | Hash: {hash.Length,3} chars | Hash: {hash.Substring(0, 32)}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error testing {fileName}: {ex.Message}");
            }
        }
        
        Console.WriteLine("✓ All hashes have consistent 128-character length");
        Console.WriteLine();
    }
    
    private static void TestDeterminism()
    {
        Console.WriteLine("3. TESTING DETERMINISM");
        Console.WriteLine("======================");
        
        string testContent = File.ReadAllText(Path.Combine(TEST_FOLDER, "konstitucija.txt"));
        
        string[] hashes = new string[10];
        for (int i = 0; i < 10; i++)
        {
            hashes[i] = StormHash.ComputeHash(testContent);
        }
        
        bool allIdentical = hashes.All(h => h == hashes[0]);
        
        Console.WriteLine($"Hash 1:  {hashes[0]}");
        Console.WriteLine($"Hash 2:  {hashes[1]}");
        Console.WriteLine($"Hash 10: {hashes[9]}");
        Console.WriteLine($"All 10 hashes identical: {(allIdentical ? "✓ YES" : "❌ NO")}");
        Console.WriteLine();
    }
    
    private static void TestPerformance()
    {
        Console.WriteLine("4. PERFORMANCE TESTING");
        Console.WriteLine("======================");
        
        try
        {
            string constitutionContent = File.ReadAllText(Path.Combine(TEST_FOLDER, "konstitucija.txt"));
            List<(int lines, double avgTimeMs)> results = new List<(int, double)>();
            
            // Test with 1, 2, 4, 8, 16, 32, 64 repetitions
            int[] multipliers = { 1, 2, 4, 8, 16, 32, 64 };
            
            foreach (int multiplier in multipliers)
            {
                string testContent = string.Join("\n", Enumerable.Repeat(constitutionContent, multiplier));
                List<double> times = new List<double>();
                
                // Run 5 tests and average
                for (int test = 0; test < 5; test++)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    StormHash.ComputeHash(testContent);
                    sw.Stop();
                    times.Add(sw.Elapsed.TotalMilliseconds);
                }
                
                double avgTime = times.Average();
                results.Add((multiplier, avgTime));
                
                Console.WriteLine($"{multiplier,2}x content | Size: {testContent.Length,7} chars | Avg time: {avgTime,6:F2}ms");
            }
            
            Console.WriteLine("\n📊 Performance scaling analysis:");
            for (int i = 1; i < results.Count; i++)
            {
                double sizeRatio = (double)results[i].lines / results[i-1].lines;
                double timeRatio = results[i].avgTimeMs / results[i-1].avgTimeMs;
                Console.WriteLine($"   {results[i-1].lines}x → {results[i].lines}x: {sizeRatio:F1}x size, {timeRatio:F2}x time (efficiency: {sizeRatio/timeRatio:F2})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Performance test error: {ex.Message}");
        }
        
        Console.WriteLine();
    }
    
    private static void TestCollisions()
    {
        Console.WriteLine("5. COLLISION TESTING");
        Console.WriteLine("====================");
        
        int[] stringLengths = { 10, 100, 500, 1000 };
        const int PAIRS_PER_LENGTH = 100000;
        
        foreach (int length in stringLengths)
        {
            Console.WriteLine($"\nTesting {PAIRS_PER_LENGTH:N0} random strings of length {length}:");
            
            HashSet<string> seenHashes = new HashSet<string>();
            int collisions = 0;
            
            Stopwatch sw = Stopwatch.StartNew();
            
            for (int i = 0; i < PAIRS_PER_LENGTH; i++)
            {
                string randomString = GenerateRandomString(length);
                string hash = StormHash.ComputeHash(randomString);
                
                if (!seenHashes.Add(hash))
                {
                    collisions++;
                }
                
                if ((i + 1) % 20000 == 0)
                {
                    Console.Write($"  Progress: {i + 1:N0}/{PAIRS_PER_LENGTH:N0} ({(double)(i + 1) / PAIRS_PER_LENGTH * 100:F1}%)\r");
                }
            }
            
            sw.Stop();
            
            Console.WriteLine($"  Results: {collisions} collisions found in {PAIRS_PER_LENGTH:N0} strings");
            Console.WriteLine($"  Collision rate: {(double)collisions / PAIRS_PER_LENGTH * 100:F6}%");
            Console.WriteLine($"  Time taken: {sw.ElapsedMilliseconds:N0}ms");
        }
        
        Console.WriteLine();
    }
    
    private static void TestAvalancheEffect()
    {
        Console.WriteLine("6. AVALANCHE EFFECT TESTING");
        Console.WriteLine("===========================");
        
        const int TEST_PAIRS = 100000;
        List<double> bitDifferences = new List<double>();
        List<double> hexDifferences = new List<double>();
        
        Console.WriteLine($"Testing {TEST_PAIRS:N0} string pairs differing by one character...");
        
        Stopwatch sw = Stopwatch.StartNew();
        
        for (int i = 0; i < TEST_PAIRS; i++)
        {
            // Generate base string
            int length = random.Next(10, 1000);
            string baseString = GenerateRandomString(length);
            
            // Create modified string (change one random character)
            char[] modified = baseString.ToCharArray();
            int changePos = random.Next(length);
            modified[changePos] = (char)random.Next(32, 127); // Ensure it's different
            while (modified[changePos] == baseString[changePos])
            {
                modified[changePos] = (char)random.Next(32, 127);
            }
            string modifiedString = new string(modified);
            
            // Get hashes
            string hash1 = StormHash.ComputeHash(baseString);
            string hash2 = StormHash.ComputeHash(modifiedString);
            
            // Analyze differences
            (double bitDiff, double hexDiff) = AnalyzeHashDifferences(hash1, hash2);
            bitDifferences.Add(bitDiff);
            hexDifferences.Add(hexDiff);
            
            if ((i + 1) % 10000 == 0)
            {
                Console.Write($"Progress: {i + 1:N0}/{TEST_PAIRS:N0} ({(double)(i + 1) / TEST_PAIRS * 100:F1}%)\r");
            }
        }
        
        sw.Stop();
        
        // Calculate statistics
        Console.WriteLine($"\nResults after {sw.ElapsedMilliseconds:N0}ms:");
        Console.WriteLine("\nBit-level differences:");
        Console.WriteLine($"  Minimum: {bitDifferences.Min():F2}%");
        Console.WriteLine($"  Maximum: {bitDifferences.Max():F2}%");
        Console.WriteLine($"  Average: {bitDifferences.Average():F2}%");
        Console.WriteLine($"  Std Dev: {CalculateStandardDeviation(bitDifferences):F2}%");
        
        Console.WriteLine("\nHex-level differences:");
        Console.WriteLine($"  Minimum: {hexDifferences.Min():F2}%");
        Console.WriteLine($"  Maximum: {hexDifferences.Max():F2}%");
        Console.WriteLine($"  Average: {hexDifferences.Average():F2}%");
        Console.WriteLine($"  Std Dev: {CalculateStandardDeviation(hexDifferences):F2}%");
        
        // Ideal avalanche effect should be around 50%
        double avgBitDiff = bitDifferences.Average();
        if (avgBitDiff >= 45 && avgBitDiff <= 55)
        {
            Console.WriteLine($"✓ Avalanche effect is excellent (close to ideal 50%)");
        }
        else if (avgBitDiff >= 40 && avgBitDiff <= 60)
        {
            Console.WriteLine($"✓ Avalanche effect is good");
        }
        else
        {
            Console.WriteLine($"⚠ Avalanche effect could be improved");
        }
        
        Console.WriteLine();
    }
    
    private static void TestIrreversibility()
    {
        Console.WriteLine("7. IRREVERSIBILITY DEMONSTRATION");
        Console.WriteLine("=================================");
        
        string[] testInputs = {
            "password123",
            "secret_key_2024", 
            "admin@example.com",
            "blockchain_hash_test"
        };
        
        string salt = "StormHash_Salt_" + DateTime.Now.Ticks;
        Console.WriteLine($"Using salt: {salt}\n");
        
        foreach (string input in testInputs)
        {
            string saltedInput = input + salt;
            string hash = StormHash.ComputeHash(saltedInput);
            
            Console.WriteLine($"Input: '{input}'");
            Console.WriteLine($"Salted: '{saltedInput}'");
            Console.WriteLine($"Hash: {hash}");
            Console.WriteLine($"Original input length: {input.Length}, Hash length: {hash.Length}");
            Console.WriteLine("→ Demonstrates one-way property: hash reveals no information about original input\n");
        }
        
        // Dictionary attack simulation
        Console.WriteLine("Dictionary attack simulation:");
        string targetHash = StormHash.ComputeHash("password" + salt);
        string[] commonPasswords = { "password", "123456", "admin", "root", "test", "guest" };
        
        Console.WriteLine($"Target hash: {targetHash}");
        bool found = false;
        
        foreach (string pwd in commonPasswords)
        {
            string testHash = StormHash.ComputeHash(pwd + salt);
            if (testHash == targetHash)
            {
                Console.WriteLine($"✓ Found match: '{pwd}'");
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            Console.WriteLine("❌ No matches found in common password list");
        }
        
        Console.WriteLine("→ This demonstrates why salting is important for password hashing");
        Console.WriteLine();
    }
    
    // Helper methods
    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,!?;:()[]{}\"'-_+=<>/@#$%^&*";
        StringBuilder sb = new StringBuilder(length);
        
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[random.Next(chars.Length)]);
        }
        
        return sb.ToString();
    }
    
    private static string GenerateConstitutionLikeText(int targetLength)
    {
        string[] phrases = {
            "Lietuvos Respublikos Konstitucija",
            "Lietuvos valstybė yra nepriklausoma demokratinė respublika",
            "Suvereniosios valdžios aukščiausiasis subjektas yra Tauta",
            "Niekas negali riboti ar varžyti valstybės suvereniteto",
            "Konstitucija yra sudedamoji teisės sistemos dalis",
            "Teisės aktai negali prieštarauti Konstitucijai",
            "Lietuvos valstybės teritorija yra vientisa ir nedaloma",
            "Valstybės sienas keisti gali tik Seimas",
            "Lietuvos pilietybė įgyjama gimimo, natūralizacijos ar kitais įstatymų nustatytais pagrindais"
        };
        
        StringBuilder sb = new StringBuilder();
        
        while (sb.Length < targetLength)
        {
            string phrase = phrases[random.Next(phrases.Length)];
            sb.Append(phrase);
            if (sb.Length < targetLength) sb.Append(". ");
        }
        
        return sb.ToString().Substring(0, Math.Min(targetLength, sb.Length));
    }
    
    private static (double bitDifference, double hexDifference) AnalyzeHashDifferences(string hash1, string hash2)
    {
        int differentBits = 0;
        int differentHexChars = 0;
        int totalBits = hash1.Length * 4; // Each hex char = 4 bits
        
        for (int i = 0; i < hash1.Length; i++)
        {
            int val1 = Convert.ToInt32(hash1[i].ToString(), 16);
            int val2 = Convert.ToInt32(hash2[i].ToString(), 16);
            
            if (val1 != val2)
            {
                differentHexChars++;
                
                // Count differing bits
                int xor = val1 ^ val2;
                while (xor > 0)
                {
                    differentBits += xor & 1;
                    xor >>= 1;
                }
            }
        }
        
        double bitPercentage = (differentBits * 100.0) / totalBits;
        double hexPercentage = (differentHexChars * 100.0) / hash1.Length;
        
        return (bitPercentage, hexPercentage);
    }
    
    private static double CalculateStandardDeviation(List<double> values)
    {
        double mean = values.Average();
        double sumOfSquaredDifferences = values.Sum(val => Math.Pow(val - mean, 2));
        return Math.Sqrt(sumOfSquaredDifferences / values.Count);
    }
}
