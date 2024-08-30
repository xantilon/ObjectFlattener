namespace ObjectFlattenerTests
{
    public class Nest
    {
        public string Name { get; set; } = "";
        public List<Nest>? SubNests { get; set; }
        public List<OtherNest>? OtherNests { get; set; }

    }

    public class OtherNest { public required string Name { get; set; } }

    public class NestHost
    {
        public List<Nest>? MyList { get; init; }
    }

    public class NestFixture : IDisposable
    {
        public NestHost Host { get; init; }

        public readonly Dictionary<string, string> Dict;

        public NestFixture()
        {
            Host = new NestHost
            {
                MyList = new List<Nest> {
                    new Nest {
                        Name = "nest1",
                        SubNests = new List<Nest> {
                            new Nest {
                                Name = "subnest1",
                                SubNests = new List<Nest> { new Nest { Name = "subsubnest1" } },
                                OtherNests = new List<OtherNest> { new OtherNest { Name = "othernest1" } }
                            },
                            new Nest {
                                Name = "subnest2",
                                SubNests = new List<Nest> { new Nest { Name = "subsubnest2" } }
                            }
                        }
                    },
                    new Nest {
                        Name = "nest2",
                        SubNests = new List<Nest> {
                            new Nest {
                                Name = "subnest3",
                                SubNests = new List<Nest> { new Nest { Name = "subsubnest3" } }
                            },
                            new Nest {
                                Name = "subnest4",
                                SubNests = new List<Nest> { new Nest { Name = "subsubnest4" } }
                            }
                        },
                        OtherNests = new List<OtherNest> { new OtherNest { Name = "othernest2" } }
                    }
                }
            };

            Dict = new Dictionary<string, string>
            {
                { "NestList:0:Name", "nest1" },
                { "NestList:0:SubNests:0:Name", "subnest1" },
                { "NestList:0:SubNests:0:SubNests:0:Name", "subsubnest1" },
                { "NestList:0:SubNests:0:OtherNests:0:Name", "subothernest1" },
                { "NestList:0:SubNests:1:Name", "subnest2" },
                { "NestList:0:SubNests:1:SubNests:0:Name", "subsubnest2" },
                { "NestList:1:Name", "nest2" },
                { "NestList:1:SubNests:0:Name", "subnest3" },
                { "NestList:1:SubNests:0:SubNests:0:Name", "subsubnest3" },
                { "NestList:1:SubNests:1:Name", "subnest4" },
                { "NestList:1:SubNests:1:SubNests:0:Name", "subsubnest4" },
                { "NestList:1:OtherNests:0:Name", "othernest2" }
            };
        }

        public void Dispose()
        {

        }
    }
}
