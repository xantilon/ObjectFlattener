using ObjectFlattener;
using FluentAssertions;
using Xunit;

namespace ObjectFlattenerTests
{

    public class Nest
    {
        public string Name { get; set; } = "";
        public List<Nest>? SubNests { get; set; }
        public List<OtherNest>? OtherNests { get; set; }

    }

    public class OtherNest { public required string Name { get; set; } }

    public class NestFixture : IDisposable
    {
        public List<Nest>? MyList { get; init; }

        public NestFixture()
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
            };

        }

        public void Dispose()
        {

        }
    }

    public class ListExtensionTests : IClassFixture<NestFixture>
    {

        [Fact]
        public void Flatten_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            NestFixture fixture = new NestFixture();

            // Act
            Dictionary<string, string> result = fixture.MyList!.Flatten("NestList");

            // Assert            
            result.Should().NotBeNull();
            result.Count.Should().Be(12);
            result["NestList:0:Name"].Should().Be("nest1");
            result["NestList:1:OtherNests:0:Name"].Should().Be("othernest2");
            result["NestList:1:SubNests:1:SubNests:0:Name"].Should().Be("subsubnest4");
        }

        [Fact]
        public void UnflattenList_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            NestFixture fixture = new NestFixture();
            Dictionary<string, string> flat = fixture.MyList!.Flatten("NestList");

            // Act
            List<Nest> result = flat.UnflattenList<Nest>("NestList");

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result[0].Name.Should().Be("nest1");
            result[1]!.OtherNests[0]!.Name.Should().Be("othernest2");

        }
    }
}
