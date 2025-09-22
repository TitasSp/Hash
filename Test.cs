using System.Diagnostics;

public static class Test
{
    public static void RunAll(int hashSize)
    {
        TestOutputSize(hashSize);
        TestDeterminism(hashSize);
        TestPerformance(hashSize);
        TestCollisions(hashSize);
        TestAvalanche(hashSize);
        TestIrreversibility(hashSize);
    }

    // 2. Išvedimo dydis
    static void TestOutputSize(int hashSize)
    {
        Console.WriteLine("\n[2] OUTPUT SIZE TEST:");
        string folder = "test";

        foreach (var file in new[] { "a.txt", "b.txt", "empty.txt", "random1.txt", "random2.txt" })
        {
            string full = Path.Combine(folder, file);
            string text = File.ReadAllText(full);
            string h = Hash.Mixing(text, hashSize);
            Console.WriteLine($"{file}: {h.Length} chars (expected {hashSize * 2})");
        }
    }

    // 3. Deterministiškumas
    static void TestDeterminism(int hashSize)
    {
        Console.WriteLine("\n[3] DETERMINISM TEST:");
        string text = File.ReadAllText(Path.Combine("test", "a.txt"));
        string h1 = Hash.Mixing(text, hashSize);
        string h2 = Hash.Mixing(text, hashSize);
        Console.WriteLine($"Hashes equal? {h1 == h2}");
    }

    // 4. Efektyvumas
    static void TestPerformance(int hashSize)
    {
        Console.WriteLine("\n[4] PERFORMANCE TEST:");
        string file = Path.Combine("test", "konstitucija.txt");
        if (!File.Exists(file))
        {
            Console.WriteLine("konstitucija.txt not found in test/, skipping performance test.");
            return;
        }

        string[] lines = File.ReadAllLines(file);
        int[] sizes = { 1, 2, 4, 8, 16, 32, 64, 128, 256 };

        foreach (int s in sizes)
        {
            if (s > lines.Length)
                break;
            string input = string.Join("\n", lines, 0, s);

            long total = 0;
            for (int i = 0; i < 5; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                Hash.Mixing(input, hashSize);
                sw.Stop();
                total += sw.ElapsedMilliseconds;
            }
            Console.WriteLine($"{s} lines -> avg {total / 5.0} ms");
        }
    }

    // 5. Kolizijų paieška
    static void TestCollisions(int hashSize)
    {
        Console.WriteLine("\n[5] COLLISION TEST:");
        int[] lengths = { 10, 100, 500, 1000 };
        Random rnd = new Random();

        foreach (int len in lengths)
        {
            int collisions = 0;
            int total = 100000;
            for (int i = 0; i < total; i++)
            {
                string a = RandomString(len, rnd);
                string b = RandomString(len, rnd);
                if (Hash.Mixing(a, hashSize) == Hash.Mixing(b, hashSize))
                    collisions++;
            }
            Console.WriteLine($"Length {len}: {collisions}/{total} collisions");
        }
    }

    // 6. Lavinos efektas
    static void TestAvalanche(int hashSize)
    {
        Console.WriteLine("\n[6] AVALANCHE TEST:");
        Random rnd = new Random();
        int tests = 100000;
        int totalBits = 0,
            totalHex = 0;
        int minBits = hashSize * 8,
            maxBits = 0;
        int minHex = hashSize * 2,
            maxHex = 0;

        for (int i = 0; i < tests; i++)
        {
            string a = RandomString(100, rnd);
            char[] arr = a.ToCharArray();
            arr[50] = arr[50] == 'a' ? 'b' : 'a';
            string b = new string(arr);

            string ha = Hash.Mixing(a, hashSize);
            string hb = Hash.Mixing(b, hashSize);

            int diffBits = CountBitDiff(ha, hb);
            totalBits += diffBits;
            minBits = Math.Min(minBits, diffBits);
            maxBits = Math.Max(maxBits, diffBits);

            int diffHex = CountHexDiff(ha, hb);
            totalHex += diffHex;
            minHex = Math.Min(minHex, diffHex);
            maxHex = Math.Max(maxHex, diffHex);
        }

        Console.WriteLine(
            $"Bits difference: avg {(totalBits / (double)tests) / (hashSize * 8) * 100:F2}% "
                + $"(min {minBits}, max {maxBits})"
        );
        Console.WriteLine(
            $"Hex difference: avg {(totalHex / (double)tests) / (hashSize * 2) * 100:F2}% "
                + $"(min {minHex}, max {maxHex})"
        );
    }

    // 7. Negrįžtamumas su salt
    static void TestIrreversibility(int hashSize)
    {
        Console.WriteLine("\n[7] IRREVERSIBILITY TEST:");
        string input = "password123";
        for (int i = 0; i < 3; i++)
        {
            string salt = Guid.NewGuid().ToString();
            string h = Hash.Mixing(input + salt, hashSize);
            Console.WriteLine($"Hash with salt {i + 1}: {h}");
        }
    }

    // --- Helpers ---
    static string RandomString(int length, Random rnd = null)
    {
        rnd ??= new Random();
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] arr = new char[length];
        for (int i = 0; i < length; i++)
            arr[i] = chars[rnd.Next(chars.Length)];
        return new string(arr);
    }

    static int CountBitDiff(string a, string b)
    {
        byte[] ba = HexToBytes(a);
        byte[] bb = HexToBytes(b);
        int diff = 0;
        for (int i = 0; i < ba.Length; i++)
            diff += CountBits(ba[i] ^ bb[i]);
        return diff;
    }

    static int CountHexDiff(string a, string b)
    {
        int diff = 0;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                diff++;
        return diff;
    }

    static byte[] HexToBytes(string hex)
    {
        byte[] arr = new byte[hex.Length / 2];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return arr;
    }

    static int CountBits(int n)
    {
        int c = 0;
        while (n != 0)
        {
            c++;
            n &= n - 1;
        }
        return c;
    }
}
