using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoyaltyCRM.Services.Services
{
    public class TransactionService : ITransactionService
    {
            private readonly LoyaltyContext _context;

        public TransactionService()
        {
            
        }

        public TransactionService(LoyaltyContext context)
        {
            _context = context;
        }

        public Task<IDbContextTransaction> BeginTransactionAsync()
            => _context.Database.BeginTransactionAsync();
    }
}