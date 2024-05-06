using Base.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Common;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IRoleRepository RoleRepository { get; }
    Task<bool> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _applicationDbContext;
    
    public IUserRepository UserRepository { get; private set; }
    public IRoleRepository RoleRepository { get; private set; }

    public UnitOfWork(ApplicationDbContext applicationDbContext, 
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _applicationDbContext = applicationDbContext;
        UserRepository = userRepository;
        RoleRepository = roleRepository;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _applicationDbContext.SaveChangesAsync() > 0);
    }

    public void Dispose()
    {
        _applicationDbContext.Dispose();
    }
}
