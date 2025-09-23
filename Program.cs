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
        // prijungiam salt
        string data = input + salt;

        byte[] output = new byte[output_size];

        for (int i = 0; i < data.Length; i++)
        {
            byte ch = (byte)data[i];

            for (int j = 0; j < output_size; j++)
            {
                output[j] ^= (byte)((ch + j * 13) & 0xFF);
                output[j] = (byte)((output[j] << 3) | (output[j] >> 5));
                output[j] = (byte)((output[j] + ch + i) & 0xFF);
            }
        }

        //convert from byte array to string
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
