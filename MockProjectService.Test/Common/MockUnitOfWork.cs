using MockProjectService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Test.Common
{
    public class MockUnitOfWork : IUnitOfWork
    {
        public bool Committed { get; private set; }
        public bool RolledBack { get; private set; }

        public Task CommitAsync()
        {
            Committed = true;
            return Task.CompletedTask;
        }

        public Task RollbackAsync()
        {
            RolledBack = true;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // No-op for mock
        }
    }

}
