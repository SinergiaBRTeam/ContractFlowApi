using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContractsMvc.Data;
using ContractsMvc.Models;
using ContractsMvc.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ContractsMvc.Services
{
    /// <summary>
    /// Encapsulates CRUD operations for obligations. Obligations are
    /// always tied to a contract. This service validates the existence of
    /// the parent contract when creating an obligation and handles soft
    /// deletion when removing records.
    /// </summary>
    public class ObligationService
    {
        private readonly ContractsDbContext _db;

        public ObligationService(ContractsDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new obligation under a contract. Returns null when
        /// the contract does not exist.
        /// </summary>
        public async Task<ObligationSummaryDto?> CreateAsync(Guid contractId, CreateObligationRequest request, CancellationToken ct)
        {
            var contract = await _db.Contracts.FirstOrDefaultAsync(c => c.Id == contractId, ct);
            if (contract == null) return null;
            var ob = new Obligation
            {
                ContractId = contract.Id,
                Contract = contract,
                ClauseRef = string.IsNullOrWhiteSpace(request.ClauseRef) ? "N/A" : request.ClauseRef,
                Description = string.IsNullOrWhiteSpace(request.Description) ? "" : request.Description,
                DueDate = request.DueDate,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status
            };
            _db.Obligations.Add(ob);
            await _db.SaveChangesAsync(ct);
            return new ObligationSummaryDto
            {
                Id = ob.Id,
                ContractId = contract.Id,
                ClauseRef = ob.ClauseRef,
                Description = ob.Description,
                DueDate = ob.DueDate,
                Status = ob.Status
            };
        }

        /// <summary>
        /// Retrieves an obligation by id. Returns null when not found.
        /// </summary>
        public async Task<ObligationSummaryDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var ob = await _db.Obligations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);
            if (ob == null) return null;
            return new ObligationSummaryDto
            {
                Id = ob.Id,
                ContractId = ob.ContractId,
                ClauseRef = ob.ClauseRef,
                Description = ob.Description,
                DueDate = ob.DueDate,
                Status = ob.Status
            };
        }

        /// <summary>
        /// Lists all obligations for a given contract.
        /// </summary>
        public async Task<List<ObligationSummaryDto>> ListForContractAsync(Guid contractId, CancellationToken ct)
        {
            return await _db.Obligations.AsNoTracking()
                .Where(o => o.ContractId == contractId)
                .OrderBy(o => o.ClauseRef)
                .Select(o => new ObligationSummaryDto
                {
                    Id = o.Id,
                    ContractId = o.ContractId,
                    ClauseRef = o.ClauseRef,
                    Description = o.Description,
                    DueDate = o.DueDate,
                    Status = o.Status
                })
                .ToListAsync(ct);
        }

        /// <summary>
        /// Updates an existing obligation. Returns false when the record
        /// cannot be found.
        /// </summary>
        public async Task<bool> UpdateAsync(Guid id, UpdateObligationRequest request, CancellationToken ct)
        {
            var ob = await _db.Obligations.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (ob == null) return false;
            ob.ClauseRef = string.IsNullOrWhiteSpace(request.ClauseRef) ? ob.ClauseRef : request.ClauseRef;
            ob.Description = string.IsNullOrWhiteSpace(request.Description) ? ob.Description : request.Description;
            ob.DueDate = request.DueDate;
            ob.Status = string.IsNullOrWhiteSpace(request.Status) ? ob.Status : request.Status;
            ob.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Soft deletes an obligation. Returns false when not found.
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            var ob = await _db.Obligations.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (ob == null) return false;
            ob.IsDeleted = true;
            ob.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}