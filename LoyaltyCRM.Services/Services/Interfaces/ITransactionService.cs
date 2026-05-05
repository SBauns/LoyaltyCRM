using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface ITransactionService
    {
            Task<IDbContextTransaction> BeginTransactionAsync();
    }
}