using Roller.Infrastructure.Repository.Mongo;

namespace Roller.Tutorial.Models;

public class Person:IdentifiableBase<long>
{
    /// <summary>
    /// 年龄
    /// </summary>
    public int Age { get; set; }
}