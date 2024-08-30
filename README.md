# ObjectFlattener

This is a simple utility to flatten a nested object into a single level Dictionary. It supports nested Lists.

## Example

This complex class with nested Lists:

```csharp
 var host = new NestHost {
                Name   = "host1",
                NestList = new List<Nest> {
                            new Nest {
                                Name = "nest1",
                                SubNests = new List<Nest> {
                                    new Nest {
                                        Name = "subnest1",
                                        SubNests = new List<Nest> { new Nest { Name = "subsubnest1" } }
                                    },
                                    new Nest {
                                        Name = "subnest2",
                                        SubNests = new List<Nest> { new Nest { Name = "subsubnest2" } }
                                    }
                                }
                            }
                        }
            };
```

can be flattened into a single level Dictionary:

```csharp
Dictionary<string, string> flattened = host.Flatten();

Dictionary<string, string>
{
    { "Name", "host1" },
    { "NestList:0:Name", "nest1" },
    { "NestList:0:SubNests:0:Name", "subnest1" },
    { "NestList:0:SubNests:0:SubNests:0:Name", "subsubnest1" },    
    { "NestList:0:SubNests:1:Name", "subnest2" },
    { "NestList:0:SubNests:1:SubNests:0:Name", "subsubnest2" }
}
```

To get your object back from Dictionary just use 

```csharp
NestHost unflattened = flattened.Unflatten<NestHost>();
```





