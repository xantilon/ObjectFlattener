using ObjectFlattener;
using FluentAssertions;
using Xunit;
using System.Text.Json;

namespace ObjectFlattenerTests
{
    public class FlattenTests : IClassFixture<NestFixture>
    {
        [Fact]
        public void Flatten_null_should_not_create_key()
        {
            //Arrange
            List<Nest> list = [];

            //Act
            Dictionary<string, string> result = list.FlattenList("NestList");

            //Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(0);
        }

        [Fact]
        public void Flatten_null_property_should_not_create_key()
        {
            //Arrange
            NestFixture fixture = new NestFixture();
            List<Nest> myList = fixture.Host.MyList!;

            Dictionary<string, string> flathost = fixture.Host.Flatten();
            NestHost host = flathost.Unflatten<NestHost>();

            myList![0].SubNests = null;
            myList![0].OtherNests = null;
            myList!.RemoveAt(1);

            //Act
            Dictionary<string, string> result = myList!.FlattenList("NestList");

            //Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result.ContainsKey("NestList:0:SubNests").Should().BeFalse();
        }


        [Fact]
        public void Flatten_simple()
        {
            //Arrange
            var nest = new Nest { Name = "nest1" };

            //Act
            Dictionary<string, string> result = nest.Flatten();

            //Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result["Name"].Should().Be("nest1");
        }

        [Fact]
        public void Flatten_simple_with_List()
        {
            //Arrange
            var nest = new Nest { Name = "nest1", SubNests = new List<Nest> { new Nest { Name = "subnest1" } } };

            //Act
            Dictionary<string, string> result = nest.Flatten();

            //Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result["Name"].Should().Be("nest1");
            result["SubNests:0:Name"].Should().Be("subnest1");
        }



        [Fact]
        public void Flatten_simple_with_multi_List()
        {
            //Arrange
            var nest = new Nest
            {
                Name = "nest1",
                SubNests = new List<Nest> { 
                    new Nest { Name = "subnest1" },
                    new Nest { Name = "subnest2" },
                    new Nest { Name = "subnest3" }
                },
                OtherNests = new List<OtherNest> { new OtherNest { Name = "othernest1" } }
            };

            //Act
            Dictionary<string, string> result = nest.Flatten();

            //Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(5);
            result["Name"].Should().Be("nest1");
            result["SubNests:0:Name"].Should().Be("subnest1");
            result["SubNests:1:Name"].Should().Be("subnest2");
            result["SubNests:2:Name"].Should().Be("subnest3");
            result["OtherNests:0:Name"].Should().Be("othernest1");
        }

        
        [Fact]
        public void Flatten_should_return_all_properties()
        {
            // Arrange
            NestFixture fixture = new NestFixture();

            // Act
            Dictionary<string, string> result = fixture.Host.MyList!.FlattenList("NestList");

            // Assert            
            result.Should().NotBeNull();
            result.Count.Should().Be(12);
            result["NestList:0:Name"].Should().Be("nest1");
            result["NestList:1:OtherNests:0:Name"].Should().Be("othernest2");
            result["NestList:1:SubNests:1:SubNests:0:Name"].Should().Be("subsubnest4");

            string json = JsonSerializer.Serialize(result);
        }

    }
}
