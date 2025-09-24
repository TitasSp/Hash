public class Hash
{
    static int Main()
    {
        int n = -1;
        while (true)
        {
            Console.WriteLine(
                "write:\n1 - fixed salt example\n2 - dynamic salt example\n3 - hash yout word\n4 - hash your word with your salt\n5 - run tests\n0 - quit"
            );
            n = Convert.ToInt32(Console.ReadLine());
            if (n == 0)
            {
                break;
            }
            else if (n == 1)
            {
                // Fiksuotas salt pavyzdys
                string saltedHash = Mixing("Lietuva", 32, "MySecretSalt");
                Console.WriteLine(saltedHash);
            }
            else if (n == 2)
            {
                // Dinaminis salt pavyzdys
                string dynamicSalt = Guid.NewGuid().ToString();
                Console.WriteLine("Dynamic salt: " + dynamicSalt);
                Console.WriteLine("Hash with dynamic salt: " + Mixing("Lietuva", 32, dynamicSalt));
            }
            else if (n == 3)
            {
                Console.WriteLine("write text for hashing: ");
                string input = Console.ReadLine();
                Console.WriteLine(Mixing(input, 32, Guid.NewGuid().ToString()));
            }
            else if (n == 4)
            {
                Console.WriteLine("write text for hashing: ");
                string input = Console.ReadLine();
                Console.WriteLine("write salt for hashing: ");
                string salt = Console.ReadLine();

                Console.WriteLine(Mixing(input, 32, salt));
            }
            else if (n == 5)
            {
                Test.RunAll(32);
            }
            else
            {
                break;
            }
        }

        return 0;
    }

    public static string Mixing(string input, int output_size, string salt = "")
    {
        // sujungiame input + salt
        string data = input + "|" + salt;

        // pradinis bufferis
        byte[] output = new byte[output_size];
        for (int i = 0; i < output.Length; i++)
            output[i] = (byte)(i * 31 ^ 0xA5); // inicializacija su konstanta

        // kelios maišymo iteracijos
        int rounds = 5;
        for (int r = 0; r < rounds; r++)
        {
            for (int i = 0; i < data.Length; i++)
            {
                byte ch = (byte)data[i];
                for (int j = 0; j < output_size; j++)
                {
                    // XOR su rotuotu inputu
                    output[j] ^= (byte)((ch + (j * 131) + r) & 0xFF);

                    // rotacijos į kairę ir į dešinę
                    output[j] = (byte)(((output[j] << (r + 1)) | (output[j] >> (7 - r))) & 0xFF);

                    // daugyba su pirminiu skaičiumi ir pridėta konstanta
                    output[j] = (byte)((output[j] * 31 + 0x9E + r) & 0xFF);

                    // papildomas XOR su aplink esančiais baitais (difuzija)
                    int k = (j + 7) % output_size;
                    output[j] ^= (byte)(output[k] >> 3);

                    // nelinijinis keitimas
                    if (((output[j] >> 3) & 1) == 1)
                        output[j] = (byte)~output[j];
                }
            }
        }

        // papildomas maišymas – pasukti visą masyvą
        for (int r = 0; r < rounds; r++)
        {
            for (int j = 0; j < output_size; j++)
            {
                int k = (j * 7 + r * 13) % output_size;
                output[j] ^= output[k];
                output[j] = (byte)((output[j] << 1) | (output[j] >> 7));
            }
        }

        // konvertavimas į HEX string
        char[] c = new char[output.Length * 2];
        int b;
        for (int i = 0; i < output.Length; i++)
        {
            b = output[i] >> 4;
            c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
            b = output[i] & 0xF;
            c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
        }

        return new string(c);
    }
}
