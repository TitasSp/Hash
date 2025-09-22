public class Hash
{
    static int Main()
    {
        // Fiksuotas salt pavyzdys
        string saltedHash = Mixing("Lietuva", 32, "MySecretSalt");
        Console.WriteLine(saltedHash);

        // Dinaminis salt pavyzdys
        string dynamicSalt = Guid.NewGuid().ToString();
        Console.WriteLine("Dynamic salt: " + dynamicSalt);
        Console.WriteLine("Hash with dynamic salt: " + Mixing("Lietuva", 32, dynamicSalt));

        Test.RunAll(32);
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

