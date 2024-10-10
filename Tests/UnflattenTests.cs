using System.Text.Json;
using FluentAssertions;
using ObjectFlattener;
using Xunit;

namespace ObjectFlattenerTests;

public partial class UnflattenTests : IClassFixture<NestFixture>
{
    [Fact]
    public void Unflatten_simple()
    {
        //Arrange
        Dictionary<string, string> dict = new Dictionary<string, string>
        {
            { "Name", "nest1" }
        };

        //Act
        var result = dict.Unflatten<Nest>();

        //Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("nest1");
    }

    [Fact]
    public void Unflatten_simple_with_List()
    {
        //Arrange
        var dict = new Dictionary<string, string>
        {
            //{ "Name", "nest1" },
            { "SubNests:0:Name", "subnest1" },
            { "SubNests:1:Name", "subnest2" },
            { "OtherNests:0:Name", "othernest1" }
        };

        //Act
        var result = dict.Unflatten<Nest>();

        //Assert
        result.Name.Should().Be("");
        result.SubNests!.First().Name.Should().Be("subnest1");
        result.SubNests!.Last().Name.Should().Be("subnest2");
        result.OtherNests!.First().Name.Should().Be("othernest1");
    }

    [Fact]
    public void Unflatten_simple_with_nested_List()
    {
        //Arrange
        var dict = new Dictionary<string, string>
        {
            { "Name", "nest1" },
            { "SubNests:0:Name", "subnest1" },
            { "SubNests:0:SubNests:0:Name", "subsubnest1" },
            { "SubNests:0:SubNests:1:Name", "subsubnest2" },
            { "SubNests:0:SubNests:2:Name", "subsubnest3" },
            { "SubNests:0:SubNests:3:Name", "subsubnest4" },
            { "SubNests:0:SubNests:4:Name", "subsubnest5" },
            { "SubNests:0:SubNests:5:Name", "subsubnest6" },
            { "SubNests:0:SubNests:6:Name", "subsubnest7" },
            { "SubNests:0:SubNests:7:Name", "subsubnest8" },
            { "SubNests:0:SubNests:8:Name", "subsubnest9" },
            { "SubNests:0:SubNests:9:Name", "subsubnest10" },
            { "SubNests:0:SubNests:10:Name", "subsubnest11" },
            { "SubNests:0:SubNests:11:Name", "subsubnest12" },
            { "SubNests:0:SubNests:12:Name", "subsubnest13" }

        };

        //Act
        var result = dict.Unflatten<Nest>();

        //Assert            
        result.SubNests!.First().SubNests![0].Name.Should().Be("subsubnest1");
        result.SubNests!.First().SubNests![1].Name.Should().Be("subsubnest2");
        result.SubNests!.First().SubNests![2].Name.Should().Be("subsubnest3");
        result.SubNests!.First().SubNests![5].Name.Should().Be("subsubnest6");
        result.SubNests!.First().SubNests![12].Name.Should().Be("subsubnest13");
    }                                    


    [Fact]
    public void Unflatten_List_with_missing_indexes()
    {
        //Arrange
        var dict = new Dictionary<string, string>
        {
            { "Name", "nest1" },
            { "SubNests:0:Name", "subnest1" },
            { "SubNests:0:SubNests:0:Name", "subsubnest1" },
            { "SubNests:0:SubNests:12:Name", "subsubnest13" }
        };

        //Act
        var result = dict.Unflatten<Nest>();

        //Assert            
        result.SubNests!.First().SubNests![0].Name.Should().Be("subsubnest1");
        result.SubNests!.First().SubNests![1].Name.Should().Be("subsubnest13");
        
    }

    [Fact]
    public void UnflattenList_should_fill_all_properties()
    {
        // Arrange
        NestFixture fixture = new NestFixture();
        var dict = fixture.Dict;

        // Act
        List<Nest> result = dict.UnflattenList<Nest>("NestList");

        // Assert
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[0].Name.Should().Be("nest1");
        result![0].SubNests![0].Name.Should().Be("subnest1");
        result![0].SubNests![0].SubNests![0].Name.Should().Be("subsubnest1");
        result![0].SubNests![0].OtherNests![0].Name.Should().Be("subothernest1");
                                 
        result![0].SubNests![1].Name.Should().Be("subnest2");
        result![0].SubNests![1].SubNests![0].Name.Should().Be("subsubnest2");
                                 
        result![1].Name.Should().Be("nest2");
        result![1].SubNests![0].Name.Should().Be("subnest3");
        result![1].SubNests![0].SubNests![0].Name.Should().Be("subsubnest3");
        result![1].OtherNests![0].Name.Should().Be("othernest2");
    }

    [Fact]
    public void Unflatten_different_types()
    {
        var sut = new DifferentTypes();
        sut.Initialize();
        var flat = sut.Flatten();

        var result = flat.Unflatten<DifferentTypes>();
        
        result.Should().BeEquivalentTo(sut);

    }

    [Fact]
		public void Flatten_can_do_DictionaryStringString()
		{
			var src = new Dictionary<string, string> {
				{ "key1", "value1" },
				{ "key2", "value2" },
				{ "key3", "value3" }
			};

			var flat1 = src.FlattenDictionary("src");

			flat1.Should().HaveCount(3);

			var src2 = new DictNest();
			src2.dict = src;
			var flat2 = src2.Flatten();

			var unflat2 = flat2.Unflatten<DictNest>();
			unflat2.dict.Should().HaveCount(3);

		}
	}

	public class DictNest()
	{
		public Dictionary<string, string>? dict { get; set; }
	}
