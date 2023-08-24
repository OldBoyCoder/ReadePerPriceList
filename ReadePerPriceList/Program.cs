using System.Text;

namespace ReadePerPriceList
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var f = new FileStream(@"C:\Program Files (x86)\Fiat\ePER\data\SP.PL.04210 - Copy.accdb", FileMode.Open);
            using (var o = new StreamWriter(@"c:\temp\prices.txt"))
            {
                var b = new BinaryReader(f);
                var MarketCodes = new Dictionary<int, uint>();
                var markets = b.ReadUInt16();
                var records = b.ReadUInt32();
                var rows = 0;
                var maxLen = int.MinValue;
                Console.WriteLine($"{markets} {records}");
                for (int i = 0; i < markets; i++)
                {
                    var marketCode = b.ReadUInt32();
                    var daterange = b.ReadBytes(16);
                    MarketCodes.Add(i, marketCode);
                    //Console.WriteLine($"{i} {marketCode}");
                }
                b.BaseStream.Seek(2006, SeekOrigin.Begin);
                for (int i = 0; i < records; i++)
                {
                    var productCodeBytes = b.ReadBytes(20);
                    var productCode = Encoding.ASCII.GetString(productCodeBytes);
                    var nz = 0;
                    while (productCode[nz] == '0') nz++;
                    productCode = productCode.Substring(nz);
                    var macroFamily = Encoding.ASCII.GetString(b.ReadBytes(1));
                    var family = Encoding.ASCII.GetString(b.ReadBytes(4));
                    var DigitArray = ReadPackedBytes(b.ReadBytes(6));
                    var quantity = int.Parse(DigitArray.Substring(0, 6));
                    var grams = int.Parse(DigitArray.Substring(0, 6));
                    var units = int.Parse(ReadPackedBytes(b.ReadBytes(1)));
                    b.ReadByte();
                    //Console.WriteLine($"{productCode} {macroFamily} {family} {quantity} {units} {grams}");
                    for (int j = 0; j < markets; j++)
                    {
                        var discount = b.ReadChar();
                        var price = double.Parse(ReadPackedBytes(b.ReadBytes(5)));
                        if (MarketCodes[j] == 43)
                            price /= 100.0;
                        else
                            price /= 10000.0;
                        var fee = double.Parse(ReadPackedBytes(b.ReadBytes(2))) /100.0;
                        //Console.WriteLine($"\t{MarketCodes[j]}: {discount} {price / 10000.0} {fee / 100.0}");
                        if (price > 0.001)
                        {
                            o.WriteLine($"{productCode},{MarketCodes[j]},{discount},{price},{fee}");
                            rows++;
                            if (productCode.Length > maxLen) maxLen = productCode.Length;
                        }
                    }

                }
                Console.WriteLine($"{markets} {records} {rows} {maxLen}");
                Console.ReadLine();
            }

        }
        static string ReadPackedBytes(byte[] bytes)
        {
            var ba = new byte[2 * bytes.Length];
            var ix = 0;
            foreach (var b in bytes)
            {
                var h = b & 0xf0;
                h >>= 4;
                ba[ix++] = (byte)(h + 0x30);
                var l = b & 0xf;
                ba[ix++] = (byte)(l + 0x30);
            }
            return Encoding.ASCII.GetString(ba);
        }
    }
}