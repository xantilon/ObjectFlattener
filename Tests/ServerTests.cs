using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectFlattener;
using FluentAssertions;
using Xunit;

namespace ObjectFlattenerTests
{

    public class KSServerGroup
    {
        public string ServerName { get; set; }
        public List<KSService> Services { get; set; }

        public KSServerGroup(string serverName, List<KSService> services)
        {
            ServerName = serverName;
            Services = services;
        }
    }

	public record KSService(string name);

	public class KSServiceDto
    {
        public KSServiceDto() { }
        
        public string Server { get; set; } = "";
        public string Name { get; set; } = "";
        public string? InstallFolder { get; set; }
        public string? EvtLogName { get; set; }
    }

    public class ServerTests
    {
        [Fact]
        public void first_test()
        {


            var servers = new List<KSServiceDto>
            {
                new KSServiceDto { Server = "Server1", Name = "Service1", InstallFolder = "C:\\Program Files\\Service1", EvtLogName = "Service1Log" },
                new KSServiceDto { Server = "Server1", Name = "Service2", InstallFolder = "C:\\Program Files\\Service2", EvtLogName = "Service2Log" }
            };

            var flat = servers.FlattenList("Servers");

            flat.Should().NotBeNull();
            flat.Count.Should().Be(8);


            var unflat = flat.UnflattenList<KSServiceDto>("Servers");

        }
    }
}
