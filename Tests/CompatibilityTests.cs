using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ObjectFlattener;
using Xunit;

namespace ObjectFlattenerTests;

public class CompatibilityTests
{
	[Fact]
	public void Test1()
	{
		ToDictionaryExtension.EnsureCompatibility<SimpleClass>().Should().BeTrue();
	}

	[Fact]
	public void Test2()
	{
		ToDictionaryExtension.EnsureCompatibility<NestHost>().Should().BeTrue();
	}

	[Fact]
	public void Test3()
	{
		ToDictionaryExtension.EnsureCompatibility<DictNest>().Should().BeTrue();
	}
}

public class SimpleClass {
	public SimpleClass() { }

	string PropString { get; set; } = "";
	int PropInt { get; set; }
	bool PropBool { get; set; }
	DateTime PropDT { get; set; }
	double PropDouble { get; set; }
	byte PropByte { get; set; }

};

public class MyDict
{	
	public Dictionary<string,string>? NullDict { get; set; }
	public Dictionary<string, string> Dict { get; set; }

}



