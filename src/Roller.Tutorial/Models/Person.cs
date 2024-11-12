using Roller.Infrastructure.Repository.Mongo;

namespace Roller.Tutorial.Models;

public class Person:IdentifiableBase<long>
{
    public int Age { get; set; }
}