using Roller.Infrastructure.Repository.Mongo;
using Roller.Tutorial.Models;

namespace Roller.Tutorial.Services;

public interface IPersonService : IMongoServiceBase<Person, long>
{
    
}

public class PersonService(IMongoRepositoryBase<Person, long> repositoryBase) :MongoServiceBase<Person,long>(repositoryBase),IPersonService
{
    
}