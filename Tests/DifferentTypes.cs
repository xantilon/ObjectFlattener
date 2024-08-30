namespace ObjectFlattenerTests
{
    public partial class UnflattenTests
    {
        class DifferentTypes
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
            public bool BoolProp { get; set; }
            public float FloatProp { get; set; }
            public double DoubleProp { get; set; }
            public decimal DecimalProp { get; set; }
            public DateTime DateTimeProp { get; set; }
            public short ShortProp { get; set; }
            public long LongProp { get; set; }
            public byte ByteProp { get; set; }
            public char CharProp { get; set; }
            public uint UIntProp { get; set; }

            public void Initialize()
            {
                StringProp = "string";
                IntProp = int.MaxValue;
                BoolProp = true;
                FloatProp = float.MaxValue;
                DoubleProp = double.MaxValue;
                DecimalProp = decimal.MaxValue;
                DateTimeProp = DateTime.MaxValue;
                ShortProp = short.MaxValue;
                LongProp = long.MaxValue;
                ByteProp = byte.MaxValue;
                CharProp = 'c';
                UIntProp = uint.MaxValue;
            }

            public override bool Equals(object? obj)
            {
                if(obj is DifferentTypes other)
                {
                    bool isEqual = StringProp == other.StringProp &&
                                   IntProp == other.IntProp &&
                                   BoolProp == other.BoolProp &&
                                   FloatProp == other.FloatProp &&
                                   DoubleProp == other.DoubleProp &&
                                   DecimalProp == other.DecimalProp &&
                                   DateTimeProp.ToString("yyyy-MM-dd HH:mm:ss") == other.DateTimeProp.ToString("yyyy-MM-dd HH:mm:ss") &&
                                   ShortProp == other.ShortProp &&
                                   LongProp == other.LongProp &&
                                   ByteProp == other.ByteProp &&
                                   CharProp == other.CharProp &&
                                   UIntProp == other.UIntProp;

                    if(!isEqual)
                    {
                        Console.WriteLine("Equality check failed for the following properties:");
                        if(StringProp != other.StringProp)
                            Console.WriteLine("StringProp: " + StringProp + " != " + other.StringProp);
                        if(IntProp != other.IntProp)
                            Console.WriteLine("IntProp: " + IntProp + " != " + other.IntProp);
                        if(BoolProp != other.BoolProp)
                            Console.WriteLine("BoolProp: " + BoolProp + " != " + other.BoolProp);
                        if(FloatProp != other.FloatProp)
                            Console.WriteLine("FloatProp: " + FloatProp + " != " + other.FloatProp);
                        if(DoubleProp != other.DoubleProp)
                            Console.WriteLine("DoubleProp: " + DoubleProp + " != " + other.DoubleProp);
                        if(DecimalProp != other.DecimalProp)
                            Console.WriteLine("DecimalProp: " + DecimalProp + " != " + other.DecimalProp);
                        if(DateTimeProp.ToString("yyyy-MM-dd HH:mm:ss") != other.DateTimeProp.ToString("yyyy-MM-dd HH:mm:ss"))
                            Console.WriteLine("DateTimeProp: " + DateTimeProp + " != " + other.DateTimeProp);
                        if(ShortProp != other.ShortProp)
                            Console.WriteLine("ShortProp: " + ShortProp + " != " + other.ShortProp);
                        if(LongProp != other.LongProp)
                            Console.WriteLine("LongProp: " + LongProp + " != " + other.LongProp);
                        if(ByteProp != other.ByteProp)
                            Console.WriteLine("ByteProp: " + ByteProp + " != " + other.ByteProp);
                        if(CharProp != other.CharProp)
                            Console.WriteLine("CharProp: " + CharProp + " != " + other.CharProp);
                        if(UIntProp != other.UIntProp)
                            Console.WriteLine("UIntProp: " + UIntProp + " != " + other.UIntProp);
                    }

                    return isEqual;
                }
                return base.Equals(obj);
            }
        }
    }
}
